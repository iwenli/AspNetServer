using System;
using System.Diagnostics;
using System.IO;

namespace AspNet
{
    class Program
    {
        #region 常量
        /// <summary>
        /// 版本
        /// </summary>
        public static readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        /// <summary>
        /// 程序名称,不包含文件名
        /// </summary>
        public static readonly string Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        /// <summary>
        /// 作者
        /// </summary>
        public const string Author = "Iwenli";
        /// <summary>
        /// 标题
        /// </summary>
        public const string Caption = "AspNet网站运行助手";
        /// <summary>
        /// 描述
        /// </summary>
        public const string Description = "Run ASP.NET applications locally.";
        /// <summary>
        /// 用法
        /// </summary>
        public static readonly string Usage = "用法：\n" +
                                              "    {0} -port:<port number> -path:<physical path> -vpath:<virtual path> \n\n" +
                                              "其中：\n" +
                                              "    port number：\t[可选] 1〜65535之间的未使用端口号，默认为80.\n" +
                                              "    physical path：\t[可选]Web应用程序为rooted的有效目录名，默认程序运行目录.\n" +
                                              "    virtual path：\t[可选]虚拟路径或应用程序根文件，默认为\"/\"。\n\n" +
                                              "示例：\n" +
                                              "    {0} -port:8080 -path:\"c:\\inetpub\\wwwroot\\MyApp\\\" - vpath:\"/index.htm\"\n\n" +
                                              "    您可以访问Web应用程序使用以下形式的网址：\"http://localhost:8080/index.html\"";
        #endregion

        private static string m_path = AppDomain.CurrentDomain.BaseDirectory;
        private static int m_port = 8020;
        private static string m_vpath = @"/";
        private static bool m_requireAuthentication = false;

        static void Main(string[] args)
        {
            Init();
            WL();
            if (args.Length > 0)
            {
                CommandLine commandLine = new CommandLine(args);
                if (commandLine.ShowHelp || CheckCommond(commandLine) < 0)
                {
                    ShowUsage();
                    goto IL_EXIT;
                }
                m_requireAuthentication = commandLine.Options["ntlm"] != null;
            }

            try
            {
                Server.Server server = new Server.Server(m_port, m_vpath, m_path, m_requireAuthentication);
                server.Start();
                Process.Start(server.RootUrl);
            }
            catch (Exception ex)
            {
                WL();
                WL("端口［" + m_port + "］已被占用，或已成功建立服务器！");
                Process.Start("http://localhost:" + m_port);
            }

            IL_EXIT:
            WL(); WL();
            WL("按任意键退出.");
            Console.ReadKey();
        }

        private static void Init()
        {
            Console.Title = Caption + " v" + Version + "  Power by " + Author;
            WL();
            WL("{0} v{1} Copyright (C) 2017 admin@iwenli.org", Name, Version);
            WL("Latest version and source code: https://github.com/iwenli/AspNetServer");
            WL();
            WL();
            Console.ForegroundColor = ConsoleColor.Green;
            WL("=={0}v{1} by:{2}==", Name, Version, Author);
            WL();
            WL(Description);
            WL();
            Console.ResetColor();
        }

        private static void ShowUsage()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            WL(Usage, Name);
            Console.ResetColor();
        }

        /// <summary>
        /// 验证命令 返回1表示正确 -1表示path错误 -2错误的端口号 -3端口号超出范围
        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns></returns>
        private static int CheckCommond(CommandLine commandLine)
        {
            string vpath = (string)commandLine.Options["vpath"];
            if (vpath != null && vpath.Trim().Length > 0 && vpath.StartsWith(@"/", StringComparison.Ordinal))
            {
                m_vpath = vpath;
            }

            string path = (string)commandLine.Options["path"];
            if (path != null && path.Trim().Length > 0)
            {
                if (!Directory.Exists(path))
                {
                    WL();
                    WL("The directory \"" + path + "\" does not exist.");
                    return -1;
                }
                else
                {
                    if (path.EndsWith("\\", StringComparison.Ordinal) == false)
                    {
                        path += "\\";
                    }
                    m_path = path;
                }
            }

            int port = 0;
            try
            {
                port = int.Parse((string)commandLine.Options["port"]);
            }
            catch
            {
                WL();
                WL("Invalid port \"" + port + "\"");
                return -2;
            }
            if (port < 1 || port > 65535)
            {
                WL();
                WL("Port out of range. Port is between 1 and 65535.");
                return -3;
            }
            m_port = port;
            return 1;
        }

        private static void WL()
        {
            Console.WriteLine(string.Empty);
        }
        private static void WL(string value)
        {
            Console.WriteLine(value);
        }
        private static void WL(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
