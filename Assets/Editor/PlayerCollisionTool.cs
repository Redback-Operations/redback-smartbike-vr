using UnityEngine;
using UnityEditor;
using System.Linq;

public class PlayerCollisionTool : EditorWindow
{
    [MenuItem("Tools/Player Collision")]
    public static void ShowWindow()
    {
        GetWindow<PlayerCollisionTool>("Player Collision");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add 'Collision' Tag to Selected Objects (including children)", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply Tag"))
        {
            ApplyCollisionTag();
        }
    }

    private void ApplyCollisionTag()
    {
        string tagName = "Collision";

        // Ensure tag exists
        if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(tagName))
        {
            AddTag(tagName);
        }

        int count = 0;

        foreach (GameObject obj in Selection.gameObjects)
        {
            count += ApplyTagRecursively(obj, tagName);
        }

        Debug.Log($"Applied '{tagName}' tag to {count} object(s).");
    }

    // Recursively apply tag to GameObject and its children
    private int ApplyTagRecursively(GameObject obj, string tagName)
    {
        int applied = 0;
        Undo.RecordObject(obj, "Set Collision Tag");
        obj.tag = tagName;
        EditorUtility.SetDirty(obj);
        applied++;

        foreach (Transform child in obj.transform)
        {
            applied += ApplyTagRecursively(child.gameObject, tagName);
        }

        return applied;
    }

    private void AddTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
                return;
        }

        tagsProp.InsertArrayElementAtIndex(0);
        tagsProp.GetArrayElementAtIndex(0).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"Created new tag: '{tagName}'");
    }
}