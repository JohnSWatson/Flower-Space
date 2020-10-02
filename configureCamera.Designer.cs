namespace Flower_Space
{
    partial class configureCamera
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(configureCamera));
            this.comboCameras = new System.Windows.Forms.ComboBox();
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            this.textBoxViewDescription = new System.Windows.Forms.TextBox();
            this.bloomViewsList = new System.Windows.Forms.CheckedListBox();
            this.cameraAngleList = new System.Windows.Forms.CheckedListBox();
            this.textBoxCameraAngle = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBoxCameraAngle = new System.Windows.Forms.PictureBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCameraAngle)).BeginInit();
            this.SuspendLayout();
            // 
            // comboCameras
            // 
            this.comboCameras.FormattingEnabled = true;
            this.comboCameras.Location = new System.Drawing.Point(12, 22);
            this.comboCameras.Name = "comboCameras";
            this.comboCameras.Size = new System.Drawing.Size(317, 24);
            this.comboCameras.TabIndex = 1;
            this.comboCameras.SelectedIndexChanged += new System.EventHandler(this.comboCameras_SelectedIndexChanged);
            // 
            // buttonYes
            // 
            this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonYes.Enabled = false;
            this.buttonYes.Location = new System.Drawing.Point(642, 472);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(77, 32);
            this.buttonYes.TabIndex = 6;
            this.buttonYes.Text = "OK";
            this.buttonYes.UseVisualStyleBackColor = true;
            this.buttonYes.Click += new System.EventHandler(this.buttonYes_Click);
            // 
            // buttonNo
            // 
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonNo.Location = new System.Drawing.Point(725, 472);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(77, 32);
            this.buttonNo.TabIndex = 5;
            this.buttonNo.Text = "Cancel";
            this.buttonNo.UseVisualStyleBackColor = true;
            this.buttonNo.Click += new System.EventHandler(this.buttonNo_Click);
            // 
            // textBoxViewDescription
            // 
            this.textBoxViewDescription.Enabled = false;
            this.textBoxViewDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxViewDescription.Location = new System.Drawing.Point(15, 290);
            this.textBoxViewDescription.Multiline = true;
            this.textBoxViewDescription.Name = "textBoxViewDescription";
            this.textBoxViewDescription.Size = new System.Drawing.Size(207, 176);
            this.textBoxViewDescription.TabIndex = 8;
            // 
            // bloomViewsList
            // 
            this.bloomViewsList.CheckOnClick = true;
            this.bloomViewsList.FormattingEnabled = true;
            this.bloomViewsList.Location = new System.Drawing.Point(15, 76);
            this.bloomViewsList.Name = "bloomViewsList";
            this.bloomViewsList.Size = new System.Drawing.Size(207, 208);
            this.bloomViewsList.TabIndex = 2;
            this.bloomViewsList.SelectedIndexChanged += new System.EventHandler(this.bloomViewsList_SelectedIndexChanged);
            // 
            // cameraAngleList
            // 
            this.cameraAngleList.CheckOnClick = true;
            this.cameraAngleList.FormattingEnabled = true;
            this.cameraAngleList.Location = new System.Drawing.Point(228, 76);
            this.cameraAngleList.Name = "cameraAngleList";
            this.cameraAngleList.Size = new System.Drawing.Size(207, 208);
            this.cameraAngleList.TabIndex = 3;
            this.cameraAngleList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.cameraAngleList_ItemCheck);
            this.cameraAngleList.SelectedIndexChanged += new System.EventHandler(this.cameraAngleList_SelectedIndexChanged);
            // 
            // textBoxCameraAngle
            // 
            this.textBoxCameraAngle.Enabled = false;
            this.textBoxCameraAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxCameraAngle.Location = new System.Drawing.Point(228, 290);
            this.textBoxCameraAngle.Multiline = true;
            this.textBoxCameraAngle.Name = "textBoxCameraAngle";
            this.textBoxCameraAngle.Size = new System.Drawing.Size(207, 176);
            this.textBoxCameraAngle.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "Select camera";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(175, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "View(s) of the bloom taken";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(228, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 17);
            this.label4.TabIndex = 15;
            this.label4.Text = "Camera angle";
            // 
            // pictureBoxCameraAngle
            // 
            this.pictureBoxCameraAngle.BackColor = System.Drawing.Color.White;
            this.pictureBoxCameraAngle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxCameraAngle.Image = global::Flower_Space.Properties.Resources.Spherical_Coordinates;
            this.pictureBoxCameraAngle.Location = new System.Drawing.Point(442, 76);
            this.pictureBoxCameraAngle.Name = "pictureBoxCameraAngle";
            this.pictureBoxCameraAngle.Size = new System.Drawing.Size(206, 208);
            this.pictureBoxCameraAngle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxCameraAngle.TabIndex = 16;
            this.pictureBoxCameraAngle.TabStop = false;
            // 
            // textBox1
            // 
            this.textBox1.Enabled = false;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(442, 290);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(360, 176);
            this.textBox1.TabIndex = 17;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(441, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(197, 17);
            this.label5.TabIndex = 18;
            this.label5.Text = "How camera angle are named";
            // 
            // configureCamera
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonNo;
            this.ClientSize = new System.Drawing.Size(815, 552);
            this.ControlBox = false;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.pictureBoxCameraAngle);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxCameraAngle);
            this.Controls.Add(this.cameraAngleList);
            this.Controls.Add(this.bloomViewsList);
            this.Controls.Add(this.textBoxViewDescription);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYes);
            this.Controls.Add(this.comboCameras);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "configureCamera";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.configureCamera_FormClosing);
            this.Load += new System.EventHandler(this.configureCamera_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCameraAngle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox comboCameras;
        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.Button buttonNo;
        private System.Windows.Forms.TextBox textBoxViewDescription;
        private System.Windows.Forms.CheckedListBox bloomViewsList;
        private System.Windows.Forms.CheckedListBox cameraAngleList;
        private System.Windows.Forms.TextBox textBoxCameraAngle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBoxCameraAngle;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
    }
}