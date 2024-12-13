using UnityEngine;

public class CloudControlNormal : MonoBehaviour {
    public float moveSpeed = 1f; // Speed of the movement
    public float moveRange = 10f; // The distance the cloud moves back and forth

    private Vector3 startPosition;

    void Start() {
        startPosition = transform.position;
    }

    void Update() {
        float movement = Mathf.PingPong(Time.time * moveSpeed, moveRange);
        transform.position = new Vector3(startPosition.x + movement, transform.position.y, transform.position.z);
    }
}
