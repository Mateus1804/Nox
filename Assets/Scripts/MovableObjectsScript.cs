using System.Collections.Generic;
using UnityEngine;

public class MovableObjectsScript : MonoBehaviour
{
    public PlayerScript playerScript;
    public float countdown; // Contrato serializado legado.
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
                    item.attachedRigidbody.CompareTag("Box")) return true;
            return false;
        }
    }

    private void Awake()
    {
        if (playerScript == null) playerScript = GetComponent<PlayerScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InRange") && other.attachedRigidbody != null) candidates.Add(other);
    }
    private void OnTriggerExit(Collider other) { candidates.Remove(other); }

    public void Player_GrabDrop()
    {
        if (playerScript == null || !playerScript.IsAlive || !NoxSession.CanPlay || !Input.GetKeyDown(KeyCode.E)) return;
        if (heldBody != null) { Drop(); return; }
        if (!playerScript.grounded) return;
        candidates.RemoveWhere(item => item == null || !item.enabled || !item.gameObject.activeInHierarchy);
        Rigidbody selected = null;
        float nearest = float.PositiveInfinity;
        foreach (Collider candidate in candidates)
        {
            Rigidbody body = candidate.attachedRigidbody;
            if (body == null || body == playerScript.rigid || !body.CompareTag("Box")) continue;
            float distance = (body.position - transform.position).sqrMagnitude;
            if (distance < nearest) { nearest = distance; selected = body; }
        }
        if (selected == null) return;
        heldBody = selected;
        originalKinematic = heldBody.isKinematic;
        heldBody.isKinematic = false;
        // A junta mantém a distância sem herdar escala nem atravessar obstáculos por teletransporte.
        grip = heldBody.gameObject.AddComponent<FixedJoint>();
        grip.connectedBody = playerScript.rigid;
        grip.enableCollision = false;
        grip.breakForce = 2000f;
        grip.breakTorque = 2000f;
        playerScript.SetPushing(true);
        countdown = 1;
    }

    private void Drop()
    {
        if (grip != null)
        {
            grip.connectedBody = null;
            Destroy(grip);
        }
        if (heldBody != null) heldBody.isKinematic = originalKinematic;
        heldBody = null;
        grip = null;
        countdown = 0;
        if (playerScript != null) playerScript.SetPushing(false);
    }

    private void Update()
    {
        if (playerScript == null || !playerScript.IsAlive || (countdown != 0 && (heldBody == null || grip == null)))
        { Drop(); return; }
        Player_GrabDrop();
    }
    private void OnDisable() { Drop(); candidates.Clear(); }
}
