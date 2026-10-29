using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Regressões no motor PhysX real, em cena temporária, sem editar as fases.
public static class NoxPhysicsChecks
{
    [MenuItem("Nox/Verificar física em cena isolada")]
    public static void Run()
    {
        if (Application.isPlaying)
            throw new InvalidOperationException("Encerrar o Play antes da verificação física.");
        Scene scene = SceneManager.CreateScene("Nox-Physics-Regression-" + Guid.NewGuid(),
            new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        NoxGameState previous = NoxSession.State;
        float previousScale = Time.timeScale;
        NoxPlayerMotor motor = null;
        try
        {
            NoxSession.SetState(NoxGameState.Playing);
            PhysicsScene physics = scene.GetPhysicsScene();
            GameObject platform = CreateBox(scene, "Plataforma", Vector3.zero, new Vector3(6, 1, 8));
            Rigidbody supportBody = platform.AddComponent<Rigidbody>();
            supportBody.isKinematic = true;
            supportBody.useGravity = false;
            GameObject target = new GameObject("Destino");
            SceneManager.MoveGameObjectToScene(target, scene);
            target.transform.position = new Vector3(20, 0, 0);
            NoxMovingPlatform route = platform.AddComponent<NoxMovingPlatform>();
            route.waypoints = new[] { target };
            route.speed = 2;
            route.SendMessage("Awake");
            GameObject player = CreateBox(scene, "Jogador", Vector3.up, Vector3.one);
            Rigidbody body = player.AddComponent<Rigidbody>();
            motor = new NoxPlayerMotor(body, player.GetComponent<Collider>());
            Physics.SyncTransforms();
            for (int i = 0; i < 60; i++)
                Step(route, motor, physics, false);
            Check(body.position.x > 1.5f, "O jogador não acompanhou a plataforma horizontal.");
            Check(player.transform.parent == null, "A plataforma adotou o jogador na hierarquia.");
            player.transform.rotation = Quaternion.Euler(0, 45, 0);
            Check((player.transform.lossyScale - Vector3.one).sqrMagnitude < 0.0001f,
                "A escala do jogador mudou ao girar sobre a plataforma.");
            float above = body.position.y;
            Step(route, motor, physics, true);
            Check(!motor.Grounded && body.velocity.y > 1, "O salto não soltou o apoio.");
            for (int i = 0; i < 8; i++)
                Step(route, motor, physics, false);
            Check(body.position.y > above + 0.4f, "O jogador permaneceu preso ao apoio ao saltar.");
            // Novo contato: elevador e inversão de direção, sem herança de Transform.
            body.position = supportBody.position + Vector3.up;
            body.velocity = Vector3.zero;
            Physics.SyncTransforms();
            target.transform.position = supportBody.position + Vector3.up * 4;
            for (int i = 0; i < 35; i++)
                Step(route, motor, physics, false);
            Check(body.position.y > above + 0.7f, "O elevador não carregou o jogador para cima.");
            target.transform.position = supportBody.position - Vector3.up * 4;
            for (int i = 0; i < 35; i++)
                Step(route, motor, physics, false);
            Check(Mathf.Abs(body.position.y - supportBody.position.y - 1) < 0.3f,
                "O jogador perdeu o apoio na descida.");
            Debug.Log("Nox: 7 verificações físicas aprovadas em cena isolada. O percurso real ainda requer Play.");
        }
        finally
        {
            motor?.Dispose();
            EditorSceneManager.CloseScene(scene, true);
            NoxSession.SetState(previous);
            Time.timeScale = previousScale;
        }
    }

    private static void Step(NoxMovingPlatform route, NoxPlayerMotor motor, PhysicsScene physics, bool jump)
    {
        route.SendMessage("FixedUpdate");
        motor.Step(Vector3.zero, 6, jump, 3.5f, ~0);
        physics.Simulate(Time.fixedDeltaTime);
    }
    private static GameObject CreateBox(Scene scene, string name, Vector3 position, Vector3 scale)
    {
        GameObject box = new GameObject(name);
        SceneManager.MoveGameObjectToScene(box, scene);
        box.transform.position = position;
        box.transform.localScale = scale;
        box.AddComponent<BoxCollider>();
        return box;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
