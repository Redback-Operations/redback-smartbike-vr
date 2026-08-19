using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Rigs bikes that were modelled but never set up for the movement controllers.
///
/// Both RealisticBikeController and SimpleBikeController dereference the wheel
/// colliders / wheel transforms on the selected <see cref="Bike"/> every frame.
/// A bike whose fields are empty throws an UnassignedReferenceException per
/// frame. Historically only RoadBikeV5 was ever rigged.
///
/// This tool takes the fully-rigged bike in the prefab as a reference and gives
/// the un-rigged ones the same shape: a FrontWheelCollider / RearWheelCollider
/// sized from each wheel's own renderer bounds, with the reference bike's
/// suspension and friction settings copied across, plus the transform
/// references the controllers need.
///
/// Positions come from the meshes themselves, so the result is close but worth
/// eyeballing in the Scene view (Unity draws WheelCollider gizmos) before
/// committing.
/// </summary>
public static class BikeRigTool
{
    private const string PlayerPrefabPath = "Assets/Prefabs/Player_New.prefab";

    [MenuItem("Tools/Missions/Rig Unrigged Bikes in Player Prefab", false, 40)]
    public static void RigPlayerPrefab() { RigPlayerPrefab(false); }

    [MenuItem("Tools/Missions/Re-rig ALL Bikes in Player Prefab (force)", false, 41)]
    public static void ForceRigPlayerPrefab()
    {
        if (EditorUtility.DisplayDialog("Re-rig all bikes?",
                "This recomputes wheel colliders, mount point and placement for every bike " +
                "except the reference bike, overwriting what is there now.\n\nContinue?",
                "Re-rig", "Cancel"))
            RigPlayerPrefab(true);
    }

