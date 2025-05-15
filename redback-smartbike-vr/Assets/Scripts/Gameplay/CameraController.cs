using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject targetObject;
    public float distanceFromBike = 5f;
    public float heightOffset = 2f;
    public float rotationSpeed = 5f;

    private Vector3 offset;
    private float cameraAngle;

    // Start is called before the first frame update
    void Start()
    {
        // Calculate the initial offset between the camera and the cube
        offset = transform.position - targetObject.transform.position;

        // Set the initial camera angle
        cameraAngle = 0f;
    }

    // LateUpdate is called after all Update functions have been called
    void LateUpdate()
    {
        // Update the camera angle based on mouse movement
        UpdateCameraAngle();

        // Move the camera around the cube
        MoveCameraAroundCube();
    }

    void UpdateCameraAngle()
    {
        float horizontalRotation = Input.GetAxis("Mouse X") * rotationSpeed;

        // Update the camera angle based on horizontal mouse movement
        cameraAngle += horizontalRotation;

        // Keep the camera angle within 360 degrees
        cameraAngle %= 360f;
    }

    void MoveCameraAroundCube()
    {
        // Calculate the position of the camera based on the camera angle, distance, and height offset from the cube
        Vector3 cameraPosition = targetObject.transform.position + Quaternion.Euler(0f, cameraAngle, 0f) * (Vector3.back * distanceFromBike);
        cameraPosition.y = targetObject.transform.position.y + heightOffset;

        // Update the camera's position and rotation
        transform.position = cameraPosition;
        transform.LookAt(targetObject.transform.position);
    }
}
