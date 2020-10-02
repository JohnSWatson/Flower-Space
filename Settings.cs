using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flower_Space
{
    public partial class Settings : Form
    {
        #region Form events
        public Settings()
        {
            InitializeComponent();
            // panelCameraViewNotConfigured.Refresh();

            panelCameraViewNotConfigured.BackColor = Properties.Settings.Default.CameraViewNotConfigured;
            panelCameraViewShowingCameraImage.BackColor = Properties.Settings.Default.CameraViewShowingCameraImage;
            panelCameraViewStreamingCameraImage.BackColor = Properties.Settings.Default.CameraViewStreamingCameraImage;
            panelCameraViewShowingSavedImage.BackColor = Properties.Settings.Default.CameraViewShowingSavedImage;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            this.Refresh();
        }

        #endregion  //Form events

        #region Camera viewer status colours
        private void panelCameraViewNotConfigured_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                PointF point = new Point(panelCameraViewNotConfigured.Width / 2, panelCameraViewNotConfigured.Height / 2);
                e.Graphics.DrawString("Not Used", this.Font, br, new PointF(170, 10), sf);
                panelCameraViewNotConfigured.BackColor = Properties.Settings.Default.CameraViewNotConfigured;
            }
        }

        private void panelCameraViewShowingCameraImage_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                PointF point = new Point(panelCameraViewNotConfigured.Width / 2, panelCameraViewNotConfigured.Height / 2);
                e.Graphics.DrawString("Camera Image", this.Font, br, new PointF(170, 10), sf);
                panelCameraViewShowingCameraImage.BackColor = Properties.Settings.Default.CameraViewShowingCameraImage;
            }
        }

        private void panelCameraViewStreamingCameraImage_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                PointF point = new Point(panelCameraViewNotConfigured.Width / 2, panelCameraViewNotConfigured.Height / 2);
                e.Graphics.DrawString("Streaming Video", this.Font, br, new PointF(170, 10), sf);
                panelCameraViewStreamingCameraImage.BackColor = Properties.Settings.Default.CameraViewStreamingCameraImage;
            }

        }

        private void panelCameraViewShowingSavedImage_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                PointF point = new Point(panelCameraViewNotConfigured.Width / 2, panelCameraViewNotConfigured.Height / 2);
                e.Graphics.DrawString("Saved Image", this.Font, br, new PointF(170, 10), sf);
                panelCameraViewShowingSavedImage.BackColor = Properties.Settings.Default.CameraViewShowingSavedImage;
            }
        }

        private void panelCameraViewNotConfigured_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            cd.Color = panelCameraViewNotConfigured.BackColor;

            DialogResult result = cd.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Set form background to the selected color.
                panelCameraViewNotConfigured.BackColor = cd.Color;
                Properties.Settings.Default.CameraViewNotConfigured = cd.Color;
                Properties.Settings.Default.Save();
            }
            this.Refresh();
        }

        private void panelCameraViewShowingCameraImage_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            cd.Color = panelCameraViewShowingCameraImage.BackColor;

            DialogResult result = cd.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Set form background to the selected color.
                panelCameraViewShowingCameraImage.BackColor = cd.Color;
                Properties.Settings.Default.CameraViewShowingCameraImage = cd.Color;
                Properties.Settings.Default.Save();
            }
            this.Refresh();
        }

        private void panelCameraViewStreamingCameraImage_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            cd.Color = panelCameraViewStreamingCameraImage.BackColor;

            DialogResult result = cd.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Set form background to the selected color.
                panelCameraViewStreamingCameraImage.BackColor = cd.Color;
                Properties.Settings.Default.CameraViewStreamingCameraImage = cd.Color;
                Properties.Settings.Default.Save();
            }
            this.Refresh();
        }

        private void panelCameraViewShowingSavedImage_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();

            cd.Color = panelCameraViewShowingSavedImage.BackColor;

            DialogResult result = cd.ShowDialog();

            if (result == DialogResult.OK)
            {
                // Set form background to the selected color.
                panelCameraViewShowingSavedImage.BackColor = cd.Color;
                Properties.Settings.Default.CameraViewShowingSavedImage = cd.Color;
                Properties.Settings.Default.Save();

            }
            this.Refresh();
        }

        #endregion  // Camera viewer status colours

    }
}
