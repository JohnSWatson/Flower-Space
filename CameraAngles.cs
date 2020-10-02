using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flower_Space
{

    public class CameraAngles
    {
        Dictionary<string, CameraAngle> cameraAngle = new Dictionary<string, CameraAngle>();
        string nl = Environment.NewLine;

        /// <summary>
        /// Constructor
        /// </summary>
        public CameraAngles()
        {
            DataTable angles = BloomData.GetCamera_Angle_Table();

            foreach (DataRow row in angles.Rows)
            {
                CameraAngle angle = new CameraAngle();
                angle._name = row.Field<string>(0);
                angle._description = row.Field<string>(1);
                angle._imagename = row.Field<string>(2);
                angle._allocated = false;
                angle._allocatedTo = -1;

                cameraAngle.Add(angle._name, angle);
            }
        }

        public bool AllocateBloomView(string AngleName, int ViewIndex)
        {
            foreach (CameraAngle angle in cameraAngle.Values)
            {
                if (angle._name == AngleName)
                {
                    if (!angle._allocated)
                    {
                        angle._allocated = true;
                        angle._allocatedTo = ViewIndex;
                        angle._anglename = AngleName;
                    }
                }
            }

            return false;
        }

        public string GetAllocatedBloomView(int ViewIndex)
        {
            foreach (CameraAngle angle in cameraAngle.Values)
            {
                if (angle._allocatedTo == ViewIndex)
                {
                    return angle._anglename;
                }
            }

            return "Not available";
        }

        public List<string> UnAllocatedCameraAngles()
        {
            List<string> mylist = new List<string>();

            foreach (CameraAngle angle in cameraAngle.Values)
            {
                if (!angle._allocated) { mylist.Add(angle._name ); };
            }
            return mylist;
        }

        public void FillCameraAnglesComboBox(System.Windows.Forms.CheckedListBox CameraAnglesListBox)
        {
            foreach (CameraAngle angle in cameraAngle.Values)
            {
                if ( !angle._allocated)
                { CameraAnglesListBox.Items.Add(angle._name); }
            }
        }

        public string getAngleDescription(string Name)
        {
            foreach (CameraAngle angle in cameraAngle.Values)
            {
                if (angle._name == Name)
                { return angle._description; }
            }
            return "";
        }
       
        public string Report(int ViewerIndex)
        {
            string rpt = "Camera angle:";

            foreach (CameraAngle angle in cameraAngle.Values)
            {
                if (angle._allocatedTo == ViewerIndex)
                {
                    rpt = rpt + nl + angle._name + nl + angle._description + nl;
                }
            }
            return rpt;
        }
    }

    public class CameraAngle
    {
        public string _name;
        public string _description;
        public string _imagename;
        public string _anglename;
        public bool _allocated;
        public int _allocatedTo;
    }
}
