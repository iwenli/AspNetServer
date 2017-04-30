using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Iwenli.Simulateiis.Utility
{
    public static class AppMessage
    {
        public static void Show(string msg)
        {
            if (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                MessageBox.Show(msg, Config.Caption, MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            }
            else
            {
                MessageBox.Show(msg, Config.Caption, MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }
        public static void Show(string msg, MessageBoxIcon icon)
        {
            if (Thread.CurrentThread.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                MessageBox.Show(msg, Config.Caption, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading);
            }
            else
            {
                MessageBox.Show(msg, Config.Caption, MessageBoxButtons.OK, icon);
            }
        }
    }
}
