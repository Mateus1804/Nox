using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class NoxBossController : MonoBehaviour
{
    private readonly NoxChaseController chase = new NoxChaseController();
    private bool deathHandled;
    private NoxEnemyStrike strike;
    private float maximumHealth;
    public GameObject player;
    public NavMeshAgent agent;

    [FormerlySerializedAs("BossHealthBar")]
    [FormerlySerializedAs("healthBar")]
    public Image healthImage;
    [FormerlySerializedAs("BossHealthBarObj")]
    public GameObject healthBarObject;
    [FormerlySerializedAs("CanvasLifeBoss")]
    [FormerlySerializedAs("healthBarBg")]
    public GameObject healthCanvas;
    [FormerlySerializedAs("hpBoss")]
    [FormerlySerializedAs("hp")]
    public float health = 200;
    [FormerlySerializedAs("maxDistance")]
    public float followRange = 25;
    [FormerlySerializedAs("currentDistance")]
    public float targetDistance = 200;
    [FormerlySerializedAs("rangeAttack")]
    public float attackRange;
    [FormerlySerializedAs("areaAttack")]
    public Collider damageArea;
    public bool attack = false;
    [FormerlySerializedAs("tradeTime")]
    [HideInInspector]
    public float legacyAttackInterval;

    public void HandleDeath()
    {
        if (deathHandled || health > 0)
            return;
        deathHandled = true;
        NoxChaseController.Stop(agent);
        if (damageArea != null)
            damageArea.enabled = false;
        UnlockReward();
        if (NoxLevelResultScreen.PHP != null)
            NoxLevelResultScreen.PHP.PanelWinOrLose("Stage Completed");
        Destroy(gameObject);
    }

    public void FollowPlayer()
    {
        if (NoxSession.CanPlay && player != null && NoxChaseController.Ready(agent))
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void ApplyDamage(float damage)
    {
        if (deathHandled || !NoxSession.CanPlay || damage <= 0 || !NoxRules.IsFinite(damage))
            return;
        health = NormalizeHealth(NoxRules.AdjustHealth(health, -damage, maximumHealth));
        if (healthImage != null)
            healthImage.fillAmount = NoxRules.HealthRatio(health, maximumHealth);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!deathHandled && NoxAttackHitbox.Accept(other, this))
        {
            ApplyDamage(1);
        }

    }

    protected virtual void Start()
    {
        strike = GetComponent<NoxEnemyStrike>();
        maximumHealth = Mathf.Max(1, health);
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (healthBarObject != null)
            healthImage = healthBarObject.GetComponent<Image>();

        if (healthImage != null)
            healthImage.fillAmount = NoxRules.HealthRatio(health, maximumHealth);
    }

    protected virtual void Update()
    {
        if (healthCanvas != null)
            healthCanvas.SetActive(!deathHandled && NoxSession.CanPlay);
        if (deathHandled)
            return;
        HandleDeath();
        if (deathHandled)
            return;
        if (strike != null && strike.Committed)
            NoxChaseController.Stop(agent);
        else
            attack = chase.Update(agent, player != null ? player.transform : null,
            attackRange, followRange, Time.time, out targetDistance);
        if (strike != null)
            attack = strike.Active;
        else if (damageArea != null)
            damageArea.enabled = attack;
        if (!NoxSession.CanPlay || player == null || (strike != null && strike.Committed))
            return;
        Vector3 facing = player.transform.position - transform.position;
        facing.y = 0;
        if (facing.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(facing);
    }

    protected virtual void OnDisable()
    {
        NoxChaseController.Stop(agent);
        if (damageArea != null)
            damageArea.enabled = false;
    }

    [SerializeField] private bool unlocksFire;
    [SerializeField] private bool integerHealth;
    public virtual bool UnlocksFire => unlocksFire;
    public void ConfigureProfile(bool fireReward, bool useIntegerHealth)
    {
        unlocksFire = fireReward;
        integerHealth = useIntegerHealth;
    }
    protected virtual float NormalizeHealth(float value) => integerHealth ? (int)value : value;
    protected virtual void UnlockReward()
    {
        if (NoxLevelSelection.Instance == null)
            return;
        if (UnlocksFire)
            NoxLevelSelection.Instance.UnlockFire();
        else
            NoxLevelSelection.Instance.UnlockWater();
    }

    // Nomes antigos de eventos e scripts mantidos como delegação, sem outra implementação.
    public void Morte() => HandleDeath();
    public void BossDeath() => HandleDeath();
    public void Persegue() => FollowPlayer();
    public void LifeCountBoss(float damage, float hp_amount) => ApplyDamage(damage);
    public void BarCount(int damage, float amount) => ApplyDamage(damage);
    public Image BossHealthBar
    {
        get => healthImage; set => healthImage = value;
    }
    public GameObject BossHealthBarObj
    {
        get => healthBarObject; set => healthBarObject = value;
    }
    public GameObject CanvasLifeBoss
    {
        get => healthCanvas; set => healthCanvas = value;
    }
    public float hpBoss
    {
        get => health; set => health = value;
    }
    public float maxDistance
    {
        get => followRange; set => followRange = value;
    }
    public float currentDistance
    {
        get => targetDistance; set => targetDistance = value;
    }
    public float rangeAttack
    {
        get => attackRange; set => attackRange = value;
    }
    public float tradeTime
    {
        get => legacyAttackInterval; set => legacyAttackInterval = value;
    }
    public Image healthBar
    {
        get => healthImage; set => healthImage = value;
    }
    public GameObject healthBarBg
    {
        get => healthCanvas; set => healthCanvas = value;
    }
}
