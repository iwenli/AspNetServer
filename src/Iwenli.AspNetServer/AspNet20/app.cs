using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AspNet20
{
    static class App
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            #region 返回值错误码
            // -1 vpath路径错误
            // -2 path路径为空
            // -3 path路径不存在
            // -4 port为无效数字
            // -5 port超出范围[应该为1~65535]
            // -6 port已结被使用
            #endregion

            #region 注册右键

            if (!File.Exists(Config.MenuStartPaht))
            {
                try
                {
                    RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("Directory\\shell\\", true).CreateSubKey("AspNet");
                    registryKey.SetValue("", "在此启动 AspNet 服务器");
                    registryKey.CreateSubKey("command").SetValue("", "\"" + Config.MenuStartPaht + "\" \"%1\"");
                    registryKey.Close();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("不允许"))
                    {
                        AppMessage.Show("请以管理员身份运行!", MessageBoxIcon.Asterisk);
                    }
                }
            }
            string location = Assembly.GetExecutingAssembly().Location;
            try
            {
                if (File.Exists(Config.MenuStartPaht))
                {
                    File.Delete(Config.MenuStartPaht);
                }
                File.Copy(location, Config.MenuStartPaht);
            }
            catch
            {
            }
            #endregion

            #region 参数初始化
            string str = AppDomain.CurrentDomain.BaseDirectory;
            if (args.Length != 0)
            {
                str = args[0];
            }
            string[] array = new string[]
            {
                "-port:" + new Random().Next(3000, 65535).ToString(),
                "-path:" + str,
                "-vpath:"
            };
            args = array;

            CommandLine commandLine = new CommandLine(args);
            #endregion


            #region vpath  处理
            string vpath = (string)commandLine.Options["vpath"];
            if (vpath == null || vpath.Trim().Length == 0)
            {
                vpath = "/";
            }
            if (!vpath.StartsWith("/", StringComparison.Ordinal))
            {
                AppMessage.Show(string.Format(Config.Usage, "ASPNET", "ASPNET"));
                return -1;
            }
            #endregion

            #region path 处理
            string path = (string)commandLine.Options["path"];
            if (path == null || path.Trim().Length == 0)
            {
                AppMessage.Show(Config.Usage);
                return -2;
            }

            if (!Directory.Exists(path))
            {
                AppMessage.Show("The directory '" + path + "' does not exist.");
                return -3;
            }
            if (path.EndsWith("\\", StringComparison.Ordinal) == false)
            {
                path += "\\";
            }

            #endregion

            #region port 处理
            int port = 0;
            string portStr = (string)commandLine.Options["port"];

            if (portStr == null || portStr.Trim().Length == 0)
            {
                port = 80;
            }
            else
            {
                try
                {
                    port = int.Parse(portStr, CultureInfo.InvariantCulture);
                }
                catch
                {
                    AppMessage.Show("Invalid port'" + port + "'");
                    return -4;
                }

                if (port < 1 || port > 65535)
                {
                    AppMessage.Show("Port is between 1 and 65535.");
                    return -5;
                }
            }
            //从配置中读取端口
            if (File.Exists(path + Config.PortIniPath))
            {
                try
                {
                    StreamReader streamReader = new StreamReader(path + Config.PortIniPath, Encoding.UTF8);
                    string value = streamReader.ReadToEnd();
                    streamReader.Close();
                    port = Convert.ToInt32(value);
                }
                catch (Exception ex)
                {
                }
            }
            #endregion

            bool requireAuthentication = commandLine.Options["ntlm"] != null;

            try
            {
                Server server = new Server(port, vpath, path, requireAuthentication);
                server.Start();

                Application.SetCompatibleTextRenderingDefault(false);
                Application.EnableVisualStyles();
                Application.Run(new AppForm(server));
            }
            catch (Exception ex)
            {
                AppMessage.Show("端口［" + port + "］已被占用，或已成功建立服务器！");
                Process.Start("http://localhost:" + port);
                return -6;
            }

            return 1;
        }
    }
}
