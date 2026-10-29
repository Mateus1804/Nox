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
