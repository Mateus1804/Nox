using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdSlash : MonoBehaviour
{
    public float speed = 0f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        speed = 4f;
        this.transform.position = transform.position - transform.right * speed * Time.deltaTime;

    }
}
