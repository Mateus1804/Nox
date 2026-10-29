using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "MovableObjectsScript")]
public class NoxCrateInteraction : MonoBehaviour
{
    [FormerlySerializedAs("playerScript")]
    public NoxPlayerController player;
    [FormerlySerializedAs("countdown")]
    [HideInInspector]
    public float legacyGripState; // Contrato serializado legado.
    private readonly HashSet<Collider> candidates = new HashSet<Collider>();
    private Rigidbody heldBody;
    private FixedJoint grip;
    private bool originalKinematic;
    public bool Holding => heldBody != null;
    public bool HasCandidate
    {
        get
        {
            foreach (Collider item in candidates)
                if (item != null && item.enabled && item.gameObject.activeInHierarchy && item.attachedRigidbody != null &&
                    item.attachedRigidbody.CompareTag("Box"))
                    return true;
            return false;
        }
    }

    private void Awake()
    {
        if (player == null)
            player = GetComponent<NoxPlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InRange") && other.attachedRigidbody != null)
            candidates.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        candidates.Remove(other);
    }

    public void ReadGrabInput()
    {
        if (player == null || !player.IsAlive || !NoxSession.CanPlay || !Input.GetKeyDown(KeyCode.E))
            return;
        if (heldBody != null)
        {
            Drop();
            return;
        }
        if (!player.grounded)
            return;
        candidates.RemoveWhere(item => item == null || !item.enabled || !item.gameObject.activeInHierarchy);
        Rigidbody selected = null;
        float nearest = float.PositiveInfinity;
        foreach (Collider candidate in candidates)
        {
            Rigidbody body = candidate.attachedRigidbody;
            if (body == null || body == player.rigid || !body.CompareTag("Box"))
                continue;
            float distance = (body.position - transform.position).sqrMagnitude;
            if (distance < nearest)
            {
                nearest = distance;
                selected = body;
            }
        }
        if (selected == null)
            return;
        heldBody = selected;
        originalKinematic = heldBody.isKinematic;
        heldBody.isKinematic = false;
        // A junta mantém a distância sem herdar escala nem atravessar obstáculos por teletransporte.
        grip = heldBody.gameObject.AddComponent<FixedJoint>();
        grip.connectedBody = player.rigid;
        grip.enableCollision = false;
        grip.breakForce = 2000f;
        grip.breakTorque = 2000f;
        player.SetPushing(true);
        legacyGripState = 1;
    }

    private void Drop()
    {
        if (grip != null)
        {
            grip.connectedBody = null;
            Destroy(grip);
        }
        if (heldBody != null)
            heldBody.isKinematic = originalKinematic;
        heldBody = null;
        grip = null;
        legacyGripState = 0;
        if (player != null)
            player.SetPushing(false);
    }

    private void Update()
    {
        if (player == null || !player.IsAlive || (legacyGripState != 0 && (heldBody == null || grip == null)))
        {
            Drop();
            return;
        }
        ReadGrabInput();
    }
    private void OnDisable()
    {
        Drop();
        candidates.Clear();
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public NoxPlayerController playerScript
    {
        get => player; set => player = value;
    }
    public float countdown
    {
        get => legacyGripState; set => legacyGripState = value;
    }

    // Nomes serializados nos UnityEvents de cenas anteriores.
    public void Player_GrabDrop() => ReadGrabInput();
}
