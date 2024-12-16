using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static IPAddress Ip;
        static int Port;
        static string ClientToken;
        static DateTime ClientDateConnection;
        static void Main(string[] args)
        {

        }
        static void OnSetings()
        {
            string path = Directory.GetCurrentDirectory() + "/.config";
            string ipAddress = "";

            if (File.Exists(path))
            {
                StreamReader streamReader = new StreamReader(path);
                ipAddress = streamReader.ReadLine();
                Ip = IPAddress.Parse(ipAddress);
                Port = int.Parse(streamReader.ReadLine());
                streamReader.Close();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Адрес сервера: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(ipAddress);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Порт сервера: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Port);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Введите IP-адрес сервера: ");
                Console.ForegroundColor = ConsoleColor.Green;
                ipAddress = Console.ReadLine();
                Ip = IPAddress.Parse(ipAddress);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Введите порт сервера: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Port = int.Parse(Console.ReadLine());

                StreamWriter streamWriter = new StreamWriter(path);
                streamWriter.WriteLine(ipAddress);
                streamWriter.WriteLine(Port);
                streamWriter.Close();
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Чтобы изменить настройки, введите команду: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("/config");
        }
        static void Connect()
        {
            IPEndPoint endPoint = new IPEndPoint(Ip, Port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(endPoint);
            }
            catch (Exception exp)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + exp.Message);
            }
            if (socket.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Успешное подключение к серверу");
                socket.Send(Encoding.UTF8.GetBytes("/token"));
                byte[] bytes = new byte[10485760];
                int byteRec = socket.Receive(bytes);
                string response = Encoding.UTF8.GetString(bytes, 0, byteRec);
                if (response == "/limit")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("На сервере нет свободных мест");
                }
                else
                {
                    ClientToken = response;
                    ClientDateConnection = DateTime.Now;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Токен подключения: " + ClientToken);
                }
            }
        }

    }
}
