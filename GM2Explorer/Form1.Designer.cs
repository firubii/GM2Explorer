namespace GM2Explorer
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TXTRtab = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textureDisplay = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.texList = new System.Windows.Forms.ListBox();
            this.textureContextStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AUDOtab = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.filelabel = new System.Windows.Forms.Label();
            this.playPause = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.audioList = new System.Windows.Forms.TreeView();
            this.audioContextStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportAudioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllAudioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.replaceTextureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.TXTRtab.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textureDisplay)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.textureContextStrip.SuspendLayout();
            this.AUDOtab.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.audioContextStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.TXTRtab);
            this.tabControl1.Controls.Add(this.AUDOtab);
            this.tabControl1.Location = new System.Drawing.Point(13, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(775, 410);
            this.tabControl1.TabIndex = 1;
            // 
            // TXTRtab
            // 
            this.TXTRtab.Controls.Add(this.groupBox2);
            this.TXTRtab.Controls.Add(this.groupBox1);
            this.TXTRtab.Location = new System.Drawing.Point(4, 22);
            this.TXTRtab.Name = "TXTRtab";
            this.TXTRtab.Padding = new System.Windows.Forms.Padding(3);
            this.TXTRtab.Size = new System.Drawing.Size(767, 384);
            this.TXTRtab.TabIndex = 1;
            this.TXTRtab.Text = "Textures";
            this.TXTRtab.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textureDisplay);
            this.groupBox2.Location = new System.Drawing.Point(272, 7);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(489, 371);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Preview";
            // 
            // textureDisplay
            // 
            this.textureDisplay.Location = new System.Drawing.Point(77, 12);
            this.textureDisplay.Name = "textureDisplay";
            this.textureDisplay.Size = new System.Drawing.Size(350, 350);
            this.textureDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.textureDisplay.TabIndex = 1;
            this.textureDisplay.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.texList);
            this.groupBox1.Location = new System.Drawing.Point(7, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(259, 371);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Textures";
            // 
            // texList
            // 
            this.texList.ContextMenuStrip = this.textureContextStrip;
            this.texList.FormattingEnabled = true;
            this.texList.Location = new System.Drawing.Point(7, 20);
            this.texList.Name = "texList";
            this.texList.Size = new System.Drawing.Size(246, 342);
            this.texList.TabIndex = 0;
            this.texList.SelectedIndexChanged += new System.EventHandler(this.texList_SelectedIndexChanged);
            // 
            // textureContextStrip
            // 
            this.textureContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveTextureToolStripMenuItem,
            this.exportAllTexturesToolStripMenuItem,
            this.replaceTextureToolStripMenuItem});
            this.textureContextStrip.Name = "contextMenuStrip1";
            this.textureContextStrip.Size = new System.Drawing.Size(181, 92);
            // 
            // saveTextureToolStripMenuItem
            // 
            this.saveTextureToolStripMenuItem.Name = "saveTextureToolStripMenuItem";
            this.saveTextureToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveTextureToolStripMenuItem.Text = "Export Texture";
            this.saveTextureToolStripMenuItem.Click += new System.EventHandler(this.saveTextureToolStripMenuItem_Click);
            // 
            // exportAllTexturesToolStripMenuItem
            // 
            this.exportAllTexturesToolStripMenuItem.Name = "exportAllTexturesToolStripMenuItem";
            this.exportAllTexturesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exportAllTexturesToolStripMenuItem.Text = "Export all Textures";
            this.exportAllTexturesToolStripMenuItem.Click += new System.EventHandler(this.exportAllTexturesToolStripMenuItem_Click);
            // 
            // AUDOtab
            // 
            this.AUDOtab.Controls.Add(this.groupBox3);
            this.AUDOtab.Controls.Add(this.groupBox4);
            this.AUDOtab.Location = new System.Drawing.Point(4, 22);
            this.AUDOtab.Name = "AUDOtab";
            this.AUDOtab.Padding = new System.Windows.Forms.Padding(3);
            this.AUDOtab.Size = new System.Drawing.Size(767, 384);
            this.AUDOtab.TabIndex = 2;
            this.AUDOtab.Text = "Audio";
            this.AUDOtab.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.filelabel);
            this.groupBox3.Controls.Add(this.playPause);
            this.groupBox3.Controls.Add(this.trackBar1);
            this.groupBox3.Location = new System.Drawing.Point(272, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(489, 371);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Preview";
            // 
            // filelabel
            // 
            this.filelabel.AutoSize = true;
            this.filelabel.Location = new System.Drawing.Point(7, 154);
            this.filelabel.Name = "filelabel";
            this.filelabel.Size = new System.Drawing.Size(29, 13);
            this.filelabel.TabIndex = 4;
            this.filelabel.Text = "File: ";
            // 
            // playPause
            // 
            this.playPause.Location = new System.Drawing.Point(193, 224);
            this.playPause.Name = "playPause";
            this.playPause.Size = new System.Drawing.Size(75, 23);
            this.playPause.TabIndex = 1;
            this.playPause.Text = "Play";
            this.playPause.UseVisualStyleBackColor = true;
            this.playPause.Click += new System.EventHandler(this.playPause_Click);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(6, 173);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(477, 45);
            this.trackBar1.TabIndex = 0;
            this.trackBar1.TickFrequency = 5000;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.audioList);
            this.groupBox4.Location = new System.Drawing.Point(7, 7);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(259, 371);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Audio";
            // 
            // audioList
            // 
            this.audioList.ContextMenuStrip = this.audioContextStrip;
            this.audioList.Location = new System.Drawing.Point(7, 20);
            this.audioList.Name = "audioList";
            this.audioList.Size = new System.Drawing.Size(246, 342);
            this.audioList.TabIndex = 0;
            this.audioList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.audioList_AfterSelect);
            // 
            // audioContextStrip
            // 
            this.audioContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAudioToolStripMenuItem,
            this.exportAllAudioToolStripMenuItem});
            this.audioContextStrip.Name = "audioContextStrip";
            this.audioContextStrip.Size = new System.Drawing.Size(158, 48);
            // 
            // exportAudioToolStripMenuItem
            // 
            this.exportAudioToolStripMenuItem.Name = "exportAudioToolStripMenuItem";
            this.exportAudioToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.exportAudioToolStripMenuItem.Text = "Export Audio";
            this.exportAudioToolStripMenuItem.Click += new System.EventHandler(this.exportAudioToolStripMenuItem_Click);
            // 
            // exportAllAudioToolStripMenuItem
            // 
            this.exportAllAudioToolStripMenuItem.Name = "exportAllAudioToolStripMenuItem";
            this.exportAllAudioToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.exportAllAudioToolStripMenuItem.Text = "Export all Audio";
            this.exportAllAudioToolStripMenuItem.Click += new System.EventHandler(this.exportAllAudioToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 441);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusProgress
            // 
            this.statusProgress.Name = "statusProgress";
            this.statusProgress.Size = new System.Drawing.Size(100, 16);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // replaceTextureToolStripMenuItem
            // 
            this.replaceTextureToolStripMenuItem.Name = "replaceTextureToolStripMenuItem";
            this.replaceTextureToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.replaceTextureToolStripMenuItem.Text = "Replace Texture";
            this.replaceTextureToolStripMenuItem.Click += new System.EventHandler(this.replaceTextureToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 463);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GM2Explorer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.TXTRtab.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.textureDisplay)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.textureContextStrip.ResumeLayout(false);
            this.AUDOtab.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.audioContextStrip.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage TXTRtab;
        private System.Windows.Forms.PictureBox textureDisplay;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox texList;
        private System.Windows.Forms.ContextMenuStrip textureContextStrip;
        private System.Windows.Forms.ToolStripMenuItem saveTextureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllTexturesToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar statusProgress;
        private System.Windows.Forms.TabPage AUDOtab;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TreeView audioList;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Button playPause;
        private System.Windows.Forms.Label filelabel;
        private System.Windows.Forms.ContextMenuStrip audioContextStrip;
        private System.Windows.Forms.ToolStripMenuItem exportAudioToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllAudioToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceTextureToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    }
}

