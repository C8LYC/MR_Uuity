using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;


[System.Serializable]
public class NoteEvent
{
    public int note;       // 音高
    public float time;     // 開始時間
    public float duration; // 持續時間
}

[System.Serializable]
public class NoteEventList
{
    public List<NoteEvent> notes;
}

public class Board : MonoBehaviour
{
    public GameObject boardPrefab;  // 板子的預製件
    public GameObject boxPrefab;    // 箱子的預製件
    public GameObject checkpointPrefab; // Checkpoint 的預製件
    public Transform spawnParent;  // 板子生成的父物件
    public string jsonFilePath;    // MIDI 文件路徑

    [Header("Height Configuration")]
    public float groundLevel = 0f;  // 地面高度
    public float heightOffset = 0f; // 高度偏移量
    public float ceilingLevel = 40f; // 天花板高度
    public int H_range_Midi = 80;   // 最高音對應的 MIDI 碼
    public int L_range_Midi = 53;   // 最低音對應的 MIDI 碼

    [Header("Horizontal Range")]
    public float left = 0f;  // 最左邊的位置
    public float right = 50f;  // 最右邊的位置

    [Header("Board Configuration")]
    //public float baseLength = 1f;  // 最短音符對應的板子長度
    //public float spacing = 3f;   // 板子之間的最小間距
    public float boxOffset = 2f; // 箱子距離板子最左邊的距離

    [Header("Flight Configuration")]
    public GameObject airplane; // 飛機物件
    public float flightSpeed = 3; // 飛行速度

    private List<Vector3> flightPath; // 保存飛行路徑
    private int currentTargetIndex = 0; // 當前目標點索引

    public Transform propeller;       // Reference to the propeller Transform
    public float maxSpinSpeed = 10000f; // Maximum spin speed for the propeller
    //private bool trillState = false;  // Lip trill intensity (0 to 1)
    public bool isCrashed = false;

    [Header("UI Elements")]
    public Text gameOverText;
    public Text gameWonText;

    public static int count_box = 0;
    private int boardRotationIndex = 0;
    private Vector3 prePosition;
    private bool start = false;

    void Start()
    {
        //flightSpeed = Slime.dyn_speed;
        //flightPath = new List<Vector3>();
        Slime.slimePath = new List<Vector3>();
        LoadAndGenerateBoards(jsonFilePath);

        //if (flightPath.Count > 0)
        //{
        //    airplane.transform.position = new Vector3(-25, flightPath[0].y, 0);
        //}
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger: " + other.tag);
        if (other.tag == "Box")
        {
            Debug.Log("crash");
            Crash();
        }
        else if (other.tag == "Checkpoint")
        {
            Debug.Log("Checkpoint reached!");
            GameWon();
        }

    }

