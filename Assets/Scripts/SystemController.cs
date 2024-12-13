using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemController : MonoBehaviour {
    public float amplitude;
    public float pitch;
    public bool started;
    public bool finished;

    public GameObject dandelionGeneratorObject;
    private GenerateObj dandelionGenerator;
    public GameObject swayControllerObject;
    private SwayController[] swayControllers;
    private Vector3 originalRotationOffset;
    public float swayOffset;

    private float originalSwaySpeed = 1f;

    // timer
    public float TIMER;
    public float CREATE_TIMER;
    private float timeRemaining;
    private float timeRemainingToCreate;

    // amplitude range
    public float MIN_AMPLIFY;
    public float MAX_AMPLIFY;
    public float MAX_GAME_OVER_AMPLIFY;

    // Start is called before the first frame update
    void Start() {
        started = false;
        finished = false;

        TIMER = 60f;
        CREATE_TIMER = 0.5f;
        timeRemaining = TIMER;
        timeRemainingToCreate = CREATE_TIMER;

        MIN_AMPLIFY = 0.4f;
        MAX_AMPLIFY = 0.6f;
        MAX_GAME_OVER_AMPLIFY = 0.7f;

        dandelionGenerator = dandelionGeneratorObject.GetComponent<GenerateObj>();
        swayControllers = swayControllerObject.GetComponents<SwayController>();
        foreach (SwayController swayController in swayControllers) {
            originalRotationOffset = swayController.rotationOffset;
            originalSwaySpeed = swayController.swaySpeed;
        }
        swayOffset = 1f;
    }

    // Update is called once per frame
    void Update() {
        if (finished) {
            return;
        }

        swayOffset = 0.5f + (amplitude * 5f);
        foreach (SwayController swayController in swayControllers) {
            swayController.rotationOffset = originalRotationOffset * swayOffset;
            swayController.swaySpeed = originalSwaySpeed * swayOffset;
        }

        if (!started || finished) {
            return;
        }

        if (isWithinRange()) {
            timeRemaining -= Time.deltaTime;
            timeRemainingToCreate -= Time.deltaTime;
        } else {
            timeRemaining = TIMER;
            timeRemainingToCreate = CREATE_TIMER;
        }

        if (timeRemainingToCreate <= 0f) {
            dandelionGenerator.GenerateOneObj();
            timeRemainingToCreate = CREATE_TIMER;
        }

        if (timeRemaining <= 0f) {
            finished = true;
        }
    }

    public bool isWithinRange() {
        return (amplitude >= MIN_AMPLIFY && amplitude <= MAX_AMPLIFY);
    }
}
