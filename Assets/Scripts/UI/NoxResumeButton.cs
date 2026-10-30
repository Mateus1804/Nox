using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoxResumeButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI label;
    private void OnEnable()
    {
        if (button != null)
            button.interactable = NoxProgress.CanResume;
        if (label != null)
            label.text = NoxProgress.CanResume ? "Continuar fase" : "Sem fase salva";
    }
}
