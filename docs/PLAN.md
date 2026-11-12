# Seat Finder – Project Plan and Checklist (v2)

A small app that finds movie showings where our preferred row or view is still open, with an agent that watches showtimes and flags a match. Built in C#/.NET with a React + TypeScript front end, using a checklist-driven, session-based AI workflow.

_Revised 2026-11-11: smaller sessions, a "learn by building" working style, Postgres instead of SQLite, and a single-container deploy._

## Why this project

- Portfolio piece for ticketing and entertainment roles (SeatGeek, Vivid Seats, StubHub, Fandango)
- Refresh C#/.NET and React, and learn TypeScript properly
- Real practice building an agent in C#/.NET with the Anthropic C# SDK
- Shows the checklist + planned session workflow in a public repo
- Deploys to joeydev.net

## Scope for v1

Keep it small and finished.

- One theater (or one chain) as the data source
- Save seat preferences (rows, a center range, avoid aisles, number of seats together)
- Search a date range of showtimes and rank showings by how well open seats match
- Show the seat map with matching seats highlighted
- An agent that runs on a schedule, checks upcoming showings, and reports when a strong match opens up
- Live on joeydev.net with a clear README

Out of scope for v1: accounts, payments or booking, multiple chains, mobile app, push notifications.

## Data source rule

Many theater sites have no public API, and scraping can break their terms of service.

1. Check whether the chosen theater has a documented public API or permitted access
2. If not, build on mock data behind an interface (`IShowtimeProvider`) so a real source can plug in later
3. Never commit API keys or connection strings. Use user secrets locally and environment variables in deployment

## Stack

| Layer | Choice | Why |
|---|---|---|
| Backend | ASP.NET Core Web API (.NET 10 LTS) | Strongest existing skill; typed, testable |
| Front end | React + TypeScript (Vite), `strict: true` | Industry standard for ticketing UIs; learning goal |
| Agent | Anthropic C# SDK, model name from config | Practice building an agent in .NET |
| Storage | PostgreSQL via EF Core (Npgsql), hosted on Neon free tier | Survives container restarts; what ticketing companies actually run |
| Tests | xUnit | Scoring, seat search, provider, and agent tool logic |
| Hosting | One container on Cloud Run (API also serves the React build); joeydev.net routes to it via a Firebase Hosting rewrite | One thing to deploy instead of two |
| Scheduling | Cloud Scheduler calls `POST /agent/run` with a shared-secret header | Cloud Run scales to zero, so an in-process timer won't fire reliably |

Neon setup: one `dev` branch for local work, one `prod` branch for deployment.

## Architecture

```
Browser
   |
joeydev.net (Firebase Hosting rewrite)
   |
Cloud Run container
  ASP.NET Core
    - serves React build (static files)
    - PreferencesController
    - ShowingsController  -> SeatScoringService
    - AgentController (run now / view last report)
        |
  IShowtimeProvider  ->  MockShowtimeProvider (v1) | RealProvider (later)
        |
  Postgres on Neon (preferences, agent reports)

Cloud Scheduler --(shared secret)--> POST /agent/run
  Agent run:
  - pulls upcoming showings via IShowtimeProvider
  - scores them with SeatScoringService
  - asks Claude to summarize the best matches in plain language
  - saves an AgentReport
```

Key idea: the scoring is plain, tested C#. The agent calls tools that wrap that code. Claude decides what to check and writes the summary, but it never invents seat data.

## How we work (Claude Code sessions)

- Start each session by reading `docs/PLAN.md` and the current session's checklist
- Open each session with a short "here's what we're about to build", in OutSystems terms where it helps
- Keep scoring and data logic in plain, tested C#. The agent only calls tools
- No secrets in code or commits
- Every session closes the same way: tests pass → commit → `docs/checklists/session-NN.md` with what was done, what changed from the plan, **what I learned**, and what's next

### Review as I go

This is my first hand-coded project in a while, so understanding the code matters as much as shipping it.

- Before writing code for a checklist item, explain the plan in plain language and **wait for my go-ahead**
- One checklist item at a time. **Stop after each one** so I can read the diff
- After each item, walk me through the important files line by line and relate new concepts to OutSystems where it helps (screens, actions, entities, REST integrations)
- Point out which files are generated boilerplate I can skip and which ones matter
- **I run the commands** (build, run, test, commit); Claude explains what each one does and why
- For core logic marked **I write** (seat search, scoring, the agent loop, and others), describe the approach and let me write it first, then review my version
- Add each new concept to `docs/learning.md` in a sentence or two, in my words when I give them

## Concept map: OutSystems → this stack

| OutSystems | Here |
|---|---|
| Entities | EF Core model classes + migrations |
| Aggregates | LINQ queries |
| Server Actions | C# service classes |
| Expose / Consume REST | ASP.NET controllers / `HttpClient` / `fetch` |
| Screens and Blocks | React pages and components |
| Screen variables, client actions | `useState`, event handlers |
| Timers | Cloud Scheduler → `/agent/run` |
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
- [ ] Models: `Theater`, `Auditorium`, `Seat` (row, number, isAisle, isAccessible), `Showing`, `SeatStatus` — **I write**
- [ ] `IShowtimeProvider` interface
- [ ] Close out
- Learn: C# records vs classes, nullable reference types, interfaces as seams

