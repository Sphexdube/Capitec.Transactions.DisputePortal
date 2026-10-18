# Solution Overview

## Purpose

The portal demonstrates a customer-facing transaction-dispute workflow using synthetic data. A customer can review a bounded transaction history, submit a dispute, and review the dispute status, event history, and simulated confirmation.

The application is an independent assessment. It has no connection to bank systems, does not move money, and is not a production service.

## Scope

| Included | Excluded |
|---|---|
| Local demo sign-in for seeded synthetic customers | Real customer identity or bank integrations |
| Customer-owned transaction history and date filtering | Staff investigation or case-management screens |
| Idempotent dispute submission and append-only status history | Payment reversal or real email/SMS notifications |
| Simulated asynchronous confirmation | Production deployment, retention, or service-level commitments |

The approved requirements and assumptions are in the [product specification](../specs/product-spec.md). The approved component, data, and API design is in the [technical specification](../specs/technical-spec.md).

## Components

| Component | Responsibility |
|---|---|
| `web/` | Next.js customer interface and same-origin BFF. The session token is stored in an HttpOnly cookie. |
| `Capitec.Transactions.DisputePortal.Api` | Versioned HTTP endpoints, authentication, response envelopes, and composition root. |
| `Capitec.Transactions.DisputePortal.Application` | Use-case handlers, request validation, paging, and confirmation processing. |
| `Capitec.Transactions.DisputePortal.Domain` | Dispute rules, value objects, and persistence/processing contracts. |
| `Capitec.Transactions.DisputePortal.Infrastructure` | PostgreSQL persistence, repositories, unit of work, health checks, and confirmation worker. |
| `dbDisputePortal/up/` | Grate-owned PostgreSQL schema and synthetic local data scripts. |

## Customer flow

1. Select one of the synthetic demo customers.
2. Review and filter the customer's transaction history.
3. Submit a dispute with an idempotency key.
4. Review its status, append-only history, and simulated confirmation.

Customer identity comes from the validated signed token. The API applies ownership checks to every customer data request.