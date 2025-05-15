using UnityEngine;
using UnityEditor;

public class PhysicalCollisionTool : EditorWindow
{
    [MenuItem("Tools/Physical Collision")]
    public static void ShowWindow()
    {
        GetWindow<PhysicalCollisionTool>("Physical Collision");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add Physical Colliders and Rigidbody", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply to Selected Objects"))
        {
            ApplyPhysics();
        }
    }

    private void ApplyPhysics()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj, "Add Collider and Rigidbody");

            // Add collider if none exists
            if (!obj.TryGetComponent<Collider>(out _))
            {
                Collider col = TryAddBestFitCollider(obj);
                if (col != null)
                    Debug.Log($"Added {col.GetType().Name} to {obj.name}");
            }

            // Add Rigidbody if none exists
            if (!obj.TryGetComponent<Rigidbody>(out _))
            {
                Rigidbody rb = obj.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                Debug.Log($"Added Rigidbody to {obj.name}");
            }

            EditorUtility.SetDirty(obj);
        }
    }

    private Collider TryAddBestFitCollider(GameObject obj)
    {
        // Try mesh-based if mesh exists
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            MeshCollider mc = obj.AddComponent<MeshCollider>();
            mc.convex = true; // Required for Rigidbody interaction
            return mc;
        }

        // Default to box collider
        return obj.AddComponent<BoxCollider>();
    }
}