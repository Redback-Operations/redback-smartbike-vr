using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


public class AvatarSelector : MonoBehaviour
{
    public GameObject[] avatarPrefabs;
    private GameObject avatar;
    [SerializeField] private BikeSelector bikeSelector;

    IEnumerator Start()
    {
        // saveLoadBike.onBikeSelected += SetupBike;
        yield return null;
        DisplayAvatar(PlayerPrefs.GetInt("selectedAvatar", 0));
        SetupBike(bikeSelector.CurrentBike);
    }

    private void SetupBike(Bike bike)
    {
        var playerIkController = avatar.GetComponent<AvatarIkController>();
        if (playerIkController)
        {
            playerIkController.Setup(bike);
        }
    }

    public void DisplayAvatar(int selectedAvatar)
    {
        // make sure the avatar selected is valid
        if (avatarPrefabs.Length == 0 || selectedAvatar < 0 || selectedAvatar >= avatarPrefabs.Length)
            return;

        // Spawn the avatar as a child of the bike (Player)
        GameObject prefab = avatarPrefabs[selectedAvatar];
        avatar = Instantiate(prefab, bikeSelector.CurrentBike.mountTf);
        // Adjusting the avatar's position relative to the bike
        avatar.transform.localRotation = Quaternion.Euler(70,0,0); // Resets rotation to align with bike
        avatar.transform.localPosition = Vector3.zero; // Resets rotation to align with bike

        Vector3 scale = new Vector3(0.45f, 0.45f, 0.45f); // Adjust scale as needed (0.5 is 50% of the original size)
        avatar.transform.localScale = scale;

        var avatarAnimator = avatar.GetComponent<Animator>();
        var movementController = GetComponent<PlayerAnimationController>();

        if (avatarAnimator != null && movementController != null)
            movementController
                .AssignCharacterAnimator(avatarAnimator); // Assigning avatar's animator to the bike's controller
        else
            Debug.Log("No animator found on avatar prefab or PlayerMovementController is missing.");
    }
}