using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmplifySliderController : MonoBehaviour
{
    public List<Sprite> CDSprite;
    public Image CDTextImage;
    public Image CDBGImage;
    public GradeCounter gradeCounter;
    private Text countDownText;
    public GameObject scale;
    [SerializeField] private GameObject scaleMarkPrefab;
    public bool isScrolling;
    public float ScrollingShift;
    private SystemController systemController;

    private RectTransform actualAmplifyRectTransform;
    private float MAX_Y;
    private float MIN_Y;
    private float OFFSET;

    // timer
    private float TIMER;
    private float timeRemaining;
    private float SCROLL_TIMER;
    private float scrollTimeRemaining;

    // Start is called before the first frame update
    void Start() {
        countDownText = gameObject.GetComponentInChildren<Text>();
        actualAmplifyRectTransform = gameObject.transform.GetChild(1).gameObject.GetComponent<RectTransform>();
        systemController = GameObject.Find("System").GetComponent<SystemController>();
        systemController.amplitude = 0.0f;
        
        isScrolling = false;
        ScrollingShift = 200f;

        MAX_Y = 240;
        MIN_Y = -240;
        OFFSET = MAX_Y - MIN_Y;

        TIMER = 3.2f;
        timeRemaining = TIMER;
        SCROLL_TIMER = 0.8f;
        scrollTimeRemaining = SCROLL_TIMER;

        InitGenerateScale();
    }

    // Update is called once per frame
    void Update() {
        float posY = (systemController.amplitude * OFFSET) + MIN_Y;
        actualAmplifyRectTransform.anchoredPosition = new Vector2(0, posY);

        if (systemController.isWithinRange()) {
            timeRemaining -= Time.deltaTime;
            countDownText.text = "" + (int)timeRemaining;
            CDTextImage.sprite = CDSprite[Mathf.Min((int) timeRemaining + 1, CDSprite.Count - 1)];
			CDTextImage.color = new Color(255f, 255f, 255f);
            CDBGImage.fillAmount = timeRemaining - (int) timeRemaining;
		} else {
            timeRemaining = TIMER;
            countDownText.text = "";
			CDTextImage.sprite = null;
            CDTextImage.color = new Color(255f, 255f, 255f, 0f);
            CDBGImage.fillAmount = 0f;
            gradeCounter.AllDandelionDead();
		}

        if (timeRemaining <= 0f) {
            gameObject.SetActive(false);
            systemController.started = true;
        }

        if (isScrolling) {
            Scroll(ScrollingShift);
        }
    }

    public void StartScroll(float shift) {
        isScrolling = true;
        ScrollingShift = shift;
    }

    void InitGenerateScale() {
        for (float posY = 241.5f; posY >= -241.5f; posY -= 28f) {
            GenerateScale(posY);
        }
    }

    void GenerateScale(float posY) {
        GameObject newScaleMark = Instantiate(scaleMarkPrefab);
        newScaleMark.transform.parent = scale.transform;
        newScaleMark.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.5f, posY);
    }

    void ReSetScroll() {
        scrollTimeRemaining = SCROLL_TIMER;
    }

    void Scroll(float shift) {
        if (scrollTimeRemaining >= 0f) {
            scrollTimeRemaining -= Time.deltaTime;
            float maxPosY = 0;
            float minPosY = 0;
            for (int i = 0; i < scale.transform.childCount; i++) {
                GameObject scaleMark = scale.transform.GetChild(i).gameObject;
                float posY = scaleMark.GetComponent<RectTransform>().anchoredPosition.y + (shift * Time.deltaTime / SCROLL_TIMER);
                if (posY > maxPosY) {
                    maxPosY = posY;
                }
                if (posY < minPosY) {
                    minPosY = posY;
                }
                if (posY < -241.5f || posY > 241.5f) {
                    Destroy(scaleMark);
                }
                scaleMark.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.5f, posY);
            }
            for (; maxPosY <= 241.5f; maxPosY += 28f) {
                Debug.Log("maxPosY = " + maxPosY);
                GenerateScale(maxPosY);
            }
            for (; minPosY >= -241.5f; minPosY -= 28f) {
                Debug.Log("minPosY = " + minPosY);
                GenerateScale(minPosY);
            }
        } else {
            isScrolling = false;
            ReSetScroll();
        }
    }
}
