using Microsoft.Data.SqlClient;
using SafeVault.Web.Helpers;

namespace SafeVault.Web.Services
{
    /// <summary>
    /// Handles user registration and authentication.
    /// Security considerations:
    /// - Input validation to prevent XSS and SQL injection
    /// - Parameterized SQL queries
    /// - BCrypt password hashing
    /// - Role-based authorization support
    /// </summary>
    public class LoginService_Azure
    {
        private readonly string _connectionString;

        public LoginService_Azure(string connectionString)
        {
            _connectionString = connectionString;
        }

        // -------------------------------
        // REGISTER USER
        // -------------------------------
        public bool RegisterUser(string username, string password, string role = "User")
        {
            // ✅ Input validation (Copilot-assisted)
            // Prevents XSS and SQL injection payloads
            if (!ValidationHelpers.IsValidInput(username) ||
                !ValidationHelpers.IsValidInput(password, "!@#$%^&*?"))
                return false;

            // ✅ Prevent duplicate users
            if (UserExists(username))
                return false;

            // ✅ Secure password hashing (BCrypt with salt)
            string hashed = BCrypt.Net.BCrypt.HashPassword(password);

            try
            {
                using var connection = new SqlConnection(_connectionString);

                // ✅ Parameterized query prevents SQL injection
                string query = @"
                    INSERT INTO Users (Username, PasswordHash, Role)
                    VALUES (@Username, @PasswordHash, @Role)";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@PasswordHash", hashed);
                command.Parameters.AddWithValue("@Role", role);

                connection.Open();
                int rows = command.ExecuteNonQuery();

                return rows > 0;
            }
            catch (SqlException ex)
            {
                // ✅ Handle unique constraint violation safely
                if (ex.Number == 2627) // duplicate username
                    return false;

                throw;
            }
        }

        // -------------------------------
        // CHECK IF USER EXISTS
        // -------------------------------
        private bool UserExists(string username)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ Parameterized query
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            connection.Open();
            int count = (int)command.ExecuteScalar();

            return count > 0;
        }

        // -------------------------------
        // AUTHENTICATE USER
        // -------------------------------
        public bool AuthenticateUser(string username, string password, out string role)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ Parameterized query prevents injection
            string query = @"
                SELECT PasswordHash, Role
                FROM Users
                WHERE Username = @Username";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            connection.Open();
            using var reader = command.ExecuteReader();

            // ❌ User not found
            if (!reader.Read())
            {
                role = "None";
                return false;
            }

            string storedHash = reader.GetString(0);
            role = reader.GetString(1);

            // ✅ Secure password verification
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
