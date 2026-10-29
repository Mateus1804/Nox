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
