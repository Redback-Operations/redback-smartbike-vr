using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    public GameObject[] piecePrefabs;       // Track piece prefabs: Straight, Turn_L, Turn_R
    public GameObject crossPrefab;          // Prefab for cross intersection
    public Transform startPoint;            // Starting point of the track
    public int numberOfPieces = 20;         // Total number of pieces to generate

    public GuideLineManager guideLine;      // Reference to the component with LineRenderer

    private Vector3 currentPosition;        // Current generation position
    private Quaternion forward;             // Current forward direction
    private HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(); // Used grid positions

    void Start()
    {
        GenerateTrack();
    }

    // Main function to generate the track sequence
    void GenerateTrack()
    {
        currentPosition = startPoint.position;
        forward = Quaternion.identity;
        occupied.Clear();

        guideLine.ClearPoints();                   // Clear old guide line
        guideLine.AddPoint(currentPosition);       // Add start position to the guide line

        for (int count = 0; count < numberOfPieces; count++)
        {
            GameObject prefab = piecePrefabs[Random.Range(0, piecePrefabs.Length)];
            GameObject obj = Instantiate(prefab);
            TrackPiece piece = obj.GetComponent<TrackPiece>();

            obj.transform.rotation = forward;

            // Align the new piece's start point with the current position
            Vector3 offset = obj.transform.TransformPoint(piece.startPoint.localPosition) - obj.transform.position;
            obj.transform.position = currentPosition - offset;

            Vector2Int mid = Round2D(piece.midPoint.position);

            // If the new piece overlaps an occupied tile
            if (occupied.Contains(mid))
            {
                GameObject old = Find(mid);
                if (old != null) Destroy(old);
                Destroy(obj);

                // Replace with a cross piece
                GameObject cross = Instantiate(crossPrefab);
                TrackPiece crossPiece = cross.GetComponent<TrackPiece>();
                cross.transform.rotation = forward;

                Vector3 crossOffset = cross.transform.TransformPoint(crossPiece.startPoint.localPosition) - cross.transform.position;
                cross.transform.position = currentPosition - crossOffset;

                occupied.Add(Round2D(crossPiece.midPoint.position));

                // Collect valid exit points from the cross piece
                List<(Transform, Quaternion)> exits = new();

                if (!occupied.Contains(Round2D(crossPiece.endPoint.position)))
                    exits.Add((crossPiece.endPoint, forward));
                if (!occupied.Contains(Round2D(crossPiece.endPointC1.position)))
                    exits.Add((crossPiece.endPointC1, Quaternion.Euler(0, -90, 0) * forward));
                if (!occupied.Contains(Round2D(crossPiece.endPointC2.position)))
                    exits.Add((crossPiece.endPointC2, Quaternion.Euler(0, 90, 0) * forward));

                // Randomly pick a valid exit, or fallback if all are blocked
                (Transform exit, Quaternion newForward) = exits.Count > 0
                    ? exits[Random.Range(0, exits.Count)]
                    : (crossPiece.endPoint, forward);

                guideLine.AddPoint(exit.position);    // Add exit position to the guide line
                currentPosition = exit.position;
                forward = newForward;
                continue;
            }

            // No conflict, proceed with normal placement
            occupied.Add(mid);
            guideLine.AddPoint(piece.endPoint.position);  // Add end point to the guide line

            currentPosition = piece.endPoint.position;

            // Update direction if the piece is a turn
            if (piece.trackType == TrackType.TurnL)
                forward = Quaternion.Euler(0, -90, 0) * forward;
            else if (piece.trackType == TrackType.TurnR)
                forward = Quaternion.Euler(0, 90, 0) * forward;
        }
    }

    // Converts a Vector3 position to grid-aligned Vector2Int for conflict detection
    Vector2Int Round2D(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
    }

    // Finds an existing track piece occupying a specific grid position
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