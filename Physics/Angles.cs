namespace Avi583Logo.Physics;

/// <summary>Angle helpers. Angles are in degrees, clockwise on screen (same as CSS rotate).</summary>
public static class Angles
{
    public const double Deg = 180 / Math.PI;

    // Rotate a screen-space vector by `deg` (clockwise on screen, same as CSS rotate)
    public static (double X, double Y) Rotate(double x, double y, double deg)
    {
        double r = deg / Deg, c = Math.Cos(r), s = Math.Sin(r);
        return (x * c - y * s, x * s + y * c);
    }

    // Wrap an angle into (-180, 180]
    public static double Wrap(double deg) => deg - 360 * Math.Round(deg / 360);

    // Pick the equivalent of `target` that is closest to `current` (avoids spinning the long way round)
    public static double Nearest(double target, double current) => current + Wrap(target - current);
}
