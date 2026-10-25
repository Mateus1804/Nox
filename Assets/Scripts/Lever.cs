using Unity.VisualScripting;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject[] waypoints;
    public int currentWaypointIndex = 0;

    public bool playerCheck = false;
    public bool boxCheck = false;

    public float speed = 1f;

    public float distance;

    public GameObject door;

    #region Colliders
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerCheck = true;
        }

        if (other.gameObject.tag == "Box")
        {
            boxCheck = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerCheck = false;
        }

        if (other.gameObject.tag == "Box")
        {
            boxCheck = false;
        }
    }

    #endregion


    void Update()
    {
        //Se a variável "boxCheck" ou a variável "playerCheck" forem true, a porta sobe 
        if (playerCheck == true || boxCheck == true)
        {
            this.gameObject.GetComponent<Renderer>().material.color = Color.green;

            if (Vector3.Distance(door.transform.position, waypoints[currentWaypointIndex].transform.position) <= 0)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = 1;
                }
            }

            door.transform.position = Vector3.MoveTowards(door.transform.position, waypoints[currentWaypointIndex].transform.position, speed * Time.deltaTime);
        }

        //Se a variável"boxCheck" ou a variável "playerCheck" forem false, a porta desce
        if (playerCheck == false && boxCheck == false)
        {
            this.gameObject.GetComponent<Renderer>().material.color = Color.red;

            //Para que ela desça, declaramos que a variável "currentWaypointIndex" tem o valor de 0.
            currentWaypointIndex = 0;

            //Depois declaramos que o "transform" da porta, deveria se mover a posição atual do "transform" do waypoint "0", por uma velocidade fixa.
            door.transform.position = Vector3.MoveTowards(door.transform.position, waypoints[currentWaypointIndex].transform.position, (speed * 5) * Time.deltaTime);
        } 

        



    }
}
