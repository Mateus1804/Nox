using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAir : MonoBehaviour
{
    public Animator animin;
    private bool deathHandled;
    public float hpTurtle;


    public void Morte()
    {
        if (deathHandled || hpTurtle > 0) return;
        deathHandled = true;
        if (animin != null) animin.SetBool("IsDie", true);
        foreach (Collider hitbox in GetComponents<Collider>()) hitbox.enabled = false;
        Destroy(gameObject, 1.5f);
    }


    public void LifeCountTurtle(float damage)
    {

        if (!deathHandled && NoxSession.CanPlay && damage > 0) hpTurtle = Mathf.Max(0, hpTurtle - damage);

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
            if (animin != null) animin.SetBool("Hit", false);
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
