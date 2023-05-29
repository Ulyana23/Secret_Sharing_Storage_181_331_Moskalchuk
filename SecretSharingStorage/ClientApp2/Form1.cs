using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonGet_Click(object sender, EventArgs e)
        {
            var getForm = new Form3();
            getForm.ShowDialog();
        }

        private void buttonSet_Click(object sender, EventArgs e)
        {
            var setForm = new Form2();
            setForm.ShowDialog();
        }
    }
}
