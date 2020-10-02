using AForge.Video;
using AForge.Video.DirectShow;
using Flower_Space;
using FlowerControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Flower_Space
{
    public enum Enum_State
    {
        CameraViewNotConfigured,
        CameraViewShowingCameraImage,
        CameraViewStreamingCameraImage,
        CameraViewShowingSavedImage,
        NoSavedImageToShow
    }


    public partial class Form1 : Form
    {
        public CameraDevice cameraDevice;
        public WriteLog writelog = new WriteLog();

        string nl = Environment.NewLine;
        private string[] logText = new string[] { "Invalid" };

        static private DataSet MetaData = new DataSet("MetaData");
        List<PictureBox> cameraPhotoViewers = new List<PictureBox>();

        private Flower_Space.Views bloomViews;
        private Viewers viewers;
        private CameraAngles cameraAngles;
        private ViewPortState viewPortState;
        private bool ApplicationInitialised = false;
        

        // ==================================================================================================================
        // ========================================= Form Loading Unloading Region ==========================================
        // ==================================================================================================================
        #region Form Loading Unloading
        /// <summary>
        /// Form constructor
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            writelog.OpenLog();

            Cursor = Cursors.WaitCursor;

            if ( !Helper.ApplicationDataBaseExists() )
            {
                MessageBox.Show("We have no database, start panicking about now.", "Wow supper bad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                logText = new string[]
                { "Started       ",
                "User name " + Helper.GetUserName(),
                "Computer name " +  System.Environment.MachineName,
                "Executable folder " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Application data folder " + Helper.ApplicationDataPath(),
                "Application version " + Application.ProductVersion,
                " No data base found"
                };
                writelog.LogWriter(logText);
                this.Close();
            }

            /// <summary>
            /// Validate the database and take appropriate action
            /// </summary>
            if (!BloomData.ValidDataBase())
            {
                if (emergencyMoveData())
                {
                    MessageBox.Show("The problem data has been moved to the export folder.\nTry starting the application again", "Refere to the manual", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Attempts to correct the problem have failed.", "Refere to the manual", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                logText = new string[]
                { "Started       ",
                "User name " + Helper.GetUserName(),
                "Computer name " +  System.Environment.MachineName,
                "Executable folder " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Application data folder " + Helper.ApplicationDataPath(),
                "Application version " + Application.ProductVersion,
                "Invalid data base"
               };
                writelog.LogWriter(logText);


                System.Environment.Exit(0);                   // Invalid database we must close
                System.Windows.Forms.Application.Exit();      // Invalid database we must close
                this.Close();                                 // Invalid database we must close
            }

            #region Write to log startup
            logText = new string[]
            { "Started       ",
                "User name " + Helper.GetUserName(),
                "Computer name " +  System.Environment.MachineName,
                "Executable folder " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Application data folder " + Helper.ApplicationDataPath(),
                "Application version " + Application.ProductVersion,
                "Database version " + BloomData.DataBaseVersion()
            };
            writelog.LogWriter(logText); 
            #endregion

            // Validate shape and Inflorescences data
            // These tables are empty when the application is first installed
            bool gotshapes = BloomData.IsThereAnyShapeMetaData();
            if (!(BloomData.IsThereAnyShapeMetaData()) )
            {
                string message = @"There is no Shape metadata, this normally occurs when the application is run " + 
                            "for the first time after installation." + nl + nl +
                            "The appropriate files will be contained in the deployment package, for " +
                            "further information refer to the user guide." + nl + nl + " \nSelect \"OK\" to load " +
                            "the Shape meta data or \"Cancel\" to quit.";
                DialogResult rslt = MessageBox.Show(message, "Shape data and pictures required", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if ( rslt  == DialogResult.Cancel)
                {
                    writelog.LogWriter("No shape data found application closing");
                    Application.Exit();
                    System.Environment.Exit(1);
                }

                ApplyNewShapeMetaData();
                writelog.LogWriter("No shape data found. New shape data applied");
            }


            bool gotinflorescence = BloomData.IsThereAnyInflorescenceMetaData();
            if (!(BloomData.IsThereAnyInflorescenceMetaData()))
            {
                string message = @"There is no Inflorescence metadata, this normally occurs when the application is run " +
                            "for the first time after installation." + nl + nl +
                            "The appropriate files will be contained in the deployment package, for " +
                            "further information refer to the user guide." + nl + nl + " \nSelect \"OK\" to load " +
                            "the Shape meta data or \"Cancel\" to quit.";
                DialogResult rslt = MessageBox.Show(message, "Shape data and pictures required", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (rslt == DialogResult.Cancel)
                {
                    writelog.LogWriter("No shape data found application closing");
                    Application.Exit();
                    System.Environment.Exit(1);
                }

                ApplyNewInflorescenceMetaData();
                writelog.LogWriter("No Inflorescence data found. New shape data applied");
            }

            bloomViews = new Flower_Space.Views();
            viewers = new Viewers();
            cameraAngles = new CameraAngles();
            viewPortState = new ViewPortState();
            ApplicationInitialised = true;


            // Start up the ListView
            LoadAllforSelection();

            // Load the MetaData
            MetaData = BloomData.getMetaData();
            writelog.LogGroupStart("Loading metta data");
            foreach (DataTable tab in MetaData.Tables)
            {
                writelog.LogGroupLine("Table " + tab.TableName + " " + tab.Rows.Count.ToString());
            }

            #region Debug Log enteries
            /*
    writelog.LogGroupStart("Loading Shapes");
    foreach (DataRow row in MetaData.Tables["Shape"].Rows)
    {
        writelog.LogGroupLine(row.Field<string>(2) + ", " + row.Field<string>(0) + ", " + row.Field<string>(1));
    }

    writelog.LogGroupStart("Loading Inflorescence");
    foreach (DataRow row in MetaData.Tables["Inflorescence"].Rows)
    {
        writelog.LogGroupLine(row.Field<string>(2) + ", " + row.Field<string>(0) + ", " + row.Field<string>(1));
    }*/ 
            #endregion

            // Create Camera and CameraConfigurations class's
            cameraDevice = new CameraDevice();
            writelog.LogWriter(cameraDevice.Count.ToString() + " cameras attached");

            // Load the swatch of colours into the colour picker
            ColourPicker.UPOVcolours = BloomData.getUPOVColours();

            // Start up the ListView
            LoadAllforSelection();
            SetCommonNameAutoComplete();
            SetCultivarNameAutoComplete();
            SetGenusNameAutoComplete();
            SetSpeciesNameAutoComplete();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Add picture boxies to the list
            cameraPhotoViewers.Add(cameraPhoto0);
            cameraPhotoViewers.Add(cameraPhoto1);
            cameraPhotoViewers.Add(cameraPhoto2);
            cameraPhotoViewers.Add(cameraPhoto3);
            cameraPhotoViewers.Add(cameraPhoto4);

            Notification.ConfigurationRequired();
            Cursor = Cursors.Default;
            selectedView = Views.normal;
            layoutPannels();
            LoadListViewInflorescences();
            LoadListViewShape();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            writelog.CloseLog();
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cameraDevice.StopAllCameras();     // Or the application will not close
        }

        #endregion  // Form Loading Unloading
        // ====================================== End of Form Loading Unloading Region ======================================


        /// <summary>
        /// Adjust spliter distance when form resizes so camera viewers areSquare
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitContainer1_Panel1_Resize(object sender, EventArgs e)
        {
            //textBox1.Text = splitContainer1.Panel2.Width.ToString()  + ".   Hight shold = " + (splitContainer1.Panel2.Width / 5).ToString()     ;

            splitContainer2.SplitterDistance = panelDataGrid.Panel2.Width / 5;
        }

        // ========================================= Manage database Region ==========================================
        #region Manage database

        private bool emergencyMoveData()
        {
            string fromPath = Helper.ApplicationDataPath();
            string toPath = Helper.ExportPath();
            DirectoryInfo dir = new DirectoryInfo(fromPath);
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                if (file.Length > 0)
                {
                    // you can delete file here if you want (destination file)
                    if (File.Exists(toPath + "\\" + file.Name))
                    {
                        File.Delete(toPath + "\\" + file.Name);
                    }

                    // then copy the file here
                    file.MoveTo(toPath + "\\" + file.Name);
                }
            }
            Helper.CopyNewDataBase();

            bool dbvalid = BloomData.ValidDataBase();
            if (dbvalid) 
            {
                writelog.LogWriter("Emergeny move data completed");
                return true; 
            } 
            else 
            {
                writelog.LogWriter("Emergeny move data failed");
                return false; 
            }
        }

        #endregion  //Manage database
        // ====================================== End of Manage database Region ======================================


        #region Manage the Cmera viewer functions


        // ==========================================  Cycle all cameras  =============================================
        // Starting with the viewer double clicked cycle through all the viewers
        // assigned a camera and capture a new image.
        // Leave the viewer double click streaming video
        #region Cycle all cameras
        private void cameraPhoto0_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CycleAllCameras(0);
        }

        private void cameraPhoto1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CycleAllCameras(1);
        }

        private void cameraPhoto2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CycleAllCameras(2);
        }

        private void cameraPhoto3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CycleAllCameras(3);
        }

        private void cameraPhoto4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            CycleAllCameras(4);
        }

        private void CycleAllCameras(int StartingAt)
        {
            for (int i = StartingAt; i < 5; ++i)
            {
                if (cameraDevice.AllocatedCamera(i))
                {
                    cameraDevice.StartCamera(i);
                    ResetViewerStatus();
                    System.Threading.Thread.Sleep(1000);
                }
            }

            for (int i = 0; i < StartingAt; ++i)
            {
                if (cameraDevice.AllocatedCamera(i))
                {
                    cameraDevice.StartCamera(i);
                    ResetViewerStatus();
                    this.Refresh();
                    System.Threading.Thread.Sleep(1000);
                }

                Notification.AllImagesRefreshed();
            }

            // the double clicked viewer is left streaming
            if (cameraDevice.AllocatedCamera(StartingAt))
            {
                cameraDevice.StartCamera(StartingAt);
                //CameraConfigurations.SetViewerState(StartingAt, Enum_State.CameraViewStreamingCameraImage);
                ResetViewerStatus();
                this.Refresh();
                System.Threading.Thread.Sleep(1000);
            }

        }

        #endregion
        // ==========================================  Cycle all cameras  =============================================



        // SetThisconfiguration
        private DialogResult ConfigurationThisViewer(int ViewerIndex, System.Windows.Forms.PictureBox viewer)
        {
            // Create an instance of the configureCameras form passing the configuration
            configureCamera cf = new configureCamera(cameraDevice, viewer, ViewerIndex, bloomViews, cameraAngles);

            return cf.ShowDialog();
        }


        // ==========================================  Handel viewer single click  =====================================
        // One click on the viewer start streaming video or if not configured start confiuration
        #region Handel one clcik on camera viewer
        /// <summary>
        /// Handel the click then update the status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cameraPhoto0_MouseClick(object sender, MouseEventArgs e)
        {
            if (!cameraDevice.AllocatedCamera(0)) return;
            SingleClickOnViewer(0);
            textBoxViewerConfiguration0.Text = ConfigurationReport(0);
            this.Refresh();
        }

        private void cameraPhoto1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!cameraDevice.AllocatedCamera(1)) return;
            SingleClickOnViewer(1);
            textBoxViewerConfiguration1.Text = ConfigurationReport(1);
            this.Refresh();
        }

        private void cameraPhoto2_MouseClick(object sender, MouseEventArgs e)
        {
            if (!cameraDevice.AllocatedCamera(2)) return;
            SingleClickOnViewer(2);
            textBoxViewerConfiguration2.Text = ConfigurationReport(2);
            this.Refresh();
        }

        private void cameraPhoto3_MouseClick(object sender, MouseEventArgs e)
        {
            if (!cameraDevice.AllocatedCamera(3)) return;
            SingleClickOnViewer(3);
            textBoxViewerConfiguration3.Text = ConfigurationReport(3);
            this.Refresh();
        }

        private void cameraPhoto4_MouseClick(object sender, MouseEventArgs e)
        {
            if (!cameraDevice.AllocatedCamera(4)) return;
            SingleClickOnViewer(4);
            textBoxViewerConfiguration4.Text = ConfigurationReport(4);
            this.Refresh();
        }

        private void SingleClickOnViewer(int ViewerIndex)
        {
            if (cameraDevice.AllocatedCamera(ViewerIndex))
            {
                cameraDevice.StartCamera(ViewerIndex);
                //CameraConfigurations.SetViewerState(ViewerIndex, Enum_State.CameraViewStreamingCameraImage);
                ResetViewerStatus();
            }

        }

        private void ResetViewerStatus()
        {
            if (cameraDevice.AllocatedCamera(0))
            {
                if (cameraDevice.IsCameraRunning(0))
                { viewPortState.SetState(0, Enum_State.CameraViewStreamingCameraImage); }
                else
                { viewPortState.SetState(0, Enum_State.CameraViewShowingCameraImage); }
            }

            if (cameraDevice.AllocatedCamera(1))
            {
                if (cameraDevice.IsCameraRunning(1))
                { viewPortState.SetState(1, Enum_State.CameraViewStreamingCameraImage); }
                else
                { viewPortState.SetState(1, Enum_State.CameraViewShowingCameraImage); }
            }

            if (cameraDevice.AllocatedCamera(2))
            {
                if (cameraDevice.IsCameraRunning(2))
                { viewPortState.SetState(2, Enum_State.CameraViewStreamingCameraImage); }
                else
                { viewPortState.SetState(2, Enum_State.CameraViewShowingCameraImage); }
            }

            if (cameraDevice.AllocatedCamera(3))
            {
                if (cameraDevice.IsCameraRunning(3))
                { viewPortState.SetState(3, Enum_State.CameraViewStreamingCameraImage); }
                else
                { viewPortState.SetState(3, Enum_State.CameraViewShowingCameraImage); }
            }

            if (cameraDevice.AllocatedCamera(4))
            {
                if (cameraDevice.IsCameraRunning(4))
                { viewPortState.SetState(4, Enum_State.CameraViewStreamingCameraImage); }
                else
                { viewPortState.SetState(4, Enum_State.CameraViewShowingCameraImage); }
            }
        }

        #endregion
        // Handel one clcik on camera viewer
        #endregion  
        // Manage camera viewers


        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings setting = new Settings();
            setting.ShowDialog();
        }


        #region Global camera viewer global validations  ( must be one Thumnail and Camera Angle must be used only once)
        private string[] cameraAnglesUsed()
        {
            string[] _cameraAnglesUsed = new string[6];


            //  _cameraAnglesUsed[3] = cameraConfig3._selectedCameraAngle;
            //  _cameraAnglesUsed[4] = cameraConfig4._selectedCameraAngle;
            //  _cameraAnglesUsed[5] = cameraConfig5._selectedCameraAngle;


            return _cameraAnglesUsed;
        }

        #endregion

        // ====================================== Manage the Cmera viewer functions Region ======================================
        #region Manage  cameraViewerIndicators

        /// <summary>
        /// Refresh all the camera viewers
        /// </summary>
        private void RefreshAllcameraViewerIndicators()
        {
            cameraViewerIndicator0.Refresh();
            cameraViewerIndicator1.Refresh();
            cameraViewerIndicator2.Refresh();
            cameraViewerIndicator3.Refresh();
            cameraViewerIndicator4.Refresh();
        }

        private void cameraViewerIndicator0_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                e.Graphics.DrawString(viewPortState.GetStateText(0), this.Font, br, new PointF(109, 7), sf);
            }
            cameraViewerIndicator0.BackColor = viewPortState.GetStateColour(0);
        }

        private void cameraViewerIndicator1_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                e.Graphics.DrawString(viewPortState.GetStateText(1), this.Font, br, new PointF(109, 7), sf);
            }
            cameraViewerIndicator1.BackColor = viewPortState.GetStateColour(1);
        }

        private void cameraViewerIndicator2_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                e.Graphics.DrawString(viewPortState.GetStateText(2), this.Font, br, new PointF(109, 7), sf);
            }
            cameraViewerIndicator2.BackColor = viewPortState.GetStateColour(2);
        }

        private void cameraViewerIndicator3_Paint(object sender, PaintEventArgs e)
        {
            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                e.Graphics.DrawString(viewPortState.GetStateText(3), this.Font, br, new PointF(109, 7), sf);
            }
            cameraViewerIndicator3.BackColor = viewPortState.GetStateColour(3);
        }

        private void cameraViewerIndicator4_Paint(object sender, PaintEventArgs e)
        {
            if (!ApplicationInitialised) return;

            using (SolidBrush br = new SolidBrush(Color.Black))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;

                e.Graphics.DrawString(viewPortState.GetStateText(4), this.Font, br, new PointF(109, 7), sf);
            }
            cameraViewerIndicator4.BackColor = viewPortState.GetStateColour(4);
        }


        private void cameraConfigurationReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Form report = new Report(getCameraConfigurationReport());
           // report.ShowDialog();
        }


        #endregion
        // ====================================== End of Manage the Cmera viewer functions Region ================================

        // ====================================== Manage Data grid view Region ===================================================
        #region Manage Data grid view

        private void LoadAllforSelection()
        {
            if (SearchText.Text.Length > 3)
            {
                LoadForSelection(BloomData.GetSelectListData(SearchText.Text));
            }
            else
            { LoadForSelection(BloomData.GetSelectListData()); }
        }

        private void LoadForSelection(DataTable selectionData)
        {
            dataListView.Clear();   // Throw out all the old rubish first
            dataListView.View = View.Details;

            dataListView.Columns.Add("Photo");
            dataListView.Columns.Add("Name");
            dataListView.Columns.Add("Botanical name");
            dataListView.Columns.Add("Bloom ID");

            dataListView.Columns[1].Width = 150;
            dataListView.Columns[2].Width = 500;
            dataListView.Columns[3].Width = 1;

            // Clear the image lists for the new stuff
            DataListLargeImage.Images.Clear();
            DataListSmallImage.Images.Clear();

            int ii = 0;
            
            foreach (DataRow row in selectionData.Rows)
            {
                string ltp = Helper.ApplicationDataPath() + "\\" + row.Field<string>("ltnname");
                string stp = Helper.ApplicationDataPath() + "\\" + row.Field<string>("stnname");

                try
                {
                    using (FileStream stream = new FileStream(ltp, FileMode.Open, FileAccess.Read))
                    {
                        DataListLargeImage.Images.Add(Image.FromStream(stream));
                        stream.Dispose();
                    }
                    DataListLargeImage.Images.Add(Image.FromFile(ltp));
                }
                catch (Exception)
                {

                    // throw;
                }

                try
                {
                    using (FileStream stream = new FileStream(stp, FileMode.Open, FileAccess.Read))
                    {
                        DataListSmallImage.Images.Add(Image.FromStream(stream));
                        stream.Dispose();
                    }

                    DataListSmallImage.Images.Add(Image.FromFile(stp));

                }
                catch (Exception)
                {

                    // throw;
                }


                string BotName = BotanicalName(row.Field<string>("Genus"), row.Field<string>("Species"), row.Field<string>("Cultivar"));

                dataListView.Items.Add(new ListViewItem(new string[] { row.Field<string>("Name"), row.Field<string>("Name"), BotName, row.Field<string>("bloomid") }, ii));
                ++ii;
            }

            string str = string.Format("Loaded {0} Bloom records from the database", selectionData.Rows.Count);
            writelog.LogWriter(str);
        }

        private void dataListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dataListView.SelectedItems.Count > 0)
            {
                cameraDevice.StopAllCameras();                  // We are most likly going to swop out cmaera images
                var item = dataListView.SelectedItems[0];       // The selected list view item

                // Get the details from selected item
                string s1 = item.Text;
                string s2 = item.SubItems[2].Text;
                string bloomID = item.SubItems[3].Text;          // BloomID

                statusLabel.Text = "Bloom " + s2 + " selected";

                // StopAllViewers();
                unloadDataSet(bloomID);
                statusLabel.Text = "Bloom " + s2 + " selected";
                RefreshAllcameraViewerIndicators();
            }
        }

        /// <summary>
        /// Handel the selected record in the datalist
        ///  Get all related data from the dataset loaded from the database
        /// </summary>
        /// <param name="ds"></param>
        private void unloadDataSet(string BloomID)
        {
            int photoIndex = 0;
            string fileName = "";
            string pictureInformation = "";

            for (int i = 0; i < 5; ++i) { cameraPhotoViewers[i].Image = Properties.Resources.NoPhoto; }

            foreach (DataRow bloomrow in BloomData.GetListViewSelectedItemData(BloomID).Rows)
            {
                if (photoIndex == 0)  // Process bloom data
                {
                    txtCommonName.Text = bloomrow.Field<string>("Name");
                    txtCultivarName.Text = bloomrow.Field<string>("Cultivar");
                    txtSpecies.Text = bloomrow.Field<string>("Species");
                    txtGenus.Text = bloomrow.Field<string>("Genus");

                    textBoxShapeName.Text = bloomrow.Field<string>("Shape");
                    textBoxlInflorescences.Text = bloomrow.Field<string>("Inflorescence");


                    int Bloomsize = bloomrow.Field<int>("Size");
                    if (Bloomsize >= macTrackBarBloomSIze.Minimum & Bloomsize <= macTrackBarBloomSIze.Maximum)
                    { macTrackBarBloomSIze.Value = Bloomsize; }
                    if (Bloomsize < macTrackBarBloomSIze.Minimum) { macTrackBarBloomSIze.Value = macTrackBarBloomSIze.Minimum; }
                    if (Bloomsize > macTrackBarBloomSIze.Maximum) { macTrackBarBloomSIze.Value = macTrackBarBloomSIze.Maximum; }

                    int InflorescenseHight = bloomrow.Field<int>("height");
                    if (InflorescenseHight >= macTrackBarInflorescenceHight.Minimum & InflorescenseHight <= macTrackBarInflorescenceHight.Maximum)
                    { macTrackBarInflorescenceHight.Value = InflorescenseHight; }
                    if (InflorescenseHight < macTrackBarInflorescenceHight.Minimum) { macTrackBarInflorescenceHight.Value = macTrackBarInflorescenceHight.Minimum; }
                    if (InflorescenseHight > macTrackBarInflorescenceHight.Maximum) { macTrackBarInflorescenceHight.Value = macTrackBarInflorescenceHight.Maximum; }

                    int InflorescenseWidth = bloomrow.Field<int>("width");
                    if (InflorescenseWidth >= macTrackBarInflorescenceWidth.Minimum & InflorescenseWidth <= macTrackBarInflorescenceHight.Maximum)
                    { macTrackBarInflorescenceWidth.Value = InflorescenseWidth; }
                    if (InflorescenseWidth < macTrackBarInflorescenceWidth.Minimum) { macTrackBarInflorescenceWidth.Value = macTrackBarInflorescenceWidth.Minimum; }
                    if (InflorescenseWidth > macTrackBarInflorescenceWidth.Maximum) { macTrackBarInflorescenceWidth.Value = macTrackBarInflorescenceWidth.Maximum; }


                    // Build the colour bands and then set in colour picker
                    SortedDictionary<int, colourband> cbs = new SortedDictionary<int, colourband>();
                    foreach (DataRow cr in BloomData.GetListViewSelectedItemColour(BloomID).Rows)
                    {
                        Color colour = Color.FromArgb(cr.Field<Int32>("Red"), cr.Field<Int32>("Green"), cr.Field<Int32>("Blue"));
                        colourband cb = new colourband();
                        cb.index = cr.Field<Int32>("BandIndex");
                        cb.colourindex = cr.Field<Int32>("UPOV_Colour_ID");
                        cb.value = cr.Field<Int32>("BandWidth");
                        cb.colour = colour;
                        cb.colourname = cr.Field<string>("colour");
                        cbs.Add(cb.index, cb);
                    }

                    ColourPicker.SetBands(cbs);
                }

                if (bloomrow.Field<string>("thumbnail") == "Yes")
                { pictureInformation = "This picture used for thumbnail" + nl + nl; }
                else { pictureInformation = ""; }

                pictureInformation = pictureInformation + "Camera angle:" + nl + bloomrow.Field<string>(15) + nl +
                bloomrow.Field<string>(16) + nl + nl + nl + "Views:" + nl;

                // Load the photo into the viewer
                fileName = bloomrow.Field<string>("filename");
                string photoFullName = Path.Combine(Helper.ApplicationDataPath(), fileName);
                cameraPhotoViewers[photoIndex].Image = Image.FromFile(photoFullName);

                // Load the picture information
                foreach (DataRow viewrow in BloomData.GetViewsForPhoto(bloomrow.Field<string>("photoid")).Rows)
                {
                    pictureInformation = pictureInformation + viewrow.Field<string>(1) + nl +
                        viewrow.Field<string>(1) + nl + nl;
                }

//                cameraDevice.SetDiskImageReport(photoIndex, pictureInformation);
                viewers.SetViewerReport(photoIndex, WhatTheViewerIsDoing.ShowingPictureFromDisk, pictureInformation);
//                CameraConfigurations.SetCameraViewerInformation(photoIndex, pictureInformation);
//                CameraConfigurations.SetViewerState(photoIndex, Enum_State.CameraViewShowingSavedImage);
                ++photoIndex;
                RefreshViewConfigurationInformation();
                Notification.ShowingDataFromDatabase();
            }

            this.Refresh();

            /*                ShowTheSavedBloomImages(ds.Tables["Bloom_Photo"], selectedItemData.Tables["BloomPhotoView"]);

                        // Get the data and loaded into the edit fields

                        // Load the names
                        txtCommonName.Text = selectedItemData.Tables["Bloom"].Rows[0].Field<string>("Name");
                        txtCultivarName.Text = selectedItemData.Tables["Bloom"].Rows[0].Field<string>("Cultivar");
                        txtGenus.Text = selectedItemData.Tables["Bloom"].Rows[0].Field<string>("Genus");
                        txtSpecies.Text = selectedItemData.Tables["Bloom"].Rows[0].Field<string>("Species");

                        // Load the sizes
                        int size = selectedItemData.Tables["Bloom"].Rows[0].Field<Int32>("Size");
                        if (size < macTrackBarBloomSIze.Minimum) size = macTrackBarBloomSIze.Minimum;
                        if (size < macTrackBarBloomSIze.Maximum) size = macTrackBarBloomSIze.Maximum;
            //            trackBarSize.Value = size;
            //            txtSize.Text = size.ToString();
            //            textBoxShapeName.Text = ds.Tables["Bloom"].Rows[0].Field<string>("Shape"); 


                        // Build the colour bands and then set in colour picker
                        SortedDictionary<int, colourband> cbs = new SortedDictionary<int, colourband>();
                        foreach (DataRow cr in selectedItemData.Tables["Bloom_Colour"].Rows)
                        {
                            Color colour = Color.FromArgb(cr.Field<Int32>("Red"), cr.Field<Int32>("Green"), cr.Field<Int32>("Blue"));
                            colourband cb = new colourband();
                            cb.index = cr.Field<Int32>("BandIndex");
                            cb.colourindex = cr.Field<Int32>("UPOV_Colour_ID");
                            cb.value = cr.Field<Int32>("BandWidth");
                            cb.colour = colour;
                            cb.colourname = cr.Field<string>("ColourName");
                            cbs.Add(cb.index, cb);
                        }

                        ColourPicker.SetBands(cbs);

                        // Process the photos
                        int PhotoCount = selectedItemData.Tables["Bloom_Photo"].Rows.Count;
                        int ViewCount = selectedItemData.Tables["BloomPhotoView"].Rows.Count;

                        int imagecount = 1;  // used to load images into the viewers

                        //  Load viewer 0
                        if (selectedItemData.Tables["Bloom_Photo"].Rows.Count >= 1)
                        {
                            // We have a photo for this viewer
                            string fileName = selectedItemData.Tables["Bloom_Photo"].Rows[0].Field<string>("filename");
                            string photoFullName = Helper.ApplicationDataPath() + "\\" + fileName;
                            loadViewerWithSavedPhoto(0, photoFullName);
                        }
                        else
                        {  // No photo for this viewer
                            loadViewerWithNoPhoto(0);
                        }

                        //  Load viewer 1
                        if (selectedItemData.Tables["Bloom_Photo"].Rows.Count >= 2)
                        {
                            // We have a photo for this viewer
                            string fileName = selectedItemData.Tables["Bloom_Photo"].Rows[1].Field<string>("filename");
                            string photoFullName = Helper.ApplicationDataPath() + "\\" + fileName;
                            loadViewerWithSavedPhoto(1, photoFullName);
                        }
                        else
                        {  // No photo for this viewer
                            loadViewerWithNoPhoto(1);
                        }

                        //  Load viewer 2
                        if (selectedItemData.Tables["Bloom_Photo"].Rows.Count >= 3)
                        {
                            // We have a photo for this viewer
                            string fileName = selectedItemData.Tables["Bloom_Photo"].Rows[2].Field<string>("filename");
                            string photoFullName = Helper.ApplicationDataPath() + "\\" + fileName;
                            loadViewerWithSavedPhoto(2, photoFullName);
                        }
                        else
                        {  // No photo for this viewer
                            loadViewerWithNoPhoto(2);
                        }

                        //  Load viewer 3
                        if (selectedItemData.Tables["Bloom_Photo"].Rows.Count >= 4)
                        {
                            // We have a photo for this viewer
                            string fileName = selectedItemData.Tables["Bloom_Photo"].Rows[3].Field<string>("filename");
                            string photoFullName = Helper.ApplicationDataPath() + "\\" + fileName;
                            loadViewerWithSavedPhoto(3, photoFullName);
                        }
                        else
                        {  // No photo for this viewer
                            loadViewerWithNoPhoto(3);
                        }

                        //  Load viewer 4
                        if (selectedItemData.Tables["Bloom_Photo"].Rows.Count >= 5)
                        {
                            // We have a photo for this viewer
                            string fileName = ds.Tables["Bloom_Photo"].Rows[4].Field<string>("filename");
                            string photoFullName = Helper.ApplicationDataPath() + "\\" + fileName;
                            loadViewerWithSavedPhoto(4, photoFullName);
                        }
                        else
                        {  // No photo for this viewer
                            loadViewerWithNoPhoto(4);
                        }

                        return;
                        foreach (DataRow pr in selectedItemData.Tables["Bloom_Photo"].Rows)
                        {
                            List<string> myViews = new List<string>();
                            string photoID = pr.Field<string>("photoid");
                            string photoName = Helper.ApplicationDataPath() + "\\" + pr.Field<string>("filename");

                            foreach (DataRow vr in ds.Tables["BloomPhotoView"].Rows)
                            {
                                if (vr.Field<string>("photoid") == photoID)
                                {
                                    string view = BloomData.GetBloomViewName(vr.Field<Int32>("viewid"));
                                    myViews.Add(view);
                                }
                            }

                            switch ( imagecount ) 
                            {
                                case 1:
                                    loadImageFromFile(cameraPhoto0, photoName, CameraConfigurations.GetCameraConfiguration(4));
                                    break;
                                case 2:
                                    loadImageFromFile(cameraPhoto1, photoName, CameraConfigurations.GetCameraConfiguration(4));
                                    break;
                                case 3:
                                    loadImageFromFile(cameraPhoto2, photoName, CameraConfigurations.GetCameraConfiguration(4));
                                    break;
                                case 4:
                                    loadImageFromFile(cameraPhoto3, photoName, CameraConfigurations.GetCameraConfiguration(4));
                                    break;
                                case 5:
                                    loadImageFromFile(cameraPhoto4, photoName, CameraConfigurations.GetCameraConfiguration(4));
                                    break;
                            }

                            ++imagecount;
                        }
                        this.Refresh();
                    }  // unloadDataSet(DataSet ds)


                    private void ShowTheSavedBloomImages(DataTable BloomPhoto, DataTable BloomPhotoView)
                    {
                        int photoCount = BloomPhoto.Rows.Count;

                        foreach ( DataRow row in BloomPhoto.Rows)
                        {

                        }
                    }

                    private void loadViewerWithSavedPhoto(int index, string PhotoFullName)
                    {
                        switch (index)
                        {
                            case 0:
                                cameraPhoto0.Image = Image.FromFile(PhotoFullName);
                                CameraConfigurations.SetViewerState(0, Enum_State.CameraViewShowingSavedImage);
                                CameraConfigurations.SetSavedPhotoID(0, PhotoFullName);
                                SetViewer0Text();
                                break;
                            case 1:
                                cameraPhoto1.Image = Image.FromFile(PhotoFullName);
                                CameraConfigurations.SetViewerState(1, Enum_State.CameraViewShowingSavedImage);
                                CameraConfigurations.SetSavedPhotoID(1, PhotoFullName);
                                SetViewer1Text();
                                break;
                            case 2:
                                cameraPhoto2.Image = Image.FromFile(PhotoFullName);
                                CameraConfigurations.SetViewerState(2, Enum_State.CameraViewShowingSavedImage);
                                CameraConfigurations.SetSavedPhotoID(2, PhotoFullName);
                                SetViewer2Text();
                                break;
                            case 3:
                                cameraPhoto3.Image = Image.FromFile(PhotoFullName);
                                CameraConfigurations.SetViewerState(3, Enum_State.CameraViewShowingSavedImage);
                                CameraConfigurations.SetSavedPhotoID(3, PhotoFullName);
                                SetViewer3Text();
                                break;
                            case 4:
                                cameraPhoto4.Image = Image.FromFile(PhotoFullName);
                                CameraConfigurations.SetViewerState(4, Enum_State.CameraViewShowingSavedImage);
                                CameraConfigurations.SetSavedPhotoID(4, PhotoFullName);
                                SetViewer4Text();
                                break;
                        }
                        //CameraConfigurations.SetViewerState(index, Enum_State.CameraViewShowingSavedImage);
                    }

                    private void loadViewerWithNoPhoto(int index)
                    {
                        switch (index)
                        {
                            case 0:
                                cameraPhoto0.Image = Properties.Resources.NoPhoto;
                                CameraConfigurations.SetViewerState(0, Enum_State.NoSavedImageToShow);
                                break;
                            case 1:
                                cameraPhoto1.Image = Properties.Resources.NoPhoto;
                                CameraConfigurations.SetViewerState(1, Enum_State.NoSavedImageToShow);
                                break;
                            case 2:
                                cameraPhoto2.Image = Properties.Resources.NoPhoto;
                                CameraConfigurations.SetViewerState(2, Enum_State.NoSavedImageToShow);
                                break;
                            case 3:
                                cameraPhoto3.Image = Properties.Resources.NoPhoto;
                                CameraConfigurations.SetViewerState(3, Enum_State.NoSavedImageToShow);
                                break;
                            case 4:
                                cameraPhoto4.Image = Properties.Resources.NoPhoto;
                                CameraConfigurations.SetViewerState(4, Enum_State.NoSavedImageToShow);
                                break;
                        }*/
        }


        #endregion //Manage Data grid view
        // ====================================== End of Manage Data grid view Region ============================================


        #region Manage Bloom Data
        /// <summary>
        /// Create a botanical name structured according to the rules
        /// </summary>
        /// <param name="genus"></param>
        /// <param name="species"></param>
        /// <param name="cultivar"></param>
        /// <returns></returns>
        private string BotanicalName(string genus, string species, string cultivar)
        {
            //If there are three names given, one capitalized, the second lower case, and the third is in single quotations
            //you have respectively the genus, species and cultivar names. 

            // Capitalise genus
            if (String.IsNullOrEmpty(genus)) genus = "Invalid";
            if (String.IsNullOrEmpty(species)) species = "Invalid";
            if (String.IsNullOrEmpty(cultivar)) cultivar = "Invalid";
            genus = genus.First().ToString().ToUpper() + genus.Substring(1);

            return genus + " " + species.ToLower() + " '" + cultivar + "'";
        }



        #endregion  // Manage Bloom Data
        // ====================================== End of Manage Bloom Data Region ===================================================


        // =========================================================================================================
        // ========================================== Manage application ===========================================
        #region Manage application views


        #region Handel View menu eventsMyRegion
        private void normalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedView = Views.normal;
            layoutPannels();
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedView = Views.search;
            layoutPannels();
        }

        private void reviewPhotosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedView = Views.reviewphotos;
            layoutPannels();
        }

        private void enterPhotoDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectedView = Views.enterdata;
            layoutPannels();
        }

        #endregion // Handel View menu eventsMyRegion

        Views selectedView = Views.normal;
        private enum Views { normal, search, reviewphotos, enterdata }
        private void layoutPannels()
        {
            // Adjust spliter pannels to enlarge appropriate area
            switch (selectedView)
            {
                case Views.enterdata:
                    panelDataGrid.SplitterDistance = (int)(toolStripContainer1.Height * 0.15);
                    splitContainer2.SplitterDistance = (int)(toolStripContainer1.ClientSize.Height * 0.25);
                    splitContainerData.SplitterDistance = (int)(tabControlEditData.ClientSize.Width * 0.6);
                    break;
                case Views.normal:
                    panelDataGrid.SplitterDistance = (int)(toolStripContainer1.Height * 0.35);
                    splitContainer2.SplitterDistance = (int)(toolStripContainer1.ClientSize.Height * 0.25);
                    splitContainerData.SplitterDistance = (int)(tabControlEditData.ClientSize.Width * 0.6);
                    break;
                case Views.reviewphotos:
                    panelDataGrid.SplitterDistance = (int)(toolStripContainer1.Height * 0.10);
                    splitContainer2.SplitterDistance = (int)(toolStripContainer1.ClientSize.Height * 0.55);
                    splitContainerData.SplitterDistance = (int)(tabControlEditData.ClientSize.Width * 0.6);
                    break;
                case Views.search:
                    panelDataGrid.SplitterDistance = (int)(toolStripContainer1.Height * 0.65);
                    splitContainer2.SplitterDistance = (int)(toolStripContainer1.ClientSize.Height * 0.25);
                    splitContainerData.SplitterDistance = (int)(tabControlEditData.ClientSize.Width * 0.6);
                    break;
            }
        }



        #endregion  // Manage application views
        // =============================== End of Manage application views Region ==================================

        // =========================================================================================================
        // ============================================ Save Bloom Data ============================================
        #region Save Bloom Data

        /// <summary>
        /// Save current data.  We cant get here unless the data is valid
        /// We need to save
        ///     A small thumbnail
        ///     A large thumbnail
        ///     A colour thumbnail
        ///     All the images in the viewers
        ///     Tables
        ///         Bloom
        ///         Bloom_Photo
        ///         BloomPhotoView
        ///         Bloom_Colour
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            buttonSave.Enabled = false;

            DataSet saveDataDataSet = BloomData.GetEmptyBloomDataSet();
            string Bloom_ID = Guid.NewGuid().ToString();

            createBloomRecord(saveDataDataSet, Bloom_ID);                              // one record only
            createBloomColourViewRecord(saveDataDataSet, Bloom_ID);
            createBloomPhotoRecord(saveDataDataSet, Bloom_ID);

            writelog.LogGroupStart("Saving picture");
            
            BloomData.WriteBloomData(saveDataDataSet);

            writelog.LogGroupLine("Common name " + txtCommonName.Text);
            writelog.LogGroupLine("Cultivar " + txtCultivarName.Text);
            writelog.LogGroupLine("Species " + txtSpecies.Text);
            writelog.LogGroupLine("Genus " + txtGenus.Text);
            writelog.LogGroupLine("Shape  " + textBoxShapeName.Text);
            writelog.LogGroupLine("Inflorescence " + textBoxlInflorescences.Text);

            colourband cb;
            int colourcount = ColourPicker.BandCount;
            for (int i = 0; i < colourcount; ++i)
            {
                cb = ColourPicker.BandData(i);
                writelog.LogGroupLine("Colour   index " + cb.colourindex.ToString() + "\t" + cb.colourname + "\t" + cb.value.ToString("00%") + "\t RGB " + cb.colour.R + ", " + cb.colour.G + ", " + cb.colour.B);
            }


            LoadAllforSelection();                         // Refresh the search list view
            buttonSave.Enabled = false;
