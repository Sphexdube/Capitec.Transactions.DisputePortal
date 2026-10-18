# Application Guide

## API conventions

- API routes use URL versioning under `/api/v1`.
- Successful responses use a `{ data, correlationId }` envelope; errors use `{ errors, correlationId }`.
- Customer endpoints require a bearer token. Customer identity is read from the validated token, never from a request parameter or body.
- Collection responses use bounded keyset pagination. The default page size is 20; the maximum is 50.
- Dispute submission requires an `Idempotency-Key` header.

## Endpoints

| Method | Route | Access | Purpose |
|---|---|---|---|
| `GET` | `/api/v1/demo-session` | Local/Development, anonymous | List seeded demo customers. |
| `POST` | `/api/v1/demo-session` | Local/Development, anonymous | Issue a short-lived token for a selected demo customer. |
| `GET` | `/api/v1/transaction` | Customer token | Return a bounded transaction page; supports cursor and date filters. |
| `GET` | `/api/v1/transaction/{transactionId}` | Customer token | Return an owned transaction summary. |
| `POST` | `/api/v1/transaction/{transactionId}/dispute` | Customer token | Accept an idempotent dispute submission; returns `202 Accepted`. |
| `GET` | `/api/v1/dispute` | Customer token | Return a bounded customer dispute page. |
| `GET` | `/api/v1/dispute/{disputeId}` | Customer token | Return an owned dispute and transaction summary. |
| `GET` | `/api/v1/dispute/{disputeId}/history` | Customer token | Return ordered dispute history. |
| `GET` | `/api/v1/confirmation/{disputeId}` | Customer token | Return `Pending` or `Recorded` confirmation status. |
| `GET` | `/api/v1/health/live` | Anonymous | Liveness check. |
| `GET` | `/api/v1/health/ready` | Anonymous | Readiness check, including PostgreSQL. |

Swagger is served at `/swagger` in Local and Development environments.

The transaction page limits selectable history to the latest three calendar months in Africa/Johannesburg time. Transactions are grouped into collapsible month sections, closed by default; loading another cursor page updates the matching month group.

## Submission and confirmation

The submission handler validates request fields and transaction ownership. A unit-of-work transaction commits the dispute, initial history event, idempotency record, and outbox event together. A retry with the same key and payload returns the original dispute; reusing the key with another payload returns a conflict.

The hosted confirmation worker claims available outbox work with PostgreSQL row locking and `SKIP LOCKED`. It records one confirmation per event and removes the processed outbox item in the same transaction. The confirmation endpoint remains `Pending` until that record exists.

## Current operational signals

Structured application logs and liveness/readiness health checks are implemented. OpenTelemetry metrics, distributed tracing, and hosted alerting are design targets only; they are not currently configured in this assessment repository.