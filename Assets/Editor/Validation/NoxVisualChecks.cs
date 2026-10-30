using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NoxVisualChecks
{
    [MenuItem("Nox/Verificar animações e cenário")]
    public static void Run()
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Encerrar o Play antes da verificação.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(NoxCharacterAssets.ModelPath).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        foreach (string name in new[] { "Nox_Idle", "Nox_Walk", "Nox_Jump", "Nox_Attack", "Nox_Fall", "Nox_Land", "Nox_Death" })
        {
            AnimationClip clip = clips.FirstOrDefault(c => c.name.EndsWith(name, StringComparison.Ordinal));
            Require(clip != null, "Clip ausente: " + name);
            Require(clip.length > 0, "Clip sem duração: " + name);
            Require(clip.isLooping == (name == "Nox_Idle" || name == "Nox_Walk"), "Repetição incorreta: " + name);
        }
        ValidateImportedPoses(clips);
        Shader surfaceShader = Shader.Find("Nox/Superficie mineral");
        Require(surfaceShader != null && !ShaderUtil.ShaderHasError(surfaceShader), "Shader mineral ausente ou inválido.");
        Shader sky = Shader.Find("Nox/Ceu espiritual");
        Require(sky != null && !ShaderUtil.ShaderHasError(sky), "Shader do céu ausente ou com erro de compilação.");
        SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
        int players = 0;
        try
        {
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes.Where(s => s.enabled))
            {
                Scene scene = EditorSceneManager.OpenScene(entry.path, OpenSceneMode.Single);
                GameObject[] roots = scene.GetRootGameObjects();
                GameObject art = roots.FirstOrDefault(r => r.name == NoxSceneArt.RootName);
                Require(art != null, "Cena sem ambientação v7: " + entry.path);
                Require(art.GetComponentsInChildren<Collider>(true).Length == 0, "Cenografia com collider: " + entry.path);
                Require(RenderSettings.sun != null && RenderSettings.sun.isActiveAndEnabled,
                    "Luz principal ausente ou herdada de uma ambientação desativada: " + entry.path);
                foreach (NoxCharacterVisual visual in roots.SelectMany(r => r.GetComponentsInChildren<NoxCharacterVisual>(true)))
                {
                    players++;
                    Transform child = visual.transform.Find("NoxVisual");
                    Require(child != null, "Jogador sem visual: " + entry.path);
                    Animator animator = child.GetComponentInChildren<Animator>();
                    Require(animator != null && animator.runtimeAnimatorController != null && !animator.applyRootMotion,
                        "Animator inválido: " + entry.path);
                    Require(Vector3.Dot(NoxCharacterSetup.FaceDirection(child.gameObject),
                        Vector3.ProjectOnPlane(visual.transform.forward, Vector3.up).normalized) > 0.9f,
                        "O rosto não acompanha a direção física: " + entry.path);
                    Require(animator.parameters.Any(p => p.name == "Cadence") &&
                        animator.parameters.Any(p => p.name == "Alive") && animator.parameters.Any(p => p.name == "VerticalSpeed"), "Controller anterior à revisão v7: " + entry.path);
                    foreach (Transform surface in roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Where(t => t.name == "NoxSurface-v7"))
                        Require(surface.GetComponentsInChildren<Collider>(true).Length == 0, "Detalhe visual com collider.");
                }
            }
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
        Require(players > 0, "Nenhum jogador preparado nas cenas da build.");
        Debug.Log("Verificação de animações e cenário concluída: " + players + " jogador(es). O percurso no Play ainda precisa ser conferido.");
    }

    public static void ValidateImportedPoses(AnimationClip[] clips)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(NoxCharacterAssets.ModelPath);
        GameObject instance = UnityEngine.Object.Instantiate(model);
        instance.hideFlags = HideFlags.HideAndDontSave;
        try
        {
            foreach (string name in new[] { "Nox_Idle", "Nox_Walk", "Nox_Jump", "Nox_Fall", "Nox_Land" })
            {
                AnimationClip clip = clips.Single(c => c.name.EndsWith(name, StringComparison.Ordinal));
                foreach (float fraction in new[] { 0f, 0.25f, 0.5f, 0.75f, 0.99f })
                {
                    clip.SampleAnimation(instance, clip.length * fraction);
                    Transform[] bones = instance.GetComponentsInChildren<Transform>(true);
                    Vector3 head = bones.Single(b => b.name == "cabeça").position;
                    Vector3 feet = (bones.Single(b => b.name == "pe.L").position + bones.Single(b => b.name == "pe.D").position) / 2;
                    Require(Vector3.Dot((head - feet).normalized, Vector3.up) > 0.85f,
                        "Pose inclinada no Unity: " + name + " / " + fraction);
                    Require(head.y - feet.y > 0.6f, "Altura do esqueleto inválida: " + name);
                }
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
