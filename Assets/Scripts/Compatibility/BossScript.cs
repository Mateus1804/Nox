using UnityEngine;

// Adapta os chefes de vida inteira e a recompensa original da fase de ar.
public class BossScript : NoxBossController
{
    public int hp
    {
        get => (int)health; set => health = value;
    }
    public SphereCollider areaAttack
    {
        get => damageArea as SphereCollider; set => damageArea = value;
    }
    protected override float NormalizeHealth(float value) => (int)value;
    public override bool UnlocksFire => true;
    private void Reset()
    {
        health = 0;
        followRange = 10;
        attackRange = 1;
    }
}
