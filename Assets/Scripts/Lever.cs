using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject[] waypoints;
    public int currentWaypointIndex;
    public bool playerCheck;
    public bool boxCheck;
    public float speed = 1f;
    public float distance;
    public GameObject door;
    public TextMesh stateLabel;
    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private Renderer indicator;
    private Material indicatorMaterial;
    private bool wasOccupied;

    private void Awake()
    {
        indicator = GetComponent<Renderer>();
        if (indicator != null)
        {
            indicatorMaterial = indicator.material;
            indicatorMaterial.color = Color.red;
        }
        if (stateLabel != null) stateLabel.text = "Aguardando peso";
        if (door == null || waypoints == null || waypoints.Length < 2 || waypoints[0] == null || waypoints[1] == null)
        {
            Debug.LogError("A alavanca exige uma porta e dois pontos válidos.", this);
            enabled = false;
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box")) occupants.Add(other);
    }
    public void OnTriggerExit(Collider other) { occupants.Remove(other); }
    private void Update()
    {
        if (!NoxSession.CanPlay) return;
        occupants.RemoveWhere(item => item == null || !item.enabled || !item.gameObject.activeInHierarchy);
        playerCheck = false;
        boxCheck = false;
        foreach (Collider item in occupants)
        {
            playerCheck |= item.CompareTag("Player");
            boxCheck |= item.CompareTag("Box");
        }
        bool occupied = playerCheck || boxCheck;
        if (indicatorMaterial != null && occupied != wasOccupied)
            indicatorMaterial.color = occupied ? Color.green : Color.red;
        if (stateLabel != null && occupied != wasOccupied) stateLabel.text = occupied ? "Ativada" : "Aguardando peso";
        wasOccupied = occupied;
        currentWaypointIndex = occupied ? 1 : 0;
        door.transform.position = Vector3.MoveTowards(door.transform.position,
            waypoints[currentWaypointIndex].transform.position, speed * (occupied ? 1 : 5) * Time.deltaTime);
    }
    private void OnDisable()
    {
        occupants.Clear(); playerCheck = false; boxCheck = false; wasOccupied = false;
        if (stateLabel != null) stateLabel.text = "Aguardando peso";
        if (indicatorMaterial != null) indicatorMaterial.color = Color.red;
    }
    private void OnDestroy() { if (indicatorMaterial != null) Destroy(indicatorMaterial); }
}
