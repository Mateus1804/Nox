using UnityEngine;

public class EnemyAir : MonoBehaviour
{
    public Animator animin;
    public float hpTurtle;
    private bool deathHandled;
    private bool hasDeathParameter;
    private bool hasHitParameter;

    public void Morte()
    {
        if (deathHandled || hpTurtle > 0) return;
        deathHandled = true;
        NoxEnemyStrike strike = GetComponent<NoxEnemyStrike>();
        if (strike != null) strike.Interrupt();
        CancelInvoke(nameof(ClearHit));
        if (animin != null && hasDeathParameter) animin.SetBool("IsDie", true);
        foreach (Collider hitbox in GetComponentsInChildren<Collider>()) hitbox.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    public void LifeCountTurtle(float damage)
    {
        if (deathHandled || !NoxSession.CanPlay || damage <= 0 || !NoxRules.IsFinite(damage)) return;
        hpTurtle = Mathf.Max(0, hpTurtle - damage);
        if (animin != null && hasHitParameter)
        {
            animin.SetBool("Hit", true);
            CancelInvoke(nameof(ClearHit));
            Invoke(nameof(ClearHit), 0.15f);
        }
        Morte();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!deathHandled && NoxAttackHitbox.Accept(other, this)) LifeCountTurtle(1);
    }
    private void ClearHit() { if (animin != null && hasHitParameter) animin.SetBool("Hit", false); }
    private void Start()
    {
        if (animin == null) animin = GetComponent<Animator>();
        if (animin == null) return;
        foreach (AnimatorControllerParameter parameter in animin.parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Bool) continue;
            hasDeathParameter |= parameter.name == "IsDie";
            hasHitParameter |= parameter.name == "Hit";
        }
    }
    private void Update() { Morte(); }
    private void OnDisable() { CancelInvoke(nameof(ClearHit)); }
}
