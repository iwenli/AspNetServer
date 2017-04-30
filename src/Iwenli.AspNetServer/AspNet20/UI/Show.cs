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
    public partial class Show : BaseForm
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
