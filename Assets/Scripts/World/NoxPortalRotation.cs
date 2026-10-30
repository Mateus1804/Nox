using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "portalR")]
public class NoxPortalRotation : MonoBehaviour
{
    public float degreesPerSecond = 10f;
    public virtual float AngularSpeed => degreesPerSecond;
    public void PRotation()
    {
        transform.Rotate(0, AngularSpeed * Time.deltaTime, 0, Space.Self);
    }
    public void InRotation() => PRotation();

    protected virtual void Update()
    {
        PRotation();
    }
}
