using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tulpep.NotificationWindow;

namespace Flower_Space
{
    public static class Notification
    {
        private const int ShortNotification = 3000;
        private const int MediumNotification = 5000;
        private const int LongNotification = 10000;

        public static void ConfigurationRequired()
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Configuration required";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.NotConfigured;
            popup.ContentText = "The camera viewers must be attached to a camera. To do this right click each of the Test Patterns.\n\nWhen configured a Left click will start streaming from the camers.";
            popup.Delay = LongNotification;

            popup.Popup();// show 

        }

        public static void Ready()
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Ready";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.WhatsNext;
            popup.ContentText = "Pictures from each attached camera are shown\nRight click each picture and configre the views for that camera.";
            popup.Delay = LongNotification;
           
            popup.Popup();// show 
        }


        public static void LiveStreaming()
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Streaming live video";
            //popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.CameraStream;
            popup.ContentText = "The camera with the icon on the left is streaming live vedeo.";
            popup.Delay = MediumNotification;

            popup.Popup();// show 
        }

        public static void SnapShotTaken()
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Snapshot taken";
            //popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.CameraSnapShot;
            popup.ContentText = "The camera with the icon on the left is showing a snapshot ready to save./n/nFirst complete all the data.";
            popup.Delay = MediumNotification;

            popup.Popup();// show 
        }

        public static void StartConfigureCameras()
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Great idea";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.WhatsNext;
            popup.ContentText = "Select one or more views from the list/nEach camera must have at least one view or be, not used";
            popup.Delay = LongNotification;

            popup.Popup();// show 

        }

        public static void StopConfigureCameras()
        {
                string string1 = "Well done.@@Keepit up.";
                string string2 = string1.Replace("@", "\n");
         //       NI.ShowBalloonTip(ShortNotification, "Configure cameras", string2, ToolTipIcon.Info);
        }

        public static void DoingConfigureCameras()
        {
                string string1 = "Get on with it.@@Keepit up.@";
                string string2 = string1.Replace("@", "\n");
         //       NI.ShowBalloonTip(ShortNotification, "Configure cameras", string2, ToolTipIcon.Info);
        }

        public static void AllImagesRefreshed()
        {
            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Images refreshedd";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.CameraSnapShot;
            popup.ContentText = "All the pictures have been refreshed from the cameras.";
            popup.Delay = ShortNotification;

            popup.Popup();// show 
        }

        public static void ShowingDataFromDatabase()
        {
            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Images and data from the database";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.Disk;
            popup.ContentText = "The data and images are from the database.";
            popup.Delay = ShortNotification;

            popup.Popup();// show 
        }


        public static void ShapeSelected(string name)
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Shape selected";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.Shape;
                ;
            popup.ContentText = "This flower has a  " + name + " shape.";
            popup.Delay = ShortNotification;

            popup.Popup();// show 

        }

        public static void MetaDataFileCreated(string Folder)
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = "Meta data XML file created";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.MetaData;
            ;
            popup.ContentText = "The file(s) are in here  " + Folder + ".";
            popup.Delay = ShortNotification;

            popup.Popup();// show 

        }

        public static void MetaDataFileApplied(string MetaDataName)
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = MetaDataName + " meta data has been applied.";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.MetaData;
            
            popup.ContentText = "The " + MetaDataName + " in the database has been replaced.";
            popup.Delay = ShortNotification;

            popup.Popup();// show 

        }

        public static void MetaDataFileNotApplied(string MetaDataName)
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = MetaDataName + " meta data has been applied.";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.MetaData;
            
            popup.ContentText = "The " + MetaDataName + " has not been updated due to an error.";
            popup.Delay = ShortNotification;

            popup.Popup();// show 

        }

        public static void Exception(string Doing, string Message)
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = Doing;
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.Shape;
            ;
            popup.ContentText = Message;
            popup.Delay = ShortNotification;

            popup.Popup();// show 

        }

        public static void ApplicationDataExported(string path)
        {
            if (Properties.Settings.Default.NotificationState == false) return;

            PopupNotifier popup = new PopupNotifier();
            popup.TitleText = " Application data has been exported.";
            popup.HeaderColor = Color.AliceBlue;
            popup.Image = Properties.Resources.ApplicationData;

            popup.ContentText = "The data was saved to:\n" + path;
            popup.Delay = ShortNotification;

            popup.Popup();// show 

        }
    }
}
