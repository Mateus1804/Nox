using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Executado pelo Unity, separado dos testes puros do CI.
public static class NoxGameplayChecks
{
    [MenuItem("Nox/Verificar visão e ações de interface")]
    public static void Run()
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Encerrar o Play antes da verificação isolada.");
        Scene scene = SceneManager.CreateScene("Nox-Gameplay-" + Guid.NewGuid(), new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        NoxGameState previous = NoxSession.State;
        float previousTimeScale = Time.timeScale;
        NoxPlayerController previousPlayer = NoxPlayerController.Active;
        NoxUiProbe receiver = ScriptableObject.CreateInstance<NoxUiProbe>();
        try
        {
            GameObject observer = Box(scene, "Observador", Vector3.zero);
            GameObject target = Box(scene, "Alvo", Vector3.forward * 5);
            Physics.SyncTransforms();
            Require(NoxPerception.CanSee(observer.transform, target.transform), "Alvo livre não foi visto.");
            GameObject wall = Box(scene, "Parede", Vector3.forward * 2.5f);
            Physics.SyncTransforms();
            Require(!NoxPerception.CanSee(observer.transform, target.transform), "A visão atravessou uma parede.");
            wall.SetActive(false);
            Physics.SyncTransforms();
            Require(NoxPerception.CanSee(observer.transform, target.transform), "A remoção do obstáculo não liberou a visão.");
            GameObject control = new GameObject("Controle", typeof(RectTransform), typeof(Text), typeof(Button), typeof(EventTrigger));
            SceneManager.MoveGameObjectToScene(control, scene);
            EventTrigger trigger = control.GetComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            UnityEventTools.AddVoidPersistentListener(entry.callback, receiver.Count);
            entry.callback.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
            trigger.triggers.Add(entry);
            Button button = control.GetComponent<Button>();
            NoxScenePolish.KeyboardAction(button);
            Require(button.targetGraphic is Image && button.targetGraphic.gameObject != control,
                "O botão com texto não recebeu uma área de clique independente.");
            Require(button.onClick.GetPersistentEventCount() == 1, "A ação do mouse não foi migrada para o botão.");
            button.onClick.Invoke();
            entry.callback.Invoke(null);
            Require(receiver.Calls == 1, "O mesmo clique executou duas ações.");
            NoxScenePolish.KeyboardAction(button);
            button.onClick.Invoke();
            Require(receiver.Calls == 2 && button.onClick.GetPersistentEventCount() == 1, "Reaplicar a preparação duplicou callbacks.");
            NoxSession.SetState(NoxGameState.Playing);
            GameObject playerObject = Box(scene, "Jogador", Vector3.right * 20);
            NoxPlayerController player = playerObject.AddComponent<NoxPlayerController>();
            player.hp = 3;
            GameObject slash = Box(scene, "Golpe", Vector3.right * 25);
            slash.tag = "Slash";
            NoxAttackHitbox hitbox = slash.AddComponent<NoxAttackHitbox>();
            hitbox.Configure(playerObject.transform, 1);
            NoxEnemyHealth enemy = target.AddComponent<NoxEnemyHealth>();
            enemy.health = 3;
            Collider source = slash.GetComponent<Collider>();
            Require(NoxAttackHitbox.Accept(source, enemy), "Golpe ativo não atingiu o alvo.");
            Require(!NoxAttackHitbox.Accept(source, enemy), "Golpe repetiu dano no mesmo alvo.");
            hitbox.Configure(playerObject.transform, 0);
            Require(!NoxAttackHitbox.Accept(source, observer.transform), "Golpe expirado atingiu outro alvo.");
            hitbox.Configure(playerObject.transform, 1);
            player.hp = 0;
            Require(!NoxAttackHitbox.Accept(source, observer.transform), "Jogador morto manteve o golpe ativo.");
            player.hp = 3;
            NoxSession.SetState(NoxGameState.Paused);
            Require(!NoxAttackHitbox.Accept(source, observer.transform), "Golpe causou dano durante pausa.");
            Debug.Log("Nox: 12 verificações de visão e ações de interface aprovadas. Não substituem o percurso no Play.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(receiver);
            EditorSceneManager.CloseScene(scene, true);
            NoxPlayerController.Active = previousPlayer;
            NoxSession.SetState(previous);
            Time.timeScale = previousTimeScale;
        }
    }

    [MenuItem("Nox/Capturar Play atual")]
    public static void CapturePlay()
    {
        if (!Application.isPlaying)
            throw new InvalidOperationException("Iniciar o Play para capturar o jogo real.");
        string folder = Path.GetFullPath(Path.Combine("Logs", "Capturas-v6"));
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, SceneManager.GetActiveScene().name + "-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".png");
        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Captura solicitada para o próximo frame: " + path + ". Conferir o arquivo após a gravação.");
    }

    private static GameObject Box(Scene scene, string name, Vector3 position)
    {
        GameObject item = new GameObject(name);
        SceneManager.MoveGameObjectToScene(item, scene);
        item.transform.position = position;
        item.AddComponent<BoxCollider>();
        return item;
    }
    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}

public sealed class NoxUiProbe : ScriptableObject
{
    public int Calls;
    public void Count()
    {
        Calls++;
    }
}
