using UnityEngine;

/// <summary>
/// New. Put this on a trigger collider at the start line. Replaces the old
/// behaviour where the race silently began the moment ANYONE touched
/// checkpoint index 0 (which is also awkward once checkpoint 0 is reused as
/// the lap line for multi-lap races). Only reacts to real players - NPCs
/// starting is still driven centrally once the countdown finishes.
/// </summary>
[RequireComponent(typeof(Collider))]
public class RaceStartZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (RaceManager.Instance == null) return;

        RaceManager.Instance.RequestBegin();
    }
}
