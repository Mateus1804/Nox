using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdSlash : MonoBehaviour
{
    public float speed = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        speed = Mathf.Lerp(speed, 4, 1);
        this.transform.position = transform.position - transform.right * speed * Time.deltaTime;
        Destroy(this.gameObject, 1.5f);
    }
}
