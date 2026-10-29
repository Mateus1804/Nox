using System.Collections;
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

    public void PanelWinOrLose(string text)
    {
        if (resultShown || (HUD != null && HUD.main_menu != null && HUD.main_menu.activeSelf))
            return;
        bool won = text == "Stage Completed";
        resultShown = true;
        NoxSession.SetState(won ? NoxGameState.Won : NoxGameState.Lost);
        if (placeHolderText != null)
            placeHolderText.text = won ?
            (SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1 ? "Jornada concluída" : "Fase concluída") : "Fim de jogo";
        if (won)
            ShowResult();
        else
            StartCoroutine(ShowDefeat());
        if (PlayerScript.PT != null && PlayerScript.PT.healthBar_Canvas != null)
            PlayerScript.PT.healthBar_Canvas.SetActive(false);
        bool hasNext = SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
        if (buttonNextLevel != null)
            buttonNextLevel.SetActive(won && hasNext);
        if (buttonRestart != null)
            buttonRestart.SetActive(!won);
    }

    public void BackPlaceHolder_Button()
    {
        ResetPanel();
        NoxSession.LoadScene(0);
    }

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

    private IEnumerator ShowDefeat()
    {
        // A morte visual usa tempo não escalado; o mundo já fica bloqueado.
        yield return new WaitForSecondsRealtime(0.65f);
        ShowResult();
    }
    private void ShowResult()
    {
        if (placeHolderPanel != null)
            placeHolderPanel.SetActive(true);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }
    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Additive)
            ResetPanel();
    }
    private void ResetPanel()
    {
        StopAllCoroutines();
        resultShown = false;
        if (placeHolderPanel != null)
            placeHolderPanel.SetActive(false);
    }
    private void OnDestroy()
    {
        if (PHP == this)
            PHP = null;
    }
}
