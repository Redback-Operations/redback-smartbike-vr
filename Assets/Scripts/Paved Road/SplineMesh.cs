using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Splines
{
    public static class SplineMesh
    {
        /// <summary>
        /// Generates a flat road mesh along one or more splines.
        /// </summary>
        /// <param name="splines">Splines to extrude along.</param>
        /// <param name="mesh">Mesh to write to.</param>
        /// <param name="roadWidth">Total width of the road.</param>
        /// <param name="segmentsPerUnit">Number of subdivisions per unit length.</param>
        /// <param name="capped">Whether to close off the ends with quads.</param>
        /// <param name="range">Normalized start/end range along spline [0-1].</param>
        public static void ExtrudeRoad(IReadOnlyList<Spline> splines, Mesh mesh,
            float roadWidth, float segmentsPerUnit, bool capped, Vector2 range)
        {
            if (splines == null || splines.Count == 0)
                return;

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            foreach (var spline in splines)
            {
                if (spline == null || spline.Count < 2)
                    continue;

                float length = spline.GetLength();
                int steps = Mathf.Max(2, Mathf.CeilToInt(length * segmentsPerUnit));
                float halfWidth = roadWidth * 0.5f;

                Vector3 prevLeft = Vector3.zero;
                Vector3 prevRight = Vector3.zero;
                int baseIndex = vertices.Count;

                for (int i = 0; i < steps; i++)
                {
                    float t = Mathf.Lerp(range.x, range.y, (float)i / (steps - 1));
                    Vector3 position = spline.EvaluatePosition(t);
                    Vector3 tangent = Vector3.Normalize(spline.EvaluateTangent(t));
                    Vector3 normal = Vector3.up; // Road faces upward by default
                    Vector3 binormal = Vector3.Cross(normal, tangent).normalized;

                    Vector3 left = position - binormal * halfWidth;
                    Vector3 right = position + binormal * halfWidth;

                    vertices.Add(left);
                    vertices.Add(right);

                    normals.Add(normal);
                    normals.Add(normal);

                    float v = (t - range.x) / (range.y - range.x);
                    uvs.Add(new Vector2(0, v * length));
                    uvs.Add(new Vector2(1, v * length));

                    if (i > 0)
                    {
                        // Build two triangles for each quad
                        int i0 = baseIndex + (i - 1) * 2;
                        int i1 = i0 + 1;
                        int i2 = baseIndex + i * 2;
                        int i3 = i2 + 1;

                        triangles.Add(i0);
                        triangles.Add(i2);
                        triangles.Add(i1);

                        triangles.Add(i1);
                        triangles.Add(i2);
                        triangles.Add(i3);
                    }

                    prevLeft = left;
                    prevRight = right;
                }

                if (capped)
                {
                    // Cap start
                    int firstLeft = baseIndex;
                    int firstRight = baseIndex + 1;
                    int lastLeft = vertices.Count - 2;
                    int lastRight = vertices.Count - 1;

                    Vector3 capNormal = Vector3.Normalize(-spline.EvaluateTangent(range.x));

                    // start cap
                    int capStart = vertices.Count;
                    vertices.Add(vertices[firstLeft]);
                    vertices.Add(vertices[firstRight]);
                    normals.Add(capNormal);
                    normals.Add(capNormal);
                    uvs.Add(Vector2.zero);
                    uvs.Add(Vector2.right);
                    triangles.Add(capStart);
                    triangles.Add(capStart + 1);
                    triangles.Add(firstRight);
                    triangles.Add(capStart);
                    triangles.Add(firstRight);
                    triangles.Add(firstLeft);

                    // end cap
                    capNormal = Vector3.Normalize(spline.EvaluateTangent(range.y));
                    int capEnd = vertices.Count;
                    vertices.Add(vertices[lastLeft]);
                    vertices.Add(vertices[lastRight]);
                    normals.Add(capNormal);
                    normals.Add(capNormal);
                    uvs.Add(Vector2.zero);
                    uvs.Add(Vector2.right);
                    triangles.Add(lastLeft);
                    triangles.Add(lastRight);
                    triangles.Add(capEnd + 1);
                    triangles.Add(lastLeft);
                    triangles.Add(capEnd + 1);
                    triangles.Add(capEnd);
                }
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }
    }
}
