using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCon : MonoBehaviour
{
    public GameObject startButton; // 按钮对象
    public Text countdownText;     // 倒计时的文本

    private bool gameStarted = false;
    private bool gameEnded = false;
    public Text endText;
    public GameObject slime;


    //public GameObject startButton;        // UI按钮
    public GameObject boxPrefab;         // 阶梯箱子预制体
    //public GameObject slime;             // Slime对象
    //public Transform groundStartPoint;   // 地板起始点
    public static Transform firstBoard;         // 第一块板子的 Transform
    public float stepSize = 1f;          // 每级阶梯的高度增量
    public float stepDelay = 0.1f;       // 每级阶梯生成延迟
    public float slimeSpeed = 2f;        // Slime移动速度
    //public Text countdownText;           // 倒计时文本
    //public Text winText;                 // 游戏胜利或失败文本
    public float groundHeight = 0f;
    public float stepHorizontalSpacing = 2f; // 每级阶梯的水平间隔
    public float stepHeight = 1f;         // 每级阶梯的高度增量
    private List<Transform> stairs = new List<Transform>(); // 保存所有生成的阶梯
    //private bool gameStarted = false;
    //private Animator slimeAnimator; // 角色的 Animator


    void Start()
    {
        // 确保游戏开始前按钮可见，倒计时文本隐藏
        startButton.SetActive(true);
        countdownText.gameObject.SetActive(false);
        GameObject targetObject = GameObject.Find("Player");
        GlobalSettings.midiNotePlayer = targetObject.GetComponent<MidiNotePlayer>();


    }

    void Update()
    {
        if (!gameEnded)
        {
            if (slime.transform.position.x > Slime.slimePath[4].x)
            {
                // 获取场景中剩余的箱子数量
                if (Board.count_box == 0)
                {
                    EndGame("You Win!");
                }
                else
                {
                    EndGame("Game Over!");
                }
            }

        }
    }

    public void StartGame()
    {
        // 禁用按钮并开始倒计时
        startButton.SetActive(false);
        StartCoroutine(BuildStaircaseAndAnimateSlime());
        //StartCoroutine(CountdownToStart());
    }

    private IEnumerator CountdownToStart()
    {
        
        Debug.Log("CountDoenToStart");
        GlobalSettings.midiNotePlayer.PlayNoteForDuration(60, 3);
        countdownText.gameObject.SetActive(true);

        int countdown = 3; // 倒计时时间
        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1);
            countdown--;
        }

        // 倒计时结束
        countdownText.text = "Go!";
        yield return new WaitForSeconds(1);
        countdownText.gameObject.SetActive(false);

        // 开始游戏逻辑
        gameStarted = true;
        Debug.Log("Game Started!");
        Slime.GameStart = true;
    }

    void Win()
    {
        EndGame("You Win!");
    }

    void GameOver()
    {
        EndGame("Game Over!");
    }

    void EndGame(string message)
    {
        gameEnded = true;

        // 显示结束文本
        if (endText != null)
        {
            endText.gameObject.SetActive(true);
            endText.text = message;
        }

        // 暂停游戏
        Time.timeScale = 0f;
    }

    private IEnumerator BuildStaircaseAndAnimateSlime()
    {
        // 计算初始位置和目标位置
        Vector3 startPosition = new Vector3(firstBoard.position.x - stepHorizontalSpacing * (firstBoard.position.y - groundHeight)/stepHeight - 15, groundHeight, firstBoard.position.z);
        Vector3 targetPosition = firstBoard.position;

        // 生成阶梯
        Vector3 currentPosition = startPosition;

        while (currentPosition.y < targetPosition.y)
        {
            GameObject step = Instantiate(boxPrefab, currentPosition, Quaternion.identity);
            stairs.Add(step.transform);
            // 水平方向移动为 x 轴方向，增加 stepHorizontalSpacing
            currentPosition += new Vector3(stepHorizontalSpacing, stepHeight, 0);
            yield return new WaitForSeconds(stepDelay);
        }

        // 确保最后一个阶梯对齐第一块板子
        //GameObject finalStep = Instantiate(boxPrefab, targetPosition, Quaternion.identity);
        //stairs.Add(finalStep.transform);

        // Slime 爬上阶梯
        yield return StartCoroutine(SlimeClimbStairs());
        
    }


    private IEnumerator SlimeClimbStairs()
    {
        // 确保 Slime 从起点开始
        slime.transform.position = stairs[0].position + new Vector3(0, 0.5f, 0); // 稍微抬高，避免卡住

        // 获取 Slime 的 Animator 组件
        Animator slimeAnimator = slime.GetComponent<Animator>();

        foreach (Transform step in stairs)
        {
            // 计算目标位置
            Vector3 targetPosition = step.position + new Vector3(0, 2f, 0); // 确保在阶梯顶部

            // 播放 Jump 动画
            if (slimeAnimator != null)
            {
                slimeAnimator.speed = slimeSpeed; // Adjust speed; modify divisor for desired effect

                //slimeAnimator.SetBool("jump", true);
                //slimeAnimator.SetBool("jump", false);
                slimeAnimator.SetTrigger("jump");
                Debug.Log("jump");
            }

            // 开始跳跃
            yield return StartCoroutine(JumpToPosition(slime.transform, targetPosition, 1.5f)); // 跳跃幅度调整为 1.5f
        }
        if (slimeAnimator != null)
        {
            slimeAnimator.speed = 1f; // Reset to default
        }
        // 动画结束后，可以启动游戏逻辑
        Debug.Log("Animation finished, start the game!");
        slimeAnimator.SetBool("jump", false);
        yield return StartCoroutine(CountdownToStart());
    }

    private IEnumerator JumpToPosition(Transform objectToMove, Vector3 targetPosition, float jumpHeight)
    {
        Vector3 startPosition = objectToMove.position;

        // 跳跃时间控制
        float jumpDuration = 0.5f; // 跳跃持续时间
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            // 计算跳跃进度（0 到 1）
            float progress = elapsedTime / jumpDuration;

            // 计算当前高度（使用抛物线公式）
            float heightOffset = jumpHeight * 4 * progress * (1 - progress);

            // 更新 Slime 的位置
            objectToMove.position = Vector3.Lerp(startPosition, targetPosition, progress) + new Vector3(0, heightOffset, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保跳跃结束时完全对齐目标位置
        objectToMove.position = targetPosition;
    }
}