namespace AspNet20.UI
{
    partial class Show
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtBoxPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lklabRootUrl = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.txtBoxPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.lklabRootUrl);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(386, 114);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "运行信息";
            // 
            // txtBoxPath
            // 
            this.txtBoxPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBoxPath.Location = new System.Drawing.Point(90, 33);
            this.txtBoxPath.Name = "txtBoxPath";
            this.txtBoxPath.Size = new System.Drawing.Size(272, 21);
            this.txtBoxPath.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "程序根URL：";
            // 
            // lklabRootUrl
            // 
            this.lklabRootUrl.AutoSize = true;
            this.lklabRootUrl.Location = new System.Drawing.Point(88, 69);
            this.lklabRootUrl.Name = "lklabRootUrl";
            this.lklabRootUrl.Size = new System.Drawing.Size(65, 12);
            this.lklabRootUrl.TabIndex = 1;
            this.lklabRootUrl.TabStop = true;
            this.lklabRootUrl.Text = "linkLabel1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "物理路径：";
            // 
            // Show
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(411, 143);
            this.Controls.Add(this.groupBox1);
            this.Name = "Show";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel lklabRootUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBoxPath;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}
