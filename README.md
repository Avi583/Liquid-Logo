# Pinned GitHub Logo C# Demo

**▶ Try it live: https://avi583.github.io/Liquid-Logo/**

An interactive GitHub logo card pinned to a cork board. Stretch it, unpin it, and throw it around. It runs entirely in your browser.

## How to play

- **Stretch:** click and drag the card. The point you grab follows your cursor.
- **Snap back:** with **Snap back** on, the card springs back when you let go. Turn it off and the stretch stays.
- **Unpin:** click the red pin (or the button). The pin pops out and the card falls, bounces and settles. Then you can grab it and throw it.
- **Right-click:** pins the card anywhere on the board. If it's already pinned, the pin moves there and the card swings into place.
- **Pin it back:** puts everything back the way it started.

## Run it yourself

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
mkdir -p wwwroot/images
curl -fsSL https://raw.githubusercontent.com/primer/octicons/main/icons/mark-github-24.svg -o wwwroot/images/github-mark.svg
dotnet run
```

Then open the URL it prints (e.g. http://localhost:5000).

## Built with

C# and Blazor WebAssembly, hosted on GitHub Pages.

## License

See [LICENSE](LICENSE).
