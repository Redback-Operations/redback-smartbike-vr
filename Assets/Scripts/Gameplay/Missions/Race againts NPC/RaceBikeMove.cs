using Fusion;
using UnityEngine;

/// <summary>
/// NPC waypoint follower. Same Catmull-Rom spline math as the original, but:
///  1) Converted to a NetworkBehaviour driven from FixedUpdateNetwork so the
///     NPC's position is deterministic/synced instead of every client
///     running its own diverging local simulation (the original ran in
///     Update() with Time.deltaTime and no networking at all - fine for a
///     single local player, broken the moment a second real player joins).
///  2) Its own OnTriggerEnter checkpoint handling was removed - that's now
///     unified in RacerIdentity (this component requires one on the same
///     GameObject).
///  3) Added rubber-banding: speed scales with how far ahead/behind the NPC
///     is versus the best human racer, so races stay close instead of the
///     NPC being a fixed speed the whole time.
///
/// Setup: NPC bike prefab needs NetworkObject + NetworkTransform (for
/// position/rotation replication) + RacerIdentity (isNpc = true) + this
/// script, and must be placed in the scene (not spawned at runtime) so
/// Fusion assigns it Shared-Mode-Master authority automatically.
/// </summary>
[RequireComponent(typeof(RacerIdentity))]
public class RaceBikeMove : NetworkBehaviour
{
    [Header("Path")]
    [Tooltip("Catmull-Rom spline control points - needs at least 4.")]
    public Transform[] waypoints;

    [Header("Speed")]
    public float baseSpeed = 5f;
    public float rotationSpeed = 5f;

    [Header("Rubber-Banding")]
    [Tooltip("How aggressively the NPC speeds up when behind / slows down when ahead of the leading human racer.")]
    [Range(0f, 3f)] public float catchUpStrength = 1.5f;
    public float minSpeedMultiplier = 0.6f;
    public float maxSpeedMultiplier = 1.4f;

    [Networked] private NetworkBool Racing { get; set; }
    [Networked] private int WaypointIndex { get; set; }
    [Networked] private float T { get; set; }

    private RacerIdentity _identity;

    private void Awake()
    {
        _identity = GetComponent<RacerIdentity>();

        if (waypoints == null || waypoints.Length < 4)
            Debug.LogError($"{name}: Catmull-Rom splines require at least 4 waypoints.", this);
    }

    /// <summary>Called by NPCBikeManager once the race countdown finishes. Only the authoritative side actually starts moving.</summary>
    public void StartRacing()
    {
        if (!Object.HasStateAuthority) return;
        Racing = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || !Racing) return;
        if (waypoints == null || waypoints.Length < 4) return;

        var speed = baseSpeed * GetRubberBandMultiplier();

        var newPos = CatmullRomSpline(T);
        transform.position = Vector3.MoveTowards(transform.position, newPos, speed * Runner.DeltaTime);

        var direction = (newPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Runner.DeltaTime);
        }

        var nextIndex = (WaypointIndex + 1) % waypoints.Length;
        var segmentLength = Vector3.Distance(waypoints[WaypointIndex].position, waypoints[nextIndex].position);

        if (segmentLength > 0.001f)
            T += speed * Runner.DeltaTime / segmentLength;

        if (T >= 1f)
        {
            T = 0f;
            WaypointIndex = nextIndex;
        }
    }

    private float GetRubberBandMultiplier()
    {
        if (RaceManager.Instance == null || _identity == null)
            return 1f;

        var leaderProgress = RaceManager.Instance.GetBestHumanProgress();
        if (leaderProgress < 0)
            return 1f; // no human racers registered yet - run at base pace

        var delta = leaderProgress - _identity.GetProgressScore(); // positive: NPC behind, negative: NPC ahead
        var multiplier = 1f + Mathf.Clamp(delta, -3f, 3f) * 0.15f * catchUpStrength;

        return Mathf.Clamp(multiplier, minSpeedMultiplier, maxSpeedMultiplier);
    }

    private Vector3 CatmullRomSpline(float t)
    {
        var p0 = Mathf.Clamp(WaypointIndex - 1, 0, waypoints.Length - 1);
        var p1 = Mathf.Clamp(WaypointIndex, 0, waypoints.Length - 1);
        var p2 = Mathf.Clamp(WaypointIndex + 1, 0, waypoints.Length - 1);
        var p3 = Mathf.Clamp(WaypointIndex + 2, 0, waypoints.Length - 1);

        var P0 = waypoints[p0].position;
        var P1 = waypoints[p1].position;
        var P2 = waypoints[p2].position;
        var P3 = waypoints[p3].position;

        return 0.5f * ((2 * P1) + (-P0 + P2) * t + (2 * P0 - 5 * P1 + 4 * P2 - P3) * t * t + (-P0 + 3 * P1 - 3 * P2 + P3) * t * t * t);
    }
}
