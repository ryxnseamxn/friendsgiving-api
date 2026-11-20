# Friendsgiving.Api

Minimal .NET API for the Friendsgiving potluck app. Deployed to Railway with a managed PostgreSQL database.

## Endpoints (v1)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/gatherings/{id}` | Get gathering + dishes + claims |
| `POST` | `/api/gatherings/{id}/items/{itemId}/claim` | Claim a dish (`{ guestName, note? }`) |
| `DELETE` | `/api/gatherings/{id}/items/{itemId}/claim` | Unclaim a dish |
| `GET` | `/health` | Health check |

## SignalR Hub

| Path | Purpose |
|------|---------|
| `/hubs/friendsgiving` | Live dish-list updates. Clients join group `<gatheringId>` to receive `OnDishClaimed`, `OnDishUnclaimed`, `OnTableUpdated`. |

## Local development

```bash
# Start Postgres (Homebrew)
brew services start postgresql@15
createdb friendsgiving

# Run migrations
cd src/Friendsgiving.Api
dotnet ef database update --connection "Host=localhost;Port=5432;Database=friendsgiving;Username=ryanseaman;Password=password"

# Run the API
dotnet run --urls "http://localhost:5000"
```

## Railway deployment

1. Create a PostgreSQL service in Railway.
2. Create a Web service from this repo (Railway auto-detects the Dockerfile).
3. Set env var `ConnectionStrings__DefaultConnection` — Railway auto-injects `{{Postgres.DATABASE_URL}}` into it.
4. The app listens on port `8080` (via `ASPNETCORE_URLS=http://+:8080`).
5. Railway runs `dotnet ef database update` on deploy (migration applied automatically).

No auth — open for friends to use. Add an API key later if needed.

No seeding — dishes are pre-created by hand in the DB or via a separate script.
