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
dotnet run
```

The API listens on `http://localhost:5000` by default. On first run against an empty database it syncs the player roster from THFCDB; on later runs it skips that sync unless triggered manually via `POST /api/admin/sync`.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Set `NEXT_PUBLIC_API_URL` (pointing at the backend) and `NEXT_PUBLIC_MAPBOX_TOKEN` in `frontend/.env.local` before starting. The app runs at `http://localhost:3000`.
