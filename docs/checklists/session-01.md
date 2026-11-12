# Session 1 – Repo and API skeleton

**Date:** 2026-11-11

## Checklist

- [x] Create repo `seat-finder` with `/api`, `/web`, `/docs/checklists`
- [x] `.gitignore`, `README.md` stub, copy plan to `docs/PLAN.md`
- [x] ASP.NET Core Web API project with a `GET /health` endpoint
- [x] Close out (build clean, `/health` returns `200 Healthy`, commit)

## What was done

- Solution `SeatFinder.slnx` at the repo root (new XML solution format in .NET 10)
- `api/SeatFinder.Api` created from the `webapi` template with controllers; weather sample removed
- `/health` uses ASP.NET's built-in health checks (`AddHealthChecks` + `MapHealthChecks`) instead of a hand-written controller
- OpenAPI document available at `/openapi/v1.json` in Development (for TS codegen in Session 20)
- `public partial class Program { }` added so Session 3's tests can host the app

## What changed from the plan

- Repo lives in `C:\Users\joeyp\source\repos`, not OneDrive, to avoid sync issues with `.git`, `bin/obj`, and `node_modules`
- Installed the .NET 10 SDK and updated Git (2.24 → 2.56) before starting
- Removed `UseHttpsRedirection`: Cloud Run terminates HTTPS in front of the container

## What I learned

- `Program.cs` uses top-level statements: the file is the entry point, no `Main` or `Startup` class
- Three parts: register services (`builder.Services`), build, then configure middleware and map endpoints (`app.*`). Middleware order matters
- Built-in features (health checks, OpenAPI) are a one-line registration plus a one-line mapping

## What's next

Session 2 – Vite + React + TypeScript front end with `strict: true`, a dev proxy to the API, and a typed `fetch` to `/health`.
