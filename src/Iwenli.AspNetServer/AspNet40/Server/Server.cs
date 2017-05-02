using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace AspNet40.Server
{
    /// <summary>
    /// ASP.NET简易服务器
    /// </summary>
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class Server : MarshalByRefObject
    {
        #region 常量和系统API
        /// <summary>
        /// 令牌模拟级别
        /// </summary>
        private const int SecurityImpersonation = 2;
        /// <summary>
        /// 访问令牌的请求类型
        /// </summary>
		private const int TOKEN_ALL_ACCESS = 983551;
        private const int TOKEN_EXECUTE = 131072;
        private const int TOKEN_IMPERSONATE = 4;
        private const int TOKEN_READ = 131080;

        /// <summary>
        /// 获取调用线程的伪句柄。
        /// </summary>
        /// <returns></returns>
        [DllImport("KERNEL32.DLL", SetLastError = true)]
        private static extern IntPtr GetCurrentThread();

        /// <summary>
        /// 获取模拟调用进程的安全上下文的访问令牌。 令牌被分配给调用线程。
        /// </summary>
        /// <param name="level">新令牌的模拟级别。一个SECURITY_IMPERSONATION_LEVEL枚举类型</param>
        /// <returns>如果成功返回！0</returns>
        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        private static extern bool ImpersonateSelf(int level);

        /// <summary>
        /// 获取关联线程的访问令牌。
        /// </summary>
        /// <param name="thread">线程的句柄，其访问令牌被打开</param>
        /// <param name="access">指定一个访问掩码，指定访问令牌的请求类型。这些请求的访问类型与令牌的自由访问控制列表（DACL）进行协调，以确定哪些访问被授予或拒绝。</param>
        /// <param name="openAsSelf">如果访问检查是针对进程级安全上下文进行的，则为TRUE。如果访问检查是针对调用OpenThreadToken函数的线程的当前安全上下文，则为FALSE。</param>
        /// <param name="hToken">指向接收新打开的访问令牌的句柄的变量的指针。</param>
        /// <returns></returns>
        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        private static extern int OpenThreadToken(IntPtr thread, int access, bool openAsSelf, ref IntPtr hToken);

        /// <summary>
        /// 还原模拟
        /// </summary>
        /// <returns>如果失败返回0</returns>
        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        private static extern int RevertToSelf();


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
        #endregion

        #region 字段

        private object m_lockObject;   //线程锁对象
        private ApplicationManager m_appManager;
        private IntPtr m_processToken;
        private Host m_host;
        private WaitCallback m_onSocketAccept;
        private WaitCallback m_onStart;
        private string m_processUser;  //当前系统登录的用户名
        private bool m_requireAuthentication;  //是否需要身份验证
        private bool m_shutdownInProgress;   //进程是否关闭
        private int m_port;
        private string m_virtualPath;
        private string m_physicalPath;


        private bool m_disableDirectoryListing = true;  //是否禁用目录列表
        private Socket m_socketIpv4;
        private Socket m_socketIpv6;
        #endregion

        #region 属性
        /// <summary>
        /// 物理路径  path
        /// </summary>
        public string PhysicalPath
        {
            get
            {
                return this.m_physicalPath;
            }
        }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get
            {
                return this.m_port;
            }
        }
        /// <summary>
        /// 请求的完整路径 host:port/vPath
        /// </summary>
        public string RootUrl
        {
            get
            {
                if (this.m_port != 80)
                {
                    return "http://localhost:" + this.m_port + this.m_virtualPath;
                }
                else
                {
                    return "http://localhost" + this.m_virtualPath;
                }

            }
        }
        public string VirtualPath
        {
            get
            {
                return this.m_virtualPath;
            }
        }
        #endregion

        #region 构造函数

        public Server(int port, string virtualPath, string physicalPath)
            : this(port, virtualPath, physicalPath, false, false)
        {
        }

        public Server(int port, string virtualPath, string physicalPath, bool requireAuthentication)
            : this(port, virtualPath, physicalPath, requireAuthentication, false)
        {
        }

        public Server(int port, string virtualPath, string physicalPath, bool requireAuthentication, bool disableDirectoryListing)
        {
            this.m_lockObject = new object();
            this.m_port = port;
            this.m_virtualPath = virtualPath;
            this.m_physicalPath = physicalPath.EndsWith(@"\", StringComparison.Ordinal) ? physicalPath : (physicalPath + @"\");
            this.m_requireAuthentication = requireAuthentication;
            this.m_disableDirectoryListing = disableDirectoryListing;
            this.m_onSocketAccept = new WaitCallback(this.OnSocketAccept);
            this.m_onStart = new WaitCallback(this.OnStart);
            this.m_appManager = ApplicationManager.GetApplicationManager();
            this.ObtainProcessToken();
        }
        #endregion

        #region  Private方法
        private Host GetHost()
        {
            if (this.m_shutdownInProgress)
            {
                return null;
            }

            Host host = this.m_host;
            if (host == null)
            {
                lock (this.m_lockObject)
                {
                    host = this.m_host;
                    if (host == null)
                    {
                        string appId = (this.m_virtualPath + this.m_physicalPath).ToLowerInvariant().GetHashCode().ToString("x", CultureInfo.InvariantCulture);
                        //this._host = (Host) this._appManager.CreateObject(appId, typeof(Host), this._virtualPath, this._physicalPath, false);

                        Type hostType = typeof(Host);
                        var buildManagerHostType = typeof(HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
                        var buildManagerHost = m_appManager.CreateObject(appId, buildManagerHostType, m_virtualPath, m_physicalPath, false);
                        buildManagerHostType.InvokeMember("RegisterAssembly",
                            BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                            null, buildManagerHost, new object[] { hostType.Assembly.FullName, hostType.Assembly.Location });

                        this.m_host = (Host)this.m_appManager.CreateObject(appId, hostType, this.m_virtualPath, this.m_physicalPath, false);
                        this.m_host.Configure(this, this.m_port, this.m_virtualPath, this.m_physicalPath, this.m_requireAuthentication, this.m_disableDirectoryListing);
                        host = this.m_host;
                    }
                }
            }
            return host;
        }

        internal void HostStopped()
        {
            this.m_host = null;
        }

        /// <summary>
        /// 获取处理Token
        /// </summary>
        private void ObtainProcessToken()
        {
            if (Server.ImpersonateSelf(SecurityImpersonation))
            {
                Server.OpenThreadToken(Server.GetCurrentThread(), TOKEN_ALL_ACCESS, true, ref this.m_processToken);
                Server.RevertToSelf();
                this.m_processUser = WindowsIdentity.GetCurrent().Name;
            }
        }

        private void OnSocketAccept(object acceptedSocket)
        {
            if (!this.m_shutdownInProgress)
            {
                Connection conn = new Connection(this, (Socket)acceptedSocket);
                if (conn.WaitForRequestBytes() == 0)
                {
                    conn.WriteErrorAndClose(400);
                }
                else
                {
                    Host host = this.GetHost();
                    if (host == null)
                    {
                        conn.WriteErrorAndClose(500);
                    }
                    else
                    {
                        host.ProcessRequest(conn);
                    }
                }
            }
        }
        private void OnStart(object listeningSocket)
        {
            while (!this.m_shutdownInProgress)
            {
                try
                {
                    if (listeningSocket != null)
                    {
                        Socket state = ((Socket)listeningSocket).Accept();
                        ThreadPool.QueueUserWorkItem(this.m_onSocketAccept, state);
                    }
                    continue;
                }
                catch
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        private Socket CreateSocketBindAndListen(AddressFamily family, IPAddress ipAddress, int port)
        {
            Socket socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp)
            {
                ExclusiveAddressUse = false
            };
            try
            {
                socket.Bind(new IPEndPoint(ipAddress, port));
            }
            catch
            {
                socket.Close();
                socket = null;
                throw;
            }
            socket.Listen(0x7fffffff);
            return socket;
        }
        #endregion

        #region Public方法
        /// <summary>
        /// 获取处理进程Token
        /// </summary>
        /// <returns></returns>
        public IntPtr GetProcessToken()
        {
            return this.m_processToken;
        }
        /// <summary>
        /// 获取用户的windows登录名
        /// </summary>
        /// <returns></returns>
        public string GetProcessUser()
        {
            return this.m_processUser;
        }

        /// <summary>
        /// 启动Socket 此处需要做.net 2.0 & 4.0适配
        /// </summary>
        public void Start()
        {
            if (Socket.OSSupportsIPv6)
            {
                try
                {
                    this.m_socketIpv6 = this.CreateSocketBindAndListen(AddressFamily.InterNetworkV6, IPAddress.IPv6Loopback, this.m_port);
                }
                catch (SocketException exception)
                {
                    if ((exception.SocketErrorCode == SocketError.AddressAlreadyInUse) || !Socket.OSSupportsIPv4)
                    {
                        throw;
                    }
                }
            }
            if (Socket.OSSupportsIPv4)
            {
                try
                {
                    IPHostEntry hosts = Dns.GetHostEntry(Environment.MachineName);
                    IPAddress address = IPAddress.Loopback;
                    if (hosts.AddressList.Length > 0)
                        address = hosts.AddressList[0];
                    this.m_socketIpv4 = this.CreateSocketBindAndListen(AddressFamily.InterNetwork, address, this.m_port);
                }
                catch (SocketException)
                {
                    if (this.m_socketIpv6 == null)
                    {
                        throw;
                    }
                }
            }
            if (this.m_socketIpv6 != null)
            {
                ThreadPool.QueueUserWorkItem(this.m_onStart, this.m_socketIpv6);
            }
            if (this.m_socketIpv4 != null)
            {
                ThreadPool.QueueUserWorkItem(this.m_onStart, this.m_socketIpv4);
            }

        }
        /// <summary>
        /// 停止Socket
        /// </summary>
        public void Stop()
        {
            this.m_shutdownInProgress = true;
            try
            {
                if (this.m_socketIpv4 != null)
                {
                    this.m_socketIpv4.Close();
                }
                if (this.m_socketIpv6 != null)
                {
                    this.m_socketIpv6.Close();
                }
            }
            catch
            {
            }
            finally
            {
                this.m_socketIpv4 = null;
                this.m_socketIpv6 = null;
            }
            try
            {
                if (this.m_host != null)
                {
                    this.m_host.Shutdown();
                }
                while (this.m_host != null)
                {
                    Thread.Sleep(100);
                }
            }
            catch
            {
            }
            finally
            {
                this.m_host = null;
            }
        }
        #endregion
    }
}
