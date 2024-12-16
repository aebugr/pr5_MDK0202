using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            OnSetings();
            Thread tCheckToken = new Thread(CheckToken);
            tCheckToken.Start();
            while (true)
            {
                SetCommand();
            }
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
        static void GetStatus()
        {
            int Duration = (int)DateTime.Now.Subtract(ClientDateConnection).TotalSeconds;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Клиент: {ClientToken}, время подключения: {ClientDateConnection:HH:mm:ss dd.MM}, продолжительность: {Duration} секунд");
        }
        static void CheckToken()
        {
            while (true)
            {
                if (!string.IsNullOrEmpty(ClientToken))
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
                        socket.Send(Encoding.UTF8.GetBytes(ClientToken));
                        byte[] bytes = new byte[10485760];
                        int byteRec = socket.Receive(bytes);
                        string response = Encoding.UTF8.GetString(bytes, 0, byteRec);
                        if (response == "/disconnect")
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Клиент был отключен от сервера");
                            ClientToken = string.Empty;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Доступные команды: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/config");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" : настроить начальные параметры");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/connect");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" : подключиться к серверу");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("/status");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" : показать данные клиента");
        }
        static void SetCommand()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string command = Console.ReadLine();
            if (command == "/config")
            {
                File.Delete(Directory.GetCurrentDirectory() + "/.config");
                OnSetings();
            }
            else if (command == "/connect")
            {
                Connect();
            }
            else if (command == "/status")
            {
                GetStatus();
            }
            else if (command == "/help")
            {
                Help();
            }
        }

    }
}
