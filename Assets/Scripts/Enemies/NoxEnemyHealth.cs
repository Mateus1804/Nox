using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "EnemyAir")]
public class NoxEnemyHealth : MonoBehaviour
{
    [FormerlySerializedAs("animin")]
    public Animator animator;
    [FormerlySerializedAs("hpTurtle")]
    public float health;
    private bool deathHandled;
    private bool hasDeathParameter;
    private bool hasHitParameter;

    public void Morte()
    {
        if (deathHandled || health > 0)
            return;
        deathHandled = true;
        NoxEnemyStrike strike = GetComponent<NoxEnemyStrike>();
        if (strike != null)
            strike.Interrupt();
        CancelInvoke(nameof(ClearHit));
        if (animator != null && hasDeathParameter)
            animator.SetBool("IsDie", true);
        foreach (Collider hitbox in GetComponentsInChildren<Collider>())
            hitbox.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    public void LifeCountTurtle(float damage)
    {
        if (deathHandled || !NoxSession.CanPlay || damage <= 0 || !NoxRules.IsFinite(damage))
            return;
        health = Mathf.Max(0, health - damage);
        if (animator != null && hasHitParameter)
        {
            animator.SetBool("Hit", true);
            CancelInvoke(nameof(ClearHit));
            Invoke(nameof(ClearHit), 0.15f);
        }
        Morte();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!deathHandled && NoxAttackHitbox.Accept(other, this))
            LifeCountTurtle(1);
    }
    private void ClearHit()
    {
        if (animator != null && hasHitParameter)
            animator.SetBool("Hit", false);
    }
    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            return;
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Bool)
                continue;
            hasDeathParameter |= parameter.name == "IsDie";
            hasHitParameter |= parameter.name == "Hit";
        }
    }
    private void Update()
    {
        Morte();
    }
    private void OnDisable()
    {
        CancelInvoke(nameof(ClearHit));
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public Animator animin
    {
        get => animator; set => animator = value;
    }
    public float hpTurtle
    {
        get => health; set => health = value;
    }
}
