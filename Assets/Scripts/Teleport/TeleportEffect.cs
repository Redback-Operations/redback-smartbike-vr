using UnityEngine;

public class TeleportEffect : MonoBehaviour
{
    // Target scene to teleport to
    private string targetScene;
    // Start is called before the first frame update
    void Start()
    {
        // Destroy if spawned on Bikes object
        if(transform.parent.gameObject.name == "Bikes")
        {
            Destroy(gameObject);
        }
        Debug.Log("Parent gameObject name: " + transform.parent.gameObject.name);

        // Destroy if TeleportEffect is already present 
        if (transform.parent.gameObject.GetComponentsInChildren<TeleportEffect>().Length > 1)
        {
            Destroy(gameObject);
        }

        this.GetComponent<Animation>().Play();
    }

    // Set target scene with passed string
    public void SetScene (string scene)
    {
        targetScene = scene;
    }

    // Trigger teleportation and load new scene
    void Teleport ()
    {
        if (NetworkManagement.Instance != null)
            NetworkManagement.Instance.Disconnect();
        
        MapLoader.LoadScene (targetScene);
    }
}
