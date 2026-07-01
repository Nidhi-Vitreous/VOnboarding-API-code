# Application layer

Use cases and orchestration. Depends on Domain only.

| Folder | Purpose |
|--------|---------|
| `Auth/` | Authentication and token refresh |
| `Authorization/` | Permission checks and department resolution |
| `Roles/` | Role and department management |
| `Users/` | User management |
| `Interfaces/` | Ports for repositories and external services |

Add new vertical slices as feature folders (e.g. `Onboarding/`) when implementing additional domains.
