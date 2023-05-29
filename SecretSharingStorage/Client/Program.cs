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
            int clientId = 3;
            try
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect("127.0.0.1", 8001);
                //Console.WriteLine("Client connected.");

                JObject json = new JObject();
                string message = "";
                string jsonString = "";
                string choice = "";

                while (true)
                {
                    Console.WriteLine("Выберите один из трёх вариантов:");
                    Console.WriteLine("1. Я хочу добавить новый секрет в хранилище");
                    Console.WriteLine("2. Я хочу получить секрет из хранилища");
                    Console.WriteLine("3. Выход");
                    choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            Console.WriteLine("Вы выбрали добавить новый секрет в хранилище");
                            Console.Write("Введите сообщение: ");
                            message = Console.ReadLine();

                            // Добавляем параметры json
                            json.Add("clientId", clientId);
                            json.Add("message", message);
                            json.Add("type", "set");

                            // Преобразуем объект JObject в строку json
                            jsonString = json.ToString();

                            byte[] dataSet = Encoding.UTF8.GetBytes(jsonString);

                            clientSocket.Send(dataSet, 0, dataSet.Length, SocketFlags.None);

                            // Получаем буфер для чтения ответа от сервера
                            dataSet = new byte[1024];
                            int bytesRead = clientSocket.Receive(dataSet);
                            string secretId = Encoding.ASCII.GetString(dataSet, 0, bytesRead);
                            if (secretId == "false")
                            {
                                Console.WriteLine("К сожалению, сервера сейчас недоступны, попробуйте позже.\r\n");
                                return;
                            }
                            else Console.WriteLine("ID вашего секрета: " + secretId + "\r\n");
                            break;

                        case "2":
                            Console.WriteLine("Вы выбрали получить секрет из хранилища");
                            Console.Write("Введите ID вашего секрета: ");
                            message = Console.ReadLine();

                            // Добавляем параметры json
                            json.Add("clientId", clientId);
                            json.Add("message", message);
                            json.Add("type", "get");

                            // Преобразуем объект JObject в строку json
                            jsonString = json.ToString();

                            byte[] dataGet = Encoding.UTF8.GetBytes(jsonString);

                            clientSocket.Send(dataGet, 0, dataGet.Length, SocketFlags.None);

                            // Получаем буфер для чтения ответа от сервера
                            dataSet = new byte[1024];
                            int bytesRead2 = clientSocket.Receive(dataSet);
                            string secret = Encoding.ASCII.GetString(dataSet, 0, bytesRead2);
                            if (secret == "false")
                            {
                                Console.WriteLine("К сожалению, сервера сейчас недоступны, попробуйте позже.\r\n");
                                return;
                            }
                            else Console.WriteLine("Ваш секрет: " + secret + "\r\n");

                            break;
                        case "3":
                            Console.WriteLine("Выход...");
                            return;
                        default:
                            Console.WriteLine("Некорректный выбор");
                            break;
                    }
                    json.RemoveAll();
                }

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
