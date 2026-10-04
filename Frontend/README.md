# DocuNest web application

React 19 / TypeScript 6 / Vite 8 / Tailwind 4 foundation. Node 24.11+ (24.x),
npm 11.6+ (11.x). Exact dependencies and transitive integrity hashes are in the
committed package-lock.json. TypeScript 6 is selected for ESLint compatibility;
jsdom 29 supports the installed Node 24.11 runtime. Vitest 4.1 is selected because
Vitest 5's new assertion type signature is incompatible with jest-dom 7.

From this directory:

```powershell
npm ci --ignore-scripts
npm run dev
npm run typecheck
npm run lint
npm test
npm run build
$env:PLAYWRIGHT_BROWSERS_PATH = "$PWD/.cache/ms-playwright"
npx playwright install chromium
npm run test:e2e
```

Vite serves loopback port 5173 and proxies `/api` to loopback port 5080. Copy
`.env.example` to `.env.local` to change the development target. Production is
same origin; no third-party document services, fonts, telemetry, or scripts are
used. The anonymous shell only probes API health and displays development state
honestly. Later features are unavailable until their backend milestones pass.

English strings live in `src/i18n/en.ts`; components use translation keys and
logical CSS properties. Theme preferences may use localStorage. Access JWTs
live in memory only. API requests disable browser caching and redirect following;
query state has no persistent cache. Logout/workspace changes must invoke
`clearScopeCache`. Phase 2 supplies the audited CSRF and refresh contract before
refresh behavior is enabled. No document service-worker cache is registered.

Browser camera/PWA capability fallbacks arrive in Phase 3. Native mobile apps
remain outside v1. See `../document/Implementation-Status.md` for executed checks,
remaining gates, and limitations; automation does not establish WCAG conformance.
