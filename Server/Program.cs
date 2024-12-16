using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        static IPAddress Ip;
        static int Port;
        static int MaxClient;
        static int Duration;
        // Список всех подключенных клиентов (с хранением информации о каждом клиенте)
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
                Console.Write("Укажите время жизни токена (в секундах): ");
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
    }
}
