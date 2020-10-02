using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AForge.Video;
using AForge.Video.DirectShow;

namespace Flower_Space
{
    public partial class configureCamera : Form
    {
        #region Private values and Properties
        private bool formLoading;               // needed to ignor some process when loading;
        private int viewerIndex = -1;
        private bool thumnailSet;
        public bool ThumnailSet
        {
            get { return thumnailSet; }
            set { thumnailSet = value; }
        }

        private string[] assignedCameraAngles;
        public string[] AssignedCameraAngles
        {
            get { return assignedCameraAngles; }
            set { assignedCameraAngles = value; }
        }

        #endregion

        private CameraDevice _cameraDevice;
        private CameraAngles _cameraAngles;
        private Views _bloomViews;

        private System.Windows.Forms.PictureBox cameraForViewer;

        /// <summary>
        /// Class constructor  initialise some values
        /// Get a collection of cameras attached to the computer
        /// Add the camera name to a combo box
        /// </summary>
        public configureCamera(CameraDevice cameraDevice, System.Windows.Forms.PictureBox CameraForViewer, int ViewerIndex,  Views bloomViews, CameraAngles cameraAngles)
        {
            InitializeComponent();

            _cameraDevice = cameraDevice;
            viewerIndex = ViewerIndex;
            cameraForViewer = CameraForViewer;
            _cameraAngles = cameraAngles;
            _bloomViews = bloomViews;
            int cameraCount = _cameraDevice.Count;

            //
            // Set the form header
            //
            this.Text = "Configuring camera viewer " + (ViewerIndex + 1).ToString();

            cameraDevice.LoadListOfAvailableCameras(this.comboCameras);
        }

        /// <summary>
        /// Handel camera selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            _cameraDevice.AllocateCamera(comboCameras.Text, cameraForViewer , viewerIndex);

            ValidateMe();

        }


        /// <summary>
        /// Stop the active (started) camera before the form closes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void configureCamera_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        /// <summary>
        /// Initialise the values from stored configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void configureCamera_Load(object sender, EventArgs e)
        {
            formLoading = true;

            //
            // Populate the list of camera views
            //
            _bloomViews.FillBloomViewComboBox(bloomViewsList);


            // Populate the list of camera angles
            // ==================================
            _cameraAngles.FillCameraAnglesComboBox(cameraAngleList);



            // Populate the list of camera angles
            // ==================================

       //     comboCameras.Text = myConfig._cameraName;

            ValidateMe();

            formLoading = false;
        }

        #region Handel the camera viwer views
        private void bloomViewsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = bloomViewsList.SelectedIndex;
            if (selected != -1)
            {
                string viewName = bloomViewsList.Items[selected].ToString();
                textBoxViewDescription.Text = _bloomViews.getViewDescription(viewName);
                ValidateMe();
            }
        }


        #endregion  // Handel the camera viwer views

        /// <summary>
        /// Validate form elements
        /// </summary>
        private void ValidateMe()
        {
            buttonYes.Enabled = false;

            if ( cameraAngleList.CheckedItems.Count < 1 ) return;
            if ( bloomViewsList.CheckedItems.Count < 1 ) return;
            if (comboCameras.Text.Length < 3) return;

            buttonYes.Enabled = true;
        }

        private void cameraAngleList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            for (int ix = 0; ix < cameraAngleList.Items.Count; ++ix)
                if (ix != e.Index) cameraAngleList.SetItemChecked(ix, false);
        }

        private void cameraAngleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = cameraAngleList.SelectedIndex;
            if (selected != -1)
            {
                string angleName = cameraAngleList.Items[selected].ToString();
                textBoxCameraAngle.Text = _cameraAngles.getAngleDescription(angleName);
                ValidateMe();
            }
        }

        /// <summary>
        /// Set the configuration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonYes_Click(object sender, EventArgs e)
        {
            _cameraDevice.StartCamera(viewerIndex, comboCameras.Text, cameraForViewer);

            foreach ( string itm in cameraAngleList.CheckedItems)
            {
                _cameraAngles.AllocateBloomView(itm, viewerIndex);
            }


            foreach (string itm in bloomViewsList.CheckedItems)
            {
                _bloomViews.AllocateBloomView(itm, viewerIndex);
            }

            this.Close();
        }

        private void buttonNo_Click(object sender, EventArgs e)
        {
            _cameraDevice.UnallocateCamera(viewerIndex);
            this.Close();
        }


    }  // End configureCamera

}  // End Flower_Space
