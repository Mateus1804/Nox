using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class HUDAndSounds : MonoBehaviour
{
    public static HUDAndSounds Singleton;
    private bool isMuted;
    private AudioSource[] controlledSources;

    public void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Singleton = this;
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
        if (NoxSession.State != NoxGameState.Paused) return;
        PauseAndContinue();
        if (PlayerScript.PT != null && PlayerScript.PT.healthBar_Canvas != null)
            PlayerScript.PT.healthBar_Canvas.SetActive(true);
        Click();
    }

    public void QuitThePauseMenu_Button()
    {
        pause_panel.SetActive(false);
        NoxSession.LoadScene(0);
    }

    public void PauseAndContinue()
    {
        if (NoxSession.State == NoxGameState.Playing)
        {
            NoxSession.SetState(NoxGameState.Paused);
            pause_panel.SetActive(true);
        }
        else if (NoxSession.State == NoxGameState.Paused)
        {
            NoxSession.SetState(NoxGameState.Playing);
            pause_panel.SetActive(false);
            options_panel.SetActive(false);
        }
    }

    public void Pause_Keycode()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (NoxSession.State != NoxGameState.Playing && NoxSession.State != NoxGameState.Paused) return;
        PauseAndContinue();
        if (PlayerScript.PT != null && PlayerScript.PT.healthBar_Canvas != null)
            PlayerScript.PT.healthBar_Canvas.SetActive(NoxSession.CanPlay);
        PopMenu();
    }
    #endregion

    #region Metódo de Niveis
    public void AirPhase_Button()
    {
        NoxSession.LoadScene(1);
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
        NoxSession.LoadScene(4);
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
        isMuted = false;
        ApplyVolume();
    }
    public void Sound_Off()
    {
        isMuted = true;
        ApplyVolume();
    }
    #endregion


    void Start()
    {
        controlledSources = new[] { mainMenuMusic, in_GameMusic, in_GameMusic2, popSound, backSound, clickSound };
        NoxSession.SetState(NoxGameState.Menu);
        ApplyVolume();
        MenuMusic();
    }

    void Update()
    {
        Pause_Keycode();
        if (sunBeaming != null && shadowQuality != null) ShadowsQuality();
        if (sunBeaming != null && shadowLevel != null) sunBeaming.shadowStrength = shadowLevel.value;
        ApplyVolume();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (NoxSession.CanPlay && Input.GetKeyDown(KeyCode.L)) NoxSession.LoadNextScene();
#endif
    }

    private void ApplyVolume()
    {
        float volume = isMuted ? 0f : volume_slider != null ? volume_slider.value : 1f;
        if (controlledSources == null) return;
        foreach (AudioSource source in controlledSources)
            if (source != null) source.volume = volume;
        if (PlayerScript.PT != null && PlayerScript.PT.soundBossWater != null)
            PlayerScript.PT.soundBossWater.volume = volume;
    }

    private void OnDestroy()
    {
        if (Singleton == this) Singleton = null;
    }
}
