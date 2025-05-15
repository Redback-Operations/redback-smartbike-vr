using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.TextCore.Text;

public class LoadAvatar : MonoBehaviour {

    public GameObject[] avatarPrefabs;
    public Transform spawnPoint;
    public TMP_Text label;

    public GameObject bike;
    private PlayerMovementController movementController;

    //Gets the selectedAvatar saved in the Avatar selection  and spawns it
    private void Start() {
        int selectedAvatar = PlayerPrefs.GetInt("selectedAvatar", 0);
        Debug.Log("Selected Avatar Index: " + selectedAvatar);

        GameObject prefab = avatarPrefabs[selectedAvatar];

        GameObject avatar = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        avatar.transform.SetParent(bike.transform); //Settting the character as a child to the bike

        //Adjusting avatar's position and rotation
        avatar.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        avatar.transform.Rotate(0, -90, 0);
        Vector3 newPosition = avatar.transform.position;
        newPosition.y += 2.0f;
        

        //Getting the avatar's Animator
        Animator avatarAnimator = avatar.GetComponent<Animator>();

        //Finding the PlayerMovementController on the bike
        movementController = bike.GetComponent<PlayerMovementController>();

        if (avatarAnimator != null && movementController != null) {
            movementController.AssignCharacterAnimator(avatarAnimator); //Passing the avatar's animator to PlayerMovementController
        } else {
            Debug.LogError("No animator found on avatar prefab or PlayerMovementController is missing.");
        }
    }

}
 



