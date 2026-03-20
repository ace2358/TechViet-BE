# TechViet-BE

Monolithic ASP.NET Core Web API for e-commerce MVP.

## Tech stack
- .NET 9 Web API
- Entity Framework Core + PostgreSQL
- JWT auth
- Cloudinary image upload
- VNPAY sandbox payment integration

## Project structure
- `techviet_be/` - API project
- `db-script.sql` - PostgreSQL schema script
- `docs/` - handoff and setup instructions

## Quick start (local)

### 1) Configure app settings
Copy and edit:
- `techviet_be/appsettings.Example.json` -> `techviet_be/appsettings.Development.json`

Update at least:
- `ConnectionStrings:DefaultConnection`
- `Jwt:Secret`
- `Cloudinary` keys
- `VnPay` config

### 2) Setup database
Follow:
- `docs/SETUP_DATABASE.md`

### 3) Run API
From repository root:

```powershell
dotnet run --project .\techviet_be\techviet_be.csproj
```

Or inside project folder:

```powershell
cd .\techviet_be
dotnet run
```

Swagger:
- `http://127.0.0.1:53011/`

## Docker option

### Build and run with Docker Compose
From repository root:

```powershell
docker compose up --build
```

Services:
- API: `http://localhost:53011`
- PostgreSQL: `localhost:5432`

Compose file:
- `docker-compose.yml`

Dockerfile:
- `Dockerfile`

## API usage/demo
- Full API request flow:
  - `techviet_be/techviet_be.http`
- Frontend handoff guide:
  - `docs/API_DEMO_GUIDE.md`

## Notes for frontend teammate
- Must set own connection string and secrets in local config.
- Use JWT access token in `Authorization: Bearer <token>`.
