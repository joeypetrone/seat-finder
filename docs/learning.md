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

## Session 2 – Front end skeleton

- **Vite** _(draft)_: the front-end dev server and build tool. `npm run dev` serves the app at localhost:5173 and refreshes the page on every save (live preview); `npm run build` type-checks and bundles it into `dist/` (the compile part of 1-Click Publish).
- **`package.json` / `node_modules`** _(draft)_: `package.json` lists the project's scripts and dependencies; `npm install` downloads them into `node_modules/` (git-ignored). Run `npm` commands from the folder that has `package.json`.
- **React component and JSX** _(draft)_: a function that returns the UI, like a Screen or Block. JSX is the HTML-looking syntax inside it; curly braces `{ }` switch back to TypeScript, like an Expression widget.
- **`useState`** _(draft)_: a screen local variable. `const [status, setStatus] = useState<ApiStatus>('checking')` gives the value and a setter; calling the setter is an Assign that also redraws the screen.
- **`useEffect(() => { ... }, [])`** _(draft)_: runs once after the first draw, like OnReady. The effect can't be `async`, so declare an `async` function inside it and call it.
- **`fetch` and `Response`** _(draft)_: `fetch` is a consumed REST call. It returns an envelope: check `response.ok`, then read the body with `await response.text()` (plain text) or `.json()` (JSON). It throws if the server can't be reached, so wrap it in `try`/`catch`.
- **Union type** _(draft)_: `type ApiStatus = 'checking' | 'healthy' | 'down'` allows only those three values, like a Static Entity. Typos become compile errors.
- **`Record<K, V>`** _(draft)_: a lookup table type. `Record<ApiStatus, string>` requires exactly one label per status.
- **TypeScript strict mode** _(draft)_: extra checks (no implicit `any`, no unchecked `null`, unused variables) that catch mistakes while typing, like TrueChange. On by default in TypeScript 6; written out in `tsconfig.app.json` anyway.
- **Vite dev proxy** _(draft)_: in `vite.config.ts`, forwards `/health` from :5173 to the API on :5166, like a consumed REST base URL. The browser only talks to one origin, so no CORS setup is needed. Dev only.
- **ESLint** _(draft)_: checks for likely mistakes that still compile (like TrueChange warnings), including React hooks rules. `npm run lint`.
- **`index.css`** _(draft)_: app-wide styles, like the Theme's style sheet. CSS variables (`--text`) work like Theme colors.
