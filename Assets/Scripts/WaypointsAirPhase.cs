using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsAirPhase : MonoBehaviour
{

    public GameObject[] waypoints;
    int currentWaypointIndex = 0;

    public float speed = 1f;

    public float distance;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!NoxSession.CanPlay || waypoints == null || waypoints.Length == 0) return;
        if (!NoxRules.IsValidIndex(currentWaypointIndex, waypoints.Length)) currentWaypointIndex = 0;
        if (waypoints[currentWaypointIndex] == null) return;

        distance = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position);

        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position) <= 1)
        {

            currentWaypointIndex++;


            if (currentWaypointIndex >= waypoints.Length)
            {

                currentWaypointIndex = 0;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, speed * Time.deltaTime);
    }
}
