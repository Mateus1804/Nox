using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NoxProjectAudit
{
    [MenuItem("Nox/Auditar cenas e referências")]
    public static void Run()
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Encerrar o Play antes da auditoria.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
        var findings = new List<string>();
        try
        {
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes.Where(item => item.enabled))
            {
                if (!File.Exists(entry.path))
                {
                    findings.Add(entry.path + ": cena ausente");
                    continue;
                }
                Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                    {
                        string location = entry.path + " / " + PathOf(node);
                        int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject);
                        if (missing > 0)
                            findings.Add(location + ": " + missing + " componente(s) com script ausente");
                        foreach (MonoBehaviour component in node.GetComponents<MonoBehaviour>())
                        {
                            if (component == null)
                                continue;
                            using (var serialized = new SerializedObject(component))
                            {
                                SerializedProperty property = serialized.GetIterator();
                                while (property.NextVisible(true))
                                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                                        property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                                        findings.Add(location + " / " + component.GetType().Name + ": referência perdida em " + property.propertyPath);
                            }
                        }
                        NoxPlayerController player = node.GetComponent<NoxPlayerController>();
                        if (player != null)
                        {
                            if (node.GetComponent<Rigidbody>() == null || node.GetComponent<Collider>() == null)
                                findings.Add(location + ": jogador sem Rigidbody/collider");
                            if (player.crosshair == null || player.slash1 == null || player.slash2 == null || player.slash3 == null)
                                findings.Add(location + ": ataque sem mira ou um dos efeitos");
                            if (player.WhatIsGround.value == 0)
                                findings.Add(location + ": máscara de chão vazia");
                            if (node.GetComponent<NoxCharacterVisual>() == null)
                                findings.Add(location + ": visual original Nox ainda não vinculado");
                        }
                    }
            }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        Directory.CreateDirectory("Logs");
        string report = Path.GetFullPath(Path.Combine("Logs", "Nox-Auditoria.txt"));
        var content = new StringBuilder("Auditoria de referências do Nox\n");
        content.AppendLine("Achados: " + findings.Count);
        foreach (string finding in findings)
            content.AppendLine(finding);
        File.WriteAllText(report, content.ToString());
        Debug.Log("Auditoria concluída: " + findings.Count + " achado(s). Relatório: " + report);
    }
    private static string PathOf(Transform node)
    {
        string path = node.name;
        while (node.parent != null)
        {
            node = node.parent;
            path = node.name + "/" + path;
        }
        return path;
    }
}
