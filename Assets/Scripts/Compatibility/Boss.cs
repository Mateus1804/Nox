using UnityEngine;

// Mantém o GUID dos chefes recuperados; comportamento único em NoxBossController.
public class Boss : NoxBossController
{
    public CapsuleCollider areaAttack
    {
        get => damageArea as CapsuleCollider; set => damageArea = value;
    }
}
