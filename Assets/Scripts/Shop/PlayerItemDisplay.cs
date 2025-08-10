using UnityEngine;
using UnityEngine.Serialization;

public class PlayerItemDisplay : MonoBehaviour
{
    public string itemName;
    [FormerlySerializedAs("applePrefab")] public GameObject itemPrefab;
    public Transform spawnPoint;
    void Start()
    {
        var inventory = new PlayerInventory();
        for (int i = 0; i < inventory.GetItemCount(itemName); i++)
        {
            Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}
