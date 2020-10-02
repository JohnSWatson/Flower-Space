using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flower_Space
{

    public class CameraDevice
    {
        string nl = Environment.NewLine;
        private FilterInfoCollection _videoDevices;
        private Dictionary<string, Camera> _camera = new Dictionary<string, Camera>();

        /// <summary>
        /// Class constructor
        /// </summary>
        public CameraDevice()
        {
            EnumerateCameras();
        }
        // Class constructor


        public int Count { get { return _camera.Count; } }
       //public int AvailableCameras { get { return _videoDevices.Count - _allocatedVideoDevices.Count; } }

        /// <summary>
        /// List all the cameras attached, give each a unique ENGLISH name
        /// Track there use and allocations
        /// </summary>
        private void EnumerateCameras()
        {
            _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);


            //FilterInfo VideoCaptureDevice in VideoCaptureDevices
            int i = 0;
            foreach (FilterInfo dev in _videoDevices)
            {
                //CameraInfo cameraInfo = new CameraInfo();
                Camera thisCamera = new Camera();


                thisCamera.name = i + ": " +  dev.Name;
                thisCamera.moniker = dev.MonikerString;
                thisCamera.cameraIndex = i;
                thisCamera.viewerIndex = -1;
                thisCamera.running = false;
                
                thisCamera.allocated = false;
                _camera.Add(thisCamera.name, thisCamera);

//                cameraInfos.Add(cameraInfo);

                i++;
            }

        }
        
        public bool LoadListOfAvailableCameras(System.Windows.Forms.ComboBox cameraSelector)
        {
            foreach( Camera info in _camera.Values)
            {
                if ( !info.allocated )    // then this camera is allocated
                {
                    cameraSelector.Items.Add(info.name);
                }
            }
            return true;
        }

        public Camera GetCameraInfo(string CameraName)
        {
            foreach (Camera info in _camera.Values)
            {
                if (info.name == CameraName)    // then this camera is allocated
                {
                    return info;
                }
            }

            return new Camera();   // A null return
        }

        public void AllocateCamera(string AllocatedName, System.Windows.Forms.PictureBox AllocatedViewer,int AllocatedViewerIndex)
        {
            foreach (Camera cam in _camera.Values)
             {
                if ( cam.name == AllocatedName )
                {
                    StopAllCameras();               // We are going to start this one
                    cam.allocated = true;
                    cam.myViewer = AllocatedViewer;
                    cam.viewerIndex = AllocatedViewerIndex;
                    cam.name = AllocatedName;
                    cam.StartMe();
                }
             }
        }

        public void UnallocateCamera(int ViewerIndex)
        {
            foreach (Camera cam in _camera.Values)
            {
                if (cam.viewerIndex == ViewerIndex)
                {
                    cam.StopMe();
                    cam.allocated = false;
                    cam.running = false;
                    cam.myViewer = null;
                    cam.viewerIndex = -1;
                    cam.cameraIndex = -1;
                }
            }
        }

        public bool AllocatedCamera(int ViewerIndex)
        {
            foreach (Camera cam in _camera.Values)
            {
                if (cam.viewerIndex == ViewerIndex)
                {
                    return cam.allocated;
                }
            }

            return false;
        }


        public int CountAllocatedCameras()
        {
            int i = 0;

            foreach (Camera cam in _camera.Values)
            {
                if (cam.allocated) ++i;
            }
            return i;
        }
        public void StartCamera(int ViewerIndex, string CameraName, System.Windows.Forms.PictureBox AllocatedViewer)
        {
            foreach (Camera cam in _camera.Values)
            {
                if (cam.name == CameraName)
                {
                    StopAllCameras();
                    cam.myViewer = AllocatedViewer;
                    cam.viewerIndex = ViewerIndex;
                    StartACamera(cam);
                    return;
                }
            }
        }

        public void StartCamera(int ViewerIndex)
        {
            foreach (Camera cam in _camera.Values)
            {
                if (cam.viewerIndex == ViewerIndex)
                {
                    StopAllCameras();
                    StartACamera(cam);
                    return;
                }
            }
        }

        public bool IsCameraRunning(int ViewerIndex)
        {
            foreach (Camera cam in _camera.Values)
            {
                if (cam.viewerIndex == ViewerIndex)
                {
                    return cam.running;
                }

            }
            return false;
        }

        private void StartACamera(Camera info)
        {
            if (!info.running )
            {
                StopAllCameras();
                info.StartMe();
            }
        }

        public string Report(int ViewerIndex)
        {
            string rpt = "Camera:" + nl;

            foreach (Camera info in _camera.Values)
            {
                if (info.viewerIndex == ViewerIndex)
                {
                    return info.Report();
                }
            }
            return "No report";
        }

    public void SetDiskImageReport(int ViewerIndex, string Report)
        {
            foreach (Camera info in _camera.Values)
            {
                if (info.viewerIndex == ViewerIndex)
                {
                    info.diskImageReport = Report;
                    info.showingDiskImage = true;  // we just set the report so it must be true
                }
            }
        }

        
        public void StopAllCameras()
        {
            foreach (Camera info in _camera.Values)
            {
                if (info.running)
                { 
                    info.StopMe();
                }
            }
        }


    }
    //class CameraDevice

    public class Camera
    {
        public int cameraIndex;
        public string name;
        public string moniker;
        public bool allocated = false;
        public int viewerIndex;
        public System.Windows.Forms.PictureBox myViewer;
        public VideoCaptureDevice myCaptureDevice;

        public bool showingDiskImage = false;
        public string diskImageReport = "No report";

        public bool running = false;

        string nl = Environment.NewLine;

        private void video_NewFrameHandler(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                //myViewer.Image = new Bitmap(eventArgs.Frame);
                myViewer.Image = Helper.CropToMaxSquare( new Bitmap(eventArgs.Frame));
            }
            catch (Exception ex)
            {
            }        
        }

        public void StartMe()
        {
            if (!running)
            {
                showingDiskImage = false;
                myCaptureDevice = new VideoCaptureDevice(moniker);
                myCaptureDevice.NewFrame += new NewFrameEventHandler(video_NewFrameHandler);
                myCaptureDevice.Start();
                running = true;
            }
        }

        public void StopMe()
        {
            if (running == true)
            {
                myCaptureDevice.SignalToStop();
                myCaptureDevice.WaitForStop();
                myCaptureDevice.NewFrame -= new NewFrameEventHandler(video_NewFrameHandler);
                running = false;
            }
        }

        public string Report()
        {
            if (showingDiskImage) return diskImageReport;

            string rpt = "";
            rpt = rpt + "Name         " + name + nl;
            rpt = rpt + "Index          " + cameraIndex + nl;
            rpt = rpt + "Assigned     " + ((allocated) ? "Yes" : "No") + nl;
            rpt = rpt + "Viewer         " + (viewerIndex + 1).ToString() + nl;

            return rpt;
        }
    }
    //  class Camera
}
// namespace Flower_Space
