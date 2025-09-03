using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// A single score entry. We record both Steps and Seconds and keep the latest 10 runs.
public class Score
{
    public DateTime When;
    public int Steps;
    public int Seconds;

    public Score(DateTime when, int steps, int seconds)
    {
        When = when;
        Steps = steps;
        Seconds = seconds;
    }
}

public static class ScoreService
{
    private const string PathScores = "scores.txt";

    public static void AppendLatest10(Score s)
    {
        try
        {
            var line = $"{s.When:yyyy-MM-dd HH:mm:ss},{s.Steps},{s.Seconds}";
            File.AppendAllLines(PathScores, new[] { line });

            var lines = File.ReadAllLines(PathScores);
            if (lines.Length > 10)
            {
                var keep = new List<string>();
                int start = Math.Max(0, lines.Length - 10);
                for (int i = start; i < lines.Length; i++) keep.Add(lines[i]);
                File.WriteAllLines(PathScores, keep);
            }
        }
        catch { }
    }

    public static List<string> ReadLatest(int count)
    {
        var result = new List<string>();
        try
        {
            if (!File.Exists(PathScores)) return result;
            var all = File.ReadAllLines(PathScores);
            int start = Math.Max(0, all.Length - count);
            for (int i = start; i < all.Length; i++) result.Add(all[i]);
        }
        catch { }
        return result;
    }
}
