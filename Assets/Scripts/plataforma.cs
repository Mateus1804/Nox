using System.Collections.Generic;
using UnityEngine;

public class plataforma : MonoBehaviour
{
    public Transform platforms;
    private readonly Dictionary<Transform, Transform> parents = new Dictionary<Transform, Transform>();
    private readonly Dictionary<Transform, HashSet<Collider>> contacts = new Dictionary<Transform, HashSet<Collider>>();
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        Transform player = collision.rigidbody != null ? collision.rigidbody.transform : collision.transform;
        if (!contacts.TryGetValue(player, out HashSet<Collider> colliders))
        {
            colliders = new HashSet<Collider>();
            contacts.Add(player, colliders);
            parents.Add(player, player.parent);
            player.SetParent(platforms != null ? platforms : transform, true);
        }
        colliders.Add(collision.collider);
    }
    private void OnCollisionExit(Collision collision)
    {
        Transform player = collision.rigidbody != null ? collision.rigidbody.transform : collision.transform;
        if (!contacts.TryGetValue(player, out HashSet<Collider> colliders)) return;
        colliders.Remove(collision.collider);
        if (colliders.Count != 0) return;
        if (player != null) player.SetParent(parents[player], true);
        contacts.Remove(player);
        parents.Remove(player);
    }
    private void OnDisable()
    {
        foreach (var entry in parents) if (entry.Key != null) entry.Key.SetParent(entry.Value, true);
        parents.Clear();
        contacts.Clear();
    }
}
