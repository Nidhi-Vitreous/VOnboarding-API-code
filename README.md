# Vitreous Onboarding — Backend (real project)

.NET **10** Clean Architecture solution. Ready for feature-by-feature development with Cursor.

## Quick start

```bash
cd "D:\Vitreous Onboarding Project\VOnboarding-API-code"
dotnet restore
dotnet build
dotnet run --project src/Api/Vitreous.Onboarding.Api.csproj
```

Health check: http://localhost:5000/health

## Solution

| Project | Path | Layer |
|---------|------|-------|
| Vitreous.Onboarding.Api | `src/Api/` | HTTP, middleware, DI |
| Vitreous.Onboarding.Application | `src/Application/` | Use cases by feature |
| Vitreous.Onboarding.Domain | `src/Domain/` | Entities, enums, value objects |
| Vitreous.Onboarding.Infrastructure | `src/Infrastructure/` | EF Core, repos, integrations |
| Vitreous.Onboarding.UnitTests | `tests/UnitTests/` | Unit tests |
| Vitreous.Onboarding.IntegrationTests | `tests/IntegrationTests/` | API integration tests |

## Feature folders (Application)

- `Features/Onboarding/`
- `Features/Workflow/`
- `Features/Tasks/`
- `Features/Dashboard/`
- `Features/Notifications/`
- `Features/Administration/`

Add controllers under `src/Api/Controllers/` per feature when implementing APIs.

## Configuration

- `src/Api/appsettings.json` — connection string, JWT, CORS placeholders
- `.env.example` — local environment variables

## Bitbucket

Push this folder as standalone repo `vitreous-onboarding-api`.
