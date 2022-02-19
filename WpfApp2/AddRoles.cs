using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfApp2
{
    public partial class AddRoles : Form
    {
        public List<string> Rezult;
        public AddRoles(List<string> roles, List<string> thisRoles)
        {
            InitializeComponent();

            ListBox.ObjectCollection collection = new ListBox.ObjectCollection(listBox2, roles.Where(w => !thisRoles.Contains(w)).ToArray());
            ListBox.ObjectCollection collection2 = new ListBox.ObjectCollection(listBox1, thisRoles.ToArray());

            listBox1.Items.AddRange(collection2);
            listBox2.Items.AddRange(collection);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.AddRange(listBox2.Items);
            listBox2.Items.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.AddRange(listBox1.Items);
            listBox1.Items.Clear();
        }

        private void listBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            var item = (sender as ListBox);
            if (item?.SelectedItem != null)
            {
                listBox1.Items.Add(item.SelectedItem.ToString());
                listBox2.Items.Remove(item.SelectedItem);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (sender as ListBox);
            if (item?.SelectedItem != null)
            {
                listBox2.Items.Add(item.SelectedItem.ToString());
                listBox1.Items.Remove(item.SelectedItem);
            }
        }

        private void AddRoles_FormClosing(object sender, FormClosingEventArgs e)
        {
            Rezult = listBox1.Items.Cast<string>().ToList();

        }
    }
}
