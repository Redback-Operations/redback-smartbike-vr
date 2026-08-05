using Fusion;

/// <summary>
/// Shared types for the Race Mission. Split into its own file so both
/// RaceManager and RacerIdentity can reference them without a circular
/// dependency on a single "god" script.
/// </summary>
public enum RaceState
{
    Idle,       // Nobody has crossed the start line yet.
    Countdown,  // "3.. 2.. 1.. GO" - movement should be locked/limited here if you want a proper grid start.
    Racing,     // Race is live, checkpoints/laps are being tracked.
    Finished    // Every registered racer has crossed the line (or the manager force-ended it).
}

/// <summary>
/// One row of the results table. Networked (replicated) so every client -
/// including racers who are still mid-race - sees the same finish order.
/// Kept intentionally small/blittable since it lives inside a NetworkArray.
/// </summary>
public struct RaceResultEntry : INetworkStruct
{
    public NetworkId RacerId;   // NetworkObject.Id of the finishing RacerIdentity (works for players AND NPCs).
    public float FinishTime;    // Seconds, measured from RaceManager.RaceStartSimTime.
    public int Placement;       // 1 = 1st place, 2 = 2nd, etc. 0 = unset.
    public NetworkBool Valid;   // Whether this slot has been written to yet.
}
