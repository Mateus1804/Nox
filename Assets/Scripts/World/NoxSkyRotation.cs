using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "RotateSky")]
public class NoxSkyRotation : MonoBehaviour
{
    public float rotateSpeed = 1.2f;

    void Update()
    {
        if (RenderSettings.skybox == null || !RenderSettings.skybox.HasProperty("_Rotation"))
            return;
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotateSpeed);
    }
}
