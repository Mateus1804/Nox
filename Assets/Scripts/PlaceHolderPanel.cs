using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class PlaceHolderPanel : MonoBehaviour

{
    public HUDAndSounds HUD;
    private bool resultShown;
    public static PlaceHolderPanel PHP;


    public GameObject placeHolderPanel;
    public GameObject buttonNextLevel;
    public GameObject buttonRestart;
    public Text placeHolderText;

    #region Awake()
    void Awake()
    {
        if (PHP == null)
        {
            PHP = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region GameWinLose
    public void PanelWinOrLose(string text)
    {
        if (resultShown || (HUD != null && HUD.main_menu.activeSelf)) return;
        bool won = text == "Stage Completed";
        resultShown = true;
        NoxSession.SetState(won ? NoxGameState.Won : NoxGameState.Lost);
        if (placeHolderText != null) placeHolderText.text = won ? "Fase concluída" : "Fim de jogo";
        if (placeHolderPanel != null) placeHolderPanel.SetActive(true);
        if (PlayerScript.PT != null && PlayerScript.PT.healthBar_Canvas != null)
            PlayerScript.PT.healthBar_Canvas.SetActive(false);
        bool hasNext = SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
        if (buttonNextLevel != null) buttonNextLevel.SetActive(won && hasNext);
        if (buttonRestart != null) buttonRestart.SetActive(!won);
    }
    #endregion

    #region ButtonMenu
    public void BackPlaceHolder_Button()
    {
        ResetPanel();
        NoxSession.LoadScene(0);
    }
    #endregion
    public void ButtonRestart()
    {
        ResetPanel();
        NoxSession.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ButtonNextLevel()
    {
        ResetPanel();
        NoxSession.LoadNextScene();
    }


    private void OnEnable() { SceneManager.sceneLoaded += SceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= SceneLoaded; }
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetPanel();
        if (HUD != null)
        {
            HUD.main_menu.SetActive(scene.buildIndex == 0);
            HUD.levels_menu.SetActive(scene.buildIndex == 0);
            HUD.pause_panel.SetActive(false);
            if (HUD.in_GameMusic != null) HUD.in_GameMusic.Stop();
            if (HUD.in_GameMusic2 != null) HUD.in_GameMusic2.Stop();
            if (scene.buildIndex == 0) HUD.MenuMusic();
            else
            {
                if (HUD.mainMenuMusic != null) HUD.mainMenuMusic.Stop();
                if (scene.buildIndex == 4) HUD.InGameSound2(); else if (scene.buildIndex != 3) HUD.InGameSound();
            }
        }
        NoxSession.SetState(scene.buildIndex == 0 ? NoxGameState.Menu : NoxGameState.Playing);
    }
    private void ResetPanel()
    {
        resultShown = false;
        if (placeHolderPanel != null) placeHolderPanel.SetActive(false);
    }
    private void OnDestroy() { if (PHP == this) PHP = null; }
}
