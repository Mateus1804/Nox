using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "LoadLevels")]
public class NoxLevelSelection : MonoBehaviour
{
    public static NoxLevelSelection Instance;
    [FormerlySerializedAs("acessLevelFire")]
    public int fireAccess;
    [FormerlySerializedAs("acessLevelWater")]
    public int waterAccess;
    [FormerlySerializedAs("acessLevelLand")]
    public int landAccess;
    public GameObject buttonAirLevel;
    public GameObject buttonFireLevel;
    public GameObject buttonWaterLevel;
    public GameObject buttonLandLevel;
    private int lastFire = -1, lastWater = -1, lastLand = -1;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        fireAccess = Mathf.Max(fireAccess, NoxProgress.FireUnlocked ? 1 : 0);
        waterAccess = Mathf.Max(waterAccess, NoxProgress.WaterUnlocked ? 1 : 0);
        landAccess = Mathf.Max(landAccess, NoxProgress.LandUnlocked ? 1 : 0);
        if (fireAccess > 0 && !NoxProgress.FireUnlocked)
            NoxProgress.Unlock("Fire");
        if (waterAccess > 0 && !NoxProgress.WaterUnlocked)
            NoxProgress.Unlock("Water");
        if (landAccess > 0 && !NoxProgress.LandUnlocked)
            NoxProgress.Unlock("Land");
        RefreshFireAccess();
        RefreshWaterAccess();
        RefreshLandAccess();
    }
    public void RefreshFireAccess()
    {
        if (buttonFireLevel != null)
            buttonFireLevel.SetActive(fireAccess > 0);
        lastFire = fireAccess;
    }
    public void RefreshWaterAccess()
    {
        if (buttonWaterLevel != null)
            buttonWaterLevel.SetActive(waterAccess > 0);
        lastWater = waterAccess;
    }
    public void RefreshLandAccess()
    {
        if (buttonLandLevel != null)
            buttonLandLevel.SetActive(landAccess > 0);
        lastLand = landAccess;
    }
    private void Update()
    {
        // Compatibilidade com campos públicos alterados por eventos/scripts legados.
        if (lastFire != fireAccess)
            RefreshFireAccess();
        if (lastWater != waterAccess)
            RefreshWaterAccess();
        if (lastLand != landAccess)
            RefreshLandAccess();
    }
    public void UnlockFire()
    {
        fireAccess = 1;
        NoxProgress.Unlock("Fire");
        RefreshFireAccess();
    }
    public void UnlockWater()
    {
        waterAccess = 1;
        NoxProgress.Unlock("Water");
        RefreshWaterAccess();
    }
    public void UnlockLand()
    {
        landAccess = 1;
        NoxProgress.Unlock("Land");
        RefreshLandAccess();
    }
    public void ButtonLevelWater()
    {
        Debug.Log("A fase de água ainda não faz parte desta versão.");
    }
    public void ButtonLevelLand()
    {
        Debug.Log("A fase de terra ainda não faz parte desta versão.");
    }
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public int acessLevelFire
    {
        get => fireAccess; set => fireAccess = value;
    }
    public int acessLevelWater
    {
        get => waterAccess; set => waterAccess = value;
    }
    public int acessLevelLand
    {
        get => landAccess; set => landAccess = value;
    }

    // Nomes serializados nos UnityEvents de cenas anteriores.
    public void FireLevelUnlocked() => RefreshFireAccess();
    public void WaterLevelUnlocked() => RefreshWaterAccess();
    public void LandLevelUnlocked() => RefreshLandAccess();
}