    string GetNoteName(int midiNote)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int octave = (midiNote / 12) - 1;
        int noteIndex = midiNote % 12;
        return $"{noteNames[noteIndex]}{octave}";
    }

    void LoadAndGenerateBoards(string filePath)
    {
        string jsonContent = File.ReadAllText(filePath);
        NoteEventList noteEventList = JsonUtility.FromJson<NoteEventList>("{\"notes\":" + jsonContent + "}");

        // 生成板子
        float levelWidth = right - left;
        float horizontalStep = Mathf.Max(0.5f, levelWidth / (noteEventList.notes.Count - 1));
        float currentX = left;
        float currentZ = 0; // 控制 Z 轴偏移
        Vector3 targetPoint;
        foreach (var note in noteEventList.notes)
        {
            float height = CalculateHeight(note.note);
            float length = CalculateLength(note.duration);

            // 动态调整位置
            Vector3 boardPosition = new Vector3(currentX, height, currentZ);


            GameObject currentBoard = SpawnBoard(boardPosition, length, note.note);
            // 如果存在前一个板子，计算目标点（中心交汇点）
            if (start)
            {
                // 计算两点的中心交点

                if (boardRotationIndex == 0 || boardRotationIndex == 2)
                {
                    targetPoint = new Vector3(currentBoard.transform.position.x, height, prePosition.z);
                    prePosition = currentBoard.transform.position;
                }
                else
                {

                    targetPoint = new Vector3(prePosition.x, height, currentBoard.transform.position.z);
                    prePosition = currentBoard.transform.position;
                }
                Slime.slimePath.Add(targetPoint);

                UnityEngine.Debug.Log("Add target point: " + targetPoint);
            }
            else
            {
                prePosition = boardPosition;
                GameCon.firstBoard = currentBoard.transform;
            }

            //SpawnBoard(boardPosition, length, note.note);

            // 更新位置偏移
            currentX += horizontalStep;
            currentZ += horizontalStep; // 每次在 Z 轴增加偏移量

            start = true;
        }

        if (boardRotationIndex == 0 || boardRotationIndex == 2)
        {
            targetPoint = new Vector3(prePosition.x + 20f, 0, prePosition.z);
        }
        else
        {

            targetPoint = new Vector3(prePosition.x + 20f, 0, prePosition.z);
        }
        Slime.slimePath.Add(targetPoint);

        UnityEngine.Debug.Log("Add target point: " + targetPoint);
    }

    float CalculateHeight(int midiNote)
    {
        return groundLevel + heightOffset +
               (ceilingLevel - groundLevel - heightOffset) *
               (midiNote - L_range_Midi) / (H_range_Midi - L_range_Midi);
    }

    float CalculateLength(float duration)
    {
        return duration * 15f; // 將持續時間轉換為板子長度
    }

    GameObject SpawnBoard(Vector3 position, float length, int note)
    {
        // 计算旋转角度
        Quaternion rotation = Quaternion.Euler(0, 90 * boardRotationIndex, 0);

        // 生成板子
        GameObject board = Instantiate(boardPrefab, position, rotation, spawnParent);
        Vector3 newScale = new Vector3(
            length,
            board.transform.localScale.y,
            board.transform.localScale.z
        );
        float lengthChange = length - board.transform.localScale.x; // 长度的变化量
        Vector3 positionOffset = new Vector3(lengthChange / 2, 0, 0); // 偏移位置，沿 x 轴前移

        // 应用缩放
        board.transform.localScale = newScale;

        // 调整位置
        board.transform.localPosition += positionOffset;

        // 生成箱子
        SpawnBox(board, length);

        GenerateNoteName(board, position, note);

        // 更新旋转状态索引
        boardRotationIndex = (boardRotationIndex + 1) % 4; // 循环变化：0, 1, 2, 3

        return board;

    }

    void SpawnBox(GameObject board, float length)
    {
        if (boxPrefab == null)
        {
            Debug.LogWarning("Box prefab is not assigned!");
            return;
        }
        Vector3 boardStartPosition;
        Vector3 boxPosition;
        if (boardRotationIndex == 1 || boardRotationIndex == 2)
        {
            boardStartPosition = board.transform.position + board.transform.right * (length / 2);
            boxPosition = boardStartPosition - board.transform.right * boxOffset; // 起始点向右偏移 boxOffset 距离
        }
        else
        {
            boardStartPosition = board.transform.position - board.transform.right * (length / 2);
            boxPosition = boardStartPosition + board.transform.right * boxOffset; // 起始点向右偏移 boxOffset 距离
        }
        // 获取板子的起始点
        //Vector3 boardStartPosition = board.transform.position - board.transform.right * (length / 2);

        // 计算箱子的位置
        //Vector3 boxPosition = boardStartPosition + board.transform.right * boxOffset; // 起始点向右偏移 boxOffset 距离
        boxPosition.y = board.transform.position.y + 0.5f; // 稍微抬高以便可见

        // 生成箱子
        GameObject box = Instantiate(boxPrefab, boxPosition, Quaternion.identity, spawnParent);
        count_box++;

        // 确保箱子为板子的子物件
        box.transform.SetParent(board.transform);

        // 添加 BoxController 并设置属性
        BoxController boxController = box.AddComponent<BoxController>();
        boxController.board = board; // 关联箱子的板子
        boxController.destroyHeight = board.transform.position.y - 5f; // 设置最低销毁高度
        boxController.boardMargin = 1f; // 设置板子边界范围
        boxController.box = box;


    }

    void GenerateNoteName(GameObject board, Vector3 position, int note)
    {
        // 計算音階名稱
        string noteName = GetNoteName(Mathf.RoundToInt(note));

        // 創建一個 TextMesh 物件
        GameObject textObject = new GameObject("NoteName");
        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = noteName;
        textMesh.fontSize = 12;
        textMesh.color = Color.black;
        textMesh.alignment = TextAlignment.Center;

        // 設置文字位置（位於板子上方）
        textObject.transform.position = new Vector3(position.x, position.y + 2f, position.z);
        textObject.transform.SetParent(board.transform); // 將文字作為板子的子物件
    }


    private void Crash()
    {
        isCrashed = true;
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }
        // 停止飛機移動
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // 啟用物理效果
            rb.AddForce(Vector3.down * 10f, ForceMode.Impulse); // 模擬墜落
        }
    }
    void GameWon()
    {
        if (gameWonText != null) gameWonText.gameObject.SetActive(true);
    }
}

