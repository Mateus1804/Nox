using System;

public static class NoxMotionRules
{
    // Estimativa inicial de passada; o ajuste artístico final depende do percurso no Unity.
    public static float Cadence(float speed, float height)
    {
        if (!NoxRules.IsFinite(speed) || !NoxRules.IsFinite(height) || height <= 0 || speed < 0)
            return 1;
        return Math.Max(0.6f, Math.Min(3f, speed * 0.8f / (height * 1.2f)));
    }
}

// Buffer curto e tolerância na borda; o mesmo pedido nunca produz dois saltos.
public sealed class NoxJumpAssist
{
    private const double InputBufferSeconds = 0.12;
    private const double CoyoteSeconds = 0.10;
    private double requestedAt = double.NegativeInfinity;
    private double groundedAt = double.NegativeInfinity;
    private bool consumed;
    public void Request(double now)
    {
        if (NoxRules.IsFinite(now))
            requestedAt = now;
    }
    public bool TryConsume(double now, bool grounded)
    {
        if (!NoxRules.IsFinite(now))
            return false;
        if (grounded)
        {
            groundedAt = now;
            consumed = false;
        }
        if (consumed || now - requestedAt > InputBufferSeconds || now - groundedAt > CoyoteSeconds)
            return false;
        requestedAt = double.NegativeInfinity;
        consumed = true;
        return true;
    }
    public void Reset()
    {
        requestedAt = groundedAt = double.NegativeInfinity;
        consumed = false;
    }
}

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
