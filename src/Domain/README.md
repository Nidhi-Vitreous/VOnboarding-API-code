# Domain layer

Core business model. **No dependencies** on Application, Infrastructure, or Api.

| Folder | Purpose |
|--------|---------|
| `Entities/` | Aggregate roots and entities |
| `Enums/` | Domain enumerations (status, roles, etc.) |
| `ValueObjects/` | Immutable value types (TaxId, Email, etc.) |

Pure domain rules live here or in entity methods.
