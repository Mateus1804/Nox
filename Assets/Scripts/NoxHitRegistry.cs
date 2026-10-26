using System.Collections.Generic;

// Uma instância por golpe: não acumula histórico na vida inteira do inimigo.
public sealed class NoxHitRegistry
{
    private readonly HashSet<int> targets = new HashSet<int>();
    public bool TryHit(int targetId) { return targets.Add(targetId); }
}
