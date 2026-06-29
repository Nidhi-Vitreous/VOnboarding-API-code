# Vitreous Onboarding — Backend

.NET **10** Clean Architecture Web API.

## Prerequisites

- [.NET SDK 10.0.102](https://dotnet.microsoft.com/download) (see `global.json`)
- PostgreSQL **16**

## First-time setup

1. Clone the repository.
2. Copy the local development config template:

   ```bash
   cp src/Api/appsettings.Development.example.json src/Api/appsettings.Development.json
   ```

   On Windows (PowerShell):

   ```powershell
   Copy-Item src/Api/appsettings.Development.example.json src/Api/appsettings.Development.json
   ```

3. Edit `src/Api/appsettings.Development.json` and set:
   - `ConnectionStrings:DefaultConnection` — your PostgreSQL password
   - `Jwt:Secret` — at least 32 characters
   - `Cors:AllowedOrigins` — usually `http://localhost:4200` for local Angular

   Alternatively use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables (see `.env.example`).

4. Create the database if it does not exist:

   ```sql
   CREATE DATABASE vonboarding_db;
   ```

## Run locally

```bash
dotnet restore
dotnet build
dotnet run --project src/Api/Vitreous.Onboarding.Api.csproj --launch-profile http
```

- Health: `http://localhost:5000/api/v1/health`
- Swagger (Development only): `http://localhost:5000/swagger`

When `Seed:Enabled` is `true` in Development, a default admin user is created on first run (see `appsettings.Development.example.json`). **Seeding is disabled in Production.**

## Configuration

| File | Purpose |
|------|---------|
| `src/Api/appsettings.json` | Base structure — **no secrets** |
| `src/Api/appsettings.Development.json` | Local overrides (**gitignored**) |
| `src/Api/appsettings.Development.example.json` | Template for new developers |
| `src/Api/appsettings.Production.json` | Production structure — values from env vars / Key Vault |
| `appsettings.example.json` | Reference at repo root |
| `.env.example` | Environment variable names |

### Production

Set at deploy time (never commit real values):

- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `Cors__AllowedOrigins__0` (your frontend URL)
- `ASPNETCORE_ENVIRONMENT=Production`

## Solution layout

| Project | Path | Layer |
|---------|------|-------|
| Vitreous.Onboarding.Api | `src/Api/` | HTTP, middleware, DI |
| Vitreous.Onboarding.Application | `src/Application/` | Use cases |
| Vitreous.Onboarding.Domain | `src/Domain/` | Entities |
| Vitreous.Onboarding.Infrastructure | `src/Infrastructure/` | EF Core, security |
| Vitreous.Onboarding.UnitTests | `tests/UnitTests/` | Unit tests |
| Vitreous.Onboarding.IntegrationTests | `tests/IntegrationTests/` | Integration tests |

API routes use the prefix configured in `Application:ApiRoutePrefix` (default `api/v1`). Do not repeat the prefix on individual controllers.
