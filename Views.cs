using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flower_Space
{
    public class Views
    {

        Dictionary<string, ViewofBloom> View = new Dictionary<string, ViewofBloom>();
        string nl = Environment.NewLine;

        public Views()
        {
            DataTable Views = BloomData.GetViews();

            foreach ( DataRow row in Views.Rows)
            {
                ViewofBloom view = new ViewofBloom();
                view._name = row.Field<string>(0);
                view._description = row.Field<string>(1);
                view._allocated = false;
                view._allocatedTo = -1;

                View.Add(view._name, view);
            }
        }

        public bool AllocateBloomView(string ViewName, int ViewIndex)
        {
            foreach (ViewofBloom view in View.Values)
            {
                if ( view._name == ViewName)
                {
                    if ( !view._allocated )
                    {
                        view._allocated = true;
                        view._allocatedTo = ViewIndex;
                    }
                }
            }
             
            return false;
        }

        public List<string> GetAllocatedBloomView(int ViewerIndex)
        {
            List<string> views = new List<string>();

            foreach (ViewofBloom view in View.Values)
            {
                if (view._allocatedTo == ViewerIndex)
                {
                    views.Add(view._name);
                }
            }

            return views;
        }

        public List<string> UnAllocatedBloomViews()
        {
            List<string> mylist = new List<string>();

            foreach (ViewofBloom view in View.Values)
            {
                if ( !view._allocated) { mylist.Add(view._name); };
            }
            return mylist;
        }

        public void FillBloomViewComboBox(System.Windows.Forms.CheckedListBox BloomViewCheckedListBox)
        {
            foreach (ViewofBloom view in View.Values)
            {
                if (!view._allocated)
                { BloomViewCheckedListBox.Items.Add(view._name); }
            }
        }

        public string getViewDescription(string PhotoViewName)
        {
            foreach (ViewofBloom view in View.Values)
            {
                if (view._name == PhotoViewName)
                {
                    return view._description; 
                }
            }
            return "";
        }

        public string Report(int ViewerIndex)
        {
            string rpt = "Bloom viewes:";

            foreach (ViewofBloom view in View.Values)
            {
                if (view._allocatedTo == ViewerIndex)
                {
                    rpt = rpt + nl + view._name + nl + view._description + nl;
                }
            }
            return rpt;
        }
    }



    public class ViewofBloom
    {
        public string _name;
        public string _description;
        public bool _allocated;
        public int _allocatedTo;
    }
}
