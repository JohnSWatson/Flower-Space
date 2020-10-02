using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flower_Space
{
    public enum WhatTheViewerIsDoing
    {
        Nothing,
        ShowingPictureFromCamera,
        ShowingPictureFromDisk
    }

    public class Viewers
    {

        Dictionary<int, ViewerInstance> viewerInstance = new Dictionary<int, ViewerInstance>();


        /// <summary>
        /// Constructors
        /// </summary>
        public Viewers()
        {

            for (int i = 0; i < 5; i++)
            {
                ViewerInstance vi = new ViewerInstance();
                vi.whattheviewerisdoing = WhatTheViewerIsDoing.Nothing;
                vi._nothingReport = "Not used";
                vi._ShowingPictureFromCameraReport = "Showing cameraa picture";
                vi._ShowingPictureFromCameraReport = "Streaming from Camera";

                viewerInstance.Add(i, vi);
            }
        }
    
        public void SetViewerReport(int ViewerIndex, WhatTheViewerIsDoing WTFID, string Report)
        {
            switch (WTFID)
            {
                case WhatTheViewerIsDoing.Nothing:
                    viewerInstance[ViewerIndex]._nothingReport = Report;
                    break;
                case WhatTheViewerIsDoing.ShowingPictureFromCamera:
                    viewerInstance[ViewerIndex]._ShowingPictureFromCameraReport = Report;
                    break;
                case WhatTheViewerIsDoing.ShowingPictureFromDisk:
                    viewerInstance[ViewerIndex]._ShowingPictureFromDiskReport = Report;
                    break;
            }
        }

        public string GetViewerReport(int ViewerIndex)
        {
            return viewerInstance[ViewerIndex].Report();

        }

    }

    class ViewerInstance
    {
        public WhatTheViewerIsDoing whattheviewerisdoing;
        public string _nothingReport;
        public string _ShowingPictureFromCameraReport;
        public string _ShowingPictureFromDiskReport;

        public string Report()
        {
            switch (whattheviewerisdoing)
            {
                case WhatTheViewerIsDoing.Nothing:
                    return _nothingReport;
                case WhatTheViewerIsDoing.ShowingPictureFromCamera:
                    return _ShowingPictureFromCameraReport;
                case WhatTheViewerIsDoing.ShowingPictureFromDisk:
                    return _ShowingPictureFromDiskReport;
            }

            return "Nothing to report";
        }
    }
}
