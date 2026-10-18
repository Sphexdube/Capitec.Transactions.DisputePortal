# Transaction dispute portal — web

Capitec-inspired customer interface for the independent assessment. Uses the existing ASP.NET API; it does not fabricate transactions when the service is unavailable.

## Run

The simplest way to run the web app is the full stack: `cd src && docker compose up -d`, then open http://localhost:8080.

To run it outside Docker you need Node.js 22.12+ and npm. Start the API and its database first (`cd src && docker compose up -d postgres migrations api`). The demo customers and transactions come from the Grate `*.ENV.LOCAL.sql` scripts.

```sh
cd web
npm ci
cp .env.example .env.local
npm run dev
```

Open http://localhost:8080. `API_BASE_URL` defaults to http://localhost:5147; override it if your API uses a different port. Choose a customer provided by the API. No customer IDs are hardcoded in the interface. After sign-in, the header shows the signed-in customer and the transactions page greets them by first name. The BFF stores the display name in an HttpOnly cookie next to the session cookie and deletes both at sign-out.

```sh
npm run quality
npm run build
npm start -- --port 8080
```

## Conventions

Uses the App Router, strict TypeScript, named components, kebab-case filenames, colocated CSS modules, semantic design tokens, and shared persistent error states. Components live under `src/components`, contracts under `src/types`, content under `src/constants`, and API/formatting helpers under `src/lib`. The interface uses public dependencies and project-owned styling.

`src/styles/tokens.css` owns the Capitec palette and shared scales. Small reusable primitives provide buttons, icons, status badges and state panels. Pages compose these components. The interface is responsive and includes visible keyboard focus, a skip link, native labelled form controls, and accessible error messages.

## API boundary

Server pages fetch bounded customer data. An API `404` shows the not-found page. Upstream `5xx` responses and network failures are logged on the server. The browser calls only allowlisted same-origin BFF routes. The session token stays in an HttpOnly SameSite cookie; state-changing requests require the same Origin. The API remains responsible for token validity, customer ownership and business rules. Unchanged submission retries reuse the same idempotency key within the form; leaving or refreshing the form ends that attempt, so check My disputes after an uncertain submission.

Money is formatted using BigInt and the currency's minor-unit precision. Date filters and display use Africa/Johannesburg. Pagination follows API cursors. No fake account balances, masked account references, or global counts are shown: the current transaction contract does not supply them. Date filtering is supported; transaction-type filtering awaits API support. The dispute form uses the selected list row, so the portal does not call the transaction detail endpoint.

The public Capitec symbol is attributed in `public/brand/README.md`. Local font fallbacks avoid runtime third-party font calls. The independent-assessment notice appears on every page.

## Browser validation

With the API and frontend running, use `PLAYWRIGHT_BASE_URL=http://localhost:8081 npm run test:e2e` (adjust the URL to your frontend). Against Docker Compose the URL is `http://localhost:8080` (or your `WEB_PORT`). The tests use installed Google Chrome, discover demo customers and undisputed transactions, and create synthetic disputes. They check desktop and 360px mobile journeys, Axe accessibility, same-origin enforcement, errors, empty states and pagination. Reset the demo database with `docker compose down -v` when the seeded transactions have all been disputed.

The dependency audit currently reports a braces advisory through Next's ESLint development tooling. There is no compatible fix advertised by npm; do not downgrade Next's lint configuration to the suggested older major. Recheck when upstream publishes a compatible fix.
