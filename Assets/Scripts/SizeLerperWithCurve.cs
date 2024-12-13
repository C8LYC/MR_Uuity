using UnityEngine;

public class SizeLerperWithCurve : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject; // Assign the GameObject to be scaled

    [SerializeField]
    public bool startLerp = false; // Checkbox in the Inspector

    [SerializeField]
    private float lerpDuration = 1.0f; // Duration of the scaling

    [SerializeField]
    private AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1); // Settable curve for scaling

    private Vector3 originalScale; // Store the original size
    private Vector3 startScale = new Vector3(0.01f, 0.01f, 0.01f); // Starting small size
    private float lerpTime; // Internal timer for scaling

    private void Start()
    {
        if (targetObject != null)
        {
            // Store the original size of the object
            originalScale = targetObject.transform.localScale;
            // Set the initial size to the starting small scale
            targetObject.transform.localScale = startScale;
        }
    }

    private void Update()
    {
        Grow();
    }

    public void Grow()
    {
        if (startLerp && targetObject != null)
        {
            // Increment the lerp time
            lerpTime += Time.deltaTime;

            // Calculate the curve value (normalized 0 to 1)
            float curveValue = scaleCurve.Evaluate(lerpTime / lerpDuration);

            // Interpolate the scale using the curve value
            targetObject.transform.localScale = Vector3.LerpUnclamped(startScale, originalScale, curveValue);

            // Stop scaling once the duration is reached
            if (lerpTime >= lerpDuration)
            {
                targetObject.transform.localScale = originalScale;
                startLerp = false; // Reset checkbox to prevent re-triggering
                lerpTime = 0; // Reset the timer
            }
        }
    }
}
