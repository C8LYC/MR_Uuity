using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemController : MonoBehaviour {
    public new GameObject camera;
    public float amplitude;
    public float pitch;
    public bool started;
    public bool paused;
    public bool canReset;
    public bool finished;
	public Vector3 PlayerStartPos { get; set; }
    public Transform PlayerTransform;

	// Pitch
	private int targetMidiNumber;
    public float targetPitch;
    public float minPitch;
    public float maxPitch;
    public float minDisplayPitch;
    public float maxDisplayPitch;
    // private bool isInNormalPitch;

    public GameObject dandelionGeneratorObject;
    private GenerateObj dandelionGenerator;
    public GameObject swayControllerObject;
    private SwayController[] swayControllers;
    private Vector3 originalRotationOffset;
    public float swayOffset;

    private float originalSwaySpeed = 1f;

    // timer
    public float TIMER;
    private float timeRemaining;
    public float PAUSE_TIMER;
    private float timeRemainingToRestart;
    public float CREATE_TIMER;
    private float timeRemainingToCreate;

    // amplitude range
    public float MIN_AMPLIFY;
    // public float MAX_AMPLIFY;

    // Start is called before the first frame update
    void Start() {
        started = false;
        paused = false;
        canReset = false;
        finished = false;

        setNewNote();
        // isInNormalPitch = false;

        TIMER = 60f;
        timeRemaining = TIMER;
        PAUSE_TIMER = 2f;
        timeRemainingToRestart = 0f;
        CREATE_TIMER = 0.5f;
        timeRemainingToCreate = CREATE_TIMER;

        MIN_AMPLIFY = 0.4f;
        // MAX_AMPLIFY = 0.6f;

        dandelionGenerator = dandelionGeneratorObject.GetComponent<GenerateObj>();
        /*
        swayControllers = swayControllerObject.GetComponents<SwayController>();
        foreach (SwayController swayController in swayControllers) {
            originalRotationOffset = swayController.rotationOffset;
            originalSwaySpeed = swayController.swaySpeed;
        }
        */
        swayOffset = 1f;
    }

    // Update is called once per frame
    void Update() {
        // if (finished) {
        //     return;
        // }
        // swayOffset = 0.5f + (amplitude * 5f);
        // foreach (SwayController swayController in swayControllers) {
        //     swayController.rotationOffset = originalRotationOffset * swayOffset;
        //     swayController.swaySpeed = originalSwaySpeed * swayOffset;
        // }

        if (!started || finished) {
            return;
        }
        if (isPaused()) {
            timeRemainingToRestart = PAUSE_TIMER;
            timeRemainingToCreate = CREATE_TIMER;
            if (canReset) {
                setNewNote();
                canReset = false;
            }
        }

        timeRemaining -= Time.deltaTime;
        timeRemainingToRestart -= Time.deltaTime;
        if (timeRemainingToRestart < 0f) {
            canReset = true;
            return;
        }

        timeRemainingToCreate -= Time.deltaTime;
        if (timeRemainingToCreate <= 0f) {
            if (isWithinRange()) {
                dandelionGenerator.GenerateOneObj(this);
            } else {
                dandelionGenerator.GenerateOneBoldDandelion();
            }
            timeRemainingToCreate = CREATE_TIMER;
        }

        if (timeRemaining <= 0f) {
            finished = true;
        }
    }

    void setNewNote() {
        targetMidiNumber = getRandomMidiNumber();
        playNote();
    }

    int getRandomMidiNumber() {
        int[] pitchChoices = new int[] {60, 62, 64, 65, 67, 69, 71, 72};
        int targetMidiNumber = pitchChoices[Random.Range(0, pitchChoices.Length)];
        targetPitch = getFrequency(targetMidiNumber);
        minPitch = getFrequency(targetMidiNumber-1);
        maxPitch = getFrequency(targetMidiNumber+1);
        minDisplayPitch = getFrequency(targetMidiNumber-3);
        maxDisplayPitch = getFrequency(targetMidiNumber+3);

        return targetMidiNumber;
    }

    float getFrequency(int midiNumber) {
        return (Mathf.Pow(2, (midiNumber - 69) / 12f) * 440f);
    }

    int getMidiNumber(float frequency) {
        return (69 + 12 * Mathf.RoundToInt(Mathf.Log(frequency / 440.0f, 2)));
    }

    public string getNoteNameNow() {
        string[] noteNames = {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };
        int octave = (targetMidiNumber / 12) - 1;
        string note = noteNames[targetMidiNumber % 12] + octave;

        return note;
    }

    void playNote() {
        camera.GetComponent<MidiNotePlayer>().PlayNoteForDuration(targetMidiNumber, 1f);
    }

    public bool isPaused() {
        return (amplitude <= MIN_AMPLIFY);
    }

    public bool isWithinRange() {
        return !(isPaused()) && (pitch >= minPitch && pitch <= maxPitch);
    }
}
