using UnityEngine;

/// <summary>
/// Copy-me starting point for a new mission.
///
/// Duplicate this file, rename the class and the file to match (Unity requires
/// them to be identical), then override MissionNumber / MissionName and fill in
/// the lifecycle hooks. Drop the component on a child of
/// Objectives/Missions in your scene and add it to Mission_Activator.Missions.
///
/// Mission_Activator enables exactly the child whose MissionNumber matches the
/// number stored in PlayerPrefs ("MissionNumber") and disables every other one,
/// so everything specific to this mission should live under this GameObject.
/// </summary>
public class MissionTemplate : Mission
{
    // -1 keeps the template itself from ever being activated by accident.
    public override int MissionNumber => -1;
    public override string MissionName => "Mission Template";

    [Header("Completion")]
    [Tooltip("Optional. Assigned in the inspector; shown to the player when the mission ends.")]
    [SerializeField] private GameObject completeUI;
    [SerializeField] private GameObject failUI;

    private bool _finished;

    /// <summary>
    /// Called by Mission_Activator (via the base class) once this mission is the
    /// active one. Set up spawns, timers and objectives here rather than in
    /// Awake/Start, so nothing runs while the mission is disabled.
    /// </summary>
    public override void StartMission()
    {
        base.StartMission();
        _finished = false;
    }

    protected virtual void CompleteMission()
    {
        if (_finished) return;
        _finished = true;

        if (completeUI != null) completeUI.SetActive(true);
        Debug.Log($"Mission {MissionNumber} ({MissionName}) complete");
    }

    protected virtual void FailMission()
    {
        if (_finished) return;
        _finished = true;

        if (failUI != null) failUI.SetActive(true);
        Debug.Log($"Mission {MissionNumber} ({MissionName}) failed");
    }
}
