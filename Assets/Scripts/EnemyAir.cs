using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAir : MonoBehaviour
{
    public Animator animin;
    Image TurtleHealthBar;
    public float hpTurtle;


    public void Morte()
    {
        if (hpTurtle <= 0)
        {
            animin.SetBool("IsDie", true);
            gameObject.GetComponent<SphereCollider>().enabled = false;
            Destroy(gameObject, 1.5f);
        }
    }


    public void LifeCountTurtle(float damage)
    {

        hpTurtle -= damage;

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Slash")
        {
            LifeCountTurtle(1);
            Debug.Log("Acertou miserável!");
        }
        else
        {
            animin.SetBool("Hit", false);
        }
    }

    void Start()
    {
        animin = GetComponent<Animator>();
    }

    void Update()
    {
        Morte();
    }

  
}
