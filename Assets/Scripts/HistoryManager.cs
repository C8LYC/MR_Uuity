using System;
using System.Collections.Generic;
using System.Linq;

public static class HistoryManager
{
    private static Queue<float> _historyQueue;
    private static int _maxSize = 10;

    public static void Initialize(int maxSize = 10) {
        _maxSize = maxSize;
        _historyQueue = new Queue<float>(_maxSize);
    }

    public static void AddEntry(float entry) {
        if (_historyQueue == null) {
            throw new InvalidOperationException("HistoryManager is not initialized. Call Initialize() first.");
        }

        if (_historyQueue.Count >= _maxSize) {
            _historyQueue.Dequeue(); // Remove the earliest entry
        }

        _historyQueue.Enqueue(entry);
    }

    public static float GetMean() {
        if (_historyQueue == null || _historyQueue.Count == 0) {
            return float.NaN; // Return NaN if no entries are present
        }

        return _historyQueue.Average();
    }

    public static void DisplayHistory() {
        if (_historyQueue == null || _historyQueue.Count == 0) {
            Console.WriteLine("No entries in the history.");
            return;
        }
        Console.WriteLine("History Entries: " + string.Join(", ", _historyQueue));
    }
}
