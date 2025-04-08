using UnityEngine;


public class AvatarSelector : MonoBehaviour
{
    public GameObject[] avatarPrefabs;
    private GameObject avatar;
    void Start()
    {
        DisplayAvatar(PlayerPrefs.GetInt("selectedAvatar", 0));
    }

    public void DisplayAvatar(int selectedAvatar)
    {
        // make sure the avatar selected is valid
        if (avatarPrefabs.Length == 0 || selectedAvatar < 0 || selectedAvatar >= avatarPrefabs.Length)
            return;

        // Spawn the avatar as a child of the bike (Player)
        GameObject prefab = avatarPrefabs[selectedAvatar];
        GameObject avatar = Instantiate(prefab, transform.position, transform.rotation);
        avatar.transform.SetParent(transform); // Set avatar as a child of the Player (bike)

        // Adjusting the avatar's position relative to the bike
        avatar.transform.localPosition = new Vector3(-0.02f, 0.55f, 0f); // Adjust as needed
        avatar.transform.localRotation = Quaternion.identity; // Resets rotation to align with bike

        Vector3 scale = new Vector3(0.45f, 0.45f, 0.45f); // Adjust scale as needed (0.5 is 50% of the original size)
        avatar.transform.localScale = scale;

        var avatarAnimator = avatar.GetComponent<Animator>();
        var movementController = GetComponent<PlayerMovementController>();

        if (avatarAnimator != null && movementController != null)
            movementController
                .AssignCharacterAnimator(avatarAnimator); // Assigning avatar's animator to the bike's controller
        else
            Debug.LogError("No animator found on avatar prefab or PlayerMovementController is missing.");
    }
}