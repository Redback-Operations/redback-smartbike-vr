using UnityEngine;

public static class Vector3Extensions
{
    /// <summary>
    /// Performs a Bezier linear interpolation between two positions with given control points.
    /// </summary>
    /// <param name="start">The starting position (position a).</param>
    /// <param name="end">The ending position (position b).</param>
    /// <param name="offsetControlPointA">The offset for the first control point relative to the start position.</param>
    /// <param name="offsetControlPointB">The offset for the second control point relative to the end position.</param>
    /// <param name="t">The interpolation factor, where 0 <= t <= 1.</param>
    /// <returns>The interpolated position along the Bezier curve.</returns>
    public static Vector3 BezierLerp(this Vector3 start, Vector3 end, Vector3 offsetControlPointA, Vector3 offsetControlPointB, float t){
        // Calculate control points in world space
        Vector3 controlPointA = start + offsetControlPointA;
        Vector3 controlPointB = end + offsetControlPointB;

        // Interpolate along the curve using the Bezier formula
        float oneMinusT = 1f - t;
        return Mathf.Pow(oneMinusT, 3) * start +
               3f * Mathf.Pow(oneMinusT, 2) * t * controlPointA +
               3f * oneMinusT * Mathf.Pow(t, 2) * controlPointB +
               Mathf.Pow(t, 3) * end;
    }
}