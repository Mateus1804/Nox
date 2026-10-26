using UnityEngine;

public class LoadLevels : MonoBehaviour
{
    public static LoadLevels Instance;
    public int acessLevelFire, acessLevelWater, acessLevelLand;
    public GameObject buttonAirLevel, buttonFireLevel, buttonWaterLevel, buttonLandLevel;
    private int lastFire = -1, lastWater = -1, lastLand = -1;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        acessLevelFire = Mathf.Max(acessLevelFire, NoxProgress.FireUnlocked ? 1 : 0);
        acessLevelWater = Mathf.Max(acessLevelWater, NoxProgress.WaterUnlocked ? 1 : 0);
        acessLevelLand = Mathf.Max(acessLevelLand, NoxProgress.LandUnlocked ? 1 : 0);
        if (acessLevelFire > 0 && !NoxProgress.FireUnlocked) NoxProgress.Unlock("Fire");
        if (acessLevelWater > 0 && !NoxProgress.WaterUnlocked) NoxProgress.Unlock("Water");
        if (acessLevelLand > 0 && !NoxProgress.LandUnlocked) NoxProgress.Unlock("Land");
        FireLevelUnlocked(); WaterLevelUnlocked(); LandLevelUnlocked();
    }
    public void FireLevelUnlocked() { if (buttonFireLevel != null) buttonFireLevel.SetActive(acessLevelFire > 0); lastFire = acessLevelFire; }
    public void WaterLevelUnlocked() { if (buttonWaterLevel != null) buttonWaterLevel.SetActive(acessLevelWater > 0); lastWater = acessLevelWater; }
    public void LandLevelUnlocked() { if (buttonLandLevel != null) buttonLandLevel.SetActive(acessLevelLand > 0); lastLand = acessLevelLand; }
    private void Update()
    {
        // Compatibilidade com campos públicos alterados por eventos/scripts legados.
        if (lastFire != acessLevelFire) FireLevelUnlocked();
        if (lastWater != acessLevelWater) WaterLevelUnlocked();
        if (lastLand != acessLevelLand) LandLevelUnlocked();
    }
    public void UnlockFire() { acessLevelFire = 1; NoxProgress.Unlock("Fire"); FireLevelUnlocked(); }
    public void UnlockWater() { acessLevelWater = 1; NoxProgress.Unlock("Water"); WaterLevelUnlocked(); }
    public void UnlockLand() { acessLevelLand = 1; NoxProgress.Unlock("Land"); LandLevelUnlocked(); }
    public void ButtonLevelWater() { Debug.Log("A fase de água ainda não faz parte desta versão."); }
    public void ButtonLevelLand() { Debug.Log("A fase de terra ainda não faz parte desta versão."); }
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
