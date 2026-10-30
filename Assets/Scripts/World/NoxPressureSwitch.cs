using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "Lever")]
public class NoxPressureSwitch : MonoBehaviour
{
    public GameObject[] waypoints;
    public int currentWaypointIndex;
    [FormerlySerializedAs("playerCheck")]
    public bool playerOnSwitch;
    [FormerlySerializedAs("boxCheck")]
    public bool crateOnSwitch;
    [FormerlySerializedAs("speed")]
    public float doorSpeed = 1f;
    [FormerlySerializedAs("distance")]
    [HideInInspector]
    public float legacyDistance;
    [FormerlySerializedAs("door")]
    public GameObject controlledDoor;
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
        if (stateLabel != null)
            stateLabel.text = "Aguardando peso";
        if (controlledDoor == null || waypoints == null || waypoints.Length < 2 || waypoints[0] == null || waypoints[1] == null)
        {
            Debug.LogError("A alavanca exige uma porta e dois pontos válidos.", this);
            enabled = false;
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Box"))
            occupants.Add(other);
    }
    public void OnTriggerExit(Collider other)
    {
        occupants.Remove(other);
    }
    private void Update()
    {
        if (!NoxSession.CanPlay)
            return;
        occupants.RemoveWhere(item => item == null || !item.enabled || !item.gameObject.activeInHierarchy);
        playerOnSwitch = false;
        crateOnSwitch = false;
        foreach (Collider item in occupants)
        {
            playerOnSwitch |= item.CompareTag("Player");
            crateOnSwitch |= item.CompareTag("Box");
        }
        bool occupied = playerOnSwitch || crateOnSwitch;
        if (indicatorMaterial != null && occupied != wasOccupied)
            indicatorMaterial.color = occupied ? Color.green : Color.red;
        if (stateLabel != null && occupied != wasOccupied)
            stateLabel.text = occupied ? "Ativada" : "Aguardando peso";
        wasOccupied = occupied;
        currentWaypointIndex = occupied ? 1 : 0;
        controlledDoor.transform.position = Vector3.MoveTowards(controlledDoor.transform.position,
            waypoints[currentWaypointIndex].transform.position, doorSpeed * (occupied ? 1 : 5) * Time.deltaTime);
    }
    private void OnDisable()
    {
        occupants.Clear();
        playerOnSwitch = false;
        crateOnSwitch = false;
        wasOccupied = false;
        if (stateLabel != null)
            stateLabel.text = "Aguardando peso";
        if (indicatorMaterial != null)
            indicatorMaterial.color = Color.red;
    }
    private void OnDestroy()
    {
        if (indicatorMaterial != null)
            Destroy(indicatorMaterial);
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public GameObject door
    {
        get => controlledDoor; set => controlledDoor = value;
    }
    public float speed
    {
        get => doorSpeed; set => doorSpeed = value;
    }
    public float distance
    {
        get => legacyDistance; set => legacyDistance = value;
    }
    public bool playerCheck
    {
        get => playerOnSwitch; set => playerOnSwitch = value;
    }
    public bool boxCheck
    {
        get => crateOnSwitch; set => crateOnSwitch = value;
    }
}
