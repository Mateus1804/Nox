using UnityEngine;
using UnityEngine.SceneManagement;

public static class NoxBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // O menu contém o GameADM e seus painéis persistentes.
        // Abrir uma fase isolada passa pelo mesmo ponto de inicialização.
        if (HUDAndSounds.Singleton == null && SceneManager.GetActiveScene().buildIndex != 0)
            NoxSession.LoadScene(0);
    }
}
