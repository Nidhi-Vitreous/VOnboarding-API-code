# Infrastructure layer

Technical implementations of Application interfaces.

| Folder | Purpose |
|--------|---------|
| `Persistence/` | EF Core DbContext, configurations, migrations |
| `Repositories/` | Data access implementations |
| `Integrations/` | External systems (NetSuite, email, etc.) |
| `Services/` | Cross-cutting infra (audit writer, clock, etc.) |

PostgreSQL 16 via Npgsql EF Core provider.
