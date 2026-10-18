# ADR-004: Resolve concurrent dispute submissions to a replay or a conflict

**Date:** 2026-10-18
**Status:** Accepted
**Decided by:** Engineering

---

## Context

Dispute submission checks the idempotency record and the active-dispute rule before it inserts. Two requests that arrive together both pass the checks. PostgreSQL then rejects the second insert on `UQ_Dispute_CustomerIdTransactionId_Active`. The unit of work wrapped that violation in `SaveContextException`, so the API returned `503`. A measured run of six concurrent requests with one idempotency key gave one `202` and five `503`.

A second gap came from READ COMMITTED isolation. Each statement reads the latest commit. A request could read "no idempotency record" and then, after the first request committed, read "active dispute exists". It returned `409` where it had to replay the original `202`.

## Options Considered

| Option | Pros | Cons |
|--------|------|------|
| SERIALIZABLE isolation | One consistent snapshot | Serialization failures on normal traffic; every caller must retry |
| Advisory lock per customer and transaction | No race | Extra lock protocol to maintain; holds a lock while it waits |
| Translate the unique violation and retry once | Uses the constraint that already exists; short code path | One extra round trip on the conflict path only |

## Decision

- The unit of work translates PostgreSQL `23505` (unique violation) into `UniqueConstraintException` (Domain). Other persistence failures stay `SaveContextException`.
- `SubmitDisputeHandler` catches `UniqueConstraintException` once and runs the submission again in a new unit of work. The second pass sees the committed row and replays it or returns `ACTIVE_DISPUTE_EXISTS`.
- When the active-dispute check finds a dispute, the handler reads the idempotency record again before it returns `409`.
- A `UniqueConstraintException` that reaches the API maps to `409 CONCURRENT_REQUEST`, not `503`.

## Consequences

- 100 concurrent retries with one key return 100 × `202` with one dispute id. Six concurrent requests with different keys return one `202` and five `409` (`ApiFlowTests.SubmitDispute_WhenRequestsRunConcurrently_*`).
- A retry after a lost commit acknowledgement also replays, because the second pass reads the committed idempotency record.

## References

- Product specification O-03 and UAT-03.
- Technical specification, section 8 (duplicate active dispute).
- ADR-001.
