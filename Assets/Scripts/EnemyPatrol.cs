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
        InvokeRepeating("SortPosition", 0, 3);
        anim = GetComponent<Animator> ();
    }

    void Update()
    {
        yPlayer = player.GetComponent<Transform>().position.y;
        yEnemy = transform.position.y;

        currentDistance = Vector3.Distance(transform.position, player.transform.position);
        distanceToNextMove = Vector3.Distance(transform.position, nextMove);
    }

    public void SortPosition()
    {
        nextMove = new Vector3(Random.Range(-12, 12), 0, Random.Range(10, -13));
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
