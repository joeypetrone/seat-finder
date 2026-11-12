# Session 2 – Front end skeleton

**Date:** 2026-11-12

## Checklist

- [x] Vite + React + TypeScript project with `strict: true`
- [x] Vite dev proxy to the API
- [x] Page that calls `/health` and shows the result (I wrote the typed fetch)
- [x] Close out (`npm run lint` clean, `npm run build` passes strict type check, page shows "Healthy" via the proxy)

## What was done

- `web/` created with `npm create vite@latest web -- --template react-ts` (Vite 8, React 19, TypeScript 6), ESLint chosen as the linter
- `"strict": true` written out in `web/tsconfig.app.json`
- Dev proxy in `web/vite.config.ts`: `/health` forwards to the API at `http://localhost:5166` (http profile)
- `App.tsx` (written by me): `ApiStatus` union type (`'checking' | 'healthy' | 'down'`), `useState<ApiStatus>`, `useEffect` with an inner `async` function, `fetch('/health')` + `response.text()`, `try`/`catch`, and a `Record<ApiStatus, string>` label lookup. Shows Checking… / Healthy / Down
- Demo cleanup: deleted `App.css` and `src/assets/`, trimmed `index.css` to a ~25-line base (colors, dark mode, box-sizing reset, centered `main`), page title "Seat finder"

## What changed from the plan

- TypeScript 6 makes `strict` the default, so the template's tsconfig no longer mentions it. Strict was already on (checked with a test file); we added the line anyway so it's visible to reviewers
- The plan was revised mid-session (zero-card hosting, validation rule, location filter, 23 sessions). Session 2 itself didn't change. Base path work for `joeydev.net/seat-finder` is in the static demo session, so `/health` stays as is for now
- Applied the new validation rule on the client side: only an OK response with exactly `Healthy` counts as healthy; anything else, including a network error, shows Down

## What I learned

- Run `npm` commands from `web\` (where `package.json` is), `git` and `dotnet` from `seat-finder\`
- Only one dev server per port: a second `npm run dev` moved to 5174 because the first was still running
- A React component is a Screen; `useState` is a screen variable plus an Assign that redraws; `useEffect(..., [])` is OnReady
- The effect itself can't be `async`; declare an `async` function inside it and call it (I forgot the call at first, and `noUnusedLocals` pointed it out)
- `fetch` returns an envelope (`Response`): check `.ok`, then open it with `.text()` (not `.json()`, since `/health` returns plain text)
- A union type plus `Record<ApiStatus, string>` makes TypeScript check that every status has a label
- The Vite dev proxy avoids CORS by making the browser talk only to :5173

## What's next

Session 3 – xUnit test project with an integration test for `/health` using `WebApplicationFactory`, and `dotnet test` running clean from the repo root.
