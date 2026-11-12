# Learning log

New concepts, a sentence or two each. Drafts marked _(draft)_ were written by Claude; rewrite them in my own words once they make sense.

## Session 1 – Repo and API skeleton

- **Git repo and commit** _(draft)_: a folder whose changes Git tracks. A commit is a saved version with a message, like a version in Service Studio's history.
- **Solution and project** _(draft)_: the `.slnx` solution groups projects; each `.csproj` is one buildable app or library, roughly like an OutSystems application and its modules.
- **`dotnet build` / `dotnet run`** _(draft)_: build compiles the code and reports errors (the compile part of 1-Click Publish); run starts the app locally at `http://localhost:5166`.
- **`Program.cs`**: handles the initial code needed for the module logic to run. It initializes the builder of the app, adds dependencies, and runs checks needed before the run.
  - Clarified: the `app.Use...()` steps (middleware) run on *every request*, not once at startup. `app.Map...()` lines connect URLs to code, and `app.Run()` starts listening.
- **Endpoint** _(draft)_: a URL the app answers, like an exposed REST method. `/health` just answers "Healthy" so hosts can check the app is alive.
- **`appsettings.json`** _(draft)_: settings kept outside the code, like Site Properties.
- **`.gitignore`** _(draft)_: files Git should never save, such as build output and secrets.
