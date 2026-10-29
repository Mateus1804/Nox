using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform cam;
    private void Start()
    {
        if (cam == null && Camera.main != null)
            cam = Camera.main.transform;
    }
    private void LateUpdate()
    {
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(cam.forward, cam.up);
    }
}
