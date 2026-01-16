using Microsoft.AspNetCore.Authentication.Cookies;
using SafeVault.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------
// Add MVC controllers with views
// ------------------------------
builder.Services.AddControllersWithViews();

// ------------------------------
// Configure database & services
// ------------------------------
string connectionString = builder.Configuration.GetConnectionString("Sqlite")
    ?? throw new InvalidOperationException("SQLite connection string missing");

builder.Services.AddScoped<LoginService>(sp => new LoginService(connectionString));

// ------------------------------
// Configure Session (optional)
// ------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ------------------------------
// Configure Cookie Authentication (MVC)
// ------------------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;

        // Avoid HTTPS issues in development
        options.Cookie.SecurePolicy =
            builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
    });

// ------------------------------
// Configure Authorization (RBAC)
// ------------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ------------------------------
// Build the app
// ------------------------------
var app = builder.Build();

// ------------------------------
// Configure middleware
// ------------------------------
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Running in DEVELOPMENT mode");

    // Seed test users (DEV ONLY)
    using var scope = app.Services.CreateScope();
    var loginService = scope.ServiceProvider.GetRequiredService<LoginService>();

    DatabaseSeeder.SeedTestUsers(loginService, app.Environment);
}
else
{
    Console.WriteLine($"Running in {app.Environment.EnvironmentName} mode");

    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session BEFORE auth
app.UseSession();

// Authentication BEFORE authorization
app.UseAuthentication();
app.UseAuthorization();

// ------------------------------
// Configure routing
// ------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
