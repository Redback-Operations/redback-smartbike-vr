using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AvatarSelection : MonoBehaviour {

    public GameObject[] avatars;
    public int selectedAvatar = 0;

    /*When the player presses the button, this function will be called*/
    public void NextAvatar() {
        /*Avatar will not be visible in the scene to start off with*/
        avatars[selectedAvatar].SetActive(false);
        /*To cycle through avatar options*/
        selectedAvatar = (selectedAvatar + 1) % avatars.Length;
        /*Selected avatar set to true*/
        avatars[selectedAvatar].SetActive(true);
    }

    /* Similar to above but inverse*/
    public void PreviousAvatar() {
        avatars[selectedAvatar].SetActive(false);
        selectedAvatar--;
        if (selectedAvatar < 0) {
            selectedAvatar += avatars.Length;
        }
        avatars[selectedAvatar].SetActive(true);

    }

    /*PlayerPrefs saving avatar choice based on selectedAvatar variable*/
    public void StartGame() {
        PlayerPrefs.SetInt("selectedAvatar", selectedAvatar);

        SceneManager.LoadScene(0, LoadSceneMode.Single); /*To load into the garage scene*/
    }

    /*To have the animator disabled during the character selection scene */
    private void Start() {
        avatars[selectedAvatar].SetActive(true);

        foreach (GameObject avatar in avatars) {
            Animator animator = avatar.GetComponent<Animator>();
            if (animator != null) {
                animator.enabled = false;
            }
        }

    }
}

