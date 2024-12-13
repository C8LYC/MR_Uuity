using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateObj : MonoBehaviour
{
    public GameObject GenerateObjPrefab;
    List<GameObject> objs = new List<GameObject>();
    public BoxCollider GenerateArea;
	public bool TestGenerate = true;
	public float MaxGenerateTime = 3f;
	public GameObject BoldDandelionPrefab;
	float GenerateTime = 3f;
	GradeCounter gradeCounter;

	private void Start()
	{
		gradeCounter = GetComponent<GradeCounter>();
	}

	// Update is called once per frame
	void Update()
    {
		if(TestGenerate)
		{
			GenerateTime -= Time.deltaTime;
			if (GenerateTime < 0f)
			{
				GenerateOneObj();
				GenerateTime = MaxGenerateTime;
			}
		}
	}

    public void GenerateOneObj()
    {
        GameObject newObj = Instantiate(GenerateObjPrefab, GenerateArea.transform);
        newObj.transform.parent = null;
        newObj.transform.localScale = Vector3.one;
        objs.Add(newObj);
        newObj.transform.localPosition = RandomPointInBounds(GenerateArea.bounds);
        newObj.GetComponentInChildren<SizeLerperWithCurve>().startLerp = true;
		if(gradeCounter)
		{
			gradeCounter.NewDandelion();
		}
    }
	
	public void GenerateOneBoldDandelion()
    {
        GameObject newObj = Instantiate(BoldDandelionPrefab, GenerateArea.transform);
        newObj.transform.parent = null;
        newObj.transform.localScale = Vector3.one;
        objs.Add(newObj);
        newObj.transform.localPosition = RandomPointInBounds(GenerateArea.bounds);
        newObj.GetComponentInChildren<SizeLerperWithCurve>().startLerp = true;
		if(gradeCounter)
		{
			gradeCounter.NewDandelion();
		}
    }

	public static Vector3 RandomPointInBounds(Bounds bounds)
	{
		return new Vector3(
			Random.Range(bounds.min.x, bounds.max.x),
			Random.Range(bounds.min.y, bounds.max.y),
			Random.Range(bounds.min.z, bounds.max.z)
		);
	}
}
