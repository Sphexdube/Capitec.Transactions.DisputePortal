# Local Development

## Prerequisites

- .NET SDK `10.0.300`, selected by `global.json`.
- Docker Desktop with Docker Compose.
- Node.js `22.12+` and npm for web development and web checks.
- Google Chrome for Playwright browser tests.

## Start the complete stack

From the repository root:

```sh
cd src
docker compose up -d --build
docker compose ps
```

The stack applies Grate scripts, loads synthetic demo data, and starts PostgreSQL, API, and web services. Open `http://localhost:8080`; use `http://localhost:5147/swagger` to inspect the API.

To follow startup or worker logs:

```sh
cd src
docker compose logs -f postgres migrations api web
```

## Run checks

From the repository root:

```sh
dotnet test Capitec.Transactions.DisputePortal.sln
```

The integration test project starts PostgreSQL with Testcontainers and requires Docker.

For the web application:

```sh
cd web
npm ci
npm run quality
PLAYWRIGHT_BASE_URL=http://localhost:8080 npm run test:e2e
```

The end-to-end suite requires the Compose stack and Chrome. It runs desktop and mobile customer flows and creates synthetic dispute data.

## Run a project outside Compose

Start the database and migration job first:

```sh
cd src
docker compose up -d postgres migrations
```

Then run the API project in the `Development` environment. Ensure its `ConnectionStrings__DisputePortal` points to the published PostgreSQL host port. For web-only development, set `API_BASE_URL` in `web/.env.local` to the API address and run `npm run dev` from `web/`.

## Reset data

To stop services while preserving data:

```sh
cd src
docker compose down
```

To remove the local database volume and reseed from Grate on the next start:

```sh
cd src
docker compose down -v
docker compose up -d --build
```

The `-v` option deletes local database contents. Keep disputes created by browser tests in mind when a test requires a fresh undisputed transaction.

For port conflicts, missing container space, failed migrations, and expected API error logs, see the [local troubleshooting runbook](../runbooks/local-troubleshooting.md).