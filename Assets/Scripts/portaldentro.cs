using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portaldentro : MonoBehaviour
{
    float speed = 5f;

    public void InRotation()
    {
        gameObject.transform.Rotate(0.00f, -2f * speed * Time.deltaTime, 0.0f, Space.Self);
    }

    void Start()
    {

    }

    void Update()
    {
        InRotation();
    }
}
