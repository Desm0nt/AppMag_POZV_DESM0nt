
namespace App
{
    partial class ResultView
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.sceneControl1 = new SharpGL.SceneControl();
            this.panel2 = new System.Windows.Forms.Panel();
            this.right1 = new System.Windows.Forms.Button();
            this.left1 = new System.Windows.Forms.Button();
            this.down = new System.Windows.Forms.Button();
            this.right = new System.Windows.Forms.Button();
            this.left = new System.Windows.Forms.Button();
            this.up = new System.Windows.Forms.Button();
            this.Far = new System.Windows.Forms.Button();
            this.btn3 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sceneControl1)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.sceneControl1);
            this.panel1.Location = new System.Drawing.Point(2, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(604, 634);
            this.panel1.TabIndex = 0;
            // 
            // sceneControl1
            // 
            this.sceneControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sceneControl1.DrawFPS = true;
            this.sceneControl1.FrameRate = 10;
            this.sceneControl1.Location = new System.Drawing.Point(0, 0);
            this.sceneControl1.Name = "sceneControl1";
            this.sceneControl1.OpenGLVersion = SharpGL.Version.OpenGLVersion.OpenGL2_1;
            this.sceneControl1.RenderContextType = SharpGL.RenderContextType.FBO;
            this.sceneControl1.RenderTrigger = SharpGL.RenderTrigger.TimerBased;
            this.sceneControl1.Size = new System.Drawing.Size(604, 634);
            this.sceneControl1.TabIndex = 6;
            this.sceneControl1.OpenGLInitialized += new System.EventHandler(this.sceneControl1_OpenGLInitialized);
            this.sceneControl1.OpenGLDraw += new SharpGL.RenderEventHandler(this.sceneControl1_OpenGLDraw);
            this.sceneControl1.Resized += new System.EventHandler(this.sceneControl1_Resized);
            this.sceneControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseDown);
            this.sceneControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseMove);
            this.sceneControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.openGlControl_MouseUp);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.right1);
            this.panel2.Controls.Add(this.left1);
            this.panel2.Controls.Add(this.down);
            this.panel2.Controls.Add(this.right);
            this.panel2.Controls.Add(this.left);
            this.panel2.Controls.Add(this.up);
            this.panel2.Controls.Add(this.Far);
            this.panel2.Controls.Add(this.btn3);
            this.panel2.Location = new System.Drawing.Point(612, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(211, 634);
            this.panel2.TabIndex = 1;
            // 
            // right1
            // 
            this.right1.Location = new System.Drawing.Point(137, 416);
            this.right1.Name = "right1";
            this.right1.Size = new System.Drawing.Size(52, 39);
            this.right1.TabIndex = 8;
            this.right1.Text = "right1";
            this.right1.UseVisualStyleBackColor = true;
            this.right1.Click += new System.EventHandler(this.right1_Click);
            // 
            // left1
            // 
            this.left1.Location = new System.Drawing.Point(13, 416);
            this.left1.Name = "left1";
            this.left1.Size = new System.Drawing.Size(52, 39);
            this.left1.TabIndex = 7;
            this.left1.Text = "left1";
            this.left1.UseVisualStyleBackColor = true;
            this.left1.Click += new System.EventHandler(this.left1_Click);
            // 
            // down
            // 
            this.down.Location = new System.Drawing.Point(76, 326);
            this.down.Name = "down";
            this.down.Size = new System.Drawing.Size(52, 39);
            this.down.TabIndex = 6;
            this.down.Text = "down";
            this.down.UseVisualStyleBackColor = true;
            this.down.Click += new System.EventHandler(this.down_Click);
            // 
            // right
            // 
            this.right.Location = new System.Drawing.Point(137, 284);
            this.right.Name = "right";
            this.right.Size = new System.Drawing.Size(52, 39);
            this.right.TabIndex = 5;
            this.right.Text = "right";
            this.right.UseVisualStyleBackColor = true;
            this.right.Click += new System.EventHandler(this.right_Click);
            // 
            // left
            // 
            this.left.Location = new System.Drawing.Point(13, 284);
            this.left.Name = "left";
            this.left.Size = new System.Drawing.Size(52, 39);
            this.left.TabIndex = 4;
            this.left.Text = "left";
            this.left.UseVisualStyleBackColor = true;
            this.left.Click += new System.EventHandler(this.left_Click);
            // 
            // up
            // 
            this.up.Location = new System.Drawing.Point(76, 241);
            this.up.Name = "up";
            this.up.Size = new System.Drawing.Size(52, 39);
            this.up.TabIndex = 3;
            this.up.Text = "up";
            this.up.UseVisualStyleBackColor = true;
            this.up.Click += new System.EventHandler(this.up_Click);

            // 
            // Far
            // 
            this.Far.Location = new System.Drawing.Point(13, 178);
            this.Far.Name = "Far";
            this.Far.Size = new System.Drawing.Size(52, 39);
            this.Far.TabIndex = 1;
            this.Far.Text = "-";
            this.Far.UseVisualStyleBackColor = true;
            this.Far.Click += new System.EventHandler(this.Far_Click);
            // 
            // btn3
            // 
            this.btn3.Location = new System.Drawing.Point(13, 105);
            this.btn3.Name = "btn3";
            this.btn3.Size = new System.Drawing.Size(176, 41);
            this.btn3.TabIndex = 0;
            this.btn3.Text = "Отобразить";
            this.btn3.UseVisualStyleBackColor = true;
            this.btn3.Click += new System.EventHandler(this.button1_Click);
            // 
            // ResultView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(825, 639);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "ResultView";
            this.Text = "ResultView";
            this.Load += new System.EventHandler(this.ResultView_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sceneControl1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private SharpGL.SceneControl sceneControl1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button right1;
        private System.Windows.Forms.Button left1;
        private System.Windows.Forms.Button down;
        private System.Windows.Forms.Button right;
        private System.Windows.Forms.Button left;
        private System.Windows.Forms.Button up;
        private System.Windows.Forms.Button Far;
        private System.Windows.Forms.Button btn3;
    }
}