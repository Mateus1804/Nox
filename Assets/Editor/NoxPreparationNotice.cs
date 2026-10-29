#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Atualizar scripts não regrava cenas por conta própria. O aviso torna essa etapa visível.
public static class NoxPreparationNotice
{
    [InitializeOnLoadMethod]
    private static void Initialize() { EditorApplication.delayCall += Notify; }
    private static void Notify()
    {
        if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode ||
            SessionState.GetBool("Nox.PreparationNotice.v7", false) ||
            File.Exists("ProjectSettings/NoxScenesPrepared-v7.txt") ||
            !EditorBuildSettings.scenes.Any(s => s.enabled && File.Exists(s.path))) return;
        SessionState.SetBool("Nox.PreparationNotice.v7", true);
        Debug.LogWarning("Revisão v7 instalada nos scripts, mas ainda não aplicada às cenas. Usar Nox > Preparar cenas recuperadas.");
        if (EditorUtility.DisplayDialog("Preparação do Nox pendente",
            "Os scripts da revisão 7 foram instalados. O personagem e o cenário só são atualizados após preparar as cenas. As cenas anteriores serão copiadas para Backups/Preparacao-v7.",
            "Preparar cenas", "Agora não")) NoxBuild.PrepareScenes();
    }
}
#endif
