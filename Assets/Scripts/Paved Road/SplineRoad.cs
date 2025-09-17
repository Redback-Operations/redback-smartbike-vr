using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace Paved_Road
{
    
    /// <summary>
    /// A component for creating a road mesh from a Spline at runtime.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    [AddComponentMenu("Splines/Spline Road")]
    public class SplineRoad : MonoBehaviour
    {
        [SerializeField, Tooltip("The Spline that defines the centerline of the road.")]
        SplineContainer m_Container;

        [SerializeField, Tooltip("Enable to regenerate the road mesh when the target Spline is modified.")]
        bool m_RebuildOnSplineChange;

        [SerializeField, Tooltip("Maximum number of times per second the mesh can rebuild.")]
        int m_RebuildFrequency = 15;

        [SerializeField, Tooltip("Automatically update any Mesh, Box, or MeshCollider components when the mesh is extruded.")]
#pragma warning disable 414
        bool m_UpdateColliders = true;
#pragma warning restore 414

        [SerializeField, Tooltip("The width of the road.")]
        float m_RoadWidth = 4f;

        [SerializeField, Tooltip("The number of edge loops per unit length of the road.")]
        float m_SegmentsPerUnit = 2;

        [SerializeField, Tooltip("If true, fills the start and end of the road mesh.")]
        bool m_Capped = false;

        [SerializeField, Tooltip("The section of the Spline to generate the road on.")]
        Vector2 m_Range = new Vector2(0f, 1f);

        Mesh m_Mesh;
        bool m_RebuildRequested;
        float m_NextScheduledRebuild;

        public SplineContainer Container
        {
            get => m_Container;
            set => m_Container = value;
        }

        public bool RebuildOnSplineChange
        {
            get => m_RebuildOnSplineChange;
            set => m_RebuildOnSplineChange = value;
        }

        public int RebuildFrequency
        {
            get => m_RebuildFrequency;
            set => m_RebuildFrequency = Mathf.Max(value, 1);
        }

        public float RoadWidth
        {
            get => m_RoadWidth;
            set => m_RoadWidth = Mathf.Max(value, 0.1f);
        }

        public float SegmentsPerUnit
        {
            get => m_SegmentsPerUnit;
            set => m_SegmentsPerUnit = Mathf.Max(value, 0.01f);
        }

        public bool Capped
        {
            get => m_Capped;
            set => m_Capped = value;
        }

        public Vector2 Range
        {
            get => m_Range;
            set => m_Range = new Vector2(Mathf.Min(value.x, value.y), Mathf.Max(value.x, value.y));
        }

        public Spline Spline => m_Container?.Spline;
        public IReadOnlyList<Spline> Splines => m_Container?.Splines;

        internal void Reset()
        {
            TryGetComponent(out m_Container);

            if (TryGetComponent<MeshFilter>(out var filter))
                filter.sharedMesh = m_Mesh = CreateMeshAsset();

            if (TryGetComponent<MeshRenderer>(out var renderer) && renderer.sharedMaterial == null)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var mat = cube.GetComponent<MeshRenderer>().sharedMaterial;
                DestroyImmediate(cube);
                renderer.sharedMaterial = mat;
            }

            Rebuild();
        }

        void Start()
        {
            if (m_Container == null || m_Container.Spline == null)
            {
                Debug.LogError("Spline Road does not have a valid SplineContainer set.");
                return;
            }

            if ((m_Mesh = GetComponent<MeshFilter>().sharedMesh) == null)
                Debug.LogError("SplineRoad mesh is missing. Please assign or create a writable mesh asset.");

            Rebuild();
        }

        void OnEnable()
        {
            Spline.Changed += OnSplineChanged;
        }

        void OnDisable()
        {
            Spline.Changed -= OnSplineChanged;
        }

        void OnSplineChanged(Spline spline, int knotIndex, SplineModification modificationType)
        {
            if (m_Container != null && Splines.Contains(spline) && m_RebuildOnSplineChange)
                m_RebuildRequested = true;
        }

        void Update()
        {
            if (m_RebuildRequested && Time.time >= m_NextScheduledRebuild)
                Rebuild();
        }

        /// <summary>
        /// Builds or updates the road mesh along the spline.
        /// </summary>
        public void Rebuild()
        {
            if ((m_Mesh = GetComponent<MeshFilter>().sharedMesh) == null)
                return;

            // Here you would call a custom road extrusion function.
            // You can implement this similarly to SplineMesh.Extrude but generate a flat mesh strip.
            SplineMesh.ExtrudeRoad(Splines, m_Mesh, m_RoadWidth, m_SegmentsPerUnit, m_Capped, m_Range);

            m_NextScheduledRebuild = Time.time + 1f / m_RebuildFrequency;

#if UNITY_PHYSICS_MODULE
            if (m_UpdateColliders)
            {
                if (TryGetComponent<MeshCollider>(out var meshCollider))
                    meshCollider.sharedMesh = m_Mesh;

                if (TryGetComponent<BoxCollider>(out var boxCollider))
                {
                    boxCollider.center = m_Mesh.bounds.center;
                    boxCollider.size = m_Mesh.bounds.size;
                }
            }
#endif
        }

        void OnValidate()
        {
            Rebuild();
        }

        internal Mesh CreateMeshAsset()
        {
            var mesh = new Mesh();
            mesh.name = name + "_Road";

#if UNITY_EDITOR
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var sceneDataDir = "Assets";

            if (!string.IsNullOrEmpty(scene.path))
            {
                var dir = Path.GetDirectoryName(scene.path);
                sceneDataDir = $"{dir}/{Path.GetFileNameWithoutExtension(scene.path)}";
                if (!Directory.Exists(sceneDataDir))
                    Directory.CreateDirectory(sceneDataDir);
            }

            var path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{sceneDataDir}/SplineRoad_{mesh.name}.asset");
            UnityEditor.AssetDatabase.CreateAsset(mesh, path);
            UnityEditor.EditorGUIUtility.PingObject(mesh);
#endif
            return mesh;
        }
    }
}