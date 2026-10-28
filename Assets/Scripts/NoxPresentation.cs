using UnityEngine;

public static class NoxPresentation
{
    private const string Prefix = "Nox.v1.Presentation.";
    public static bool DamageFlash { get; private set; }
    public static bool ReducedMotion { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Load()
    {
        DamageFlash = PlayerPrefs.GetInt(Prefix + "DamageFlash", 1) != 0;
        ReducedMotion = PlayerPrefs.GetInt(Prefix + "ReducedMotion", 0) != 0;
    }
    public static void SetDamageFlash(bool enabled)
    { DamageFlash = enabled; PlayerPrefs.SetInt(Prefix + "DamageFlash", enabled ? 1 : 0); PlayerPrefs.Save(); }
    public static void SetReducedMotion(bool enabled)
    { ReducedMotion = enabled; PlayerPrefs.SetInt(Prefix + "ReducedMotion", enabled ? 1 : 0); PlayerPrefs.Save(); }
}
