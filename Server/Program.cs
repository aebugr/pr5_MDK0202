using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        static void Main(string[] args)
        {

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
        static string SetCommandClient(string Command)
        {
            if (Command == "/token")
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
                int index = ClientTokens.IndexOf(Command);
                return index != -1 ? "/connect" : "/disconnect";
            }
        }
        static string GenerateToken()
        {
            Random random = new Random();
            string Chars = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm123456789";
            return new string(Enumerable.Repeat(Chars, 15).Select(x => x[random.Next(Chars.Length)]).ToArray());
        }
    }
}
