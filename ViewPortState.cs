using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flower_Space
{
    public class ViewPortState
    {
        Dictionary<int, ViewPort> viewPort = new Dictionary<int, ViewPort>();

        public ViewPortState()
        {
            for (int i = 0; i < 5; ++i)
            {
                ViewPort port = new ViewPort();
                port.viewPortIndex = i;
                port.configured = false;
                port.state = Enum_State.CameraViewNotConfigured;

                viewPort.Add(i, port);
            }
        }

        public void SetState(int ViewerIndex, Enum_State State)
        {
            viewPort[ViewerIndex].state = State;

            if (State == Enum_State.CameraViewStreamingCameraImage) { viewPort[ViewerIndex].configured = true; }
        }

        public Enum_State GetState(int ViewerIndex)
        {
            return viewPort[ViewerIndex].state;
        }

        public string GetStateText(int ViewerIndex)
        {
            return viewPort[ViewerIndex].GetStateText();
        }

        public Color GetStateColour(int ViewerIndex)
        {
            return viewPort[ViewerIndex].GetStateColor();
        }

        public void SetConfigured(int ViewerIndex)
        {
            viewPort[ViewerIndex].configured = true;
        }
        public bool IsConfigured(int ViewerIndex)
        {
            return viewPort[ViewerIndex].configured;
        }
    }

    public class ViewPort
    {
        public int viewPortIndex;
        public bool configured;
        public Enum_State state;

        public string GetStateText()
        {
            switch (state)
            {
                case Enum_State.CameraViewNotConfigured:
                    return "Not Used";
                case Enum_State.CameraViewShowingCameraImage:
                    return "Captured Image";
                case Enum_State.CameraViewShowingSavedImage:
                    return "Saved Image";
                case Enum_State.CameraViewStreamingCameraImage:
                    return "Streaming";
                case Enum_State.NoSavedImageToShow:
                    return "No Image";
            }

            return "Unknown";
                
        }

        public Color GetStateColor()
        {
            switch (state)
            {
                case Enum_State.CameraViewNotConfigured:
                    return Properties.Settings.Default.CameraViewNotConfigured;
                case Enum_State.CameraViewShowingCameraImage:
                    return Properties.Settings.Default.CameraViewShowingCameraImage;
                case Enum_State.CameraViewShowingSavedImage:
                    return Properties.Settings.Default.CameraViewShowingSavedImage;
                case Enum_State.CameraViewStreamingCameraImage:
                    return Properties.Settings.Default.CameraViewStreamingCameraImage;
                case Enum_State.NoSavedImageToShow:
                    return Properties.Settings.Default.CameraViewNoPhotoAvailable;
            }
            return Color.Red;
        }

    }
}
