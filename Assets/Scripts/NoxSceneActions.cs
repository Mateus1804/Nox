using UnityEngine;
using UnityEngine.SceneManagement;

public class NoxSceneActions : MonoBehaviour
{
    public void Restart()
    {
        NoxSession.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnToMenu()
    {
        NoxSession.LoadScene(0);
    }
}
