using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AspNet40.Utility
{
    public static class RegisterHelper
    {
        /// <summary>
        /// 注册右键菜单
        /// </summary>
        public static void RegisterRightClick()
        {
            if (!File.Exists(Config.MenuStartPaht))
            {
                try
                {
                    RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("Directory\\shell\\", true).
                        CreateSubKey(Config.AppName);
                    registryKey.SetValue("", Config.MenuStartName);
                    registryKey.CreateSubKey("command").SetValue("", "\"" + Config.MenuStartPaht + "\" \"%1\"");
                    registryKey.Close();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("不允许"))
                    {
                        AppMessage.Show("在右键中启动服务器.\n需要以管理员权限运行!", MessageBoxIcon.Asterisk);
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
        }

        /// <summary>
        /// 取消右键菜单
        /// </summary>
        public static void CancelRightClick()
        {
            try
            {
                if (File.Exists(Config.MenuStartPaht))
                {
                    File.Delete(Config.MenuStartPaht);
                    Registry.ClassesRoot.OpenSubKey("Directory\\shell\\" + Config.AppName, true).DeleteSubKey("command");
                    Registry.ClassesRoot.OpenSubKey("Directory\\shell", true).DeleteSubKey(Config.AppName);
                }
            }
            catch
            {
            }
        }
    }
}
