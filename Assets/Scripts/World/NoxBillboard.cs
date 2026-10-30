using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "Billboard")]
public class NoxBillboard : MonoBehaviour
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
