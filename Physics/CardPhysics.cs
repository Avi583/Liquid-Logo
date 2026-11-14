using static Avi583Logo.Physics.Angles;

namespace Avi583Logo.Physics;

public enum DragMode { None, Stretch, Carry }

/// <summary>
/// State and physics of the logo card: stretching and swinging while pinned,
/// carrying, throwing, falling and bouncing once the pin is pulled.
/// Pure C#, no Blazor dependencies. Times are in seconds, distances in CSS px.
/// </summary>
public class CardPhysics
{
    // ---- Card geometry (CSS px) ----
    public const double W = 240, H = 290;
    public const double PinOffset = 22;            // pin sits this far below the card's top edge

    // ---- Physics tuning ----
    const double Gravity = 2400;                   // px/s²
    const double Bounce = 0.35;                    // restitution on walls/floor
    const double SpringK = 220, SpringC = 11;
    const double MinScale = 0.35, MaxScale = 4;
    const double SwingK = 60, SwingC = 3.5;        // pendulum spring pulling a pinned card back to hanging straight
    const double TurnRate = 10;                    // how quickly a carried card turns to face its drag direction (1/s)

    // Viewport
    public double ViewW { get; private set; } = 1024;
    public double ViewH { get; private set; } = 768;

    // Pin state
    public bool Pinned { get; private set; } = true;
    public double PinX { get; private set; }
    public double PinY { get; private set; }
    public double HoleX { get; private set; }
    public double HoleY { get; private set; }
    bool customPin;                                // pin placed by the user (right-click) rather than the default spot

    // Stretch (scale) state, with a spring back to 1
    public bool Elastic { get; set; } = true;
    bool forceRelax;
    public double Sx { get; private set; } = 1;
    public double Sy { get; private set; } = 1;
    double vsx, vsy;

    // Free-body state when unpinned (centre of card, angle in degrees)
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Angle { get; private set; }
    double vx, vy, va;

    // Drag state
    public DragMode Mode { get; private set; } = DragMode.None;
    double grabLocalX, grabLocalY, grabSx, grabSy, grabDist;
    double carryLocalX, carryLocalY, lastPx, lastPy, lastTime, throwVx, throwVy;

    static double ClampScale(double v) =>
        double.IsFinite(v) ? Math.Clamp(v, MinScale, MaxScale) : MinScale;

    // ---------- Setup ----------

    public void Resize(double w, double h)
    {
        ViewW = w;
        ViewH = h;
        if (!Pinned) return;
        if (customPin)
        {
            PinX = Math.Clamp(PinX, 0, ViewW);
            PinY = Math.Clamp(PinY, 0, ViewH);
        }
        else PlacePin();
    }

    void PlacePin()
    {
        customPin = false;
        PinX = ViewW / 2;
        PinY = Math.Max(70, ViewH * 0.2);
    }

    // ---------- Dragging ----------

    /// <summary>Pointer pressed on the card at (px, py); `time` in seconds.</summary>
    public void Grab(double px, double py, double time)
    {
        if (Pinned)
        {
            // Remember the grabbed point in unrotated, unscaled card space (relative to the pin)
            Mode = DragMode.Stretch;
            Angle = Wrap(Angle);
            va = 0;
            grabSx = Sx;
            grabSy = Sy;
            double gx = px - PinX, gy = py - PinY;
            grabDist = Math.Sqrt(gx * gx + gy * gy);
            var l = Rotate(gx, gy, -Angle);
            grabLocalX = l.X / Sx;
            grabLocalY = l.Y / Sy;
            vsx = vsy = 0;
            forceRelax = false;
        }
        else
        {
            // Remember the grabbed point in card space so it stays under the cursor while the card turns
            Mode = DragMode.Carry;
            var l = Rotate(px - X, py - Y, -Angle);
            carryLocalX = l.X / Sx;
            carryLocalY = l.Y / Sy;
            vx = vy = va = 0;
            throwVx = throwVy = 0;
            lastPx = px;
            lastPy = py;
            lastTime = time;
        }
    }

