using UnityEngine;

public class CameraFollowAirplane : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform airplane; // Reference to the airplane transform

    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(0f, 5f, -10f); // Offset from the airplane (relative to its local space)
    public float followSpeed = 5f; // Speed at which the camera moves to follow the airplane
    public float rotationSpeed = 5f; // Speed at which the camera adjusts its rotation

    void LateUpdate()
    {
        if (airplane == null) return;

        // Calculate the desired position based on the airplane's position and offset
        Vector3 desiredPosition = airplane.position + airplane.TransformDirection(offset);

        // Smoothly move the camera to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Smoothly rotate the camera to align with the airplane's rotation
        Quaternion desiredRotation = Quaternion.LookRotation(airplane.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }
}
