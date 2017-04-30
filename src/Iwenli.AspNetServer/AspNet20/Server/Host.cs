using System;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace AspNet20.Server
{
    /// <summary>
    /// 处理请求
    /// </summary>
    internal sealed class Host : MarshalByRefObject, IRegisteredObject
    {
        #region 字段
        private bool m_disableDirectoryListing;
        private string m_installPath;
        private string m_lowerCasedClientScriptPathWithTrailingSlash;
        private string m_lowerCasedVirtualPath;
        private string m_lowerCasedVirtualPathWithTrailingSlash;
        private int m_pendingCallsCount;
        private string m_physicalClientScriptPath;
        private string m_physicalPath;
        private int m_port;
        private bool m_requireAuthentication;
        private Server m_server;
        private string m_virtualPath;
        #endregion

        #region 属性
        /// <summary>
        /// 获取是否禁用目录列表
        /// </summary>
        public bool DisableDirectoryListing
        {
            get
            {
                return this.m_disableDirectoryListing;
            }
        }
        /// <summary>
        /// 获取入口路径
        /// </summary>
        public string InstallPath
        {
            get
            {
                return this.m_installPath;
            }
        }
        /// <summary>
        /// 获取客户端脚本路径(规范)
        /// </summary>
        public string NormalizedClientScriptPath
        {
            get
            {
                return this.m_lowerCasedClientScriptPathWithTrailingSlash;
            }
        }
        /// <summary>
        /// 获取虚拟目录（规范）
        /// </summary>
        public string NormalizedVirtualPath
        {
            get
            {
                return this.m_lowerCasedVirtualPathWithTrailingSlash;
            }
        }
        /// <summary>
        /// 物理客户端脚本路径
        /// </summary>
        public string PhysicalClientScriptPath
        {
            get
            {
                return this.m_physicalClientScriptPath;
            }
        }
        /// <summary>
        /// 获取物理路径
        /// </summary>
        public string PhysicalPath
        {
            get
            {
                return this.m_physicalPath;
            }
        }
        /// <summary>
        /// 获取端口
        /// </summary>
        public int Port
        {
            get
            {
                return this.m_port;
            }
        }
        /// <summary>
        /// 获取是否需要身份验证
        /// </summary>
        public bool RequireAuthentication
        {
            get
            {
                return this.m_requireAuthentication;
            }
        }
        /// <summary>
        /// 获取虚拟路径
        /// </summary>
        public string VirtualPath
        {
            get
            {
                return this.m_virtualPath;
            }
        }
        #endregion

        public Host()
        {
            HostingEnvironment.RegisterObject(this);
        }

        #region Peivate方法
        /// <summary>
        /// 连接数+1  原子操作
        /// </summary>
        private void AddPendingCall()
        {
            Interlocked.Increment(ref this.m_pendingCallsCount);
        }

        /// <summary>
        /// 连接数-1 原子操作
        /// </summary>
        private void RemovePendingCall()
        {
            Interlocked.Decrement(ref this.m_pendingCallsCount);
        }

        /// <summary>
        /// 从应用程序的已注册对象中移除一个对象
        /// </summary>
        /// <param name="immediate"></param>
        void IRegisteredObject.Stop(bool immediate)
        {
            if (this.m_server != null)
            {
                this.m_server.HostStopped();
            }
            this.WaitForPendingCallsToFinish();
            HostingEnvironment.UnregisterObject(this);
        }
        /// <summary>
        /// 等待当前连接用户完成请求处理
        /// </summary>
		private void WaitForPendingCallsToFinish()
        {
            while (this.m_pendingCallsCount > 0)
            {
                Thread.Sleep(250);
            }
        }
        #endregion

        #region Public方法
        public void Configure(Server server, int port, string virtualPath, string physicalPath, bool requireAuthentication)
        {
            this.Configure(server, port, virtualPath, physicalPath, requireAuthentication, false);
        }
        public void Configure(Server server, int port, string virtualPath, string physicalPath, bool requireAuthentication, bool disableDirectoryListing)
        {
            this.m_server = server;
            this.m_port = port;
            this.m_installPath = null;
            this.m_virtualPath = virtualPath;
            this.m_requireAuthentication = requireAuthentication;
            this.m_disableDirectoryListing = disableDirectoryListing;
            this.m_lowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(this.m_virtualPath);
            this.m_lowerCasedVirtualPathWithTrailingSlash = virtualPath.EndsWith("/", StringComparison.Ordinal) ? virtualPath : (virtualPath + "/");
            this.m_lowerCasedVirtualPathWithTrailingSlash = CultureInfo.InvariantCulture.TextInfo.ToLower(this.m_lowerCasedVirtualPathWithTrailingSlash);
            this.m_physicalPath = physicalPath;
            this.m_physicalClientScriptPath = HttpRuntime.AspClientScriptPhysicalPath + @"\";
            this.m_lowerCasedClientScriptPathWithTrailingSlash = CultureInfo.InvariantCulture.TextInfo.ToLower(HttpRuntime.AspClientScriptVirtualPath + "/");
        }

        /// <summary>
        /// 获取用户的安全标识符SID
        /// </summary>
        /// <returns></returns>
        public SecurityIdentifier GetProcessSID()
        {
            SecurityIdentifier user;
            using (WindowsIdentity windowsIdentity = new WindowsIdentity(this.m_server.GetProcessToken()))
            {
                user = windowsIdentity.User;
            }
            return user;
        }
        public IntPtr GetProcessToken()
        {
            new SecurityPermission(PermissionState.Unrestricted).Assert();
            return this.m_server.GetProcessToken();
        }
        public string GetProcessUser()
        {
            return this.m_server.GetProcessUser();
        }
        /// <summary>
        /// 让当前对象生存期无限延长
        /// </summary>
        /// <returns></returns>
		public override object InitializeLifetimeService()
        {
            return null;
        }
        public bool IsVirtualPathAppPath(string path)
        {

            if (path == null)
            {
                return false;
            }
            else
            {
                path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
                return (path == this.m_lowerCasedVirtualPath || path == this.m_lowerCasedVirtualPathWithTrailingSlash);
            }
        }
        public bool IsVirtualPathInApp(string path)
        {
            bool flag;
            return this.IsVirtualPathInApp(path, out flag);
        }
        public bool IsVirtualPathInApp(string path, out bool isClientScriptPath)
        {
            isClientScriptPath = false;
            if (path != null)
            {
                path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
                if (this.m_virtualPath == "/" && path.StartsWith("/", StringComparison.Ordinal))
                {
                    if (path.StartsWith(this.m_lowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal))
                    {
                        isClientScriptPath = true;
                    }
                    return true;
                }
                if (path.StartsWith(this.m_lowerCasedVirtualPathWithTrailingSlash, StringComparison.Ordinal))
                {
                    return true;
                }
                if (path == this.m_lowerCasedVirtualPath)
                {
                    return true;
                }
                if (path.StartsWith(this.m_lowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal))
                {
                    isClientScriptPath = true;
                    return true;
                }
            }
            return  false;
        }
        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="conn"></param>
		public void ProcessRequest(Connection conn)
        {
            this.AddPendingCall();
            try
            {
                new Request(this, conn).Process();
            }
            finally
            {
                this.RemovePendingCall();
            }
        }
        /// <summary>
        /// 关闭与宿主关联的web应用程序,并从系统中移除注册对象
        /// </summary>
		public void Shutdown()
        {
            HostingEnvironment.InitiateShutdown();
        }
        #endregion
    }
}
