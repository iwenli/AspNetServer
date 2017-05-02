using System.Windows.Forms;

namespace AspNet40.UI
{
    public partial class BaseForm : Form
    {
        public BaseForm()
        {
            InitializeComponent();
            this.Text = Utility.Config.Caption + " V" + Utility.Config.Version;
        }
    }
}
