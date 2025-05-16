using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GuideLineManager : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private List<Vector3> guidePoints = new List<Vector3>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// 添加一个路径点，并更新线路
    /// </summary>
    public void AddPoint(Vector3 point)
    {
    point.y += 0.6f; // 可调节：贴在表面，不进模块内部
    guidePoints.Add(point);
    UpdateLine();
    }

    /// <summary>
    /// 添加多个路径点（批量）
    /// </summary>
    public void AddPoints(IEnumerable<Vector3> points)
    {
        guidePoints.AddRange(points);
        UpdateLine();
    }

    /// <summary>
    /// 清空所有路径点
    /// </summary>
    public void ClearPoints()
    {
        guidePoints.Clear();
        UpdateLine();
    }

    /// <summary>
    /// 获取当前路径点数量
    /// </summary>
    public int GetPointCount()
    {
        return guidePoints.Count;
    }

    /// <summary>
    /// 获取指定 index 的点
    /// </summary>
    public Vector3 GetPoint(int index)
    {
        return guidePoints[index];
    }

    /// <summary>
    /// 更新 LineRenderer 展示
    /// </summary>
    private void UpdateLine()
    {
        lineRenderer.positionCount = guidePoints.Count;
        lineRenderer.SetPositions(guidePoints.ToArray());
    }
}