using System.Diagnostics;

namespace AspNet20.UI
{
    public partial class Show : AspNet20.UI.BaseForm
    {
        public Show(string rootUrl, string path)
        {
            InitializeComponent();
            this.txtBoxPath.Text = path;
            this.lklabRootUrl.Text = rootUrl;
            lklabRootUrl.LinkClicked += (s, e) => { Process.Start(rootUrl); };
        }
    }
}
