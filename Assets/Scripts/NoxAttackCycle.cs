using System;

public enum NoxAttackPhase { Ready, Windup, Active, Recovery }

// Relógio da ação: um clique é consumido uma vez; somente a fila recente encadeia golpes.
public sealed class NoxAttackCycle
{
    public const double Windup = 0.12;
    public const double Active = 0.18;
    public const double Duration = 0.60;
    private double beganAt = double.NegativeInfinity;
    private double queuedUntil = double.NegativeInfinity;
    private double lastEnd = double.NegativeInfinity;
    private int nextStage;
    private bool delivered;
    public bool Running { get; private set; }
    public int Stage { get; private set; }
    public NoxAttackPhase Phase(double now)
    {
        if (!Running || now - beganAt >= Duration) return NoxAttackPhase.Ready;
        return now - beganAt < Windup ? NoxAttackPhase.Windup :
            now - beganAt < Windup + Active ? NoxAttackPhase.Active : NoxAttackPhase.Recovery;
    }
    public void Request(double now)
    {
        if (double.IsNaN(now) || double.IsInfinity(now)) return;
        queuedUntil = now + 0.20;
    }
    public void Tick(double now, bool allowed, out bool started, out bool strike)
    {
        started = strike = false;
        if (!allowed) { Cancel(); return; }
        if (Running && now - beganAt >= Duration)
        { Running = false; lastEnd = beganAt + Duration; }
        if (!Running && queuedUntil >= now)
        {
            if (now - lastEnd > 0.70) nextStage = 0;
            Stage = nextStage; nextStage = (Stage + 1) % 3;
            beganAt = now; Running = true; delivered = false;
            queuedUntil = double.NegativeInfinity; started = true;
        }
        if (Running && !delivered && Phase(now) == NoxAttackPhase.Active)
        { delivered = true; strike = true; }
    }
    public void Cancel()
    {
        Running = false; delivered = false; nextStage = 0;
        queuedUntil = lastEnd = double.NegativeInfinity;
    }
}
