using System;

// Buffer curto e tolerância na borda; o mesmo pedido nunca produz dois saltos.
public sealed class NoxJumpAssist
{
    private double requestedAt = double.NegativeInfinity;
    private double groundedAt = double.NegativeInfinity;
    private bool consumed;
    public void Request(double now) { requestedAt = now; }
    public bool TryConsume(double now, bool grounded)
    {
        if (grounded) { groundedAt = now; consumed = false; }
        if (consumed || now - requestedAt > 0.12 || now - groundedAt > 0.10) return false;
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
