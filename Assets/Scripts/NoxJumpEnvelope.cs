using System;

// Estimativa balística: não inclui obstáculos, aceleração nem movimento do destino.
public static class NoxJumpEnvelope
{
    public static double Range(double speed, double height, double gravity, double rise)
    {
        if (!Finite(speed) || !Finite(height) || !Finite(gravity) || !Finite(rise) ||
            speed <= 0 || height <= 0 || gravity <= 0 || rise > height) return 0;
        double initial = Math.Sqrt(2 * gravity * height);
        return speed * (initial + Math.Sqrt(Math.Max(0, initial * initial - 2 * gravity * rise))) / gravity;
    }
    private static bool Finite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }
}
