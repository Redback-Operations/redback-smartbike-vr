using UnityEngine;

/// <summary>
/// Marker placed on every checkpoint trigger collider. RaceManager assigns
/// the Index automatically at Awake() based on the order of its
/// `checkpoints` array, so nothing needs to be configured on the checkpoint
/// object itself beyond the "Checkpoint" tag (unchanged from before).
/// </summary>
[RequireComponent(typeof(Collider))]
public class RaceCheckpoint : MonoBehaviour
{
    public int Index { get; internal set; } = -1;
}
