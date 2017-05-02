using AspNet20.Utility;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AspNet20.UI
{
    public partial class Setting : BaseForm
    {
        private string m_webPath = string.Empty;
        public Setting(string path)
        {
            this.m_webPath = path;
            InitializeComponent();
            this.Load += (sender, e) =>
            {
                Init();
                BindEvent();
            };
        }
        /// <summary>
        /// 初始化控件状态
        /// </summary>
        private void Init()
        {
            if (File.Exists(m_webPath + Utility.Config.PortIniPath))
            {
                try
                {
                    StreamReader streamReader = new StreamReader(m_webPath + Utility.Config.PortIniPath, Encoding.UTF8);
                    this.txtPort.Text = streamReader.ReadToEnd();
                    streamReader.Close();
                }
                catch
                {
                }
            }
            try
            {
                RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("Directory\\shell\\" + Config.AppName);
                string text = registryKey.GetValue(null).ToString();
                if (registryKey.GetValue(null).ToString().Trim() == Config.MenuStartName)
                {
                    this.chkAddMenu.Checked = true;
                }
                registryKey.Close();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvent()
        {
            //只能输入数字
            txtPort.KeyPress += (sender, e) =>
            {
                if (e.KeyChar != '\b' && (e.KeyChar < 47 || e.KeyChar > 56))
                {
                    e.Handled = true;
                }
            };
            //值必须在1~65535之间
            txtPort.TextChanged += (sender, e) =>
            {
                TextBox currentBox = (TextBox)sender;
                if (currentBox.Text.Length > 0 && currentBox.Text.Length < 6)
                {
                    int currentValue = int.Parse(currentBox.Text);
                    if (currentValue < 1 || currentValue > 65535)
                    {
                        currentBox.Text = currentValue == 0 ? "1" : "65535";
                        AppMessage.Show("端口只能是 1 ~ 65535 之间的数字，并且还不能被占用！");
                    }
                }
            };
            //保存
            btnSave.Click += (sender, e) =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(this.txtPort.Text))
                    {
                        StreamWriter streamWriter = new StreamWriter(m_webPath + Config.PortIniPath, false, Encoding.UTF8);
                        streamWriter.Write(this.txtPort.Text);
                        streamWriter.Close();
                    }

                    if (this.chkAddMenu.Checked)
                    {
                        RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("Directory\\shell\\", true).CreateSubKey(Config.AppName);
                        registryKey.SetValue("", Config.MenuStartName);
                        registryKey.CreateSubKey("command").SetValue("", "\"" + Config.MenuStartPaht + "\" \"%1\"");
                        registryKey.Close();
                    }
                    else
                    {
                        Registry.ClassesRoot.OpenSubKey("Directory\\shell\\" + Config.AppName, true).DeleteSubKey("command");
                        Registry.ClassesRoot.OpenSubKey("Directory\\shell", true).DeleteSubKey(Config.AppName);
                    }
                    Application.Restart();
                    base.Close();
                }
                catch (Exception ex)
                {
                    AppMessage.Show(ex.Message);
                }
            };
        }
    }
}
