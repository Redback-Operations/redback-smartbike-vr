using UnityEngine;

/// <summary>
/// Enum to represent the type of track piece.
/// </summary>
public enum TrackType 
{ 
    Straight,    // Straight path
    TurnL,       // Left turn
    TurnR,       // Right turn
    Cross        // 4-way intersection
}

/// <summary>
/// Defines key reference points for a modular track piece.
/// This component is attached to each track prefab.
/// </summary>
public class TrackPiece : MonoBehaviour
{
    public Transform startPoint;      // Starting point of this piece
    public Transform endPoint;        // Main end point (used for straight and turns)
    public Transform midPoint;        // Midpoint used for overlap detection
    public Transform endPointC1;      // Secondary exit (left for cross piece)
    public Transform endPointC2;      // Tertiary exit (right for cross piece)
    public TrackType trackType;       // Type of this track piece
}