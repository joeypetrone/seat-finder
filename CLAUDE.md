# Seat Finder – notes for Claude Code

Project conventions for AI-assisted work in this repo. A gitignored `CLAUDE.local.md` may add personal context.

## Commands

Run from the repo root.

```bash
dotnet build                                   # build the solution (SeatFinder.slnx)
dotnet test                                    # run the test projects
dotnet run --project api/SeatFinder.Api        # API on http://localhost:5166
cd web && npm install && npm run dev           # front end on http://localhost:5173 (proxies the API)
cd web && npm run lint && npm run build        # lint and type-check the front end
```

- .NET SDK is pinned to 10 by `global.json`. Don't remove the pin.
- Line endings are LF (`.gitattributes`).

## Workflow

- Branch from an up-to-date `main`, named by feature: `feat/...`, `fix/...`, `chore/...`, `docs/...`.
- Small, logical commits with clear messages. Run build and tests before pushing.
- Open a pull request with `gh pr create`. The description covers what changed and why, which files are worth reading, how to try it locally, and open questions.
- Never merge pull requests or push to `main`. The maintainer reviews and merges.
- Commits use the GitHub noreply email. Check `git config user.email` ends in `users.noreply.github.com` before committing.

## Rules

- **No secrets** in code or commits. Keys and connection strings go in `dotnet user-secrets` locally.
- **Cost: $0.** Never add a service that needs a billing account or card on file. The Claude API runs only on the maintainer's PC, on prepaid credits with auto-reload off. Nothing public may trigger a paid call; the live demo is static files.
- **Validate on the back end.** Every endpoint validates its input and returns 400 with details on bad input. The agent's tools validate the arguments Claude sends, the same way.
- **Agent guardrails.** Caps on tool calls, run time, tokens per run, and runs per day; log the cost of each run.
- **Comments explain why,** not what the language does.

## Layout

```
api/SeatFinder.Api/   ASP.NET Core Web API (.NET 10)
web/                  React + TypeScript (Vite)
tests/                xUnit tests
```
