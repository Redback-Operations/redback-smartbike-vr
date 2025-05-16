using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GuideLineManager : MonoBehaviour
{
    // Reference to the LineRenderer component used to draw the guide line
    private LineRenderer lineRenderer;

    // List of points that define the guide path
    private List<Vector3> guidePoints = new List<Vector3>();

    // Called when the script instance is being loaded
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    // Add a single point to the guide path and update the line
    public void AddPoint(Vector3 point)
    {
        point.y += 0.6f; // Offset upward to keep the line above the track surface
        guidePoints.Add(point);
        UpdateLine();
    }

    // Add multiple points to the guide path at once
    public void AddPoints(IEnumerable<Vector3> points)
    {
        guidePoints.AddRange(points);
        UpdateLine();
    }

    // Clear all guide points from the path
    public void ClearPoints()
    {
        guidePoints.Clear();
        UpdateLine();
    }

    // Get the total number of guide points
    public int GetPointCount()
    {
        return guidePoints.Count;
    }

    // Get the point at the specified index
    public Vector3 GetPoint(int index)
    {
        return guidePoints[index];
    }

    // Update the LineRenderer to match the current guide points
    private void UpdateLine()
    {
        lineRenderer.positionCount = guidePoints.Count;
        lineRenderer.SetPositions(guidePoints.ToArray());
    }
}