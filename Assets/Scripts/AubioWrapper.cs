using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class AubioWrapper
{
    private const string DllName = "libaubio_wrapper.dylib";

    // Import the initialization and cleanup functions
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void InitPitchDetector(uint bufferSize, uint hopSize, uint sampleRate);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void CleanupPitchDetector();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float DetectPitch([In] float[] audioData, int bufferSize);

    private static Queue<float> pitchHistory = new Queue<float>();
    private const int MaxHistorySize = 50; // Number of recent pitches to store
    private const float MaxDeviation = 10f; // Maximum deviation in semitones
    private static float previousValidPitch = -1f;

    public static void Initialize(uint bufferSize, uint hopSize, uint sampleRate)
    {
        InitPitchDetector(bufferSize, hopSize, sampleRate);
    }

    public static void CleanUp()
    {
        CleanupPitchDetector();
    }

    public static float GetPitch(float[] audioData)
    {
        int bufferSize = audioData.Length;
        float pitch = DetectPitch(audioData, bufferSize);

        // If pitch is invalid or out of range, return the last valid pitch
        if (pitch <= -40) {
            return (previousValidPitch > -40 ? previousValidPitch : -40f) + 69;
        }

        // Calculate the mean pitch from the history
        float meanPitch = pitchHistory.Count > 0 ? CalculateMean(pitchHistory) : pitch;

        // Check if the pitch is within the deviation range
        if (pitchHistory.Count > 0 && Math.Abs(pitch - meanPitch) > MaxDeviation) {
            return previousValidPitch + 69; // Return the previous valid pitch
        }

        // Update pitch history and store the last valid pitch
        pitchHistory.Enqueue(pitch);
        if (pitchHistory.Count > MaxHistorySize) {
            pitchHistory.Dequeue();
        }

        meanPitch = CalculateMean(pitchHistory);
        previousValidPitch = meanPitch;
        return meanPitch + 69;
    }

    private static float CalculateMean(Queue<float> history)
    {
        float sum = 0;
        foreach (float value in history)
        {
            sum += value;
        }
        Debug.Log($"History count: {history.Count}, AVG: {sum / history.Count}");
        return sum / history.Count;
    }
}
