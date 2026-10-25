using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevels : MonoBehaviour
{
    /*
    Esse é o script que desbloqueia a próxima fase no menu, após o Player concluir a fase anterior. Para ele funciona você precisa:
    • Associar esse script ao GameADM
    • Associar


    */


    public static LoadLevels Instance;

    #region Awake()
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public int acessLevelFire = 0;
    public int acessLevelWater = 0;
    public int acessLevelLand = 0;

    //Variável que armazena o botão da fase de ar
    public GameObject buttonAirLevel;

    //Variável que armazena o botão da fase de fogo
    public GameObject buttonFireLevel;

    //Variável que armazena o botão da fase de água
    public GameObject buttonWaterLevel;

    //Variável que armazena o botão da fase de terra
    public GameObject buttonLandLevel;


    public void FireLevelUnlocked()
    {
        if (acessLevelFire > 0)
        {
            if (buttonFireLevel != null) buttonFireLevel.SetActive(true);
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public void WaterLevelUnlocked()
    {
        if (acessLevelWater > 0)
        {
            if (buttonWaterLevel != null) buttonWaterLevel.SetActive(true);
            DontDestroyOnLoad(this.gameObject);
        }

    }
    public void LandLevelUnlocked()
    {
        if (acessLevelLand > 0)
        {
            if (buttonLandLevel != null) buttonLandLevel.SetActive(true);
            DontDestroyOnLoad(this.gameObject);
        }

    }

    void Start()
    {

    }

    // Declaramos os metódos abaixo, para que a condição de acesso as fases seja checada a todo momento.
    void Update()
    {
        FireLevelUnlocked();
        WaterLevelUnlocked();
        LandLevelUnlocked();
    }


    public void UnlockFire() { acessLevelFire = 1; FireLevelUnlocked(); }
    public void UnlockWater() { acessLevelWater = 1; WaterLevelUnlocked(); }
    public void UnlockLand() { acessLevelLand = 1; LandLevelUnlocked(); }

    // Estas fases não existem na lista original de build; nunca carregar um índice inventado.
    public void ButtonLevelWater() { Debug.Log("A fase de água ainda não faz parte desta versão."); }
    public void ButtonLevelLand() { Debug.Log("A fase de terra ainda não faz parte desta versão."); }

    private void OnDestroy() { if (Instance == this) Instance = null; }
}
