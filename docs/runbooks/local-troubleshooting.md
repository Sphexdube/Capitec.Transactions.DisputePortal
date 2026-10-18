# Local troubleshooting

## Check the disk first

`input/output error` from BuildKit, apt or npm during `docker compose build` means the host disk is full.

```sh
df -h /System/Volumes/Data
docker system df
```

Free space on the host, then build again.

## A port is already in use

The web app uses `WEB_PORT` (default 8080), the API uses `API_PORT` (default 5147), and PostgreSQL uses `POSTGRES_PORT` (default 5432).

```sh
lsof -nP -iTCP:8080 -sTCP:LISTEN
WEB_PORT=8082 docker compose up -d
```

## Requests reach an old build

A `dotnet run` or `next dev` that is still running can hold the port before the container does. The container then logs nothing, and the requests go to the old process. Confirm which process listens before you debug:

```sh
lsof -nP -iTCP:5147 -sTCP:LISTEN
docker compose ps
```

## The demo data is used up

Each seeded transaction can have one active dispute. After many e2e runs, every transaction is disputed. Reset the database. The Grate scripts seed it again.

```sh
docker compose down -v
docker compose up -d
```

## Expected log lines

- `info: ... The API request was rejected. StatusCode: 404 ...` and `StatusCode: 409` are expected outcomes, not failures.
- `fail:` lines are real failures. Each one includes the correlation id that the API returned to the caller.
