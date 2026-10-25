using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform cam;
    void Update()
    {
        if (cam == null) return;
        transform.LookAt(transform.position + cam.forward);
    }
}
