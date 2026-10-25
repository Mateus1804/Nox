using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    public float speed = 0.125f;
    public Vector3 offset;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }


    void FixedUpdate()
    {
        // Posição deesejada e a própria posição + um ajuste(offset)
        Vector3 desiredPosition = target.transform.position + offset;

        // Faz uma transição suave entre a posição inicial e a posição final
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, speed);

        // Atualiza a posição da câmerda
        transform.position = smoothedPosition;

    }
}
