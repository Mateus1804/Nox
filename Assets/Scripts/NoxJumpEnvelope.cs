using System;

// Estimativa balística: não inclui obstáculos, aceleração nem movimento do destino.
public static class NoxJumpEnvelope
{
    public static double Range(double speed, double height, double gravity, double rise)
    {
        if (!NoxRules.IsFinite(speed) || !NoxRules.IsFinite(height) || !NoxRules.IsFinite(gravity) || !NoxRules.IsFinite(rise) ||
            speed <= 0 || height <= 0 || gravity <= 0 || rise > height)
            return 0;
        double initial = Math.Sqrt(2 * gravity * height);
        return speed * (initial + Math.Sqrt(Math.Max(0, initial * initial - 2 * gravity * rise))) / gravity;
    }
}
