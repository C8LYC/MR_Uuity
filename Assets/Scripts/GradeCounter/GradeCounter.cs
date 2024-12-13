using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GradeCounter : MonoBehaviour
{
    public int SeedPerDandelion = 50;
    int AliveDandelion = 0;
    public TextMeshProUGUI GradeText;
    bool ShowGradeProcess = false;
    int CurrentShowGrade = 0;
    float Timer = 0.05f;
    public float TotalGradeAnimTime = 0.5f;
    public float AnimChangeTimes = 20f;
    float TimePerAnimChange;
    int CurAnimTimes = 0;

	private void Update()
	{
		if(ShowGradeProcess)
        {
            TimePerAnimChange = TotalGradeAnimTime / AnimChangeTimes;

			if (GradeText != null)
            {
				Timer -= Time.deltaTime;
                if(Timer < 0)
                {
                    Timer = TimePerAnimChange;
                    CurAnimTimes++;
                    if(CurAnimTimes < AnimChangeTimes)
                    {
						CurrentShowGrade = GetGrade() / (int) AnimChangeTimes * CurAnimTimes;
					}
                    else
                    {
                        CurrentShowGrade = GetGrade();
						ShowGradeProcess = false;
					}
					GradeText.text = "" + CurrentShowGrade;
				}
			}
        }
	}

	public void NewDandelion()
    {
        Debug.Log("NewDandelion");
        AliveDandelion++;
	}

    public void AllDandelionDead()
    {
        Debug.Log("AllDandelionDead");
        //AliveDandelion = 0;
	}

    public int GetGrade()
    {
        return AliveDandelion * SeedPerDandelion;
    }

    public void SetGradeUI()
    {
		GradeText.text = "0";
        CurrentShowGrade = 0;
        CurAnimTimes = 0;
		ShowGradeProcess = true;
	}
}
