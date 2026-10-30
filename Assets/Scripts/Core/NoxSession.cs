using UnityEngine;
using UnityEngine.SceneManagement;

public enum NoxGameState
{
    Menu, Playing, Paused, Won, Lost
}

public static class NoxSession
{
    public static NoxGameState State { get; private set; } = NoxGameState.Menu;
    private static readonly NoxSceneLoadGate loading = new NoxSceneLoadGate();
    public static bool CanPlay => State == NoxGameState.Playing && !loading.Busy;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        loading.Complete();
        SceneManager.sceneLoaded -= SceneLoaded;
        SceneManager.sceneLoaded += SceneLoaded;
        State = NoxGameState.Menu;
        Time.timeScale = 1f;
    }

    public static void SetState(NoxGameState state)
    {
        State = state;
        Time.timeScale = state == NoxGameState.Paused ||
            state == NoxGameState.Won || state == NoxGameState.Lost ? 0f : 1f;
    }

    public static void LoadScene(int index)
    {
        if (loading.Busy)
            return;
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("A cena solicitada não está nas configurações de build.");
            return;
        }
        if (!loading.TryBegin(index, SceneManager.sceneCountInBuildSettings))
            return;
        NoxGameState previous = State;
        SetState(index == 0 ? NoxGameState.Menu : NoxGameState.Playing);
        try
        {
            SceneManager.LoadScene(index);
        }
        catch { loading.Complete(); SetState(previous); throw; }
    }

    private static void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Application.isPlaying || mode == LoadSceneMode.Additive || scene.buildIndex < 0)
            return;
        loading.Complete();
        SetState(scene.buildIndex == 0 ? NoxGameState.Menu : NoxGameState.Playing);
        NoxProgress.RecordScene(scene.buildIndex);
    }

    public static void LoadNextScene()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int count = SceneManager.sceneCountInBuildSettings;
        if (count <= 0 || current < 0 || current >= count)
            return;
        if (current == count - 1 && NoxLevelResultScreen.PHP != null)
            NoxLevelResultScreen.PHP.PanelWinOrLose("Stage Completed");
        else
            LoadScene(NoxRules.NextScene(current, count));
    }
}
