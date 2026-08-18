using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gameplay.Tutorial;
using Nobi.UiRoundedCorners;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds Assets/Scenes/TutorialScene.unity from scratch: ground, lighting,
/// the Player_New rig driven offline, checkpoint gates, the instruction HUD
/// and the XR Device Simulator loader. Idempotent - rebuilding overwrites
/// the existing scene.
/// </summary>
public static class TutorialSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/TutorialScene.unity";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player_New.prefab";
    private const string SimulatorPrefabPath =
        "Assets/Samples/XR Interaction Toolkit/2.5.2/XR Device Simulator/XR Device Simulator.prefab";
    private const string MaterialFolder = "Assets/Materials/Tutorial";

    [MenuItem("Tools/Tutorial/Build Tutorial Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        BuildLighting();
        BuildGround();
        var player = BuildPlayer();
        var gatesRoot = BuildGates();
        var hud = BuildHud();
        BuildManager(hud, gatesRoot);
        BuildSimulatorLoader();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();

        Debug.Log($"TutorialSceneBuilder: scene saved to {ScenePath} (player: {player.name})");
    }

    // Entry point for -executeMethod in batch mode.
    public static void BuildFromBatchMode()
    {
        try
        {
            Build();
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogError($"TutorialSceneBuilder failed: {e}");
            EditorApplication.Exit(1);
        }
    }

    private static void BuildLighting()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        light.color = new Color(1f, 0.96f, 0.88f);
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.skybox = GetOrCreateMaterial("TutorialSkybox", "Skybox/Procedural", mat =>
        {
            mat.SetFloat("_SunSize", 0.04f);
            mat.SetFloat("_Exposure", 1.2f);
        });
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
    }

    private static void BuildGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(30f, 1f, 30f); // 300 x 300 m
        ground.GetComponent<Renderer>().sharedMaterial = GetOrCreateMaterial(
            "TutorialGround", "Universal Render Pipeline/Lit", mat =>
            {
                mat.color = new Color(0.42f, 0.55f, 0.35f);
                mat.SetFloat("_Smoothness", 0.05f);
            });
    }

    private static GameObject BuildPlayer()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (prefab == null)
            throw new InvalidOperationException($"Player prefab not found at {PlayerPrefabPath}");

        var player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        var driver = player.AddComponent<TutorialPlayerDriver>();

        // Copy the objects NetworkPlayer would activate for the local player.
        var networkPlayer = player.GetComponent<NetworkPlayer>();
        if (networkPlayer != null)
        {
            var serialized = new SerializedObject(networkPlayer);
            var localObjectsProp = serialized.FindProperty("localObjects");
            var localObjects = new List<GameObject>();
            for (var i = 0; i < localObjectsProp.arraySize; i++)
            {
                if (localObjectsProp.GetArrayElementAtIndex(i).objectReferenceValue is GameObject go)
                    localObjects.Add(go);
            }
            driver.localObjects = localObjects.ToArray();
        }

        var driverSerialized = new SerializedObject(driver);
        driverSerialized.FindProperty("bikeSelector").objectReferenceValue =
            player.GetComponentInChildren<BikeSelector>(true);
        driverSerialized.FindProperty("saveLoadBike").objectReferenceValue =
            player.GetComponentInChildren<SaveLoadBike>(true);
        driverSerialized.ApplyModifiedPropertiesWithoutUndo();

        return player;
    }

    private static GameObject BuildGates()
    {
        var gatesRoot = new GameObject("Tutorial Gates");

        // Gentle S-curve so the player practices steering both ways.
        var waypoints = new[]
        {
            new Vector3(0f, 0f, 25f),
            new Vector3(7f, 0f, 45f),
            new Vector3(7f, 0f, 70f),
            new Vector3(-5f, 0f, 90f),
            new Vector3(-5f, 0f, 115f),
        };

        var gateMaterial = GetOrCreateMaterial("TutorialGate", "Universal Render Pipeline/Lit", mat =>
        {
            mat.color = new Color(0.95f, 0.65f, 0.15f);
            mat.SetFloat("_Smoothness", 0.3f);
        });

        for (var i = 0; i < waypoints.Length; i++)
        {
            // Face each gate along the direction of travel.
            var forward = i < waypoints.Length - 1
                ? waypoints[i + 1] - waypoints[i]
                : waypoints[i] - waypoints[i - 1];
            forward.y = 0f;

            BuildGate(gatesRoot.transform, i, waypoints[i], Quaternion.LookRotation(forward), gateMaterial);
        }

        return gatesRoot;
    }

    private static void BuildGate(Transform parent, int index, Vector3 position, Quaternion rotation, Material material)
    {
        var gateGo = new GameObject($"Gate {index + 1}");
        gateGo.transform.SetParent(parent);
        gateGo.transform.SetPositionAndRotation(position, rotation);

        var renderers = new List<Renderer>();

        Renderer MakePart(string name, Vector3 localPos, Vector3 size)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(gateGo.transform, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = size;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            var partRenderer = part.GetComponent<Renderer>();
            partRenderer.sharedMaterial = material;
            renderers.Add(partRenderer);
            return partRenderer;
        }

        MakePart("Post Left", new Vector3(-3f, 2f, 0f), new Vector3(0.3f, 4f, 0.3f));
        MakePart("Post Right", new Vector3(3f, 2f, 0f), new Vector3(0.3f, 4f, 0.3f));
        MakePart("Crossbar", new Vector3(0f, 4.15f, 0f), new Vector3(6.3f, 0.3f, 0.3f));

        var trigger = gateGo.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 2f, 0f);
        trigger.size = new Vector3(6f, 4f, 1.5f);

        var gate = gateGo.AddComponent<TutorialGate>();
        gate.Index = index;
        var gateSerialized = new SerializedObject(gate);
        var tintProp = gateSerialized.FindProperty("tintRenderers");
        tintProp.arraySize = renderers.Count;
        for (var i = 0; i < renderers.Count; i++)
            tintProp.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
        gateSerialized.ApplyModifiedPropertiesWithoutUndo();

        // Floating gate number.
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(gateGo.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 4.9f, 0f);
        labelGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // face the approaching player
        var label = labelGo.AddComponent<TextMeshPro>();
        label.text = (index + 1).ToString();
        label.fontSize = 8f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.rectTransform.sizeDelta = new Vector2(2f, 1.2f);
    }

    // Fitness-app palette.
    private static readonly Color CardColor = new Color(0.07f, 0.075f, 0.09f, 0.92f);
    private static readonly Color AccentColor = new Color(0.19f, 0.82f, 0.35f);   // activity green
    private static readonly Color TrackColor = new Color(1f, 1f, 1f, 0.12f);
    private static readonly Color SubtleTextColor = new Color(1f, 1f, 1f, 0.55f);

    private static TutorialHUD BuildHud()
    {
        var hudGo = new GameObject("Tutorial HUD");
        hudGo.transform.position = new Vector3(0f, 1.8f, 3.5f);

        var canvas = hudGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var canvasRect = hudGo.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1000f, 900f);
        canvasRect.localScale = Vector3.one * 0.003f;

        RectTransform MakeRect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            params Type[] components)
        {
            var go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            if (rect == null)
                rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        TextMeshProUGUI MakeText(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size,
            float fontSize, Color color, FontStyles style = FontStyles.Normal)
        {
            var rect = MakeRect(name, parent, anchor, position, size, typeof(TextMeshProUGUI));
            var text = rect.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = color;
            text.fontStyle = style;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        // --- Instruction card (auto-fades after each step announcement) ---
        var cardRect = MakeRect("Instruction Card", hudGo.transform, new Vector2(0.5f, 1f), new Vector2(0f, -210f),
            new Vector2(860f, 400f), typeof(CanvasGroup), typeof(Image), typeof(ImageWithRoundedCorners));
        cardRect.GetComponent<Image>().color = CardColor;
        cardRect.GetComponent<ImageWithRoundedCorners>().radius = 48f;
        var cardGroup = cardRect.GetComponent<CanvasGroup>();

        var title = MakeText("Title", cardRect, new Vector2(0.5f, 1f), new Vector2(0f, -70f),
            new Vector2(780f, 90f), 52f, Color.white, FontStyles.Bold);
        var body = MakeText("Body", cardRect, new Vector2(0.5f, 0f), new Vector2(0f, 155f),
            new Vector2(780f, 250f), 34f, SubtleTextColor);

        // --- Activity ring (persistent, compact, docked low in view) ---
        var knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        var ringRoot = MakeRect("Activity Ring", hudGo.transform, new Vector2(0.5f, 0f), new Vector2(0f, 140f),
            new Vector2(240f, 240f), typeof(CanvasGroup));
        var ringGroup = ringRoot.GetComponent<CanvasGroup>();

        Image MakeCircle(string name, Vector2 size, Color color)
        {
            var rect = MakeRect(name, ringRoot, new Vector2(0.5f, 0.5f), Vector2.zero, size, typeof(Image));
            var image = rect.GetComponent<Image>();
            image.sprite = knobSprite;
            image.color = color;
            return image;
        }

        MakeCircle("Ring Background", new Vector2(240f, 240f), CardColor);

        var track = MakeCircle("Ring Track", new Vector2(210f, 210f), TrackColor);
        track.type = Image.Type.Filled;
        track.fillMethod = Image.FillMethod.Radial360;
        track.fillOrigin = (int)Image.Origin360.Top;
        track.fillAmount = 1f;

        var fill = MakeCircle("Ring Fill", new Vector2(210f, 210f), AccentColor);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Radial360;
        fill.fillOrigin = (int)Image.Origin360.Top;
        fill.fillClockwise = true;
        fill.fillAmount = 0f;

        // Donut hole turns the filled circles into a ring.
        MakeCircle("Ring Hole", new Vector2(160f, 160f), CardColor);

        var value = MakeText("Value", ringRoot, new Vector2(0.5f, 0.5f), new Vector2(0f, 14f),
            new Vector2(150f, 60f), 42f, Color.white, FontStyles.Bold);
        var caption = MakeText("Caption", ringRoot, new Vector2(0.5f, 0.5f), new Vector2(0f, -28f),
            new Vector2(150f, 40f), 20f, SubtleTextColor);

        var hud = hudGo.AddComponent<TutorialHUD>();
        var hudSerialized = new SerializedObject(hud);
        hudSerialized.FindProperty("cardGroup").objectReferenceValue = cardGroup;
        hudSerialized.FindProperty("titleText").objectReferenceValue = title;
        hudSerialized.FindProperty("bodyText").objectReferenceValue = body;
        hudSerialized.FindProperty("ringGroup").objectReferenceValue = ringGroup;
        hudSerialized.FindProperty("ringFill").objectReferenceValue = fill;
        hudSerialized.FindProperty("valueText").objectReferenceValue = value;
        hudSerialized.FindProperty("captionText").objectReferenceValue = caption;
        hudSerialized.ApplyModifiedPropertiesWithoutUndo();

        return hud;
    }

    private static void BuildManager(TutorialHUD hud, GameObject gatesRoot)
    {
        var managerGo = new GameObject("Tutorial Manager");
        var manager = managerGo.AddComponent<TutorialManager>();
        var serialized = new SerializedObject(manager);
        serialized.FindProperty("hud").objectReferenceValue = hud;
        serialized.FindProperty("gatesRoot").objectReferenceValue = gatesRoot;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildSimulatorLoader()
    {
        var loaderGo = new GameObject("XR Device Simulator Loader");
        var loader = loaderGo.AddComponent<XRDeviceSimulatorLoader>();

        var simulatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SimulatorPrefabPath);
        if (simulatorPrefab == null)
        {
            Debug.LogWarning($"TutorialSceneBuilder: XR Device Simulator prefab not found at {SimulatorPrefabPath}");
            return;
        }

        var serialized = new SerializedObject(loader);
        serialized.FindProperty("simulatorPrefab").objectReferenceValue = simulatorPrefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Material GetOrCreateMaterial(string name, string shaderName, Action<Material> configure)
    {
        if (!Directory.Exists(MaterialFolder))
        {
            Directory.CreateDirectory(MaterialFolder);
            AssetDatabase.Refresh();
        }

        var path = $"{MaterialFolder}/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
                throw new InvalidOperationException($"Shader not found: {shaderName}");
            material = new Material(shader);
            configure(material);
            AssetDatabase.CreateAsset(material, path);
        }

        return material;
    }

    private static void AddSceneToBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == ScenePath))
            return;

        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
