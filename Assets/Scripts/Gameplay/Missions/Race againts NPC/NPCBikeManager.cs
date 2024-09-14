using UnityEngine;

public class NPCBikeManager : MonoBehaviour
{
    public GameObject[] npcBikes;

    public void StartRace()
    {
        foreach (GameObject npcBike in npcBikes)
        {
            npcBike.GetComponent<RaceBikeMove>().StartRacing();
        }
    }
}
