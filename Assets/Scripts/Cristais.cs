using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cristais : MonoBehaviour
{

    public AudioSource breakCristals;

    public void BreakCristal()
    {
        breakCristals.Play();
    }

    void Start()
    {

    }


    void Update()
    {

    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            BreakCristal();
            Destroy(gameObject);
        }
    }
}
