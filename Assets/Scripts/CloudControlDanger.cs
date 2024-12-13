using UnityEngine;

public class CloudControlDanger : MonoBehaviour
{
    // Parameters for movement
    public float moveSpeed = 20f; // Speed of the movement
    public float moveRange = 40f; // The distance the cloud moves back and forth

    // Parameters for flashing effect
    public float flashDuration = 3f; // Duration for flashing effect
    public Color flashColor1 = Color.white;
    public Color flashColor2 = Color.gray;
    private float flashTimer = 0f; // Timer for flashing effect
    private bool isFlashing = false; // Whether the cloud is flashing


    // For tracking positions
    private Vector3 startPosition;
    private Material cloudMaterial;

    void Start() {
        startPosition = transform.position;
        cloudMaterial = GetComponent<Renderer>().material; // Get the material of the cloud
    }

    void Update() {
        HandleMovement();
        HandleFlashing();
    }

    // Method to handle cloud's back and forth movement
    void HandleMovement() {
        // Move the cloud back and forth using PingPong
        float movement = Mathf.PingPong(Time.time * moveSpeed, moveRange);
        transform.position = new Vector3(startPosition.x + movement, transform.position.y, transform.position.z);
    }

    // Method to handle the flashing effect of the cloud
    void HandleFlashing()
    {
        if (GlobalSettings.changeTime - GlobalSettings.curBeat() <= flashDuration) {
            Debug.Log("Flashing");
            isFlashing = true;
            flashTimer += Time.deltaTime;
            float lerpValue = Mathf.PingPong(flashTimer * 2f, 1f);
            cloudMaterial.color = Color.Lerp(flashColor1, flashColor2, lerpValue);
        }
        else {
            if (isFlashing) {
                cloudMaterial.color = flashColor2; // Set back to gray after flashing
                isFlashing = false;
            }
        }
    }

}
