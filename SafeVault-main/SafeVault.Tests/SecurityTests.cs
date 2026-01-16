

namespace SafeVault.Tests
{
    [TestFixture]
    public class AuthTests
    {
        private LoginService _loginService;

        [SetUp]
        public void Setup()
        {
            _loginService = new LoginService();
        }

        [Test]
        public void RegisterAndAuthenticateUser_ShouldSucceed()
        {
            string username = "testuser";
            string password = "Test123!";
            _loginService.RegisterUser(username, password);

            bool auth = _loginService.AuthenticateUser(username, password, out string role);
            Assert.That(auth, Is.True);
            Assert.That(role, Is.EqualTo("User"));
        }

        [Test]
        public void AdminAuthorization_ShouldFailForNonAdmin()
        {
            string username = "normaluser";
            string password = "Pass123!";
            _loginService.RegisterUser(username, password, "User");

            bool auth = _loginService.AuthenticateUser(username, password, out string role);
            Assert.That(auth, Is.True);
            Assert.That(role, Is.EqualTo("User"));
            Assert.That(role, Is.Not.EqualTo("Admin"));
        }

        [Test]
        public void SqlInjection_ShouldFail()
        {
            string maliciousInput = "'; DROP TABLE Users; --";
            Assert.That(_loginService.AuthenticateUser(maliciousInput, "anyPassword", out _), Is.False);
        }

        [Test]
        public void XssInput_ShouldBeRejected()
        {
            string maliciousInput = "<script>alert('XSS')</script>";
            Assert.That(ValidationHelpers.IsValidInput(maliciousInput), Is.False);
        }
    }

        public class LoginService
    {
        private readonly Dictionary<string, (string PasswordHash, string Role)> _users 
            = new Dictionary<string, (string, string)>();

        // Register user
        public bool RegisterUser(string username, string password, string role = "User")
        {
            if (!ValidationHelpers.IsValidInput(username) ||
                !ValidationHelpers.IsValidInput(password))
                return false;

            if (_users.ContainsKey(username))
                return false;

            string hashed = BCrypt.Net.BCrypt.HashPassword(password);
            _users[username] = (hashed, role);
            return true;
        }

        // Authenticate user
        public bool AuthenticateUser(string username, string password, out string role)
        {
            if (!_users.TryGetValue(username, out var data))
            {
                role = "None";
                return false;
            }

            role = data.Role;
            return BCrypt.Net.BCrypt.Verify(password, data.PasswordHash);
        }
    }
    public static class ValidationHelpers
    {
        public static bool IsValidInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Basic XSS / SQL injection checks
            if (input.Contains("<script>") || input.Contains("'") || input.Contains("--"))
                return false;

            return true;
        }
    }

}
