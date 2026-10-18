# Lessons learned

Incidents and near-misses from this build, and the change each one produced. The second section records broader engineering lessons applied here.

## From this build

### A concurrency test finds what a review does not

The specification asks for "100 concurrent retries with one outcome". No such test existed. When it was measured, six concurrent requests with one idempotency key gave five `503` responses: the unique-index violation was reported as "persistence unavailable". At 100 requests a second gap appeared. READ COMMITTED reads the latest commit per statement, so a request could miss the idempotency record and then see the dispute. See ADR-004.

Any check-then-insert needs a conflict path, and a concurrent test that proves it.

### Test configuration must be applied before the host reads it

`WebApplicationFactory.ConfigureAppConfiguration` runs after `Program` has already read the connection string. The integration tests silently used the Docker Compose database, and they passed. Use `UseSetting`, and prove isolation by running the tests with the compose stack stopped.

### A redirect cached by the client router outlives the session change

Next.js prefetched protected pages while signed out and cached their redirect to `/login`. After sign-in, the cached redirect sent the customer back to sign-in. Links on the sign-in page do not prefetch.

### Port mapping changes what "same origin" means

The BFF compared `Origin` with the container's own host. Behind Docker port mapping (`8082:3000`) every write returned `403`. Compare with `X-Forwarded-Host` first, then `Host`.

### An upstream 404 is not an outage

An unknown dispute id showed "The service may be temporarily unavailable", because the server fetch treated every non-2xx response the same. The server fetch now sends `404` to the not-found page.

### Expected outcomes must not be logged as failures

`404` and `409` responses were logged as `fail:` by both the handler and the middleware. That hides real failures in noise. Rejections derive from `RequestRejectedException`, handlers do not log them as errors, and the middleware logs `4xx` at Information and `5xx` at Error.

### A full disk looks like a Docker fault

Builds failed with `input/output error` in BuildKit and apt. The cause was 128 MB free on the host. Check `df -h` before you debug Docker.

## Operational lessons applied

| Operational lesson | Application here |
|---|---|
| Per-replica state breaks under load | The API keeps no state per replica. Sessions are bearer tokens. Idempotency, outbox and confirmations are in PostgreSQL, and the outbox worker uses `FOR UPDATE SKIP LOCKED`. ASP.NET Core data protection is registered by authentication but protects nothing, so its key-storage warnings are filtered. If the API ever issues cookies, persist the key ring to PostgreSQL first. |
| A value derived from a changed source goes stale | The customer id comes from the token claim, never from configuration or the request body. |
| A fallback hides the failure | `API_BASE_URL` falls back to `localhost:5147` only outside production. In production a missing value throws. |
| Measure the thing, not a proxy | A submitted dispute is logged at Information (`Dispute {DisputeId} was submitted.`). Production does not write Debug. |
| A guard that cannot fail | A missing or non-numeric customer claim returns `401`. Controller tests and API tests assert it. |
| Dead metrics read as good news | No metrics are emitted yet, so there are no dashboards to go stale. |
| Caught is not reported | The BFF and the server fetch log upstream `5xx` and network failures to the server log. There is no Sentry in this assessment. |
| Trust behaviour over documentation | Not applicable. No third-party API. |
| Confirm which process is answering | See the [local troubleshooting runbook](../runbooks/local-troubleshooting.md). |
| A merge to `main` is not a release | Not applicable. No deployment pipeline. |
| Scan dependencies when you add a package | `dotnet list package --vulnerable --include-transitive` finds nothing. `npm audit --omit=dev` finds nothing. `npm audit` reports `braces` through `eslint-config-next`. It is development tooling only, and no patched `braces` exists. |
| Two collection Includes multiply rows | No query uses `Include`. Reads project to records with `AsNoTracking`. |
| A read-then-write upsert is a race | Found and fixed. See ADR-004. |
| A retrying strategy changes what a failed write means | Writes run inside the execution strategy (ADR-001). A re-run after a lost commit acknowledgement replays through the idempotency record (ADR-004). |
