using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp1
{
    public partial class ErrorForm : Form
    {
        public ErrorForm(string text)
        {
            InitializeComponent();
            this.errorMessage.TabStop = false;
            this.errorMessage.Text = text;
        }
    }
}
