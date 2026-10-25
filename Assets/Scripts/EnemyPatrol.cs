using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;

    public Vector3 nextMove;
    public float distanceToNextMove;
    public Animator anim;

    public float yPlayer;
    public float yEnemy;
    public float followDistance = 20;
    public float currentDistance;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        nextMove = transform.position;
        InvokeRepeating(nameof(SortPosition), 0, 3);
        anim = GetComponent<Animator> ();
    }

    void Update()
    {
        if (!NoxSession.CanPlay || player == null || agent == null || !agent.isOnNavMesh) return;
        yPlayer = player.transform.position.y;
        yEnemy = transform.position.y;

        currentDistance = Vector3.Distance(transform.position, player.transform.position);
        distanceToNextMove = Vector3.Distance(transform.position, nextMove);
        Movimentation(yEnemy, yPlayer, agent, nextMove, currentDistance, followDistance, player);
    }

    public void SortPosition()
    {
        Vector3 sample = transform.position + new Vector3(Random.Range(-12f, 12f), 0, Random.Range(-13f, 10f));
        if (NavMesh.SamplePosition(sample, out NavMeshHit hit, 5f, NavMesh.AllAreas)) nextMove = hit.position;
    }

    public void Movimentation(float enemyYY, float playerYY, NavMeshAgent agentZ, Vector3 newMoviment, float distanciaAtual, float distanciaPersegue, GameObject player)
    {
        if (playerYY > enemyYY)
        {
            agentZ.SetDestination(newMoviment);
        }
        else if (distanciaAtual <= distanciaPersegue)
        {
            agentZ.SetDestination(player.transform.position);
        }
        else
        {
            agentZ.SetDestination(newMoviment);
        }
    }

}
