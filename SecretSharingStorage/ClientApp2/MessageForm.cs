﻿using System;
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
    public partial class MessageForm : Form
    {
        public MessageForm(string text)
        {
            InitializeComponent();
            this.messageText.Text = text;
        }
    }
}
