using Iwenli.Simulateiis.Utility;
using Iwenli.Simulateiis.WebHost;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Iwenli.Simulateiis
{
    public partial class AppForm : Form
    {
        private Server m_server;

        public AppForm(Server server)
        {
            InitializeComponent();

            Init(server);
            BindEvent();
        }

        /// <summary>
        /// 初始化工作
        /// </summary>
        /// <param name="server"></param>
        private void Init(Server server)
        {
            this.WindowState = FormWindowState.Minimized;
            this.m_server = server;
            //托盘提示
            this.notify.Text = string.Format("{0} V{1} By-{2}", Config.Caption, Config.Version, Config.Author);// "AspNet网站运行助手V1.0 By-Iwenli";
            // 显示气泡提示
            string msg = string.Format("URL：{0}\r\nPath：{1}", m_server.RootUrl, m_server.PhysicalPath);
            this.notify.ShowBalloonTip(1000, Config.Caption, msg, ToolTipIcon.Info);
            //启动
            DoLaunch();
        }
        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvent()
        {

            //双击notify
            this.notify.DoubleClick += (s, e) => { DoLaunch(); };
            //在web中打开
            this.menuOpen.Click += (s, e) => { DoLaunch(); };
            //显示
            this.menuShow.Click += (s, e) => { new UI.Show(m_server.RootUrl, m_server.PhysicalPath).Show(); };
            //关于
            this.menuAbout.Click += (s, e) => { new UI.About().Show(); };
            //设置
            this.menuSetting.Click += (s, e) => { new UI.Setting(m_server.PhysicalPath).Show(); };
            //退出

            this.menuEixt.Click += (s, e) => {Application.Exit();};

        }
        /// <summary>
        /// 打开主页
        /// </summary>
        private void DoLaunch()
        {
            Process.Start(m_server.RootUrl);
        }
    }
}
