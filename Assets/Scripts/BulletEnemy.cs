using UnityEngine;

public class BulletEnemy : MonoBehaviour
{
    public float speed;
    public float radius;
    public float force;
    private bool consumed;
    private void OnDrawGizmos() { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, Mathf.Max(0, radius)); }
    public void OnTriggerEnter(Collider other) { Hit(other); }
    public void Hit(Collider other)
    {
        if (consumed || other == null || !NoxSession.CanPlay) return;
        PlayerScript player = other.GetComponentInParent<PlayerScript>();
        PlayerTwo alternate = other.GetComponentInParent<PlayerTwo>();
        if (player == null && alternate == null) return;
        consumed = true;
        foreach (Collider item in GetComponentsInChildren<Collider>()) item.enabled = false;
        if (player != null) player.TakeDamage(1f); else alternate.TakeDamage(1f);
        BulletCollision();
        Destroy(gameObject);
    }
    public void BulletCollision() { NoxExplosion.Apply(transform.position, radius, force); }
    private void Start() { Destroy(gameObject, 5); }
    private void Update()
    {
        if (NoxSession.CanPlay && !consumed) transform.position += transform.forward * speed * Time.deltaTime;
    }
}
