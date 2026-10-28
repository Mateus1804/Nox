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
        // O golpe tem preparação, um impacto e recuperação; segurar não cria novos pedidos.
        var combo = new NoxAttackCycle();
        combo.Request(0);
        combo.Tick(0, true, out bool began, out bool strike);
        Equal(1, began ? 1 : 0); Equal(0, strike ? 1 : 0);
        Equal((int)NoxAttackPhase.Windup, (int)combo.Phase(0.05));
        combo.Tick(0.121, true, out began, out strike);
        Equal(1, strike ? 1 : 0);
        combo.Tick(0.15, true, out began, out strike); Equal(0, strike ? 1 : 0);
        Equal((int)NoxAttackPhase.Recovery, (int)combo.Phase(0.4));
        combo.Request(0.5); combo.Tick(0.601, true, out began, out strike);
        Equal(1, began ? 1 : 0); Equal(1, combo.Stage);
        combo.Tick(0.73, true, out began, out strike); Equal(1, strike ? 1 : 0);
        combo.Request(1.12); combo.Tick(1.202, true, out began, out strike);
        Equal(1, began ? 1 : 0); Equal(2, combo.Stage);
        combo.Tick(1.81, true, out began, out strike);
        Equal(0, began ? 1 : 0); Equal(0, strike ? 1 : 0); Equal(0, combo.Running ? 1 : 0);
        combo.Request(3); combo.Tick(3, true, out began, out strike); Equal(0, combo.Stage);
        combo.Tick(3.13, false, out began, out strike); Equal(0, strike ? 1 : 0); Equal(0, combo.Running ? 1 : 0);
        combo.Request(4); combo.Tick(4, true, out began, out strike);
        combo.Request(4.1); combo.Tick(4.61, true, out began, out strike); Equal(0, began ? 1 : 0);
        combo.Request(double.NaN); combo.Tick(5, true, out began, out strike); Equal(0, began ? 1 : 0);
        combo.Request(6); combo.Tick(6, true, out began, out strike);
        combo.Tick(6.5, true, out began, out strike); Equal(0, strike ? 1 : 0); // Quadro atrasado não cria impacto vencido.
        combo.Cancel(); combo.Tick(7, true, out began, out strike); Equal(0, began ? 1 : 0);
        // A antecipação não causa dano; fugir ou perder visão interrompe a preparação.
        var threat = new NoxEnemyCycle(0.6, 0.18, 1.1);
        threat.Tick(0, true, false, true); Equal((int)NoxThreatPhase.Ready, (int)threat.Phase);
        threat.Tick(0, true, true, true); Equal((int)NoxThreatPhase.Windup, (int)threat.Phase);
        Equal(0.5f, threat.Progress(0.3));
        threat.Tick(0.61, true, true, true); Equal((int)NoxThreatPhase.Active, (int)threat.Phase);
        threat.Tick(0.79, true, true, true); Equal((int)NoxThreatPhase.Recovery, (int)threat.Phase);
        threat.Tick(1.9, true, true, true); Equal((int)NoxThreatPhase.Ready, (int)threat.Phase);
        threat.Tick(2, true, true, true); threat.Tick(2.1, false, true, true);
        Equal((int)NoxThreatPhase.Recovery, (int)threat.Phase);
        threat.Tick(2.3, true, true, true); Equal((int)NoxThreatPhase.Recovery, (int)threat.Phase);
        threat.Tick(3.3, true, true, true); Equal((int)NoxThreatPhase.Ready, (int)threat.Phase);
        threat.Tick(4, true, true, true); threat.Tick(4.1, true, false, true);
        Equal((int)NoxThreatPhase.Recovery, (int)threat.Phase);
        threat.Tick(4.2, true, true, false); Equal((int)NoxThreatPhase.Ready, (int)threat.Phase);
        threat.Tick(5, true, true, true); threat.Tick(8, true, true, true);
        Equal((int)NoxThreatPhase.Ready, (int)threat.Phase); // Salto do relógio não causa golpe tardio.
        Throws(() => new NoxEnemyCycle(0, 1, 1)); Throws(() => new NoxEnemyCycle(double.NaN, 1, 1));
        Equal(0.6f, NoxMotionRules.Cadence(0, 1));
        Equal(2, NoxMotionRules.Cadence(3, 1)); Equal(3, NoxMotionRules.Cadence(100, 1));
        Equal(1, NoxMotionRules.Cadence(3, 0)); Equal(1, NoxMotionRules.Cadence(float.NaN, 1));
        Equal(1, NoxMotionRules.Cadence(3, float.PositiveInfinity));
        hits.Clear(); Equal(1, hits.TryHit(42) ? 1 : 0); Equal(0, hits.TryHit(42) ? 1 : 0);
        Console.WriteLine($"{passed} verificações de regressão aprovadas.");
    }
}
