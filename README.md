# Rosary Communities — ASP.NET Core Web API

REST API for a multi-platform application supporting parish Rosary communities. The backend handles user accounts, Rosary groups, parish administration, meditation content, messages, and permissions.

**Related project:** [.NET MAUI mobile application](https://github.com/D0MINoX/aplikacja_roza)

> **Portfolio project.** The source code demonstrates backend development with C#, ASP.NET Core, Entity Framework Core, and relational databases. A reproducible database schema and additional security hardening are needed before treating this repository as a standalone production deployment.

## Technology stack

| Area | Technologies |
| --- | --- |
| Language and runtime | C#, .NET 10 |
| Backend | ASP.NET Core Web API, controllers, dependency injection |
| Database | MySQL, Entity Framework Core, Pomelo MySQL provider, LINQ |
| Authentication | JWT Bearer, BCrypt password hashing |
| Authorization | Role-based authorization with ASP.NET Core attributes |
| Documentation | Swagger / OpenAPI (Development environment) |

## Main features

- **User accounts:** registration, login, profile editing, password changes, and account deletion.
- **Authentication and roles:** JWT tokens and protected endpoints with role-specific permissions.
- **Rosary groups and parishes:** browsing groups, joining a group, managing memberships and parishes.
- **Messaging:** creating and retrieving group messages, as well as managing external contacts for the client app's SMS workflow.
- **Meditations:** searching meditation content and recording completed prayers.
- **Administration:** user verification, permissions management, and external-member management.
- **Error reports:** submitting reports, viewing reports, and updating their status.

The **client application** implements the mobile user interface and uses this API over HTTP/JSON. SMS composition is performed by the client application; the backend manages messages and contact data rather than sending SMS directly.

## Architecture

```text
.NET MAUI client
      |
      | HTTPS / JSON
      v
ASP.NET Core Web API
  |       |        |
  |       |        +-- JWT authentication / role-based authorization
  |       +----------- Controllers (Auth, Admin, Messages, Rosaries, ...)
  +------------------- Dependency injection / ApplicationDbContext
                                  |
                           Entity Framework Core
                                  |
                                MySQL
```

The EF Core data model contains entities for users, groups, memberships, parishes, meditations, messages, external members, consents, and error reports. Relationships and delete behavior are configured in `ApplicationDbContext`.

## Selected API endpoints

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `POST` | `/api/Auth/register` | Create an account |
| `POST` | `/api/Auth/login` | Authenticate and return a JWT |
| `PUT` | `/api/Auth/changePassword` | Change an account password |
| `GET` | `/api/Parish/getAllParish` | List parishes |
| `GET` | `/api/Rosaries/available-rosaries/{parishId}` | Browse available groups |
| `POST` | `/api/Rosaries/JoinRosary` | Join a group |
| `GET` | `/api/Messages/getMessages/{rosaryId}` | Retrieve group messages |
| `POST` | `/api/Messages/newMessage` | Add a group message |
| `GET` | `/api/Meditations/search` | Search meditation content |
| `PUT` | `/api/Admin/UpdatePermissions` | Update a user's permissions |
| `POST` | `/api/Error/submit` | Submit an error report |

Most endpoints require authentication by default. Actions marked `[AllowAnonymous]` are publicly accessible, and administrative operations use role-based `[Authorize]` attributes.

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- MySQL server
- A database containing the tables and relations expected by `ApplicationDbContext`

**Database limitation:** this repository does not currently include a complete database creation script or committed EF Core migrations. Before running it against a new MySQL instance, you must prepare the schema separately. Merely creating an empty database is not sufficient.

### 1. Clone the repository

```bash
git clone https://github.com/D0MINoX/WebAPI.git
cd WebAPI
dotnet restore WebAPI/WebAPI.csproj
```

### 2. Configure local secrets

The application reads:

- `ConnectionStrings:DefaultConnection` — a MySQL connection string.
- `JwtSettings:SecretKey` — a sufficiently long, randomly generated JWT signing secret.

The tracked `appsettings.json` file has been removed. You can supply these settings through environment variables, with `__` representing a configuration section separator.

**Windows PowerShell** (for the current terminal session):

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Port=3306;Database=rosary_db;User=YOUR_USER;Password=YOUR_PASSWORD;"
$env:JwtSettings__SecretKey = "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_BYTES"
```

**Linux / macOS (Bash):**

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=rosary_db;User=YOUR_USER;Password=YOUR_PASSWORD;"
export JwtSettings__SecretKey="REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_BYTES"
```

These are **examples**, not working credentials. Replace every placeholder with your own local values. Never commit real database credentials or JWT secrets.

### 3. Run the API

After the MySQL schema and configuration are ready:

```bash
dotnet run --project WebAPI/WebAPI.csproj --launch-profile https
```

The checked-in launch profile uses `https://localhost:7206` and `http://localhost:5049`. In the Development environment, Swagger UI is available at:

```text
https://localhost:7206/swagger
```

If necessary, trust the local ASP.NET Core development HTTPS certificate using `dotnet dev-certs https --trust`.

### 4. Example login request

```http
POST https://localhost:7206/api/Auth/login
Content-Type: application/json

{
  "username": "example@example.com",
  "password": "YOUR_PASSWORD"
}
```

An existing user and a correctly prepared database are required. The login endpoint returns a token which can be sent to protected endpoints in the `Authorization: Bearer <token>` header.

## Repository structure

```text
WebAPI/
├── WebAPI.slnx
├── README.md
└── WebAPI/
    ├── Program.cs
    ├── WebAPI.csproj
    ├── Data/
    │   └── ApplicationDbContext.cs
    ├── Models/
    ├── AuthController.cs
    ├── AdminController.cs
    ├── RosariesController.cs
    ├── ParishController.cs
    ├── MessagesController.cs
    ├── MeditationsController.cs
    └── ErrorController.cs
```

## Possible improvements

- Add EF Core migrations, database initialization instructions, and non-sensitive sample data.
- Add unit and integration tests (especially for authentication, permissions, and group management).
- Restrict CORS and enforce HTTPS in production deployments.
- Configure and validate JWT issuer/audience and review token expiration.
- Add automated backend build and test checks in GitHub Actions.
- Provide a repeatable demo environment (for example, a local MySQL container).

## Security notice

This repository previously included application configuration. If real secrets were ever pushed to GitHub, deleting `appsettings.json` from the latest commit does **not** remove them from Git history. Revoke or rotate the exposed secrets and, where appropriate, clean the repository history. Use environment variables or local secret storage for configuration going forward.

---

**Related client repository:** [D0MINoX/aplikacja_roza](https://github.com/D0MINoX/aplikacja_roza)
