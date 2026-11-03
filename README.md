# LilyWhiteMap

An interactive map charting Tottenham Hotspur player birthplaces, supporters clubs, and European away nights.

🔗 Live site: [tottenhamplayersmap.com](https://tottenhamplayersmap.com/)

## Stack

- **Frontend** — Next.js (React, TypeScript) with Mapbox GL, in [frontend/](frontend/)
- **Backend** — ASP.NET Core Web API with EF Core / PostgreSQL, in [backend/](backend/)

## Getting started

### Backend

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:LilyWhiteMap" "Host=localhost;Port=5432;Database=<db-name>;Username=<user>;Password=<password>"
dotnet user-secrets set "Jwt:SigningKey" "REPLACE-WITH-32-BYTE-RANDOM-STRING"
dotnet user-secrets set "Admin:Password" "test"
dotnet run
```

The API listens on `http://localhost:5000` by default. On first run against an empty database it syncs the player roster from THFCDB; on later runs it skips that sync unless triggered manually via `POST /api/admin/sync`.

The admin dashboard needs two more settings. Locally, set them with `dotnet user-secrets`:

- `Jwt:SigningKey` — HS256 signing key for admin JWTs, at least 32 bytes.
- `Admin:Password` (plain text, local dev only) or `Admin:PasswordHash` (SHA-256 hash, preferred for production).

In **Production** (`ASPNETCORE_ENVIRONMENT=Production`), these — plus the DB connection string — are instead loaded from **AWS SSM Parameter Store** under `/lilywhitemap/prod/*` (see `Program.cs`), so the EC2 instance role needs `ssm:GetParametersByPath` + `kms:Decrypt` permission. Parameter names map `/` to `:`, e.g. `/lilywhitemap/prod/Jwt/SigningKey` → `Jwt:SigningKey`.

#### API surface

| Endpoint | Auth | Description |
| --- | --- | --- |
| `GET /health` | — | Liveness check |
| `GET /api/players` | — | List all players |
| `GET /api/players/{slug}` | — | Get a single player |
| `POST /api/admin/sync` | — | Manually re-sync the roster from THFCDB |
| `POST /api/admin/auth/login` | — | Exchange the admin password for a JWT |
| `GET /api/admin/analytics/summary` | JWT | Traffic totals (today / 7d / all-time) |
| `GET /api/admin/analytics/recent` | JWT | Paged recent visit log |
| `GET /api/admin/analytics/top-routes` | JWT | Most-viewed paths |
| `GET /api/admin/analytics/top-referrers` | JWT | Top referrers |

A `VisitorTrackingMiddleware` logs page views (path, visitor ID, response time) through a non-blocking queue and background batch writer, which backs the analytics endpoints above.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Set `NEXT_PUBLIC_API_URL` (pointing at the backend) and `NEXT_PUBLIC_MAPBOX_TOKEN` in `frontend/.env.local` before starting. The app runs at `http://localhost:3000`.
