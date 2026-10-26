#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NoxCharacterSetup
{
    [MenuItem("Nox/Vincular modelo selecionado do Nox")]
    public static void BindSelectedModel()
    {
        GameObject model = Selection.activeObject as GameObject;
        if (model == null || !AssetDatabase.Contains(model) || model.GetComponentsInChildren<Renderer>(true).Length == 0)
            throw new InvalidOperationException("Selecionar no painel Project o prefab ou FBX original do personagem Nox.");
        // O modelo não pode trazer outro controlador físico para dentro do jogador.
        if (model.GetComponentsInChildren<Rigidbody>(true).Length != 0 ||
            model.GetComponentsInChildren<Collider>(true).Length != 0 ||
            model.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
            throw new InvalidOperationException("Usar um prefab visual, sem scripts, colliders ou Rigidbody. Preservar o original ao criar essa cópia.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
        int changed = 0;
        try
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes.Where(item => item.enabled))
            {
                Scene opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                foreach (GameObject root in opened.GetRootGameObjects())
                    foreach (PlayerScript player in root.GetComponentsInChildren<PlayerScript>(true))
                    { Bind(player.gameObject, model); changed++; }
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        Debug.Log("Modelo do Nox vinculado a " + changed + " jogador(es). Conferir orientação, materiais e animações no Play.");
    }

    private static void Bind(GameObject player, GameObject model)
    {
        Collider body = player.GetComponent<Collider>();
        if (body == null) throw new InvalidOperationException("Jogador sem collider: " + player.name);
        Transform previous = player.transform.Find("NoxVisual");
        // Somente o filho de apresentação gerado por este comando é substituído.
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        GameObject holder = new GameObject("NoxVisual");
        holder.transform.SetParent(player.transform, false);
        GameObject visual = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (visual == null) throw new InvalidOperationException("O modelo não pôde ser instanciado.");
        visual.transform.SetParent(holder.transform, false);
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        if (bounds.size.y <= 0.0001f) throw new InvalidOperationException("Modelo sem altura válida.");
        float fit = body.bounds.size.y / bounds.size.y;
        holder.transform.localScale *= fit;
        bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        holder.transform.position += new Vector3(body.bounds.center.x - bounds.center.x,
            body.bounds.min.y - bounds.min.y, body.bounds.center.z - bounds.center.z);
        foreach (Animator animator in visual.GetComponentsInChildren<Animator>(true)) animator.applyRootMotion = false;
        NoxCharacterVisual adapter = player.GetComponent<NoxCharacterVisual>();
        if (adapter == null) adapter = player.AddComponent<NoxCharacterVisual>();
        adapter.modelPrefab = model;
        MeshRenderer cube = player.GetComponent<MeshRenderer>();
        if (cube != null) cube.enabled = false;
        EditorUtility.SetDirty(player);
    }
}
#endif
