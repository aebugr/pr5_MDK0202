using Server.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static IPAddress Ip;
        static int Port;
        static int MaxClient;
        static int Duration;

        static List<string> ClientTokens = new List<string>();
        static List<DateTime> ClientConnections = new List<DateTime>();

        static DatabaseManager DbManager;
        static BlacklistManager BlacklistManager;
        static Classes.AuthenticationManager AuthManager;

        static void Main(string[] args)
        {
            ClientTokens.Clear();
            ClientConnections.Clear();
            OnSetings();

            DbManager = new DatabaseManager("Server=127.0.0.1;Database=PR5;port=3306;Uid=root;Pwd=;");
            BlacklistManager = new BlacklistManager(DbManager);
            AuthManager = new Classes.AuthenticationManager(DbManager);

            Thread tListener = new Thread(Connect);
            tListener.Start();
            Thread tDisconnect = new Thread(CheckDisconnectClient);
            tDisconnect.Start();
            while (true)
            {
                SetCommand();
            }
        }
        static void OnSetings()
        {
            string Path = Directory.GetCurrentDirectory() + "/.config";
            string IpAddress = "";
            if (File.Exists(Path))
            {
                StreamReader streamReader = new StreamReader(Path);
                IpAddress = streamReader.ReadLine();
                Ip = IPAddress.Parse(IpAddress);
                Port = int.Parse(streamReader.ReadLine());
                MaxClient = int.Parse(streamReader.ReadLine());
                Duration = int.Parse(streamReader.ReadLine());
                streamReader.Close();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Адрес сервера: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(IpAddress);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Порт : ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Port.ToString());
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Максимальное количество клиентов: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(MaxClient.ToString());
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Время использования токена (в секундах): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Duration.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Укажите IP-адрес сервера: ");
                Console.ForegroundColor = ConsoleColor.Green;
                IpAddress = Console.ReadLine();
                Ip = IPAddress.Parse(IpAddress);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Укажите порт сервера: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Port = int.Parse(Console.ReadLine());
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Укажите максимальное количество клиентов: ");
                Console.ForegroundColor = ConsoleColor.Green;
                MaxClient = int.Parse(Console.ReadLine());
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Укажите время использования токена (в секундах): ");
                Console.ForegroundColor = ConsoleColor.Green;
                Duration = int.Parse(Console.ReadLine());

                StreamWriter streamWriter = new StreamWriter(Path);
                streamWriter.WriteLine(IpAddress);
                streamWriter.WriteLine(Port.ToString());
                streamWriter.WriteLine(MaxClient.ToString());
                streamWriter.WriteLine(Duration.ToString());
                streamWriter.Close();
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Чтобы изменить настройки, используйте команду: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/config");
        }
        static void Connect()
        {
            IPEndPoint endPoint = new IPEndPoint(Ip, Port);
            Socket SocketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketListener.Bind(endPoint);
            SocketListener.Listen(10);
            while (true)
            {
                Socket Handler = SocketListener.Accept();
                byte[] Bytes = new byte[10485760];
                int ByteRec = Handler.Receive(Bytes);
                string Message = Encoding.UTF8.GetString(Bytes, 0, ByteRec);
                string Response = SetCommandClient(Message);
                Handler.Send(Encoding.UTF8.GetBytes(Response));
                Handler.Shutdown(SocketShutdown.Both);
                Handler.Close();
            }
        }
        static void Disconnect(string command)
        {
            try
            {
                string Token = command.Replace("/disconnect ", "");
                int index = ClientTokens.IndexOf(Token);
                if (index != -1)
                {
                    ClientTokens.RemoveAt(index);
                    ClientConnections.RemoveAt(index);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Клиент с токеном {Token} был отключен");
                }
            }
            catch (Exception exp)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + exp.Message);
            }
        }
        static void SetCommand()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            string Command = Console.ReadLine();
            if (Command.Contains("/config"))
            {
                File.Delete(Directory.GetCurrentDirectory() + "/.config");
                OnSetings();
            }
            else if (Command.Contains("/disconnect"))
            {
                Disconnect(Command);
            }
            else if (Command == "/status")
            {
                GetStatus();
            }
            else if (Command.StartsWith("/blacklist"))
            {
                string Token = Command.Replace("/blacklist ", "");
                BlacklistManager.AddToBlacklist(Token);
            }
            else if (Command == "/help")
            {
                Help();
            }
        }
        static string SetCommandClient(string Command)
        {
            if (BlacklistManager.IsBlacklisted(Command))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Клиент с токеном {Command} заблокирован.");
                return "/blacklist";
            }

            if (Command.StartsWith("/auth"))
            {
                string[] parts = Command.Split(' ');
                string login = parts[1];
                string password = parts[2];

                if (AuthManager.Authenticate(login, password))
                {
                    if (ClientTokens.Count < MaxClient)
                    {
                        string newToken = GenerateToken();
                        ClientTokens.Add(newToken);
                        ClientConnections.Add(DateTime.Now);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Новый клиент подключен: {newToken}");
                        return newToken;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Нет места для нового клиента на сервере");
                        return "/limit";
                    }
                }
                else
                {
                    return "/auth_error";
                }
            }

            int index = ClientTokens.IndexOf(Command);
            if (index != -1)
            {
                int duration = (int)DateTime.Now.Subtract(ClientConnections[index]).TotalSeconds;
                return $"Клиент: {ClientTokens[index]}, время подключения: {ClientConnections[index]:HH:mm:ss dd.MM}, продолжительность: {duration} секунд";
            }
            else
            {
                return "/disconnect";
            }
        }
        static string GenerateToken()
        {
            Random random = new Random();
            string Chars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm123456789";
            return new string(Enumerable.Repeat(Chars, 9).Select(x => x[random.Next(Chars.Length)]).ToArray());
        }
        static void CheckDisconnectClient()
        {
            while (true)
            {
                for (int iClient = 0; iClient < ClientTokens.Count; iClient++)
                {
                    int ClientDuration = (int)DateTime.Now.Subtract(ClientConnections[iClient]).TotalSeconds;

                    if (ClientDuration > Duration)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Клиент {ClientTokens[iClient]} отключен из-за превышения времени подключения");
                        ClientTokens.RemoveAt(iClient);
                        ClientConnections.RemoveAt(iClient);
                    }
                }
                Thread.Sleep(1000);
            }
        }
        static void GetStatus()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Количество подключенных клиентов: {ClientTokens.Count}");
            for (int i = 0; i < ClientTokens.Count; i++)
            {
                int ClientDuration = (int)DateTime.Now.Subtract(ClientConnections[i]).TotalSeconds;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Клиент: {ClientTokens[i]}, время подключения: {ClientConnections[i].ToString("HH:mm:ss dd.MM")}, " +
                    $"длительность подключения: {ClientDuration} секунд");
            }
        }
        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Доступные команды: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/config");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" : изменить настройки сервера");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/disconnect");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" : отключить клиента от сервера");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/status");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" : показать статус подключений");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/blacklist");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" : добавить клиента в ЧС");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/help");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" : получить справку по командам");
        }
    }
}