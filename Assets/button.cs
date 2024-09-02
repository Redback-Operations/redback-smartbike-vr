using System.Collections;
using UnityEngine;

//Pressing a button from player that opens up the gate - placed in the button GameObject/Prefab
public class button : MonoBehaviour
{
    //Referencing the gate, joints and bike.
    public GameObject LJoint;
    public GameObject RJoint;
    public GameObject player;
    public GameObject LeftEmpty;
    public GameObject RightEmpty;


    //Player has not pressed button yet.
    private bool isPressed = false;

    //For time delay to reset.
    private bool isResetting = false;

    //Speed for the gate.
    private float speed = 2.0f;
    private float rotationSpeed = 90.0f;

    //Time delay
    private float delay = 2.0f;

    //A position
    private Vector3 emptyL;
    private Vector3 emptyR;

    private Vector3 gateLeft;
    private Vector3 gateRight;
    // Target rotation
    private Quaternion targetRotation;
    private Quaternion counterTargetRotation;
    private Quaternion gaterRight;
    private Quaternion gaterLeft;

        IEnumerator StartResetting()
    {
        // Set resetting flag to true to prevent multiple coroutines running simultaneously
        isResetting = true;
        
        // Wait for the specified delay before resetting the gate
        yield return new WaitForSeconds(delay);
        
        // Reset the gate position
        isResetting = false;
    }

    //Collision requires both rigidBody, kinematics, isTrigger
    //When GameObject collides with another GameObject
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            isPressed = true; //Button is pressed when player enters the trigger.
            isResetting = false; //Cancels time delay reset
        }
    }

    //When Collider other has stopped touching the trigger.
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player) 
        { 
            isPressed = false;
            //Starting resetting process
            StartCoroutine(StartResetting());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Position of empty
        emptyL = LeftEmpty.transform.position;
        emptyR = RightEmpty.transform.position;
        //Position of gate
        gateLeft = LJoint.transform.position;
        gateRight = RJoint.transform.position;
        //Original rotation of gate - Leave this commented if you want to move the gate apart instead of swinging it open
        gaterLeft = LJoint.transform.rotation;
        gaterRight = RJoint.transform.rotation;
        //Set the target rotation to 180 degrees around the Y-axis
        targetRotation = Quaternion.Euler(0, 170, 0);
        counterTargetRotation = Quaternion.Euler(0, 10, 0);
    }

    // Update is called once per frame
    void Update()
    {
        var step = Time.deltaTime * speed;
        var rotation = Time.deltaTime * rotationSpeed;

        if (isPressed)
        {
            //Moves the gate to empty. - To move it apart
            // LJoint.transform.position = Vector3.MoveTowards(LJoint.transform.position, emptyL, step);
            // RJoint.transform.position = Vector3.MoveTowards(RJoint.transform.position, emptyR, step);
            //To rotate the gate
            LJoint.transform.rotation = Quaternion.RotateTowards(LJoint.transform.rotation, targetRotation, rotation);
            RJoint.transform.rotation = Quaternion.RotateTowards(RJoint.transform.rotation, counterTargetRotation, rotation);
        }
        
        else if (!isResetting)
        {
            //Place it back to original position
            // LJoint.transform.position = Vector3.MoveTowards(LJoint.transform.position, gateLeft, step);
            // RJoint.transform.position = Vector3.MoveTowards(RJoint.transform.position, gateRight, step);
            //To place rotation back to original
            LJoint.transform.rotation = Quaternion.RotateTowards(LJoint.transform.rotation, gaterLeft, rotation);
            RJoint.transform.rotation = Quaternion.RotateTowards(RJoint.transform.rotation, gaterRight, rotation);
        }
    }
}
