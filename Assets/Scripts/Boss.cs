using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    private bool deathHandled;
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
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (areaAttack != null) areaAttack.enabled = false;
        if (LoadLevels.Instance != null) LoadLevels.Instance.UnlockWater();
        if (PlaceHolderPanel.PHP != null) PlaceHolderPanel.PHP.PanelWinOrLose("Stage Completed");
        Destroy(gameObject);
    }

    public void Persegue()
    {
        agent.SetDestination(player.transform.position);
    }

    #region Metodo > Ataque
    void Attack()
    {
        if (!attack)
        {
            attack = true;
            if (areaAttack != null) areaAttack.enabled = true;
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


    public void LifeCountBoss(float damage, float hp_amount)
    {
        if (deathHandled || !NoxSession.CanPlay || damage <= 0) return;
        hpBoss = NoxRules.AdjustHealth(hpBoss, -damage, maximumHealth);
        if (BossHealthBar != null) BossHealthBar.fillAmount = NoxRules.HealthRatio(hpBoss, maximumHealth);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Slash")
        {
            LifeCountBoss(1, 0.10f);
        }

    }

    void Start()
    {
        maximumHealth = Mathf.Max(1, hpBoss);
        player = GameObject.FindGameObjectWithTag("Player");

        agent = GetComponent<NavMeshAgent>();
        if (BossHealthBarObj != null) BossHealthBar = BossHealthBarObj.GetComponent<Image>();

    }

    void Update()
    {
        if (CanvasLifeBoss != null) CanvasLifeBoss.SetActive(!deathHandled && NoxSession.CanPlay);
        if (deathHandled) return;
        Morte();
        if (deathHandled || !NoxSession.CanPlay || player == null || agent == null || !agent.isOnNavMesh) return;

        #region Movimentação e Ataque
        transform.LookAt(player.transform.position);
        currentDistance = Vector3.Distance(transform.position, player.transform.position);

        if (currentDistance <= rangeAttack)
        {
            Attack();
        }

        else if (currentDistance <= maxDistance)

        {
            attack = false;
            if (areaAttack != null) areaAttack.enabled = false;
            agent.SetDestination(player.transform.position);
        }

        else
        {
            attack = false;
            if (areaAttack != null) areaAttack.enabled = false;
            agent.SetDestination(this.transform.position);
        }
        #endregion
    }





}
