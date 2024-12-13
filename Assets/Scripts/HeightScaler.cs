using UnityEngine;

public class HeightScaler : MonoBehaviour
{
    // Public variable to control the Z-axis scaling multiplier
    [Tooltip("Multiplier to scale the Z-axis of the 'root' child.")]
    public float zScaleMultiplier = 1.0f;

    // The name of the child GameObject to scale
    private string childName = "root";

    // Store the original Z scale of the child
    private float originalZScale;

    void Start()
    {
        // Find the child GameObject named "root"
        Transform rootTransform = transform.Find(childName);

        // If the child exists, store its original Z scale
        if (rootTransform != null)
        {
            originalZScale = rootTransform.localScale.z;
        }
        else
        {
            Debug.LogError($"Child GameObject '{childName}' not found! Please ensure the child exists.");
        }
    }

    void Update()
    {
        // Find the child GameObject named "root"
        Transform rootTransform = transform.Find(childName);

        // If the child exists, scale its Z-axis
        if (rootTransform != null)
        {
            // Adjust only the Z scale, keeping X and Y scales unchanged
            Vector3 currentScale = rootTransform.localScale;
            rootTransform.localScale = new Vector3(currentScale.x, currentScale.y, originalZScale * zScaleMultiplier);
        }
    }
}