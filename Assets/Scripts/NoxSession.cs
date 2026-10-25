using UnityEngine;
using UnityEngine.SceneManagement;

public enum NoxGameState { Menu, Playing, Paused, Won, Lost }

public static class NoxSession
{
    public static NoxGameState State { get; private set; } = NoxGameState.Menu;
    public static bool CanPlay => State == NoxGameState.Playing;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
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
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("A cena solicitada não está nas configurações de build.");
            return;
        }
        SetState(index == 0 ? NoxGameState.Menu : NoxGameState.Playing);
        SceneManager.LoadScene(index);
    }

    public static void LoadNextScene()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int count = SceneManager.sceneCountInBuildSettings;
        if (count > 0 && current >= 0 && current < count)
            LoadScene(NoxRules.NextScene(current, count));
    }
}
