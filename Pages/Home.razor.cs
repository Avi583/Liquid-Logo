using System.Diagnostics;
using System.Globalization;
using Avi583Logo.Physics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Avi583Logo.Pages;

/// <summary>Page plumbing: pointer input, JS animation loop and rendering. Physics lives in <see cref="CardPhysics"/>.</summary>
public partial class Home : IAsyncDisposable
{
    const double W = CardPhysics.W, H = CardPhysics.H, PinOffset = CardPhysics.PinOffset;

    [Inject] IJSRuntime JS { get; set; } = default!;

    readonly CardPhysics body = new();
    DotNetObjectReference<Home>? selfRef;
    bool ready;
    int dropCount;                                       // re-keys the falling pin so its animation restarts

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        selfRef = DotNetObjectReference.Create(this);
        var size = await JS.InvokeAsync<double[]>("stage.init", selfRef);
        body.Resize(size[0], size[1]);
        ready = true;
        StateHasChanged();
    }

    // ---------- Rendering ----------

    string CardStyle => body.Pinned
        // Pinned: scale around the pin point so the pin stays put while stretching
        ? $"width:{F(W)}px;height:{F(H)}px;" +
          $"transform-origin:{F(W / 2)}px {F(PinOffset)}px;" +
          $"transform:translate({F(body.PinX - W / 2)}px,{F(body.PinY - PinOffset)}px) rotate({F(body.Angle)}deg) scale({F(body.Sx)},{F(body.Sy)})"
        // Free: position by centre, rotate and scale around centre
        : $"width:{F(W)}px;height:{F(H)}px;transform-origin:50% 50%;" +
          $"transform:translate({F(body.X - W / 2)}px,{F(body.Y - H / 2)}px) rotate({F(body.Angle)}deg) scale({F(body.Sx)},{F(body.Sy)})";

    static string F(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

    static double Now => Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;

    // ---------- Input ----------

    void OnCardPointerDown(PointerEventArgs e) => body.Grab(e.ClientX, e.ClientY, Now);

    void OnPointerMove(PointerEventArgs e) => body.DragTo(e.ClientX, e.ClientY, Now);

    void OnPointerUp(PointerEventArgs e)
    {
        if (body.Release(Now)) StartLoop();
    }

    // ---------- Actions ----------

    void Unpin()
    {
        if (!body.Unpin()) return;
        dropCount++;
        StartLoop();
    }

    void Repin() => body.Repin();

    void ResetStretch()
    {
        body.ResetStretch();
        StartLoop();
    }

    void ToggleElastic(ChangeEventArgs e)
    {
        body.Elastic = e.Value is true;
        if (body.Elastic) StartLoop();
    }

    void StartLoop()
    {
        if (ready) _ = JS.InvokeVoidAsync("stage.start");
    }

    // ---------- Called from JS (wwwroot/js/stage.js) ----------

    [JSInvokable]
    public bool Tick(double dt)
    {
        bool more = body.Tick(dt);
        StateHasChanged();
        return more;
    }

    [JSInvokable]
    public void OnResize(double w, double h)
    {
        body.Resize(w, h);
        if (!body.Pinned) StartLoop();
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        try { await JS.InvokeVoidAsync("stage.dispose"); }
        catch (JSDisconnectedException) { }
        selfRef?.Dispose();
    }
}
