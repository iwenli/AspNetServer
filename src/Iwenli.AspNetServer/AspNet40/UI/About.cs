using System.Diagnostics;

namespace AspNet40.UI
{
    public partial class About : BaseForm
    {
        public About()
        {
            InitializeComponent();
            this.richTextBox1.Text = "\n    将此程序拷贝至您的网站根目录，然后运行此程序，您的网站即可浏览了。" +
                                     "\n    如果您运行此助手程序遇到了什么问题还可以直接跟 作者<IWenli> 交流哦！" +
                                     "\n\n    博客：" + Utility.Config.Blog;

            this.richTextBox1.LinkClicked += (s, e) =>
            {
                Process.Start(Utility.Config.Blog);
            };
        }
    }
}
