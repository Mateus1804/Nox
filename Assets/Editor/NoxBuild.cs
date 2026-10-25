#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class NoxBuild
{
    [MenuItem("Nox/Preparar cenas recuperadas")]
    public static void PrepareScenes()
    {
        string marker = Path.Combine("ProjectSettings", "NoxScenesPrepared.txt");
        if (File.Exists(marker)) return;
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes.Where(item => item.enabled))
            {
                if (!File.Exists(scene.path)) throw new FileNotFoundException("Cena original ausente", scene.path);
                Scene opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                foreach (GameObject root in opened.GetRootGameObjects())
                {
                    foreach (Button button in root.GetComponentsInChildren<Button>(true)) RepairButton(button);
                    foreach (EventTrigger trigger in root.GetComponentsInChildren<EventTrigger>(true))
                        foreach (EventTrigger.Entry entry in trigger.triggers) RemoveMissingCallbacks(entry.callback, FindButtonIncludingInactive(trigger.transform));
                    foreach (TutorialMethod tutorial in root.GetComponentsInChildren<TutorialMethod>(true))
                    {
                        Match number = Regex.Match(tutorial.name, @"Collider(\d+)$");
                        if (!number.Success) throw new InvalidOperationException("Índice de tutorial não identificado: " + tutorial.name);
                        tutorial.i = int.Parse(number.Groups[1].Value) - 1;
                        if (tutorial.list == null || !NoxRules.IsValidIndex(tutorial.i, tutorial.list.tutorials.Count))
                            throw new InvalidOperationException("Tutorial sem painel correspondente: " + tutorial.name);
                        EditorUtility.SetDirty(tutorial);
                    }
                }
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
            File.WriteAllText(marker, "Preparação de cenas aplicada pelo NoxBuild.\n");
        }
        finally
        {
            if (!Application.isBatchMode) EditorSceneManager.RestoreSceneManagerSetup(previous);
        }
    }

    private static Button FindButtonIncludingInactive(Transform node)
    {
        // A sobrecarga GetComponentInParent<T>(bool) não existe no editor original.
        while (node != null)
        {
            Button button = node.GetComponent<Button>();
            if (button != null) return button;
            node = node.parent;
        }
        return null;
    }

    private static void RepairButton(Button button)
    {
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            string method = button.onClick.GetPersistentMethodName(i);
            if (button.onClick.GetPersistentTarget(i) == null && (method == "buttonRestart" || method == "BackPlaceHolder_Button"))
            {
                UnityEventTools.RemovePersistentListener(button.onClick, i);
                NoxSceneActions actions = button.GetComponent<NoxSceneActions>();
                if (actions == null) actions = button.gameObject.AddComponent<NoxSceneActions>();
                if (method == "buttonRestart") UnityEventTools.AddPersistentListener(button.onClick, actions.Restart);
                else UnityEventTools.AddPersistentListener(button.onClick, actions.ReturnToMenu);
            }
        }
        RemoveMissingCallbacks(button.onClick, button);
        EditorUtility.SetDirty(button);
    }

    private static void RemoveMissingCallbacks(UnityEventBase callback, Button button)
    {
        for (int i = callback.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEngine.Object target = callback.GetPersistentTarget(i);
            string method = callback.GetPersistentMethodName(i);
            // Água, terra e dificuldade não estão implementadas nesta versão.
            bool unavailable = method == "ButtonLevelWater" || method == "ButtonLevelLand" || method == "EasyMediumHard_Button";
            bool missing = target == null || target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).All(item => item.Name != method);
            if (!missing && !unavailable) continue;
            UnityEventTools.RemovePersistentListener(callback, i);
            if (button != null && unavailable) button.interactable = false;
        }
    }

    [MenuItem("Nox/Gerar versão Windows")]
    public static void Windows()
    {
        PrepareScenes();
        string output = Path.GetFullPath(Path.Combine("Builds", "Windows", "Nox.exe"));
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(item => item.enabled).Select(item => item.path).ToArray(),
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("A build falhou. Consultar o Console e o arquivo de log.");
        Debug.Log("Build Windows gerada: " + output);
    }
}
#endif
