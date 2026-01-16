namespace SafeVault.Web.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; } // Store hashed password
        public required string Role { get; set; }         // e.g., "Admin", "User"
    }
}