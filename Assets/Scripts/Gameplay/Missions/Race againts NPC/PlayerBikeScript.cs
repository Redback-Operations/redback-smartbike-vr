using UnityEngine;

public class PlayerBikeScript : MonoBehaviour
{
    public CheckpointManager checkpointManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            checkpointManager.CheckpointReached(other.gameObject, this.gameObject);
        }
    }
}
