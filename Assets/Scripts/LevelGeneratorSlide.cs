using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class LevelGeneratorSlide : MonoBehaviour {


    private LevelData.Level data;
    public GameObject parent;

    public GameObject hoopPrefab;
    public GameObject coinPrefab;
    public GameObject boardPrefab;

    // List of smoothed path points
    private List<Vector3> path = new List<Vector3>();

    // Other scripts
    private SceneManagement sceneManagerScript;

    // HyperParameters 
    private float coinInterval = 0.4f;


    // for position calculation
    float totalTime = 0;
    float totalDis = 0, curDis = 0, prevTime = 0;
    int curPoint = 0;
    void Awake() {
        GlobalSettings.slideControl = this;
        LoadLevelData();
        SetPath();
        GenerateLevelObjects();
        Time.timeScale = 1f;
    }

    void LoadLevelData() { // Load and parse JSON file
        data = GlobalSettings.levelData.levels[GlobalSettings.level];
        foreach (var levelEvent in data.level) totalTime += levelEvent.eventLength;
    }

    void SetPath() { // averaging neighboring points
        path.Clear();
        for (int i = 0; i < data.path.Length; i++) {
            path.Add(new Vector3(data.path[i].x, 0, data.path[i].z));
            if (i>0) totalDis += Vector3.Distance(path[i], path[i - 1]);
        }
    }

    public Vector3 getPosition(float curTime) { // make sure that the ratio can only be bigger not smaller
        curTime = Mathf.Min(totalTime, Mathf.Max(0f, curTime));
        //assert(curTime >= prevTime);
        curTime = Mathf.Max(0f, curTime);
        float dis2Next = Vector3.Distance(path[curPoint], path[curPoint + 1]);
        float tmpDis = totalDis * (curTime / totalTime);
        while (curPoint + 1 < path.Count && tmpDis > curDis + dis2Next) {
            curDis += dis2Next;
            curPoint += 1;
            dis2Next = Vector3.Distance(path[curPoint], path[curPoint + 1]);
        }
        Vector3 pos = Vector3.Lerp(path[curPoint], path[curPoint + 1], (tmpDis - curDis)/dis2Next);
        return pos;
    }

    public Vector3 getForward(float curTime) { 
        curTime = Mathf.Min(totalTime, Mathf.Max(0f, curTime));
        float dis2Next = Vector3.Distance(path[curPoint], path[curPoint + 1]);
        float tmpDis = totalDis * (curTime / totalTime);
        while (curPoint + 1 < path.Count && tmpDis > curDis + dis2Next) {
            curDis += dis2Next;
            curPoint += 1;
            dis2Next = Vector3.Distance(path[curPoint], path[curPoint + 1]);
        }
        return (path[curPoint + 1] - path[curPoint]).normalized;
    }

    void GenerateLevelObjects() { // Generate level objects based on events

        float curTime = 0;
        foreach (var levelEvent in data.level) {
            if (levelEvent.eventType == 1) CreateHoop(GlobalSettings.key2height(levelEvent.startKey), curTime, levelEvent.startKey); 
            else if (levelEvent.eventType == 2) CreateCoins(
                GlobalSettings.key2height(levelEvent.startKey), 
                GlobalSettings.key2height(levelEvent.endKey), 
                curTime, levelEvent.eventLength
            );
            else if (levelEvent.eventType == 3) {
                GlobalSettings.changeTime = curTime + GlobalSettings.curBeat();
                CreateBoard(GlobalSettings.key2height(levelEvent.startKey), curTime);
            }
            else ;
            curTime += levelEvent.eventLength;
        }
    }

    void CreateBoard(float height, float time) { // Create a hoop at the specified key point
        Vector3 position = getPosition(time);
        position.y = height - 0.5f;
        Quaternion lookRotation = Quaternion.LookRotation(getForward(time));
        //lookRotation *= Quaternion.Euler(90f, 0f, 0f);
        Instantiate(boardPrefab, position, lookRotation, parent.transform);
    }
    void CreateHoop(float height, float time, int midiNote) {
        Vector3 position = getPosition(time);
        position.y = height;

        Quaternion lookRotation = Quaternion.LookRotation(getForward(time));
        lookRotation *= Quaternion.Euler(90f, 0f, 0f);

        GameObject createdHoop = Instantiate(hoopPrefab, position, lookRotation, parent.transform);
        Vector3 tmpPos = createdHoop.transform.position;
        createdHoop.transform.position = tmpPos;
        // - createdHoop.transform.right.normalized * 13 + createdHoop.transform.forward.normalized * 13;
        HoopControl hoopControl = createdHoop.transform.Find("Hoop").GetComponent<HoopControl>();
        if (hoopControl != null) {
            hoopControl.midiNote = midiNote;
        } else {
            Debug.LogWarning("HoopControl script not found on the instantiated hoop prefab.");
        }
    }

    void CreateCoins(float startHeight, float endHeight, float startTime, float length) {
        int cnt = (int)Math.Floor(length / coinInterval);
        for (int i = 0; i < cnt; i ++) {
            Vector3 pos = getPosition(startTime + i * coinInterval);
            pos.y = startHeight + (endHeight - startHeight) * ((float) i / cnt);
            Quaternion lookRotation = Quaternion.LookRotation(getForward(startTime + i * coinInterval));
            lookRotation *= Quaternion.Euler(90f, 0f, 0f);
            Instantiate(coinPrefab, pos, lookRotation, parent.transform);
        }
    }

    // Visualize the path in the editor using Gizmos
    void OnDrawGizmos() {
        if (path != null && path.Count > 1) {
            Gizmos.color = Color.green;
            for (int i = 0; i < path.Count - 1; i++) {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
        }
    }

    // Optionally, visualize the path when the object is selected in the editor
    void OnDrawGizmosSelected() {
        OnDrawGizmos();
    }
}
