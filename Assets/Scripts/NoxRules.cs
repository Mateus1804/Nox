using System;

// Regras independentes da cena, utilizadas também pelos testes de regressão.
public static class NoxRules
{
    public static float AdjustHealth(float current, float delta, float maximum)
    {
        if (maximum <= 0 || float.IsNaN(maximum) || float.IsInfinity(maximum))
            throw new ArgumentOutOfRangeException(nameof(maximum));
        if (float.IsNaN(current) || float.IsInfinity(current) ||
            float.IsNaN(delta) || float.IsInfinity(delta))
            throw new ArgumentOutOfRangeException(nameof(current));
        return Math.Max(0, Math.Min(maximum, current + delta));
    }

    public static float HealthRatio(float current, float maximum)
    {
        return AdjustHealth(current, 0, maximum) / maximum;
    }

    public static bool IsSupport(float normalY, float relativeVerticalSpeed)
    {
        return normalY >= 0.65f && relativeVerticalSpeed <= 0.15f;
    }

    public static int NextScene(int current, int sceneCount)
    {
        if (sceneCount < 1 || current < 0 || current >= sceneCount)
            throw new ArgumentOutOfRangeException(nameof(current));
        return current + 1 < sceneCount ? current + 1 : 0;
    }

    public static bool IsValidIndex(int index, int count)
    {
        return index >= 0 && index < count;
    }
}
