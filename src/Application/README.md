# Application layer

Use cases and orchestration. Depends on Domain only.

| Folder | Purpose |
|--------|---------|
| `Features/` | Vertical slices per domain (Onboarding, Workflow, Tasks, …) |
| `DTOs/` | Request/response data transfer objects |
| `Interfaces/` | Ports for repositories and external services |
| `Validators/` | Input validation (FluentValidation or similar) |

Each feature folder will contain handlers/commands/queries when implemented.
