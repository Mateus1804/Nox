#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Direção visual aplicada no Editor: nenhuma busca ou geração de cenário a cada frame.
public static class NoxSceneArt
{
    private const string Folder = "Assets/Art/Generated-v7";
    private const string RootName = "NoxArt-v7";
    private static Material stone, top, trim, wood, metal, crystal, distant, cloud;
    private static int meshNumber;
    private static int groundMask;
    private static string sceneFolder;

    public static void Apply(Scene scene)
    {
        foreach (GameObject previous in scene.GetRootGameObjects().Where(root => root.name == RootName))
            UnityEngine.Object.DestroyImmediate(previous);
        foreach (Transform previous in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Where(item => item.name == "NoxSurface-v7").ToArray()) UnityEngine.Object.DestroyImmediate(previous.gameObject);
        // V5 permanece como cópia visual desativada, inclusive possíveis ajustes locais.
        foreach (GameObject previous in scene.GetRootGameObjects().Where(root => (root.name == "NoxArt-v5" || root.name == "NoxArt-v6"))) previous.SetActive(false);
        foreach (Transform previous in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Where(item => (item.name == "NoxSurface-v5" || item.name == "NoxSurface-v6"))) previous.gameObject.SetActive(false);
        bool fire = scene.name.StartsWith("Fire", StringComparison.OrdinalIgnoreCase);
        bool menu = scene.name.StartsWith("Menu", StringComparison.OrdinalIgnoreCase);
        Directory.CreateDirectory(Folder);
        AssetDatabase.Refresh();
        sceneFolder = Folder + "/" + scene.name;
        Directory.CreateDirectory(sceneFolder);
        AssetDatabase.Refresh();
        meshNumber = 0;
        string theme = fire ? "Fogo" : "Ar";
        stone = Surface(theme + "-Rocha", fire ? Hex("342F40") : Hex("3B5362"));
        top = Surface(theme + "-Piso", fire ? Hex("64505B") : Hex("89A99C"));
        trim = Surface(theme + "-Detalhe", fire ? Hex("D18B58") : Hex("D4BD84"));
        wood = Surface("Madeira", Hex("795643"));
        metal = Surface("Metal", Hex("384651"));
        crystal = Surface(theme + "-Cristal", fire ? Hex("F1A45B") : Hex("83DDCF"), true);
        distant = Surface(theme + "-Distante", fire ? Hex("493747") : Hex("526F82"));
        cloud = Surface(theme + "-Nuvem", fire ? Hex("826879") : Hex("C3D9D4"));
        GameObject art = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(art, scene);
        groundMask = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (PlayerScript player in root.GetComponentsInChildren<PlayerScript>(true)) groundMask |= player.WhatIsGround.value;
            foreach (PlayerTwo player in root.GetComponentsInChildren<PlayerTwo>(true)) groundMask |= player.WhatIsGround.value;
        }
        Renderer[] renderers = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Renderer>(true)).ToArray();
        Bounds extent = new Bounds(Vector3.zero, Vector3.one * 30);
        bool hasExtent = false;
        int decorated = 0;
        foreach (Renderer renderer in renderers)
        {
            if (!IsPrimitiveSurface(renderer)) continue;
            if (!hasExtent) { extent = renderer.bounds; hasExtent = true; }
            else extent.Encapsulate(renderer.bounds);
            renderer.sharedMaterial = renderer.CompareTag("Box") ? wood : top;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
            decorated++; DressSurface(renderer, fire);
        }
        if (!menu) AddBackdrop(art.transform, extent, fire, renderers.Where(IsPrimitiveSurface).Select(r => r.bounds.center).ToArray());
        SetAtmosphere(scene, art, fire);
        AssetDatabase.SaveAssets();
        Debug.Log("Direção visual aplicada: " + scene.name + "; superfícies: " + decorated + ". Colisões e trajetos preservados.");
    }

    public static bool IsPrimitiveSurface(Renderer renderer)
    {
        if (renderer.tag == "Enemy" || renderer.tag == "Boss" || renderer.tag == "Fogo" ||
            renderer.tag == "Heal" || renderer.tag == "Player") return false;
        MeshFilter filter = renderer.GetComponent<MeshFilter>();
        Collider collider = renderer.GetComponent<Collider>();
        if (filter == null || filter.sharedMesh == null || collider == null || collider.isTrigger) return false;
        string mesh = filter.sharedMesh.name;
        if (mesh != "Cube" && mesh != "Plane") return false;
        // Personagens, inimigos, armas e VFX mantêm seus materiais e geometrias.
        if (renderer.GetComponentInParent<PlayerScript>() != null || renderer.GetComponentInParent<PlayerTwo>() != null ||
            renderer.GetComponentInParent<Animator>() != null) return false;
        if (renderer.CompareTag("Box")) return true;
        for (Transform item = renderer.transform; item != null; item = item.parent)
        {
            string name = item.name.ToLowerInvariant();
            if (name.Contains("platform") || name.Contains("plataform") || name.Contains("plataforma")) return true;
        }
        // O piso não depende do nome do objeto. A máscara de chão é a mesma dos jogadores.
        Bounds b = renderer.bounds;
        return (groundMask & (1 << renderer.gameObject.layer)) != 0 &&
            Vector3.Dot(renderer.transform.up, Vector3.up) > 0.95f &&
            Mathf.Min(b.size.x, b.size.z) >= 1.5f && b.size.y <= Mathf.Min(b.size.x, b.size.z) * 0.8f;
    }

    private static void DressSurface(Renderer renderer, bool fire)
    {
        GameObject group = new GameObject("NoxSurface-v7");
        group.transform.SetParent(renderer.transform, false);
        Bounds b = renderer.GetComponent<MeshFilter>().sharedMesh.bounds;
        if (renderer.CompareTag("Box"))
        {
            MeshObject("Caixa chanfrada", group.transform, SurfaceMesh(b, 0.96f), wood);
            renderer.enabled = false;
            for (int side = -1; side <= 1; side += 2)
            {
                Part(PrimitiveType.Cube, "Cinta", group.transform,
                    b.center + new Vector3(side * b.extents.x * 0.65f, 0, 0),
                    new Vector3(b.size.x * 0.07f, b.size.y * 1.015f, b.size.z * 1.015f), metal);
            }
            return;
        }
        if (Vector3.Dot(renderer.transform.up, Vector3.up) < 0.95f) return;
        Vector3 size = b.size;
        if (size.y > 0.01f)
        {
            MeshObject("Plataforma talhada", group.transform, SurfaceMesh(b, 0.86f), top);
            renderer.enabled = false;
        }
        // Não altera a área de pouso. A silhueta rochosa fica abaixo da face caminhável.
        float depth = Mathf.Max(size.y * 0.65f, Mathf.Min(size.x, size.z) * 0.28f);
        Mesh mesh = IslandMesh(b.center - Vector3.up * (b.extents.y + depth * 0.51f),
            new Vector3(size.x * 0.96f, depth, size.z * 0.96f));
        MeshObject("Base rochosa", group.transform, mesh, stone);
        Part(PrimitiveType.Cube, "Friso", group.transform,
            b.center + Vector3.up * (b.extents.y - Mathf.Max(size.y * 0.10f, 0.005f)),
            new Vector3(size.x * 1.007f, Mathf.Max(size.y * 0.12f, 0.01f), size.z * 1.007f), trim);
        // Peças decorativas pequenas e sem collider deixam o caminho livre.
        if (Mathf.Min(renderer.bounds.size.x, renderer.bounds.size.z) < 3f) return;
        Vector3 world = new Vector3(renderer.bounds.min.x + 0.4f, renderer.bounds.max.y, renderer.bounds.max.z - 0.4f);
        GameObject shard = Part(PrimitiveType.Sphere, "Cristal", null, world + Vector3.up * 0.25f,
            new Vector3(0.18f, 0.5f, 0.18f), crystal);
        shard.transform.SetParent(group.transform, true);
        if (!fire)
        {
            Vector3 anchor = new Vector3(renderer.bounds.max.x - 0.35f, renderer.bounds.max.y, renderer.bounds.max.z - 0.35f);
            Mesh grass = GrassMesh(renderer.transform, anchor);
            MeshObject("Vegetação de borda", group.transform, grass, top);
        }
    }

    private static void AddBackdrop(Transform parent, Bounds bounds, bool fire, Vector3[] surfaces)
    {
        // Duas faixas fora do volume jogável; nenhuma geometria cenográfica tem collider.
        float span = Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.z), 60, 600);
        Camera camera = parent.gameObject.scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Camera>(true)).FirstOrDefault(c => c.CompareTag("MainCamera"));
        Vector3 behind = camera != null ? Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized : Vector3.forward;
        if (behind.sqrMagnitude < 0.5f) behind = Vector3.forward;
        Vector3 across = Vector3.Cross(Vector3.up, behind).normalized;
        float depth = Mathf.Abs(behind.x) * bounds.extents.x + Mathf.Abs(behind.z) * bounds.extents.z;
        for (int i = 0; i < 36; i++)
        {
            float along = (i % 18 / 17f - 0.5f) * span;
            Vector3 position = bounds.center + across * along + behind * (depth + 22 + i / 18 * 30);
            Vector3 nearest = surfaces.Length == 0 ? bounds.center : surfaces.OrderBy(p => Mathf.Abs(Vector3.Dot(p - position, across))).First();
            position.y = nearest.y - 6 - i / 18 * 5 - i % 3 * 2;
            MeshObject("Ilha distante " + i, parent,
                IslandMesh(position, new Vector3(14 + i % 4 * 5, 18 + i % 3 * 7, 12 + i % 5 * 4)), distant);
            if (i % 3 == 0)
            {
                Vector3 basePoint = position + Vector3.up * 10;
                for (int pillar = -1; pillar <= 1; pillar += 2)
                    Part(PrimitiveType.Cylinder, "Ruína", parent, basePoint + Vector3.right * pillar * 3,
                        new Vector3(1.3f, 4, 1.3f), stone);
                Part(PrimitiveType.Cube, "Verga", parent, basePoint + Vector3.up * 4,
                    new Vector3(8, 1.1f, 1.7f), trim);
            }
            if (!fire)
                Part(PrimitiveType.Sphere, "Nuvem distante", parent, position + Vector3.up * (10 + i % 3 * 4),
                    new Vector3(28, 3.5f, 13), cloud);
        }
    }

    private static void SetAtmosphere(Scene scene, GameObject art, bool fire)
    {
        Shader shader = Shader.Find("Nox/Ceu espiritual");
        if (shader == null) throw new InvalidOperationException("Shader do céu ausente. Atualizar também Assets/Art.");
        string path = Folder + (fire ? "/Ceu-Fogo.mat" : "/Ceu-Ar.mat");
        Material sky = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (sky == null)
        {
            sky = new Material(shader);
            sky.SetColor("_Top", fire ? Hex("19162E") : Hex("142E4A"));
            sky.SetColor("_Horizon", fire ? Hex("B77567") : Hex("94BDB9"));
            sky.SetColor("_Bottom", fire ? Hex("433040") : Hex("3C596C"));
            sky.SetColor("_SunColor", fire ? Hex("FFC08A") : Hex("F7DCA4"));
            AssetDatabase.CreateAsset(sky, path);
        }
        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = fire ? Hex("9785A4") : Hex("A9C6DC");
        RenderSettings.ambientEquatorColor = fire ? Hex("77616D") : Hex("839E9D");
        RenderSettings.ambientGroundColor = Hex("343144");
        RenderSettings.reflectionIntensity = 0.35f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fire ? Hex("725160") : Hex("718F9F");
        RenderSettings.fogDensity = 0.0028f;
        Light[] lights = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Light>(true))
            .Where(light => light.type == LightType.Directional && light.isActiveAndEnabled).ToArray();
        Light sun;
        if (lights.Length == 0)
        {
            GameObject lamp = new GameObject("Luz do horizonte"); lamp.transform.SetParent(art.transform, false);
            sun = lamp.AddComponent<Light>(); sun.type = LightType.Directional;
        }
        else sun = lights[0];
        for (int i = 1; i < lights.Length; i++) lights[i].intensity = 0.15f;
        sun.transform.rotation = Quaternion.Euler(38, -32, 0);
        sun.color = fire ? Hex("FFD0AC") : Hex("FFF0D4");
        sun.intensity = 1.05f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.7f;
        RenderSettings.sun = sun;
        foreach (Camera camera in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>(true)))
        {
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, 500);
            Skybox overrideSky = camera.GetComponent<Skybox>();
            if (overrideSky != null) overrideSky.material = sky;
        }
    }

    private static Material Surface(string name, Color color, bool emissive = false)
    {
        string path = Folder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Nox/Superficie mineral");
        if (shader == null) throw new InvalidOperationException("Render pipeline Standard não disponível.");
        material = new Material(shader) { color = color };
        material.SetFloat("_Glossiness", 0.12f);
        if (emissive) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * 0.4f); }
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = name;
        UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
        item.transform.SetParent(parent, false);
        item.transform.localPosition = position;
        item.transform.localScale = scale;
        Renderer renderer = item.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = true;
        return item;
    }

    private static void MeshObject(string name, Transform parent, Mesh mesh, Material material)
    {
        GameObject item = new GameObject(name);
        item.transform.SetParent(parent, false);
        item.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = item.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
    }

    private static Mesh IslandMesh(Vector3 center, Vector3 size)
    {
        string path = sceneFolder + "/Rocha-" + meshNumber++ + ".asset";
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        // Faces independentes conservam o aspecto facetado ao recalcular normais.
        Vector3[] ring = new Vector3[8];
        for (int i = 0; i < ring.Length; i++)
        {
            float angle = i * Mathf.PI / 4;
            ring[i] = center + new Vector3(Mathf.Cos(angle) * size.x / 2, size.y / 2, Mathf.Sin(angle) * size.z / 2);
        }
        Vector3 bottom = center - Vector3.up * size.y / 2;
        for (int i = 0; i < ring.Length; i++)
        {
            int start = vertices.Count;
            vertices.Add(ring[i]); vertices.Add(ring[(i + 1) % ring.Length]); vertices.Add(bottom);
            triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
            start = vertices.Count;
            vertices.Add(center + Vector3.up * size.y / 2); vertices.Add(ring[(i + 1) % ring.Length]); vertices.Add(ring[i]);
            triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
        }
        Mesh mesh = new Mesh { name = "Ilha facetada" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Mesh SurfaceMesh(Bounds bounds, float inset)
    {
        string path = sceneFolder + "/Talha-" + meshNumber++ + ".asset";
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;
        var vertices = new List<Vector3>(); var triangles = new List<int>();
        Vector2[] ring = { new Vector2(-1,-1), new Vector2(-1,0), new Vector2(-1,1), new Vector2(0,1),
            new Vector2(1,1), new Vector2(1,0), new Vector2(1,-1), new Vector2(0,-1) };
        for (int i = 0; i < ring.Length; i++)
        {
            Vector2 current = ring[i], next = ring[(i + 1) % ring.Length];
            Vector3 a = bounds.center + new Vector3(current.x * bounds.extents.x, bounds.extents.y, current.y * bounds.extents.z);
            Vector3 b = bounds.center + new Vector3(next.x * bounds.extents.x, bounds.extents.y, next.y * bounds.extents.z);
            Vector3 c = bounds.center + new Vector3(next.x * bounds.extents.x * inset, -bounds.extents.y, next.y * bounds.extents.z * inset);
            Vector3 d = bounds.center + new Vector3(current.x * bounds.extents.x * inset, -bounds.extents.y, current.y * bounds.extents.z * inset);
            Face(vertices, triangles, bounds.center + Vector3.up * bounds.extents.y, a, b);
            Face(vertices, triangles, a, d, c); Face(vertices, triangles, a, c, b);
            Face(vertices, triangles, bounds.center - Vector3.up * bounds.extents.y, c, d);
        }
        Mesh mesh = new Mesh { name = "Superfície talhada" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path); return mesh;
    }
    private static Mesh GrassMesh(Transform platform, Vector3 world)
    {
        string path = sceneFolder + "/Vegetacao-" + meshNumber++ + ".asset";
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;
        var vertices = new List<Vector3>(); var triangles = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            Vector3 center = world + new Vector3((i - 2) * 0.07f, 0, 0);
            Vector3 a = platform.InverseTransformPoint(center - Vector3.right * 0.04f);
            Vector3 b = platform.InverseTransformPoint(center + Vector3.right * 0.04f);
            Vector3 c = platform.InverseTransformPoint(center + Vector3.up * (0.18f + i % 3 * 0.06f) + Vector3.forward * 0.09f);
            Face(vertices, triangles, a, b, c); Face(vertices, triangles, c, b, a);
        }
        Mesh mesh = new Mesh { name = "Tufo de borda" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path); return mesh;
    }
    private static void Face(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
    {
        int index = vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c);
        triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
    }

    private static Color Hex(string text)
    {
        if (!ColorUtility.TryParseHtmlString("#" + text, out Color color)) throw new ArgumentException(text);
        return color;
    }
}
#endif
