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
