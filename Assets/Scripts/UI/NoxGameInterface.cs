using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "HUDAndSounds")]
public class NoxGameInterface : MonoBehaviour
{
    public static NoxGameInterface Singleton;
    [FormerlySerializedAs("pause_panel")]
    public GameObject pausePanel;
    [FormerlySerializedAs("main_menu")]
    public GameObject mainMenu;
    [FormerlySerializedAs("levels_menu")]
    public GameObject levelMenu;
    [FormerlySerializedAs("options_panel")]
    public GameObject optionsPanel;
    [FormerlySerializedAs("controls_panel")]
    public GameObject controlsPanel;
    [FormerlySerializedAs("audio_panel")]
    public GameObject audioPanel;
    [FormerlySerializedAs("graphics_panel")]
    public GameObject graphicsPanel;
    [FormerlySerializedAs("howToPlay_panel")]
    public GameObject instructionsPanel;
    [FormerlySerializedAs("credits_panel")]
    public GameObject creditsPanel;
    public Light sunBeaming;
    public Slider shadowLevel;
    public Dropdown shadowQuality;
    public AudioSource clickSound;
    public AudioSource backSound;
    public AudioSource popSound;
    public AudioSource mainMenuMusic;
    [FormerlySerializedAs("in_GameMusic")]
    public AudioSource gameMusic;
    [FormerlySerializedAs("in_GameMusic2")]
    public AudioSource fireMusic;
    [FormerlySerializedAs("volume_slider")]
    public Slider volumeSlider;
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
        if (volumeSlider != null)
        {
            float saved = PlayerPrefs.GetFloat(VolumeKey, volumeSlider.value);
            volumeSlider.SetValueWithoutNotify(NoxRules.EffectiveVolume(saved, false));
            volumeSlider.onValueChanged.AddListener(VolumeChanged);
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
    public void ToggleOptions()
    {
        TogglePanel(optionsPanel);
    }
    public void ToggleControls()
    {
        TogglePanel(controlsPanel);
    }
    public void ToggleAudio()
    {
        TogglePanel(audioPanel);
    }
    public void ToggleGraphics()
    {
        TogglePanel(graphicsPanel);
    }
    public void ToggleInstructions()
    {
        TogglePanel(instructionsPanel);
    }
    public void ToggleCredits()
    {
        TogglePanel(creditsPanel);
    }
    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
    public void ShowLevelSelection()
    {
        Back();
        SetPanel(mainMenu, false);
        SetPanel(levelMenu, true);
    }
    public void ResumeSavedLevel()
    {
        if (NoxProgress.CanResume)
            NoxSession.LoadScene(NoxProgress.LastScene);
    }
    public void ResumeGame()
    {
        if (NoxSession.State != NoxGameState.Paused)
            return;
        PauseAndContinue();
        Click();
    }
    public void ReturnToMainMenu()
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
        SetPanel(pausePanel, NoxSession.State == NoxGameState.Paused);
        if (NoxSession.CanPlay)
            CloseOptions();
        if (NoxPlayerController.Active != null)
            SetPanel(NoxPlayerController.Active.healthCanvas, NoxSession.CanPlay);
    }
    public void ReadPauseInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) ||
            (NoxSession.State != NoxGameState.Playing && NoxSession.State != NoxGameState.Paused))
            return;
        PauseAndContinue();
        PopMenu();
    }
    public void StartAirLevel()
    {
        NoxSession.LoadScene(1);
    }
    public void StartFireLevel()
    {
        if (NoxProgress.FireUnlocked || (NoxLevelSelection.Instance != null && NoxLevelSelection.Instance.acessLevelFire > 0))
            NoxSession.LoadScene(4);
    }
    public void ShowMainMenu()
    {
        Click();
        SetPanel(mainMenu, true);
        SetPanel(levelMenu, false);
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
        if (gameMusic != null)
            gameMusic.Stop();
        if (fireMusic != null)
            fireMusic.Stop();
        if (NoxPlayerController.Active != null && NoxPlayerController.Active.bossMusic != null)
            NoxPlayerController.Active.bossMusic.Stop();
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
        PlayMusic(gameMusic);
    }
    public void InGameSound2()
    {
        PlayMusic(fireMusic);
    }
    public void InGameSound2Stop()
    {
        if (fireMusic != null)
            fireMusic.Stop();
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
        PlayerPrefs.SetFloat(VolumeKey, NoxRules.EffectiveVolume(volumeSlider != null ? volumeSlider.value : 1, false));
        ApplyVolume();
    }
    private void ApplyVolume()
    {
        float volume = NoxRules.EffectiveVolume(volumeSlider != null ? volumeSlider.value : 1, isMuted);
        if (volume == appliedVolume)
            return;
        // Preserva o mix de cada AudioSource e alcança também os sons de coleta e chefes.
        AudioListener.volume = volume;
        appliedVolume = volume;
    }
    private void CloseOptions()
    {
        SetPanel(optionsPanel, false);
        SetPanel(controlsPanel, false);
        SetPanel(audioPanel, false);
        SetPanel(graphicsPanel, false);
        SetPanel(instructionsPanel, false);
        SetPanel(creditsPanel, false);
    }
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Singleton != this || !initialized || mode == LoadSceneMode.Additive)
            return;
        CloseOptions();
        SetPanel(pausePanel, false);
        SetPanel(mainMenu, scene.buildIndex == 0);
        SetPanel(levelMenu, false);
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
        ReadPauseInput();
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
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(VolumeChanged);
        PlayerPrefs.Save();
        AudioListener.volume = previousListenerVolume;
        Singleton = null;
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public GameObject pause_panel
    {
        get => pausePanel; set => pausePanel = value;
    }
    public GameObject main_menu
    {
        get => mainMenu; set => mainMenu = value;
    }
    public GameObject levels_menu
    {
        get => levelMenu; set => levelMenu = value;
    }
    public GameObject options_panel
    {
        get => optionsPanel; set => optionsPanel = value;
    }
    public GameObject controls_panel
    {
        get => controlsPanel; set => controlsPanel = value;
    }
    public GameObject audio_panel
    {
        get => audioPanel; set => audioPanel = value;
    }
    public GameObject graphics_panel
    {
        get => graphicsPanel; set => graphicsPanel = value;
    }
    public GameObject howToPlay_panel
    {
        get => instructionsPanel; set => instructionsPanel = value;
    }
    public GameObject credits_panel
    {
        get => creditsPanel; set => creditsPanel = value;
    }
    public Slider volume_slider
    {
        get => volumeSlider; set => volumeSlider = value;
    }
    public AudioSource in_GameMusic
    {
        get => gameMusic; set => gameMusic = value;
    }
    public AudioSource in_GameMusic2
    {
        get => fireMusic; set => fireMusic = value;
    }

    // Nomes serializados nos UnityEvents de cenas anteriores.
    public void Options_Button() => ToggleOptions();
    public void Controls_Button() => ToggleControls();
    public void Audio_Button() => ToggleAudio();
    public void Graphics_Button() => ToggleGraphics();
    public void HowToPlay_Button() => ToggleInstructions();
    public void Credits_Button() => ToggleCredits();
    public void QuitTheGame_Button() => QuitGame();
    public void Play_Button() => ShowLevelSelection();
    public void ContinueSavedGame_Button() => ResumeSavedLevel();
    public void Continue_Button() => ResumeGame();
    public void QuitThePauseMenu_Button() => ReturnToMainMenu();
    public void Pause_Keycode() => ReadPauseInput();
    public void AirPhase_Button() => StartAirLevel();
    public void FirePhase_Button() => StartFireLevel();
    public void LevelsMenu_BackButton() => ShowMainMenu();
}
