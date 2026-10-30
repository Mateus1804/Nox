using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "TutorialList")]
public class NoxTutorialSequence : MonoBehaviour
{
    [FormerlySerializedAs("colliderCount")]
    public int nextStep = 0;

    public List<GameObject> tutorials = new List<GameObject>();
    // API de compatibilidade para os dados e callbacks recuperados.
    public int colliderCount
    {
        get => nextStep; set => nextStep = value;
    }
}
