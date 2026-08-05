using Fusion;
using UnityEngine;

/// <summary>
/// Replaces PlayerBikeScript.cs. One of these sits on every racer - the
/// player's NetworkPlayer bike AND every NPC bike - so checkpoint/lap
/// tracking is unified instead of duplicated (PlayerBikeScript had its own
/// OnTriggerEnter, RaceBikeMove had a second, near-identical copy) and
/// keyed by CompareTag("Player")/CompareTag("NPC") string checks.
///
/// Per-racer progress (CurrentCheckpoint/CurrentLap) is Networked so it
/// replicates to every client - that's what lets the live leaderboard show
/// everyone's progress, not just the local player's.
///
/// IMPORTANT: only the client with State Authority over THIS object acts on
/// trigger events. For a player's own bike that's the owning client (Fusion
/// Shared mode gives input authority + state authority to the same client
/// by default). For NPC bikes (scene-placed NetworkObjects) that's whichever
/// client is the Shared Mode Master Client. Every other client still gets
/// the OnTriggerEnter callback locally (physics runs on every client) but
/// bails out immediately, so there's no double-counting.
/// </summary>
public class RacerIdentity : NetworkBehaviour
{
    [Header("Identity")]
    [SerializeField] private bool isNpc;
    [SerializeField] private string npcDisplayName = "NPC Racer";

    public bool IsNpc => isNpc;

    public string DisplayName =>
        isNpc ? npcDisplayName : $"Player {Object.InputAuthority.PlayerId + 1}";

    [Networked] public int CurrentCheckpoint { get; private set; }
    [Networked] public int CurrentLap { get; private set; }
    [Networked] public NetworkBool HasFinished { get; private set; }

    private EventBinding<RaceCheckpointPassedEvent> _checkpointBinding;

    public override void Spawned()
    {
        CurrentCheckpoint = 0;
        CurrentLap = 0;
        HasFinished = false;
    }

    /// <summary>Lap*checkpointCount + checkpoint - used for sorting the live leaderboard and for NPC rubber-banding.</summary>
    public int GetProgressScore()
    {
        if (RaceManager.Instance == null || RaceManager.Instance.CheckpointCount == 0)
            return 0;

        return CurrentLap * RaceManager.Instance.CheckpointCount + CurrentCheckpoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the authoritative side for this specific racer processes checkpoints.
        if (Object == null || !Object.HasStateAuthority)
            return;

        if (HasFinished)
            return;

        var raceManager = RaceManager.Instance;
        if (raceManager == null || raceManager.State != RaceState.Racing)
            return;

        if (!other.CompareTag("Checkpoint"))
            return;

        var checkpoint = other.GetComponent<RaceCheckpoint>();
        if (checkpoint == null || checkpoint.Index != CurrentCheckpoint)
            return; // must be hit in order - skipping ahead does nothing

        CurrentCheckpoint++;

        if (CurrentCheckpoint >= raceManager.CheckpointCount)
        {
            CurrentCheckpoint = 0;
            CurrentLap++;

            EventBus<RaceCheckpointPassedEvent>.Raise(new RaceCheckpointPassedEvent
            {
                Lap = CurrentLap,
                Checkpoint = CurrentCheckpoint,
                RacerId = Object.Id
            });

            if (CurrentLap >= raceManager.TotalLaps)
            {
                HasFinished = true;
                var finishTime = raceManager.GetRaceClock();
                raceManager.RequestFinishRpc(Object.Id, finishTime);
            }
        }
        else
        {
            EventBus<RaceCheckpointPassedEvent>.Raise(new RaceCheckpointPassedEvent
            {
                Lap = CurrentLap,
                Checkpoint = CurrentCheckpoint,
                RacerId = Object.Id
            });
        }
    }
}
