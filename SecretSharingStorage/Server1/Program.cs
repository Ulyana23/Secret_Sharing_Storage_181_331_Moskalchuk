using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.SQLite;
using System.Numerics;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Server1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                List<int> availableServersList = new List<int>();

                // Устанавливаем IP-адрес и порт
                IPAddress sourceIpAddress = IPAddress.Parse("127.0.0.1");
                int sourcePort = 8001;

                Socket sourceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                sourceSocket.Bind(new IPEndPoint(sourceIpAddress, sourcePort));
                sourceSocket.Listen(10);
                Console.WriteLine("Сервер 1 запущен. Ожидание подключений...");

                Socket destinationSocket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket destinationSocket3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket destinationSocket4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket destinationSocket5 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    destinationSocket2.Connect("127.0.0.1", 8002);
                    Console.WriteLine("Подключение к Server2 выполнено успешно.");
                    availableServersList.Add(2);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Ошибка при подключении к Server2: " + ex.Message);
                }

                try
                {
                    destinationSocket3.Connect("127.0.0.1", 8003);
                    Console.WriteLine("Подключение к Server3 выполнено успешно.");
                    availableServersList.Add(3);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Ошибка при подключении к Server3: " + ex.Message);
                }

                try
                {
                    destinationSocket4.Connect("127.0.0.1", 8004);
                    Console.WriteLine("Подключение к Server4 выполнено успешно.");
                    availableServersList.Add(4);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Ошибка при подключении к Server4: " + ex.Message);
                }

                try
                {
                    destinationSocket5.Connect("127.0.0.1", 8005);
                    Console.WriteLine("Подключение к Server5 выполнено успешно.");
                    availableServersList.Add(5);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("Ошибка при подключении к Server5: " + ex.Message);
                }



                while (true)
                {
                    // Принимаем входящее соединение от клиента
                    Socket clientSocket = sourceSocket.Accept();
                    Console.WriteLine("Клиент подключен к серверу 1.");

                    // Запускаем обработку запроса в отдельном потоке
                    Thread clientThread = new Thread(() =>
                    {
                        while (true)
                        {
                            // получаем сообщение от клиента
                            byte[] buffer = new byte[1024];
                            int length = clientSocket.Receive(buffer);
                            string jsonString = Encoding.UTF8.GetString(buffer, 0, length);

                            JObject jsonObject = JObject.Parse(jsonString);

                            string message = (string)jsonObject.GetValue("message");
                            int clientId = (int)jsonObject.GetValue("clientId");
                            string type = (string)jsonObject.GetValue("type"); // получить или записать секрет

                            switch (type)
                            {
                                case "set":
                                    // разделяем секрет
                                    List<BigInteger> secretParts = ShamirSecretSharing.SecretSharing(WordToNumber(message));
                                    BigInteger P = secretParts[0];
                                    secretParts.RemoveAt(0);

                                    // заполняем базу данных сервера 1
                                    int secretId = DatabaseWork.FillDatabase(1, secretParts[0].ToString(), P.ToString(), clientId); // получаем id секрета
                                    // DatabaseWork.ReadDataFromSecretsTable();

                                    // отправляем запросы на остальные сервера
                                    if (availableServersList.Contains(2)) ServersConnection.SecretPartsToServers(destinationSocket2, clientId, secretId, secretParts[1].ToString(), P.ToString(), type, 2);
                                    if (availableServersList.Contains(3)) ServersConnection.SecretPartsToServers(destinationSocket3, clientId, secretId, secretParts[2].ToString(), P.ToString(), type, 3);
                                    if (availableServersList.Contains(4)) ServersConnection.SecretPartsToServers(destinationSocket4, clientId, secretId, secretParts[3].ToString(), P.ToString(), type, 4);
                                    if (availableServersList.Contains(5)) ServersConnection.SecretPartsToServers(destinationSocket5, clientId, secretId, secretParts[4].ToString(), P.ToString(), type, 5);

                                    if (availableServersList.Count != 4)
                                    {
                                        byte[] data = Encoding.UTF8.GetBytes("false");
                                        clientSocket.Send(data, 0, data.Length, SocketFlags.None);
                                        return;
                                    }
                                    else
                                    {
                                        byte[] data = Encoding.UTF8.GetBytes(secretId.ToString());
                                        // отправляем клиенту ID секрета
                                        clientSocket.Send(data, 0, data.Length, SocketFlags.None);
                                    }

                                    break;

                                case "get":
                                    Console.WriteLine("\r\nПолучен ID секрета: " + message + "\r\n");

                                    if (availableServersList.Count < 2) // если подключено меньше 3 серверов, то завершение программы
                                    {
                                        byte[] data = Encoding.UTF8.GetBytes("false");
                                        clientSocket.Send(data, 0, data.Length, SocketFlags.None);
                                        return;
                                    }

                                    List<Tuple<int, BigInteger, BigInteger>> secretPartsCurrent = new List<Tuple<int, BigInteger, BigInteger>>();

                                    Tuple<int, BigInteger, BigInteger> result = DatabaseWork.GetSecretPartFromDatabase(clientId, int.Parse(message)); // получаем часть секрета с сервера 1

                                    if (result != null)
                                    {
                                        secretPartsCurrent.Add(result);
                                        if (availableServersList.Contains(2)) secretPartsCurrent.Add(ServersConnection.SecretPartsFromServers(destinationSocket2, clientId, int.Parse(message), type, 2));
                                        if (availableServersList.Contains(3)) secretPartsCurrent.Add(ServersConnection.SecretPartsFromServers(destinationSocket3, clientId, int.Parse(message), type, 3));
                                        if (availableServersList.Contains(4)) secretPartsCurrent.Add(ServersConnection.SecretPartsFromServers(destinationSocket4, clientId, int.Parse(message), type, 4));
                                        if (availableServersList.Contains(5)) secretPartsCurrent.Add(ServersConnection.SecretPartsFromServers(destinationSocket5, clientId, int.Parse(message), type, 5));

                                        // Console.WriteLine(secretPartsCurrent);

                                        if (secretPartsCurrent.All(x => x.Item3 == secretPartsCurrent[0].Item3)) // если все модули совпадают
                                        {
                                            BigInteger modulus = secretPartsCurrent[0].Item3;
                                            BigInteger n = ShamirSecretSharing.SecretRecovery(secretPartsCurrent, modulus);
                                            string decodedWord = NumberToWord(n);

                                            byte[] data = Encoding.UTF8.GetBytes(decodedWord);
                                            // отправляем секрет клиенту
                                            clientSocket.Send(data, 0, data.Length, SocketFlags.None);
                                        }

                                        else
                                        {
                                            Console.WriteLine("Error");
                                            return;
                                        }
                                    }

                                    else
                                    {
                                        Console.WriteLine("Этот секрет принадлежит другому пользователю.");
                                        byte[] data = Encoding.UTF8.GetBytes("userIdError");
                                        // отправляем сообщение клиенту
                                        clientSocket.Send(data, 0, data.Length, SocketFlags.None);
                                        return;
                                    }

                                    break;

                                default:
                                    Console.WriteLine("Error");
                                    break;
                            }
                        }

                                // Console.WriteLine(clientId + " " + type);

                        /*
                        // соединяем секрет
                        BigInteger n = ShamirSecretSharing.SecretRecovery(secretParts, P);
                        string decodedWord = NumberToWord(n); */
                    });

                    // Запускаем поток для обработки запроса
                    clientThread.Start();
                }


                // Закрываем соединения
                sourceSocket.Shutdown(SocketShutdown.Both);
                sourceSocket.Close();

                destinationSocket2.Shutdown(SocketShutdown.Both);
                destinationSocket2.Close();

                destinationSocket3.Shutdown(SocketShutdown.Both);
                destinationSocket3.Close();

                destinationSocket4.Shutdown(SocketShutdown.Both);
                destinationSocket4.Close();

                destinationSocket5.Shutdown(SocketShutdown.Both);
                destinationSocket5.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("{0} Error code: {1}.", e.Message, e.ErrorCode);
            }

            Console.ReadLine();
        }


        public static BigInteger WordToNumber(string word)
        {
            // получаем байты
            byte[] bytes = Encoding.UTF8.GetBytes(word);

            Console.WriteLine(BitConverter.ToString(bytes));

            // преобразуем байты в строку
            string hexStr = BitConverter.ToString(bytes).Replace("-", "");

            // переводим в десятичную и приводим к типу BigInteger
            BigInteger decimalNumber = BigInteger.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);

            //Console.WriteLine($"Десятичное число: {decimalNumber}");

            return decimalNumber;
        }


        public static string NumberToWord(BigInteger decimalNumber)
        {
            byte[] bytes2 = decimalNumber.ToByteArray();

            Array.Reverse(bytes2);

            string decodedWord = Encoding.UTF8.GetString(bytes2);
            // Console.WriteLine($"ПОлученный секрет: {decodedWord}");

            return decodedWord;
        }
    }

    public static class ServersConnection
    {
        public static void SecretPartsToServers(Socket destinationSocket, int clientId, int secretId, string secretPart, string modulus, string type, int secretPartId)
        {
            JObject json = new JObject
            {
                { "clientId", clientId },
                { "secretId", secretId },
                { "secretPartId", secretPartId },
                { "secretPart", secretPart },
                { "modulus", modulus },
                { "type", type }
            };

            // Отправляем сообщение на сервер
            byte[] dataSecretPart = Encoding.UTF8.GetBytes(json.ToString());
            destinationSocket.Send(dataSecretPart);
            Console.WriteLine($"Часть секрета отправлена на сервер {secretPartId}.");

            // получаем сообщение от клиента
            byte[] buffer = new byte[1024];
            int length = destinationSocket.Receive(buffer);
            string flagString = Encoding.UTF8.GetString(buffer, 0, length);
            bool flag = Convert.ToBoolean(flagString);
            if (flag) Console.WriteLine($"Часть секрета успешно получена сервером {secretPartId}.");
        }

        public static Tuple<int, BigInteger, BigInteger> SecretPartsFromServers(Socket destinationSocket, int clientId, int secretId, string type, int serverId)
        {
            JObject json = new JObject
            {
                { "clientId", clientId },
                { "secretId", secretId },
                { "type", type }
            };

            // Отправляем сообщение на сервер
            byte[] data = Encoding.UTF8.GetBytes(json.ToString());
            destinationSocket.Send(data);
            Console.WriteLine($"Сообщение отправлено на сервер {serverId}.");

            // получаем сообщение от клиента
            byte[] buffer = new byte[1024];
            int length = destinationSocket.Receive(buffer);
            string jsonString = Encoding.UTF8.GetString(buffer, 0, length);

            JObject jsonObject = JObject.Parse(jsonString);

            string secretPart = (string)jsonObject.GetValue("secretPart");
            string modulus = (string)jsonObject.GetValue("modulus");
            int secretPartId = (int)jsonObject.GetValue("secretPartId");

            Console.WriteLine($"Часть секрета от сервера {serverId}: {secretPart}");

            Tuple<int, BigInteger, BigInteger> result = new Tuple<int, BigInteger, BigInteger>(secretPartId, BigInteger.Parse(secretPart), BigInteger.Parse(modulus));
            
            return result;
        }

    }

        public static class ShamirSecretSharing
    {
        public static List<BigInteger> SecretSharing(BigInteger sMessageNum)
        {
            BigInteger P = RandomNumberGenerator.RandomPrime(sMessageNum, 10000000000000000000); // модуль 10000000000000000000

            BigInteger a = RandomNumberGenerator.RandomBigInteger(1, P - 1);
            BigInteger b = RandomNumberGenerator.RandomBigInteger(1, P - 1);

            // находим каждую из частей
            BigInteger p1 = Modulus(FindP(sMessageNum, 1, a, b), P);
            BigInteger p2 = Modulus(FindP(sMessageNum, 2, a, b), P);
            BigInteger p3 = Modulus(FindP(sMessageNum, 3, a, b), P);
            BigInteger p4 = Modulus(FindP(sMessageNum, 4, a, b), P);
            BigInteger p5 = Modulus(FindP(sMessageNum, 5, a, b), P);

            List<BigInteger> secretParts = new List<BigInteger> { P, p1, p2, p3, p4, p5 };

            return secretParts;
        }

        public static BigInteger SecretRecovery(List<Tuple<int, BigInteger, BigInteger>> secretPartsCurrent, BigInteger P)
        {
            BigInteger sMessageNum = 0;
            BigInteger EylerNum = fi(secretPartsCurrent[0].Item3); // функция Эйлера

            for (int i = 0; i < secretPartsCurrent.Count; i++)
            {
                sMessageNum += FindPart(secretPartsCurrent, secretPartsCurrent[i].Item1, 
                    secretPartsCurrent[i].Item2, EylerNum);
            }

            sMessageNum = Modulus(sMessageNum, P);
            return sMessageNum;
        }

        public static BigInteger fi(BigInteger n)
        {
            BigInteger f = n;
            if (n % 2 == 0)
            {
                while (n % 2 == 0) n /= 2;
                f /= 2;
            }
            for (BigInteger i = 3; i * i <= n; i += 2)
            {
                if (n % i == 0)
                {
                    while (n % i == 0) n /= i;
                    f /= i;
                    f *= (i - 1);
                }
            }
            if (n > 1)
            {
                f /= n;
                f *= (n - 1);
            }
            return f;
        }

        public static BigInteger Eyler(BigInteger n)
        {
            BigInteger result = n;
            for (BigInteger i = 2; i * i <= n; ++i)
                if (n % i == 0)
                {
                    while (n % i == 0)
                        n /= i;
                    result -= result / i;
                }
            if (n > 1)
                result -= result / n;
            return result;
        }

        private static BigInteger FindP(BigInteger message, BigInteger x, BigInteger a, BigInteger b)
        {
            BigInteger p = (BigInteger)(message + a * x + b * x * x);
            Console.WriteLine("p" + x + " = " + p);

            return p;
        }


        private static BigInteger FindPart(List<Tuple<int, BigInteger, BigInteger>> secretPartsCurrent, 
            BigInteger index, BigInteger number, BigInteger EylerNum)
        {
            BigInteger numerator = 1;
            for (BigInteger i = 1; i < 6; i++)
            {
                if (i != index && secretPartsCurrent.Any(x => x.Item1 == i))
                {
                    numerator *= i;
                }
            }
            BigInteger denominator = 1;
            for (BigInteger i = 1; i < 6; i++)
            {
                if (i != index && secretPartsCurrent.Any(x => x.Item1 == i))
                {
                    denominator *= (i - index);
                }
            }

            BigInteger result1 = BigInteger.ModPow(denominator, EylerNum - 1, secretPartsCurrent[0].Item3);
            BigInteger result = Modulus(Modulus(number * numerator, secretPartsCurrent[0].Item3) * result1, 
                secretPartsCurrent[0].Item3);

            return result;
        }

        public static BigInteger FastPower(BigInteger a, BigInteger n, BigInteger m)
        {
            BigInteger result = 1;
            while (n > 0)
            {
                if ((n & 1) == 1)
                {
                    result = (result * a) % m;
                }
                a = (a * a) % m;
                n >>= 1;
            }
            return result;
        }


        public static BigInteger Modulus(BigInteger number, BigInteger divisor)
        {
            BigInteger modulus = number % divisor;
            if (modulus < 0) modulus += divisor;
            return modulus;
        }
    }


    // --- КЛАСС ДЛЯ РАБОТЫ С БД ---
    public static class DatabaseWork
    {
        public static int FillDatabase(int secretPartId, string secretPart, string P, int userId)
        {
            string connectionString = "Data Source=C:\\Users\\User\\source\\repos\\SecretSharingStorage\\Server1\\Server1Database.sqlite;Version=3;";
            int lastId = 0;

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
                        using (SQLiteCommand command = new SQLiteCommand("INSERT INTO SecretsTable (secret_part_id, secret_part, modulus, user_id) VALUES (@secretPartId, @secretPart, @P, @userId)", connection))
                        {
                            // Добавляем параметры запроса
                            command.Parameters.AddWithValue("@secretPartId", secretPartId);
                            command.Parameters.AddWithValue("@secretPart", secretPart);
                            command.Parameters.AddWithValue("@P", P);
                            command.Parameters.AddWithValue("@userId", userId);

                            command.ExecuteNonQuery();

                            lastId = (int)connection.LastInsertRowId;
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
            return lastId;
        }

        // Читаем данные из таблицы SecretsTable и выводим их на экран
        public static void ReadDataFromSecretsTable()
        {
            string connectionString = "Data Source=C:\\Users\\User\\source\\repos\\SecretSharingStorage\\Server1\\Server1Database.sqlite;Version=3;";

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

        public static Tuple<int, BigInteger, BigInteger> GetSecretPartFromDatabase(int clientId, int id)
        {
            // создаем строку подключения к базе данных SQLite
            string connectionString = "Data Source=C:\\Users\\User\\source\\repos\\SecretSharingStorage\\Server1\\Server1Database.sqlite;Version=3;";

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

                            // получаем значения параметров secretPart и modulus из строки результата
                            string secretPart = reader.GetString(0);
                            string modulus = reader.GetString(1);
                            int secretPartId = reader.GetInt32(2);

                            // создаем объект Tuple для возвращения нескольких значений
                            Tuple<int, BigInteger, BigInteger> result = new Tuple<int, BigInteger, BigInteger>(secretPartId, BigInteger.Parse(secretPart), BigInteger.Parse(modulus));
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


    public static class RandomNumberGenerator
    {
        private static bool IsPrime(BigInteger n)
        {
            // Check if the number is less than 2 or even
            if (n < 2 || n % 2 == 0)
            {
                return false;
            }

            // Find the largest power of 2 that divides n-1
            BigInteger d = n - 1;
            int s = 0;
            while (d % 2 == 0)
            {
                d /= 2;
                s++;
            }

            // Perform the Miller-Rabin test with k=10 iterations
            for (int i = 0; i < 10; i++)
            {
                BigInteger a = RandomBigInteger(2, n - 1);
                BigInteger x = BigInteger.ModPow(a, d, n);
                if (x == 1 || x == n - 1)
                {
                    continue;
                }
                for (int j = 0; j < s - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, n);
                    if (x == n - 1)
                    {
                        break;
                    }
                }
                if (x != n - 1)
                {
                    return false;
                }
            }

            return true;
        }

        public static BigInteger RandomBigInteger(BigInteger min, BigInteger max)
        {
            Random rnd = new Random();
            string numeratorString, denominatorString;
            double fraction = rnd.NextDouble();
            BigInteger inRange;

            //Maintain all 17 digits of precision, 
            //but remove the leading zero and the decimal point;
            numeratorString = fraction.ToString("G17").Remove(0, 2);

            //Use the length instead of 17 in case the random
            //fraction ends with one or more zeros
            denominatorString = string.Format("1E{0}", numeratorString.Length);

            inRange = (max - min) * BigInteger.Parse(numeratorString) /
                BigInteger.Parse(denominatorString,
                System.Globalization.NumberStyles.AllowExponent)
                + min;
            return inRange;
        }


        public static BigInteger RandomPrime(BigInteger min, BigInteger max)
        {
            BigInteger candidate;
            while (true)
            {
                // Generate a random candidate number in the range [min, max]
                candidate = RandomBigInteger(min, max);

                // Check if the candidate is prime using the Miller-Rabin test
                if (IsPrime(candidate))
                {
                    return candidate;
                }
            }
        }

    }
}