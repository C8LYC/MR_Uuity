using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public static class GlobalSettings {
    
    // Some important scripts
    public static LevelGeneratorSlide slideControl = null;
    public static SceneManagement sceneManager = null;
    public static MidiNotePlayer midiNotePlayer = null;
    public static float heightOffset = 1f;
    public static int basePitch = 69;
    public static float pitchSensitivity = 0.5f;
    public static float timeRatio = 0.5f;
    public static string jsonFilePath = "Assets/Levels/game.json";
    public static LevelData levelData = null;
    public static bool isInit = false;

    public static float groundLevel = 0f;
    public static float ceilingLevel = 4f;


    public static int mode = 0;
    public static float changeTime = 100000000;

    public static int level = -1;
    public static int gameState = 0; // 0: haven't started yet, 1: started, 2: win, -1: lose (?)
    public static int score = 0;

	public static float key2height(float key) {
	    float lowestKey = 60f;
	    float highestKey = 85f;

	    float height = Mathf.Lerp(groundLevel, ceilingLevel, (key - lowestKey) / (highestKey - lowestKey));
	    return Mathf.Clamp(height, groundLevel, ceilingLevel);
    }
    public static float curBeat() { //Time to 拍子 
        return Time.time / timeRatio;
    }
    public static void Initialize() {
        if (File.Exists(jsonFilePath)) {
            string json = File.ReadAllText(jsonFilePath);
            levelData = JsonConvert.DeserializeObject<LevelData>(json);  // Deserialize to your specific class
            Debug.Log("Level Data Loaded Successfully");
        }
    }
}

