# Seat Finder – briefing for Claude

Read this, then `docs/PLAN.md`, then the latest note in `docs/checklists/`, before doing anything else.

## Who I am

- Coming back to hand-written code after several years in **OutSystems** (low-code). I did a C#/.NET + React bootcamp in 2020 and know the basics, not modern patterns. I also know MCP servers.
- **Visual learner.** Show me where things fit (the mockup, a diagram, the seat map) before code.
- Explain new ideas with **OutSystems comparisons** where they help (screens, blocks, actions, entities, aggregates, exposed/consumed REST, site properties, timers).
- Understanding the code matters as much as shipping it. This is a portfolio project for ticketing companies (SeatGeek and others).

## My setup

- Windows 11. Repo at `C:\Users\joeyp\source\repos\seat-finder`, on GitHub at `joeypetrone/seat-finder`
- **Visual Studio 2026** for C# (open `SeatFinder.slnx`; run with the `http` profile on port 5166)
- **VS Code** for the React/TypeScript front end in `web/`
- .NET SDK pinned to 10 by `global.json` (VS also installed SDK 11; don't remove the pin)
- Commits use my GitHub noreply email (set in this repo's local git config). Never commit with my Gmail.

## How we work

Follow "How we work" and "Review as I go" in `docs/PLAN.md`. The short version:

1. **Start of session:** say which session we're on, give a plain-language overview of what we'll build (point at the matching card in the mockup for front-end sessions), and wait for my go-ahead.
2. **One checklist item at a time.** Explain the plan, wait for my OK, do it, then stop so I can read the diff.
3. **After each item:** walk me through the important files, and say which files are generated boilerplate I can skip.
4. **I run the commands** (build, run, test, git). Give each command in its own block with one line on what it does. Remind me to run them from the `seat-finder` folder.
5. **Core logic marked "I write"** in the plan: describe the approach, let me write it, then review my version. Don't write it for me unless I ask.
6. **New concepts** go in `docs/learning.md`, in my words when I give them. Mark your own drafts _(draft)_.
7. No secrets in code or commits. Keys and connection strings go in user secrets.
8. **Cost rule (important):** keep costs at $0. Never suggest a service that needs a billing account or card on file (no Google Cloud, Cloud Run, Firebase Blaze). Claude API only on prepaid credits with auto-reload off. Nothing public may trigger a paid call. See "Cost rule" in `docs/PLAN.md`.

## End of every session

1. Tests pass (once a test project exists)
2. Tick the session's items in `docs/PLAN.md`
3. Write `docs/checklists/session-NN.md`: what was done, what changed from the plan, what I learned, what's next
4. **Update the progress board:** in `docs/progress/index.html`, set the finished session's `status` to `"done"` and the next one to `"current"` in the `SESSIONS` array, then republish it with the Artifact tool to the URL below (pass `url`)
5. Give me the commit commands to run

Progress board: https://claude.ai/artifact/FaAD9nWNUse3fWkD6dQNVB

## Repo map

```
api/SeatFinder.Api/   ASP.NET Core Web API (.NET 10)
web/                  React + TypeScript (Vite) – from Session 2
docs/PLAN.md          plan and per-session checklists (source of truth)
docs/learning.md      my learning log
docs/checklists/      one note per finished session
docs/progress/        source of the progress board artifact
```
