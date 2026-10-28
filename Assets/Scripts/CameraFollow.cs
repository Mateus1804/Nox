using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;
    public float speed = 0.125f;
    public Vector3 offset;
    private void Start() { if (target == null) target = GameObject.FindGameObjectWithTag("Player"); }
    private void LateUpdate()
    {
        if (target == null) return;
        float blend = 1f - Mathf.Exp(-Mathf.Max(0, speed) * 50f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, target.transform.position + offset, blend);
    }
}
