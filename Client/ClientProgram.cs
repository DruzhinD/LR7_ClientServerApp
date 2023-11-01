using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

namespace Client
{
    internal class ClientProgram
    {
        static void Main(string[] args)
        {
            Console.Title = "Клиент";

            do
            {
                Processing();
                Console.WriteLine("Для продолжения нажмите любую клавишу. Для выхода - Escape.");
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        static void Processing()
        {
            int port = 8005; // порт сервера
            
            Console.Write("Введите IP-адрес сервера: ");
            string address = Console.ReadLine();
            try
            {
                IPEndPoint ipPoint = new(IPAddress.Parse(address), port);
                Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);

                string? message;
                StringBuilder builder = new();

                do
                {
                    //получение задания от сервера
                    builder = Receiving(socket);
                    Console.WriteLine($"Пример: {builder}");

                    //отправка решения серверу
                    Console.Write("Введите ответ: ");
                    message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message))
                        message = "no answer";
                    Sending(socket, message);

                    //получение ответа (вердикта) от сервера
                    builder = Receiving(socket);
                    Console.WriteLine($"Ответ сервера: {builder}");

                    //если пример решен неверно, то
                    //запрос повторного ввода или вывода ответа
                    if (builder.ToString().ToLower() == "неверно")
                    {
                        Console.Write("Для повторного ввода нажмите enter, для ответа - ans: ");
                        string? input = Console.ReadLine();
                        if (input == "ans")
                        {
                            //отправка запроса на получение правильного ответа
                            Sending(socket, input);
                            //получение правильного ответа
                            Console.WriteLine("Правильный ответ: {0}",
                                arg0: Receiving(socket).ToString());
                        }
                        else
                        {
                            Sending(socket, "repeat");
                        }
                    }

                    Console.WriteLine(new string('-', 20));
                } while (message != "end");

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
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
            int bytes = 0;
            byte[] data = new byte[256];
            do
            {
                bytes = socket.Receive(data);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            } while (socket.Available > 0);

            return builder;
        }

        /// <summary>
        /// Отправка сообщения на сервер
        /// </summary>
        /// <param name="socket">сокет</param>
        /// <param name="message">сообщение</param>
        static void Sending(Socket socket, string message)
        {
            byte[] data;
            data = Encoding.Unicode.GetBytes(message);
            socket.Send(data);
        }
    }
}