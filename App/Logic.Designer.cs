namespace App
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    partial class Logic
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.открытьФайлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.открытьПапкуToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сохранитьToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderTimer = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.btn_zoom_plus = new System.Windows.Forms.Button();
            this.btn_zoom_minus = new System.Windows.Forms.Button();
            this.btn_render_start = new System.Windows.Forms.Button();
            this.btn_render_stop = new System.Windows.Forms.Button();
            this.btn_rotate_plus = new System.Windows.Forms.Button();
            this.btn_rotate_minus = new System.Windows.Forms.Button();
            this.cb_X = new System.Windows.Forms.CheckBox();
            this.cb_Y = new System.Windows.Forms.CheckBox();
            this.cb_Z = new System.Windows.Forms.CheckBox();
            this.panelOpenGl = new Tao.Platform.Windows.SimpleOpenGlControl();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(684, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.открытьФайлToolStripMenuItem,
            this.открытьПапкуToolStripMenuItem,
            this.сохранитьToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // открытьФайлToolStripMenuItem
            // 
            this.открытьФайлToolStripMenuItem.Name = "открытьФайлToolStripMenuItem";
            this.открытьФайлToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.открытьФайлToolStripMenuItem.Text = "Открыть файл";
            this.открытьФайлToolStripMenuItem.Click += new System.EventHandler(this.ОткрытьФайлToolStripMenuItem_Click);
            // 
            // открытьПапкуToolStripMenuItem
            // 
            this.открытьПапкуToolStripMenuItem.Name = "открытьПапкуToolStripMenuItem";
            this.открытьПапкуToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.открытьПапкуToolStripMenuItem.Text = "Открыть папку";
            this.открытьПапкуToolStripMenuItem.Click += new System.EventHandler(this.ОткрытьПапкуToolStripMenuItem_Click);
            // 
            // сохранитьToolStripMenuItem
            // 
            this.сохранитьToolStripMenuItem.Enabled = false;
            this.сохранитьToolStripMenuItem.Name = "сохранитьToolStripMenuItem";
            this.сохранитьToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.сохранитьToolStripMenuItem.Text = "Сохранить как...";
            this.сохранитьToolStripMenuItem.Click += new System.EventHandler(this.СохранитьToolStripMenuItem_Click);
            // 
            // renderTimer
            // 
            this.renderTimer.Interval = 50;
            this.renderTimer.Tick += new System.EventHandler(this.RenderTimer_Tick);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 535);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(684, 22);
            this.statusStrip1.TabIndex = 20;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(567, 17);
            this.toolStripStatusLabel1.Spring = true;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // btn_zoom_plus
            // 
            this.btn_zoom_plus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_zoom_plus.Enabled = false;
            this.btn_zoom_plus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_zoom_plus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btn_zoom_plus.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn_zoom_plus.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btn_zoom_plus.Location = new System.Drawing.Point(612, 27);
            this.btn_zoom_plus.Margin = new System.Windows.Forms.Padding(0);
            this.btn_zoom_plus.Name = "btn_zoom_plus";
            this.btn_zoom_plus.Size = new System.Drawing.Size(33, 33);
            this.btn_zoom_plus.TabIndex = 9;
            this.btn_zoom_plus.Text = "+";
            this.btn_zoom_plus.UseVisualStyleBackColor = false;
            this.btn_zoom_plus.Click += new System.EventHandler(this.Btn_zoom_plus_Click);
            // 
            // btn_zoom_minus
            // 
            this.btn_zoom_minus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_zoom_minus.BackColor = System.Drawing.SystemColors.Control;
            this.btn_zoom_minus.Enabled = false;
            this.btn_zoom_minus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_zoom_minus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold);
            this.btn_zoom_minus.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn_zoom_minus.Location = new System.Drawing.Point(644, 27);
            this.btn_zoom_minus.Margin = new System.Windows.Forms.Padding(0);
            this.btn_zoom_minus.Name = "btn_zoom_minus";
            this.btn_zoom_minus.Size = new System.Drawing.Size(33, 33);
            this.btn_zoom_minus.TabIndex = 10;
            this.btn_zoom_minus.Text = "-";
            this.btn_zoom_minus.UseVisualStyleBackColor = false;
            this.btn_zoom_minus.Click += new System.EventHandler(this.Btn_zoom_minus_Click);
            // 
            // btn_render_start
            // 
            this.btn_render_start.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_render_start.Enabled = false;
            this.btn_render_start.Location = new System.Drawing.Point(201, 515);
            this.btn_render_start.Name = "btn_render_start";
            this.btn_render_start.Size = new System.Drawing.Size(76, 22);
            this.btn_render_start.TabIndex = 12;
            this.btn_render_start.Text = "Start";
            this.btn_render_start.UseVisualStyleBackColor = true;
            this.btn_render_start.Click += new System.EventHandler(this.Btn_render_start_Click);
            // 
            // btn_render_stop
            // 
            this.btn_render_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_render_stop.Enabled = false;
            this.btn_render_stop.Location = new System.Drawing.Point(283, 515);
            this.btn_render_stop.Name = "btn_render_stop";
            this.btn_render_stop.Size = new System.Drawing.Size(74, 22);
            this.btn_render_stop.TabIndex = 13;
            this.btn_render_stop.Text = "Stop";
            this.btn_render_stop.UseVisualStyleBackColor = true;
            this.btn_render_stop.Click += new System.EventHandler(this.Btn_render_stop_Click);
            // 
            // btn_rotate_plus
            // 
            this.btn_rotate_plus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_rotate_plus.Enabled = false;
            this.btn_rotate_plus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_rotate_plus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btn_rotate_plus.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn_rotate_plus.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btn_rotate_plus.Location = new System.Drawing.Point(645, 476);
            this.btn_rotate_plus.Margin = new System.Windows.Forms.Padding(0);
            this.btn_rotate_plus.Name = "btn_rotate_plus";
            this.btn_rotate_plus.Size = new System.Drawing.Size(33, 33);
            this.btn_rotate_plus.TabIndex = 14;
            this.btn_rotate_plus.Text = ">";
            this.btn_rotate_plus.UseVisualStyleBackColor = false;
            this.btn_rotate_plus.Click += new System.EventHandler(this.Btn_rotate_plus_Click);
            // 
            // btn_rotate_minus
            // 
            this.btn_rotate_minus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_rotate_minus.BackColor = System.Drawing.SystemColors.Control;
            this.btn_rotate_minus.Enabled = false;
            this.btn_rotate_minus.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_rotate_minus.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold);
            this.btn_rotate_minus.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btn_rotate_minus.Location = new System.Drawing.Point(612, 476);
            this.btn_rotate_minus.Margin = new System.Windows.Forms.Padding(0);
            this.btn_rotate_minus.Name = "btn_rotate_minus";
            this.btn_rotate_minus.Size = new System.Drawing.Size(33, 33);
            this.btn_rotate_minus.TabIndex = 15;
            this.btn_rotate_minus.Text = "<";
            this.btn_rotate_minus.UseVisualStyleBackColor = false;
            this.btn_rotate_minus.Click += new System.EventHandler(this.Btn_rotate_minus_Click);
            // 
            // cb_X
            // 
            this.cb_X.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_X.AutoSize = true;
            this.cb_X.Enabled = false;
            this.cb_X.ForeColor = System.Drawing.Color.Red;
            this.cb_X.Location = new System.Drawing.Point(566, 519);
            this.cb_X.Name = "cb_X";
            this.cb_X.Size = new System.Drawing.Size(33, 17);
            this.cb_X.TabIndex = 16;
            this.cb_X.Text = "X";
            this.cb_X.UseVisualStyleBackColor = true;
            // 
            // cb_Y
            // 
            this.cb_Y.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_Y.AutoSize = true;
            this.cb_Y.Enabled = false;
            this.cb_Y.ForeColor = System.Drawing.Color.Green;
            this.cb_Y.Location = new System.Drawing.Point(605, 519);
            this.cb_Y.Name = "cb_Y";
            this.cb_Y.Size = new System.Drawing.Size(33, 17);
            this.cb_Y.TabIndex = 17;
            this.cb_Y.Text = "Y";
            this.cb_Y.UseVisualStyleBackColor = true;
            // 
            // cb_Z
            // 
            this.cb_Z.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cb_Z.AutoSize = true;
            this.cb_Z.Enabled = false;
            this.cb_Z.ForeColor = System.Drawing.Color.Blue;
            this.cb_Z.Location = new System.Drawing.Point(644, 519);
            this.cb_Z.Name = "cb_Z";
            this.cb_Z.Size = new System.Drawing.Size(33, 17);
            this.cb_Z.TabIndex = 18;
            this.cb_Z.Text = "Z";
            this.cb_Z.UseVisualStyleBackColor = true;
            // 
            // panelOpenGl
            // 
            this.panelOpenGl.AccumBits = ((byte)(0));
            this.panelOpenGl.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.panelOpenGl.AutoCheckErrors = false;
            this.panelOpenGl.AutoFinish = false;
            this.panelOpenGl.AutoMakeCurrent = true;
            this.panelOpenGl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelOpenGl.AutoSwapBuffers = true;
            this.panelOpenGl.BackColor = System.Drawing.Color.Black;
            this.panelOpenGl.ColorBits = ((byte)(32));
            this.panelOpenGl.DepthBits = ((byte)(16));
            this.panelOpenGl.Location = new System.Drawing.Point(201, 26);
            this.panelOpenGl.Name = "panelOpenGl";
            this.panelOpenGl.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.panelOpenGl.Size = new System.Drawing.Size(476, 483);
            this.panelOpenGl.StencilBits = ((byte)(0));
            this.panelOpenGl.TabIndex = 8;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.Location = new System.Drawing.Point(10, 27);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(185, 312);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Управление списком файлов";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(101, 17);
            this.radioButton1.TabIndex = 4;
            this.radioButton1.Text = "Показать слой";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.Location = new System.Drawing.Point(8, 67);
            this.listBox1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(169, 238);
            this.listBox1.TabIndex = 28;
            this.listBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListBox1_MouseDoubleClick);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(6, 42);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(95, 17);
            this.radioButton3.TabIndex = 3;
            this.radioButton3.Text = "Удалить слой";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(18, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 29);
            this.button1.TabIndex = 31;
            this.button1.Text = "Создание модели";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Enabled = false;
            this.groupBox2.Location = new System.Drawing.Point(10, 340);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(185, 109);
            this.groupBox2.TabIndex = 32;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "МКЭ";
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(18, 62);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(128, 29);
            this.button2.TabIndex = 32;
            this.button2.Text = "Расчет";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Logic
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 557);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cb_Z);
            this.Controls.Add(this.cb_Y);
            this.Controls.Add(this.cb_X);
            this.Controls.Add(this.btn_rotate_minus);
            this.Controls.Add(this.btn_rotate_plus);
            this.Controls.Add(this.btn_render_stop);
            this.Controls.Add(this.btn_render_start);
            this.Controls.Add(this.btn_zoom_minus);
            this.Controls.Add(this.btn_zoom_plus);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.panelOpenGl);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Logic";
            this.Text = "CT-Scene";
            this.Load += new System.EventHandler(this.Logic_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private OpenFileDialog openFileDialog1;
        private FolderBrowserDialog folderBrowserDialog1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem файлToolStripMenuItem;
        private ToolStripMenuItem открытьФайлToolStripMenuItem;
        private ToolStripMenuItem открытьПапкуToolStripMenuItem;
        private Timer renderTimer;
        private ToolStripMenuItem сохранитьToolStripMenuItem;
        private SaveFileDialog saveFileDialog1;
        private StatusStrip statusStrip1;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private Button btn_zoom_plus;
        private Button btn_zoom_minus;
        private Button btn_render_start;
        private Button btn_render_stop;
        private Button btn_rotate_plus;
        private Button btn_rotate_minus;
        private CheckBox cb_X;
        private CheckBox cb_Y;
        private CheckBox cb_Z;
        private Tao.Platform.Windows.SimpleOpenGlControl panelOpenGl;
        private GroupBox groupBox1;
        private RadioButton radioButton1;
        private RadioButton radioButton3;
        private ListBox listBox1;
        private Button button1;
        private GroupBox groupBox2;
        private Button button2;
    }
}

