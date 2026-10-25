using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class HUDAndSounds : MonoBehaviour
{
    public static HUDAndSounds Singleton;

    public void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    // Variaveis dos menus
    public GameObject pause_panel;
    public GameObject main_menu;
    public GameObject levels_menu;

    // Variáveis dos paineis
    public GameObject options_panel;
    public GameObject controls_panel;
    public GameObject audio_panel;
    public GameObject graphics_panel;
    public GameObject howToPlay_panel;
    public GameObject credits_panel;

    // Variaveis das sombras
    public Light sunBeaming;
    public Slider shadowLevel;
    public Dropdown shadowQuality;

    // Variaveis AudioSource
    public AudioSource clickSound;
    public AudioSource backSound;
    public AudioSource popSound;
    public AudioSource mainMenuMusic;
    public AudioSource in_GameMusic;
    public AudioSource in_GameMusic2;

    // Variaveis de áudio
    public Slider volume_slider;

    #region Metodos Paineis

    public void Options_Button()
    {

        if (options_panel.activeSelf)
        {
            options_panel.SetActive(false);
            Click();
        }

        else
        {
            options_panel.SetActive(true);
            Back();
        }


    }

    public void Controls_Button()
    {
        if (controls_panel.activeSelf)
        {
            controls_panel.SetActive(false);
            Click();
        }

        else
        {
            controls_panel.SetActive(true);
            Back();
        }
    }
    public void Audio_Button()
    {

        if (audio_panel.activeSelf)
        {
            audio_panel.SetActive(false);
            Click();
        }

        else
        {
            audio_panel.SetActive(true);
            Back();
        }
    }

    public void Graphics_Button()
    {

        if (graphics_panel.activeSelf)
        {
            graphics_panel.SetActive(false);
            Click();
        }

        else
        {
            graphics_panel.SetActive(true);
            Back();
        }
    }

    public void HowToPlay_Button()
    {

        if (howToPlay_panel.activeSelf)
        {
            howToPlay_panel.SetActive(false);
            Click();
        }

        else
        {
            howToPlay_panel.SetActive(true);
            Back();
        }
    }


    public void Credits_Button()
    {

        if (credits_panel.activeSelf)
        {
            credits_panel.SetActive(false);
            Click();
        }

        else
        {
            credits_panel.SetActive(true);
            Back();
        }
    }
    #endregion

    public void QuitTheGame_Button()
    {
        Application.Quit();
    }


    public void Play_Button()
    {
        Back();
        main_menu.SetActive(false);
    }

    #region Pause Menu - Metodos
    public void Continue_Button()
    {
        if (pause_panel.activeSelf)
        {
            pause_panel.SetActive(false);
            PlayerScript.PT.healthBar_Canvas.SetActive(true);
            Click();
            Time.timeScale = 1;
        }

        else
        {
            pause_panel.SetActive(true);
            Back();
        }
    }

    public void QuitThePauseMenu_Button()
    {
        Time.timeScale = 0;
        SceneManager.LoadScene(0);
        Click();
        main_menu.SetActive(true);
        levels_menu.SetActive(true);
        pause_panel.SetActive(false);
    }

    public void PauseAndContinue()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            pause_panel.SetActive(false);
            options_panel.SetActive(false);
        }

        else
        {
            Time.timeScale = 0;
            pause_panel.SetActive(true);
        }
    }

    public void Pause_Keycode()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && main_menu.activeSelf == false && levels_menu.activeSelf == false)
        {
            if(PlayerScript.PT.healthBar_Canvas.activeSelf == true)
            {
                PlayerScript.PT.healthBar_Canvas.SetActive(false);
            }
            else
            {
                PlayerScript.PT.healthBar_Canvas.SetActive(true);
            }

            PauseAndContinue();
            PopMenu();
        }
    }
    #endregion

    #region Metódo de Niveis
    public void AirPhase_Button()
    {
        SceneManager.LoadScene(1);
        InGameSound();

        if (levels_menu.activeSelf == true)
        {
            Back();
            levels_menu.SetActive(false);
            Time.timeScale = 1;
            mainMenuMusic.Stop();
        }
    }

    public void FirePhase_Button()
    {
        SceneManager.LoadScene(4);
        InGameSound2();

        if (levels_menu.activeSelf == true)
        {
            Back();
            levels_menu.SetActive(false);
            Time.timeScale = 1;
            mainMenuMusic.Stop();
        }
    }

    public void LevelsMenu_BackButton()
    {

        if (levels_menu.activeSelf == true)
        {
           Click();
           main_menu.SetActive(true);
        }
    }


    #endregion

    #region Shadows Methods
    public void SoftShadows()
    {
        sunBeaming.shadows = LightShadows.Soft;
    }

    public void HardShadows()
    {
        sunBeaming.shadows = LightShadows.Hard;

    }

    public void DisableShadows()
    {
        sunBeaming.shadows = LightShadows.None;
    }

    public void ShadowsQuality()
    {
        if (shadowQuality.value == 0)
        {
            sunBeaming.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Low;
        }

        else if (shadowQuality.value == 1)
        {
            sunBeaming.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
        }

        else if (shadowQuality.value == 2)
        {
            sunBeaming.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
        }
    }
    #endregion

    #region Audio Methods
    public void Click()
    {
        clickSound.Play();
    }

    public void Back()
    {
        backSound.Play();
    }

    public void PopMenu()
    {
        popSound.Play();
    }

    public void MenuMusic()
    {
        mainMenuMusic.Play();
    }

    public void InGameSound()
    {
        in_GameMusic.Play();
    }
    public void InGameSound2()
    {
        in_GameMusic2.Play();
    }   
    public void InGameSound2Stop()
    {
        in_GameMusic2.Stop();
    }

    public void Sound_On()
    {
        if (main_menu.activeSelf == true)
        {
            mainMenuMusic.Play();
        }
        if (main_menu.activeSelf == false && SceneManager.GetActiveScene() != SceneManager.GetSceneByBuildIndex(4))
        {
            in_GameMusic.Play();
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3))
        {
            PlayerScript.PT.soundBossWater.Play();
        }

        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(4))
        {
            in_GameMusic2.Play();
        }

        popSound.volume = 1;
        backSound.volume = 1;
        clickSound.volume = 1;
    }
    public void Sound_Off()
    {
        mainMenuMusic.Stop();
        in_GameMusic.Stop();
        in_GameMusic2.Stop();
        if (main_menu.activeSelf == false && SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3))
        {
            PlayerScript.PT.soundBossWater.Stop();
        }
        popSound.volume = 0;
        backSound.volume = 0;
        clickSound.volume = 0;
    }
    #endregion


    void Start()
    {
        MenuMusic();
    }

    void Update()
    {
        Pause_Keycode();
        ShadowsQuality();

       sunBeaming.shadowStrength = shadowLevel.value;
      
        if (main_menu.activeSelf == true)
        {
            mainMenuMusic.volume = volume_slider.value;
        }

        if(in_GameMusic == true)
        {
            in_GameMusic.volume = volume_slider.value;
        }
        if (in_GameMusic2 == true)
        {
            in_GameMusic2.volume = volume_slider.value;
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3) && PlayerScript.PT.soundBossWater == true)
        {
            PlayerScript.PT.soundBossWater.volume = volume_slider.value;
        }
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(3))
        {
            in_GameMusic.Stop();
        }

        popSound.volume = volume_slider.value;
        backSound.volume = volume_slider.value;
        clickSound.volume = volume_slider.value;

        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
