using UnityEngine;

public class RotateSky : MonoBehaviour
{
    public float rotateSpeed = 1.2f;

    void Update()
    {
        if (RenderSettings.skybox == null || !RenderSettings.skybox.HasProperty("_Rotation"))
            return;
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotateSpeed);
    }
}
