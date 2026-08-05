using Fusion;

public interface IEvent { }

public struct TestEvent : IEvent { }

// --- Race Mission events (new) ---------------------------------------
// Raised locally (per-client) by RacerIdentity/RaceManager so UI (RaceHUD)
// and anything else that cares can react without being tightly coupled to
// the networking code, following the same pattern TeleportEvent already
// uses elsewhere in the project.

public struct RaceCheckpointPassedEvent : IEvent
{
    public NetworkId RacerId;
    public int Checkpoint;
    public int Lap;
}

public struct RaceFinishedEvent : IEvent
{
    public NetworkId RacerId;
    public int Placement;
    public float FinishTime;
}
