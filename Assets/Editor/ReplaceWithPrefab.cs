using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab
{
    [MenuItem("Tools/Replace Selected With Prefab")]
    static void Replace()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Buildings/Residence.prefab");

        foreach (GameObject obj in Selection.gameObjects)
        {
            Transform parent = obj.transform.parent;
            int siblingIndex = obj.transform.GetSiblingIndex();

            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(newObj, "Replace Objects");

            newObj.transform.SetParent(parent);
            newObj.transform.SetSiblingIndex(siblingIndex);

            newObj.transform.position = obj.transform.position;
            newObj.transform.rotation = obj.transform.rotation;
            newObj.transform.localScale = obj.transform.localScale;

            Undo.DestroyObjectImmediate(obj);
        }
    }
}