# API layer

HTTP boundary for the application.

| Folder | Purpose |
|--------|---------|
| `Controllers/` | Thin API controllers; map HTTP to Application features |
| `Middleware/` | Exception handling, correlation ID, auth pipeline |
| `Configuration/` | DI registration, Swagger, CORS, JWT setup |

No business logic in this layer.
