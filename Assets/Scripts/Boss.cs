using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    private readonly NoxChaseController chase = new NoxChaseController();
    private bool deathHandled;
    private NoxEnemyStrike strike;
    private float maximumHealth;
    public GameObject player;
    public NavMeshAgent agent;

    public Image BossHealthBar;
    public GameObject BossHealthBarObj;
    public GameObject CanvasLifeBoss;
    public float hpBoss = 200;
    public float maxDistance = 25;
    public float currentDistance = 200;
    public float rangeAttack;
    public CapsuleCollider areaAttack;
    private bool attack = false;
    public float tradeTime;

    public void Morte()
    {
        if (deathHandled || hpBoss > 0) return;
        deathHandled = true;
        NoxChaseController.Stop(agent);
        if (areaAttack != null) areaAttack.enabled = false;
        if (LoadLevels.Instance != null) LoadLevels.Instance.UnlockWater();
        if (PlaceHolderPanel.PHP != null) PlaceHolderPanel.PHP.PanelWinOrLose("Stage Completed");
        Destroy(gameObject);
    }

    public void Persegue()
    {
        if (NoxSession.CanPlay && player != null && NoxChaseController.Ready(agent))
        {
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);
        }
    }

    #region OnDrawGizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeAttack);
    }
    #endregion

    public void LifeCountBoss(float damage, float hp_amount)
    {
        if (deathHandled || !NoxSession.CanPlay || damage <= 0 || !NoxRules.IsFinite(damage)) return;
        hpBoss = NoxRules.AdjustHealth(hpBoss, -damage, maximumHealth);
        if (BossHealthBar != null) BossHealthBar.fillAmount = NoxRules.HealthRatio(hpBoss, maximumHealth);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!deathHandled && NoxAttackHitbox.Accept(other, this))
        {
            LifeCountBoss(1, 0.10f);
        }

    }

    void Start()
    {
        strike = GetComponent<NoxEnemyStrike>();
        maximumHealth = Mathf.Max(1, hpBoss);
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (BossHealthBarObj != null) BossHealthBar = BossHealthBarObj.GetComponent<Image>();

        if (BossHealthBar != null) BossHealthBar.fillAmount = NoxRules.HealthRatio(hpBoss, maximumHealth);
    }

    void Update()
    {
        if (CanvasLifeBoss != null) CanvasLifeBoss.SetActive(!deathHandled && NoxSession.CanPlay);
        if (deathHandled) return;
        Morte();
        if (deathHandled) return;
        if (strike != null && strike.Committed) NoxChaseController.Stop(agent);
        else attack = chase.Update(agent, player != null ? player.transform : null,
            rangeAttack, maxDistance, Time.time, out currentDistance);
        if (strike != null) attack = strike.Active;
        else if (areaAttack != null) areaAttack.enabled = attack;
        if (!NoxSession.CanPlay || player == null || (strike != null && strike.Committed)) return;
        Vector3 facing = player.transform.position - transform.position;
        facing.y = 0;
        if (facing.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(facing);
    }

    private void OnDisable()
    {
        NoxChaseController.Stop(agent);
        if (areaAttack != null) areaAttack.enabled = false;
    }
}