    /// <summary>Pointer moved to (px, py) while dragging; `time` in seconds.</summary>
    public void DragTo(double px, double py, double time)
    {
        switch (Mode)
        {
            case DragMode.Stretch:
            {
                double nx = px - PinX;
                double ny = py - PinY;

                if (grabDist > 30)
                {
                    // Stretch lengthwise (thinning a little) with the pull distance,
                    // then swing around the pin so the grabbed point points at the cursor
                    double k = Math.Sqrt(nx * nx + ny * ny) / grabDist;
                    Sy = ClampScale(grabSy * k);
                    Sx = ClampScale(grabSx / Math.Sqrt(Math.Max(k, 1e-3)));

                    double lx = grabLocalX * Sx, ly = grabLocalY * Sy;
                    if (lx * lx + ly * ly > 1)
                        Angle = Nearest((Math.Atan2(ny, nx) - Math.Atan2(ly, lx)) * Deg, Angle);
                }
                else
                {
                    // Grabbed right next to the pin: too close to steer, just stretch
                    var n = Rotate(nx, ny, -Angle);
                    Sx = ClampScale(grabSx + (Math.Abs(n.X) - Math.Abs(grabLocalX * grabSx)) / (W / 2));
                    Sy = ClampScale(grabSy + (n.Y - grabLocalY * grabSy) / (H - PinOffset));
                }
                break;
            }
            case DragMode.Carry:
            {
                double dt = Math.Max(time - lastTime, 0.001);
                throwVx = throwVx * 0.5 + (px - lastPx) / dt * 0.5;
                throwVy = throwVy * 0.5 + (py - lastPy) / dt * 0.5;
                lastPx = px;
                lastPy = py;
                lastTime = time;

                // Turn so the top of the logo faces the direction of the drag
                double speed = Math.Sqrt(throwVx * throwVx + throwVy * throwVy);
                if (speed > 150)
                {
                    double target = Nearest(Math.Atan2(throwVx, -throwVy) * Deg, Angle);
                    Angle += (target - Angle) * (1 - Math.Exp(-TurnRate * dt));
                }

                // Keep the grabbed point under the cursor
                var o = Rotate(carryLocalX * Sx, carryLocalY * Sy, Angle);
                X = px - o.X;
                Y = py - o.Y;
                break;
            }
        }
    }

    /// <summary>Pointer released; returns true if a drag ended (the animation loop should run).</summary>
    public bool Release(double time)
    {
        if (Mode == DragMode.Stretch)
        {
            Mode = DragMode.None;
            Angle = Wrap(Angle);                          // swing back to hanging straight
            return true;
        }
        if (Mode == DragMode.Carry)
        {
            Mode = DragMode.None;
            if (time - lastTime > 0.08) throwVx = throwVy = 0;   // held still before letting go
            vx = Math.Clamp(throwVx, -3000, 3000);
            vy = Math.Clamp(throwVy, -3000, 3000);
            va = vx * 0.08;
            return true;
        }
        return false;
    }

    // ---------- Actions ----------

    /// <summary>Pull the pin out. Returns false if it was already out.</summary>
    public bool Unpin()
    {
        if (!Pinned) return false;
        Pinned = false;
        Mode = DragMode.None;
        HoleX = PinX;
        HoleY = PinY;

        // Convert pinned pose (rotated and scaled around the pin) into a centre position
        var c = Rotate(0, (H / 2 - PinOffset) * Sy, Angle);
        X = PinX + c.X;
        Y = PinY + c.Y;
        vx = 0;
        vy = -120;                                       // tiny hop as the pin pops out
        Angle = Wrap(Angle);
        va += (Random.Shared.NextDouble() - 0.5) * 90;
        forceRelax = true;
        return true;
    }

    public void Repin()
    {
        Pinned = true;
        Mode = DragMode.None;
        Sx = Sy = 1;
        vsx = vsy = 0;
        vx = vy = va = Angle = 0;
        forceRelax = false;
        PlacePin();
    }

