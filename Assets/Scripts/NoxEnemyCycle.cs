using System;

public enum NoxThreatPhase { Ready, Windup, Active, Recovery }

public sealed class NoxEnemyCycle
{
    private readonly double windup, active, recovery;
    private double beganAt;
    public NoxThreatPhase Phase { get; private set; }
    public float Progress(double now) => Phase == NoxThreatPhase.Windup ?
        (float)Math.Max(0, Math.Min(1, (now - beganAt) / windup)) : 0;
    public NoxEnemyCycle(double windup, double active, double recovery)
    {
        if (windup <= 0 || active <= 0 || recovery <= 0 ||
            double.IsNaN(windup + active + recovery) || double.IsInfinity(windup + active + recovery))
            throw new ArgumentOutOfRangeException(nameof(windup));
        this.windup = windup; this.active = active; this.recovery = recovery;
    }
    public void Tick(double now, bool inRange, bool visible, bool allowed)
    {
        if (!allowed) { Reset(); return; }
        if (Phase == NoxThreatPhase.Ready)
        {
            if (inRange && visible) { beganAt = now; Phase = NoxThreatPhase.Windup; }
            return;
        }
        double age = now - beganAt;
        if (Phase == NoxThreatPhase.Windup && (!inRange || !visible))
        { Phase = NoxThreatPhase.Recovery; beganAt = now - windup - active; return; }
        if (age >= windup + active + recovery) Phase = NoxThreatPhase.Ready;
        else if (age >= windup + active) Phase = NoxThreatPhase.Recovery;
        else if (age >= windup) Phase = NoxThreatPhase.Active;
    }
    public void Reset() { Phase = NoxThreatPhase.Ready; }
}
