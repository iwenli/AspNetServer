using AspNet20.Utility;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
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
            // -1 path路径不存在
            // -2 port超出范围[应该为1~65535]
            // -3 port已结被使用
            #endregion

            #region 注册右键
            RegisterHelper.RegisterRightClick();
            #endregion

            #region 参数初始化
            int port = new Random().Next(3000, 65535);
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string vpath = "/";
            if (args.Length != 0)
            {
                path = args[0];
            }
            #endregion

            #region path 处理
            if (!Directory.Exists(path))
            {
                AppMessage.Show("The directory '" + path + "' does not exist.");
                return -1;
            }
            if (path.EndsWith("\\", StringComparison.Ordinal) == false)
            {
                path += "\\";
            }
            #endregion

            #region port 处理
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
            if (port < 1 || port > 65535)
            {
                AppMessage.Show("Port is between 1 and 65535.");
                return -2;
            }
            #endregion

            #region 创建服务 并 启动
            try
            {
                Server.Server server = new Server.Server(port, vpath, path, false);
                server.Start();

                Application.SetCompatibleTextRenderingDefault(false);
                Application.EnableVisualStyles();
                Application.Run(new AppForm(server));
            }
            catch (Exception ex)
            {
                AppMessage.Show("端口［" + port + "］已被占用，或已成功建立服务器！");
                Process.Start("http://localhost:" + port);
                return -3;
            } 
            #endregion

            return 1;
        }
    }
}
