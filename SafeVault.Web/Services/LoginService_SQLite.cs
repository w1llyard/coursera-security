using Microsoft.Data.Sqlite;
using SafeVault.Web.Helpers;

namespace SafeVault.Web.Services
{
    public class LoginService
    {
        private readonly string _connectionString;

        public LoginService(string connectionString)
        {
            _connectionString = connectionString;
            EnsureDatabase();
        }

        // -------------------------------
        // CREATE TABLE IF NOT EXISTS
        // -------------------------------
        private void EnsureDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL
                );";
            cmd.ExecuteNonQuery();
        }

        // -------------------------------
        // REGISTER USER
        // -------------------------------
        public bool RegisterUser(string username, string password, string role = "User")
        {
            if (!ValidationHelpers.IsValidInput(username) ||
                !ValidationHelpers.IsValidInput(password))
                return false;

            if (UserExists(username))
                return false;

            string hashed = BCrypt.Net.BCrypt.HashPassword(password);

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
         
            cmd.CommandText = @"
                INSERT INTO Users (Username, PasswordHash, Role)
                VALUES (@u, @p, @r)";
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", hashed);
            cmd.Parameters.AddWithValue("@r", role);

            return cmd.ExecuteNonQuery() > 0;
        }

        // -------------------------------
        // CHECK USER
        // -------------------------------
        private bool UserExists(string username)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = @u";
            cmd.Parameters.AddWithValue("@u", username);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        // -------------------------------
        // AUTHENTICATE USER
        // -------------------------------
        public bool AuthenticateUser(string username, string password, out string role)
        {
            role = "None";

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT PasswordHash, Role
                FROM Users
                WHERE Username = @u";
            cmd.Parameters.AddWithValue("@u", username);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return false;

            var hash = reader.GetString(0);
            role = reader.GetString(1);

            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
