using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;


public class BossScript : MonoBehaviour
{

    #region Variaveis > Agent, Player, Animin
    private bool deathHandled;
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
        if (other.gameObject.tag == "Slash")
        {
            BarCount(1, 0.20f);
        }
    }
    #endregion



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

    #region BarCount
    public void BarCount(int damage, float amount)
    {
        if (deathHandled || !NoxSession.CanPlay || damage <= 0) return;
        hp = (int)NoxRules.AdjustHealth(hp, -damage, maximumHealth);
        if (healthBar != null) healthBar.fillAmount = NoxRules.HealthRatio(hp, maximumHealth);
    }
    #endregion

    public void BossDeath()
    {
        if (deathHandled || hp > 0) return;
        deathHandled = true;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (areaAttack != null) areaAttack.enabled = false;
        if (LoadLevels.Instance != null) LoadLevels.Instance.UnlockFire();
        if (PlaceHolderPanel.PHP != null) PlaceHolderPanel.PHP.PanelWinOrLose("Stage Completed");
        Destroy(gameObject);
    }

    public void Persegue()
    {
        agent.SetDestination(player.transform.position);
    }

    void Start()
    {
        maximumHealth = Mathf.Max(1, hp);
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();

    }

    void Update()
    {
        if (healthBarBg != null) healthBarBg.SetActive(!deathHandled && NoxSession.CanPlay);
        if (deathHandled) return;
        BossDeath();
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
