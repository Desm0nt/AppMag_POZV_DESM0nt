namespace App
{
    partial class AppMainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppMainForm));
            this.DrawTimer = new System.Windows.Forms.Timer(this.components);
            this.GL_Monitor2 = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // DrawTimer
            // 
            this.DrawTimer.Enabled = true;
            this.DrawTimer.Interval = 25;
            this.DrawTimer.Tick += new System.EventHandler(this.DrawTimer_Tick);
            // 
            // GL_Monitor2
            // 
            this.GL_Monitor2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.GL_Monitor2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GL_Monitor2.Location = new System.Drawing.Point(0, 0);
            this.GL_Monitor2.Name = "GL_Monitor2";
            this.GL_Monitor2.Size = new System.Drawing.Size(984, 546);
            this.GL_Monitor2.TabIndex = 15;
            this.GL_Monitor2.VSync = false;
            this.GL_Monitor2.Load += new System.EventHandler(this.GL_Monitor_Load);
            this.GL_Monitor2.DragDrop += new System.Windows.Forms.DragEventHandler(this.GL_Monitor_DragDrop);
            this.GL_Monitor2.DragEnter += new System.Windows.Forms.DragEventHandler(this.GL_Monitor_DragEnter);
            this.GL_Monitor2.Paint += new System.Windows.Forms.PaintEventHandler(this.GL_Monitor_Paint);
            // 
            // AppMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 546);
            this.Controls.Add(this.GL_Monitor2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AppMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "STL Viewer";
            this.Load += new System.EventHandler(this.AppMainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer DrawTimer;
        private OpenTK.GLControl GL_Monitor2;
    }
}