#### Session 5 – Mock showtime provider
- [ ] `MockShowtimeProvider`: 2 auditoriums, realistic layout (aisles, center), a week of showings
- [ ] Taken seats generated from a fixed random seed so tests are repeatable
- [ ] `GET /showings?from=&to=` endpoint
- [ ] Tests: same seed gives same data; date range filter works
- [ ] Close out
- Learn: DI lifetimes, seeded randomness for testable fakes

#### Session 6 – Database and preferences
- [ ] Neon project with `dev` branch; connection string in user secrets
- [ ] EF Core + Npgsql, `SeatPreference` entity (preferred rows, center range, avoid aisles, party size), first migration
- [ ] `GET /preferences` and `PUT /preferences`
- [ ] Close out
- Learn: EF Core `DbContext`, migrations, user secrets

### Phase 3 – Seat scoring

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
- [ ] Hand-written TypeScript types for the API responses — **I write**
- [ ] Close out
- Learn: LINQ projection to DTOs; TS `type` vs `interface`, union types for `SeatStatus`

### Phase 4 – Front end

#### Session 10 – Preferences form
- [ ] Form to load and save preferences — **I write**
- [ ] Basic validation (party size ≥ 1, center range valid)
- [ ] Close out
- Learn: controlled inputs, typed props and event handlers

#### Session 11 – Results list
- [ ] Date range picker + ranked results (date, time, score, best seats)
- [ ] Loading, empty, and error states using a discriminated union — **I write** the state type
- [ ] Close out
- Learn: discriminated unions and type narrowing

#### Session 12 – Seat map
- [ ] Seat map grid for a selected showing: taken, open, matching seats in different colors — **I write**
- [ ] Basic responsive layout
- [ ] Close out
- Learn: rendering grids from data, CSS grid, accessible color choices

### Phase 5 – Agent

#### Session 13 – Agent tools
- [ ] Anthropic C# SDK; API key in user secrets, model name in config
- [ ] Tools `get_preferences`, `get_upcoming_showings`, `score_showing` as thin wrappers over existing services
- [ ] Tests for each tool wrapper (no Claude calls)
- [ ] Close out
- Learn: how tool definitions and JSON schemas work

#### Session 14 – Agent loop
- [ ] Loop: send request, run requested tools, return results, repeat until a final summary — **I write** with guidance
- [ ] Guardrails: max tool calls per run, overall timeout, log every tool call
- [ ] Test the loop with a fake client
- [ ] Close out
- Learn: the agent loop pattern; why Claude never touches seat data directly

#### Session 15 – Agent reports and endpoints
- [ ] `AgentReport` entity + migration
- [ ] `POST /agent/run` protected by a shared-secret header and rate limiting
- [ ] `GET /agent/reports/latest`
- [ ] Close out
- Learn: ASP.NET rate limiter middleware, simple endpoint protection

#### Session 16 – Agent UI panel
- [ ] Panel showing the latest report and a Run now button
- [ ] Generic `useApi<T>()` hook, used by the panel and refactored into earlier pages — **I write**
- [ ] Close out
- Learn: TypeScript generics, custom hooks

### Phase 6 – Scheduling

#### Session 17 – Report-worthy logic
- [ ] Only save a report when a match beats a score threshold (config) or is new since the last run
- [ ] Tests for threshold and "new since last run"
- [ ] Close out
- Learn: comparing against previous state; config-driven thresholds

### Phase 7 – Ship

#### Session 18 – Containerize
- [ ] API serves the React build as static files with an SPA fallback
- [ ] Multi-stage Dockerfile (build web → build API → runtime image)
- [ ] Run the container locally against the Neon `dev` branch
- [ ] Close out
- Learn: Docker basics, multi-stage builds

#### Session 19 – Deploy
- [ ] Neon `prod` branch; migrations applied
- [ ] Deploy to Cloud Run; secrets as environment variables
- [ ] Firebase Hosting rewrite on joeydev.net to the Cloud Run service
- [ ] Cloud Scheduler job calling `/agent/run` with the shared secret
- [ ] Smoke test in production, tag `v1`
- [ ] Close out
- Learn: environment config, the request path from domain to container

### Phase 8 – Polish

#### Session 20 – Typed API boundary
- [ ] Generate TS types from the API's OpenAPI spec (`openapi-typescript`), replace hand-written types
- [ ] Zod validation on API responses
- [ ] Close out
- Learn: TS types vanish at runtime; codegen keeps C# and TS in sync

#### Session 21 – README
- [ ] What it does, why, architecture diagram, how scoring works, how the agent works, how to run locally
- [ ] Section on the AI workflow, linking to `docs/checklists/`
- [ ] Close out

#### Session 22 – Showcase
- [ ] Screenshots or a short GIF
- [ ] Link from joeydev.net and the LinkedIn Featured section
- [ ] "What I'd do next" (real data source, more chains, notifications)
- [ ] Close out

## Interview talking points to capture as I go

- Why scoring is deterministic code and the agent only orchestrates it
- How contiguous-seat search works and its edge cases
- How the provider interface keeps the app honest about data access
- How the agent is guarded (tool-call caps, timeouts, protected endpoint, rate limits)
- Why the scheduler lives outside the container on a scale-to-zero host
- How C# models and TS types stay in sync
- What the checklist workflow caught or sped up
- Coming from OutSystems: what low-code abstracts away and what I learned writing it by hand
