using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Referências de UI explícitas; jogador é associado uma vez na cena de gameplay.
public sealed class NoxPlayHud : MonoBehaviour
{
    public Text status, objective;
    private Canvas canvas;
    private PlayerScript player;
    private PlayerTwo alternate;
    private MovableObjectsScript boxes;
    private float nextRefresh;
    private int lastHp = -1, lastHint = -1;
    private void Start()
    {
        canvas = GetComponent<Canvas>();
        player = FindObjectOfType<PlayerScript>(); alternate = FindObjectOfType<PlayerTwo>();
        if (player != null) boxes = player.GetComponent<MovableObjectsScript>();
        if (objective != null)
        {
            string name = SceneManager.GetActiveScene().name;
            objective.text = name.StartsWith("Fire") ? "Mundo do fogo • Encontre a saída" :
                name.StartsWith("AirPhase3") ? "Santuário do ar • Derrote o guardião" : "Mundo do ar • Explore e encontre a passagem";
        }
    }
    private void Update()
    {
        if (Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.1f;
        bool show = NoxSession.CanPlay;
        if (canvas != null) canvas.enabled = show;
        if (status != null)
        {
            status.enabled = show;
            float hp = player != null ? player.hp : alternate != null ? alternate.hp : 0;
            int health = Mathf.CeilToInt(hp);
            int hintType = boxes != null && boxes.Holding ? 2 : boxes != null && boxes.HasCandidate ? 1 : 0;
            if (health != lastHp || hintType != lastHint)
            {
                string hint = hintType == 2 ? "E • Soltar caixa" : hintType == 1 ? "E • Segurar caixa" :
                    "Espaço • Saltar    Clique • Atacar    Esc • Pausar";
                status.text = "Vida " + health + "   |   " + hint;
                lastHp = health; lastHint = hintType;
            }
        }
        if (objective != null) objective.enabled = show;
    }
}
