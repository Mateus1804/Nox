using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;


public class BossScript : MonoBehaviour
{

    #region Variaveis > Agent, Player, Animin
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
            areaAttack.enabled = true;
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
        hp -= damage;
        healthBar.fillAmount -= amount;
    }
    #endregion

    public void BossDeath()
    {
        if (hp <= 0)
        {
            PlaceHolderPanel.PHP.PanelWinOrLose("Stage Completed");
            LoadLevels.Instance.acessLevelFire += 1;
            Destroy(gameObject);
        }

        if (PlaceHolderPanel.PHP.placeHolderPanel.activeSelf == true || Time.timeScale == 0)
        {
            healthBarBg.SetActive(false);
        }
        else
        {
            healthBarBg.SetActive(true);
        }
    }

    public void Persegue()
    {
        agent.SetDestination(player.transform.position);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();

    }

    void Update()
    {
        BossDeath();

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
            areaAttack.enabled = false;
            agent.SetDestination(player.transform.position);
        }

        else
        {
            agent.SetDestination(this.transform.position);
        }
        #endregion
    }
}