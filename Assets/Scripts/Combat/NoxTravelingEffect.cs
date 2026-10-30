using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "ThirdSlash")]
public class NoxTravelingEffect : MonoBehaviour
{
    public float speed = 0;
    private void Start()
    {
        speed = 4f;
        Destroy(gameObject, 1.5f);
    }
    private void Update()
    {
        if (NoxSession.CanPlay)
            transform.position -= transform.right * speed * Time.deltaTime;
    }
}
