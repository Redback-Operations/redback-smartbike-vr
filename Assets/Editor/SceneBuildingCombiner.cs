using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SceneBuildingCombiner : EditorWindow
{
    // List of GameObjects to be combined
    private List<GameObject> objectsToCombine = new List<GameObject>();

    // Scroll position for the scroll view
    private Vector2 scrollPos;

    // Option to hide original objects after combination
    private bool hideOriginals = true;

    [MenuItem("Tools/Combine Buildings In Scene")]
    public static void ShowWindow()
    {
        // Open the custom editor window
        GetWindow<SceneBuildingCombiner>("Building Combiner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Drag Scene Buildings Here", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Scroll view to show the list of objects
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
        for (int i = 0; i < objectsToCombine.Count; i++)
        {
            // Display each GameObject as an editable object field
            objectsToCombine[i] = (GameObject)EditorGUILayout.ObjectField(objectsToCombine[i], typeof(GameObject), true);
        }
        EditorGUILayout.EndScrollView();

        // Add currently selected scene objects to the list
        if (GUILayout.Button("Add Selected Objects"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                if (!objectsToCombine.Contains(obj))
                    objectsToCombine.Add(obj);
            }
        }

        // Clear the entire list
        if (GUILayout.Button("Clear List"))
        {
            objectsToCombine.Clear();
        }

        // Toggle to choose whether to hide original objects after merge
        hideOriginals = EditorGUILayout.Toggle("Hide Original Objects", hideOriginals);

        GUILayout.Space(10);
        // Trigger the combine process
        if (GUILayout.Button("Combine Now"))
        {
            Combine();
        }
    }

    private void Combine()
    {
        if (objectsToCombine.Count == 0)
        {
            Debug.LogWarning("No objects to combine.");
            return;
        }

        // Ensure a folder exists to store the combined mesh assets
        string folderPath = "Assets/CombinedMeshes";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }

        // Dictionaries to organize submeshes by material
        List<Material> materialList = new List<Material>();
        Dictionary<Material, List<CombineInstance>> combineDict = new Dictionary<Material, List<CombineInstance>>();

        foreach (GameObject go in objectsToCombine)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            MeshRenderer mr = go.GetComponent<MeshRenderer>();

            if (mf == null || mr == null)
                continue;

            Mesh mesh = mf.sharedMesh;
            if (mesh == null)
                continue;

            Material[] materials = mr.sharedMaterials;

            // For each submesh/material pair
            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                if (sub >= materials.Length) continue;

                Material mat = materials[sub];

                // Store each submesh under its material
                if (!combineDict.ContainsKey(mat))
                {
                    combineDict[mat] = new List<CombineInstance>();
                    materialList.Add(mat);
                }

                CombineInstance ci = new CombineInstance();
                ci.mesh = mesh;
                ci.subMeshIndex = sub;
                ci.transform = mf.transform.localToWorldMatrix;
                combineDict[mat].Add(ci);
            }

            // Optionally hide original objects
            if (hideOriginals)
            {
                Undo.RecordObject(go, "Hide Original");
                go.SetActive(false);
            }
        }

        // Combine submeshes for each material into temporary meshes
        List<CombineInstance> finalCombinations = new List<CombineInstance>();
        int matIndex = 0;
        foreach (var kvp in combineDict)
        {
            CombineInstance[] arr = kvp.Value.ToArray();
            Mesh subMesh = new Mesh();
            subMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            subMesh.CombineMeshes(arr, true, true);

            // Save each submesh as asset
            string tempPath = $"{folderPath}/temp_submesh_{matIndex}.asset";
            AssetDatabase.CreateAsset(subMesh, tempPath);
            AssetDatabase.SaveAssets();

            CombineInstance final = new CombineInstance();
            final.mesh = subMesh;
            final.subMeshIndex = 0;
            final.transform = Matrix4x4.identity;
            finalCombinations.Add(final);

            matIndex++;
        }

        // Combine all submeshes into one mesh with multiple submeshes
        Mesh combined = new Mesh();
        combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combined.CombineMeshes(finalCombinations.ToArray(), false, false);

        // Save the final combined mesh
        string finalPath = $"{folderPath}/Combined_{System.DateTime.Now:yyyyMMdd_HHmmss}.asset";
        AssetDatabase.CreateAsset(combined, finalPath);
        AssetDatabase.SaveAssets();

        // Create new GameObject to hold combined mesh
        GameObject result = new GameObject("Combined_Buildings");
        Undo.RegisterCreatedObjectUndo(result, "Create Combined");

        var mfResult = result.AddComponent<MeshFilter>();
        var mrResult = result.AddComponent<MeshRenderer>();
        mfResult.sharedMesh = combined;
        mrResult.sharedMaterials = materialList.ToArray();

        Selection.activeGameObject = result;
        Debug.Log("✅ Mesh merge complete. Saved to: " + finalPath);
    }
}