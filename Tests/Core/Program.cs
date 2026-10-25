using System;

public static class Program
{
    private static int passed;
    private static void Equal(float expected, float actual)
    {
        if (Math.Abs(expected - actual) > 0.0001f) throw new Exception($"Esperado {expected}; encontrado {actual}");
        passed++;
    }
    private static void Throws(Action action)
    {
        try { action(); } catch (ArgumentOutOfRangeException) { passed++; return; }
        throw new Exception("Uma entrada inválida foi aceita.");
    }
    public static void Main()
    {
        Equal(2, NoxRules.AdjustHealth(3, -1, 3));
        Equal(0, NoxRules.AdjustHealth(1, -5, 3));
        Equal(3, NoxRules.AdjustHealth(3, 1, 3));
        Equal(3, NoxRules.AdjustHealth(2, 9, 3));
        Equal(1.5f, NoxRules.AdjustHealth(2, -0.5f, 3));
        Equal(2f / 3f, NoxRules.HealthRatio(2, 3));
        Equal(0, NoxRules.HealthRatio(0, 3));
        Equal(1, NoxRules.HealthRatio(3, 3));
        Equal(1, NoxRules.HealthRatio(10, 3));
        Equal(0, NoxRules.HealthRatio(-10, 3));
        Throws(() => NoxRules.AdjustHealth(2, 0, 0));
        Throws(() => NoxRules.AdjustHealth(float.NaN, 0, 3));
        Throws(() => NoxRules.AdjustHealth(1, float.PositiveInfinity, 3));
        Throws(() => NoxRules.AdjustHealth(1, 0, float.PositiveInfinity));
        Equal(1, NoxRules.NextScene(0, 5));
        Equal(4, NoxRules.NextScene(3, 5));
        Equal(0, NoxRules.NextScene(4, 5));
        Equal(0, NoxRules.NextScene(0, 1));
        Throws(() => NoxRules.NextScene(5, 5));
        Throws(() => NoxRules.NextScene(0, 0));
        Equal(0, NoxRules.IsValidIndex(-1, 5) ? 1 : 0);
        Equal(1, NoxRules.IsValidIndex(0, 5) ? 1 : 0);
        Equal(1, NoxRules.IsValidIndex(4, 5) ? 1 : 0);
        Equal(0, NoxRules.IsValidIndex(5, 5) ? 1 : 0);
        Equal(0, NoxRules.IsValidIndex(0, 0) ? 1 : 0);
        Console.WriteLine($"{passed} verificações de regressão aprovadas.");
    }
}
