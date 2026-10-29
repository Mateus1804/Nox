using UnityEngine;
using UnityEngine.SceneManagement;

public class BulletEnemy : MonoBehaviour
{
    public float speed;
    public float radius;
    public float force;
    private bool consumed;
    private Transform owner;
    private Rigidbody body;
    private readonly RaycastHit[] sweepHits = new RaycastHit[32];
    public void Configure(Transform source)
    {
        owner = source;
        body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0, radius));
    }
    public void OnTriggerEnter(Collider other)
    {
        Hit(other);
    }
    public void Hit(Collider other)
    {
        if (consumed || other == null || !NoxSession.CanPlay)
            return;
        PlayerScript player = other.GetComponentInParent<PlayerScript>();
        PlayerTwo alternate = other.GetComponentInParent<PlayerTwo>();
        if (owner != null && other.transform.IsChildOf(owner))
            return;
        if (player == null && alternate == null)
        {
            if (!other.isTrigger && !other.transform.IsChildOf(transform))
                Consume();
            return;
        }
        consumed = true;
        foreach (Collider item in GetComponentsInChildren<Collider>())
            item.enabled = false;
        if (player != null)
            player.TakeDamage(1f);
        else
            alternate.TakeDamage(1f);
        BulletCollision();
        Destroy(gameObject);
    }
    public void BulletCollision()
    {
        NoxExplosion.Apply(transform.position, radius, force);
    }
    private void Start()
    {
        Destroy(gameObject, 5);
    }
    private void Consume()
    {
        if (consumed)
            return;
        consumed = true;
        foreach (Collider item in GetComponentsInChildren<Collider>())
            item.enabled = false;
        Destroy(gameObject);
    }
    private void FixedUpdate()
    {
        if (!NoxSession.CanPlay || consumed)
            return;
        Vector3 step = transform.forward * speed * Time.fixedDeltaTime;
        float distance = step.magnitude;
        if (distance < 0.0001f)
            return;
        int count = gameObject.scene.GetPhysicsScene().Raycast(transform.position, step.normalized,
            sweepHits, distance, ~0, QueryTriggerInteraction.Ignore);
        Collider nearest = null;
        float best = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = sweepHits[i];
            if (hit.collider.transform.IsChildOf(transform) || (owner != null && hit.collider.transform.IsChildOf(owner)))
                continue;
            if (hit.distance < best)
            {
                nearest = hit.collider;
                best = hit.distance;
            }
        }
        if (count == sweepHits.Length)
        {
            Consume();
            return;
        }
        if (nearest != null)
        {
            Hit(nearest);
            return;
        }
        if (body != null)
            body.MovePosition(body.position + step);
        else
            transform.position += step;
    }
}
