# ADR-001: Run unit-of-work transactions inside the retrying execution strategy

**Date:** 2026-10-18
**Status:** Accepted
**Decided by:** Engineering

---

## Context

The database provider is configured with `EnableRetryOnFailure`. A retrying execution strategy rejects transactions that the caller opens itself, so a unit of work with separate `BeginTransactionAsync` and `CommitTransactionAsync` methods fails at runtime. Dispute submission and outbox processing both need one transaction around several writes and a row lock.

## Options Considered

| Option | Pros | Cons |
|--------|------|------|
| Separate begin and commit methods, no retry | Simple call sites | Transient database faults fail the request |
| `ExecuteInTransactionAsync(operation)` inside the execution strategy | Transient faults retry the whole operation; one transaction boundary | The operation must be safe to run again |

## Decision

`IUnitOfWork.ExecuteInTransactionAsync` runs the operation inside the provider execution strategy. Each attempt clears the change tracker, opens a transaction, runs the operation and commits. A database failure after the last attempt becomes `SaveContextException`, which the API returns as `503 PERSISTENCE_UNAVAILABLE`.

## Consequences

- Handlers create new entities inside the operation, so a retry starts from a clean state.
- Dispute submission is safe to run again because the idempotency record is checked inside the same transaction.
- Outside a transaction, `SaveChangesAsync` still wraps failures in `SaveContextException`.

## References

- Technical specification, sections 2 and 8.
