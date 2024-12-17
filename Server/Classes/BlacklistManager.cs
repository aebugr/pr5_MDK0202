using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Classes
{
    public class BlacklistManager
    {
        private readonly DatabaseManager _dbManager;
        public BlacklistManager(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }
        public bool IsBlacklisted(string token)
        {
            return _dbManager.IsTokenBlacklisted(token);
        }
        public void AddToBlacklist(string token)
        {
            _dbManager.AddToBlacklist(token);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Токен {token} добавлен в чёрный список.");
        }
    }
}

