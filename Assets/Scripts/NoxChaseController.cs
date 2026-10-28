using UnityEngine;
using UnityEngine.AI;

// Coordena perseguição/ataque sem sobrescrever o caminho a cada quadro.
public sealed class NoxChaseController
{
    private const float RepathInterval = 0.2f;
    private float nextRepath;
    public static bool Ready(NavMeshAgent agent)
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }
    public static void Stop(NavMeshAgent agent)
    {
        if (Ready(agent) && !agent.isStopped) agent.isStopped = true;
    }
    public bool Update(NavMeshAgent agent, Transform target, float attackRange,
        float followRange, float now, out float distance)
    {
        distance = target != null ? Vector3.Distance(agent != null ? agent.transform.position : target.position, target.position) : float.PositiveInfinity;
        if (!Ready(agent) || target == null || !NoxSession.CanPlay)
        { Stop(agent); return false; }
        NoxEnemyAction action = NoxRules.ChooseEnemyAction(distance, attackRange, followRange);
        if (action != NoxEnemyAction.Chase)
        { Stop(agent); return action == NoxEnemyAction.Attack; }
        bool resuming = agent.isStopped;
        agent.isStopped = false;
        if (resuming || now >= nextRepath)
        {
            nextRepath = now + RepathInterval;
            agent.SetDestination(target.position);
        }
        return false;
    }
}
