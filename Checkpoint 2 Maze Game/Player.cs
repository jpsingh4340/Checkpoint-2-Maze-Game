using System;

static class Player
{
    public static int Steps { get; private set; }
    public static DateTime StartTime { get; private set; }

    /// <summary>
    /// Reset step counter and start a new timer.
    /// Call this when a new game begins.
    /// </summary>
    public static void Reset()
    {
        Steps = 0;
        StartTime = DateTime.Now;
    }

    /// <summary>
    /// Increment the step counter.
    /// Call this after every valid move.
    /// </summary>
    public static void BumpStep() => Steps++;

    /// <summary>
    /// Returns how long the current run has lasted.
    /// </summary>
    public static TimeSpan Elapsed() => DateTime.Now - StartTime;
}