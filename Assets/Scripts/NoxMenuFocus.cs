using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// O foco acompanha painéis abertos; o último filho ativo vence após a atualização de layout.
public sealed class NoxMenuFocus : MonoBehaviour
{
    private IEnumerator StartFocus()
    {
        yield return null;
        if (!isActiveAndEnabled || EventSystem.current == null)
            yield break;
        Selectable[] controls = GetComponentsInChildren<Selectable>();
        foreach (Selectable control in controls)
            if (control.IsActive() && control.IsInteractable())
            {
                EventSystem.current.SetSelectedGameObject(control.gameObject);
                break;
            }
    }
    private void OnEnable()
    {
        StartCoroutine(StartFocus());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        if (EventSystem.current == null)
            return;
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected != null && selected.transform.IsChildOf(transform))
            EventSystem.current.SetSelectedGameObject(null);
    }
}
