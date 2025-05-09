using TMPro;
using UnityEngine;

public class PlayerBikeScript : MonoBehaviour
{
    public TextMeshProUGUI raceResultText;
    private void Start()
    {
        CheckpointManager.Instance.raceResultText = raceResultText;
    }

    private void OnTriggerEnter(Collider other)
    {
        // must have a checkpoint manager in the scene
        if (CheckpointManager.Instance == null)
            return;

        if (other.CompareTag("Checkpoint"))
        {
            CheckpointManager.Instance.CheckpointReached(other.gameObject, this.gameObject);
        }
    }
}
