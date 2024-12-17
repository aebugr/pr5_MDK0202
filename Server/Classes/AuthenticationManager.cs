using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Classes
{
    public class AuthenticationManager
    {
        private readonly DatabaseManager _dbManager;
        public AuthenticationManager(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }
        public bool Authenticate(string login, string password)
        {
            bool isAuthenticated = _dbManager.AuthenticateUser(login, password);
            if (!isAuthenticated)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка аутентификации: неверный логин или пароль.");
            }
            return isAuthenticated;
        }
    }
}
