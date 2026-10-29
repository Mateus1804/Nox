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
