using UnityEngine;

// Carrega o GUID do marcador original; a preparação o remove das cenas migradas.
// O suporte móvel atual não altera a hierarquia nem a escala do jogador.
public class plataforma : MonoBehaviour
{
    public Transform platforms; // Referência serializada preservada.
}
