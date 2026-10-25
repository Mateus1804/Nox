using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
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
        if (hpBoss <= 0)
        {
            PlaceHolderPanel.PHP.PanelWinOrLose("Stage Completed");
            PlaceHolderPanel.PHP.buttonNextLevel.SetActive(false);
            LoadLevels.Instance.acessLevelWater += 1;
          //  HUDAndSounds.Singleton.InGameSound2Stop();
            Destroy(gameObject, 1.5f);
        }

        if (PlaceHolderPanel.PHP.placeHolderPanel.activeSelf == true || Time.timeScale == 0)
        {
            CanvasLifeBoss.SetActive(false);
        }
        else
        {
            CanvasLifeBoss.SetActive(true);
        }
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


    public void LifeCountBoss(float damage, float hp_amount)
    {

        hpBoss -= damage;

        BossHealthBar.fillAmount -= hp_amount;
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
        player = GameObject.FindGameObjectWithTag("Player");

        agent = GetComponent<NavMeshAgent>();
        BossHealthBar = BossHealthBarObj.GetComponent<Image>();

    }

    void Update()
    {
        Morte();

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
