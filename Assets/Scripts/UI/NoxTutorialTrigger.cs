using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "TutorialMethod")]
public class NoxTutorialTrigger : MonoBehaviour
{
    [FormerlySerializedAs("list")]
    public NoxTutorialSequence sequence;
    [FormerlySerializedAs("range")]
    public BoxCollider triggerArea;
    [FormerlySerializedAs("i")]
    public int stepIndex;
    private bool completed;
    private bool shown;
    private readonly HashSet<Collider> visitors = new HashSet<Collider>();

    private bool TryPanel(out GameObject panel)
    {
        panel = null;
        if (sequence == null || sequence.tutorials == null || !NoxRules.IsValidIndex(stepIndex, sequence.tutorials.Count))
            return false;
        panel = sequence.tutorials[stepIndex];
        return panel != null;
    }
    public void OnTriggerEnter(Collider other)
    {
        Visit(other);
    }
    private void OnTriggerStay(Collider other)
    {
        Visit(other);
    }
    private void Visit(Collider other)
    {
        if (completed || other == null || !other.CompareTag("Player"))
            return;
        visitors.Add(other);
        if (!NoxSession.CanPlay || !TryPanel(out GameObject panel) || stepIndex > sequence.colliderCount)
            return;
        panel.SetActive(true);
        shown = true;
    }
    public void OnTriggerExit(Collider other)
    {
        if (other == null || !visitors.Remove(other))
            return;
        if (visitors.Count != 0 || completed || !shown || !TryPanel(out GameObject panel))
            return;
        panel.SetActive(false);
        sequence.colliderCount = Mathf.Max(sequence.colliderCount, stepIndex + 1);
        completed = true;
        shown = false;
        if (triggerArea != null)
            triggerArea.enabled = false;
    }
    private void OnDisable()
    {
        visitors.Clear();
        if (shown && TryPanel(out GameObject panel))
            panel.SetActive(false);
        shown = false;
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public NoxTutorialSequence list
    {
        get => sequence; set => sequence = value;
    }
    public BoxCollider range
    {
        get => triggerArea; set => triggerArea = value;
    }
    public int i
    {
        get => stepIndex; set => stepIndex = value;
    }
}
