using System.Collections.Generic;
using UnityEngine;

public class MovableObjectsScript : MonoBehaviour
{
    // Preservado para as referências serializadas das cenas originais.
    public PlayerScript playerScript;
    public float countdown;
    private readonly HashSet<Collider> candidates = new HashSet<Collider>();
    private Transform heldObject;
    private Transform originalParent;
    private Rigidbody heldBody;
    private bool originalKinematic;

    private void Awake()
    {
        if (playerScript == null) playerScript = GetComponent<PlayerScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InRange") && other.transform.parent != null) candidates.Add(other);
    }

    private void OnTriggerExit(Collider other) { candidates.Remove(other); }

    public void Player_GrabDrop()
    {
        if (playerScript == null || !playerScript.IsAlive || !NoxSession.CanPlay) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (heldObject != null) { Drop(); return; }
        if (!playerScript.grounded) return;
        candidates.RemoveWhere(item => item == null || !item.enabled || !item.gameObject.activeInHierarchy);
        Transform selected = null;
        float nearest = float.PositiveInfinity;
        foreach (Collider candidate in candidates)
        {
            if (candidate.transform.parent == null) continue;
            float distance = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distance < nearest) { nearest = distance; selected = candidate.transform.parent; }
        }
        if (selected == null) return;
        heldObject = selected;
        originalParent = heldObject.parent;
        heldBody = heldObject.GetComponent<Rigidbody>();
        if (heldBody != null)
        {
            originalKinematic = heldBody.isKinematic;
            heldBody.isKinematic = true;
        }
        heldObject.SetParent(transform, true);
        playerScript.SetPushing(true);
        countdown = 1;
    }

    private void Drop()
    {
        if (heldObject != null) heldObject.SetParent(originalParent, true);
        if (heldBody != null) heldBody.isKinematic = originalKinematic;
        heldObject = null;
        heldBody = null;
        countdown = 0;
        if (playerScript != null) playerScript.SetPushing(false);
    }

    private void Update()
    {
        if (playerScript != null && !playerScript.IsAlive) { Drop(); return; }
        if (heldObject == null && countdown != 0) Drop();
        Player_GrabDrop();
    }
    private void OnDisable() { Drop(); candidates.Clear(); }
}
