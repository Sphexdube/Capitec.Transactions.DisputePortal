# ADR-002: Map supporting tables to persistence records

**Date:** 2026-10-18
**Status:** Accepted
**Decided by:** Engineering

---

## Context

`Dispute` is the only aggregate root with behaviour. The customer, transaction, history, idempotency, outbox and confirmation tables hold data that the domain reads as immutable records, such as `TransactionSummary` and `DisputeHistoryEntry`.

## Options Considered

| Option | Pros | Cons |
|--------|------|------|
| Map every domain type directly | One type per table | Domain records must carry EF keys and constructors |
| Map supporting tables to Infrastructure records and project to domain records | The domain stays free of persistence detail | Repositories map between the two |

## Decision

EF Core maps `Dispute` directly. Supporting tables map to `Infrastructure/Persistence/Records`, and repositories project them to domain records. A key that no code reads is an EF shadow property, not a C# property.

## Consequences

- The domain has no EF attributes, keys or parameterless constructors.
- A new supporting table needs a record, a mapping and a repository projection.

## References

- Technical specification, section 4.
