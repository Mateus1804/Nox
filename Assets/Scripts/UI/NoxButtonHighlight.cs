using TMPro;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

// Métodos públicos preservados para os EventTriggers das cenas.
[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "ChangeTextColor")]
public class NoxButtonHighlight : MonoBehaviour
{
    public Color originalColor;
    public Color secondColor = Color.white;
    public TextMeshProUGUI theText;
    private void Awake()
    {
        if (theText == null)
            theText = GetComponent<TextMeshProUGUI>();
        if (theText != null)
            originalColor = theText.color;
    }
    public void ChangeTheColor()
    {
        if (theText != null)
            theText.color = secondColor;
    }
    public void ResetTheColor()
    {
        if (theText != null)
            theText.color = originalColor;
    }
    private void OnDisable()
    {
        ResetTheColor();
    }
}
