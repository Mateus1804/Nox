using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "CameraFollow")]
public class NoxFollowCamera : MonoBehaviour
{
    public GameObject target;
    [FormerlySerializedAs("speed")]
    public float responsiveness = 0.125f;
    public Vector3 offset;
    private Vector3 previousTarget, lead;
    private bool ready;
    private void Start()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player");
    }
    private void LateUpdate()
    {
        if (target == null)
            return;
        if (!ready)
        {
            previousTarget = target.transform.position;
            ready = true;
        }
        if (!NoxSession.CanPlay)
        {
            previousTarget = target.transform.position;
            return;
        }
        Vector3 velocity = (target.transform.position - previousTarget) / Mathf.Max(Time.deltaTime, 0.001f);
        previousTarget = target.transform.position;
        velocity.y = 0;
        Vector3 desiredLead = NoxPresentation.ReducedMotion ? Vector3.zero : Vector3.ClampMagnitude(velocity * 0.10f, 0.8f);
        lead = Vector3.Lerp(lead, desiredLead, 1 - Mathf.Exp(-5 * Time.deltaTime));
        float blend = 1f - Mathf.Exp(-Mathf.Max(0, responsiveness) * 50f * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, target.transform.position + offset + lead, blend);
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public float speed
    {
        get => responsiveness; set => responsiveness = value;
    }
}
