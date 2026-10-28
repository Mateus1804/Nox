#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class NoxScenePolish
{
    public static void Apply(Scene scene)
    {
        Material cueMaterial = CueMaterial();
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            foreach (Boss boss in root.GetComponentsInChildren<Boss>(true))
            {
                Collider old = boss.areaAttack;
                if (old != null && old.isTrigger) old.enabled = false;
                NoxEnemyStrike strike = Strike(boss.gameObject, Mathf.Max(1.5f, boss.rangeAttack), cueMaterial, true);
                boss.areaAttack = (CapsuleCollider)strike.damageArea;
                boss.hpBoss = Mathf.Clamp(boss.hpBoss, 1, 30);
            }
            foreach (BossScript boss in root.GetComponentsInChildren<BossScript>(true))
            {
                Collider old = boss.areaAttack;
                if (old != null && old.isTrigger) old.enabled = false;
                NoxEnemyStrike strike = Strike(boss.gameObject, Mathf.Max(1.5f, boss.rangeAttack), cueMaterial, false);
                boss.areaAttack = (SphereCollider)strike.damageArea;
            }
            foreach (EnemyAir enemy in root.GetComponentsInChildren<EnemyAir>(true)) Strike(enemy.gameObject, 1.6f, cueMaterial, false);
            foreach (AimEnemy turret in root.GetComponentsInChildren<AimEnemy>(true)) turret.cue = Cue(turret.gameObject, cueMaterial);
            foreach (EnemyPatrol patrol in root.GetComponentsInChildren<EnemyPatrol>(true))
            {
                EnemiesWaypoint route = patrol.GetComponent<EnemiesWaypoint>();
                bool boss = patrol.GetComponent<Boss>() != null || patrol.GetComponent<BossScript>() != null;
                if (boss || route != null && route.waypoints != null && route.waypoints.Length > 0) patrol.enabled = false;
                if (boss && route != null) route.enabled = false;
            }
            foreach (Lever lever in root.GetComponentsInChildren<Lever>(true))
            {
                if (lever.stateLabel != null) continue;
                GameObject item = new GameObject("NoxLeverLabel-v6"); item.transform.SetParent(lever.transform, false);
                Renderer surface = lever.GetComponent<Renderer>();
                item.transform.position = (surface != null ? surface.bounds.center : lever.transform.position) + Vector3.up * 0.65f;
                Vector3 scale = lever.transform.lossyScale;
                item.transform.localScale = new Vector3(1 / Mathf.Max(0.001f, Mathf.Abs(scale.x)),
                    1 / Mathf.Max(0.001f, Mathf.Abs(scale.y)), 1 / Mathf.Max(0.001f, Mathf.Abs(scale.z)));
                TextMesh label = item.AddComponent<TextMesh>(); label.text = "Aguardando peso"; label.characterSize = 0.35f;
                label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                item.GetComponent<MeshRenderer>().sharedMaterial = label.font.material;
                label.fontSize = 64; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
                label.color = Ink; item.AddComponent<Billboard>(); lever.stateLabel = label;
            }
            foreach (Button button in root.GetComponentsInChildren<Button>(true)) KeyboardAction(button);
            foreach (CanvasScaler scaler in root.GetComponentsInChildren<CanvasScaler>(true))
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            }
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            { text.text = Translate(text.text); text.color = Ink; text.raycastTarget = text.GetComponentInParent<Button>() == null &&
                    (text.GetComponentInParent<Selectable>() != null || text.GetComponent<EventTrigger>() != null); }
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                text.text = Translate(text.text); text.color = Ink;
                text.raycastTarget = text.GetComponentInParent<Button>() == null &&
                    (text.GetComponentInParent<Selectable>() != null || text.GetComponent<EventTrigger>() != null);
                text.enableAutoSizing = true; text.fontSizeMin = 18;
                text.fontSizeMax = text.name.Contains("Title") ? 88 : text.GetComponentInParent<Button>() != null ? 38 : 40;
            }
            foreach (ChangeTextColor hover in root.GetComponentsInChildren<ChangeTextColor>(true))
            { hover.originalColor = Ink; hover.secondColor = new Color(0.91f, 0.79f, 0.52f); }
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                string name = image.name.ToLowerInvariant();
                if (name.Contains("background") && !name.Contains("health")) image.color = new Color(0.055f, 0.09f, 0.12f, 0.96f);
                if (name.Contains("outline")) image.color = new Color(0.45f, 0.6f, 0.59f, 0.7f);
            }
            foreach (HUDAndSounds hud in root.GetComponentsInChildren<HUDAndSounds>(true))
            {
                foreach (GameObject panel in new[] { hud.main_menu, hud.levels_menu, hud.pause_panel, hud.options_panel,
                    hud.controls_panel, hud.audio_panel, hud.graphics_panel, hud.howToPlay_panel, hud.credits_panel }) Focus(panel);
                AddOptions(hud);
            }
            foreach (PlaceHolderPanel result in root.GetComponentsInChildren<PlaceHolderPanel>(true)) Focus(result.placeHolderPanel);
        }
        if (scene.buildIndex != 0 && roots.All(root => root.name != "NoxPlayHud-v6")) CreatePlayHud(scene);
    }

    private static readonly Color Ink = new Color(0.91f, 0.94f, 0.9f);
    private static Material CueMaterial()
    {
        const string path = "Assets/Art/NoxThreat.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) throw new InvalidOperationException("Shader de sinalização ausente.");
        material = new Material(shader);
        AssetDatabase.CreateAsset(material, path); return material;
    }
    private static NoxThreatCue Cue(GameObject owner, Material material)
    {
        Transform previous = owner.transform.Find("NoxThreat-v6");
        if (previous != null) return previous.GetComponent<NoxThreatCue>();
        GameObject item = new GameObject("NoxThreat-v6"); item.transform.SetParent(owner.transform, false);
        NoxThreatCue cue = item.AddComponent<NoxThreatCue>(); cue.material = material; return cue;
    }
    private static NoxEnemyStrike Strike(GameObject owner, float reach, Material material, bool capsule)
    {
        NoxEnemyStrike existing = owner.GetComponent<NoxEnemyStrike>();
        if (existing != null) return existing;
        NoxEnemyStrike strike = owner.AddComponent<NoxEnemyStrike>();
        strike.reach = reach; strike.windup = owner.GetComponent<EnemyAir>() != null ? 0.45f : 0.65f;
        strike.cue = Cue(owner, material);
        GameObject area = new GameObject("NoxDamageArea-v6"); area.transform.SetParent(owner.transform, false);
        Vector3 scale = owner.transform.lossyScale;
        area.transform.localScale = new Vector3(1 / Mathf.Max(0.001f, Mathf.Abs(scale.x)),
            1 / Mathf.Max(0.001f, Mathf.Abs(scale.y)), 1 / Mathf.Max(0.001f, Mathf.Abs(scale.z)));
        area.transform.position = owner.transform.position + Vector3.up * 0.4f;
        if (capsule)
        {
            CapsuleCollider hit = area.AddComponent<CapsuleCollider>(); hit.radius = reach * 0.55f; hit.height = 1.5f;
            hit.center = new Vector3(0, 0, reach * 0.45f); strike.damageArea = hit;
        }
        else
        {
            SphereCollider hit = area.AddComponent<SphereCollider>(); hit.radius = reach * 0.55f;
            hit.center = new Vector3(0, 0, reach * 0.45f); strike.damageArea = hit;
        }
        strike.damageArea.isTrigger = true; strike.damageArea.enabled = false;
        return strike;
    }

    private static void Focus(GameObject panel)
    { if (panel != null && panel.GetComponent<NoxMenuFocus>() == null) panel.AddComponent<NoxMenuFocus>(); }

    public static void KeyboardAction(Button button)
    {
        if (!(button.targetGraphic is Image))
        {
            RectTransform rect = Rect("NoxButtonSurface-v6", button.transform, Vector2.zero, Vector2.zero);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero; rect.SetAsFirstSibling();
            Image background = rect.gameObject.AddComponent<Image>(); background.color = Color.white;
            button.targetGraphic = background;
        }
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.12f, 0.22f, 0.27f);
        colors.highlightedColor = colors.selectedColor = new Color(0.28f, 0.47f, 0.49f);
        colors.pressedColor = new Color(0.38f, 0.58f, 0.57f);
        colors.disabledColor = new Color(0.16f, 0.2f, 0.22f, 0.65f);
        button.colors = colors;
        Navigation navigation = button.navigation; navigation.mode = Navigation.Mode.Automatic; button.navigation = navigation;
        EventTrigger[] triggers = button.GetComponentsInChildren<EventTrigger>(true);
        foreach (EventTrigger trigger in triggers)
        {
            if (trigger.triggers == null || trigger.GetComponentInParent<Button>() != button) continue;
            foreach (EventTrigger.Entry entry in trigger.triggers.Where(item => item.eventID == EventTriggerType.PointerClick))
            {
                var migrated = new List<int>();
                for (int i = 0; i < entry.callback.GetPersistentEventCount(); i++)
                {
                    UnityEngine.Object target = entry.callback.GetPersistentTarget(i);
                    string method = entry.callback.GetPersistentMethodName(i);
                    if (target == null) continue;
                    MethodInfo info = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
                    if (info == null || info.ReturnType != typeof(void))
                    { Debug.LogWarning("Callback com argumentos preservado para revisão: " + method, button); continue; }
                    bool found = Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Any(n =>
                        button.onClick.GetPersistentTarget(n) == target && button.onClick.GetPersistentMethodName(n) == method);
                    if (!found)
                    {
                        UnityEventCallState state = ListenerState(trigger, entry, i);
                        UnityAction action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, info);
                        UnityEventTools.AddVoidPersistentListener(button.onClick, action);
                        button.onClick.SetPersistentListenerState(button.onClick.GetPersistentEventCount() - 1,
                            state);
                    }
                    migrated.Add(i);
                }
                for (int i = migrated.Count - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(entry.callback, migrated[i]);
                EditorUtility.SetDirty(trigger);
            }
        }
        EditorUtility.SetDirty(button);
    }

    private static UnityEventCallState ListenerState(EventTrigger trigger, EventTrigger.Entry entry, int index)
    {
        // Unity 2021 possui setter público, mas ainda não expõe GetPersistentListenerState.
        SerializedObject serialized = new SerializedObject(trigger);
        serialized.Update();
        SerializedProperty entries = serialized.FindProperty("m_Delegates");
        int position = trigger.triggers.IndexOf(entry);
        SerializedProperty calls = entries != null && position >= 0 && position < entries.arraySize ?
            entries.GetArrayElementAtIndex(position).FindPropertyRelative("callback.m_PersistentCalls.m_Calls") : null;
        if (calls == null || index >= calls.arraySize)
            throw new InvalidOperationException("Estado do callback não identificado: " + trigger.name);
        return (UnityEventCallState)calls.GetArrayElementAtIndex(index).FindPropertyRelative("m_CallState").intValue;
    }

    private static void AddOptions(HUDAndSounds hud)
    {
        if (hud.graphics_panel == null || hud.graphics_panel.transform.Find("NoxPresentation-v6") != null) return;
        // Abaixo dos controles originais (centro Y=0), acima do botão Voltar (base Y=150).
        RectTransform panel = Rect("NoxPresentation-v6", hud.graphics_panel.transform, new Vector2(0, -200), new Vector2(1100, 180));
        NoxPresentationOptions options = panel.gameObject.AddComponent<NoxPresentationOptions>();
        options.damageFlash = ToggleControl(panel, "Realce ao receber dano", new Vector2(0, 50));
        options.reducedMotion = ToggleControl(panel, "Reduzir movimentos de câmera e avisos", new Vector2(0, -40));
    }
    private static Toggle ToggleControl(Transform parent, string label, Vector2 position)
    {
        RectTransform row = Rect(label, parent, position, new Vector2(1050, 72));
        Image hitArea = row.gameObject.AddComponent<Image>(); hitArea.color = Color.clear;
        Toggle toggle = row.gameObject.AddComponent<Toggle>();
        RectTransform background = Rect("Caixa", row, new Vector2(-475, 0), new Vector2(54, 54));
        Image box = background.gameObject.AddComponent<Image>(); box.color = new Color(0.17f, 0.28f, 0.32f);
        RectTransform mark = Rect("Marca", background, Vector2.zero, new Vector2(30, 30));
        Image check = mark.gameObject.AddComponent<Image>(); check.color = new Color(0.65f, 0.82f, 0.7f);
        toggle.targetGraphic = box; toggle.graphic = check;
        Label(row, label, new Vector2(45, 0), new Vector2(900, 72), 28);
        return toggle;
    }
    private static void CreatePlayHud(Scene scene)
    {
        GameObject owner = new GameObject("NoxPlayHud-v6"); SceneManager.MoveGameObjectToScene(owner, scene);
        Canvas canvas = owner.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 0;
        CanvasScaler scaler = owner.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
        RectTransform ribbon = Rect("Informações", owner.transform, new Vector2(0, 110), new Vector2(1680, 54));
        ribbon.anchorMin = ribbon.anchorMax = new Vector2(0.5f, 0);
        Image surface = ribbon.gameObject.AddComponent<Image>(); surface.color = new Color(0.035f, 0.075f, 0.1f, 0.9f); surface.raycastTarget = false;
        NoxPlayHud hud = owner.AddComponent<NoxPlayHud>();
        hud.status = Label(ribbon, "", Vector2.zero, new Vector2(1620, 50), 27);
        RectTransform top = Rect("Objetivo", owner.transform, new Vector2(0, -50), new Vector2(1300, 60));
        top.anchorMin = top.anchorMax = new Vector2(0.5f, 1);
        hud.objective = Label(top, "", Vector2.zero, new Vector2(1300, 60), 30);
    }
    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject item = new GameObject(name, typeof(RectTransform)); RectTransform rect = (RectTransform)item.transform;
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
    }
    private static Text Label(Transform parent, string text, Vector2 position, Vector2 size, int fontSize)
    {
        Text label = Rect("Texto", parent, position, size).gameObject.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); label.text = text; label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter; label.color = Ink; label.raycastTarget = false;
        return label;
    }
    private static string Translate(string text)
    {
        string source = (text ?? "").Trim();
        switch (source)
        {
            case "Play": return "Jogar"; case "Back": return "Voltar"; case "Quit": return "Sair";
            case "Options": return "Opções"; case "Controls": return "Controles"; case "Graphics": return "Gráficos";
            case "Credits": return "Créditos"; case "How To Play": return "Como jogar";
            case "Continue": return "Continuar"; case "Restart": return "Tentar novamente";
            case "Next Level": return "Próxima fase"; case "Game Over": return "Fim de jogo";
            case "PAUSED": case "Pause": return "Pausa"; case "Levels": return "Mundos";
            case "Move": return "Movimentar"; case "Jump": return "Saltar"; case "Attack": return "Atacar";
            case "Interact": return "Interagir"; case "Sound": return "Som"; case "On": return "Ativado";
            case "OFF": return "Desativado"; case "Audio": return "Áudio"; case "Soft": return "Suave";
            case "Shadow Strength": return "Intensidade das sombras"; case "Shadow Quality": return "Qualidade das sombras";
            case "Coming soon!": return "Indisponível nesta versão"; case "The Island in the sky": return "Ilhas do ar";
            case "Underwater": return "Mundo da água"; case "Planet earth": return "Mundo da terra";
            case "Use        to JUMP": return "Espaço • Saltar entre as plataformas";
            case "Use    to interact with a box": return "E • Segurar ou soltar a caixa";
            case "Collect     to restore life": return "Colete itens de cura para recuperar vida";
            case "Use     to attack": return "Clique esquerdo • Atacar. Clique novamente para encadear golpes";
            case "Use        to move around": return "W A S D • Movimentar";
        }
        if (source.StartsWith("You win the level", StringComparison.Ordinal))
            return "Explore as ilhas, resolva os desafios e encontre a passagem. No santuário, derrote o guardião.\n\nW A S D para mover, Espaço para saltar, E para caixas e clique esquerdo para atacar.\n\nEvite perigos e procure itens de cura. A vida não ultrapassa o limite. Esc abre a pausa.";
        return text;
    }
}
#endif
