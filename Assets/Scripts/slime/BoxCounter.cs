using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxCounter : MonoBehaviour
{
    public Text boxCountText; // 引用 UI Text 元素
    private int box = 0;
    private int prebox = 0;

    void Start()
    {
        UpdateBoxCountUI();
    }

    private void Update()
    {
        box = Board.count_box;
        if (box != prebox) UpdateBoxCountUI();
        prebox = box;
    }

    void UpdateBoxCountUI()
    {
        //GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        int boxCount = Board.count_box;
        if (boxCountText != null)
        {
            boxCountText.text = $"Remaining Boxes: {boxCount}";
        }
    }

    public void DestroyBox(GameObject box)
    {
        Destroy(box);
        UpdateBoxCountUI();
    }
}
