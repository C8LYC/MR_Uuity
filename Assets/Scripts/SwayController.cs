using UnityEngine;

public class SwayController : MonoBehaviour
{
    public GameObject targetObject; // Assign the target GameObject in the Inspector
    public bool trigger = false;    // Control variable to toggle rotation
    public Vector3 rotationOffset;  // Maximum rotation offset in degrees
    public float swaySpeed = 1f;    // Speed of the swaying motion

    private Quaternion originalRotation;
    private float swayTimer = 0f;
    public Transform Player { get; set; }

    void Start()
    {
        if (targetObject != null)
        {
            originalRotation = targetObject.transform.rotation;
        }
    }

    void Update()
    {
        if (targetObject == null)
            return;

        if (trigger)
        {
            ApplySwayingRotation();
        }
        else
        {
            RestoreOriginalRotation();
        }
    }

    void ApplySwayingRotation()
    {
        swayTimer += Time.deltaTime * swaySpeed;
        float swayFactor = Mathf.Sin(swayTimer); // Oscillates between -1 and 1
        Vector3 currentOffset = rotationOffset * swayFactor;
        Quaternion offsetRotation = Quaternion.Euler(currentOffset);
        targetObject.transform.rotation = originalRotation * offsetRotation;
    }

    void RestoreOriginalRotation()
    {
        targetObject.transform.rotation = originalRotation;
        swayTimer = 0f; // Reset the timer to start the sway from the beginning when triggered again
    }
}
