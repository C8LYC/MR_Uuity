using UnityEngine;
public class CoinControll : MonoBehaviour {
    public int coinValue = 1; // The score value for this coin
    public float spinSpeed = 100f; // Speed of the coin's spinning

    private void Update() {
        // Rotate the coin around its Y-axis
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter(Collision collision) {
        // Check if the collider is tagged as "Player"
        Debug.Log("Collsion");
        if (collision.gameObject.CompareTag("Player")) {
            // Add to the score (uncomment if ScoreManager is used)
            // ScoreManager.AddScore(coinValue);
            Debug.Log("Coin collected!");
            GlobalSettings.score += 1;
            // Destroy the coin object
            Destroy(gameObject);
        }
    }
}
