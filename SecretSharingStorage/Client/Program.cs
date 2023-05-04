using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect("127.0.0.1", 8001);
                Console.WriteLine("Client connected.");

                Console.Write("Введите сообщение: ");
                string message = Console.ReadLine();

                byte[] data = Encoding.UTF8.GetBytes(message);

                clientSocket.Send(data);

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
