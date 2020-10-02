namespace Flower_Space
{
    partial class Settings
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
            this.label1 = new System.Windows.Forms.Label();
            this.panelCameraViewNotConfigured = new System.Windows.Forms.Panel();
            this.panelCameraViewShowingCameraImage = new System.Windows.Forms.Panel();
            this.panelCameraViewStreamingCameraImage = new System.Windows.Forms.Panel();
            this.panelCameraViewShowingSavedImage = new System.Windows.Forms.Panel();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(193, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Camera viewer status colours";
            // 
            // panelCameraViewNotConfigured
            // 
            this.panelCameraViewNotConfigured.BackColor = System.Drawing.SystemColors.Control;
            this.panelCameraViewNotConfigured.Location = new System.Drawing.Point(33, 110);
            this.panelCameraViewNotConfigured.Name = "panelCameraViewNotConfigured";
            this.panelCameraViewNotConfigured.Size = new System.Drawing.Size(440, 17);
            this.panelCameraViewNotConfigured.TabIndex = 1;
            this.panelCameraViewNotConfigured.Click += new System.EventHandler(this.panelCameraViewNotConfigured_Click);
            this.panelCameraViewNotConfigured.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCameraViewNotConfigured_Paint);
            // 
            // panelCameraViewShowingCameraImage
            // 
            this.panelCameraViewShowingCameraImage.Location = new System.Drawing.Point(33, 132);
            this.panelCameraViewShowingCameraImage.Name = "panelCameraViewShowingCameraImage";
            this.panelCameraViewShowingCameraImage.Size = new System.Drawing.Size(440, 17);
            this.panelCameraViewShowingCameraImage.TabIndex = 2;
            this.panelCameraViewShowingCameraImage.Click += new System.EventHandler(this.panelCameraViewShowingCameraImage_Click);
            this.panelCameraViewShowingCameraImage.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCameraViewShowingCameraImage_Paint);
            // 
            // panelCameraViewStreamingCameraImage
            // 
            this.panelCameraViewStreamingCameraImage.Location = new System.Drawing.Point(33, 154);
            this.panelCameraViewStreamingCameraImage.Name = "panelCameraViewStreamingCameraImage";
            this.panelCameraViewStreamingCameraImage.Size = new System.Drawing.Size(440, 17);
            this.panelCameraViewStreamingCameraImage.TabIndex = 2;
            this.panelCameraViewStreamingCameraImage.Click += new System.EventHandler(this.panelCameraViewStreamingCameraImage_Click);
            this.panelCameraViewStreamingCameraImage.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCameraViewStreamingCameraImage_Paint);
            // 
            // panelCameraViewShowingSavedImage
            // 
            this.panelCameraViewShowingSavedImage.Location = new System.Drawing.Point(33, 176);
            this.panelCameraViewShowingSavedImage.Name = "panelCameraViewShowingSavedImage";
            this.panelCameraViewShowingSavedImage.Size = new System.Drawing.Size(440, 17);
            this.panelCameraViewShowingSavedImage.TabIndex = 2;
            this.panelCameraViewShowingSavedImage.Click += new System.EventHandler(this.panelCameraViewShowingSavedImage_Click);
            this.panelCameraViewShowingSavedImage.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCameraViewShowingSavedImage_Paint);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(642, 450);
            this.Controls.Add(this.panelCameraViewStreamingCameraImage);
            this.Controls.Add(this.panelCameraViewShowingSavedImage);
            this.Controls.Add(this.panelCameraViewShowingCameraImage);
            this.Controls.Add(this.panelCameraViewNotConfigured);
            this.Controls.Add(this.label1);
            this.Name = "Settings";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelCameraViewNotConfigured;
        private System.Windows.Forms.Panel panelCameraViewShowingCameraImage;
        private System.Windows.Forms.Panel panelCameraViewStreamingCameraImage;
        private System.Windows.Forms.Panel panelCameraViewShowingSavedImage;
        private System.Windows.Forms.ColorDialog colorDialog1;
    }
}