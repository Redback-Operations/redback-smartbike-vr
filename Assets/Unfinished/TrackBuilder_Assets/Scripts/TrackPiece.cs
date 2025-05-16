using UnityEngine;

public enum TrackType { Straight, TurnL, TurnR, Cross }

public class TrackPiece : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Transform midPoint;
    public Transform endPointC1;  // Cross左
    public Transform endPointC2;  // Cross右
    public TrackType trackType;
}