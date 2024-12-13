using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    public GameObject board;       // 箱子所屬的板子
    public float destroyHeight = -5f; // 箱子低於該高度後消失
    public float boardMargin = 1f;  // 允許的邊界範圍
    public GameObject box;

    void Update()
    {
        Transform child = box.transform.Find("default");
        //UnityEngine.Debug.Log("box" + destroyHeight);
        // 檢查箱子的 y 軸高度
        if (child.transform.position.y < destroyHeight)
        {
            //UnityEngine.Debug.Log("boxdestroy");
            Board.count_box--;
            Destroy(box);
            return;
        }

        // 檢查箱子的 x 軸是否超出板子範圍
        if (board != null)
        {
            float boardLength = board.transform.localScale.x;
            float boardCenter = board.transform.position.x;

            // 算出板子的邊界
            float leftBoundary = boardCenter - boardLength / 2 - boardMargin;
            float rightBoundary = boardCenter + boardLength / 2 + boardMargin;

            if (transform.position.x < leftBoundary || transform.position.x > rightBoundary)
            {
                Destroy(gameObject);
            }
        }
    }
}
