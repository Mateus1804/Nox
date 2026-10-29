using System.Collections.Generic;
using UnityEngine;

public static class NoxExplosion
{
    public static void Apply(Vector3 position, float radius, float force)
    {
        if (radius <= 0 || !NoxRules.IsFinite(radius) || !NoxRules.IsFinite(force))
            return;
        var affected = new HashSet<Rigidbody>();
        foreach (Collider collider in Physics.OverlapSphere(position, radius))
        {
            Rigidbody body = collider.attachedRigidbody;
            if (body != null && !body.isKinematic && affected.Add(body))
                body.AddExplosionForce(force, position, radius);
        }
    }
}
