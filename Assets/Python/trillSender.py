import sounddevice as sd
import numpy as np
import socket
from scipy.signal import hilbert, filtfilt, find_peaks, butter


# UDP Configuration
UDP_IP = "127.0.0.1"  # Replace with Unity's IP if needed
UDP_PORT = 5007       # Match Unity's port for trill detection
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# Audio Configuration
SAMPLE_RATE = 44100        # Standard audio sampling rate
DURATION = 0.4
WINDOW_SIZE = int(SAMPLE_RATE * DURATION)  # Buffer size for each audio frame (0.5 seconds)

# Low-pass filter design for envelope smoothing
CUTOFF_FREQ = 30  # Cutoff frequency in Hz
ORDER = 4
nyquist = DURATION * SAMPLE_RATE
normal_cutoff = CUTOFF_FREQ / nyquist
b, a = butter(ORDER, normal_cutoff, btype='low', analog=False)

# Thresholds
AMPLITUDE_THRESH = 0.02    # Minimum amplitude threshold
FREQ_LOW = 19             # Minimum frequency threshold (Hz)
FREQ_HIGH = 32             # Maximum frequency threshold (Hz)
PERIODICITY_THRESH = 0.015  # Maximum standard deviation for peak intervals to be periodic

# Buffer for the filtered envelope
filtered_envelope = np.zeros(WINDOW_SIZE)


def is_trilling(indata, sample_rate):
    """
    Detect whether the signal contains a trill based on amplitude envelope and periodicity.

    Args:
        indata (ndarray): Audio signal (NumPy array).
        sample_rate (int): Sampling rate of the audio.

    Returns:
        bool: True if trilling is detected, False otherwise.
    """
    global filtered_envelope

    # Compute the amplitude envelope using the Hilbert transform
    analytic_signal = hilbert(indata[:, 0])  # Use the first channel if stereo
    amplitude_envelope = np.abs(analytic_signal)

    # Apply low-pass filtering to smooth the envelope
    filtered_env = filtfilt(b, a, amplitude_envelope)

    # Update the buffer with the latest frames
    filtered_envelope[:-len(filtered_env)] = filtered_envelope[len(filtered_env):]
    filtered_envelope[-len(filtered_env):] = filtered_env

    # Check if the average amplitude exceeds the threshold
    avg_amplitude = filtered_env.mean()
    if avg_amplitude < AMPLITUDE_THRESH:
        return False  # Ignore frames with low amplitude

    # Detect peaks in the smoothed envelope
    peaks, _ = find_peaks(filtered_env, height=0)

    # Calculate the peak frequency
    if len(peaks) < 2:
        return False  # Not enough peaks to calculate frequency
    peak_freq = len(peaks) / (len(indata) / sample_rate)
    print(peak_freq)

    # Check if the peak frequency is within the target range
    if not (FREQ_LOW <= peak_freq <= FREQ_HIGH):
        return False

    # Calculate intervals between peaks to assess periodicity
    peak_intervals = np.diff(peaks) / sample_rate  # Intervals in seconds
    interval_std = np.std(peak_intervals)

    # Check periodicity based on the consistency of intervals
    is_periodic = interval_std < PERIODICITY_THRESH

    # Debugging output
    print(f"Frame Analysis:")
    print(f"  - Avg Amplitude: {avg_amplitude:.4f}")
    print(f"  - Peak Frequency: {peak_freq:.2f} Hz")
    print(f"  - Periodicity (Interval STD): {interval_std:.4f}")
    print(f"  - Is Periodic: {is_periodic}")

    return is_periodic


def process_audio(indata, frames, time, status):
    """
    Callback function for real-time audio processing.
    Detects trill presence and sends results via UDP.

    Args:
        indata (ndarray): Incoming audio data.
        frames (int): Number of audio frames in the current buffer.
        time: Audio processing timing information.
        status: Status of the audio input stream.
    """
    if status:
        print(f"Audio Input Status: {status}")

    # Check if the sound is trilling
    trilling = is_trilling(indata, SAMPLE_RATE)

    # Send trill detection result to Unity
    message = "1" if trilling else "0"
    sock.sendto(message.encode(), (UDP_IP, UDP_PORT))

    # Debugging output
    print(f"Trilling: {trilling} | Message Sent: {message}")


# Start the audio stream and process audio in real-time
try:
    print(f"Sending trill detection results to {UDP_IP}:{UDP_PORT}. Press Ctrl+C to stop.")
    with sd.InputStream(channels=1, samplerate=SAMPLE_RATE, blocksize=WINDOW_SIZE, callback=process_audio):
        while True:
            pass
except KeyboardInterrupt:
    print("\nStopped by user.")
except Exception as e:
    print(f"Error: {e}")
