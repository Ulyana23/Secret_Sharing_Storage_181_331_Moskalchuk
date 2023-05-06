using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Data.SQLite;
using System.Numerics;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;

namespace Server1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                // Устанавливаем IP-адрес и порт
                IPAddress sourceIpAddress = IPAddress.Parse("127.0.0.1");
                int sourcePort = 8001;

                Socket sourceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                sourceSocket.Bind(new IPEndPoint(sourceIpAddress, sourcePort));
                sourceSocket.Listen(1);
                Console.WriteLine("Сервер 1 запущен. Ожидание подключений...");



                // Принимаем входящее соединение от клиента
                Socket clientSocket = sourceSocket.Accept();
                Console.WriteLine("Клиент подключен к серверу 1.");

                byte[] buffer = new byte[1024];
                int length = clientSocket.Receive(buffer);
                string message = Encoding.UTF8.GetString(buffer, 0, length);
                Console.WriteLine("Получено сообщение: " + message);

                // разделяем секрет
                List<BigInteger> secretParts = ShamirSecretSharing.SecretSharing(WordToNumber(message));
                Console.WriteLine("secretParts before: " + string.Join(", ", secretParts));
                BigInteger P = secretParts[0];
                secretParts.RemoveAt(0);
                Console.WriteLine("secretParts after: " + string.Join(", ", secretParts));

                DatabaseWork.FillDatabase(1, secretParts[0].ToString(), P.ToString());
                DatabaseWork.ReadDataFromSecretsTable();

                // соединяем секрет
                BigInteger n = ShamirSecretSharing.SecretRecovery(secretParts, P);
                string decodedWord = NumberToWord(n);

                byte[] data = Encoding.UTF8.GetBytes(message);

                // отправляем данные обратно клиенту
                clientSocket.Send(data, 0, data.Length, SocketFlags.None);

                // Отправляем сообщение на сервер 2
                Socket destinationSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                byte[] secretPart = Encoding.UTF8.GetBytes(secretParts[0].ToString());
                destinationSocket.Connect("127.0.0.1", 8002);
                destinationSocket.Send(secretPart);
                Console.WriteLine("Сообщение отправлено на сервер 2.");


                // Закрываем соединения
                //sourceSocket.Shutdown(SocketShutdown.Both);
                sourceSocket.Close();

                destinationSocket.Shutdown(SocketShutdown.Both);
                destinationSocket.Close();

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

            Console.WriteLine($"Десятичное число: {decimalNumber}");

            return decimalNumber;
        }


        public static string NumberToWord(BigInteger decimalNumber)
        {
            byte[] bytes2 = decimalNumber.ToByteArray();

            Array.Reverse(bytes2);

            Console.WriteLine(BitConverter.ToString(bytes2));

            string decodedWord = Encoding.UTF8.GetString(bytes2);
            Console.WriteLine($"Вот что получилось: {decodedWord}");

            return decodedWord;
        }
    }

    public static class ShamirSecretSharing
    {
        public static List<BigInteger> SecretSharing(BigInteger sMessageNum)
        {
            // BigInteger sMessageNum = 1029;
            BigInteger P = RandomNumberGenerator.RandomPrime(sMessageNum, 10000000000000000000); // модуль

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

        public static BigInteger SecretRecovery(List<BigInteger> secretParts, BigInteger P)
        {
            BigInteger sMessageNum = 0;
            Console.WriteLine("Размер списка: " + secretParts.Count);
            for (int i = 0; i < secretParts.Count; i++)
            {
                Console.WriteLine(i);
                // int S0 = Modulus((FindPart(1, p1, 5) + FindPart(2, p2, 5) + FindPart(3, p3, 5) + FindPart(4, p4, 5) + FindPart(5, p5, 5)), P);
                sMessageNum += FindPart(i + 1, secretParts[i], secretParts.Count);
            }
            sMessageNum = Modulus(sMessageNum, P);
            Console.WriteLine(sMessageNum);

            return sMessageNum;
        }

        private static BigInteger FindP(BigInteger message, BigInteger x, BigInteger a, BigInteger b)
        {
            BigInteger p = (BigInteger)(message + a * x + b * x * x);
            Console.WriteLine("p" + x + " = " + p);

            return p;
        }


        private static BigInteger FindPart(BigInteger index, BigInteger number, BigInteger secretPartsNumber)
        {
            BigInteger numerator = 1;
            for (BigInteger i = 1; i < secretPartsNumber + 1; i++)
            {
                if (i != index)
                {
                    numerator *= i;
                }
            }

            BigInteger denominator = 1;
            for (BigInteger i = 1; i < secretPartsNumber + 1; i++)
            {
                if (i != index) denominator *= (i - index);
            }

            BigInteger result = number * numerator / denominator;

            Console.WriteLine("R " + result);
            return result;
        }


        public static BigInteger Modulus(BigInteger number, BigInteger divisor)
        {
            BigInteger modulus = number % divisor;
            if (modulus < 0) modulus += divisor;
            return modulus;
        }
    }


    public static class DatabaseWork
    {
        public static void FillDatabase(int secretPartId, string secretPart, string P)
        {
            string connectionString = "Data Source=C:\\Users\\User\\source\\repos\\SecretSharingStorage\\Server1\\Server1Database.sqlite;Version=3;";
            
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
                        using (SQLiteCommand command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS SecretsTable (id INTEGER PRIMARY KEY, secret_part_id INTEGER, secret_part VARCHAR(32), modulus VARCHAR(32))", connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Заполняем таблицу Secrets одной строкой значений
                        using (SQLiteCommand command = new SQLiteCommand("INSERT INTO SecretsTable (secret_part_id, secret_part, modulus) VALUES (@secretPartId, @secretPart, @P)", connection))
                        {
                            // Добавляем параметры запроса
                            command.Parameters.AddWithValue("@secretPartId", secretPartId);
                            command.Parameters.AddWithValue("@secretPart", secretPart);
                            command.Parameters.AddWithValue("@P", P);

                            command.ExecuteNonQuery();
                        }

                        // Фиксируем транзакцию
                        transaction.Commit();
                        connection.Close();

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

                            // Выводим данные на экран
                            Console.WriteLine("id: {0}, secret_part_id: {1}, secret_part: {2}, modulus: {3}", id, secret_part_id, secret_part, modulus);
                        }
                    }
                }

                connection.Close();
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