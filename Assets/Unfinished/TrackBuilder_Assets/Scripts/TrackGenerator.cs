using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    public GameObject[] piecePrefabs;       // Straight, Turn_L, Turn_R
    public GameObject crossPrefab;          // Cross prefab
    public Transform startPoint;            // Starting point
    public int numberOfPieces = 20;

    public GuideLineManager guideLine;      // 拖入带 LineRenderer 的组件

    private Vector3 currentPosition;
    private Quaternion forward;
    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

    void Start()
    {
        GenerateTrack();
    }

    void GenerateTrack()
    {
        currentPosition = startPoint.position;
        forward = Quaternion.identity;
        occupied.Clear();

        guideLine.ClearPoints();                   // 清空旧引导线
        guideLine.AddPoint(currentPosition);       // 起点加入路径

        for (int count = 0; count < numberOfPieces; count++)
        {
            GameObject prefab = piecePrefabs[Random.Range(0, piecePrefabs.Length)];
            GameObject obj = Instantiate(prefab);
            TrackPiece piece = obj.GetComponent<TrackPiece>();

            obj.transform.rotation = forward;
            Vector3 offset = obj.transform.TransformPoint(piece.startPoint.localPosition) - obj.transform.position;
            obj.transform.position = currentPosition - offset;

            Vector2Int mid = Round2D(piece.midPoint.position);

            if (occupied.Contains(mid))
            {
                GameObject old = Find(mid);
                if (old != null) Destroy(old);
                Destroy(obj);

                GameObject cross = Instantiate(crossPrefab);
                TrackPiece crossPiece = cross.GetComponent<TrackPiece>();
                cross.transform.rotation = forward;
                Vector3 crossOffset = cross.transform.TransformPoint(crossPiece.startPoint.localPosition) - cross.transform.position;
                cross.transform.position = currentPosition - crossOffset;

                occupied.Add(Round2D(crossPiece.midPoint.position));

                List<(Transform, Quaternion)> exits = new();

                if (!occupied.Contains(Round2D(crossPiece.endPoint.position)))
                    exits.Add((crossPiece.endPoint, forward));
                if (!occupied.Contains(Round2D(crossPiece.endPointC1.position)))
                    exits.Add((crossPiece.endPointC1, Quaternion.Euler(0, -90, 0) * forward));
                if (!occupied.Contains(Round2D(crossPiece.endPointC2.position)))
                    exits.Add((crossPiece.endPointC2, Quaternion.Euler(0, 90, 0) * forward));

                (Transform exit, Quaternion newForward) = exits.Count > 0
                    ? exits[Random.Range(0, exits.Count)]
                    : (crossPiece.endPoint, forward);  // fallback if all blocked

                guideLine.AddPoint(exit.position);    // 添加交叉模块出口点
                currentPosition = exit.position;
                forward = newForward;
                continue;
            }

            occupied.Add(mid);
            guideLine.AddPoint(piece.endPoint.position);  // 正常模块终点加入路径

            currentPosition = piece.endPoint.position;

            if (piece.trackType == TrackType.TurnL)
                forward = Quaternion.Euler(0, -90, 0) * forward;
            else if (piece.trackType == TrackType.TurnR)
                forward = Quaternion.Euler(0, 90, 0) * forward;
        }
    }

    Vector2Int Round2D(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
    }

    GameObject Find(Vector2Int pos)
    {
        GameObject[] all = GameObject.FindGameObjectsWithTag("TrackPiece");
        foreach (GameObject go in all)
        {
            TrackPiece p = go.GetComponent<TrackPiece>();
            if (p != null && Round2D(p.midPoint.position) == pos)
                return go;
        }
        return null;
    }
}