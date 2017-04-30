using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Iwenli.Simulateiis.UI
{
    public partial class About : BaseForm
    {
        public About()
        {
            InitializeComponent();
            this.richTextBox1.LinkClicked += (s, e) =>
            {
                Process.Start(Utility.Config.Blog);
            };
        }
    }
}
