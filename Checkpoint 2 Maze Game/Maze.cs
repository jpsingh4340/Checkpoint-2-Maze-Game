using System;

static class Settings   // holds icons + sound toggle (used by Program.cs and Maze.cs)
{
    public static char PlayerIcon = 'P';
    public static char WallIcon = '#';
    public static bool SoundOn = true;   // toggled in main menu (Tier 3)
}

static class Maze
{
    public static char[,] Grid { get; private set; }
    public static int Rows { get; private set; }
    public static int Cols { get; private set; }
    public static (int r, int c) PlayerPos { get; private set; }
    public static (int r, int c) ExitPos { get; private set; }

    public static void LoadOrGenerate(int rows, int cols, bool randomize)
    {
        Rows = rows; Cols = cols;
        Grid = new char[Rows, Cols];

        // Fill everything with walls
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                Grid[r, c] = '#';

        // Carve a simple L-shaped corridor (always solvable)
        for (int c = 1; c < Cols - 1; c++) Grid[1, c] = '.';
        for (int r = 1; r < Rows - 1; r++) Grid[r, Cols - 2] = '.';

        PlayerPos = (1, 1);
        ExitPos = (Rows - 2, Cols - 2);

        Grid[PlayerPos.r, PlayerPos.c] = 'P';
        Grid[ExitPos.r, ExitPos.c] = 'E';

        // NOTE: if you later add a random generator for Hard, use 'randomize' here.
    }

    public static void Draw(int steps, TimeSpan time, bool showSteps, bool showTime)
    {
        Console.Clear();
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                char ch = Grid[r, c];
                // Render with chosen icons/skin; internal logic stays '#', '.', 'P', 'E', '·'
                if (ch == '#') Console.Write(Settings.WallIcon);
                else if (ch == 'P') Console.Write(Settings.PlayerIcon);
                else Console.Write(ch);
            }
            Console.WriteLine();
        }
        Console.Write("HUD: ");
        if (showSteps) Console.Write($"Steps={steps}  ");
        if (showTime) Console.Write($"Time={time:mm\\:ss}  ");
        Console.WriteLine(" (W/A/S/D move, P pause)");
    }

    public static bool IsWalkable(int r, int c)
    {
        if (r < 0 || r >= Rows || c < 0 || c >= Cols) return false;
        return Grid[r, c] != '#';
    }

    public static void MovePlayer(int dr, int dc)
    {
        int newR = PlayerPos.r + dr;
        int newC = PlayerPos.c + dc;

        if (!IsWalkable(newR, newC)) return;

        // Trail effect:
        // Leave a breadcrumb ONLY when stepping off a '.' (don’t touch Exit).
        // If stepping off the start 'P', turn it into '.' on first move.
        char prev = Grid[PlayerPos.r, PlayerPos.c];
        if (prev != 'E')
        {
            if (prev == '.') Grid[PlayerPos.r, PlayerPos.c] = '·'; // middle dot breadcrumb
            else if (prev == 'P') Grid[PlayerPos.r, PlayerPos.c] = '·';  // first move from start
        }

        PlayerPos = (newR, newC);

        // Keep exit visible if standing on it
        if (PlayerPos == ExitPos)
            Grid[newR, newC] = 'E';
        else
            Grid[newR, newC] = 'P';
    }

    public static bool AtExit() => PlayerPos == ExitPos;
}
