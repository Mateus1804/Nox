using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Correção explícita do trecho medido da primeira fase, sem inventar ligações entre puzzles.
public static class NoxTraversalRepair
{
    public static void Apply(Scene scene)
    {
        if (scene.name != "AirPhase1.4")
            return;
        Transform[] nodes = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
        Renderer left = Surface(nodes, "Platform19"), right = Surface(nodes, "Platform20");
        if (left == null || right == null)
        {
            Debug.LogWarning("Travessia v7: plataformas 19/20 não encontradas; nenhuma geometria foi inventada.");
            return;
        }
        Bounds a = left.bounds, b = right.bounds;
        float gap = b.min.x - a.max.x;
        Directory.CreateDirectory("Logs");
        double range = NoxJumpEnvelope.Range(6, 3.5, Math.Abs(Physics.gravity.y), b.max.y - a.max.y);
        File.WriteAllText("Logs/Nox-Travessia-v7.txt", "AirPhase1.4: Platform19 -> Platform20\n" +
            "Vão entre bordas: " + gap.ToString("F2") + "\nAlcance balístico estimado: " + range.ToString("F2") +
            "\nA estimativa não inclui aceleração, colisões ou tamanho do jogador. O percurso completo exige Play.\n");
        // Só modifica a posição acadêmica conhecida; personalizações não são sobrescritas silenciosamente.
        if (Mathf.Abs(a.max.x - 205.91f) > 0.15f || Mathf.Abs(b.min.x - 216f) > 0.15f ||
            Mathf.Abs(a.max.y - 49f) > 0.15f || Mathf.Abs(b.max.y - 48f) > 0.15f)
        {
            Debug.LogWarning("Travessia v7: trecho personalizado; conferir Logs/Nox-Travessia-v7.txt.");
            return;
        }
        Transform previous = nodes.FirstOrDefault(t => t.name == "NoxLanding-v7");
        if (previous != null)
            return;
        GameObject landing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        landing.name = "NoxLanding-v7";
        SceneManager.MoveGameObjectToScene(landing, scene);
        landing.layer = left.gameObject.layer;
        landing.transform.position = new Vector3((a.max.x + b.min.x) / 2, 48f, 0);
        landing.transform.localScale = new Vector3(3.5f, 1, 7);
        // A superfície faz parte do chão real; a decoração posterior continua sem collider.
        Debug.Log("Travessia v7: apoio entre as plataformas 19 e 20 criado. Vãos restantes: " + ((gap - 3.5f) / 2).ToString("F2"));
    }
    private static Renderer Surface(Transform[] nodes, string name)
    {
        Transform node = nodes.FirstOrDefault(t => t.name == name);
        return node != null ? node.GetComponent<Renderer>() : null;
    }
}
