# Directory

Every file in the project (build output in `bin/`, `obj/` and `publish/` is excluded).

```
.github/workflows/deploy.yml   Builds the app and deploys it to GitHub Pages
.gitignore                     Ignored paths
App.razor                      Router
Avi583Logo.csproj              .NET 10 Blazor WebAssembly project
CLAUDE.md                      Claude instructions (git-ignored)
DIRECTORY.md                   This file
LICENSE                        License
Pages/Home.razor               Page markup (cork board, card, pin, toolbar)
Pages/Home.razor.cs            Page code-behind: pointer input, JS loop interop, rendering styles
Physics/Angles.cs              Angle helpers (Rotate, Wrap, Nearest)
Physics/CardPhysics.cs         Card state and physics (stretch, spring, swing, gravity, bounce) and tuning constants
Program.cs                     App startup
README.md                      User-facing readme
To-Do-List.txt                 Ideas and planned work
_Imports.razor                 Shared usings
wwwroot/css/app.css            Cork board, card and pin styles
wwwroot/index.html             Host page
wwwroot/js/stage.js            requestAnimationFrame loop and resize handling; calls into C#
```

Not in the repo: `wwwroot/images/github-mark.svg` is downloaded by the deploy workflow (or by hand for local runs).
