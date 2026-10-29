using UnityEngine;

[DefaultExecutionOrder(-200)]
[RequireComponent(typeof(Rigidbody))]
public class WaypointsAirPhase : MonoBehaviour
{
    public GameObject[] waypoints;
    public float speed = 1f;
    public float distance;
    private int currentWaypointIndex;
    private Rigidbody body;
    public Vector3 SupportVelocity
    {
        get; private set;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        SupportVelocity = Vector3.zero;
        if (!NoxSession.CanPlay || waypoints == null || waypoints.Length == 0)
            return;
        if (!NoxRules.IsValidIndex(currentWaypointIndex, waypoints.Length))
            currentWaypointIndex = 0;
        GameObject target = waypoints[currentWaypointIndex];
        if (target == null)
            return;
        distance = Vector3.Distance(body.position, target.transform.position);
        if (distance <= 0.01f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            target = waypoints[currentWaypointIndex];
            if (target == null)
                return;
        }
        Vector3 next = Vector3.MoveTowards(body.position, target.transform.position,
            Mathf.Max(0, speed) * Time.fixedDeltaTime);
        SupportVelocity = (next - body.position) / Time.fixedDeltaTime;
        body.MovePosition(next);
    }

    private void OnDisable()
    {
        SupportVelocity = Vector3.zero;
    }
}
