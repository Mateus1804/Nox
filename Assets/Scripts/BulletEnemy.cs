using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BulletEnemy : MonoBehaviour

{
    public float speed;
    public float radius;
    public float force;


    #region OnDrawGizmos
    void OnDrawGizmos()
    {
        // Area da colisão do tiro
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);


    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            BulletCollision();
        }
    }
    #endregion

    #region BulletCollision
    public void BulletCollision()
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
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 5);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + transform.forward * speed * Time.deltaTime;
    }
}
