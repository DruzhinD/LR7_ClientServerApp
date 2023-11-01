using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    internal class ServerProgram
    {
        //примеры для отправки клиенту  
        static Dictionary<string, int> mathExamples = new()
        {
            {"100 + 10 + 1 = ", 111 },
            {"50 * 5 = ",  250},
            {"250 * 10 / 2 = ", 1250 },
            {"1 + 9 * 2 = ", 19 },
            {"2 + 3 - 9 = ", -4 },
            {"2 * 3 * 4 = ", 24 }
        };

        static void Main(string[] args)
        {
            Console.Title = "Сервер";

            do
            {
                Processing();
                Console.WriteLine("Для перезапуска сервера нажмите любую клавишу. " +
                    "Для выхода - Escape.");
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

        }

        //метод для отправки примеров клиенту
        static void Processing()
        {
            int port = 8005; //порт для приема входящих запросов
            IPEndPoint ipPoint = new(IPAddress.Any, port);
            IPHostEntry myHost = Dns.GetHostEntry(Dns.GetHostName());

            //вывод доступных ip адресов
            foreach (IPAddress ip in myHost.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    Console.WriteLine($"IP-адрес сервера: {ip}");
            }

            Socket listenSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(ipPoint);
            listenSocket.Listen(10); //установка состояния прослушивания с длиной очереди
            Console.WriteLine("Сервер запущен.");

            Console.WriteLine("Ожидание подключений...");
            Socket hendler = listenSocket.Accept(); //получение входящих подключений

            try
            {
                StringBuilder builder = new StringBuilder();
                //флаг на случай неверного ответа от клиента
                //false - сохранение пары ключ-значение для повтора ввода
                //true - переход к следующему примеру
                bool flagRepeat = false;
                //получаем случайный пример (string) с ответом (int)
                Random rndIndex = new Random();

                //липовая инициализация, для подавления ошибки
                int savedRndIndex = 0;

                do
                {
                    KeyValuePair<string, int> examplePair;

                    if (flagRepeat)
                    {
                        savedRndIndex = rndIndex.Next(0, mathExamples.Count);
                        examplePair = mathExamples.ElementAt(savedRndIndex);
                    }
                    else
                    {
                        //ошибка была здесь
                        examplePair = mathExamples.ElementAt(savedRndIndex);
                        flagRepeat = true;
                    }

                    //отправка задания
                    string sendedTask = examplePair.Key;
                    Sending(hendler, sendedTask);

                    //получение решения
                    builder = Receiving(hendler);

                    //вывод примера и полученного решения
                    Console.WriteLine("{0}: {1}{2}",
                        arg0: DateTime.Now.ToShortTimeString(),
                        arg1: examplePair.Key,
                        arg2: builder.ToString());

                    //проверка решения с клиента
                    string message;
                    if (int.TryParse(builder.ToString(), out int result))
                    {
                        if (examplePair.Value == result)
                            message = "Верно";
                        else
                            message = "Неверно";
                    }
                    else
                    {
                        Console.WriteLine($"Введенное значение: {builder}");
                        message = "none";
                    }
                    //отправка ответа клиенту
                    Sending(hendler, message);

                    //если решение неверное, то
                    //получение запроса от пользователя на вывод ответа
                    //или повторного решения примера
                    if (message.ToLower() == "неверно")
                    {
                        builder = Receiving(hendler);
                        if (builder.ToString() == "ans")
                            Sending(hendler, examplePair.Value.ToString());
                        else
                            flagRepeat = false;
                    }

                    Console.WriteLine(new string('-', 20));
                } while (builder.ToString() != "end");

                //закрытие сокета
                hendler.Shutdown(SocketShutdown.Both);
                hendler.Close();
                listenSocket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// метод получения сообщения с клиента
        /// </summary>
        /// <param name="socket">сокет, к которому подключен клиент</param>
        /// <returns>перезаписанный StringBuilder с сообщением с клиента</returns>
        static StringBuilder Receiving(Socket socket)
        {
            StringBuilder builder = new StringBuilder();
            int bytes; //количество полученных байтов
            byte[] data = new byte[256]; //буфер для получаемых данных
            do
            {
                bytes = socket.Receive(data);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (socket.Available > 0);

            return builder;
        }

        /// <summary>
        /// Отправка сообщения на клиент
        /// </summary>
        /// <param name="socket">сокет</param>
        /// <param name="message">сообщение</param>
        static void Sending(Socket socket, string message)
        {
            byte[] data;
            data = Encoding.Unicode.GetBytes(message);
            socket.Send(data);
        }


        //код из задания ЛР
        static void ProcessingLabExample()
        {int port = 8005; //порт для приема входящих запросов
            IPEndPoint ipPoint = new(IPAddress.Any, port);
            IPHostEntry myHost = Dns.GetHostEntry(Dns.GetHostName());

            //вывод доступных ip адресов
            foreach (IPAddress ip in myHost.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    Console.WriteLine($"IP-адрес сервера: {ip}");
            }

            Socket listenSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(ipPoint);
            listenSocket.Listen(10);
            Console.WriteLine("Сервер запущен.");

            Console.WriteLine("Ожидание подключений...");
            Socket hendler = listenSocket.Accept(); //получение входящих подключений
            try
            {
                int bytes = 0; //количество полученных байтов
                byte[] data = new byte[256]; //буфер для получаемых данных
                StringBuilder builder = new StringBuilder();

                do
                {
                    //получение сообщения
                    builder.Clear();
                    do
                    {
                        bytes = hendler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (hendler.Available > 0);

                    Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + builder.ToString());

                    //отправка ответа
                    string message = "ваше сообщение доставлено";
                    data = Encoding.Unicode.GetBytes(message);
                    hendler.Send(data);
                } while (builder.ToString() != "end");

                //закрытие сокета
                hendler.Shutdown(SocketShutdown.Both);
                hendler.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}