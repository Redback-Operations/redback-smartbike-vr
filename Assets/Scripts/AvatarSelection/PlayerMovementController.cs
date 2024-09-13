using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour {

    //public float MoveSpeed = 1.5f;
    //public float TurnSpeed = 100.0f;
    //public GameObject bike;

    //private Animator bikeAnim = null;
    //private Animator characterAnim = null;

    //private void Start() {
    //    if (bike != null) {
    //        bikeAnim = bike.GetComponent<Animator>();
    //    } else {
    //        Debug.LogError("Bike GameObject is not assigned.");
    //    }
    //}

    //// Method to assign the character's Animator
    //public void AssignCharacterAnimator(Animator animator) {
    //    characterAnim = animator;
    //}

    //private void Update() {
    //    if (bikeAnim != null) {
    //        float vert = Input.GetAxis("Vertical"); 
    //        float horz = Input.GetAxis("Horizontal");


    //        bool isMoving = vert != 0 || horz != 0;

    //        //Setting Animator parameters based on movement for bike
    //        bikeAnim.SetBool("Moving", isMoving);
    //        bikeAnim.SetFloat("Speed", isMoving ? vert : 0.0f); 

    //        //Setting Animator parameters based on movement for character
    //        if (characterAnim != null) {
    //            characterAnim.SetBool("Moving", isMoving);
    //            characterAnim.SetFloat("Speed", isMoving ? vert : 0.0f); 
    //        }

    //        this.transform.Translate(0.0f, 0.0f, vert * MoveSpeed * Time.deltaTime);
    //        this.transform.Rotate(0.0f, horz * TurnSpeed * Time.deltaTime, 0.0f);
    //    }
    //}

    public float MoveSpeed = 1.5f;
    public float TurnSpeed = 100.0f;
    public GameObject bike;

    private Animator bikeAnim = null;
    private Animator characterAnim = null;

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
        if (bikeAnim != null) {
            float vert = Input.GetAxis("Vertical");
            float horz = Input.GetAxis("Horizontal");

            bool isMoving = vert != 0 || horz != 0;

            //Setting Animator parameters based on movement for bike
            bikeAnim.SetBool("Moving", isMoving);
            bikeAnim.SetFloat("Speed", isMoving ? vert : 0.0f);

            //Setting Animator parameters based on movement for character
            if (characterAnim != null) {
                characterAnim.SetBool("Moving", isMoving);
                characterAnim.SetFloat("Speed", isMoving ? vert : 0.0f);
            }

            // Move the bike forward/backward and rotate based on input
            this.transform.Translate(0.0f, 0.0f, vert * MoveSpeed * Time.deltaTime);
            this.transform.Rotate(0.0f, horz * TurnSpeed * Time.deltaTime, 0.0f);
        }
    }

}
