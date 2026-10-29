using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Executada dentro do backup transacional de cenas; adaptadores só carregam os dados antigos.
public static class NoxComponentMigration
{
    public static void Apply(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots)
        {
            foreach (NoxPlayerController old in root.GetComponentsInChildren<NoxPlayerController>(true))
            {
                if (old.GetType() != typeof(PlayerScript) && old.GetType() != typeof(PlayerTwo))
                    continue;
                NoxPlayerController current = Undo.AddComponent<NoxPlayerController>(old.gameObject);
                EditorUtility.CopySerializedManagedFieldsOnly(old, current);
                current.enabled = old.enabled;
                RedirectReferences(roots, old, current);
                MarkChanged(current);
                Undo.DestroyObjectImmediate(old);
            }
            foreach (NoxBossController old in root.GetComponentsInChildren<NoxBossController>(true))
            {
                if (old.GetType() != typeof(Boss) && old.GetType() != typeof(BossScript))
                    continue;
                bool fire = old.UnlocksFire;
                NoxBossController current = Undo.AddComponent<NoxBossController>(old.gameObject);
                EditorUtility.CopySerializedManagedFieldsOnly(old, current);
                current.enabled = old.enabled;
                current.ConfigureProfile(fire, fire);
                RedirectReferences(roots, old, current);
                MarkChanged(current);
                Undo.DestroyObjectImmediate(old);
            }
            foreach (NoxPortalRotation old in root.GetComponentsInChildren<NoxPortalRotation>(true))
            {
                if (old.GetType() != typeof(portaldentro))
                    continue;
                NoxPortalRotation current = Undo.AddComponent<NoxPortalRotation>(old.gameObject);
                current.degreesPerSecond = old.AngularSpeed;
                current.enabled = old.enabled;
                RedirectReferences(roots, old, current);
                MarkChanged(current);
                Undo.DestroyObjectImmediate(old);
            }
            // Marcador sem comportamento desde a remoção do SetParent nas plataformas.
            foreach (plataforma marker in root.GetComponentsInChildren<plataforma>(true))
                if (marker.GetType() == typeof(plataforma))
                    Undo.DestroyObjectImmediate(marker);
        }
    }

    private static void RedirectReferences(GameObject[] roots, Component previous, Component current)
    {
        foreach (MonoBehaviour component in roots.SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true)))
        {
            if (component == null || component == previous)
                continue;
            using (var serialized = new SerializedObject(component))
            {
                SerializedProperty property = serialized.GetIterator();
                bool changed = false;
                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue != previous)
                        continue;
                    if (property.type.Contains(previous.GetType().Name) && !previous.GetType().IsAssignableFrom(current.GetType()))
                        throw new InvalidOperationException("Referência externa exige o tipo antigo: " + component.name + " / " + property.propertyPath);
                    property.objectReferenceValue = current;
                    if (property.propertyPath.EndsWith(".m_Target", StringComparison.Ordinal))
                    {
                        string prefix = property.propertyPath.Substring(0, property.propertyPath.Length - "m_Target".Length);
                        SerializedProperty type = serialized.FindProperty(prefix + "m_TargetAssemblyTypeName");
                        if (type != null)
                            type.stringValue = current.GetType().AssemblyQualifiedName;
                    }
                    changed = true;
                }
                if (!changed)
                    continue;
                serialized.ApplyModifiedProperties();
                MarkChanged(component);
            }
        }
    }
    private static void MarkChanged(Component component)
    {
        EditorUtility.SetDirty(component);
        if (PrefabUtility.IsPartOfPrefabInstance(component))
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
    }

    [Serializable]
    private sealed class Catalog
    {
        public Entry[] scripts = new Entry[0];
    }
    [Serializable]
    private sealed class Entry
    {
        public string to = "";
        public string newType = "";
        public string guid = "";
        public bool component = false;
    }

    [MenuItem("Nox/Verificar migração de scripts")]
    public static void Verify()
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Encerrar o Play antes da verificação.");
        Catalog catalog = JsonUtility.FromJson<Catalog>(File.ReadAllText("Assets/Editor/Build/script-migration.json"));
        foreach (Entry entry in catalog.scripts.Where(item => item.component))
        {
            string path = AssetDatabase.GUIDToAssetPath(entry.guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            Require(path == entry.to && script != null && script.GetClass() != null && script.GetClass().Name == entry.newType,
                "MonoScript não corresponde ao GUID/tipo esperado: " + entry.to);
        }
        string folder = "Assets/NoxMigrationCheck-" + Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(folder);
        AssetDatabase.Refresh();
        Scene preview = EditorSceneManager.NewPreviewScene();
        NoxPlayerController previous = NoxPlayerController.Active;
        PlayerScript previousLegacy = PlayerScript.PT;
        PlayerTwo previousAlternate = PlayerTwo.PT;
        try
        {
            CheckPlayerPrefab(preview, folder, typeof(PlayerScript), "slash1");
            CheckPlayerPrefab(preview, folder, typeof(PlayerTwo), "firstSlash");
            foreach (bool fire in new[] { false, true })
            {
                GameObject item = new GameObject(fire ? "Guardião ar" : "Guardião fogo");
                SceneManager.MoveGameObjectToScene(item, preview);
                NoxBossController legacy = (NoxBossController)item.AddComponent(fire ? typeof(BossScript) : typeof(Boss));
                legacy.health = 3;
                legacy.enabled = false;
                Apply(preview);
                NoxBossController migrated = item.GetComponent<NoxBossController>();
                Require(migrated.GetType() == typeof(NoxBossController) && migrated.health == 3 && migrated.UnlocksFire == fire && !migrated.enabled,
                    "Perfil ou vida do chefe não preservados.");
            }
            GameObject portal = new GameObject("Portal interno");
            SceneManager.MoveGameObjectToScene(portal, preview);
            portal.AddComponent<portaldentro>();
            portal.AddComponent<plataforma>();
            Apply(preview);
            Require(portal.GetComponent<NoxPortalRotation>().GetType() == typeof(NoxPortalRotation) &&
                portal.GetComponent<NoxPortalRotation>().degreesPerSecond == -10 && portal.GetComponent<plataforma>() == null,
                "Portal ou marcador não migrados.");
            Debug.Log("Migração verificada: catálogo de GUIDs, dois prefabs com nomes antigos, referências de jogador, chefes e portal. O Play ainda deve ser conferido.");
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(preview);
            NoxPlayerController.Active = previous;
            PlayerScript.PT = previousLegacy;
            PlayerTwo.PT = previousAlternate;
            AssetDatabase.DeleteAsset(folder);
        }
    }

    private static void CheckPlayerPrefab(Scene preview, string folder, Type legacyType, string oldAttackName)
    {
        GameObject item = new GameObject(legacyType.Name);
        SceneManager.MoveGameObjectToScene(item, preview);
        Rigidbody body = item.AddComponent<Rigidbody>();
        NoxPlayerController player = (NoxPlayerController)item.AddComponent(legacyType);
        GameObject effect = new GameObject("Efeito de teste");
        effect.transform.SetParent(item.transform, false);
        player.enabled = false;
        player.physicsBody = body;
        player.health = 2;
        player.isGrounded = true;
        player.groundLayers = 8;
        player.firstAttackPrefab = effect;
        NoxCrateInteraction interaction = item.AddComponent<NoxCrateInteraction>();
        interaction.player = player;
        string path = folder + "/" + legacyType.Name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(item, path);
        string text = File.ReadAllText(path);
        foreach (var pair in new[] { new[] { "physicsBody", "rigid" }, new[] { "health", "hp" },
            new[] { "isGrounded", "grounded" }, new[] { "groundLayers", "WhatIsGround" }, new[] { "firstAttackPrefab", oldAttackName } })
            text = Regex.Replace(text, "(?m)^  " + pair[0] + ":", "  " + pair[1] + ":");
        File.WriteAllText(path, text);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        NoxPlayerController loaded = prefab.GetComponent<NoxPlayerController>();
        Require(loaded.health == 2 && !loaded.enabled && loaded.isGrounded && loaded.groundLayers.value == 8 &&
            loaded.physicsBody != null && loaded.firstAttackPrefab != null, "Dados antigos do prefab não foram reconhecidos.");
        Apply(preview);
        NoxPlayerController migrated = item.GetComponent<NoxPlayerController>();
        Require(migrated.GetType() == typeof(NoxPlayerController) && interaction.player == migrated &&
            migrated.health == 2 && !migrated.enabled && migrated.physicsBody == body && migrated.firstAttackPrefab == effect,
            "Estado ou referências do jogador não preservados.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
