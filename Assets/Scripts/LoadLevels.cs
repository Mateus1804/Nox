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
        if (acessLevelFire == 1)
        {
            buttonFireLevel.SetActive(true);
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public void WaterLevelUnlocked()
    {
        if (acessLevelWater == 1)
        {
            buttonWaterLevel.SetActive(true);
            DontDestroyOnLoad(this.gameObject);
        }
        
    }
    public void LandLevelUnlocked()
    {
        if (acessLevelLand == 1)
        {
            buttonLandLevel.SetActive(true);
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

}
