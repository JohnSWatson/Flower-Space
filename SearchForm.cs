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
    public partial class SearchForm : Form
    {
        public SearchForm()
        {
            InitializeComponent();
        }

        private int index;
        private string selected;

         public SearchForm(DataTable dt)
        {
            InitializeComponent();
            listBox1.DataSource = dt;
            listBox1.ValueMember = "ID";
            listBox1.DisplayMember = "Name";
        }

        public int SelectedIndex()
        { return index; }

        public string Selected()
        { return selected; }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            index = listBox1.SelectedIndices[0];
            selected = listBox1.GetItemText(listBox1.SelectedItem);
            DialogResult = DialogResult.OK;
        }
    }
}