    private static void RigPlayerPrefab(bool force)
    {
        var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        if (root == null)
        {
            Debug.LogError("[BikeRig] Could not load " + PlayerPrefabPath);
            return;
        }

        try
        {
            var bikes = root.GetComponentsInChildren<Bike>(true);
            var reference = bikes.FirstOrDefault(IsRigged);

            if (reference == null)
            {
                Debug.LogError("[BikeRig] No fully-rigged bike found to copy settings from. Aborting.");
                return;
            }

            var todo = bikes.Where(b => b != reference && (force || !IsRigged(b))).ToList();
            if (todo.Count == 0)
            {
                Debug.Log("[BikeRig] Every bike in " + PlayerPrefabPath + " is already rigged. Nothing to do.");
                return;
            }

            Debug.Log($"[BikeRig] Reference bike: {reference.name}. Rigging: {string.Join(", ", todo.Select(b => b.name))}");

            bool changed = false;
            foreach (var bike in todo)
                changed |= Rig(bike, reference);

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
                AssetDatabase.Refresh();
                Debug.Log("[BikeRig] Saved " + PlayerPrefabPath +
                          ". Check the wheel collider gizmos in the Scene view before committing.");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static bool IsRigged(Bike b)
    {
        return b.frontWheelCollider != null && b.rearWheelCollider != null &&
               b.frontWheelTransform != null && b.rearWheelTransform != null &&
               b.frontHandlePivot != null && b.pedalTransform != null;
    }

    private static bool Rig(Bike bike, Bike reference)
    {
        Transform bt = bike.transform;

        Transform front = FindChild(bt, "FrontWheel");
        Transform rear = FindChild(bt, "RearWheel");
        Transform pedal = FindChild(bt, "Pedal");
        Transform handle = FindChild(bt, "Handle");

        var missing = new List<string>();
        if (front == null) missing.Add("a FrontWheel mesh");
        if (rear == null) missing.Add("a RearWheel mesh");
        if (pedal == null) missing.Add("a Pedal/Pedalier mesh");
        if (handle == null) missing.Add("a Handle mesh");

        if (missing.Count > 0)
        {
            Debug.LogError($"[BikeRig] Cannot rig '{bike.name}' - it has no {string.Join(", no ", missing)}. " +
                           "Rig this one by hand.", bike);
            return false;
        }

        bike.frontWheelTransform = front;
        bike.rearWheelTransform = rear;
        bike.pedalTransform = pedal;
        bike.frontHandlePivot = handle;

        bike.frontWheelCollider = EnsureWheelCollider(bt, "FrontWheelCollider", front, reference.frontWheelCollider);
        bike.rearWheelCollider = EnsureWheelCollider(bt, "RearWheelCollider", rear, reference.rearWheelCollider);

        bike.mountTf = BuildMountPoint(bike, reference);
        AlignToReference(bike, reference);

        EditorUtility.SetDirty(bike);
        Debug.Log($"[BikeRig] Rigged '{bike.name}': front r={bike.frontWheelCollider.radius:F3}, " +
                  $"rear r={bike.rearWheelCollider.radius:F3}", bike);
        return true;
    }

    /// <summary>
    /// Places the rider mount where the reference bike has it, expressed in the
    /// target bike's own frame: the same fraction along the wheelbase and the
    /// same height above the hub line. Copying the reference's raw local
    /// position does not work, because bike models do not share a forward axis
    /// (RoadBikeV5 lies along local X and is yawed 90 degrees by its root;
    /// MountainBikeV2 and WomenBikeV3 lie along local Z with an unrotated root).
    /// </summary>
    private static Transform BuildMountPoint(Bike bike, Bike reference)
    {
        Transform bt = bike.transform;
        Transform mount = FindChild(bt, "MountPos");
        if (mount == null)
        {
            var go = new GameObject("MountPos");
            mount = go.transform;
            mount.SetParent(bt, false);
        }

        if (reference.mountTf == null || reference.frontWheelCollider == null || reference.rearWheelCollider == null)
            return mount;

        Transform rt = reference.transform;
        Vector3 rFront = rt.InverseTransformPoint(reference.frontWheelCollider.transform.position);
        Vector3 rRear = rt.InverseTransformPoint(reference.rearWheelCollider.transform.position);
        Vector3 rMount = rt.InverseTransformPoint(reference.mountTf.position);
        Vector3 rAxis = rFront - rRear;

        float alongWheelbase = rAxis.sqrMagnitude > 1e-6f
            ? Vector3.Dot(rMount - rRear, rAxis) / rAxis.sqrMagnitude
            : 0.5f;
        float heightAboveHubs = rMount.y - rRear.y;

        Vector3 front = bt.InverseTransformPoint(bike.frontWheelCollider.transform.position);
        Vector3 rear = bt.InverseTransformPoint(bike.rearWheelCollider.transform.position);

        Vector3 local = rear + (front - rear) * alongWheelbase;
        local.y = rear.y + heightAboveHubs;
        mount.localPosition = local;

        // The avatar is parented to this transform, so it must face the way the
        // bike travels: from the rear hub towards the front hub.
        Vector3 forward = front - rear;
        forward.y = 0;
        if (forward.sqrMagnitude > 1e-6f)
            mount.localRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);

        return mount;
    }

    /// <summary>
    /// Slides the bike so its wheelbase midpoint sits where the reference
    /// bike's does. Without this, bikes authored with a different model origin
    /// ride at a different height than the rig expects - the wheel colliders
    /// push the body up until the wheels reach the ground, taking the camera
    /// and avatar with them.
    /// </summary>
    private static void AlignToReference(Bike bike, Bike reference)
    {
        Transform bt = bike.transform;
        Transform rt = reference.transform;

        if (bt.parent == null || bt.parent != rt.parent)
        {
            Debug.LogWarning($"[BikeRig] '{bike.name}' does not share a parent with the reference bike; " +
                             "skipping placement alignment.", bike);
            return;
        }

        Vector3 referenceMid = (reference.frontWheelCollider.transform.position +
                                reference.rearWheelCollider.transform.position) * 0.5f;
        Vector3 bikeMid = (bike.frontWheelCollider.transform.position +
                           bike.rearWheelCollider.transform.position) * 0.5f;

        Vector3 delta = referenceMid - bikeMid;
        if (delta.sqrMagnitude < 1e-8f)
            return;

        bt.position += delta;
        Debug.Log($"[BikeRig] Moved '{bike.name}' by {delta} to line its wheelbase up with {reference.name}.", bike);
    }

    private static WheelCollider EnsureWheelCollider(Transform bikeRoot, string name, Transform wheelMesh, WheelCollider template)
    {
        Transform existing = FindChild(bikeRoot, name);
        WheelCollider wc = existing != null ? existing.GetComponent<WheelCollider>() : null;

        if (wc == null)
        {
            var go = existing != null ? existing.gameObject : new GameObject(name);
            go.transform.SetParent(bikeRoot, false);
            wc = go.GetComponent<WheelCollider>();
            if (wc == null) wc = go.AddComponent<WheelCollider>();
        }

        var renderer = wheelMesh.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Bounds worldBounds = renderer.bounds;
            wc.transform.position = worldBounds.center;
            wc.transform.localRotation = Quaternion.identity;
            wc.transform.localScale = Vector3.one;

            // A wheel's largest half-extent is its radius. Divide out the parent
            // scale because WheelCollider.radius is in the collider's own space.
            float scale = Mathf.Max(Mathf.Abs(wc.transform.lossyScale.x), 1e-4f);
            float maxExtent = Mathf.Max(worldBounds.extents.x, Mathf.Max(worldBounds.extents.y, worldBounds.extents.z));
            wc.radius = maxExtent / scale;
        }

        if (template != null)
        {
            wc.mass = template.mass;
            wc.wheelDampingRate = template.wheelDampingRate;
            wc.suspensionDistance = template.suspensionDistance;
            wc.forceAppPointDistance = template.forceAppPointDistance;
            wc.suspensionSpring = template.suspensionSpring;
            wc.forwardFriction = template.forwardFriction;
            wc.sidewaysFriction = template.sidewaysFriction;
            wc.center = template.center;
        }

        return wc;
    }

    /// <summary>Depth-first search for a descendant whose name contains <paramref name="fragment"/>.</summary>
    private static Transform FindChild(Transform root, string fragment)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t == root) continue;
            if (t.name.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return t;
        }
        return null;
    }
}
