using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float radius;
    public float force;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 1);
    }
    public void ExplosionMethod()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(force, transform.position, radius);
            }
        }
    }

   
    void Update()
    {
        transform.position = transform.position + transform.forward * speed * Time.deltaTime;
    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Turtle")
        {
            Destroy(gameObject, 3);
        }

        if(other.gameObject.tag == "Eagle")
        {
            Destroy(gameObject, 3);
        }
    }
}
