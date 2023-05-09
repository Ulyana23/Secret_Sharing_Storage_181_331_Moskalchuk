using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            int clientId = 2;
            try
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect("127.0.0.1", 8001);
                Console.WriteLine("Client connected.");

                Console.Write("Введите сообщение: ");
                string message = Console.ReadLine();

                byte[] data = Encoding.UTF8.GetBytes(message);

                clientSocket.Send(data, 0, data.Length, SocketFlags.None);

                // Получаем буфер для чтения ответа от сервера
                data = new byte[1024];
                int bytesRead = clientSocket.Receive(data);
                string rMessage = Encoding.ASCII.GetString(data, 0, bytesRead);
                Console.WriteLine("Получен ответ от сервера: " + rMessage);

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                Console.WriteLine("Client closed.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();

        }
    }
}
