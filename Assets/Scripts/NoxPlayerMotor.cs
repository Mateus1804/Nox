using UnityEngine;
using UnityEngine.SceneManagement;

// Um único motor físico atende os dois controladores legados.
public sealed class NoxPlayerMotor
{
    private const float GroundTolerance = 0.18f;
    private readonly Rigidbody body;
    private readonly Collider collider;
    private readonly PhysicsScene physics;
    private readonly RaycastHit[] hits = new RaycastHit[16];
    private readonly PhysicMaterial originalMaterial;
    private readonly PhysicMaterial movementMaterial;
    private Collider lastSupport;
    private float lastSupportVerticalSpeed;
    public bool Grounded { get; private set; }

    public NoxPlayerMotor(Rigidbody body, Collider collider)
    {
        this.body = body;
        this.collider = collider;
        physics = body.gameObject.scene.GetPhysicsScene();
        body.constraints |= RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        if (collider == null) return;
        originalMaterial = collider.sharedMaterial;
        // Sem atrito lateral: pressionar uma parede não deve suspender o jogador.
        movementMaterial = new PhysicMaterial("Nox — movimento")
        {
            staticFriction = 0, dynamicFriction = 0, bounciness = 0,
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Minimum
        };
        collider.sharedMaterial = movementMaterial;
    }

    public void Step(Vector3 direction, float speed, bool jump, float jumpHeight, LayerMask groundMask)
    {
        Vector3 support = FindSupport(groundMask);
        Vector3 velocity = Vector3.ClampMagnitude(direction, 1f) * speed;
        velocity.x += support.x;
        velocity.z += support.z;
        velocity.y = Grounded ? support.y : body.velocity.y;
        if (jump && Grounded)
        {
            velocity.y = support.y + Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            Grounded = false;
        }
        body.velocity = velocity;
    }

    private Vector3 FindSupport(LayerMask groundMask)
    {
        bool wasGrounded = Grounded;
        Grounded = false;
        if (collider == null) return Vector3.zero;
        Bounds bounds = collider.bounds;
        float nearest = float.PositiveInfinity;
        Vector3 selectedVelocity = Vector3.zero;
        Collider selectedCollider = null;
        // Cinco amostras mantêm o apoio nas bordas sem aceitar paredes laterais.
        for (int sample = 0; sample < 5; sample++)
        {
            Vector3 origin = bounds.center;
            if (sample > 0)
            {
                origin.x += (sample % 2 == 0 ? 1 : -1) * bounds.extents.x * 0.65f;
                origin.z += (sample <= 2 ? 1 : -1) * bounds.extents.z * 0.65f;
            }
            int count = physics.Raycast(origin, Vector3.down, hits,
                bounds.extents.y + GroundTolerance, groundMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.rigidbody == body) continue;
                WaypointsAirPhase platform = hit.collider.GetComponentInParent<WaypointsAirPhase>();
                Vector3 velocity = platform != null && platform.isActiveAndEnabled
                    ? platform.SupportVelocity : hit.rigidbody != null ? hit.rigidbody.GetPointVelocity(hit.point) : Vector3.zero;
                float relativeY = body.velocity.y - (wasGrounded && hit.collider == lastSupport
                    ? lastSupportVerticalSpeed : velocity.y);
                if (!NoxRules.IsSupport(hit.normal.y, relativeY) || hit.distance >= nearest) continue;
                nearest = hit.distance;
                selectedVelocity = velocity;
                selectedCollider = hit.collider;
                Grounded = true;
            }
        }
        lastSupport = selectedCollider;
        lastSupportVerticalSpeed = selectedVelocity.y;
        return selectedVelocity;
    }

    public void Dispose()
    {
        if (collider != null && collider.sharedMaterial == movementMaterial) collider.sharedMaterial = originalMaterial;
        if (movementMaterial != null)
        {
            if (Application.isPlaying) Object.Destroy(movementMaterial);
            else Object.DestroyImmediate(movementMaterial);
        }
    }
}