    /// <summary>Push the pin in at (px, py) and hang the card from it, keeping its tilt so it swings into place.</summary>
    public void PinAt(double px, double py)
    {
        Pinned = true;
        customPin = true;
        Mode = DragMode.None;
        PinX = Math.Clamp(px, 0, ViewW);
        PinY = Math.Clamp(py, 0, ViewH);
        Angle = Wrap(Angle);
        vx = vy = 0;
        va = 0;
    }

    public void ResetStretch() => forceRelax = true;

    // ---------- Animation ----------

    /// <summary>Advance the simulation by `dt` seconds. Returns true while anything is still moving.</summary>
    public bool Tick(double dt)
    {
        bool more = false;

        // Spring the stretch back to normal size
        if (Mode != DragMode.Stretch && (Elastic || forceRelax))
        {
            vsx += (-SpringK * (Sx - 1) - SpringC * vsx) * dt;
            vsy += (-SpringK * (Sy - 1) - SpringC * vsy) * dt;
            Sx = Math.Max(0.1, Sx + vsx * dt);
            Sy = Math.Max(0.1, Sy + vsy * dt);

            if (Math.Abs(Sx - 1) < 1e-3 && Math.Abs(Sy - 1) < 1e-3 &&
                Math.Abs(vsx) < 1e-2 && Math.Abs(vsy) < 1e-2)
            {
                Sx = Sy = 1;
                vsx = vsy = 0;
                forceRelax = false;
            }
            else more = true;
        }

        // Pinned card swings back to hanging straight down
        if (Pinned && Mode != DragMode.Stretch && (Angle != 0 || va != 0))
        {
            va += (-SwingK * Angle - SwingC * va) * dt;
            Angle += va * dt;
            if (Math.Abs(Angle) < 0.05 && Math.Abs(va) < 0.5)
                Angle = va = 0;
            else more = true;
        }

        // Falling / bouncing when unpinned
        if (!Pinned && Mode != DragMode.Carry)
            more |= StepBody(dt);

        return more;
    }

    bool StepBody(double dt)
    {
        vy += Gravity * dt;
        va *= Math.Pow(0.6, dt);
        X += vx * dt;
        Y += vy * dt;
        Angle += va * dt;

        // Half extents of the rotated, scaled card
        double rad = Angle / Deg;
        double c = Math.Abs(Math.Cos(rad)), s = Math.Abs(Math.Sin(rad));
        double w = W * Sx, h = H * Sy;
        double hx = c * w / 2 + s * h / 2;
        double hy = s * w / 2 + c * h / 2;

        bool onFloor = false;
        if (Y + hy >= ViewH)
        {
            Y = ViewH - hy;
            onFloor = true;
            if (vy > 0)
            {
                if (vy < 80) vy = 0;                     // resting / sliding contact: no spin kick
                else
                {
                    // Real impact: one spin kick from sliding speed, capped so hard throws stay sane
                    va = Math.Clamp(va + vx * 0.15, -360, 360);
                    vy = -vy * Bounce;
                }
            }
        }
        if (Y - hy < 0)
        {
            Y = hy;
            if (vy < 0) vy = -vy * Bounce;
        }
        if (X - hx < 0)
        {
            X = hx;
            if (vx < 0) { vx = -vx * Bounce; va = -va * 0.5; }
        }
        if (X + hx > ViewW)
        {
            X = ViewW - hx;
            if (vx > 0) { vx = -vx * Bounce; va = -va * 0.5; }
        }

        if (onFloor)
        {
            // Friction, and tip over onto the nearest flat side
            double target = Math.Round(Angle / 90) * 90;
            Angle += (target - Angle) * Math.Min(1, 8 * dt);
            va *= Math.Pow(0.05, dt);
            vx *= Math.Pow(0.15, dt);

            if (vy == 0 && Math.Abs(vx) < 3 && Math.Abs(va) < 3 && Math.Abs(target - Angle) < 0.3)
            {
                vx = va = 0;
                Angle = target;
                return false;                            // at rest
            }
        }
        return true;
    }
}
