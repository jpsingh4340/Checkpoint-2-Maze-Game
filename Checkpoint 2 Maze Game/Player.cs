using System;

/// <summary>
/// Tracks player run stats for the current game session.
/// </summary>
static class Player
{
    /// <summary>Total valid moves taken in this run.</summary>
    public static int Steps { get; private set; }

    /// <summary>Start timestamp of current run (used for elapsed time).</summary>
    public static DateTime StartTime { get; private set; }

    /// <summary>Reset counters and start a fresh timer when a game begins.</summary>
    public static void Reset()
    {
        Steps = 0;
        StartTime = DateTime.Now;
    }

    /// <summary>Increment the step counter after a valid move.</summary>
    public static void BumpStep() => Steps++;

    /// <summary>Elapsed time since <see cref="StartTime"/>.</summary>
    public static TimeSpan Elapsed() => DateTime.Now - StartTime;
}
