using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Classes
{
    public class DatabaseManager
    {
        private readonly string _connectionString;
        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }
        public bool AuthenticateUser(string login, string password)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE login = @login AND password = @password";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
        public bool IsTokenBlacklisted(string token)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM blacklist WHERE token = @token";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@token", token);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
        public void AddToBlacklist(string token)
        {
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO blacklist (token) VALUES (@token)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@token", token);
                command.ExecuteNonQuery();
            }
        }
    }
}
