using UnityEngine;

public class CubeController : MonoBehaviour
{
    public float movementSpeed = 5f;
    public float rotationSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = 9.81f;

    private Rigidbody rb;
    private Quaternion initialRotation;
    private Vector3 lastMovementDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Freeze rotation on all axes
        rb.freezeRotation = true;

        // Store the initial rotation of the cube
        initialRotation = transform.rotation;
        lastMovementDirection = Vector3.zero;
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction
        Vector3 movementDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // If there is movement input, update the lastMovementDirection
        if (movementDirection != Vector3.zero)
        {
            lastMovementDirection = movementDirection;
        }

        // Apply gravity to the movement
        movementDirection.y -= gravity * Time.deltaTime;

        // Rotate the cube to face the movement direction
        if (lastMovementDirection != Vector3.zero)
        {
            // Apply the initial rotation to the last movement direction
            Vector3 rotatedDirection = initialRotation * lastMovementDirection;
            Quaternion targetRotation = Quaternion.LookRotation(rotatedDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move the cube using Rigidbody's velocity
        rb.velocity = new Vector3(movementDirection.x * movementSpeed, rb.velocity.y, movementDirection.z * movementSpeed);

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        // A simple raycast to check if the cube is grounded.
        // You can use more advanced methods depending on your environment setup.
        return Physics.Raycast(transform.position, Vector3.down, 0.1f);
    }
}
