using UnityEngine;
using UnityEngine.UI;

public sealed class NoxPresentationOptions : MonoBehaviour
{
    public Toggle damageFlash, reducedMotion;
    private void OnEnable()
    {
        if (damageFlash != null)
        {
            damageFlash.SetIsOnWithoutNotify(NoxPresentation.DamageFlash);
            damageFlash.onValueChanged.AddListener(NoxPresentation.SetDamageFlash);
        }
        if (reducedMotion != null)
        {
            reducedMotion.SetIsOnWithoutNotify(NoxPresentation.ReducedMotion);
            reducedMotion.onValueChanged.AddListener(NoxPresentation.SetReducedMotion);
        }
    }
    private void OnDisable()
    {
        if (damageFlash != null) damageFlash.onValueChanged.RemoveListener(NoxPresentation.SetDamageFlash);
        if (reducedMotion != null) reducedMotion.onValueChanged.RemoveListener(NoxPresentation.SetReducedMotion);
    }
}
