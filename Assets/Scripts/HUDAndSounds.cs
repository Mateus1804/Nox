using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDAndSounds : MonoBehaviour
{
    public static HUDAndSounds Singleton;
    public GameObject pause_panel, main_menu, levels_menu;
    public GameObject options_panel, controls_panel, audio_panel, graphics_panel, howToPlay_panel, credits_panel;
    public Light sunBeaming;
    public Slider shadowLevel;
    public Dropdown shadowQuality;
    public AudioSource clickSound, backSound, popSound, mainMenuMusic, in_GameMusic, in_GameMusic2;
    public Slider volume_slider;
    private const string VolumeKey = "Nox.v1.Audio.Volume";
    private const string MutedKey = "Nox.v1.Audio.Muted";
    private bool isMuted;
    private bool initialized;
    private float previousListenerVolume;
    private float appliedVolume = -1;
    private int appliedQuality = -1;
    private float appliedStrength = -1;

    public void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        previousListenerVolume = AudioListener.volume;
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        if (Singleton == this)
            SceneManager.sceneLoaded += SceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }
    private void Start()
    {
        if (Singleton != this)
            return;
        isMuted = PlayerPrefs.GetInt(MutedKey, 0) != 0;
        if (volume_slider != null)
        {
            float saved = PlayerPrefs.GetFloat(VolumeKey, volume_slider.value);
            volume_slider.SetValueWithoutNotify(NoxRules.EffectiveVolume(saved, false));
            volume_slider.onValueChanged.AddListener(VolumeChanged);
        }
        if (shadowQuality != null)
            shadowQuality.SetValueWithoutNotify(Mathf.Clamp(
            PlayerPrefs.GetInt("Nox.v1.Graphics.ShadowQuality", shadowQuality.value), 0, 2));
        if (shadowLevel != null)
            shadowLevel.SetValueWithoutNotify(Mathf.Clamp01(
            PlayerPrefs.GetFloat("Nox.v1.Graphics.ShadowStrength", shadowLevel.value)));
        initialized = true;
        ApplyVolume();
        SceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    private static void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
    private void TogglePanel(GameObject panel)
    {
        if (panel == null)
            return;
        bool closing = panel.activeSelf;
        panel.SetActive(!closing);
        if (closing)
            Click();
        else
            Back();
    }
    public void Options_Button()
    {
        TogglePanel(options_panel);
    }
    public void Controls_Button()
    {
        TogglePanel(controls_panel);
    }
    public void Audio_Button()
    {
        TogglePanel(audio_panel);
    }
    public void Graphics_Button()
    {
        TogglePanel(graphics_panel);
    }
    public void HowToPlay_Button()
    {
        TogglePanel(howToPlay_panel);
    }
    public void Credits_Button()
    {
        TogglePanel(credits_panel);
    }
    public void QuitTheGame_Button()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
    public void Play_Button()
    {
        Back();
        SetPanel(main_menu, false);
        SetPanel(levels_menu, true);
    }
    public void ContinueSavedGame_Button()
    {
        if (NoxProgress.CanResume)
            NoxSession.LoadScene(NoxProgress.LastScene);
    }
    public void Continue_Button()
    {
        if (NoxSession.State != NoxGameState.Paused)
            return;
        PauseAndContinue();
        Click();
    }
    public void QuitThePauseMenu_Button()
    {
        NoxSession.LoadScene(0);
    }
    public void PauseAndContinue()
    {
        if (NoxSession.CanPlay)
            NoxSession.SetState(NoxGameState.Paused);
        else if (NoxSession.State == NoxGameState.Paused)
            NoxSession.SetState(NoxGameState.Playing);
        else
            return;
        SetPanel(pause_panel, NoxSession.State == NoxGameState.Paused);
        if (NoxSession.CanPlay)
            CloseOptions();
        if (PlayerScript.PT != null)
            SetPanel(PlayerScript.PT.healthBar_Canvas, NoxSession.CanPlay);
    }
    public void Pause_Keycode()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) ||
            (NoxSession.State != NoxGameState.Playing && NoxSession.State != NoxGameState.Paused))
            return;
        PauseAndContinue();
        PopMenu();
    }
    public void AirPhase_Button()
    {
        NoxSession.LoadScene(1);
    }
    public void FirePhase_Button()
    {
        if (NoxProgress.FireUnlocked || (LoadLevels.Instance != null && LoadLevels.Instance.acessLevelFire > 0))
            NoxSession.LoadScene(4);
    }
    public void LevelsMenu_BackButton()
    {
        Click();
        SetPanel(main_menu, true);
        SetPanel(levels_menu, false);
    }
    public void SoftShadows()
    {
        if (sunBeaming != null)
            sunBeaming.shadows = LightShadows.Soft;
    }
    public void HardShadows()
    {
        if (sunBeaming != null)
            sunBeaming.shadows = LightShadows.Hard;
    }
    public void DisableShadows()
    {
        if (sunBeaming != null)
            sunBeaming.shadows = LightShadows.None;
    }
    public void ShadowsQuality()
    {
        if (sunBeaming == null || shadowQuality == null)
            return;
        int value = shadowQuality.value;
        sunBeaming.shadowResolution = value == 0 ? LightShadowResolution.Low :
            value == 1 ? LightShadowResolution.Medium : LightShadowResolution.VeryHigh;
        appliedQuality = value;
        PlayerPrefs.SetInt("Nox.v1.Graphics.ShadowQuality", value);
    }
    private static void Play(AudioSource source)
    {
        if (source != null && source.isActiveAndEnabled && !source.isPlaying)
            source.Play();
    }
    public void Click()
    {
        Play(clickSound);
    }
    public void Back()
    {
        Play(backSound);
    }
    public void PopMenu()
    {
        Play(popSound);
    }
    public void StopMusic()
    {
        if (mainMenuMusic != null)
            mainMenuMusic.Stop();
        if (in_GameMusic != null)
            in_GameMusic.Stop();
        if (in_GameMusic2 != null)
            in_GameMusic2.Stop();
        if (PlayerScript.PT != null && PlayerScript.PT.soundBossWater != null)
            PlayerScript.PT.soundBossWater.Stop();
    }
    private void PlayMusic(AudioSource source)
    {
        if (source != null && source.isPlaying)
            return;
        StopMusic();
        if (source != null)
            source.loop = true;
        Play(source);
    }
    public void MenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }
    public void InGameSound()
    {
        PlayMusic(in_GameMusic);
    }
    public void InGameSound2()
    {
        PlayMusic(in_GameMusic2);
    }
    public void InGameSound2Stop()
    {
        if (in_GameMusic2 != null)
            in_GameMusic2.Stop();
    }
    public void Sound_On()
    {
        isMuted = false;
        SaveAudio();
    }
    public void Sound_Off()
    {
        isMuted = true;
        SaveAudio();
    }
    private void VolumeChanged(float value)
    {
        SaveAudio();
    }
    private void SaveAudio()
    {
        PlayerPrefs.SetInt(MutedKey, isMuted ? 1 : 0);
        PlayerPrefs.SetFloat(VolumeKey, NoxRules.EffectiveVolume(volume_slider != null ? volume_slider.value : 1, false));
        ApplyVolume();
    }
    private void ApplyVolume()
    {
        float volume = NoxRules.EffectiveVolume(volume_slider != null ? volume_slider.value : 1, isMuted);
        if (volume == appliedVolume)
            return;
        // Preserva o mix de cada AudioSource e alcança também os sons de coleta e chefes.
        AudioListener.volume = volume;
        appliedVolume = volume;
    }
    private void CloseOptions()
    {
        SetPanel(options_panel, false);
        SetPanel(controls_panel, false);
        SetPanel(audio_panel, false);
        SetPanel(graphics_panel, false);
        SetPanel(howToPlay_panel, false);
        SetPanel(credits_panel, false);
    }
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Singleton != this || !initialized || mode == LoadSceneMode.Additive)
            return;
        CloseOptions();
        SetPanel(pause_panel, false);
        SetPanel(main_menu, scene.buildIndex == 0);
        SetPanel(levels_menu, false);
        sunBeaming = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Light light in root.GetComponentsInChildren<Light>(true))
                if (light.type == LightType.Directional && light.isActiveAndEnabled)
                {
                    sunBeaming = light;
                    break;
                }
        appliedQuality = -1;
        appliedStrength = -1;
        if (scene.buildIndex == 0)
            MenuMusic();
        else if (scene.buildIndex == 4)
            InGameSound2();
        else if (scene.buildIndex == 3)
            StopMusic();
        else
            InGameSound();
    }
    private void Update()
    {
        if (Singleton != this)
            return;
        Pause_Keycode();
        ApplyVolume();
        if (shadowQuality != null && shadowQuality.value != appliedQuality)
            ShadowsQuality();
        if (sunBeaming != null && shadowLevel != null && shadowLevel.value != appliedStrength)
        {
            sunBeaming.shadowStrength = shadowLevel.value;
            appliedStrength = shadowLevel.value;
            PlayerPrefs.SetFloat("Nox.v1.Graphics.ShadowStrength", shadowLevel.value);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (NoxSession.CanPlay && Input.GetKeyDown(KeyCode.L)) NoxSession.LoadNextScene();
#endif
    }
    private void OnApplicationFocus(bool focused)
    {
        if (!focused && NoxSession.CanPlay)
            PauseAndContinue();
    }
    private void OnApplicationPause(bool paused)
    {
        if (!paused)
            return;
        PlayerPrefs.Save();
        if (NoxSession.CanPlay)
            PauseAndContinue();
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
    private void OnDestroy()
    {
        if (Singleton != this)
            return;
        if (volume_slider != null)
            volume_slider.onValueChanged.RemoveListener(VolumeChanged);
        PlayerPrefs.Save();
        AudioListener.volume = previousListenerVolume;
        Singleton = null;
    }
}
