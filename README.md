# Friendsgiving.Api

Minimal .NET API for the Friendsgiving potluck app. Deployed to Railway with a managed PostgreSQL database.

Every mutation returns the full gathering, and the same shape is broadcast over SignalR, so clients just replace their state.

## Guest endpoints

| Method | Path | Body |
|--------|------|------|
| `GET` | `/api/gatherings/{id}` | |
| `POST` | `/api/gatherings/{id}/items` | `{ name, category?, emoji?, claimedBy? }` |
| `POST` | `/api/gatherings/{id}/items/{itemId}/claim` | `{ guestName, note? }`. Returns 409 if the dish is already claimed |
| `DELETE` | `/api/gatherings/{id}/items/{itemId}/claim` | |
| `PUT` | `/api/gatherings/{id}/allergies` | `{ guestName, details? }`. Omit `details` for "no allergies"; one answer per guest |
| `GET` | `/health` | |

## Admin endpoints

Require the `X-Admin-Key` header to match `Admin:ApiKey`. When no key is configured, they're open in Development and disabled elsewhere.

| Method | Path | Body |
|--------|------|------|
| `GET` | `/api/admin/gatherings` | |
| `POST` | `/api/admin/gatherings` | `{ name, hostName, date, location? }` |
| `PUT` | `/api/admin/gatherings/{id}` | same as create |
| `DELETE` | `/api/admin/gatherings/{id}` | |
| `POST` | `/api/admin/gatherings/{id}/items` | `{ name, category?, emoji? }` |
| `DELETE` | `/api/admin/gatherings/{id}/items/{itemId}` | |

## SignalR hub

`/hubs/friendsgiving`. Call `JoinGathering(gatheringId)`, then receive `GatheringUpdated(gathering)` and `GatheringDeleted(gatheringId)`.

## Local development

The whole stack (Postgres, this API, and the Expo app) runs from the app repo checked out next to this one:

```bash
cd ../blog && docker compose up --build   # API on http://localhost:5080, Postgres on localhost:5433
```

To run the API on the host instead (e.g. to debug it), start only the database and use `dotnet run`. It applies migrations on startup.

```bash
(cd ../blog && docker compose up -d db)
dotnet run --project Friendsgiving.Api      # http://localhost:5080
```

`Friendsgiving.Api.http` has sample requests.

## Railway deployment

Infrastructure is declared in `.railway/railway.ts` (Railway IaC): a managed Postgres database plus the `api` service, built from this repo's `Dockerfile`, with its connection string wired to the database. It replaces the old `railway.json`, which Railway stops reading on 2026-12-01.

One-time setup:

```bash
brew install railway                          # Railway CLI 5.42.1 or newer
railway login
(cd .railway && npm install)                  # types for railway.ts
# Set API_REPO in .railway/railway.ts to this repo's GitHub path, then:
railway config plan                           # preview what will be created
railway config apply
railway variables --service api --set "Admin__ApiKey=$(openssl rand -hex 32)"
railway domain --service api                  # public https://….up.railway.app URL
```

After that, pushes to the default branch redeploy the API. Edit `railway.ts` and run `railway config plan` and `railway config apply` to change the infrastructure. The API applies pending migrations on startup.

Production variables:

| Variable | Source |
|---|---|
| `ConnectionStrings__DefaultConnection` | Declared in `railway.ts` from the Postgres `DATABASE_URL` (the `postgres://` URL is converted automatically) |
| `Admin__ApiKey` | Set once with `railway variables`, kept out of git via `preserve()` |
