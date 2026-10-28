using UnityEngine;
using UnityEngine.SceneManagement;

public static class NoxPerception
{
    private static readonly RaycastHit[] hits = new RaycastHit[32];
    public static bool CanSee(Transform observer, Transform target)
    {
        if (observer == null || target == null) return false;
        Collider source = observer.GetComponent<Collider>();
        Collider destination = target.GetComponent<Collider>();
        Vector3 from = source != null && source.enabled ? source.bounds.center : observer.position + Vector3.up * 0.5f;
        Vector3 to = destination != null ? destination.bounds.center : target.position;
        Vector3 delta = to - from;
        if (delta.sqrMagnitude < 0.001f) return true;
        int count = observer.gameObject.scene.GetPhysicsScene().Raycast(from, delta.normalized, hits,
            delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
        if (count == hits.Length) return false; // Consulta saturada não confirma visão através de obstáculos.
        for (int i = 0; i < count; i++)
        {
            Transform item = hits[i].collider.transform;
            if (item.IsChildOf(observer) || item.IsChildOf(target)) continue;
            return false;
        }
        return true;
    }
}
