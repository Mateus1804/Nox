using System;

public static class Program
{
    private static int passed;
    private static void Equal(float expected, float actual)
    {
        if (float.IsNaN(actual) || float.IsInfinity(actual) || Math.Abs(expected - actual) > 0.0001f) throw new Exception($"Esperado {expected}; encontrado {actual}");
        passed++;
    }
    private static void Throws(Action action)
    {
        try { action(); } catch (ArgumentOutOfRangeException) { passed++; return; }
        throw new Exception("Uma entrada inválida foi aceita.");
    }
    public static void Main()
    {
        var jump = new NoxJumpAssist();
        Equal(0, jump.TryConsume(0, false) ? 1 : 0);
        jump.Request(1);
        Equal(0, jump.TryConsume(1, false) ? 1 : 0);
        Equal(1, jump.TryConsume(1.08, true) ? 1 : 0);
        Equal(0, jump.TryConsume(1.09, false) ? 1 : 0);
        jump.Reset();
        jump.TryConsume(2, true);
        jump.Request(2.08);
        Equal(1, jump.TryConsume(2.08, false) ? 1 : 0);
        jump.Request(2.09);
        Equal(0, jump.TryConsume(2.09, false) ? 1 : 0);
        jump.Reset();
        jump.Request(3);
        Equal(0, jump.TryConsume(3.13, true) ? 1 : 0);
        jump.Reset();
        jump.TryConsume(4, true);
        jump.Request(4.11);
        Equal(0, jump.TryConsume(4.11, false) ? 1 : 0);
        jump.Reset();
        jump.Request(5);
        jump.Reset();
        Equal(0, jump.TryConsume(5, true) ? 1 : 0);
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
        // Dano: contatos compostos e projéteis simultâneos não retiram toda a vida.
        var window = new NoxDamageWindow();
        Equal(1, window.TryApply(3, 1, 3, 10, 0.6f, out float hp) ? 1 : 0);
        Equal(2, hp);
        Equal(0, window.TryApply(hp, 1, 3, 10, 0.6f, out hp) ? 1 : 0);
        Equal(2, hp);
        Equal(0, window.TryApply(hp, 1, 3, 10.59f, 0.6f, out hp) ? 1 : 0);
        Equal(1, window.TryApply(hp, 1, 3, 10.61f, 0.6f, out hp) ? 1 : 0);
        Equal(1, hp);
        Equal(1, window.TryApply(hp, 20, 3, 11.3f, 0.6f, out hp) ? 1 : 0);
        Equal(0, hp);
        Equal(0, window.TryApply(0, 1, 3, 12, 0.6f, out hp) ? 1 : 0);
        var invalid = new NoxDamageWindow();
        Throws(() => invalid.TryApply(3, float.NaN, 3, 0, 1, out _));
        Throws(() => invalid.TryApply(3, 1, 3, float.PositiveInfinity, 1, out _));
        Throws(() => invalid.TryApply(3, 1, 3, 0, -1, out _));
        Equal(0, invalid.TryApply(3, -1, 3, 0, 1, out hp) ? 1 : 0);
        Equal(1, invalid.TryApply(3, 1, 3, 0, 0, out hp) ? 1 : 0);
        Equal(1, invalid.TryApply(hp, 1, 3, 0, 0, out hp) ? 1 : 0);
        // Cura fracionária, excedente e estado externo já acima do limite.
        Equal(3, NoxRules.AdjustHealth(2.8f, 1, 3));
        Equal(3, NoxRules.AdjustHealth(99, 1, 3));
        Equal(2.5f, NoxRules.AdjustHealth(2, 0.5f, 3));
        // Apoio: topo aceito; parede e subida de um salto não são chão.
        Equal(1, NoxRules.IsSupport(1, 0) ? 1 : 0);
        Equal(1, NoxRules.IsSupport(0.8f, -1) ? 1 : 0);
        Equal(0, NoxRules.IsSupport(0.2f, 0) ? 1 : 0);
        Equal(0, NoxRules.IsSupport(1, 4) ? 1 : 0);
        Equal(0, NoxRules.IsSupport(float.NaN, 0) ? 1 : 0);
        // Volume global: limite, silêncio, números inválidos e preservação da preferência.
        Equal(0.75f, NoxRules.EffectiveVolume(0.75f, false));
        Equal(0, NoxRules.EffectiveVolume(0.75f, true));
        Equal(1, NoxRules.EffectiveVolume(5, false));
        Equal(0, NoxRules.EffectiveVolume(-5, false));
        Equal(0, NoxRules.EffectiveVolume(float.NaN, false));
        Equal(0, NoxRules.EffectiveVolume(float.PositiveInfinity, false));
        Equal(0, NoxRules.EffectiveVolume(float.NegativeInfinity, false));
        Equal(1, NoxRules.IsFinite(0) ? 1 : 0);
        Equal(0, NoxRules.IsFinite(float.NaN) ? 1 : 0);
        // Um golpe afeta cada alvo uma vez; outro golpe pode atingir o mesmo inimigo.
        var hits = new NoxHitRegistry();
        Equal(1, hits.TryHit(42) ? 1 : 0);
        Equal(0, hits.TryHit(42) ? 1 : 0);
        Equal(1, hits.TryHit(99) ? 1 : 0);
        Equal(0, hits.TryHit(99) ? 1 : 0);
        Equal(1, new NoxHitRegistry().TryHit(42) ? 1 : 0);
        // Retomada é de fase, não de posição: rejeita menu, índices ausentes e fogo bloqueado.
        Equal(0, NoxProgressRules.CanResume(-1, 5, true) ? 1 : 0);
        Equal(0, NoxProgressRules.CanResume(0, 5, true) ? 1 : 0);
        Equal(1, NoxProgressRules.CanResume(1, 5, false) ? 1 : 0);
        Equal(1, NoxProgressRules.CanResume(3, 5, false) ? 1 : 0);
        Equal(0, NoxProgressRules.CanResume(4, 5, false) ? 1 : 0);
        Equal(1, NoxProgressRules.CanResume(4, 5, true) ? 1 : 0);
        Equal(0, NoxProgressRules.CanResume(5, 5, true) ? 1 : 0);
        Equal(0, NoxProgressRules.CanResume(1, 0, true) ? 1 : 0);
        // Callbacks concorrentes não iniciam duas trocas de cena.
        var gate = new NoxSceneLoadGate();
        Equal(0, gate.TryBegin(-1, 5) ? 1 : 0);
        Equal(0, gate.Busy ? 1 : 0);
        Equal(1, gate.TryBegin(1, 5) ? 1 : 0);
        Equal(1, gate.Busy ? 1 : 0);
        Equal(0, gate.TryBegin(2, 5) ? 1 : 0);
        gate.Complete();
        Equal(0, gate.Busy ? 1 : 0);
        Equal(1, gate.TryBegin(0, 5) ? 1 : 0);
        gate.Complete();
        gate.Complete();
        Equal(1, gate.TryBegin(4, 5) ? 1 : 0);
        // Ataque interrompe perseguição; configuração inválida não produz destino.
        Equal((int)NoxEnemyAction.Attack, (int)NoxRules.ChooseEnemyAction(1, 1, 10));
        Equal((int)NoxEnemyAction.Chase, (int)NoxRules.ChooseEnemyAction(5, 1, 10));
        Equal((int)NoxEnemyAction.Idle, (int)NoxRules.ChooseEnemyAction(11, 1, 10));
        Equal((int)NoxEnemyAction.Chase, (int)NoxRules.ChooseEnemyAction(10, 1, 10));
        Equal((int)NoxEnemyAction.Idle, (int)NoxRules.ChooseEnemyAction(float.NaN, 1, 10));
        Equal((int)NoxEnemyAction.Idle, (int)NoxRules.ChooseEnemyAction(1, float.PositiveInfinity, 10));
        Equal((int)NoxEnemyAction.Idle, (int)NoxRules.ChooseEnemyAction(-1, 1, 10));
        Equal((int)NoxEnemyAction.Attack, (int)NoxRules.ChooseEnemyAction(1, 2, 0));
        Console.WriteLine($"{passed} verificações de regressão aprovadas.");
    }
}
