using UnityEngine;

public class TutorialMethod : MonoBehaviour
{
    public TutorialList list;
    public BoxCollider range;
    public int i;
    private bool completed;

    public void OnTriggerEnter(Collider other)
    {
        if (completed || !other.CompareTag("Player") || list == null) return;
        // Cada trigger recebe um índice fixo ao preparar o projeto original.
        if (!NoxRules.IsValidIndex(i, list.tutorials.Count)) return;
        if (i > list.colliderCount) return;
        GameObject panel = list.tutorials[i];
        if (panel != null) panel.SetActive(true);
    }

    public void OnTriggerExit(Collider other)
    {
        if (completed || !other.CompareTag("Player") || list == null) return;
        if (!NoxRules.IsValidIndex(i, list.tutorials.Count) || i > list.colliderCount) return;
        GameObject panel = list.tutorials[i];
        if (panel != null) panel.SetActive(false);
        list.colliderCount = Mathf.Max(list.colliderCount, i + 1);
        completed = true;
        if (range != null) range.enabled = false;
    }
}
