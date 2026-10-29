using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(true, sourceNamespace: null, sourceAssembly: "Assembly-CSharp", sourceClassName: "PlaceHolderPanel")]
public class NoxLevelResultScreen : MonoBehaviour
{
    [FormerlySerializedAs("HUD")]
    public NoxGameInterface gameInterface;
    private bool resultShown;
    public static NoxLevelResultScreen PHP;

    [FormerlySerializedAs("placeHolderPanel")]
    public GameObject resultPanel;
    public GameObject buttonNextLevel;
    public GameObject buttonRestart;
    [FormerlySerializedAs("placeHolderText")]
    public Text resultText;

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
        if (resultShown || (gameInterface != null && gameInterface.mainMenu != null && gameInterface.mainMenu.activeSelf))
            return;
        bool won = text == "Stage Completed";
        resultShown = true;
        NoxSession.SetState(won ? NoxGameState.Won : NoxGameState.Lost);
        if (resultText != null)
            resultText.text = won ?
            (SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1 ? "Jornada concluída" : "Fase concluída") : "Fim de jogo";
        if (won)
            ShowResult();
        else
            StartCoroutine(ShowDefeat());
        if (NoxPlayerController.Active != null && NoxPlayerController.Active.healthCanvas != null)
            NoxPlayerController.Active.healthCanvas.SetActive(false);
        bool hasNext = SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings;
        if (buttonNextLevel != null)
            buttonNextLevel.SetActive(won && hasNext);
        if (buttonRestart != null)
            buttonRestart.SetActive(!won);
    }

    public void ReturnToMainMenu()
    {
        ResetPanel();
        NoxSession.LoadScene(0);
    }

    public void RestartLevel()
    {
        ResetPanel();
        NoxSession.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ContinueJourney()
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
        if (resultPanel != null)
            resultPanel.SetActive(true);
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
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
    private void OnDestroy()
    {
        if (PHP == this)
            PHP = null;
    }
    // API de compatibilidade para os dados e callbacks recuperados.
    public NoxGameInterface HUD
    {
        get => gameInterface; set => gameInterface = value;
    }
    public GameObject placeHolderPanel
    {
        get => resultPanel; set => resultPanel = value;
    }
    public Text placeHolderText
    {
        get => resultText; set => resultText = value;
    }

    // Nomes serializados nos UnityEvents de cenas anteriores.
    public void BackPlaceHolder_Button() => ReturnToMainMenu();
    public void ButtonRestart() => RestartLevel();
    public void ButtonNextLevel() => ContinueJourney();
}
