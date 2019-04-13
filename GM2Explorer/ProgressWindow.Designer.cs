namespace GM2Explorer
{
    partial class ProgressWindow
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
            this.progressText = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressText
            // 
            this.progressText.AutoSize = true;
            this.progressText.Location = new System.Drawing.Point(9, 13);
            this.progressText.Name = "progressText";
            this.progressText.Size = new System.Drawing.Size(56, 13);
            this.progressText.TabIndex = 0;
            this.progressText.Text = "Working...";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 30);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(332, 38);
            this.progressBar.TabIndex = 1;
            // 
            // ProgressWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 77);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.progressText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Progress";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label progressText;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}