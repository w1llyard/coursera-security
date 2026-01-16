# SafeVault

SafeVault is a secure web application for managing user authentication and role-based access control (RBAC). This project demonstrates secure coding practices including password hashing, input validation, SQL injection prevention, and session management using **ASP.NET Core 10** and **SQLite**.

---

## Features

### User Registration & Login
- Secure password hashing with BCrypt
- Input validation to prevent SQL injection and XSS
- Role assignment (Admin or User)

### Role-Based Access Control (RBAC)
- Admin users can access protected routes like the Admin Dashboard
- Users without admin privileges are redirected away from admin routes

### Session Management
- Stores user session information securely
- Session expiration and logout support

### Database
- Uses SQLite for local development and testing
- Automatic table creation and optional seeding of test users in development mode

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- SQLite (local database included via connection string)
- Visual Studio 2022/2023 or VS Code (optional)

### Setup

1. Clone the repository:

```bash
git clone https://github.com/msasa-mengtingchung/SafeVault.git
cd SafeVault
```


2. Update the SQLite connection string in appsettings.json if necessary:

```
"ConnectionStrings": {
  "Sqlite": "Data Source=SafeVault.db"
}
```

3. Run the application:
```dotnet run --project SafeVault.Web```

4. Open in browser:
```https://localhost:5001```

---

### Seeded Users (Development Only
| Username | Password | Role |
| -------- | -------- | -------- |
| admin     | Admin123!     | Admin     |
| user     | User123!     | User     |

These accounts are automatically seeded in development mode using DatabaseSeeder.

---

### Security Highlights
* Passwords are hashed using BCrypt before storing
* Input validation prevents SQL injection and XSS attacks
* Admin routes are protected using `[Authorize(Roles="Admin")]`
* Session cookies are HttpOnly and Secure
* No secrets or connection strings are pushed to GitHub
* 
---

### Project Structure
```
Controllers/    – MVC controllers for pages and account management
Services/       – LoginService for authentication and role management, DatabaseSeeder for development seeding
Views/          – Razor pages
wwwroot/        – Static files (CSS, JS)
appsettings.json – Configuration including SQLite connection string
```

---

### Getting Reviewed
To simplify peer review:
1. SQLite database will auto-create tables on first run
2. Development mode automatically seeds the admin and user accounts
3. Run the app locally and login with:
```
Admin → Username: admin | Password: Admin123!
User  → Username: user  | Password: User123!
```
4. Admin Dashboard and RBAC routes can now be tested without connecting to Azure SQL
---

---
### License
This project is for educational purposes only.