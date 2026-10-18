# ADR-003: Reschedule outbox messages that cannot be processed

**Date:** 2026-10-18
**Status:** Accepted
**Decided by:** Engineering

---

## Context

The confirmation worker reads due outbox messages in batches with `FOR UPDATE SKIP LOCKED`. If one message in a batch failed, the whole batch rolled back and was read again on the next poll, so one bad message blocked every confirmation.

## Options Considered

| Option | Pros | Cons |
|--------|------|------|
| Fail the batch | Simple | One bad message stops all confirmations |
| Reschedule the bad message and confirm the rest | Other confirmations continue | A bad message retries until fixed |

## Decision

A message with an unknown event type, an invalid payload or a missing dispute is rescheduled one minute later and its `Attempts` count goes up by one. The other messages in the batch are confirmed and deleted in the same transaction. Each reschedule is logged as a warning with the event id and the reason.

## Consequences

- One bad message no longer blocks other customers.
- `Attempts` shows messages that keep failing. A dead-letter limit is not implemented yet.

## References

- Technical specification, sections 5 and 8.
