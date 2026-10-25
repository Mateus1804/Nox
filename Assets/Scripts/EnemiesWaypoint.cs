using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesWaypoint : MonoBehaviour
{
    public EnemyAir turtle;
    public GameObject[] waypoints;

    int currentWaypointIndex = 0;

    public float speed = 1f;

    public float distance;

    void Update()
    {
        #region Turtle
        if (this.turtle.hpTurtle > 0)
        {
            distance = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position);


            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position) <= 1)
            {

                currentWaypointIndex++;
                this.transform.Rotate(Vector3.up * 180f);

                if (currentWaypointIndex >= waypoints.Length)
                {

                    currentWaypointIndex = 0;
                }
            }
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex].transform.position, speed * Time.deltaTime);
        }

        #endregion



    }
}
