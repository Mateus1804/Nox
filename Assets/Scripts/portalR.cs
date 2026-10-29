using UnityEngine;

public class portalR : MonoBehaviour
{
    private const float Speed = 5f;
    public void PRotation()
    {
        transform.Rotate(0, 2f * Speed * Time.deltaTime, 0, Space.Self);
    }
    private void Update()
    {
        PRotation();
    }
}
