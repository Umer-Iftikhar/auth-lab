# AuthLab

A hands-on JWT authentication API built with ASP.NET Core 8 and MySQL. Built to deeply understand JWT authentication internals — stateless auth, refresh token rotation, role-based authorization, and authorization policies.

> Built alongside [IndieVault](https://github.com/Umer-Iftikhar/indie-vault) as a focused deep-dive into JWT after completing a full MVC capstone.

## Screenshots

### Swagger UI
![Swagger UI](.docs/images/swagger-ui.png)

### Login Response
![Login Response](.docs/images/login-response.png)

### Protected Endpoint
![Protected Endpoint](.docs/images/protected-endpoint.png)

## What's Built

- User registration with ASP.NET Core Identity
- Login returning signed JWT access token + refresh token
- Refresh token rotation — old token revoked on every refresh
- Logout — invalidates refresh token in database
- Role-based authorization (`Admin`, `User`)
- Authorization policies (`AdminOnly`, `AdminOrUser`)
- Protected endpoints secured with JWT Bearer authentication
- Minimal API endpoints alongside controllers
- Global exception handling middleware
- Strongly typed JWT configuration using `IOptions<T>`
- Admin user and roles seeded automatically in development

## Key Concepts Demonstrated

- Stateless vs stateful authentication
- JWT structure: header, payload, signature
- Why short-lived access tokens + long-lived refresh tokens
- `ClaimTypes` — strongly typed claim keys
- `DefaultAuthenticateScheme` vs `DefaultChallengeScheme`
- `401 Unauthorized` vs `403 Forbidden`
- Authorization policies vs role strings on attributes
- Refresh token rotation — why and how
- Minimal APIs vs controller-based endpoints
- Middleware pipeline and ordering

## Tech Stack

- ASP.NET Core 8 Web API
- ASP.NET Core Identity
- Entity Framework Core 8
- MySQL 8+
- Pomelo EF Core MySQL Provider
- JWT Bearer Authentication
- Swagger / Swashbuckle

## Project Structure

```
AuthLab/
├── Controllers/
│   ├── AccountController.cs     # Register, login, refresh, logout
│   └── TestController.cs        # Public, private, me, admin, dashboard endpoints
├── Data/
│   ├── AppDbContext.cs
│   ├── DatabaseSeeder.cs        # Seeds roles and admin user
│   └── Migrations/
├── DTOs/
│   ├── RegisterDto.cs
│   ├── LoginDto.cs
│   ├── RefreshRequestDto.cs
│   └── AuthResponseDto.cs       # Access token + refresh token + expiry
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Models/
│   ├── ApplicationUser.cs
│   └── RefreshToken.cs
├── Services/
│   ├── Interfaces/
│   │   ├── ITokenService.cs
│   │   └── IRefreshTokenService.cs
│   └── Implementations/
│       ├── TokenService.cs
│       └── RefreshTokenService.cs
├── Settings/
│   └── JwtConfig.cs
├── appsettings.json
└── Program.cs
```

## Setup

### Prerequisites

- .NET 8 SDK
- MySQL 8+

### Steps

1. Clone the repository
```bash
git clone https://github.com/Umer-Iftikhar/auth-lab.git
cd auth-lab
```

2. Configure user secrets
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;database=auth-lab;user=root;password=YOUR_PASSWORD"
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-minimum-32-characters"
```

3. Add JWT settings to `appsettings.json`
```json
"JwtSettings": {
  "Issuer": "auth-lab",
  "Audience": "auth-lab-users",
  "ExpiryMinutes": 15
}
```

4. Run migrations
```bash
dotnet ef database update
```

5. Run the application
```bash
dotnet run
```

6. Open Swagger at `https://localhost:{port}/swagger`

## Seeded Admin Account

Development environment automatically seeds an admin account:

- Email: `admin123@gmail.com`
- Password: `Password123!`

> Development only.

## API Endpoints

### Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/account/register` | No | Register new user |
| POST | `/api/account/login` | No | Login, returns access + refresh token |
| POST | `/api/account/refresh` | No | Exchange refresh token for new access token |
| POST | `/api/account/logout` | No | Invalidate refresh token |

### Test Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/test/public` | No | Public endpoint |
| GET | `/api/test/private` | Bearer | Any authenticated user |
| GET | `/api/test/me` | Bearer | Returns claims from token, no DB call |
| GET | `/api/test/admin` | Bearer + Admin | Admin only |
| GET | `/api/test/dashboard` | Bearer + Any Role | Admin or User |

### Minimal API

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/minimal/hello` | No | Public minimal API endpoint |
| GET | `/api/minimal/protected` | Bearer | Protected minimal API endpoint |

## Token Lifecycle

```
Login → Access Token (15 min) + Refresh Token (7 days)
         ↓
Access token expires
         ↓
POST /api/account/refresh → New Access Token + New Refresh Token
(old refresh token revoked)
         ↓
Logout → Refresh Token revoked in DB
```

## Architecture Decisions

- `AddIdentityCore` over `AddIdentity` — no cookie auth, no redirect behavior
- `DefaultChallengeScheme = JWT` — returns 401 instead of redirecting to login page
- Refresh token rotation — old token revoked on every refresh, limits stolen token window
- `IOptions<T>` for JWT config — strongly typed, injected via DI
- Cryptographically random refresh tokens via `RandomNumberGenerator`
- Services return DTOs — controllers stay thin