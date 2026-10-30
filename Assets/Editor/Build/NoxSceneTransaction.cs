using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

// Backup externo a Assets: protege cenas antes de qualquer regravação.
public sealed class NoxSceneTransaction : IDisposable
{
    private readonly SceneSetup[] previous;
    private readonly string[] paths;
    private bool committed;
    public string BackupDirectory
    {
        get;
    }

    public NoxSceneTransaction(string purpose)
    {
        previous = EditorSceneManager.GetSceneManagerSetup();
        paths = EditorBuildSettings.scenes.Where(item => item.enabled).Select(item => item.path).Distinct().ToArray();
        if (paths.Length == 0)
            throw new InvalidOperationException("Não há cenas habilitadas na build.");
        foreach (string path in paths)
            if (!path.StartsWith("Assets/", StringComparison.Ordinal) || path.Contains("..") || !File.Exists(path))
                throw new InvalidOperationException("Caminho de cena inválido ou ausente: " + path);
        BackupDirectory = Path.GetFullPath(Path.Combine("Backups", purpose,
            DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")));
        foreach (string path in paths)
        {
            string target = Path.Combine(BackupDirectory, path);
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            File.Copy(path, target, false);
        }
        File.WriteAllLines(Path.Combine(BackupDirectory, "cenas.txt"), paths);
        UnityEngine.Debug.Log("Cópia das cenas anteriores: " + BackupDirectory);
    }
    public void Commit()
    {
        committed = true;
    }
    public void Dispose()
    {
        if (!committed)
        {
            // Descarta a cena em memória antes de restaurar os bytes anteriores.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            foreach (string path in paths)
                File.Copy(Path.Combine(BackupDirectory, path), path, true);
            AssetDatabase.Refresh();
            UnityEngine.Debug.LogWarning("Operação interrompida; as cenas foram restauradas a partir de " + BackupDirectory);
        }
        if (!UnityEngine.Application.isBatchMode)
            EditorSceneManager.RestoreSceneManagerSetup(previous);
    }
}
