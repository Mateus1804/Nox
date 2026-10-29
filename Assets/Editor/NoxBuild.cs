#if UNITY_EDITOR
using System;
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
using TMPro;

public static class NoxBuild
{
    [MenuItem("Nox/Preparar cenas recuperadas")]
    public static void PrepareScenes() { TryPrepareScenes(); }

    [MenuItem("Nox/Reaplicar preparação v7 (com backup)")]
    public static void Reapply()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Encerrar o Play antes de preparar as cenas.");
        if (!Application.isBatchMode && !EditorUtility.DisplayDialog("Reaplicar preparação",
            "As cenas habilitadas serão preparadas novamente. Uma cópia será salva em Backups/Preparacao-v7. Ajustes locais de apresentação podem ser substituídos.", "Reaplicar", "Cancelar")) return;
        string marker = Path.Combine("ProjectSettings", "NoxScenesPrepared-v7.txt");
        byte[] previous = File.Exists(marker) ? File.ReadAllBytes(marker) : null;
        if (previous != null) File.Delete(marker);
        try { if (!TryPrepareScenes() && previous != null) File.WriteAllBytes(marker, previous); }
        catch { if (previous != null) File.WriteAllBytes(marker, previous); throw; }
    }

    private static bool TryPrepareScenes()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Encerrar o Play antes de preparar as cenas.");
        string marker = Path.Combine("ProjectSettings", "NoxScenesPrepared-v7.txt");
        if (File.Exists(marker))
        {
            Debug.Log("Cenas já preparadas na revisão v7. Para reaplicar, usar Nox > Reaplicar preparação v7 (com backup).");
            return true;
        }
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        GameObject nox = NoxCharacterAssets.Prepare();
        using (var transaction = new NoxSceneTransaction("Preparacao-v7"))
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes.Where(item => item.enabled))
            {
                Scene opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                foreach (GameObject root in opened.GetRootGameObjects())
                {
                    PreparePhysics(root);
                    foreach (PlayerScript player in root.GetComponentsInChildren<PlayerScript>(true))
                        NoxCharacterSetup.Bind(player.gameObject, nox);
                    foreach (PlayerTwo player in root.GetComponentsInChildren<PlayerTwo>(true))
                        NoxCharacterSetup.Bind(player.gameObject, nox);
                    foreach (Button button in root.GetComponentsInChildren<Button>(true)) RepairButton(button);
                    foreach (EventTrigger trigger in root.GetComponentsInChildren<EventTrigger>(true))
                    {
                        if (trigger.triggers == null) continue;
                        foreach (EventTrigger.Entry entry in trigger.triggers)
                            if (entry != null) RemoveMissingCallbacks(entry.callback, FindButtonIncludingInactive(trigger.transform));
                    }
                    foreach (HUDAndSounds hud in root.GetComponentsInChildren<HUDAndSounds>(true)) PrepareContinueButton(hud);
                    foreach (TutorialMethod tutorial in root.GetComponentsInChildren<TutorialMethod>(true))
                    {
                        Match number = Regex.Match(tutorial.name, @"Collider(\d+)$");
                        if (!number.Success) throw new InvalidOperationException("Índice de tutorial não identificado: " + tutorial.name);
                        tutorial.i = int.Parse(number.Groups[1].Value) - 1;
                        if (tutorial.list == null || tutorial.list.tutorials == null ||
                            !NoxRules.IsValidIndex(tutorial.i, tutorial.list.tutorials.Count))
                            throw new InvalidOperationException("Tutorial sem painel correspondente: " + tutorial.name);
                        EditorUtility.SetDirty(tutorial);
                    }
                }
                NoxTraversalRepair.Apply(opened);
                NoxSceneArt.Apply(opened);
                NoxScenePolish.Apply(opened);
                EditorSceneManager.MarkSceneDirty(opened);
                if (!EditorSceneManager.SaveScene(opened)) throw new IOException("A cena não pôde ser salva: " + scene.path);
            }
            File.WriteAllText(marker + ".tmp", "Preparação v7 aplicada com backup das cenas.\n");
            File.Move(marker + ".tmp", marker);
            transaction.Commit();
        }
        return true;
    }

    private static void PrepareContinueButton(HUDAndSounds hud)
    {
        if (hud.levels_menu == null || hud.levels_menu.transform.Find("NoxContinueButton") != null) return;
        LoadLevels levels = hud.GetComponent<LoadLevels>();
        if (levels == null || levels.buttonAirLevel == null) return;
        GameObject item = UnityEngine.Object.Instantiate(levels.buttonAirLevel, hud.levels_menu.transform);
        item.name = "NoxContinueButton";
        RectTransform rect = item.GetComponent<RectTransform>();
        Button button = item.GetComponent<Button>();
        if (rect == null || button == null) throw new InvalidOperationException("Botão de fase sem Button/RectTransform.");
        foreach (EventTrigger trigger in item.GetComponentsInChildren<EventTrigger>(true))
        {
            if (trigger.triggers == null) continue;
            foreach (EventTrigger.Entry entry in trigger.triggers)
            {
                if (entry == null || entry.eventID != EventTriggerType.PointerClick) continue;
                entry.callback = new EventTrigger.TriggerEvent();
                UnityEventTools.AddVoidPersistentListener(entry.callback, hud.ContinueSavedGame_Button);
                ChangeTextColor color = trigger.GetComponent<ChangeTextColor>();
                if (color != null) UnityEventTools.AddVoidPersistentListener(entry.callback, color.ResetTheColor);
            }
        }
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(260, 150);
        rect.sizeDelta = new Vector2(400, 80);
        button.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(button.onClick, hud.ContinueSavedGame_Button);
        NoxResumeButton state = item.GetComponent<NoxResumeButton>();
        if (state == null) state = item.AddComponent<NoxResumeButton>();
        state.button = button;
        state.label = item.GetComponentInChildren<TextMeshProUGUI>(true);
        if (state.label != null)
        {
            state.label.text = "Continuar fase";
            state.label.enableAutoSizing = true;
            state.label.fontSizeMin = 20;
            state.label.fontSizeMax = 40;
            state.label.raycastTarget = true;
        }
        item.SetActive(true);
    }

    private static void PreparePhysics(GameObject root)
    {
        foreach (WaypointsAirPhase platform in root.GetComponentsInChildren<WaypointsAirPhase>(true))
        {
            Rigidbody body = platform.GetComponent<Rigidbody>();
            if (body == null) body = platform.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            EditorUtility.SetDirty(body);
        }
        foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
        {
            if (!item.CompareTag("Box")) continue;
            Collider solid = item.GetComponent<Collider>();
            if (solid == null)
            {
                MeshFilter mesh = item.GetComponent<MeshFilter>();
                if (mesh == null || mesh.sharedMesh == null)
                    throw new InvalidOperationException("Caixa sem geometria para colisão: " + item.name);
                BoxCollider box = item.gameObject.AddComponent<BoxCollider>();
                box.center = mesh.sharedMesh.bounds.center;
                box.size = mesh.sharedMesh.bounds.size;
                solid = box;
            }
            solid.isTrigger = false;
            Rigidbody body = item.GetComponent<Rigidbody>();
            if (body == null) body = item.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = false;
            body.useGravity = true;
            body.constraints |= RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            foreach (Collider range in item.GetComponentsInChildren<Collider>(true))
                if (range.CompareTag("InRange")) { range.isTrigger = true; EditorUtility.SetDirty(range); }
            EditorUtility.SetDirty(body);
            EditorUtility.SetDirty(solid);
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
        if (callback == null) return;
        for (int i = callback.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEngine.Object target = callback.GetPersistentTarget(i);
            string method = callback.GetPersistentMethodName(i);
            // Água, terra e dificuldade não estão implementadas nesta versão.
            bool unavailable = method == "ButtonLevelWater" || method == "ButtonLevelLand" || method == "EasyMediumHard_Button";
            bool missing = target == null || target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).All(item => item.Name != method);
            if (!missing && !unavailable) continue;
            if (missing && !unavailable)
                Debug.LogWarning("Callback inválido removido: " + method + "; botão: " + (button != null ? button.name : "sem botão"));
            UnityEventTools.RemovePersistentListener(callback, i);
            if (button != null && unavailable) button.interactable = false;
        }
    }

    [MenuItem("Nox/Gerar versão Windows")]
    public static void Windows()
    {
        if (!TryPrepareScenes()) return;
        if (EditorBuildSettings.scenes.All(item => !item.enabled))
            throw new InvalidOperationException("Não há cenas habilitadas para gerar a build.");
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
