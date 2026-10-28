using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float radius;
    public float force;
    public float speed;
    private bool consumed;
    private void Start() { Destroy(gameObject, 1); }
    public void ExplosionMethod() { NoxExplosion.Apply(transform.position, radius, force); }
    private void Update()
    {
        if (NoxSession.CanPlay && !consumed) transform.position += transform.forward * speed * Time.deltaTime;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (consumed || !NoxSession.CanPlay || other == null || (!other.CompareTag("Turtle") && !other.CompareTag("Eagle"))) return;
        consumed = true;
        foreach (Collider item in GetComponentsInChildren<Collider>()) item.enabled = false;
        Destroy(gameObject);
    }
}
