using System.Collections.Generic;
using UnityEngine;

public class TutorialMethod : MonoBehaviour
{
    public TutorialList list;
    public BoxCollider range;
    public int i;
    private bool completed;
    private bool shown;
    private readonly HashSet<Collider> visitors = new HashSet<Collider>();

    private bool TryPanel(out GameObject panel)
    {
        panel = null;
        if (list == null || list.tutorials == null || !NoxRules.IsValidIndex(i, list.tutorials.Count))
            return false;
        panel = list.tutorials[i];
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
        if (!NoxSession.CanPlay || !TryPanel(out GameObject panel) || i > list.colliderCount)
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
        list.colliderCount = Mathf.Max(list.colliderCount, i + 1);
        completed = true;
        shown = false;
        if (range != null)
            range.enabled = false;
    }
    private void OnDisable()
    {
        visitors.Clear();
        if (shown && TryPanel(out GameObject panel))
            panel.SetActive(false);
        shown = false;
    }
}
