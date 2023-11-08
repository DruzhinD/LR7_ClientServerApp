using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

//реализовать возможность чтения клиентом ответа с сервера, в случае неверного решения
//т.е. в случае ответа от сервера NO + число, клиент должен предложить пользователю
//попробовать снова или вывести правильный ответ
//а после всех манипуляций вернуться к работе с сервером
//схема находится в лабораторной работе
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

                    //сохраняем текст примера, чтобы в случае неверного решения его можно было использовать
                    string task = builder.ToString();
                    Console.WriteLine($"Пример: {builder}");

                    //отправка решения серверу
                    Console.Write("Введите ответ: ");
                    message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message))
                        message = "no answer";
                    Sending(socket, message);

                    //получение ответа (вердикта) от сервера
                    builder = Receiving(socket);

                    //прерывание цикла при вводе стоп-слова
                    //таким образом цикл никогда не сможет полностью выполниться при
                    //вводе end
                    if (message == "end")
                        break;

                    if (builder.ToString().ToLower() == "yes")
                    {
                        Console.WriteLine("Решено верно.");
                    }
                    else if (builder.ToString().ToLower().StartsWith("no"))
                    {
                        Console.WriteLine("Решено неверно.");
                        //сохраняем в переменную число, отправленное сервером
                        int answer = int.Parse(builder.ToString().Split(' ')[1]);
                        //предложение пользователю решить пример снова
                        TryAnswerAgain(task, answer);
                    }

                    Console.WriteLine(new string('-', 20));
                } while (message != "end");

                Console.WriteLine("Завершение работы...");
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

        /// <summary>
        /// метод с циклом, в котором пользователю предлагается решить пример заново
        /// </summary>
        /// <param name="task">Текст примера</param>
        /// <param name="answer">Решение примера</param>
        private static void TryAnswerAgain(string task, int answer)
        {
            bool stopFlag = true;
            do
            {
                Console.WriteLine("Чтобы попытаться снова нажмите <Enter>, \n" +
                    "Чтобы узнать ответ - любую другую клавишу.");
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    //повторный вывод примера
                    Console.Write($"{task} = ");
                    if (int.TryParse(Console.ReadLine(), out int userAns))
                    {
                        if (answer == userAns)
                        {
                            Console.WriteLine("Правильный ответ!");
                            stopFlag = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Некорректный ввод!");
                    }
                }
                else
                {
                    stopFlag = false;
                    Console.WriteLine($"Ответ: {answer}");
                }
            } while (stopFlag);
        }
    }
}