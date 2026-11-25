# Seat Finder

Find movie showings where the seats you actually want are still open.

Tell it your party size, preferred rows, aisle and accessibility needs, and how far you'll travel. Seat Finder searches each showing for blocks of contiguous open seats, scores them against your preferences, and ranks the showings. An AI agent built on the Claude API can watch showtimes and write up a report when a strong match appears.

**Status:** early development. Mock showtimes, the database, and saved preferences are in place; seat search is next.

## Architecture

```
 Full app (runs locally)                         Live demo (static)
 ┌──────────────┐   /api   ┌──────────────────┐   ┌──────────────────────┐
 │ React + TS   │ ───────▶ │ ASP.NET Core API │   │ React build +        │
 │ (Vite)       │          │  seat search     │   │ JSON snapshot of     │
 └──────────────┘          │  scoring         │   │ ranked showings      │
                           │  distance filter │   │ (no API, no AI calls)│
                           │  agent ──▶ Claude│   └──────────────────────┘
                           └────────┬─────────┘
                                    ▼
                               PostgreSQL
```

## Design decisions

- **Showtime data is mocked** behind a provider interface, so a real ticketing feed could be swapped in without touching search or scoring.
- **Validation lives on the server.** Every endpoint rejects bad input with a 400 and details, including the arguments the agent's tools receive from the model.
- **The agent has hard limits:** capped tool calls, run time, tokens per run, and runs per day, with the cost of each run logged.
- **The public demo can't spend money.** It's a static build over a data snapshot; the agent only runs locally.

## Stack

| Layer | Tech |
|---|---|
| API | ASP.NET Core (.NET 10), EF Core |
| Database | PostgreSQL |
| Web | React, TypeScript, Vite |
| Agent | Anthropic C# SDK (Claude) |
| Tests | xUnit, `WebApplicationFactory` |

## Roadmap

- [x] API and front end skeletons with a health check
- [ ] Test project and CI
- [x] Domain model: theaters, auditoriums, seats, showings
- [x] Mock showtime provider and database
- [ ] Contiguous seat search and scoring
- [ ] Ranked showings endpoint with a location filter
- [ ] Preferences form, results list, and seat map
- [ ] Agent: tools, loop, reports, and UI panel
- [ ] Static demo export and publish

## Run locally

Requires the .NET 10 SDK, Node.js, and PostgreSQL (any recent version running locally).

Point the API at your database (stored in user secrets, outside the repo), then create the tables:

```bash
dotnet user-secrets set "ConnectionStrings:SeatFinder" "Host=localhost;Port=5432;Database=seatfinder;Username=postgres;Password=<your password>" --project api/SeatFinder.Api
dotnet tool restore
dotnet ef database update --project api/SeatFinder.Api   # creates the seatfinder database if needed
```

```bash
dotnet run --project api/SeatFinder.Api    # API on http://localhost:5166
```

The tests use an in-memory database, so `dotnet test` doesn't need Postgres.

```bash
cd web
npm install
npm run dev                                # http://localhost:5173
```

## Layout

```
api/SeatFinder.Api/   ASP.NET Core Web API
web/                  React + TypeScript front end
```
