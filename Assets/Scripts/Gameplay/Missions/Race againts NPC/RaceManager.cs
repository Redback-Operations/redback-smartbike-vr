using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

/// <summary>
/// Replaces CheckpointManager.cs. Old version was a plain MonoBehaviour
/// singleton with a hard-coded 2-racer (player vs. one NPC) bool[] pair and
/// a single shared `currentCheckpointIndex` - which meant a race with more
/// than one real racer would corrupt itself (whichever racer reached a
/// checkpoint LAST would "win" the shared index race). It also never
/// touched the network at all, so in a Fusion session every client would
/// have run its own disconnected copy of the race with no agreed-upon
/// winner.
///
/// RaceManager is now a NetworkBehaviour that owns the authoritative race
/// state (countdown, live race clock, finish order) and is the single
/// source of truth all clients read from. Per-racer progress lives on each
/// racer's own RacerIdentity component instead of here.
///
/// Setup: place this on a NetworkObject in the race scene (NOT a prefab -
/// a scene-placed NetworkObject), wire up `checkpoints` in start->finish
/// order (checkpoint 0 doubles as the lap line), assign `npcBikeManager`,
/// and set `TotalLaps`.
/// </summary>
public class RaceManager : NetworkBehaviour
{
    public const int MaxRacers = 8;

    public static RaceManager Instance { get; private set; }

    [Header("Track Setup")]
    [Tooltip("Ordered start -> finish. Index 0 also acts as the lap line (crossing it after the last checkpoint completes a lap).")]
    public GameObject[] checkpoints;

    [Min(1)] public int TotalLaps = 3;
    public float countdownSeconds = 3f;

    [Header("NPCs")]
    public NPCBikeManager npcBikeManager;

    public int CheckpointCount => checkpoints?.Length ?? 0;

    [Networked] public RaceState State { get; private set; }
    [Networked] private TickTimer Countdown { get; set; }
    [Networked] private float RaceStartSimTime { get; set; }
    [Networked] private float RaceEndSimTime { get; set; }
    [Networked] private int FinishersCount { get; set; }

    [Networked, Capacity(MaxRacers)]
    public NetworkArray<RaceResultEntry> Results => default;

    private void Awake()
    {
        Instance = this;
        AssignCheckpointIndices();
    }

    private void AssignCheckpointIndices()
    {
        if (checkpoints == null) return;

        for (var i = 0; i < checkpoints.Length; i++)
        {
            if (checkpoints[i] == null) continue;

            var checkpoint = checkpoints[i].GetComponent<RaceCheckpoint>();
            if (checkpoint == null)
                checkpoint = checkpoints[i].AddComponent<RaceCheckpoint>();

            checkpoint.Index = i;
        }
    }

    public override void Spawned()
    {
        Instance = this;

        if (Object.HasStateAuthority)
            State = RaceState.Idle;
    }

    /// <summary>Call from a start-line trigger (see RaceStartZone.cs). Safe to call from any client.</summary>
    public void RequestBegin()
    {
        if (Object.HasStateAuthority)
            BeginCountdownAuthority();
        else
            RequestBeginRpc();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RequestBeginRpc() => BeginCountdownAuthority();

    private void BeginCountdownAuthority()
    {
        if (State != RaceState.Idle) return;

        State = RaceState.Countdown;
        Countdown = TickTimer.CreateFromSeconds(Runner, countdownSeconds);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (State == RaceState.Countdown && Countdown.Expired(Runner))
        {
            State = RaceState.Racing;
            RaceStartSimTime = Runner.SimulationTime;

            if (npcBikeManager != null)
                npcBikeManager.StartRace();
        }
    }

    /// <summary>
    /// Called by a racer (via RacerIdentity) the instant it completes its final lap.
    /// Routed through an RPC so there's exactly one authority deciding finish order,
    /// even though CurrentLap/CurrentCheckpoint themselves are written by each racer's own client.
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RequestFinishRpc(NetworkId racerId, float finishTime)
    {
        if (State != RaceState.Racing) return;
        if (FinishersCount >= MaxRacers) return;

        for (var i = 0; i < MaxRacers; i++)
        {
            if (Results[i].Valid && Results[i].RacerId == racerId)
                return; // already recorded, ignore duplicate/late RPC
        }

        var placement = FinishersCount + 1;

        Results.Set(FinishersCount, new RaceResultEntry
        {
            RacerId = racerId,
            FinishTime = finishTime,
            Placement = placement,
            Valid = true
        });

        FinishersCount = placement;

        if (FinishersCount >= ActiveRacerCount())
        {
            State = RaceState.Finished;
            RaceEndSimTime = Runner.SimulationTime;
        }
    }

    /// <summary>
    /// Simple by design: counts every RacerIdentity currently in the scene.
    /// Good enough for a race where all racers (players + NPCs) exist before
    /// the countdown starts. If you add mid-race joining/leaving, replace this
    /// with an explicit Networked registration counter instead.
    /// </summary>
    private int ActiveRacerCount()
    {
        return FindObjectsOfType<RacerIdentity>().Length;
    }

    /// <summary>Seconds since the race started (or the final race duration once finished).</summary>
    public float GetRaceClock()
    {
        return State switch
        {
            RaceState.Racing => Runner.SimulationTime - RaceStartSimTime,
            RaceState.Finished => RaceEndSimTime - RaceStartSimTime,
            _ => 0f
        };
    }

    public float? GetCountdownRemaining()
    {
        return State == RaceState.Countdown ? Countdown.RemainingTime(Runner) : null;
    }

    /// <summary>Best (highest) progress score among human racers - used by NPC rubber-banding.</summary>
    public int GetBestHumanProgress()
    {
        var humans = FindObjectsOfType<RacerIdentity>().Where(r => !r.IsNpc).ToList();
        return humans.Count == 0 ? -1 : humans.Max(r => r.GetProgressScore());
    }

    /// <summary>Live standings while the race is still in progress - sorted by progress, not finish time.</summary>
    public List<RacerIdentity> GetLiveStandings()
    {
        return FindObjectsOfType<RacerIdentity>()
            .OrderByDescending(r => r.GetProgressScore())
            .ToList();
    }

    /// <summary>Finished results, sorted by placement, with names resolved for display.</summary>
    public List<(NetworkId RacerId, string Name, float Time, int Placement, bool IsNpc)> GetResultsSummary()
    {
        var list = new List<(NetworkId, string, float, int, bool)>();

        for (var i = 0; i < MaxRacers; i++)
        {
            var entry = Results[i];
            if (!entry.Valid) continue;

            var name = "Racer";
            var isNpc = false;

            if (Runner != null && Runner.TryFindObject(entry.RacerId, out var obj))
            {
                var identity = obj.GetComponent<RacerIdentity>();
                if (identity != null)
                {
                    name = identity.DisplayName;
                    isNpc = identity.IsNpc;
                }
            }

            list.Add((entry.RacerId, name, entry.FinishTime, entry.Placement, isNpc));
        }

        list.Sort((a, b) => a.Item4.CompareTo(b.Item4));
        return list;
    }
}
