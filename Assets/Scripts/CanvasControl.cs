using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;  // Reference to the score text
    public GameObject gameMessage;   // Reference to the start message object
    private TextMeshProUGUI gameMessageText;


    void Start() {
        UpdateScore(0);
        gameMessage.SetActive(false);
        gameMessageText = gameMessage.GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        // Start the game on Space key press
        if (GlobalSettings.gameState == -1) {
            gameMessageText.text = "GAME OVER!!!";
            gameMessage.SetActive(true);
        } else if (GlobalSettings.gameState == 2) {
            gameMessageText.text = "LEVELS COMPLETED!!!";
            gameMessage.SetActive(true);
        } else {
            UpdateScore(GlobalSettings.score);
        }
    }

    public void UpdateScore(int newScore) {
        scoreText.text = "Score: " + newScore;
    }

}
