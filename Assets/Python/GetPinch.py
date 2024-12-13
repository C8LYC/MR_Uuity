import sounddevice as sd
import numpy as np
import aubio
import UDP

# Set up audio parameters
SAMPLE_RATE = 44100  # Sample rate for the microphone input
BUFFER_SIZE = 512  # Number of frames per buffer (buffer size)
HOP_SIZE = 512  # Number of frames between pitch calculations

# UPD
PORT = 5005

# Initialize Aubio's pitch detection
pitch_detector = aubio.pitch("default", BUFFER_SIZE, HOP_SIZE, SAMPLE_RATE)
pitch_detector.set_unit("Hz")
pitch_detector.set_silence(-50)  # Threshold to ignore low energy (silence)


def callback(indata, frames, time, status):
    """Audio callback function to process incoming audio in real-time."""
    if status:
        print(status)

    # Convert audio input to mono (average across channels if stereo)
    audio_data = np.mean(indata, axis=1)

    # Use Aubio's pitch detector
    pitch = pitch_detector(audio_data.astype(np.float32))[0]

    # Print the detected pitch frequency if above a certain threshold
    if pitch > 0 and pitch < 840:
        # UDP.send_message(f"Detected pitch: {pitch:.2f} Hz")
        print(f"{pitch:.2f}")
        UDP.send_message(f"{pitch:.2f}", PORT)


def main():
    # Run the audio stream and handle KeyboardInterrupt to stop gracefully
    try:
        with sd.InputStream(callback=callback,
                            channels=1,
                            samplerate=SAMPLE_RATE,
                            blocksize=BUFFER_SIZE):
            print("Listening for pitch... Press Ctrl+C to stop.")
            while True:
                sd.sleep(
                    100)  # Short sleep to allow checking for KeyboardInterrupt
    except KeyboardInterrupt:
        UDP.close_connection()
        print("\nStopping the program.")
    except Exception as e:
        UDP.close_connection()
        print(f"An error occurred: {e}")


def getPinch():
    return main()


if __name__ == "__main__":
    main()
