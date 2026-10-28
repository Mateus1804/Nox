using UnityEngine;

public class portaldentro : MonoBehaviour
{
    private const float Speed = 5f;
    public void InRotation() { transform.Rotate(0, -2f * Speed * Time.deltaTime, 0, Space.Self); }
    private void Update() { InRotation(); }
}
