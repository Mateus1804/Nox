using UnityEngine;

public sealed class NoxAttackHitbox : MonoBehaviour
{
    private readonly NoxHitRegistry hits = new NoxHitRegistry();

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
        return attack.hits.TryHit(target.GetInstanceID());
    }
}
