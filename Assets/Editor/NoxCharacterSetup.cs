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
        if (Application.isPlaying) throw new InvalidOperationException("Encerrar o Play antes de vincular o modelo.");
        GameObject model = Selection.activeObject as GameObject;
        if (model == null || !AssetDatabase.Contains(model) || model.GetComponentsInChildren<Renderer>(true).Length == 0)
            throw new InvalidOperationException("Selecionar no painel Project o prefab ou FBX original do personagem Nox.");
        // O modelo não pode trazer outro controlador físico para dentro do jogador.
        if (model.GetComponentsInChildren<Rigidbody>(true).Length != 0 ||
            model.GetComponentsInChildren<Collider>(true).Length != 0 ||
            model.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
            throw new InvalidOperationException("Usar um prefab visual, sem scripts, colliders ou Rigidbody. Preservar o original ao criar essa cópia.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        int changed = 0;
        using (var transaction = new NoxSceneTransaction("Visual-Nox"))
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes.Where(item => item.enabled))
            {
                Scene opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                foreach (GameObject root in opened.GetRootGameObjects())
                    foreach (PlayerScript player in root.GetComponentsInChildren<PlayerScript>(true))
                    { Bind(player.gameObject, model); changed++; }
                EditorSceneManager.MarkSceneDirty(opened);
                if (!EditorSceneManager.SaveScene(opened)) throw new InvalidOperationException("Falha ao salvar " + scene.path);
            }
            transaction.Commit();
        }
        Debug.Log("Modelo do Nox vinculado a " + changed + " jogador(es). Conferir orientação, materiais e animações no Play.");
    }

    public static void Bind(GameObject player, GameObject model)
    {
        Collider body = player.GetComponent<Collider>();
        if (body == null) throw new InvalidOperationException("Jogador sem collider: " + player.name);
        Transform previous = player.transform.Find("NoxVisual");
        // Somente o filho de apresentação gerado por este comando é substituído.
        GameObject holder = new GameObject("NoxVisual");
        holder.transform.SetParent(player.transform, false);
        Vector3 planar = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up);
        holder.transform.rotation = Quaternion.LookRotation(planar.sqrMagnitude > 0.001f ? planar : Vector3.forward);
        GameObject visual = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (visual == null) throw new InvalidOperationException("O modelo não pôde ser instanciado.");
        GameObject alignment = new GameObject("NoxModelAlignment");
        alignment.transform.SetParent(holder.transform, false);
        visual.transform.SetParent(alignment.transform, false);
        // O pivô acompanha a direção física; a correção do modelo fica em um filho.
        // Assim LateUpdate não apaga o alinhamento calculado a partir do rosto.
        AnimationClip idle = AssetDatabase.LoadAllAssetsAtPath(NoxCharacterAssets.ModelPath)
            .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).Single(c => c.name.EndsWith("Nox_Idle", StringComparison.Ordinal));
        idle.SampleAnimation(visual, 0);
        Vector3 face = FaceDirection(visual);
        Vector3 heading = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;
        if (heading.sqrMagnitude < 0.5f) heading = Vector3.forward;
        alignment.transform.rotation = Quaternion.FromToRotation(face, heading) * alignment.transform.rotation;
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
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        MeshRenderer cube = player.GetComponent<MeshRenderer>();
        if (cube != null) cube.enabled = false;
        EditorUtility.SetDirty(player);
    }
    public static Vector3 FaceDirection(GameObject visual)
    {
        Transform head = visual.GetComponentsInChildren<Transform>(true)
            .SingleOrDefault(t => t.name == "cabeça");
        if (head == null) throw new InvalidOperationException("Osso da cabeça do Nox ausente.");
        Vector3 eyes = Vector3.zero; int count = 0;
        foreach (SkinnedMeshRenderer renderer in visual.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            Mesh baked = new Mesh();
            try
            {
                renderer.BakeMesh(baked);
                Vector3[] vertices = baked.vertices;
                Material[] materials = renderer.sharedMaterials;
                for (int sub = 0; sub < baked.subMeshCount && sub < materials.Length; sub++)
                {
                    if (materials[sub] == null || !materials[sub].name.StartsWith("Nox_Eyes", StringComparison.Ordinal)) continue;
                    foreach (int index in baked.GetTriangles(sub).Distinct())
                    { eyes += renderer.transform.TransformPoint(vertices[index]); count++; }
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(baked); }
        }
        if (count == 0) throw new InvalidOperationException("Material dos olhos do Nox ausente; orientação não determinada.");
        Vector3 forward = Vector3.ProjectOnPlane(eyes / count - head.position, Vector3.up);
        if (forward.sqrMagnitude < 0.00001f) throw new InvalidOperationException("Rosto sem direção horizontal válida.");
        return forward.normalized;
    }

}
#endif
