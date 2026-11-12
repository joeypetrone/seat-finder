# Seat Finder – Project Plan and Checklist (v2)

A small app that finds movie showings where our preferred row or view is still open, with an agent that watches showtimes and flags a match. Built in C#/.NET with a React + TypeScript front end, using a checklist-driven, session-based AI workflow.

_Revised 2026-11-11: smaller sessions, a "learn by building" working style, Postgres instead of SQLite._
_Revised 2026-11-12: zero-card plan. No cloud billing account; the live demo is a static snapshot on my existing Firebase site; the agent runs only on my PC against capped prepaid Claude credits._
_Revised 2026-11-12: back-end validation everywhere (including the agent's tool inputs), usage guardrails for the agent, and a location filter (new Session 10). Now 23 sessions._

## Why this project

- Portfolio piece for ticketing and entertainment roles (SeatGeek, Vivid Seats, StubHub, Fandango)
- Refresh C#/.NET and React, and learn TypeScript properly
- Real practice building an agent in C#/.NET with the Anthropic C# SDK
- Shows the checklist + planned session workflow in a public repo
- Live demo at its own path on my site, `joeydev.net/seat-finder`, without changing the rest of the site
- Costs $0 to host, and AI spend is capped at a prepaid amount I choose

## Scope for v1

Keep it small and finished.

- One chain with 3–4 theater locations in one metro area as the data source
- Save seat preferences (rows, a center range, avoid aisles, number of seats together, search radius)
- Filter showings by distance from me (browser location) or by a chosen theater
- Search a date range of showtimes and rank showings by how well open seats match
- Show the seat map with matching seats highlighted
- An agent that runs on demand or on a schedule on my PC, checks upcoming showings, and reports when a strong match opens up
- A static demo at `joeydev.net/seat-finder` (saved snapshot of showings, scores, and the latest agent report) with a clear README
- Tests run automatically on every push (GitHub Actions)

Out of scope for v1: accounts, payments or booking, multiple chains, mobile app, push notifications, maps or geocoding services (they need keys and often a card).

## Data source rule

Many theater sites have no public API, and scraping can break their terms of service.

1. Check whether the chosen theater has a documented public API or permitted access
2. If not, build on mock data behind an interface (`IShowtimeProvider`) so a real source can plug in later
3. Never commit API keys or connection strings. Use user secrets locally

## Cost rule

Keep costs at $0.

1. **No cloud billing account.** No Google Cloud, Cloud Run, Cloud Scheduler, or Firebase Blaze plan. The personal site stays on its current plan
2. **Only free plans that need no card:** GitHub, Neon free plan (or local Postgres), Firebase Hosting on the current plan
3. **Claude API: prepaid credits only, auto-reload OFF.** Start with a small amount (about $5). When it runs out, calls fail and nothing more is charged
4. **Nothing public can trigger a paid call.** The live demo is static files; the agent runs only on my PC when I start it
5. Use a cheap model for agent runs (set in config), and check the estimated cost per run before the first real call

## Validation rule

The back end is the real gatekeeper; front-end validation is only for convenience.

1. Every endpoint validates its inputs and rejects bad requests with `400 Bad Request` (or `404 Not Found` for unknown IDs) before doing any work
2. Simple rules live on the models (`[Range]`, `[Required]`); rules that compare fields (from ≤ to) live in small, tested validation methods
3. **The agent's tools validate Claude's arguments the same way** and return an error message to Claude instead of doing the work. Never trust input, whether it comes from a browser or from an AI
4. Every validation rule gets a test with a bad input

## Agent usage guardrails

Layered so that if one fails, the next one stops it.

| Layer | Guardrail | Session |
|---|---|---|
| Account | Prepaid credits, auto-reload off | 14 |
| Per run | Max tool calls, overall timeout, **token budget** (stop when exceeded) | 15 |
| Per day | **Daily run cap** from config (`429 Too Many Requests` when reached) | 16 |
| Visibility | **Tokens used and estimated cost saved on every report** | 16 |
| Access | `/agent/run` only in Development, with rate limiting; schedule off by default | 16, 18 |

## Stack

| Layer | Choice | Why |
|---|---|---|
| Backend | ASP.NET Core Web API (.NET 10 LTS) | Strongest existing skill; typed, testable |
| Front end | React + TypeScript (Vite), `strict: true` | Industry standard for ticketing UIs; learning goal |
| Agent | Anthropic C# SDK, model name from config | Practice building an agent in .NET |
| Storage | PostgreSQL via EF Core (Npgsql), Neon free plan (no card) or local Postgres | What ticketing companies actually run |
| Tests | xUnit, run on every push by GitHub Actions | Scoring, seat search, distance, validation, and agent tool logic; CI is free for public repos |
| Location | Browser geolocation (free, asks permission) + haversine distance in C#; theater coordinates stored in the mock data | No map or geocoding service needed |
| Live demo | Static files (React build + a JSON data snapshot) in a `/seat-finder` folder on the existing joeydev.net Firebase site | No server, no billing account, nothing a visitor can run up |
| Base path | The demo build uses Vite `base: "/seat-finder/"`; front-end calls use relative URLs | Same code works locally and under the path |
| Scheduling | Optional background worker inside the API, running only while the API runs on my PC (off by default) | Free, and the agent only spends credits when I choose |
| AI spend | Anthropic prepaid credits, auto-reload off, cheap model from config | Hard cap on cost |

## Architecture

```
FULL APP (on my PC, free)
  React dev server (localhost:5173)
        |  /health, /showings, /preferences, /agent ...
  ASP.NET Core API (localhost:5166)
    - PreferencesController
    - ShowingsController  -> SeatScoringService, DistanceService
    - AgentController (run now / view last report)  -- Development only
    - optional background worker (agent on an interval, off by default)
    - input validation on every endpoint and every agent tool
        |
  IShowtimeProvider  ->  MockShowtimeProvider (v1) | RealProvider (later)
        |
  Postgres (Neon free plan or local)

  Agent run (only when I start it, within the daily cap):
  - pulls upcoming showings near me via IShowtimeProvider
  - scores them with SeatScoringService
  - asks Claude to summarize the best matches (prepaid credits, token budget)
  - saves an AgentReport with tokens used and estimated cost

LIVE DEMO (static, $0)
  Export snapshot -> web/public/demo/*.json
  Build React in demo mode -> plain files
  Copy into the personal site's /seat-finder folder -> firebase deploy
        |
  joeydev.net              personal site, unchanged
  joeydev.net/seat-finder  static demo: reads the JSON, no server, no paid calls
```

Key idea: the scoring is plain, tested C#. The agent calls tools that wrap that code. Claude decides what to check and writes the summary, but it never invents seat data.

## How we work (Claude Code sessions)

- Start each session by reading `docs/PLAN.md` and the current session's checklist
- Open each session with a short "here's what we're about to build", in OutSystems terms where it helps
- Keep scoring and data logic in plain, tested C#. The agent only calls tools
- No secrets in code or commits
- Follow the cost rule: no billing accounts, no auto-reload, nothing public that spends money
- Follow the validation rule: the back end checks every input, including the agent's
- Every session closes the same way: tests pass → commit → `docs/checklists/session-NN.md` with what was done, what changed from the plan, **what I learned**, and what's next

### Review as I go

This is my first hand-coded project in a while, so understanding the code matters as much as shipping it.

- Before writing code for a checklist item, explain the plan in plain language and **wait for my go-ahead**
- One checklist item at a time. **Stop after each one** so I can read the diff
- After each item, walk me through the important files line by line and relate new concepts to OutSystems where it helps (screens, actions, entities, REST integrations)
- Point out which files are generated boilerplate I can skip and which ones matter
- **I run the commands** (build, run, test, commit); Claude explains what each one does and why
- For core logic marked **I write** (seat search, scoring, distance, the agent loop, and others), describe the approach and let me write it first, then review my version
- Add each new concept to `docs/learning.md` in a sentence or two, in my words when I give them

## Concept map: OutSystems → this stack

| OutSystems | Here |
|---|---|
| Entities | EF Core model classes + migrations |
| Aggregates (and their filters) | LINQ queries (`Where`, `OrderBy`) |
| Server Actions | C# service classes |
| Server-side checks in actions | Validation attributes + validation methods, `400 Bad Request` |
| Expose / Consume REST | ASP.NET controllers / `HttpClient` / `fetch` |
| Screens and Blocks | React pages and components |
| Screen variables, client actions | `useState`, event handlers |
| Timers | Background worker (`BackgroundService`) in the API |
| Site Properties | `appsettings.json` + environment variables |

## Sessions

Each session is one focused step, sized to finish and review in a single sitting. "Learn" is the main concept to come away with.

### Phase 1 – Foundation

#### Session 1 – Repo and API skeleton
- [x] Create repo `seat-finder` with `/api`, `/web`, `/docs/checklists`
- [x] `.gitignore`, `README.md` stub, copy this plan to `docs/PLAN.md`
- [x] ASP.NET Core Web API project with a `GET /health` endpoint
- [x] Close out
- Learn: modern `Program.cs` (top-level statements, minimal hosting, DI registration)

#### Session 2 – Front end skeleton
- [ ] Vite + React + TypeScript project with `strict: true`
- [ ] Vite dev proxy to the API
- [ ] Page that calls `/health` and shows the result — **I write** the typed fetch
- [ ] Close out
- Learn: `tsconfig` strict mode, typing a `fetch` response, `useEffect` + `useState`

#### Session 3 – Test project
- [ ] xUnit test project referencing the API
- [ ] Integration test for `/health` using `WebApplicationFactory`
- [ ] `dotnet test` runs clean from the repo root
- [ ] Close out
- Learn: xUnit basics, arrange/act/assert, in-memory test server

### Phase 2 – Domain and data

#### Session 4 – Domain models
- [ ] Models: `Theater` (name, address, latitude, longitude), `Auditorium`, `Seat` (row, number, isAisle, isAccessible), `Showing`, `SeatStatus` — **I write**
- [ ] `IShowtimeProvider` interface
- [ ] Close out
- Learn: C# records vs classes, nullable reference types, interfaces as seams

#### Session 5 – Mock showtime provider
- [ ] `MockShowtimeProvider`: one chain, 3–4 locations in one metro (real-looking addresses and coordinates), 1–2 auditoriums each with a realistic layout (aisles, center), a week of showings
- [ ] Taken seats generated from a fixed random seed so tests are repeatable
- [ ] `GET /showings?from=&to=` endpoint
- [ ] Validation: `from` ≤ `to`, range at most 14 days, not in the past → otherwise `400`
- [ ] Tests: same seed gives same data; date range filter works; bad date ranges are rejected
- [ ] Close out
- Learn: DI lifetimes, seeded randomness for testable fakes, query-string validation

#### Session 6 – Database and preferences
- [ ] Neon free-plan project (no card), or local Postgres; connection string in user secrets
- [ ] EF Core + Npgsql, `SeatPreference` entity (preferred rows, center range, avoid aisles, party size, search radius, optional home theater), first migration
- [ ] `GET /preferences` and `PUT /preferences`
- [ ] Validation on `PUT`: party size 1–8, rows exist and "from" ≤ "to", center range start ≤ end, radius 1–50 miles, home theater exists
- [ ] Tests for each bad input
- [ ] Close out
- Learn: EF Core `DbContext`, migrations, user secrets, validation attributes and `[ApiController]` automatic `400`s

### Phase 3 – Scoring and search

#### Session 7 – Contiguous seat search
- [ ] Find all blocks of N contiguous open seats in a row — **I write**
- [ ] Tests: full house, party of 1, party larger than any gap, blocks at row ends, aisle splitting a row
- [ ] Close out
- Learn: test-first on a small algorithm; edge cases

#### Session 8 – Scoring
- [ ] `SeatScoringService`: score a block by preferred row, distance from center, aisle rules — **I write**
- [ ] Scoring weights read from config
- [ ] Tests: perfect match, partial match, avoid-aisles on/off, weights change ranking
- [ ] Close out
- Learn: options pattern (`IOptions<T>`), keeping logic pure and testable

#### Session 9 – Ranked showings endpoint
- [ ] `GET /showings/ranked?from=&to=` returns showings sorted by best score, with best seats
- [ ] Same date validation as Session 5
- [ ] Hand-written TypeScript types for the API responses — **I write**
- [ ] Close out
- Learn: LINQ projection to DTOs; TS `type` vs `interface`, union types for `SeatStatus`

#### Session 10 – Location filter
- [ ] `DistanceService`: miles between two coordinates with the haversine formula — **I write**
- [ ] Tests: same point is 0, a known pair of cities matches a published distance (within a small tolerance), symmetry (A→B = B→A)
- [ ] Ranked endpoint accepts `lat`, `lng`, `radiusMiles` **or** `theaterId`; results include `distanceMiles`
- [ ] Validation: latitude −90..90, longitude −180..180, radius 1–50, not both location and theater, theater exists
- [ ] Update the TypeScript types
- [ ] Close out
- Learn: a real formula as plain tested code; filtering with LINQ (an aggregate's filter, in OutSystems terms)

### Phase 4 – Front end

#### Session 11 – Preferences form
- [ ] Form to load and save preferences — **I write**
- [ ] Location control: "Use my location" (browser geolocation) or pick a theater; radius
- [ ] Basic validation (party size ≥ 1, center range valid, radius in range), showing the API's `400` messages too
- [ ] Close out
- Learn: controlled inputs, typed props and event handlers, browser permissions

#### Session 12 – Results list
- [ ] Date range picker + ranked results (date, time, theater, distance, score, best seats)
- [ ] Loading, empty, and error states using a discriminated union — **I write** the state type
- [ ] Close out
- Learn: discriminated unions and type narrowing

#### Session 13 – Seat map
- [ ] Seat map grid for a selected showing: taken, open, matching seats in different colors — **I write**
- [ ] Basic responsive layout
- [ ] Close out
- Learn: rendering grids from data, CSS grid, accessible color choices

### Phase 5 – Agent

#### Session 14 – Agent tools
- [ ] Anthropic Console: buy a small prepaid credit (about $5), confirm **auto-reload is off**, then create the API key
- [ ] Look up current per-token prices; pick a cheap model and estimate cost per run
- [ ] Anthropic C# SDK; API key in user secrets, model name in config
- [ ] Tools `get_preferences`, `get_upcoming_showings` (date range + location/radius), `score_showing` as thin wrappers over existing services
- [ ] **Tools validate Claude's arguments** with the same rules as the endpoints, returning an error message instead of doing the work
- [ ] Tests for each tool wrapper, including invalid arguments (no Claude calls)
- [ ] Close out
- Learn: how tool definitions and JSON schemas work; never trusting AI input

#### Session 15 – Agent loop
- [ ] Loop: send request, run requested tools, return results, repeat until a final summary — **I write** with guidance
- [ ] Guardrails: max tool calls per run, overall timeout, log every tool call
- [ ] **Token budget per run** from config: add up `usage` from each response and stop the run when it passes the budget
- [ ] Test the loop with a fake client, including hitting the tool-call cap and the token budget
- [ ] Close out
- Learn: the agent loop pattern; why Claude never touches seat data directly; metering AI usage

#### Session 16 – Agent reports and endpoints
- [ ] `AgentReport` entity + migration, including **tokens used and estimated cost** per run
- [ ] `POST /agent/run`, mapped only in Development, with rate limiting
- [ ] **Daily run cap** from config: return `429 Too Many Requests` once reached
- [ ] `GET /agent/reports/latest`
- [ ] Tests for the daily cap
- [ ] Close out
- Learn: environment-specific endpoints, ASP.NET rate limiter middleware, usage caps

#### Session 17 – Agent UI panel
- [ ] Panel showing the latest report, its estimated cost, and a Run now button
- [ ] Generic `useApi<T>()` hook, used by the panel and refactored into earlier pages — **I write**
- [ ] Close out
- Learn: TypeScript generics, custom hooks

### Phase 6 – Scheduling

#### Session 18 – Report-worthy logic and local schedule
- [ ] Only save a report when a match beats a score threshold (config) or is new since the last run
- [ ] Optional `BackgroundService` that runs the agent on an interval from config, **off by default**, only while the API runs on my PC, and respecting the daily run cap
- [ ] Tests for threshold and "new since last run"
- [ ] Close out
- Learn: comparing against previous state; background services (the code version of OutSystems Timers)

### Phase 7 – Ship

#### Session 19 – Static demo export
- [ ] Export command that writes a snapshot to `web/public/demo/` (theaters, showings, ranked results for saved preferences from a fixed sample location, latest agent report) as JSON
- [ ] Demo mode in React: reads the JSON instead of calling the API, with a banner "Demo data – run locally for the full app"
- [ ] Demo build uses Vite `base: "/seat-finder/"`; check it with `npm run build` + `npm run preview`
- [ ] Close out
- Learn: build modes and environment variables in Vite, static hosting, base paths

#### Session 20 – Publish and CI
- [ ] GitHub Actions workflow: build and run tests on every push (free for public repos)
- [ ] Copy the demo build into a `seat-finder` folder in the personal site and `firebase deploy` on its current plan (no rewrite, no billing account; check the site's existing rewrites don't catch `/seat-finder`)
- [ ] Smoke test `joeydev.net/seat-finder`, tag `v1`
- [ ] Close out
- Learn: CI basics, how static hosting serves files under a path

### Phase 8 – Polish

#### Session 21 – Typed API boundary
- [ ] Generate TS types from the API's OpenAPI spec (`openapi-typescript`), replace hand-written types
- [ ] Zod validation on API responses
- [ ] Close out
- Learn: TS types vanish at runtime; codegen keeps C# and TS in sync

#### Session 22 – README
- [ ] What it does, why, architecture diagram, how scoring works, how the agent works, how to run locally
- [ ] Section on the AI workflow, linking to `docs/checklists/`
- [ ] Close out

#### Session 23 – Showcase
- [ ] Screenshots or a short GIF
- [ ] Link from joeydev.net and the LinkedIn Featured section
- [ ] "What I'd do next" (real data source, more chains, notifications)
- [ ] Close out

## Interview talking points to capture as I go

- Why scoring is deterministic code and the agent only orchestrates it
- How contiguous-seat search works and its edge cases
- How the distance filter works without any map service (haversine in plain C#)
- How the provider interface keeps the app honest about data access
- Validating every input on the server, including arguments that come from the AI
- How the agent is guarded: tool-call caps, timeouts, token budget, daily run cap, cost logged per run, Development-only endpoint
- Designed to cost $0 to host: static demo, no billing account, paid AI calls only when I trigger them against capped prepaid credits
- How C# models and TS types stay in sync
- What the checklist workflow caught or sped up
- Coming from OutSystems: what low-code abstracts away and what I learned writing it by hand
