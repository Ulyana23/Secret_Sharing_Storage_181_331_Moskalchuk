using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientApp2
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void buttonSendSecret_Click(object sender, EventArgs e)
        {
            string text1 = textBox1.Text;
            string text2 = textBox2.Text;

            textBox1.Text = "";
            textBox2.Text = "";

            if (text1 == text2)
            {
                JObject json = new JObject();
                json.Add("clientId", Program.clientId);
                json.Add("message", text1);
                json.Add("type", "set");

                // Преобразуем объект JObject в строку json
                string jsonString = json.ToString();
                byte[] dataSet = Encoding.UTF8.GetBytes(jsonString);

                Program.clientSocket.Send(dataSet, 0, dataSet.Length, SocketFlags.None);

                // Получаем буфер для чтения ответа от сервера
                dataSet = new byte[1024];
                int bytesRead = Program.clientSocket.Receive(dataSet);
                string secretId = Encoding.ASCII.GetString(dataSet, 0, bytesRead);

                if (secretId == "false")
                {
                    var errorForm = new ErrorForm("К сожалению, сервера сейчас недоступны, попробуйте позже.");
                    errorForm.ShowDialog(this);
                    return;
                }

                else
                {
                    var messageForm = new MessageForm("ID вашего секрета: " + secretId);
                    messageForm.ShowDialog(this);
                }
            }
            else
            {
                var errorForm = new ErrorForm("Секрет не совпадает, попробуйте ещё раз!");
                errorForm.ShowDialog(this);
            }

        }
    }
}
