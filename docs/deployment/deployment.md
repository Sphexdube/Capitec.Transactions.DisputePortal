# Deployment Status

## Supported environment

The supported deployment is the local Docker Compose stack. No CI workflow, shared test environment, or production deployment is configured in this repository.

Compose starts PostgreSQL 16, runs the pinned Grate migration container, then starts the API and web application. Health and dependency conditions gate service startup. The API and web images are built from `build/Dockerfile.api` and `build/Dockerfile.web`.

| Service | Default host address | Purpose |
|---|---|---|
| Web | `http://localhost:8080` | Customer interface. |
| API | `http://localhost:5147` | Versioned API and local Swagger UI. |
| PostgreSQL | `localhost:5432` | Local persistent application data. |

Host ports can be changed with `WEB_PORT`, `API_PORT`, and `POSTGRES_PORT`. Compose uses local-only demo credentials by default; override them with environment variables for local use. Do not treat these defaults as production secrets.

## Release boundary

The technical specification describes possible CI and production concerns as design context. They are not implemented release targets. There is no deployment pipeline, production configuration, production identity provider, or production data-retention policy. Do not describe this assessment as production-ready or as an operated bank service.

## Local reset

`docker compose down` stops containers and preserves the PostgreSQL volume. `docker compose down -v` deletes the local volume and all synthetic database state; use it only when a full reset is intended.

For exact startup and test commands, see [Local development](../development/local-development.md). For operational recovery steps, see the [local troubleshooting runbook](../runbooks/local-troubleshooting.md).