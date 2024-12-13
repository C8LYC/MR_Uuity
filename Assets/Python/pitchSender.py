import sounddevice as sd
import numpy as np
import aubio
import socket

# UDP Configuration
UDP_IP = "127.0.0.1"  # Replace with Unity's IP if needed
UDP_PORT = 5005        # Match Unity's port for pitch detection
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Set up audio parameters
SAMPLE_RATE = 44100  # Sample rate for the microphone input
BUFFER_SIZE = 512  # Number of frames per buffer (buffer size)
HOP_SIZE = 512  # Number of frames between pitch calculations

# Initialize Aubio's pitch detection
pitch_detector = aubio.pitch("default", BUFFER_SIZE, HOP_SIZE, SAMPLE_RATE)
pitch_detector.set_unit("Hz")
pitch_detector.set_silence(-40)  # Threshold to ignore low energy (silence)

def callback(indata, frames, time, status):
    """Audio callback function to process incoming audio in real-time."""
    if status:
        print(f"Audio Input Status: {status}")

    # Convert audio input to mono (average across channels if stereo)
    audio_data = np.mean(indata, axis=1)

    # Use Aubio's pitch detector
    pitch = pitch_detector(audio_data.astype(np.float32))[0]

    # Print and send the detected pitch frequency if above a certain threshold
    if pitch > 0:
        message = f"{pitch:.2f}"
        print(message)
        sock.sendto(message.encode(), (UDP_IP, UDP_PORT))

def main():
    # Run the audio stream and handle KeyboardInterrupt to stop gracefully
    try:
        print(f"Sending pitch detection results to {UDP_IP}:{UDP_PORT}. Press Ctrl+C to stop.")
        with sd.InputStream(callback=callback,
                            channels=1,
                            samplerate=SAMPLE_RATE,
                            blocksize=BUFFER_SIZE):
            while True:
                pass
    except KeyboardInterrupt:
        print("\nStopped by user.")
    except Exception as e:
        print(f"Error: {e}")
    finally:
        sock.close()

if __name__ == "__main__":
    main()
