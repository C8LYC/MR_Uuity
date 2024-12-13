using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour {
    string[] scenes = {"TrillSlidePro"};
    int gameState = 0;

    GameObject FindFromScene(string sceneName, string objectName) {
        Scene targetScene = SceneManager.GetSceneByName(sceneName);
        if (targetScene.isLoaded) {
            GameObject[] rootObjects = targetScene.GetRootGameObjects();

            foreach (GameObject rootObject in rootObjects) {
                Debug.Log(rootObject.name);
                if (rootObject.name == objectName) {
                    Debug.Log("Found object: " + rootObject.name);
                    return rootObject;
                }
            }
            return null;
        }
        else {
            Debug.Log("The target scene is not loaded.");
            return null;
        }
    }
    void Awake() {
        if (GlobalSettings.isInit == false) GlobalSettings.Initialize();
        GameObject targetObject = GameObject.Find("SceneManager");
        GlobalSettings.sceneManager = targetObject.GetComponent<SceneManagement>();
        targetObject = GameObject.Find("Main Camera");
        GlobalSettings.midiNotePlayer = targetObject.GetComponent<MidiNotePlayer>();
        GlobalSettings.isInit = true;
    }

    // Load a scene additively (internal access)
    internal void LoadSceneAdditive(string sceneName) {
       // SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        Debug.Log("Loading scene additively: " + sceneName);
    }

    // Unload a scene by name (internal access)
    internal void UnloadSceneByName(string sceneName) {
     //   SceneManager.sceneLoaded -= OnSceneLoaded;
        if (SceneManager.GetSceneByName(sceneName).isLoaded) {
            SceneManager.UnloadSceneAsync(sceneName);
        } else { 
            Debug.LogWarning("Scene is not loaded, so cannot be unloaded. Scene name: " + sceneName); 
        }
    }
    public void SwitchMode() {
        int mode = 0;
        //(GlobalSettings.level & 1);
        Time.timeScale = 0f;
        //UnloadSceneByName(scenes[mode]);
        //mode = 1 - mode;
        Debug.Log("Mode switch: " + mode);
        LoadSceneAdditive(scenes[mode]);
        return;
    }
    public void EndGame(int state = -1) {
        GlobalSettings.gameState = state;
    }
    void Update() {
        if (gameState == 0 && GlobalSettings.gameState == 1) {
           LoadSceneAdditive(scenes[0]);
           GlobalSettings.level = 0;
        }
        if (GlobalSettings.changeTime - GlobalSettings.curBeat() <= 1e-6) {
            GlobalSettings.changeTime = 10000000000;
            SwitchMode();
            GlobalSettings.level += 1;
            if (GlobalSettings.level >= GlobalSettings.levelData.levels.Length) {
                EndGame(2);
            }
        }
        gameState = GlobalSettings.gameState;
    }
}
