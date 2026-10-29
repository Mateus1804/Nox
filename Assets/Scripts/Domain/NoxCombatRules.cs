using System.Collections.Generic;
using System;

// Usa tempo da partida: a pausa não consome a janela de proteção.
public sealed class NoxDamageWindow
{
    private float nextDamageTime = float.NegativeInfinity;
    public bool TryApply(float current, float amount, float maximum, float now,
        float protectionSeconds, out float next)
    {
        next = NoxRules.AdjustHealth(current, 0, maximum);
        if (!NoxRules.IsFinite(amount))
            throw new ArgumentOutOfRangeException(nameof(amount));
        if (!NoxRules.IsFinite(now))
            throw new ArgumentOutOfRangeException(nameof(now));
        if (!NoxRules.IsFinite(protectionSeconds) || protectionSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(protectionSeconds));
        if (next <= 0 || amount <= 0 || now < nextDamageTime)
            return false;
        next = NoxRules.AdjustHealth(next, -amount, maximum);
        nextDamageTime = now + protectionSeconds;
        return true;
    }
}

// Uma instância por golpe: não acumula histórico na vida inteira do inimigo.
public sealed class NoxHitRegistry
{
    private readonly HashSet<int> targets = new HashSet<int>();
    public void Clear()
    {
        targets.Clear();
    }
    public bool TryHit(int targetId)
    {
        return targets.Add(targetId);
    }
}

public enum NoxAttackPhase
{
    Ready, Windup, Active, Recovery
}

// Relógio da ação: um clique é consumido uma vez; somente a fila recente encadeia golpes.
public sealed class NoxAttackCycle
{
    public const double Windup = 0.12;
    public const double Active = 0.18;
    public const double Duration = 0.60;
    private const double InputBufferSeconds = 0.20;
    private const double ComboResetSeconds = 0.70;
    private const int ComboStages = 3;
    private double beganAt = double.NegativeInfinity;
    private double queuedUntil = double.NegativeInfinity;
    private double lastEnd = double.NegativeInfinity;
    private int nextStage;
    private bool delivered;
    public bool Running
    {
        get; private set;
    }
    public int Stage
    {
        get; private set;
    }
    public NoxAttackPhase Phase(double now)
    {
        if (!NoxRules.IsFinite(now) || !Running || now - beganAt >= Duration)
            return NoxAttackPhase.Ready;
        return now - beganAt < Windup ? NoxAttackPhase.Windup :
            now - beganAt < Windup + Active ? NoxAttackPhase.Active : NoxAttackPhase.Recovery;
    }
    public void Request(double now)
    {
        if (!NoxRules.IsFinite(now))
            return;
        queuedUntil = now + InputBufferSeconds;
    }
    public void Tick(double now, bool allowed, out bool started, out bool strike)
    {
        started = strike = false;
        if (!allowed || !NoxRules.IsFinite(now))
        {
            Cancel();
            return;
        }
        if (Running && now - beganAt >= Duration)
        {
            Running = false;
            lastEnd = beganAt + Duration;
        }
        if (!Running && queuedUntil >= now)
        {
            if (now - lastEnd > ComboResetSeconds)
                nextStage = 0;
            Stage = nextStage;
            nextStage = (Stage + 1) % ComboStages;
            beganAt = now;
            Running = true;
            delivered = false;
            queuedUntil = double.NegativeInfinity;
            started = true;
        }
        if (Running && !delivered && Phase(now) == NoxAttackPhase.Active)
        {
            delivered = true;
            strike = true;
        }
    }
    public void Cancel()
    {
        Running = false;
        delivered = false;
        nextStage = 0;
        queuedUntil = lastEnd = double.NegativeInfinity;
    }
}

public enum NoxThreatPhase
{
    Ready, Windup, Active, Recovery
}

public sealed class NoxEnemyCycle
{
    private readonly double windup, active, recovery;
    private double beganAt;
    public NoxThreatPhase Phase
    {
        get; private set;
    }
    public float Progress(double now) => NoxRules.IsFinite(now) && Phase == NoxThreatPhase.Windup ?
        (float)Math.Max(0, Math.Min(1, (now - beganAt) / windup)) : 0;
    public NoxEnemyCycle(double windup, double active, double recovery)
    {
        if (windup <= 0 || active <= 0 || recovery <= 0 ||
            double.IsNaN(windup + active + recovery) || double.IsInfinity(windup + active + recovery))
            throw new ArgumentOutOfRangeException(nameof(windup));
        this.windup = windup;
        this.active = active;
        this.recovery = recovery;
    }
    public void Tick(double now, bool inRange, bool visible, bool allowed)
    {
        if (!allowed || !NoxRules.IsFinite(now))
        {
            Reset();
            return;
        }
        if (Phase == NoxThreatPhase.Ready)
        {
            if (inRange && visible)
            {
                beganAt = now;
                Phase = NoxThreatPhase.Windup;
            }
            return;
        }
        double age = now - beganAt;
        if (Phase == NoxThreatPhase.Windup && (!inRange || !visible))
        {
            Phase = NoxThreatPhase.Recovery;
            beganAt = now - windup - active;
            return;
        }
        if (age >= windup + active + recovery)
            Phase = NoxThreatPhase.Ready;
        else if (age >= windup + active)
            Phase = NoxThreatPhase.Recovery;
        else if (age >= windup)
            Phase = NoxThreatPhase.Active;
    }
    public void Reset()
    {
        Phase = NoxThreatPhase.Ready;
    }
}
