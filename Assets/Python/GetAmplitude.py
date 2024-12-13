import sounddevice as sd
import numpy as np
from scipy.signal import butter, filtfilt, hilbert, find_peaks
# import matplotlib.pyplot as plt
# from matplotlib.animation import FuncAnimation
import UDP

# UDP
PORT = 5006

# Constants
duration = 0.1  # each frame duration in seconds
sample_rate = 44100  # typical sample rate for audio
cutoff_freq = 30  # Cutoff frequency for envelope filtering in Hz
order = 4
amplitude_thresh = 0.01  # Minimum amplitude threshold
frequency_low = 15  # Minimum frequency threshold (Hz)
frequency_high = 32  # Maximum frequency threshold (Hz)
periodicity_thresh = 0.01  # Standard deviation threshold for periodicity

# Low-pass filter design
nyquist = 0.5 * sample_rate
normal_cutoff = cutoff_freq / nyquist
b, a = butter(order, normal_cutoff, btype='low', analog=False)

# Buffer for the filtered envelope data
buffer_size = int(sample_rate * duration)
filtered_envelope = np.zeros(buffer_size)

# # Function to update the plot
# def update_plot(frame):
#     line.set_ydata(filtered_envelope)
#     return line,


# Callback to process audio input
def process_audio(indata, frames, time, status):
    if status:
        print(status, flush=True)
    global filtered_envelope

    # Compute the amplitude envelope of the signal
    analytic_signal = hilbert(
        indata[:, 0])  # Use only the first channel if stereo
    amplitude_envelope = np.abs(analytic_signal)

    # Apply the low-pass filter to smooth the envelope
    filtered_envelope = filtfilt(b, a, amplitude_envelope)

    # Check if the average amplitude exceeds the threshold
    avg_amplitude = filtered_envelope.mean()
    if avg_amplitude > 0:
        avg_amplitude = avg_amplitude**0.2
    # if avg_amplitude < amplitude_thresh:
    #     return  # Ignore frames below amplitude threshold

    # # Find peaks in the envelope
    # peaks, _ = find_peaks(filtered_envelope, height=0)

    # # Calculate peak frequency
    # peak_times = peaks / sample_rate
    # peak_freq = len(peak_times) / (len(indata) / sample_rate)

    # # Check if peak frequency is within the desired range
    # if not (frequency_low <= peak_freq <= frequency_high):
    #     return  # Ignore frames outside the frequency range

    # # Calculate intervals between peaks and assess periodicity
    # peak_intervals = np.diff(
    #     peaks) / sample_rate  # Convert intervals to seconds
    # interval_std = np.std(peak_intervals)  # Standard deviation of intervals

    # # Check if the structure is periodic based on interval consistency
    # is_periodic = interval_std < periodicity_thresh

    # Print results for frames meeting all conditions
    UDP.send_message(f"{avg_amplitude:.4f}", PORT)
    # UDP.send_message(f"{int(is_periodic)}", 5007)
    # print(f"Frame Analysis:")
    print(f"  - Avg Amplitude: {avg_amplitude:.4f}")
    # print(f"  - Peak Frequency: {peak_freq:.2f} Hz")
    # print(f"  - Periodicity (Interval STD): {interval_std:.4f}")
    # print(f"  - Is Periodic: {is_periodic}")


# Set up the plot
# fig, ax = plt.subplots()
# x = np.linspace(0, duration, buffer_size)
# line, = ax.plot(x, filtered_envelope)
# ax.set_ylim(0, 0.2)  # Adjust according to expected envelope amplitude
# ax.set_xlim(0, duration)
# ax.set_title("Real-Time Filtered Amplitude Envelope")
# ax.set_xlabel("Time (s)")
# ax.set_ylabel("Amplitude")

# Start the microphone stream
try:
    with sd.InputStream(callback=process_audio,
                        channels=1,
                        samplerate=sample_rate,
                        blocksize=buffer_size):
        print("Listening for lip trill... Press Ctrl+C to stop.")
        while True:
            sd.sleep(
                100)  # Short sleep to allow checking for KeyboardInterrupt
        # ani = FuncAnimation(fig, update_plot, blit=True)
        # plt.show()

except KeyboardInterrupt:
    UDP.close_connection()
    print("\nStopping the program.")
except Exception as e:
    UDP.close_connection()
    print(f"An error occurred: {e}")
