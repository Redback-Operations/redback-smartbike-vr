using UnityEngine;

/// <summary>
/// Minor upgrade from the original: takes RaceBikeMove[] directly instead of
/// GameObject[] + a GetComponent lookup per bike, so a missing/misconfigured
/// NPC prefab shows up as a null-reference at edit time instead of silently
/// doing nothing at runtime.
/// </summary>
public class NPCBikeManager : MonoBehaviour
{
    public RaceBikeMove[] npcBikes;

    public void StartRace()
    {
        if (npcBikes == null) return;

        foreach (var npcBike in npcBikes)
        {
            if (npcBike != null)
                npcBike.StartRacing();
        }
    }
}
