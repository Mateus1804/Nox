using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "SlashScript")]
public class NoxMeleeEffect : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 0.3f);
    }
}
