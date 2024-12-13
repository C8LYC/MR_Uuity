using UnityEngine;

public class SwayController : MonoBehaviour
{
    public GameObject targetObject; // Assign the target GameObject in the Inspector
    public bool trigger = false;    // Control variable to toggle rotation
    public Vector3 rotationOffset;  // Maximum rotation offset in degrees
    public float swaySpeed = 1f;    // Speed of the swaying motion

    private Quaternion originalRotation;
    private float swayTimer = 0f;
    Vector3 PlayerPos { get; set; }
    SystemController systemController { get; set; }
	/// <summary>
	/// Amplitude between 0.4f ~ 0.58f
	/// </summary>
    float AmplitudeRange = 0.18f;

	// max distance = 2f;

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

        if (trigger && IsAmplitudeInRange())
        {
            ApplySwayingRotation();
        }
        else
        {
            RestoreOriginalRotation();
        }
    }

    bool IsAmplitudeInRange()
    {
        float dis = Vector3.Distance(transform.position, PlayerPos);
        if(dis / 2f * AmplitudeRange + 0.4f < systemController.amplitude || dis / 2f * AmplitudeRange + 0.4f - AmplitudeRange / 10f > systemController.amplitude)
        {
            return false;
        }
        return true;
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

    public void InitSetting(SystemController systemController)
    {
        PlayerPos = systemController.PlayerStartPos;
        this.systemController = systemController;
    }
}