//            removeMettaData();
            statusLabel.Text = "Photos and data saved";
        }

        private void createBloomRecord(DataSet ds, string Bloom_ID)
        {
            string LargeColourThumbnail = Guid.NewGuid().ToString() + Properties.Settings.Default.LargeColourThumbnailPostscript + ".bmp";
            string SmallColourThumbnail = Guid.NewGuid().ToString() + Properties.Settings.Default.SmallColourThumbnailPostscript + ".bmp";
        

            ds.Tables["Bloom"].Rows.Add(
                Bloom_ID,
                Guid.NewGuid().ToString() + Properties.Settings.Default.LargeThumbnailPostscript + ".bmp",
                Guid.NewGuid().ToString() + Properties.Settings.Default.SmallThumbnailPostscript + ".bmp",
                LargeColourThumbnail,
                SmallColourThumbnail,
                txtCommonName.Text,
                txtCultivarName.Text,
                txtGenus.Text,
                txtSpecies.Text,
                textBoxShapeName.Text,
                int.Parse(textBoxBloomsize.Text),
                textBoxlInflorescences.Text,
                int.Parse(textBoxInflorescenseWidth.Text),
                int.Parse(textBoxInflorescenseHight.Text),
                System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

            // Save colour thumbnails
            Bitmap lctn = ColourPicker.GetThumbnail(200);
            Bitmap sctn = ColourPicker.GetThumbnail(62);

            lctn.Save(Path.Combine(Helper.ApplicationDataPath(), LargeColourThumbnail), ImageFormat.Bmp);
            sctn.Save(Path.Combine(Helper.ApplicationDataPath(), SmallColourThumbnail), ImageFormat.Bmp);

        }

        private void createBloomPhotoRecord(DataSet ds, string BloomID)
        {
            
            int row = 0;
            for (int i = 0; i < 5; ++i)
            {
                if ( cameraDevice.AllocatedCamera(i) )
                { 
                    string PhotoID = Guid.NewGuid().ToString();
                    string IsThumbnail = (i == ThumnailIndex) ? "Yes" : "No";
                    string CameraAngle = cameraAngles.GetAllocatedBloomView(i);
                    string fileName = Guid.NewGuid().ToString() + "-" + i.ToString("0#") + ".png";

                    ds.Tables["Bloom_Photo"].Rows.Add(PhotoID, BloomID,  fileName, CameraAngle, IsThumbnail );

                    cameraPhotoViewers[i].Image.Save(Path.Combine(Helper.ApplicationDataPath(), fileName), ImageFormat.Png);

                    // Save large and small thumbnail
                    if (IsThumbnail == "Yes") 
                    {
                        string LThumbPath = Path.Combine(Helper.ApplicationDataPath(), ds.Tables["Bloom"].Rows[0][1].ToString());
                        string SThumbPath = Path.Combine(Helper.ApplicationDataPath(), ds.Tables["Bloom"].Rows[0][2].ToString());

                        Image imgl = cameraPhotoViewers[i].Image.GetThumbnailImage(200, 200, null, IntPtr.Zero);
                        imgl.Save(LThumbPath, ImageFormat.Bmp);

                        Image imgs = cameraPhotoViewers[i].Image.GetThumbnailImage(64,64, null, IntPtr.Zero);
                        imgs.Save(SThumbPath, ImageFormat.Bmp);
                    }

                    createBloomPhotoViewRecord(ds, i,  PhotoID);

                }
                ++row;
            }
        }

        private void createBloomPhotoViewRecord(DataSet ds, int ViewerIndex,  string PhotoID)
        {
            List<string> views = bloomViews.GetAllocatedBloomView(ViewerIndex);

            foreach ( string view in views)
            {
                ds.Tables["Photo_View"].Rows.Add(
                      PhotoID,      // PhotoID
                      view
                      );
            }
        }

        private void createBloomColourViewRecord(DataSet ds, string Bloom_ID)
        {
            colourband cb;
            int Count = ColourPicker.BandCount;

            for (int i = 1; i <= Count; i++)
            {
                cb = ColourPicker.BandData(i);
                ds.Tables["Bloom_Colour"].Rows.Add(Bloom_ID, cb.index, cb.colourindex, cb.value, cb.colourname, cb.colour.R, cb.colour.G, cb.colour.B);
            }
        }

        /// <summary>
        /// When the save tab is entered validate the data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabPageDataSave_Enter(object sender, EventArgs e)
        {
            ValidateData();
        }

        /// <summary>
        /// Allow user to refresh the validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtValidateData_Click(object sender, EventArgs e)
        {
            ValidateData();
        }

        private void ValidateData()
        {
            buttonSave.Enabled = false;
            bool validated = true;

            Image thumbImage = Flower_Space.Properties.Resources.NoPhoto;
            Bitmap thumbnailColour = Flower_Space.Properties.Resources.NoPhoto;

            int imagesAvailable = cameraDevice.CountAllocatedCameras();


            if (ThumnailIndex >= 0)
            { 
                thumbImage = cameraPhotoViewers[ThumnailIndex].Image;
                thumbImage = Helper.CropToMaxSquare((Bitmap)thumbImage);
                Image thumbnail = thumbImage.GetThumbnailImage(64, 64, null, IntPtr.Zero);
                picThumbnail.Image = thumbnail;
                txtValidThumbnail.Text = "Thumbnail assigned";
                txtValidThumbnail.BackColor = Color.LightGoldenrodYellow;
            }
            else 
            {
                txtValidThumbnail.Text = "Thumbnail NOT assigned";
                txtValidThumbnail.BackColor = Color.DeepPink;
                thumbImage = Flower_Space.Properties.Resources.NoPhoto;
                validated = false;
            }

            // Test cameras
            if (imagesAvailable > 0)
            { 
                txtValidCameraAssignment.Text = imagesAvailable.ToString() + " cameras assigned";
                txtValidCameraAssignment.BackColor = Color.LightGoldenrodYellow;
            }
            else 
            {
                txtValidCameraAssignment.Text = "Invalid camera assignment";
                txtValidCameraAssignment.BackColor = Color.DeepPink;
                validated = false; 
            }

            if (ColourPicker.Valid)
            { 
                txtVaildColour.Text = "Colour selection is valid";
                txtVaildColour.BackColor = Color.LightGoldenrodYellow;
                thumbnailColour = ColourPicker.GetThumbnail(64);
                picColour.Image = thumbnailColour;
            }
            else 
            { 
                txtVaildColour.Text = "Colour selection is not valid";
                txtVaildColour.BackColor = Color.DeepPink;
                validated = false; 
            }

            if (txtCommonName.Text.Length > 4)
            { 
                txtValidName.Text = "Name is valid";
                txtValidName.BackColor = Color.LightGoldenrodYellow;
            }
            else 
            { 
                txtValidName.Text = "Name is not valid";
                txtValidName.BackColor = Color.DeepPink;
                validated = false; 
            }

            if (textBoxShapeName.Text.Length > 2)
            {
                txtValidateShape.Text = "Shape is valid";
                txtValidateShape.BackColor = Color.LightGoldenrodYellow;
            }
            else
            {
                txtValidateShape.Text = "Shape is not valid";
                txtValidateShape.BackColor = Color.DeepPink;
                validated = false;
            }

            if ( textBoxBloomsize.Text.Length > 0)
            { 
                txtValidBloomSize.Text = "Bloom size of " + textBoxBloomsize.Text + " is valid";
                txtValidBloomSize.BackColor = Color.LightGoldenrodYellow;
            }
            else { 
                txtValidBloomSize.Text = "Bloom size is not valid"; validated = false;
                txtValidBloomSize.BackColor = Color.DeepPink;
            }

            if ( textBoxlInflorescences.Text.Length > 2)
            {
                txtValidateInflorescence.Text = "Inflorescence is valid";
                txtValidateInflorescence.BackColor = Color.LightGoldenrodYellow;
            }
            else
            {
                txtValidateInflorescence.Text = "Inflorescence is not valid";
                txtValidateInflorescence.BackColor = Color.DeepPink;
                validated = false;
            }

            if ( textBoxInflorescenseWidth.Text.Length > 0 & textBoxInflorescenseHight.Text.Length > 0)
            { 
                txtValidateInflorescenceSize.Text = "Inflorescence size of " + labelInflorescenseWidthHight.Text + " is valid";
                txtValidateInflorescenceSize.BackColor = Color.LightGoldenrodYellow;
            }
            else 
            { 
                txtValidateInflorescenceSize.Text = "Inflorescence size is not valid"; validated = false;
                txtValidateInflorescenceSize.BackColor = Color.DeepPink;
            }



            // This is the end of validation
            if (validated == true) { buttonSave.Enabled = true; }

        }



        #endregion  // Save Bloom Data
        // ====================================== End of Save Bloom Data Region =====================================

        // =========================================================================================================
        // ========================================== Manage data editing ==========================================
        #region Manage data editing

        private void LoadListViewInflorescences()
        {
            string MetaDataFolder = Helper.ApplicationMetaDataFolder();

            writelog.LogGroupStart("LoadListViewInflorescences"); 

            // Clear all existing contents
            listViewInflorescences.Clear();

            //LV PROPERTIES
            listViewInflorescences.View = View.LargeIcon;
            // Allow the user to edit item text.
            listViewInflorescences.LabelEdit = false;
            // Allow the user to rearrange columns.
            listViewInflorescences.AllowColumnReorder = true;
            // Display check boxes.
            listViewInflorescences.CheckBoxes = false;
            // Select the item and subitems when selection is made.
            listViewInflorescences.FullRowSelect = true;
            // Display grid lines.
            listViewInflorescences.GridLines = true;
            // Sort the items in the list in ascending order.
            listViewInflorescences.Sorting = SortOrder.Ascending;
            // Only one item can be selected
            listViewInflorescences.MultiSelect = false;

            //CONSTRUCT COLUMNS
            listViewInflorescences.Columns.Add("Name", 200);
            listViewInflorescences.Columns.Add("Name1", 200);
            listViewInflorescences.Columns.Add("Description", -2, HorizontalAlignment.Left);
            listViewInflorescences.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            //listViewInflorescences.Columns[1].Width = 2000;

            writelog.LogGroupLine("Items to load " + MetaData.Tables["Inflorescence"].Rows.Count.ToString());
            int i = 0;
            foreach (DataRow dataRow in MetaData.Tables["Inflorescence"].Rows)
            {
                // Add image to image list
                string imagePath = Path.Combine(MetaDataFolder, dataRow.Field<string>(2));
                writelog.LogGroupLine("Loading image " + (i + 1).ToString() + ". " + dataRow.Field<string>(2));
                try
                {
                    imageListInflorescence.Images.Add(Image.FromFile(Path.Combine(MetaDataFolder, dataRow.Field<string>(2))));

                }
                catch (Exception ex)
                {
                    string msg = "The image \"" + ex.Message + "\" associated with the inflorescence metadata could not be loaded." + nl + nl +
                            "You may be able to load this data from a previously created meta data document. " +
                            "If you have a previously created meta data document, you can apply it by " + 
                            "selecting \"OK\"." + nl + nl + "Otherwise select \"Cancel\" to quit.";
                    DialogResult rslt = MessageBox.Show(msg, "Missing Inflorescence images", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if ( rslt == DialogResult.OK) 
                    {
                        writelog.LogWriter("Failed to find " + imagePath + " user opted to apply Inflorescence meta data");
                        ApplyNewInflorescenceMetaData(); 
                    }
                    else 
                    {
                        writelog.LogWriter("Failed to find " + imagePath + " user opted to apply Inflorescence meta data"); 
                        System.Environment.Exit(0); 
                    }
                }
                // Get name and description
                string name = dataRow.Field<string>(0);
                string description = dataRow.Field<string>(1);

                // Add row to list    i is index to list item
                //listViewInflorescences.Items.Add(name, description, i);
                ListViewItem lvi = new ListViewItem(name, i);
                lvi.SubItems.Add(description);
                listViewInflorescences.Items.Add(lvi);

                ++i;
            }

        }

        private void LoadListViewShape()
        {

            writelog.LogGroupStart("LoadListViewShape");

            string MetaDataFolder = Helper.ApplicationMetaDataFolder();

            TableLayoutPanel ShapesTable = new TableLayoutPanel();

            DataSet dataSetShape = BloomData.CreateShapeMetaDataDataSet(true);

           
            // Clear all existing contents
            listViewShapes.Clear();

            //LV PROPERTIES
            listViewShapes.View = View.LargeIcon;
            // Allow the user to edit item text.
            listViewShapes.LabelEdit = false;
            // Allow the user to rearrange columns.
            listViewShapes.AllowColumnReorder = true;
            // Display check boxes.
            listViewShapes.CheckBoxes = false;
            // Select the item and subitems when selection is made.
            listViewShapes.FullRowSelect = true;
            // Display grid lines.
            listViewShapes.GridLines = true;
            // Sort the items in the list in ascending order.
            listViewShapes.Sorting = SortOrder.Ascending;
            // Only one item can be selected
            listViewShapes.MultiSelect = false;

            //CONSTRUCT COLUMNS
            listViewShapes.Columns.Add("Name", 200);
            listViewShapes.Columns.Add("Description", - 2, HorizontalAlignment.Left);
            listViewShapes.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            //listViewShapes.Columns[1].Width = 2000;

            writelog.LogGroupLine("Items to load " + MetaData.Tables["Shape"].Rows.Count.ToString());
            int i = 0;
            foreach ( DataRow dataRow in dataSetShape.Tables["Shape"].Rows)
            {
                writelog.LogGroupLine("Loading image " + (i + 1).ToString() + ". " + dataRow.Field<string>(2));
                try
                {
                    imageListShape.Images.Add(Image.FromFile(Path.Combine(MetaDataFolder, dataRow.Field<string>(2))));
                }
                catch (Exception ex)
                {
                    string msg = "The image \"" + ex.Message + "\" associated with the shape metadata could not be loaded." + nl + nl +
                            "You may be able to load this data from a previously created meta data document. " +
                            "If you have a previously created meta data document, you can apply it by " +
                            "selecting \"OK\"." + nl + nl + "Otherwise select \"Cancel\" to quit.";
                    DialogResult rslt = MessageBox.Show(msg, "Missing Shape image", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (rslt == DialogResult.OK) 
                    {
                        writelog.LogWriter("Failed to find " + Path.Combine(MetaDataFolder, dataRow.Field<string>(2)) + " user opted to apply Shape meta data");
                        ApplyNewShapeMetaData(); 
                    }
                    else 
                    {
                        writelog.LogWriter("Failed to find " + Path.Combine(MetaDataFolder, dataRow.Field<string>(2)) + " user opted to exit");
                        System.Environment.Exit(0); 
                    }
                }
                string name = dataRow.Field<string>(0);
                string description = dataRow.Field<string>(1);

                listViewShapes.Items.Add(name, description, i);
                ++i;
            }

        }

        #endregion  // Manage data editing
        // ====================================== End of Manage data editing =======================================

        // =========================================================================================================
        // =========================================== Manage Meta data ============================================
        #region Manage Meta data
 
        /// <summary>
        /// Record the selected shape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewShapes_Click(object sender, EventArgs e)
        {
            textBoxShapeName.Text = listViewShapes.SelectedItems[0].Name;
        }

        // ==================================================================================================
        // ===============================  Create meta data packages =======================================
        // ==================================================================================================
        #region Creata Meta Data Document
        private void toolStripMenuItemCamera_AngleCreataMetaDataDocument_Click(object sender, EventArgs e)
        {
            string SelectedFolder = GetLastMetaDataFolder(true,"Camera angle");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " Camera Angle");

            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; 
            }

            DataSet Camera_Angle = BloomData.CreateCameraAngleMetaDataDataSet(true);
            Camera_Angle.WriteXml(Path.Combine(SelectedFolder, "CameraAngle.XML"), XmlWriteMode.WriteSchema);

            string fromPath = Helper.ApplicationMetaDataFolder();

            //  Now copy the images that relate to the data
            foreach (DataRow row in Camera_Angle.Tables["Camera_Angle"].Rows)
            {
                try
                {
                    string fromImagePath = Path.Combine(fromPath, row.Field<string>("ImageName"));
                    string toImagePath = Path.Combine(SelectedFolder, row.Field<string>("ImageName"));

                    File.Copy(fromImagePath, toImagePath, true);
                }
                catch (IOException iox)
                {
                }
            }
            Notification.MetaDataFileCreated(SelectedFolder);
        }

        private void toolStripMenuItemBloomView_Click(object sender, EventArgs e)
        {
            string SelectedFolder = GetLastMetaDataFolder(true, "bloomview");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " Views ");

            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataSet bloomview = BloomData.CreateEditbloomviewMetaDataDataSet();
            bloomview.WriteXml(Path.Combine(SelectedFolder, "Views.XML"), XmlWriteMode.WriteSchema);
            Notification.MetaDataFileCreated(SelectedFolder);

        }

        private void toolStripMenuItemColourCreataMetaDataDocument_Click(object sender, EventArgs e)
        {
            string SelectedFolder = GetLastMetaDataFolder(true, "Colour");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " Colour ");
            //SELECT * FROM Colour  Order by id;
            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataSet Colour = BloomData.CreateEditColourMetaDataDataSet();
            Colour.WriteXml(Path.Combine(SelectedFolder, "Colour.XML"), XmlWriteMode.WriteSchema);
            Notification.MetaDataFileCreated(SelectedFolder);
        }

        private void toolStripMenuItemCommon_NameCreataMetaDataDocument_Click(object sender, EventArgs e)
        {
            string SelectedFolder = GetLastMetaDataFolder(true, "Common Namer");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " Common Name");

            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataSet Common_Name = BloomData.CreateEditCommon_NameMetaDataDataSet();
            Common_Name.WriteXml(Path.Combine(SelectedFolder, "Common Name.XML"), XmlWriteMode.WriteSchema);
            Notification.MetaDataFileCreated(SelectedFolder);
        }

        private void toolStripMenuItemCultivarCreataMetaDataDocument_Click(object sender, EventArgs e)
        {
            string SelectedFolder = GetLastMetaDataFolder(true, "Cultivar");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " Cultivar");

            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataSet Cultivar = BloomData.CreateEditCultivarMetaDataDataSet();
            Cultivar.WriteXml(Path.Combine(SelectedFolder, "Cultivar.XML"), XmlWriteMode.WriteSchema);
            Notification.MetaDataFileCreated(SelectedFolder);
        }

        private void toolStripMenuItemGenusCreataMetaDataDocument_Click(object sender, EventArgs e)
        {
            string SelectedFolder = GetLastMetaDataFolder(true, "Genus");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " Genus");

            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataSet Genus = BloomData.CreateEditGenusMetaDataDataSet();
            Genus.WriteXml(Path.Combine(SelectedFolder, "Genus.XML"), XmlWriteMode.WriteSchema);
            Notification.MetaDataFileCreated(SelectedFolder);
        }

        private void toolStripMenuItemShapeCreataMetaDataDocument_Click(object sender, EventArgs e)
        {
            string SelectedFolder = GetLastMetaDataFolder(true, "Shape");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " Shape");

            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataSet Shape = BloomData.CreateShapeMetaDataDataSet(true);
            Shape.WriteXml(Path.Combine(SelectedFolder, "Shape.XML"), XmlWriteMode.WriteSchema);

            string fromPath = Helper.ApplicationMetaDataFolder();

            //  Now copy the images that relate to the data
            foreach (DataRow row in Shape.Tables["Shape"].Rows)
            {
                try
                {
                    string fromImagePath = Path.Combine(fromPath, row.Field<string>("Image"));
                    string toImagePath = Path.Combine(SelectedFolder, row.Field<string>("Image"));

                    File.Copy(fromImagePath, toImagePath, true);
                }
                catch (IOException iox)
                {
//                    Console.WriteLine(iox.Message);
                }
            }
            Notification.MetaDataFileCreated(SelectedFolder);
        }

        private void toolStripMenuItemInflorescenceCreataMetaDataDocument_Click(object sender, EventArgs e)
        {
            string SelectedFolder = GetLastMetaDataFolder(true, "Inflorescence");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") +" Inflorescence");

            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataSet Inflorescence = BloomData.CreateInflorescenceMetaDataDataSet(true);
            Inflorescence.WriteXml(Path.Combine(SelectedFolder, "Inflorescence.XML"), XmlWriteMode.WriteSchema);

            string fromPath = Helper.ApplicationMetaDataFolder();

            //  Now copy the images that relate to the data
            foreach (DataRow row in Inflorescence.Tables["Inflorescence"].Rows)
            {
                try
                {
                    string fromImagePath = Path.Combine(fromPath, row.Field<string>("Image"));
                    string toImagePath = Path.Combine(SelectedFolder, row.Field<string>("Image"));

                    try
                    {
                        File.Copy(fromImagePath, toImagePath, true);
                    }
                    catch (DirectoryNotFoundException dnfe)
                    {
                        MessageBox.Show(dnfe.Message, "Refer this to IT", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Refer this to IT", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                        this.Close();
                    }
                }
                catch (IOException iox)
                {
                    //                    Console.WriteLine(iox.Message);
                }
            }
            Notification.MetaDataFileCreated(SelectedFolder);
        }

        private void toolStripMenuItemSpeciesCreataMetaDataDocument_Click(object sender, EventArgs e)
        { 
            string SelectedFolder = GetLastMetaDataFolder(true, "Species");
            SelectedFolder = Path.Combine(SelectedFolder, DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " Species");

            if (!Helper.PathExistsCreate(SelectedFolder))
            {
                MessageBox.Show("Error creating folder " + SelectedFolder, "Create Meta Data faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataSet Species = BloomData.CreateEditSpeciesMetaDataDataSet();
            Species.WriteXml(Path.Combine(SelectedFolder, "Species.XML"), XmlWriteMode.WriteSchema);
            Notification.MetaDataFileCreated(SelectedFolder);
        }

        #endregion  // Creata Meta Data Document
        // ===============================  Create meta data packages =======================================

        // ==================================================================================================
        // ================================  Apply meta data packages =======================================
        // ==================================================================================================
        #region Apply New Meta Data

        private void toolStripMenuItemCamera_AngleApplyNewMetaData_Click(object sender, EventArgs e)
        {
            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Camera angle");

            DataSet CameraAngle = new DataSet();
            CameraAngle.ReadXml(Path.Combine(fromPath, "CameraAngle.XML"));

            DataTable dt = CameraAngle.Tables["Camera_Angle"];
            
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    string fromImagePath = Path.Combine(fromPath, row.Field<string>("ImageName"));
                    string toImagePath = Path.Combine(toPath, row.Field<string>("ImageName"));
//                    Console.WriteLine("Copy image From {0}  To {1}", fromImagePath, toImagePath);

                    //File.Copy(fromImagePath, toImagePath, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Operation Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (BloomData.InsertCamera_AngleMetaData(dt))
            { Notification.MetaDataFileApplied("Bloom View"); }
            else
            { Notification.MetaDataFileNotApplied("Bloom View"); }
        }
        // toolStripMenuItemCamera_AngleApplyNewMetaData_Click

        private void toolStripMenuItemBloomViewApplyNewMetaData_Click(object sender, EventArgs e)
        {
            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Bloom View");

            DataSet views = new DataSet();
            try
            {
                views.ReadXml(Path.Combine(fromPath, "Views.XML"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Operation Aborted",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            if ( BloomData.InsertbloomviewMetaData(views.Tables["Views"]))
            { Notification.MetaDataFileApplied("Views"); }
            else
            { Notification.MetaDataFileNotApplied("Views"); }
        } 
        // toolStripMenuItemBloomViewApplyNewMetaData_Click

        private void toolStripMenuItemColourApplyNewMetaData_Click(object sender, EventArgs e)
        {
            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Colour");

            DataSet Colour = new DataSet();
            try
            {
                Colour.ReadXml(Path.Combine(fromPath, "Colour.XML"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operation Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (BloomData.InsertColourMetaData(Colour.Tables["Colour"]))
            { Notification.MetaDataFileApplied("Colour"); }
            else
            { Notification.MetaDataFileNotApplied("Colour"); }
        }
        // toolStripMenuItemColourApplyNewMetaData_Click(

        private void toolStripMenuItemCommon_NameApplyNewMetaData_Click(object sender, EventArgs e)
        {
            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Common Name");

            DataSet Common_NameData = new DataSet();
            try
            {
                Common_NameData.ReadXml(Path.Combine(fromPath, "Common Name.XML"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operation Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (BloomData.InsertCommon_NameMetaData(Common_NameData.Tables["Common_Name"]))
            { Notification.MetaDataFileApplied("Common Name"); }
            else 
            { Notification.MetaDataFileNotApplied("Common Name"); }
        }
        // toolStripMenuItemCommon_NameApplyNewMetaData_Click

        private void toolStripMenuItemCultivarApplyNewMetaData_Click(object sender, EventArgs e)
        {
            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Cultivar");

            DataSet CultivarData = new DataSet();
            try
            {
                CultivarData.ReadXml(Path.Combine(fromPath, "Cultivar.XML"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operation Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (BloomData.InsertGenusMetaData(CultivarData.Tables["Cultivar"]))
            { Notification.MetaDataFileApplied("Cultivar"); }
            else
            { Notification.MetaDataFileNotApplied("Cultivar"); }
        }
        // toolStripMenuItemCultivarApplyNewMetaData_Click

        private void toolStripMenuItemGenusApplyNewMetaData_Click(object sender, EventArgs e)
        {
            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Genus");

            DataSet GenusData = new DataSet();
            try
            {
                GenusData.ReadXml(Path.Combine(fromPath, "Genus.XML"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operation Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (BloomData.InsertGenusMetaData(GenusData.Tables["Genus"]))
            { Notification.MetaDataFileApplied("Genus"); }
            else
            { Notification.MetaDataFileNotApplied("Genus"); }
        }
        // toolStripMenuItemGenusApplyNewMetaData_Click

        private void toolStripMenuItemShapeApplyNewMetaData_Click(object sender, EventArgs e)
        {
            ApplyNewShapeMetaData();
        }
        // toolStripMenuItemShapeApplyNewMetaData_Click

        private void ApplyNewShapeMetaData()
        {
            writelog.LogGroupStart("Applying new shape meta data");

            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Shape");

            writelog.LogGroupLine("Getting data from " + fromPath);


            DataSet Shape = new DataSet();
            Shape.ReadXml(Path.Combine(fromPath, "Shape.XML"));

            DataTable dt = Shape.Tables["Shape"];
            BloomData.InsertShapeMetaData(dt);

            writelog.LogGroupLine("Found  " + dt.Rows.Count.ToString() + " shapes");

            // Copy the pictures
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    string fromImagePath = Path.Combine(fromPath, row.Field<string>("Image"));
                    string toImagePath = Path.Combine(toPath, row.Field<string>("Image"));

                    writelog.LogGroupLine("copying from  " + fromImagePath + " To " + toImagePath);
                    try
                    {
                        File.Copy(fromImagePath, toImagePath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error message:\n" + ex.Message, "Failed to copy image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Notification.MetaDataFileNotApplied("Shape");
                        return;
                    }
                }
                catch (IOException iox)
                {
                    MessageBox.Show("Error message:\n" + iox.Message, "Failed to copy image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Notification.MetaDataFileNotApplied("Shape");
                    return;
                }
            }

            if (BloomData.InsertShapeMetaData(dt))
            { Notification.MetaDataFileApplied("Shape"); }
            else
            { Notification.MetaDataFileNotApplied("Shape"); }

        }

        private void toolStripMenuItemInflorescenceApplyNewMetaDatax_Click(object sender, EventArgs e)
        {
            ApplyNewInflorescenceMetaData();
        }

        private void ApplyNewInflorescenceMetaData()
        {
            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Inflorescence");

            DataSet Inflorescence = new DataSet();
            Inflorescence.ReadXml(Path.Combine(fromPath, "Inflorescence.XML"));

            DataTable dt = Inflorescence.Tables["Inflorescence"];
            BloomData.InsertInflorescenceMetaData(dt);

            // Copy the pictures
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    string fromImagePath = Path.Combine(fromPath, row.Field<string>("Image"));
                    string toImagePath = Path.Combine(toPath, row.Field<string>("Image"));

                    try
                    {
                        File.Copy(fromImagePath, toImagePath, true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error message:\n" + ex.Message, "Failed to copy image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Notification.MetaDataFileNotApplied("Inflorescence");
                        return;
                    }
                }
                catch (IOException iox)
                {
                    MessageBox.Show("Error message:\n" + iox.Message, "Failed to copy image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Notification.MetaDataFileNotApplied("Inflorescence");
                    return;
                }
            }

            if (BloomData.InsertInflorescenceMetaData(dt))
            { Notification.MetaDataFileApplied("Inflorescence"); }
            else
            { Notification.MetaDataFileNotApplied("Inflorescence"); }

        }

        private void toolStripMenuItemSpeciesApplyNewMetaData_Click(object sender, EventArgs e)
        {
            string toPath = Helper.ApplicationMetaDataFolder();
            string fromPath = GetLastMetaDataFolder(false, "Species");

            DataSet SpeciesData = new DataSet();
            try
            {
                SpeciesData.ReadXml(Path.Combine(fromPath, "Species.XML"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operation Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (BloomData.InsertSpeciesMetaData(SpeciesData.Tables["Species"]))
            { Notification.MetaDataFileApplied("Species"); }
            else
            { Notification.MetaDataFileNotApplied("Speciese"); }
        }
        // toolStripMenuItemSpeciesApplyNewMetaData_Click

        #endregion  
        // ================================  Apply meta data packages =======================================

        /// <summary>
        /// Meta data is writen to XML files to allow user editing
        /// It is the writen back to the database tables
        /// When creating a metta data item the user selects/creates a meta data folder ..
        ///       The program creates a specific folder named for the meta data type and date created
        /// When writing the data bake the user select a specific folder for the meta data type
        /// Some meta data has associated pictures
        /// </summary>
        /// <param name="CreatingMetaData"></param>
        /// <param name="metaDataName"></param>
        /// <returns></returns>
        private string GetLastMetaDataFolder(bool CreatingMetaData, string metaDataName)
        {
            var dialog = new FolderBrowserDialog();

            if (CreatingMetaData)     // we are after the general folder for meta data
            {
                dialog.Description = "Select the folder for the Flower Space Meta Data editing files.\n\n" +
                          "The files will be saved in a folder called " + metaDataName + DateTime.Now.ToString("yyyy-MM-dd hh mm tt") + " which be created for them.";
                dialog.SelectedPath = Properties.Settings.Default.RecentMetaDataFolder;
            }
            else         // We are after the specific folder
            {
                dialog.Description = "Select the folder containing the Meta Data editing files.\n\n";
                switch (metaDataName)
                {
                    case "Bloom View":
                        dialog.SelectedPath = Properties.Settings.Default.BloomViewLastMetaDataFolder;
                        break;
                    case "Camera Angle":
                        dialog.SelectedPath = Properties.Settings.Default.CameraAngleLastMetaDataFolder;
                        break;
                    case "Colour":
                        dialog.SelectedPath = Properties.Settings.Default.ColourLastMetaDataFolder;
                        break;
                    case "Common Name":
                        dialog.SelectedPath = Properties.Settings.Default.CommonNameLastMetaDataFolder;
                        break;
                    case "Cultivar":
                        dialog.SelectedPath = Properties.Settings.Default.CultivarLastMetaDataFolder;
                        break;
                    case "Genus":
                        dialog.SelectedPath = Properties.Settings.Default.GenusLastMetaDataFolder;
                        break;
                    case "Shape":
                        dialog.SelectedPath = Properties.Settings.Default.ShapeLastMetaDataFolder;
                        break;
                    case "Species":
                        dialog.SelectedPath = Properties.Settings.Default.SpeciesLastMetaDataFolder;
                        break;
                }
                //dialog.SelectedPath = Properties.Settings.Default.RecentMetaDataFolder;
            }
            
            dialog.ShowNewFolderButton = true;
            
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK && dialog.SelectedPath.Length > 0)
            {
                if (CreatingMetaData)     // we are after the general folder for meta data
                {
                    Properties.Settings.Default.RecentMetaDataFolder = dialog.SelectedPath;
                    Properties.Settings.Default.Save();
                    return dialog.SelectedPath;  
                }
                else         // We are after the specific folder
                {
                    switch (metaDataName)
                    {
                        case "Bloom View":
                            Properties.Settings.Default.BloomViewLastMetaDataFolder =  dialog.SelectedPath;
                            Properties.Settings.Default.Save();
                            break;
                        case "Camera Angle":
                            Properties.Settings.Default.CameraAngleLastMetaDataFolder = dialog.SelectedPath;
                            break;
                        case "Colour":
                            Properties.Settings.Default.ColourLastMetaDataFolder = dialog.SelectedPath;
                            break;
                        case "Common Name":
                            Properties.Settings.Default.CommonNameLastMetaDataFolder = dialog.SelectedPath;
                            break;
                        case "Cultivar":
                            Properties.Settings.Default.CultivarLastMetaDataFolder = dialog.SelectedPath;
                            break;
                        case "Genus":
                            Properties.Settings.Default.GenusLastMetaDataFolder = dialog.SelectedPath;
                            break;
                        case "Shape":
                            Properties.Settings.Default.ShapeLastMetaDataFolder = dialog.SelectedPath;
                            break;
                        case "Species":
                            Properties.Settings.Default.SpeciesLastMetaDataFolder = dialog.SelectedPath;
                            break;
                    }
                    Properties.Settings.Default.Save();
                    return dialog.SelectedPath;
                }
             }
                return "";
        }

        // GetLastMetaDataFolder


        #endregion  // Manage Meta data
        // ======================================= End of Manage Meta data ==--=====================================


        // ==========================================================================================================
        // ======================================= Inport/Export data Region ========================================
        // ==========================================================================================================

        #region Inport/Export data Region
        private void toolStripMenuItemImportData_Click(object sender, EventArgs e)
        {
            string lastExportFolder = Properties.Settings.Default.ExportFolder;
            string selectedPathHeader = "Select the folder you wish to load, typicaly the folder will be named like this:\n" +
                "Flower Space Data 2020-02-26 101935";
            lastExportFolder = Helper.FolderBrowser(lastExportFolder, selectedPathHeader, false);

            string pth = Path.Combine(lastExportFolder, "Application Data", "FlowerSpace.db");
            if (!File.Exists(pth))
            {
                MessageBox.Show("Could not find the folder:\n" + pth, "Inport aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; 
            }

            DialogResult ans = MessageBox.Show("Do you want add to the excting data (select Yes) or replace it (select No)", "Inport Flower Space data", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (ans == DialogResult.Cancel) return;

            if(ans == DialogResult.Yes)       // Add to the existing data
            {
                MessageBox.Show("Sorry this option not yet available", "Inport aborted",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
                return;

                /*
                    public DataTable GetDataTable(string tablename)
                        {
                        DataTable DT = new DataTable();
                        con.Open();
                        cmd = con.CreateCommand();
                        cmd.CommandText = string.Format("SELECT * FROM {0}", tablename);
                        adapter = new SQLiteDataAdapter(cmd);
                        adapter.AcceptChangesDuringFill = false;
                        adapter.Fill(DT);
                        con.Close();
                        DT.TableName = tablename;
                        foreach (DataRow row in DT.Rows)
                            {
                            row.AcceptChanges();
                            }
                        return DT;
                        }
                    public void SaveDataTable(DataTable DT)
                        {
                        try
                            {
                            con.Open();
                            cmd = con.CreateCommand();
                            cmd.CommandText = string.Format("SELECT * FROM {0}", DT.TableName);
                            adapter = new SQLiteDataAdapter(cmd);
                            SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);
                            adapter.Update(DT);
                            con.Close();
                            }
                        catch (Exception Ex)
                            {
                            System.Windows.MessageBox.Show(Ex.Message);
                            }
                        }
                */
                // Copy new content
                //               string[] filePathsx = Directory.GetFiles(fromPath);
                //             foreach (string filePath in filePathsx) File.Copy(fromPath, Helper.ApplicationDataPath() + "\\" + Path.GetFileName(filePath), true);

                //           LoadAllforSelection();
            }
            else                              // Replace existing
            {
                string fromPath = Path.Combine(lastExportFolder, "Application Data");

                // delete existing content
                string[] filePaths = Directory.GetFiles(Helper.ApplicationDataPath());
                foreach (string filePath in filePaths)
                    File.Delete(filePath);

                // Copy new content
                string[] filePathsx = Directory.GetFiles(fromPath);
                foreach (string filePath in filePathsx) File.Copy(fromPath, Helper.ApplicationDataPath() + "\\" + Path.GetFileName(filePath), true);

                LoadAllforSelection();
            }
        }
        // toolStripMenuItemImportData_Click

        private void toolStripMenuItemExportData_Click(object sender, EventArgs e)
        {
            string lastExportFolder = Properties.Settings.Default.ExportFolder;
            string selectedPathHeader = "A new folder will be created in the folder you select. The folder will be named like this:\n" +
                "Flower Space Data 2020-02-26 101935";
            lastExportFolder = Helper.FolderBrowser(lastExportFolder, selectedPathHeader, true);

            string exportPath = "Flower Space Data " + DateTime.Now.ToString("yyyy-MM-dd hh mm ss tt");
            string thisExportPath = Path.Combine(lastExportFolder, exportPath);


            bool exportDataPathExists = Helper.PathExistsCreate(thisExportPath);

            if(exportDataPathExists)
            {
                string applicationDataExportPath = Path.Combine(thisExportPath, "Application Data");
                string applicationDataXMLRecord = Path.Combine(thisExportPath, "Application Data Document");

                if ( Helper.PathExistsCreate(applicationDataXMLRecord))
                {
                    DataSet ExportDataset = BloomData.getExportData();
                    ExportDataset.WriteXml(Path.Combine(applicationDataXMLRecord, "Flower Space Data.xml"));
                }
                else 
                {
                    MessageBox.Show("Could not create the export folder:\n" + applicationDataXMLRecord, "Export aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                if (Helper.PathExistsCreate(applicationDataExportPath))
                {
                    foreach (var file in Directory.GetFiles(Helper.ApplicationDataPath()))
                        File.Copy(file, Path.Combine(applicationDataExportPath, Path.GetFileName(file)));

                    Properties.Settings.Default.LastAppDataExport = applicationDataExportPath;
                }
                else
                {
                    MessageBox.Show("Could not create the export folder:\n" + applicationDataExportPath, "Export aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else 
            {
                MessageBox.Show("Could not create the export folder:\n" + thisExportPath, "Export aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Properties.Settings.Default.ExportFolder = lastExportFolder;
            
            Properties.Settings.Default.Save();
            Notification.ApplicationDataExported(thisExportPath);
        }
        //toolStripMenuItemExportData_Click
        #endregion
        // ======================================= Inport/Export data Region ======================================= 


        //===================================================================================

        #region Name functions
        private void SetCommonNameAutoComplete()
        {
            AutoCompleteStringCollection acs = new AutoCompleteStringCollection();

            DataTable dt = BloomData.GetCommonNameAutoComplete();
            foreach (DataRow row in dt.Rows) { acs.Add(row.Field<string>("name")); }


            txtCommonName.AutoCompleteCustomSource = acs;
            txtCommonName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtCommonName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }

        private void SearchCommon_Click(object sender, EventArgs e)
        {

        }

        private void TxtCommonName_Leave(object sender, EventArgs e)
        {
            if (txtCommonName.Text.Length < 3)
            {
                txtCommonName.Text = "";
                return;
            }

            bool gotit = txtCommonName.AutoCompleteCustomSource.Contains(txtCommonName.Text);
            if (!gotit)
            {
                DialogResult dl = MessageBox.Show("Save new common name '" + txtCommonName.Text + "' to the database", "Save new common name", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dl == DialogResult.Yes)
                {
                    int index = BloomData.InsertCommon_Name(txtCommonName.Text);
                }
                SetCommonNameAutoComplete();
            }
            else
            {
                int i = txtCommonName.AutoCompleteCustomSource.IndexOf(txtCommonName.Text);
            }
        }

        private void SetCultivarNameAutoComplete()
        {
            AutoCompleteStringCollection acs = new AutoCompleteStringCollection();

            DataTable dt = BloomData.GetCultivarAutoComplete();
            foreach (DataRow row in dt.Rows) { acs.Add(row.Field<string>("Name")); }

            txtCultivarName.AutoCompleteCustomSource = acs;
            txtCultivarName.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtCultivarName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }

        private void TxtCultivarName_Leave(object sender, EventArgs e)
        {
            if (txtCultivarName.Text.Length < 3)
            {
                txtCultivarName.Text = "";
                labelBotanicalName.Text = BotanicalName(txtGenus.Text, txtSpecies.Text, txtCultivarName.Text);
                return;
            }
            bool gotit = txtCultivarName.AutoCompleteCustomSource.Contains(txtCultivarName.Text);
            if (!gotit)
            {
                DialogResult dl = MessageBox.Show("Save new cultivar name '" + txtCultivarName.Text + "' to the database", "Save new cultivar name", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dl == DialogResult.Yes)
                {
                    int index = BloomData.InsertCultivar_Name(txtCultivarName.Text);
                }
                SetCultivarNameAutoComplete();
            }
            else
            {
                int i = txtCultivarName.AutoCompleteCustomSource.IndexOf(txtCultivarName.Text);
            }
        }

        private void SearchCultivar_Click(object sender, EventArgs e)
        {
            DataTable cnsd = BloomData.GetCultivarSelect();
            SearchForm sf = new SearchForm(cnsd);
            sf.ShowDialog();
            txtCultivarName.Text = sf.Selected();
            labelBotanicalName.Text = BotanicalName(txtGenus.Text, txtSpecies.Text, txtCultivarName.Text);
            txtCultivarName.Focus();
        }


        private void SetGenusNameAutoComplete()
        {
            AutoCompleteStringCollection acs = new AutoCompleteStringCollection();

            DataTable dt = BloomData.GetGenusAutoComplete();
            foreach (DataRow row in dt.Rows) { acs.Add(row.Field<string>("Name")); }

            txtGenus.AutoCompleteCustomSource = acs;
            txtGenus.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtGenus.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }

        private void SetSpeciesNameAutoComplete()
        {
            AutoCompleteStringCollection acs = new AutoCompleteStringCollection();

            DataTable dt = BloomData.GetSpeciesAutoComplete();
            foreach (DataRow row in dt.Rows) { acs.Add(row.Field<string>("Name")); }

            txtSpecies.AutoCompleteCustomSource = acs;
            txtSpecies.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtSpecies.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        }
  
        private void SearchCommonName_Click(object sender, EventArgs e)
        {
            DataTable cnsd = BloomData.GetCommonNameSelect();
            SearchForm sf = new SearchForm(cnsd);
            sf.ShowDialog();
            txtCommonName.Text = sf.Selected();
            txtCommonName.Focus();
        }

        private void SearchCultivar_Click_1(object sender, EventArgs e)
        {
            DataTable cnsd = BloomData.GetCultivarSelect();
            SearchForm sf = new SearchForm(cnsd);
            sf.ShowDialog();
            txtCultivarName.Text = sf.Selected();
            labelBotanicalName.Text = BotanicalName(txtGenus.Text, txtSpecies.Text, txtCultivarName.Text);
            txtCultivarName.Focus();
        }

        private void SearchSpecies_Click_1(object sender, EventArgs e)
        {
            DataTable cnsd = BloomData.GetSpeciesSelect();
            SearchForm sf = new SearchForm(cnsd);
            sf.ShowDialog();
            txtSpecies.Text = sf.Selected();
            txtGenus.Text = BloomData.GetGenusOfSpecies(txtSpecies.Text);
            labelBotanicalName.Text = BotanicalName(txtGenus.Text, txtSpecies.Text, txtCultivarName.Text);
            txtSpecies.Focus();
        }

        private void SearchGenus_Click_1(object sender, EventArgs e)
        {
            DataTable cnsd = BloomData.GetGenusSelect();
            SearchForm sf = new SearchForm(cnsd);
            sf.ShowDialog();
            txtGenus.Text = sf.Selected();
            labelBotanicalName.Text = BotanicalName(txtGenus.Text, txtSpecies.Text, txtCultivarName.Text);
            txtGenus.Focus();
        }

        // SearchSpecies_Click



        #endregion

        private void macTrackBarBloomSIze_ValueChanged(object sender, decimal value)
        {
            textBoxBloomsize.Text = macTrackBarBloomSIze.Value.ToString();
            labelBloomSize.Text = textBoxBloomsize.Text + " mm";
        }

        private void macTrackBarInflorescenceWidth_ValueChanged(object sender, decimal value)
        {
            textBoxInflorescenseWidth.Text = macTrackBarInflorescenceWidth.Value.ToString();
            labelInflorescenseWidthHight.Text = textBoxInflorescenseHight.Text + " X " + textBoxInflorescenseWidth.Text + " mm";

        }

        private void macTrackBarInflorescenceHight_ValueChanged(object sender, decimal value)
        {
            textBoxInflorescenseHight.Text = macTrackBarInflorescenceHight.Value.ToString();
            labelInflorescenseWidthHight.Text = textBoxInflorescenseHight.Text + " X " + textBoxInflorescenseWidth.Text + " mm";
        }

        private void listViewInflorescences_Click(object sender, EventArgs e)
        {
            textBoxlInflorescences.Text = listViewInflorescences.SelectedItems[0].Text;
            textBoxlInflorescencesDescription.Text = listViewInflorescences.SelectedItems[0].SubItems[1].Text;
        }

        /// <summary>
        /// Temporary for text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxShapeName_Click(object sender, EventArgs e)
        {
            listViewShapes.Items[2].Selected = true;
            listViewInflorescences.Items[3].Selected = true;
        }

        private void tabPageViewerConfiguration_Enter(object sender, EventArgs e)
        {
            RefreshViewConfigurationInformation();
        }

        private void RefreshViewConfigurationInformation()
        {
            textBoxViewerConfiguration0.Text = ConfigurationReport(0);
            textBoxViewerConfiguration1.Text = ConfigurationReport(1);
            textBoxViewerConfiguration2.Text = ConfigurationReport(2);
            textBoxViewerConfiguration3.Text = ConfigurationReport(3);
            textBoxViewerConfiguration4.Text = ConfigurationReport(4);
        }

        // ================================== Manage thumbnail selection =============================================
        #region Manage thumbnail selection

        int ThumnailIndex = -1;

        /// <summary>
        /// Only one button should indicate as the Thumbnail as per the ThumbnailIndex
        /// </summary>
        private void NormaliseThmbnailButtons()
        {
            if (!(ThumnailIndex == 0)) { NormaliseThmbnailButton(buttonSetThumbnail0); }
            if (!(ThumnailIndex == 1)) { NormaliseThmbnailButton(buttonSetThumbnail1); }
            if (!(ThumnailIndex == 2)) { NormaliseThmbnailButton(buttonSetThumbnail2); }
            if (!(ThumnailIndex == 3)) { NormaliseThmbnailButton(buttonSetThumbnail3); }
            if (!(ThumnailIndex == 4)) { NormaliseThmbnailButton(buttonSetThumbnail4); }
        }

        /// <summary>
        /// Set the buttons back to the normal state
        /// </summary>
        /// <param name="button"></param>
        private void NormaliseThmbnailButton(System.Windows.Forms.Button button)
        {
            button.Text = "Set As Thumbnail";
            button.BackColor = Color.Gainsboro;
        }


        private void buttonSetThumbnail0_MouseClick(object sender, MouseEventArgs e)
        {
            if (cameraDevice.AllocatedCamera(0))           // Does this viewer have a cammera allocated
            {
                if (buttonSetThumbnail0.Text == "Set As Thumbnail")
                {
                    buttonSetThumbnail0.Text = "Thumbnail";
                    buttonSetThumbnail0.BackColor = Color.Yellow;
                    ThumnailIndex = 0;
                    NormaliseThmbnailButtons();
                }
            }
            else { SystemSounds.Beep.Play(); }
        }

        private void buttonSetThumbnail1_MouseClick(object sender, MouseEventArgs e)
        {
            if (cameraDevice.AllocatedCamera(1))           // Does thid viewer have a cammera allocated
            {
                if (buttonSetThumbnail1.Text == "Set As Thumbnail")
                {
                    buttonSetThumbnail1.Text = "Thumbnail";
                    buttonSetThumbnail1.BackColor = Color.Yellow;
                    ThumnailIndex = 1;
                    NormaliseThmbnailButtons();
                }
            }
            else { SystemSounds.Beep.Play(); }
        }

        private void buttonSetThumbnail2_MouseClick(object sender, MouseEventArgs e)
        {
            if (cameraDevice.AllocatedCamera(2))           // Does thid viewer have a cammera allocated
            {
                if (buttonSetThumbnail2.Text == "Set As Thumbnail")
                {
                    buttonSetThumbnail2.Text = "Thumbnail";
                    buttonSetThumbnail2.BackColor = Color.Yellow;
                    ThumnailIndex = 2;
                    NormaliseThmbnailButtons();
                }
            }
            else { SystemSounds.Beep.Play(); }
        }

        private void buttonSetThumbnail3_MouseClick(object sender, MouseEventArgs e)
        {
            if (cameraDevice.AllocatedCamera(3))           // Does thid viewer have a cammera allocated
            {
                if (buttonSetThumbnail3.Text == "Set As Thumbnail")
                {
                    buttonSetThumbnail3.Text = "Thumbnail";
                    buttonSetThumbnail3.BackColor = Color.Yellow;
                    ThumnailIndex = 3;
                    NormaliseThmbnailButtons();
                }
            }
            else { SystemSounds.Beep.Play(); }
        }

        private void buttonSetThumbnail4_MouseClick(object sender, MouseEventArgs e)
        {
            if (cameraDevice.AllocatedCamera(4))           // Does thid viewer have a cammera allocated
            {
                if (buttonSetThumbnail4.Text == "Set As Thumbnail")
                {
                    buttonSetThumbnail4.Text = "Thumbnail";
                    buttonSetThumbnail4.BackColor = Color.Yellow;
                    ThumnailIndex = 4;
                    NormaliseThmbnailButtons();
                }
            }
            else { SystemSounds.Beep.Play(); }
        }


        private string ConfigurationReport(int ViewerIndex)
        {
            string rpt2 = viewers.GetViewerReport(ViewerIndex);

            string rpt = "";

            rpt = bloomViews.Report(ViewerIndex);
            rpt = rpt + nl + cameraAngles.Report(ViewerIndex);
            rpt = rpt + nl + cameraDevice.Report(ViewerIndex);

            return rpt;
        }

        private void buttonViewer0Configure_Click(object sender, EventArgs e)
        {
            if (buttonViewer0Configure.Text == "Un Configure" ) { return;}

            ConfigurationThisViewer(0, cameraPhoto0);

            if (cameraDevice.AllocatedCamera(0))
            {
                buttonViewer0Configure.Text = "Un Configure";
                viewPortState.SetState(0, Enum_State.CameraViewStreamingCameraImage);
            }

            ResetViewerStatus();
            textBoxViewerConfiguration0.Text = ConfigurationReport(0);
            this.Refresh();
        }


        private void buttonViewer1Configure_Click(object sender, EventArgs e)
        {
            if (buttonViewer1Configure.Text == "Un Configure") { return; }

            ConfigurationThisViewer(1, cameraPhoto1);

            if (cameraDevice.AllocatedCamera(1))
            {
                buttonViewer1Configure.Text = "Un Configure";
                viewPortState.SetState(1, Enum_State.CameraViewStreamingCameraImage);
            }

            ResetViewerStatus();
            textBoxViewerConfiguration1.Text = ConfigurationReport(1);
            this.Refresh();
        }

        private void buttonViewer2Configure_Click(object sender, EventArgs e)
        {
            if (buttonViewer2Configure.Text == "Un Configure") { return; }

            ConfigurationThisViewer(2, cameraPhoto2);

            if (cameraDevice.AllocatedCamera(2))
            {
                buttonViewer2Configure.Text = "Un Configure";
                viewPortState.SetState(2, Enum_State.CameraViewStreamingCameraImage);

            }
            ResetViewerStatus();
            textBoxViewerConfiguration2.Text = ConfigurationReport(2);
            this.Refresh();
        }

        private void buttonViewer3Configure_Click(object sender, EventArgs e)
        {
            if (buttonViewer3Configure.Text == "Un Configure") { return; }

            ConfigurationThisViewer(3, cameraPhoto3);

            if (cameraDevice.AllocatedCamera(3))
            {
                buttonViewer3Configure.Text = "Un Configure";
                viewPortState.SetState(3, Enum_State.CameraViewStreamingCameraImage);

            }
            ResetViewerStatus();
            textBoxViewerConfiguration3.Text = ConfigurationReport(3);
            this.Refresh();
        }

        private void buttonViewer4Configure_Click(object sender, EventArgs e)
        {
            if (buttonViewer4Configure.Text == "Un Configure") { return; }

            ConfigurationThisViewer(4, cameraPhoto4);

            if (cameraDevice.AllocatedCamera(4))
            {
                buttonViewer4Configure.Text = "Un Configure";
                viewPortState.SetState(4, Enum_State.CameraViewStreamingCameraImage);

            }
            ResetViewerStatus();
            textBoxViewerConfiguration4.Text = ConfigurationReport(4);
            this.Refresh();
        }

        private void toolStripComboBoxListViewView_TextChanged(object sender, EventArgs e)
        {
            switch (toolStripComboBoxListViewView.Text)
            {
                case "Detail":
                    dataListView.View = View.Details;
                    //                    dataListView.CheckBoxes = true;
                    break;
                case "Small icon":
                    dataListView.View = View.SmallIcon;
                    //                    dataListView.CheckBoxes = true;
                    break;
                case "Large icon":
                    dataListView.View = View.LargeIcon;
                    //                    dataListView.CheckBoxes = true;
                    break;
                case "Tile":
                    dataListView.CheckBoxes = false;
                    //                    dataListView.View = View.Tile;
                    break;
                case "List":
                    dataListView.View = View.List;
                    //                    dataListView.CheckBoxes = true;
                    break;
                default:
                    dataListView.View = View.Details;
                    //                    dataListView.CheckBoxes = true;
                    break;
            }
        }

        private void buttonViewer0Future_Click(object sender, EventArgs e)
        {
            //BloomData.TestBuildWrite();
        }

        #region Handle Help menu
        private void toolStripMenuItemUserGuide_Click(object sender, EventArgs e)
        {
            string filename = "Unknown";

            try
            {
                filename = Path.Combine(Application.StartupPath, "Flower Space Capture User Guide.pdf");
                System.Diagnostics.Process.Start(filename);
            }
            catch (Exception ex)
            {
                string[] LogText = new string[] { ("Error opening user guide"), "File name " + filename };
                writelog.LogWriter(LogText);
                MessageBox.Show(ex.Message + nl + filename, "Error opening file ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            Form about = new AboutBox1();
            about.ShowDialog();
        }

        private void toolStripMenuItemApplicationLog_Click(object sender, EventArgs e)
        {
            string filename = "Unknown";

            try
            {
                filename = Path.Combine(Helper.ApplicationDataPath(), "log.txt");
                System.Diagnostics.Process.Start(filename);
            }
            catch (Exception ex)
            {
                string[] LogText = new string[] { ("Error opening the application log"), "File name " + filename };
                writelog.LogWriter(LogText);
                MessageBox.Show(ex.Message + nl + filename, "Error opening file ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButtonSearch_Click(object sender, EventArgs e)
        {
            LoadAllforSelection();
        }

        #endregion
        // Handlel Help menu

        #endregion
        // ================================== Manage thumbnail selection =============================================


    }   // End of Form1
        // ============================================= End of Form1 ===================================================


}
