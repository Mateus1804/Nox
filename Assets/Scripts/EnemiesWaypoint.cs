using UnityEngine;

public class EnemiesWaypoint : MonoBehaviour
{
    public EnemyAir turtle;
    public GameObject[] waypoints;
    private int currentWaypointIndex;
    public float speed = 1f;
    public float distance;

    private void Update()
    {
        if (!NoxSession.CanPlay || turtle == null || turtle.hpTurtle <= 0 || waypoints == null || waypoints.Length == 0) return;
        if (!NoxRules.IsValidIndex(currentWaypointIndex, waypoints.Length)) currentWaypointIndex = 0;
        GameObject target = waypoints[currentWaypointIndex];
        if (target == null) return;
        distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance <= 1)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            target = waypoints[currentWaypointIndex];
            if (target == null) return;
            transform.Rotate(Vector3.up * 180f);
        }
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position,
            Mathf.Max(0, speed) * Time.deltaTime);
    }
}
