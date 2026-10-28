using UnityEngine;

// Identifica o suporte móvel. O jogador permanece na própria hierarquia:
// herdar a escala não uniforme da plataforma deforma o modelo ao girar.
public class plataforma : MonoBehaviour
{
    public Transform platforms; // Referência serializada preservada.
}
