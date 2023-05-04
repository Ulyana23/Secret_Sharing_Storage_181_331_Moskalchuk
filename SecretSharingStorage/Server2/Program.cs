using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.SQLite;

namespace Server2
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress destinationIpAddress = IPAddress.Parse("127.0.0.1");
            int destinationPort = 8002;

            Socket destinationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            destinationSocket.Bind(new IPEndPoint(destinationIpAddress, destinationPort));
            destinationSocket.Listen(1);
            Console.WriteLine("Сервер 2 запущен. Ожидание подключений...");

            Socket sourceSocket = destinationSocket.Accept();
            Console.WriteLine("Сервер 2 подключен к серверу 1.");

            byte[] buffer = new byte[1024];
            int length = sourceSocket.Receive(buffer);
            string message = Encoding.UTF8.GetString(buffer, 0, length);
            Console.WriteLine("Получено сообщение: " + message);

            // Закрываем соединения
            //destinationSocket.Shutdown(SocketShutdown.Both);
            destinationSocket.Close();

            Console.ReadLine();
        }
    }
}
