using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.SQLite;
using Newtonsoft.Json.Linq;

namespace Server3
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                bool flag = false;
                IPAddress destinationIpAddress = IPAddress.Parse("127.0.0.1");
                int destinationPort = 8003;

                Socket destinationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                destinationSocket.Bind(new IPEndPoint(destinationIpAddress, destinationPort));
                destinationSocket.Listen(1);
                Console.WriteLine("Сервер 2 запущен. Ожидание подключений...");
                Socket sourceSocket = destinationSocket.Accept();
                Console.WriteLine("Сервер 2 подключен к серверу 1.");

                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int length = sourceSocket.Receive(buffer);
                    string jsonString = Encoding.UTF8.GetString(buffer, 0, length);

                    // получаем данные из json
                    JObject jsonObject = JObject.Parse(jsonString);

                    string type = (string)jsonObject.GetValue("type");
                    int clientId = (int)jsonObject.GetValue("clientId");
                    int secretId = (int)jsonObject.GetValue("secretId");

                    switch (type)
                    {
                        case "set":
                            int secretPartId = (int)jsonObject.GetValue("secretPartId");
                            string secretPart = (string)jsonObject.GetValue("secretPart");
                            string modulus = (string)jsonObject.GetValue("modulus");

                            // заполняем базу данных
                            DatabaseWork.FillDatabase(secretPartId, secretPart, modulus, clientId, secretId);
                            Console.WriteLine(secretId);
                            DatabaseWork.ReadDataFromSecretsTable();

                            flag = true;
                            byte[] dataSet = Encoding.UTF8.GetBytes(flag.ToString());

                            // отправляем данные обратно серверу 1
                            sourceSocket.Send(dataSet, 0, dataSet.Length, SocketFlags.None);

                            break;

                        case "get":
                            Tuple<string, string, int> result = DatabaseWork.GetSecretPartFromDatabase(clientId, secretId);
                            JObject json = new JObject
                            {
                                { "modulus", result.Item2 },
                                { "secretPart", result.Item1 },
                                { "secretPartId", result.Item3 },
                            };

                            byte[] dataGet = Encoding.UTF8.GetBytes(json.ToString());

                            // отправляем данные обратно серверу 1
                            sourceSocket.Send(dataGet, 0, dataGet.Length, SocketFlags.None);

                            break;

                        default:
                            break;
                    }
                }

                // Закрываем соединения
                // destinationSocket.Shutdown(SocketShutdown.Both);
                destinationSocket.Close();
                Console.ReadLine();
            }

            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
            }
        }
    }

    // --- КЛАСС ДЛЯ РАБОТЫ С БД ---
    public static class DatabaseWork
    {
        public static void FillDatabase(int secretPartId, string secretPart, string P, int userId, int secretId)
        {
            string connectionString = "Data Source=C:\\Users\\User\\source\\repos\\SecretSharingStorage\\Server3\\Server3Database.sqlite;Version=3;";

            // Создаем подключение к базе данных
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Начинаем транзакцию
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Создаем таблицу Secrets, если ее нет
                        using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS SecretsTable (id INTEGER PRIMARY KEY, secret_part_id INTEGER, secret_part VARCHAR(32), modulus VARCHAR(32), user_id INTEGER)", connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Заполняем таблицу Secrets одной строкой значений
                        using (SQLiteCommand command = new SQLiteCommand("INSERT INTO SecretsTable (id, secret_part_id, secret_part, modulus, user_id) VALUES (@secretId, @secretPartId, @secretPart, @P, @userId)", connection))
                        {
                            // Добавляем параметры запроса
                            command.Parameters.AddWithValue("@secretId", secretId);
                            command.Parameters.AddWithValue("@secretPartId", secretPartId);
                            command.Parameters.AddWithValue("@secretPart", secretPart);
                            command.Parameters.AddWithValue("@P", P);
                            command.Parameters.AddWithValue("@userId", userId);

                            command.ExecuteNonQuery();
                        }

                        // Фиксируем транзакцию
                        transaction.Commit();
                        // connection.Close();
                    }

                    catch (Exception ex)
                    {
                        // Если возникла ошибка, откатываем транзакцию
                        transaction.Rollback();
                        Console.WriteLine("Ошибка при создании таблицы SecretsTable: " + ex.Message);
                    }
                }
            }
        }

        // Читаем данные из таблицы SecretsTable и выводим их на экран
        public static void ReadDataFromSecretsTable()
        {
            string connectionString = "Data Source=C:\\Users\\User\\source\\repos\\SecretSharingStorage\\Server3\\Server3Database.sqlite;Version=3;";

            // Создаем подключение к базе данных
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Выполняем запрос к таблице SecretsTable
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM SecretsTable", connection))
                {
                    // Получаем результаты запроса
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // Читаем данные из каждой строки
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            int secret_part_id = reader.GetInt32(1);
                            string secret_part = reader.GetString(2);
                            string modulus = reader.GetString(3);
                            int user_id = reader.GetInt32(4);

                            // Выводим данные на экран
                            Console.WriteLine("id: {0}, secret_part_id: {1}, secret_part: {2}, modulus: {3}", id, secret_part_id, secret_part, modulus);
                        }
                    }
                }

                connection.Close();
            }
        }

        public static Tuple<string, string, int> GetSecretPartFromDatabase(int clientId, int id)
        {
            // создаем строку подключения к базе данных SQLite
            string connectionString = "Data Source=C:\\Users\\User\\source\\repos\\SecretSharingStorage\\Server3\\Server3Database.sqlite;Version=3;";

            // создаем SQL-запрос для получения параметров secretPart и modulus из таблицы SecretParts
            string query = "SELECT secret_part, modulus, secret_part_id FROM SecretsTable WHERE id = @id AND user_id = @clientId";

            // создаем объект SQLiteConnection для подключения к базе данных
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                // создаем объект SQLiteCommand для выполнения SQL-запроса
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    // добавляем параметры в SQL-запрос
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@clientId", clientId);

                    // открываем соединение с базой данных
                    connection.Open();

                    // создаем объект SQLiteDataReader для чтения результатов SQL-запроса
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        // проверяем, что результат содержит хотя бы одну строку
                        if (reader.HasRows)
                        {
                            // читаем первую строку результата
                            reader.Read();

                            // получаем значения параметров secretPart, modulus и secretPartId из строки результата
                            string secretPart = reader.GetString(0);
                            string modulus = reader.GetString(1);
                            int secretPartId = reader.GetInt32(2);

                            // создаем объект Tuple для возвращения двух значений
                            Tuple<string, string, int> result = new Tuple<string, string, int>(secretPart, modulus, secretPartId);
                            return result;
                        }
                        else
                        {
                            // если результат не содержит строк, то возвращаем null
                            return null;
                        }
                    }
                }
            }
        }

    }
}

