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
    private NoxEnemyStrike strike;
    private readonly NavMeshPath patrolPath = new NavMeshPath();
    private float nextPatrol;
    private float nextRepath;

    private void Start()
    {
        strike = GetComponent<NoxEnemyStrike>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (anim == null) anim = GetComponent<Animator>();
        nextMove = transform.position;
    }
    private void Update()
    {
        if (strike != null && strike.Committed) { NoxChaseController.Stop(agent); return; }
        if (!NoxSession.CanPlay || player == null || !NoxChaseController.Ready(agent))
        { NoxChaseController.Stop(agent); return; }
        if (Time.time >= nextPatrol) { SortPosition(); nextPatrol = Time.time + 3; }
        yPlayer = player.transform.position.y;
        yEnemy = transform.position.y;
        currentDistance = Vector3.Distance(transform.position, player.transform.position);
        distanceToNextMove = Vector3.Distance(transform.position, nextMove);
        if (agent.isStopped || Time.time >= nextRepath)
        {
            nextRepath = Time.time + 0.2f;
            Movimentation(yEnemy, yPlayer, agent, nextMove, currentDistance, followDistance, player);
        }
    }
    public void SortPosition()
    {
        if (!NoxSession.CanPlay || !NoxChaseController.Ready(agent)) return;
        Vector3 sample = transform.position + new Vector3(Random.Range(-12f, 12f), 0, Random.Range(-13f, 10f));
        if (NavMesh.SamplePosition(sample, out NavMeshHit hit, 5f, agent.areaMask) &&
            agent.CalculatePath(hit.position, patrolPath) && patrolPath.status == NavMeshPathStatus.PathComplete)
            nextMove = hit.position;
    }
    public void Movimentation(float enemyYY, float playerYY, NavMeshAgent agentZ,
        Vector3 newMoviment, float distanciaAtual, float distanciaPersegue, GameObject player)
    {
        if (!NoxSession.CanPlay || !NoxChaseController.Ready(agentZ) || player == null) return;
        agentZ.isStopped = false;
        agentZ.SetDestination(Mathf.Abs(playerYY - enemyYY) <= 2f && distanciaAtual <= distanciaPersegue &&
            NoxPerception.CanSee(agentZ.transform, player.transform)
            ? player.transform.position : newMoviment);
    }
    private void OnDisable() { NoxChaseController.Stop(agent); }
}
