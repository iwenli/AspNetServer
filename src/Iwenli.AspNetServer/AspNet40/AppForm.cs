using AspNet40.Utility;
using System.Diagnostics;
using System.Windows.Forms;

namespace AspNet40
{
    public partial class AppForm : Form
    {
        private Server.Server m_server;

        public AppForm(Server.Server server)
        {
            InitializeComponent();

            Init(server);
            BindEvent();
        }

        /// <summary>
        /// 初始化工作
        /// </summary>
        /// <param name="server"></param>
        private void Init(Server.Server server)
        {
            this.Text = Config.Caption;
            this.WindowState = FormWindowState.Minimized;
            this.m_server = server;
            //托盘提示
            this.notify.Text = string.Format("{0} V{1} By-{2}", Config.Caption, Config.Version, Config.Author);// "AspNet网站运行助手V1.0 By-Iwenli";
            // 显示气泡提示
            string msg = string.Format("URL：{0}\r\nPath：{1}", m_server.RootUrl, m_server.PhysicalPath);
            this.notify.ShowBalloonTip(1000, Config.Caption, msg, ToolTipIcon.Info);
            //启动
            Open();
        }
        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvent()
        {
            //双击notify
            this.notify.DoubleClick += (s, e) => { Open(); };
            //在web中打开
            this.menuOpen.Click += (s, e) => { Open(); };
            //显示
            this.menuShow.Click += (s, e) => { new UI.Show(m_server.RootUrl, m_server.PhysicalPath).Show(); };
            //关于
            this.menuAbout.Click += (s, e) => { new UI.About().Show(); };
            //设置
            this.menuSetting.Click += (s, e) => { new UI.Setting(m_server.PhysicalPath).Show(); };
            //退出
            this.menuEixt.Click += (s, e) => { Application.Exit(); };
        }
        /// <summary>
        /// 打开主页
        /// </summary>
        private void Open()
        {
            Process.Start(m_server.RootUrl);
        }
    }
}
