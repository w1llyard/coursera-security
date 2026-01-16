using Microsoft.Extensions.Hosting;

namespace SafeVault.Web.Services
{
    public static class DatabaseSeeder
    {
        public static void SeedTestUsers(LoginService loginService, IHostEnvironment env)
        {
            // 🚨 Never seed production
            if (!env.IsDevelopment())
                return;

            // Admin
            loginService.RegisterUser(
                username: "admin",
                password: "Admin123!",
                role: "Admin"
            );

            // Normal user
            loginService.RegisterUser(
                username: "user",
                password: "User123!",
                role: "User"
            );

        }
    }
}
