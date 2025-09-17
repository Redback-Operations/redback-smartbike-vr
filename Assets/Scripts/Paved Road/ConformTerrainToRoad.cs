using UnityEditor;
using UnityEngine;

namespace Paved_Road
{
    using UnityEngine;

    [ExecuteInEditMode] // Allows running in editor
    public class ConformTerrainToRoad : MonoBehaviour
    {
        public Terrain terrain;
        public float extraWidth = 2f; // Extra margin on each side
        public float smoothness = 3f; // Higher = smoother edges

        public void Conform()
        {
            if (terrain == null) return;

            TerrainData data = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;
            int res = data.heightmapResolution;
            float[,] heights = data.GetHeights(0, 0, res, res);

            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) return;
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) return;

            foreach (Vector3 vertex in mesh.vertices)
            {
                Vector3 worldPos = transform.TransformPoint(vertex);
                Vector3 normalizedPos = new Vector3(
                    (worldPos.x - terrainPos.x) / data.size.x,
                    0,
                    (worldPos.z - terrainPos.z) / data.size.z
                );

                int x = Mathf.RoundToInt(normalizedPos.x * (res - 1));
                int z = Mathf.RoundToInt(normalizedPos.z * (res - 1));

                // Raise/lower terrain height to match mesh vertex.y
                float targetHeight = (worldPos.y - terrainPos.y) / data.size.y;

                for (int i = -Mathf.RoundToInt(extraWidth); i <= Mathf.RoundToInt(extraWidth); i++)
                {
                    for (int j = -Mathf.RoundToInt(extraWidth); j <= Mathf.RoundToInt(extraWidth); j++)
                    {
                        int xi = Mathf.Clamp(x + i, 0, res - 1);
                        int zj = Mathf.Clamp(z + j, 0, res - 1);
                        float t = Mathf.Clamp01(1 - (Mathf.Sqrt(i * i + j * j) / extraWidth));
                        heights[zj, xi] = Mathf.Lerp(heights[zj, xi], targetHeight, t * smoothness * 0.1f);
                    }
                }
            }

            data.SetHeights(0, 0, heights);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(ConformTerrainToRoad))]
    public class ConformTerrainToRoadEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ConformTerrainToRoad script = (ConformTerrainToRoad)target;
            if (GUILayout.Button("Conform Terrain"))
            {
                script.Conform();
            }
        }
    }
#endif
    
}
