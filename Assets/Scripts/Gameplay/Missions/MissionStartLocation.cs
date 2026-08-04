using System.Collections;
using UnityEngine;

// Raised once the Ready/Set/Go sequence has finished for a given mission,
// signalling that gameplay (Mission.StartMission) is now allowed to begin.
// See Mission_Activator.CountdownFinished, which listens for this.
public struct MissionCountdownFinishedEvent : IEvent
{
    public int MissionNumber;
}

// Place this on a marker positioned wherever a mission's gameplay should
// actually begin - typically the same spot as that mission's MissionSpawn
// (Assets/Scripts/UI/Objective/MissionSpawn.cs), so the player rides straight
// up to the marker on scene load.
//
// Requires a trigger Collider on this GameObject (or a child) sized to cover
// where the player spawns/arrives, and a "Player" tagged rider (see
// TeleportGate.cs for the same tag convention used elsewhere in the project).
//
// Flow: player enters trigger -> Ready/Set/Go notifications play via
// UIManager -> MissionCountdownFinishedEvent is raised -> Mission_Activator
// flips CountdownFinished -> PlayerController allows Mission.StartMission().
[RequireComponent(typeof(Collider))]
public class MissionStartLocation : MonoBehaviour
{
    [Tooltip("Must match the Mission.MissionNumber this marker starts.")]
    public int MissionNumber;

    [Tooltip("Optional in-world visual (e.g. a reskinned portal ring) shown at the start location. Deactivated once the countdown finishes if assigned.")]
    public GameObject visualMarker;

    [Header("Ready / Set / Go timing (seconds)")]
    [Tooltip("Hold time for each phase. UIManager.ShowNotification adds its own ~0.5s fade in/out on top of this, so actual on-screen time per phase is roughly duration + 1s. Tune during playtesting.")]
    public float readyDuration = 0.4f;
    public float setDuration = 0.4f;
    public float goDuration = 0.4f;

    private bool _countdownStarted;

    private void OnTriggerEnter(Collider other)
    {
        if (_countdownStarted) return;
        if (!other.CompareTag("Player")) return;

        // only react if this marker belongs to the mission that's actually active
        if (Mission_Activator.ActiveMission == null || Mission_Activator.ActiveMission.MissionNumber != MissionNumber)
            return;

        _countdownStarted = true;
        StartCoroutine(RunCountdown());
    }

    private IEnumerator RunCountdown()
    {
        if (UIManager.Instance != null)
        {
            yield return UIManager.Instance.ShowNotification("Ready...", readyDuration);
            yield return UIManager.Instance.ShowNotification("Set...", setDuration);
            yield return UIManager.Instance.ShowNotification("GO!", goDuration);
        }

        EventBus<MissionCountdownFinishedEvent>.Raise(new MissionCountdownFinishedEvent { MissionNumber = MissionNumber });

        if (visualMarker != null)
            visualMarker.SetActive(false);
    }
}
