# Avi583 — Pinned GitHub Logo (C# / Blazor WebAssembly)

A tiny C# web app (Blazor WebAssembly) that runs entirely in the browser, so it can be hosted on GitHub Pages.

## What it does

- Shows the GitHub logo with **Avi583** below it, as a single card.
- The card is **pinned** to a cork-board background with a red push pin.
- **Click and drag** the card to stretch it; the point you grab follows your cursor.
  With **Snap back** on, it springs back when released; turn it off to keep the stretch.
- **Remove the pin** (click the pin or the button): the pin pops out and the card falls,
  bounces and settles. You can then grab and throw it. **Pin it back** resets everything.

## Project layout

```
Avi583Logo.csproj          .NET 10 Blazor WebAssembly project
Program.cs                 App startup
App.razor                  Router
_Imports.razor             Shared usings
Pages/Home.razor           The page + all C# logic (stretch, spring, gravity, bounce)
wwwroot/index.html         Host page
wwwroot/css/app.css        Cork board, card, pin styles
wwwroot/js/stage.js        requestAnimationFrame loop + resize -> calls into C#
wwwroot/images/            Put github-mark.svg here (see below)
.github/workflows/deploy.yml  Builds and deploys to GitHub Pages
```

## The logo file

The page loads `wwwroot/images/github-mark.svg`. The deploy workflow downloads
GitHub's official mark automatically, so nothing is needed for GitHub Pages.

For local runs, fetch it once:

```bash
mkdir -p wwwroot/images
curl -fsSL https://raw.githubusercontent.com/primer/octicons/main/icons/mark-github-24.svg -o wwwroot/images/github-mark.svg
```

(Or download it from https://github.com/logos.) Without it, a placeholder circle is shown.

## Run locally

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run
```

Then open the URL it prints (e.g. http://localhost:5000).

## Deploy to GitHub Pages

1. Create a repo (e.g. `Avi583/pinned-logo`, or `Avi583/Avi583.github.io` for your main site).
2. Push these files to the `main` branch.
3. In the repo: **Settings → Pages → Build and deployment → Source: GitHub Actions**.
4. The workflow runs on each push. The site appears at
   `https://avi583.github.io/<repo-name>/` (or `https://avi583.github.io/` for a `.github.io` repo).

The workflow sets the correct `<base href>` for your repo name, adds `.nojekyll`
(so the `_framework` folder is served), and copies `index.html` to `404.html`.

## Tweaking

Constants at the top of the `@code` block in `Pages/Home.razor`:

| Constant | Meaning |
|---|---|
| `W`, `H` | Card size in px |
| `Gravity` | Fall acceleration (px/s²) |
| `Bounce` | How bouncy walls/floor are (0–1) |
| `SpringK`, `SpringC` | Stiffness / damping of the snap-back |
| `MinScale`, `MaxScale` | Stretch limits |

Suggested `.gitignore`: `bin/`, `obj/`, `publish/`.
