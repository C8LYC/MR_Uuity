using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MidiNotePlayer : MonoBehaviour {
    public TextMeshProUGUI noteText; // Assign a Text UI element in the Inspector
    private float frequency = 440f; // Default frequency (A4)
    private bool isPlaying = false; // Flag to control playback
    private int sampleRate = 44100; // Standard sample rate
    private double phase = 0.0; // Phase for sine wave generation

    private string[] noteNames = {
        "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };

    // Start playing a note based on the MIDI number
    public void StartNote(int midiNumber) {
        if (midiNumber < 0 || midiNumber > 127) {
            Debug.LogError("Invalid MIDI number. Must be between 0 and 127.");
            return;
        }

        // Calculate frequency
        frequency = Mathf.Pow(2, (midiNumber - 69) / 12f) * 440f;

        // Display the note
        int octave = (midiNumber / 12) - 1;
        string note = noteNames[midiNumber % 12] + octave;
        if (noteText != null) {
            noteText.text = "Playing: " + note + " (" + frequency.ToString("F2") + " Hz)";
        }

        isPlaying = true; // Start generating the sine wave
    }

    // Stop playing the note
    public void StopNote() {
        isPlaying = false; // Stop generating the sine wave
        if (noteText != null) {
            noteText.text = "Stopped";
        }
    }

    // Play a note for a fixed duration
    public void PlayNoteForDuration(int midiNumber, float duration) {
        StartNote(midiNumber);
        StartCoroutine(StopNoteAfterDuration(duration));
    }

    // Coroutine to stop the note after a duration
    private System.Collections.IEnumerator StopNoteAfterDuration(float duration) {
        yield return new WaitForSeconds(duration);
        StopNote();
    }

    // Generate audio samples dynamically
    private void OnAudioFilterRead(float[] data, int channels) {
        if (!isPlaying)
            return;

        double increment = frequency * 2.0 * Mathf.PI / sampleRate;

        for (int i = 0; i < data.Length; i += channels) {
            phase += increment;
            float sample = Mathf.Sin((float)phase);

            for (int channel = 0; channel < channels; channel++) {
                data[i + channel] = sample;
            }

            if (phase > Mathf.PI * 2)
                phase -= Mathf.PI * 2;
        }
    }

    // Test with keyboard input
    /*
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            StartNote(60); // Start Middle C
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            StopNote(); // Stop playing
        }
        if (Input.GetKeyDown(KeyCode.D)) {
            PlayNoteForDuration(62, 1.0f); // Play D4 for 1 second
        }
    }
    */
}
