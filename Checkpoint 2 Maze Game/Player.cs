using System;

static class Player
{
    public static int Steps { get; private set; }
    public static DateTime StartTime { get; private set; }

    public static void Reset()
    {
        Steps = 0;
        StartTime = DateTime.Now;
    }

    public static void BumpStep() => Steps++;

    public static TimeSpan Elapsed() => DateTime.Now - StartTime;
}
