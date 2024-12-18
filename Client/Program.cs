﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    internal class Program
    {
        static IPAddress Ip;
        static int Port;
        static string ClientToken;
        static DateTime ClientDateConnection;
        static bool IsConnected = false; 
        static bool IsBlacklisted = false; 
        static bool ShouldRun = true; 
        static void Main(string[] args)
        {
            OnSetings();
            Thread tCheckToken = new Thread(CheckToken);
            tCheckToken.Start();
            while (ShouldRun) 
            {
                SetCommand();
            }
        }
        static void OnSetings()
        {
            string path = Directory.GetCurrentDirectory() + "/.config";
            string ipAddress = "";
            ClientToken = string.Empty;
            ClientDateConnection = DateTime.MinValue;
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
            if (IsBlacklisted) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Вы заблокированы на сервере.");
                return;
            }
            if (IsConnected) 
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Вы уже подключены к серверу.");
                return;
            }
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
                return;
            }
            if (socket.Connected)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Успешное подключение к серверу");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Введите логин: ");
                string login = Console.ReadLine();
                Console.Write("Введите пароль: ");
                string password = Console.ReadLine();
                string authMessage = $"/auth {login} {password}";
                socket.Send(Encoding.UTF8.GetBytes(authMessage));
                byte[] bytes = new byte[10485760];
                int byteRec = socket.Receive(bytes);
                string response = Encoding.UTF8.GetString(bytes, 0, byteRec);
                if (response == "/limit")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("На сервере нет свободных мест");
                }
                else if (response == "/blacklist")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ваш токен заблокирован на сервере.");
                    IsBlacklisted = true; 
                    ShouldRun = false;
                }
                else if (response == "/auth_error")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка аутентификации.");
                }
                else
                {
                    ClientToken = response;
                    ClientDateConnection = DateTime.Now;
                    IsConnected = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Токен подключения: " + ClientToken);
                }
            }
        }
        static void GetStatus()
        {
            if (IsBlacklisted) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Вы заблокированы на сервере.");
                return;
            }
            if (!string.IsNullOrEmpty(ClientToken))
            {
                string response = SendCommandToServer(ClientToken);
                if (response == "/disconnect")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Клиент был отключен от сервера.");
                    ClientToken = string.Empty;
                    IsConnected = false;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Ответ сервера: " + response);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Вы не подключены к серверу.");
            }
        }
        static string SendCommandToServer(string command)
        {
            if (IsBlacklisted) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Вы заблокированы на сервере.");
                return string.Empty; 
            }
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
                return string.Empty;
            }
            if (socket.Connected)
            {
                socket.Send(Encoding.UTF8.GetBytes(command));
                byte[] bytes = new byte[10485760];
                int byteRec = socket.Receive(bytes);
                string response = Encoding.UTF8.GetString(bytes, 0, byteRec);
                if (response == "/blacklist")
                {
                    IsBlacklisted = true; 
                    IsConnected = false; 
                }
                else if (response == "/disconnect")
                {
                    ClientToken = string.Empty;
                    IsConnected = false; 
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Ответ сервера: " + response);
                }
                return response;
            }
            return string.Empty; 
        }
        static void CheckToken()
        {
            while (ShouldRun) 
            {
                if (!string.IsNullOrEmpty(ClientToken) && !IsBlacklisted) 
                {
                    int duration = (int)DateTime.Now.Subtract(ClientDateConnection).TotalSeconds;
                    if (duration > 60) 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Время подключения истекло. Отключение от сервера.");
                        ClientToken = string.Empty;
                        IsConnected = false; 
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