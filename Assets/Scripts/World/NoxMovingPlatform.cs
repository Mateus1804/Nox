using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[DefaultExecutionOrder(-200)]
[RequireComponent(typeof(Rigidbody))]
[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "WaypointsAirPhase")]
public class NoxMovingPlatform : MonoBehaviour
{
    public GameObject[] waypoints;
    [FormerlySerializedAs("speed")]
    public float movementSpeed = 1f;
    [FormerlySerializedAs("distance")]
    public float distanceToWaypoint;
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
        distanceToWaypoint = Vector3.Distance(body.position, target.transform.position);
        if (distanceToWaypoint <= 0.01f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            target = waypoints[currentWaypointIndex];
            if (target == null)
                return;
        }
        Vector3 next = Vector3.MoveTowards(body.position, target.transform.position,
            Mathf.Max(0, movementSpeed) * Time.fixedDeltaTime);
        SupportVelocity = (next - body.position) / Time.fixedDeltaTime;
        body.MovePosition(next);
    }

    private void OnDisable()
    {
        SupportVelocity = Vector3.zero;
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public float speed
    {
        get => movementSpeed; set => movementSpeed = value;
    }
    public float distance
    {
        get => distanceToWaypoint; set => distanceToWaypoint = value;
    }
}
