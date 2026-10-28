using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class BossScript : MonoBehaviour
{

    #region Variaveis > Agent, Player, Animin
    private readonly NoxChaseController chase = new NoxChaseController();
    private bool deathHandled;
    private NoxEnemyStrike strike;
    private float maximumHealth;
    public GameObject player;
    public NavMeshAgent agent;
    #endregion

    #region Variaveis > Distância, Collider e Ataque
    public float maxDistance = 10;
    float currentDistance = 200;

    public float rangeAttack = 1;
    public SphereCollider areaAttack;
    public bool attack = false;
    #endregion

    #region Variáveis > HP e Healthbar
    public int hp;
    public Image healthBar;
    public GameObject healthBarBg;
    #endregion

    #region Collider
    void OnTriggerEnter(Collider other)
    {
        if (!deathHandled && NoxAttackHitbox.Accept(other, this))
        {
            BarCount(1, 0.20f);
        }
    }
    #endregion

    #region OnDrawGizmos
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeAttack);
    }
    #endregion

    #region BarCount
    public void BarCount(int damage, float amount)
    {
        if (deathHandled || !NoxSession.CanPlay || damage <= 0 || !NoxRules.IsFinite(damage)) return;
        hp = (int)NoxRules.AdjustHealth(hp, -damage, maximumHealth);
        if (healthBar != null) healthBar.fillAmount = NoxRules.HealthRatio(hp, maximumHealth);
    }
    #endregion

    public void BossDeath()
    {
        if (deathHandled || hp > 0) return;
        deathHandled = true;
        NoxChaseController.Stop(agent);
        if (areaAttack != null) areaAttack.enabled = false;
        if (LoadLevels.Instance != null) LoadLevels.Instance.UnlockFire();
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

    void Start()
    {
        strike = GetComponent<NoxEnemyStrike>();
        maximumHealth = Mathf.Max(1, hp);
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (healthBar != null) healthBar.fillAmount = NoxRules.HealthRatio(hp, maximumHealth);
    }

    void Update()
    {
        if (healthBarBg != null) healthBarBg.SetActive(!deathHandled && NoxSession.CanPlay);
        if (deathHandled) return;
        BossDeath();
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
