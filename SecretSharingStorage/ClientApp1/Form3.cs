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
using System.Threading;

namespace ClientApp1
{
    public partial class Form3 : Form
    {
        
        private MessageForm waitForm;
        BackgroundWorker backgroundWorker1;


        public Form3()
        {
            InitializeComponent();
            waitForm = new MessageForm("Обработка...");
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }


        private void ButtonGetSecret_Click(object sender, EventArgs e)
        {
            string secretId = secretIdtextBox.Text;
            JObject json = new JObject();

            secretIdtextBox.Text = "";

            // Добавляем параметры json
            json.Add("clientId", Program.clientId);
            json.Add("message", secretId);
            json.Add("type", "get");

            // Преобразуем объект JObject в строку json
            string jsonString = json.ToString();

            byte[] dataGet = Encoding.UTF8.GetBytes(jsonString);

            Program.clientSocket.Send(dataGet, 0, dataGet.Length, SocketFlags.None);


            backgroundWorker1.RunWorkerAsync();
            waitForm.ShowDialog();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Получаем буфер для чтения ответа от сервера
            byte[] dataSet = new byte[1024];
            int bytesRead2 = Program.clientSocket.Receive(dataSet);
            string secret = Encoding.ASCII.GetString(dataSet, 0, bytesRead2);
            if (secret == "false")
            {
                var errorForm = new ErrorForm("К сожалению, сервера сейчас недоступны, попробуйте позже.");
                errorForm.ShowDialog();
                return;
            }
            
            else
            {
                var messageForm = new MessageForm("Ваш секрет: " + secret);
                messageForm.ShowDialog();
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Скрываем форму-заглушку
            waitForm.Hide();
        }
    }
}
