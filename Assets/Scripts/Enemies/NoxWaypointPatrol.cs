using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "EnemiesWaypoint")]
public class NoxWaypointPatrol : MonoBehaviour
{
    [FormerlySerializedAs("turtle")]
    public NoxEnemyHealth enemyHealth;
    public GameObject[] waypoints;
    private NoxEnemyStrike strike;
    private void Start()
    {
        strike = GetComponent<NoxEnemyStrike>();
    }
    private int currentWaypointIndex;
    public float speed = 1f;
    public float distance;

    private void Update()
    {
        if (strike != null && strike.Committed)
            return;
        if (!NoxSession.CanPlay || enemyHealth == null || enemyHealth.health <= 0 || waypoints == null || waypoints.Length == 0)
            return;
        if (!NoxRules.IsValidIndex(currentWaypointIndex, waypoints.Length))
            currentWaypointIndex = 0;
        GameObject target = waypoints[currentWaypointIndex];
        if (target == null)
            return;
        distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance <= 1)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            target = waypoints[currentWaypointIndex];
            if (target == null)
                return;
            transform.Rotate(Vector3.up * 180f);
        }
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position,
            Mathf.Max(0, speed) * Time.deltaTime);
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public NoxEnemyHealth turtle
    {
        get => enemyHealth; set => enemyHealth = value;
    }
}
