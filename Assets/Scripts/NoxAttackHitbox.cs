using UnityEngine;

public sealed class NoxAttackHitbox : MonoBehaviour
{
    private readonly NoxHitRegistry hits = new NoxHitRegistry();

    private Transform owner;
    private float expiresAt = float.PositiveInfinity;
    public void Configure(Transform source, float duration)
    {
        owner = source;
        expiresAt = Time.time + Mathf.Max(0, duration);
    }
    private bool Active
    {
        get
        {
            if (Time.time >= expiresAt) return false;
            if (owner == null) return float.IsPositiveInfinity(expiresAt);
            PlayerScript player = owner.GetComponent<PlayerScript>();
            PlayerTwo alternate = owner.GetComponent<PlayerTwo>();
            return player != null ? player.IsAlive : alternate != null && alternate.IsAlive;
        }
    }

    public static bool Accept(Collider source, Component target)
    {
        if (source == null || target == null || !source.CompareTag("Slash") || !NoxSession.CanPlay) return false;
        NoxAttackHitbox attack = source.GetComponentInParent<NoxAttackHitbox>();
        if (attack == null)
        {
            // Compatibilidade com efeitos antigos não instanciados pelo controlador atual.
            SlashScript slash = source.GetComponentInParent<SlashScript>();
            ThirdSlash third = source.GetComponentInParent<ThirdSlash>();
            GameObject owner = slash != null ? slash.gameObject : third != null ? third.gameObject : source.gameObject;
            attack = owner.GetComponent<NoxAttackHitbox>();
            if (attack == null) attack = owner.AddComponent<NoxAttackHitbox>();
        }
        return attack.Active && attack.hits.TryHit(target.GetInstanceID());
    }
}
