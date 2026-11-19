# Seat Finder – briefing for Claude

Read this, then `docs/PLAN.md`, then the latest note in `docs/checklists/`, before doing anything else.

## Who I am

- Coming back to hand-written code after several years in **OutSystems** (low-code). I did a C#/.NET + React bootcamp in 2020 and know the basics, not modern patterns. I also know MCP servers.
- **Visual learner.** Show me where things fit (the mockup, a diagram, the seat map) before code.
- Explain new ideas with **OutSystems comparisons** where they help (screens, blocks, actions, entities, aggregates, exposed/consumed REST, site properties, timers).
- I want to learn the way AI-assisted teams work: clear specs, AI builds, small pull requests, careful review. This is a portfolio project for ticketing companies (SeatGeek and others).

## My setup

- Windows 11. Repo at `C:\Users\joeyp\source\repos\seat-finder`, on GitHub at `joeypetrone/seat-finder`
- **Visual Studio 2026** for C# (open `SeatFinder.slnx`; run with the `http` profile on port 5166)
- **VS Code** for the React/TypeScript front end in `web/`
- .NET SDK pinned to 10 by `global.json` (VS also installed SDK 11; don't remove the pin)
- GitHub CLI (`gh`) for pull requests
- Commits use my GitHub noreply email (set in this repo's local git config). Never commit with my Gmail. Check `git config user.email` ends in `users.noreply.github.com` before the first commit of a session.

## How we work: build fast, review per session (PR workflow)

1. **Start of session:** say which session we're on and give a short plain-language overview (point at the matching card in the mockup for front-end sessions). Wait for my go-ahead once, then build.
2. **Branch:** create `session-NN-short-name` from an up-to-date `main` (`git switch main`, `git pull`, `git switch -c ...`).
3. **Build the session end to end:** setup, code, tests. Run the build and tests yourself. Commit in small, logical commits with clear messages.
4. **"I write" items** in the plan (seat search, scoring, distance, the agent loop, and the TypeScript pieces marked so): stop, describe the approach, and let me write it. Review my version, then continue. Don't write these for me unless I ask.
5. **Open a pull request** with `gh pr create` when the session is done. The PR description is my review guide:
   - what changed and why, in plain language
   - **files worth reading** (and which are generated boilerplate I can skip)
   - new concepts, with OutSystems comparisons
   - how to try it locally (commands, URLs)
   - anything you were unsure about
6. **I review on GitHub,** ask questions or request changes, and **I merge.** Never merge PRs or push to `main` yourself.
7. No secrets in code or commits. Keys and connection strings go in user secrets.
8. **Cost rule (important):** keep costs at $0. Never suggest a service that needs a billing account or card on file (no Google Cloud, Cloud Run, Firebase Blaze). Claude API only on prepaid credits with auto-reload off. Nothing public may trigger a paid call. See "Cost rule" in `docs/PLAN.md`.
9. **New ideas** that aren't about safety or cost go on a "v2 ideas" list in the session note, not into the current session.

## Learning log (private, not in the repo)

My learning log lives outside the repo: `C:\Users\joeyp\OneDrive\Documents\2026\Projects\seat-finder-learning.md`. At the end of each session, add the session's new concepts there (a sentence or two each, marked _(draft)_ until I rewrite them in my own words). Never copy it into the repo.

## End of every session (in the PR)

1. Tests pass (once a test project exists)
2. Tick the session's items in `docs/PLAN.md`
3. Write `docs/checklists/session-NN.md`: what was done, what changed from the plan, what's next, v2 ideas
4. **Update the progress board:** in `docs/progress/index.html`, set the finished session's `status` to `"done"` and the next one to `"current"` in the `SESSIONS` array, then republish it with the Artifact tool to the URL below (pass `url`)
5. Add the session's concepts to the private learning log
6. Push the branch and open the PR, then give me the link and what to look at first

Progress board: https://claude.ai/artifact/FaAD9nWNUse3fWkD6dQNVB

## After I merge

Remind me to update my local copy:

```bash
git switch main
git pull
```

## Repo map

```
api/SeatFinder.Api/   ASP.NET Core Web API (.NET 10)
web/                  React + TypeScript (Vite)
docs/PLAN.md          plan and per-session checklists (source of truth)
docs/checklists/      one note per finished session
docs/progress/        source of the progress board artifact
```
