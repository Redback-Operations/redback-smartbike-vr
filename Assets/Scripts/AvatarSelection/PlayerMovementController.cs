using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour {

    public float MoveSpeed = 1.5f; // Speed at which the player moves
    public float TurnSpeed = 100.0f; // Speed at which the player rotates
    public GameObject bike; // Reference to the bike GameObject

    private Animator bikeAnim = null; // Animator for the bike
    private Animator characterAnim = null; // Animator for the character

    private Vector3 stopPosition; // Variable to lock the player's position
    private bool isStopped = false; // Flag to check if the player is stopped

    private void Start() {
        if (bike != null) {
            bikeAnim = bike.GetComponent<Animator>();
        } else {
            Debug.LogError("Bike GameObject is not assigned.");
        }
    }

    // Method to assign the character's Animator
    public void AssignCharacterAnimator(Animator animator) {
        characterAnim = animator;
    }

    private void Update() {
        if (isStopped) {
            // Lock the player at the stop position
            transform.position = stopPosition;
        }

        if (!isStopped && bikeAnim != null) {
            float vert = Input.GetAxis("Vertical");
            float horz = Input.GetAxis("Horizontal");

            bool isMoving = vert != 0 || horz != 0;

            // Setting Animator parameters based on movement for bike
            bikeAnim.SetBool("Moving", isMoving);
            bikeAnim.SetFloat("Speed", isMoving ? vert : 0.0f);

            // Setting Animator parameters based on movement for character
            if (characterAnim != null) {
                characterAnim.SetBool("Moving", isMoving);
                characterAnim.SetFloat("Speed", isMoving ? vert : 0.0f);
            }

            // Move the bike forward/backward and rotate based on input
            this.transform.Translate(0.0f, 0.0f, vert * MoveSpeed * Time.deltaTime);
            this.transform.Rotate(0.0f, horz * TurnSpeed * Time.deltaTime, 0.0f);
        }
    }

    // Detect collision with BuildingCollisions objects
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Collision")) {
            Debug.Log("Player entered the BuildingCollisions trigger!");

            // Stop the player by locking their position
            stopPosition = transform.position;
            isStopped = true;

            // Stop animations to reflect the stopped state
            if (bikeAnim != null) {
                bikeAnim.SetBool("Moving", false);
                bikeAnim.SetFloat("Speed", 0.0f);
            }
            if (characterAnim != null) {
                characterAnim.SetBool("Moving", false);
                characterAnim.SetFloat("Speed", 0.0f);
            }
        }
    }

    // Allow the player to move again when leaving the trigger
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Collision")) {
            Debug.Log("Player exited the BuildingCollisions trigger!");

            // Allow movement again
            isStopped = false;
        }
    }
}