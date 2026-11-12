# Seat Finder

Finds movie showings where your preferred seats are still open, and runs an agent that watches showtimes and flags a strong match.

**Status:** in progress. See [docs/PLAN.md](docs/PLAN.md) for the plan and [docs/checklists/](docs/checklists/) for session notes.

## Stack

- API: ASP.NET Core (.NET 10)
- Web: React + TypeScript (Vite)
- Agent: Anthropic C# SDK
- Database: PostgreSQL (EF Core)

## Layout

```
api/    ASP.NET Core Web API
web/    React + TypeScript front end
docs/   Plan and per-session checklists
```

## Run locally

_Coming in later sessions._
