using UnityEngine;

public class HoopControl : MonoBehaviour {
    public int midiNote; // Set this in the Inspector for each hoop
    public float triggerRadius = 5.0f; // Radius to detect proximity
    private Transform airplaneTransform; // Reference to the airplane's transform

    private void Start() {
        GameObject airplane = GameObject.FindWithTag("Player");
        if (airplane != null) {
            airplaneTransform = airplane.transform;
        }
        else Debug.LogError("Airplane not found. Make sure the airplane GameObject has the 'Player' tag.");
    }
    

    private void Update() {
        if (airplaneTransform == null) return;

        // Check if the airplane is within the trigger radius
        Vector3 hoopPosition = transform.position;
        Vector3 airplanePosition = airplaneTransform.position;

        // Only consider x and z axes
        float distance = Vector2.Distance(new Vector2(hoopPosition.x, hoopPosition.z),
                                          new Vector2(airplanePosition.x, airplanePosition.z));

        if (distance <= triggerRadius) TriggerNote();
    }

    private void TriggerNote()
    {
        GlobalSettings.midiNotePlayer.PlayNoteForDuration(midiNote, 0.5f);
        enabled = false;
    }
}
