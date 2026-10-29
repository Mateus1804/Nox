using UnityEngine;
using UnityEngine.SceneManagement;

public static class NoxProgress
{
    private const string Prefix = "Nox.v1.";
    public static bool FireUnlocked => PlayerPrefs.GetInt(Prefix + "Fire", 0) > 0;
    public static bool WaterUnlocked => PlayerPrefs.GetInt(Prefix + "Water", 0) > 0;
    public static bool LandUnlocked => PlayerPrefs.GetInt(Prefix + "Land", 0) > 0;
    public static int LastScene => PlayerPrefs.GetInt(Prefix + "LastScene", -1);
    public static bool CanResume => NoxProgressRules.CanResume(LastScene, SceneManager.sceneCountInBuildSettings, FireUnlocked);
    public static void Unlock(string element)
    {
        if (element != "Fire" && element != "Water" && element != "Land")
            return;
        PlayerPrefs.SetInt(Prefix + element, 1);
        PlayerPrefs.Save();
    }
    public static void RecordScene(int index)
    {
        if (index <= 0 || index >= SceneManager.sceneCountInBuildSettings)
            return;
        PlayerPrefs.SetInt(Prefix + "LastScene", index);
        PlayerPrefs.Save();
    }
}
