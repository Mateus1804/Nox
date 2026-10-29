using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.AI;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "EnemyPatrol")]
public class NoxEnemyNavigation : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;
    [FormerlySerializedAs("nextMove")]
    public Vector3 patrolTarget;
    [FormerlySerializedAs("distanceToNextMove")]
    public float distanceToPatrolTarget;
    [FormerlySerializedAs("anim")]
    public Animator animator;
    [FormerlySerializedAs("yPlayer")]
    public float playerHeight;
    [FormerlySerializedAs("yEnemy")]
    public float enemyHeight;
    [FormerlySerializedAs("followDistance")]
    public float followRange = 20;
    [FormerlySerializedAs("currentDistance")]
    public float targetDistance;
    private NoxEnemyStrike strike;
    private readonly NavMeshPath patrolPath = new NavMeshPath();
    private float nextPatrol;
    private float nextRepath;

    private void Start()
    {
        strike = GetComponent<NoxEnemyStrike>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        if (animator == null)
            animator = GetComponent<Animator>();
        patrolTarget = transform.position;
    }
    private void Update()
    {
        if (strike != null && strike.Committed)
        {
            NoxChaseController.Stop(agent);
            return;
        }
        if (!NoxSession.CanPlay || player == null || !NoxChaseController.Ready(agent))
        {
            NoxChaseController.Stop(agent);
            return;
        }
        if (Time.time >= nextPatrol)
        {
            SortPosition();
            nextPatrol = Time.time + 3;
        }
        playerHeight = player.transform.position.y;
        enemyHeight = transform.position.y;
        targetDistance = Vector3.Distance(transform.position, player.transform.position);
        distanceToPatrolTarget = Vector3.Distance(transform.position, patrolTarget);
        if (agent.isStopped || Time.time >= nextRepath)
        {
            nextRepath = Time.time + 0.2f;
            Movimentation(enemyHeight, playerHeight, agent, patrolTarget, targetDistance, followRange, player);
        }
    }
    public void SortPosition()
    {
        if (!NoxSession.CanPlay || !NoxChaseController.Ready(agent))
            return;
        Vector3 sample = transform.position + new Vector3(Random.Range(-12f, 12f), 0, Random.Range(-13f, 10f));
        if (NavMesh.SamplePosition(sample, out NavMeshHit hit, 5f, agent.areaMask) &&
            agent.CalculatePath(hit.position, patrolPath) && patrolPath.status == NavMeshPathStatus.PathComplete)
            patrolTarget = hit.position;
    }
    public void Movimentation(float enemyYY, float playerYY, NavMeshAgent agentZ,
        Vector3 newMoviment, float distanciaAtual, float distanciaPersegue, GameObject player)
    {
        if (!NoxSession.CanPlay || !NoxChaseController.Ready(agentZ) || player == null)
            return;
        agentZ.isStopped = false;
        agentZ.SetDestination(Mathf.Abs(playerYY - enemyYY) <= 2f && distanciaAtual <= distanciaPersegue &&
            NoxPerception.CanSee(agentZ.transform, player.transform)
            ? player.transform.position : newMoviment);
    }
    private void OnDisable()
    {
        NoxChaseController.Stop(agent);
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public Animator anim
    {
        get => animator; set => animator = value;
    }
    public float yPlayer
    {
        get => playerHeight; set => playerHeight = value;
    }
    public float yEnemy
    {
        get => enemyHeight; set => enemyHeight = value;
    }
    public Vector3 nextMove
    {
        get => patrolTarget; set => patrolTarget = value;
    }
    public float distanceToNextMove
    {
        get => distanceToPatrolTarget; set => distanceToPatrolTarget = value;
    }
    public float currentDistance
    {
        get => targetDistance; set => targetDistance = value;
    }
    public float followDistance
    {
        get => followRange; set => followRange = value;
    }
}
