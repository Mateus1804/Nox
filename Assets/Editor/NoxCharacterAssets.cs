using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class NoxCharacterAssets
{
    public const string ModelPath = "Assets/Characters/Nox/Nox.fbx";
    private const string ControllerPath = "Assets/Characters/Nox/Nox-v7.controller";
    private const string PrefabPath = "Assets/Characters/Nox/NoxPlayable-v7.prefab";

    public static GameObject Prepare()
    {
        // Reimportação explícita também corrige projetos já preparados na revisão anterior.
        ModelImporter importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
        if (importer == null)
            throw new InvalidOperationException("FBX do Nox ausente.");
        NoxModelImporter.ConfigureClips(importer);
        importer.SaveAndReimport();
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null)
            throw new InvalidOperationException("Modelo original Nox não importado: " + ModelPath);
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        string[] names = { "Nox_Idle", "Nox_Walk", "Nox_Jump", "Nox_Attack", "Nox_Fall", "Nox_Land", "Nox_Death" };
        AnimationClip[] motions = names.Select(name => clips.FirstOrDefault(c => c.name.EndsWith(name, StringComparison.Ordinal))).ToArray();
        if (motions.Any(c => c == null))
            throw new InvalidOperationException("Um dos sete clips do Nox não foi importado. Conferir a aba Animation do FBX.");
        if (!motions[0].isLooping || !motions[1].isLooping)
            throw new InvalidOperationException("Repouso e caminhada precisam de Loop Time. Reimportar o FBX antes de preparar as cenas.");
        NoxVisualChecks.ValidateImportedPoses(clips);
        // Clips próprios permitem excluir somente as curvas do objeto de importação.
        // O esqueleto mantém todas as curvas; o motor continua dono do deslocamento.
        Directory.CreateDirectory("Assets/Characters/Nox/Generated-v7");
        AssetDatabase.Refresh();
        motions = motions.Select(PrepareClip).ToArray();
        if (existing != null)
            return existing;
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Cadence", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Alive", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            AnimatorControllerParameter[] defaults = controller.parameters;
            foreach (AnimatorControllerParameter parameter in defaults)
                if (parameter.name == "Alive")
                    parameter.defaultBool = true;
            controller.parameters = defaults;
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idle = machine.AddState("Repouso");
            idle.motion = motions[0];
            AnimatorState walk = machine.AddState("Caminhada");
            walk.motion = motions[1];
            walk.speedParameter = "Cadence";
            walk.speedParameterActive = true;
            AnimatorState jump = machine.AddState("Subida");
            jump.motion = motions[2];
            AnimatorState attack = machine.AddState("Ataque");
            attack.motion = motions[3];
            AnimatorState fall = machine.AddState("Queda");
            fall.motion = motions[4];
            AnimatorState land = machine.AddState("Pouso");
            land.motion = motions[5];
            AnimatorState death = machine.AddState("Morte");
            death.motion = motions[6];
            machine.defaultState = idle;
            Transition(idle, walk, AnimatorConditionMode.Greater, 0.1f, "Speed");
            Transition(walk, idle, AnimatorConditionMode.Less, 0.1f, "Speed");
            foreach (AnimatorState state in new[] { idle, walk })
            {
                Transition(state, jump, AnimatorConditionMode.IfNot, 0, "Grounded")
                    .AddCondition(AnimatorConditionMode.Greater, 0.1f, "VerticalSpeed");
                Transition(state, fall, AnimatorConditionMode.IfNot, 0, "Grounded")
                    .AddCondition(AnimatorConditionMode.Less, 0.1f, "VerticalSpeed");
            }
            Transition(jump, fall, AnimatorConditionMode.Less, 0.1f, "VerticalSpeed");
            Transition(jump, land, AnimatorConditionMode.If, 0, "Grounded");
            Transition(fall, land, AnimatorConditionMode.If, 0, "Grounded");
            Exit(land, idle);
            AnimatorStateTransition die = machine.AddAnyStateTransition(death);
            die.hasExitTime = false;
            die.duration = 0.06f;
            die.canTransitionToSelf = false;
            die.AddCondition(AnimatorConditionMode.IfNot, 0, "Alive");
            AnimatorStateTransition strike = machine.AddAnyStateTransition(attack);
            strike.hasExitTime = false;
            strike.duration = 0.03f;
            strike.canTransitionToSelf = false;
            strike.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            strike.AddCondition(AnimatorConditionMode.If, 0, "Alive");
            Exit(attack, idle).AddCondition(AnimatorConditionMode.If, 0, "Grounded");
            Exit(attack, fall).AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");

        }
        GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (instance == null)
            throw new InvalidOperationException("Não foi possível preparar o prefab Nox.");
        try
        {
            instance.name = "NoxPlayable";
            Animator animator = instance.GetComponent<Animator>();
            if (animator == null)
                animator = instance.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            // A pose inicial vem do repouso explícito, nunca do take padrão do FBX.
            motions[0].SampleAnimation(instance, 0);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Falha ao salvar prefab do Nox.");
            AssetDatabase.SaveAssets();
            return prefab;
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }
    private static AnimationClip PrepareClip(AnimationClip source)
    {
        string name = source.name.Substring(source.name.LastIndexOf('|') + 1);
        string path = "Assets/Characters/Nox/Generated-v7/" + name + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip != null)
            return clip;
        clip = UnityEngine.Object.Instantiate(source);
        clip.name = name;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
            if (binding.type == typeof(Transform) && string.IsNullOrEmpty(binding.path))
                AnimationUtility.SetEditorCurve(clip, binding, null);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float value, string parameter)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = 0.1f;
        transition.AddCondition(mode, value, parameter);
        return transition;
    }
    private static AnimatorStateTransition Exit(AnimatorState from, AnimatorState to)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = true;
        transition.exitTime = 0.95f;
        transition.duration = 0.02f;
        return transition;
    }
}

public class NoxModelImporter : AssetPostprocessor
{
    private void OnPreprocessModel()
    {
        if (assetPath != NoxCharacterAssets.ModelPath)
            return;
        ModelImporter importer = (ModelImporter)assetImporter;
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.importAnimation = true;
        importer.animationCompression = ModelImporterAnimationCompression.Off;
        importer.importCameras = false;
        importer.importLights = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
    }
    private void OnPreprocessAnimation()
    {
        if (assetPath != NoxCharacterAssets.ModelPath)
            return;
        ModelImporter importer = (ModelImporter)assetImporter;
        ConfigureClips(importer);
    }

    public static void ConfigureClips(ModelImporter importer)
    {
        importer.animationType = ModelImporterAnimationType.Generic;
        importer.importAnimation = true;
        importer.animationCompression = ModelImporterAnimationCompression.Off;
        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        foreach (ModelImporterClipAnimation clip in clips)
        {
            bool loop = clip.name.EndsWith("Nox_Idle", StringComparison.Ordinal) ||
                clip.name.EndsWith("Nox_Walk", StringComparison.Ordinal);
            clip.loopTime = loop;
            clip.loopPose = loop;
            clip.wrapMode = loop ? WrapMode.Loop : WrapMode.ClampForever;
            clip.keepOriginalOrientation = true;
            clip.keepOriginalPositionY = true;
            clip.keepOriginalPositionXZ = true;
            clip.lockRootRotation = true;
            clip.lockRootHeightY = true;
            clip.lockRootPositionXZ = true;
        }
        importer.clipAnimations = clips;
    }
}
