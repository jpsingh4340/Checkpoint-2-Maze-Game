using System;

static class Settings   //Program.cs
{
    public enum Level { Easy, Medium, Hard }
    public static Level Difficulty = Level.Medium;

    public static char PlayerIcon = 'P';
    public static char WallIcon = '#';
    public static bool SoundOn = true;   // toggled in main menu

    // Fog 
    public static bool FogOn = true;
    public static int FogRadius = 3;
}

static class Maze
{
    public static char[,] Grid { get; private set; }
    public static int Rows { get; private set; }
    public static int Cols { get; private set; }
    public static (int r, int c) PlayerPos { get; private set; }
    public static (int r, int c) ExitPos { get; private set; }

   
    public static System.Collections.Generic.Dictionary<(int r, int c), (int r, int c)> Portals
        = new System.Collections.Generic.Dictionary<(int r, int c), (int r, int c)>();

    public static void LoadOrGenerate(int rows, int cols, bool randomize)
    {
        Rows = rows; Cols = cols;
        Grid = new char[Rows, Cols];
        Portals.Clear();

        // start all walls
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                Grid[r, c] = '#';

        PlayerPos = (1, 1);
        ExitPos = (Rows - 2, Cols - 2);

        // generate corridors as '.' first
        if (Settings.Difficulty == Settings.Level.Hard)
        {
            GenerateDFS();            // hardest
        }
        else if (Settings.Difficulty == Settings.Level.Medium)
        {
            GenerateDFS();
            AddShortcuts(6);          
        }
        else // Easy
        {
            GenerateSimpleL();        // very easy
        }

        // hide all unvisited corridors -> turn '.' into space
        ConvertOpensToSpaces();

        // Place portals for Medium/Hard
        if (Settings.Difficulty == Settings.Level.Medium) PlacePortals(pairCount: 2);
        if (Settings.Difficulty == Settings.Level.Hard) PlacePortals(pairCount: 3);

        
        Grid[PlayerPos.r, PlayerPos.c] = 'P';
        Grid[ExitPos.r, ExitPos.c] = 'E';
    }

    // -------- Generators --------

    static void GenerateSimpleL()
    {
        for (int c = 1; c < Cols - 1; c++) Grid[1, c] = '.';
        for (int r = 1; r < Rows - 1; r++) Grid[r, Cols - 2] = '.';
    }

    static void GenerateDFS()
    {
        for (int r = 1; r < Rows - 1; r++)
            for (int c = 1; c < Cols - 1; c++)
                Grid[r, c] = '#';

        var rnd = new Random();
        var stack = new System.Collections.Generic.Stack<(int r, int c)>();

        Grid[1, 1] = '.';
        stack.Push((1, 1));

        int[] dr = { -2, 2, 0, 0 };
        int[] dc = { 0, 0, -2, 2 };

        while (stack.Count > 0)
        {
            var (r, c) = stack.Peek();

            var nbrs = new System.Collections.Generic.List<(int nr, int nc, int wr, int wc)>();
            for (int i = 0; i < 4; i++)
            {
                int nr = r + dr[i];
                int nc = c + dc[i];
                int wr = r + dr[i] / 2;
                int wc = c + dc[i] / 2;
                if (nr > 0 && nr < Rows - 1 && nc > 0 && nc < Cols - 1 && Grid[nr, nc] == '#')
                    nbrs.Add((nr, nc, wr, wc));
            }

            if (nbrs.Count == 0)
            {
                stack.Pop();
            }
            else
            {
                var pick = nbrs[rnd.Next(nbrs.Count)];
                Grid[pick.wr, pick.wc] = '.';
                Grid[pick.nr, pick.nc] = '.';
                stack.Push((pick.nr, pick.nc));
            }
        }

        // ensure exit area is open
        if (Grid[ExitPos.r, ExitPos.c] == '#') Grid[ExitPos.r, ExitPos.c] = '.';
        if (ExitPos.c - 1 >= 1) Grid[ExitPos.r, ExitPos.c - 1] = '.';
        if (ExitPos.r - 1 >= 1) Grid[ExitPos.r - 1, ExitPos.c] = '.';
    }

    static void AddShortcuts(int percent)
    {
        if (percent <= 0) return;
        var rnd = new Random();

        int candidates = 0;
        for (int r = 1; r < Rows - 1; r++)
            for (int c = 1; c < Cols - 1; c++)
            {
                if (Grid[r, c] != '#') continue;
                bool horiz = Grid[r, c - 1] == '.' && Grid[r, c + 1] == '.';
                bool vert = Grid[r - 1, c] == '.' && Grid[r + 1, c] == '.';
                if (horiz || vert) candidates++;
            }
        int toOpen = Math.Max(1, candidates * percent / 100);

        for (int opened = 0; opened < toOpen;)
        {
            int r = rnd.Next(1, Rows - 1);
            int c = rnd.Next(1, Cols - 1);
            if (Grid[r, c] != '#') continue;

            bool horiz = Grid[r, c - 1] == '.' && Grid[r, c + 1] == '.';
            bool vert = Grid[r - 1, c] == '.' && Grid[r + 1, c] == '.';
            if (!horiz && !vert) continue;

            if ((r, c) == PlayerPos || (r, c) == ExitPos) continue;

            Grid[r, c] = '.';
            opened++;
        }
    }

    static void ConvertOpensToSpaces()
    {
        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
                if (Grid[r, c] == '.') Grid[r, c] = ' ';
    }

    static void PlacePortals(int pairCount)
    {
        var rnd = new Random();
        int placedPairs = 0;
        int attempts = 0;

        bool IsGood((int r, int c) p)
        {
            if (p.r <= 0 || p.r >= Rows - 1 || p.c <= 0 || p.c >= Cols - 1) return false;
            if (Grid[p.r, p.c] != ' ') return false;                    // must be open
            if (p == PlayerPos || p == ExitPos) return false;           // avoid start/exit
            if (Portals.ContainsKey(p)) return false;                   // not already a portal
            int dStart = Math.Abs(p.r - PlayerPos.r) + Math.Abs(p.c - PlayerPos.c);
            int dExit = Math.Abs(p.r - ExitPos.r) + Math.Abs(p.c - ExitPos.c);
            if (dStart < 4 || dExit < 3) return false;
            return true;
        }

        while (placedPairs < pairCount && attempts < 4000)
        {
            attempts++;
            var a = (rnd.Next(1, Rows - 1), rnd.Next(1, Cols - 1));
            var b = (rnd.Next(1, Rows - 1), rnd.Next(1, Cols - 1));
            if (a == b) continue;
            if (!IsGood(a) || !IsGood(b)) continue;

            Portals[a] = b;
            Portals[b] = a;
            placedPairs++;
        }
    }

    

    // Make fog very light
    static int EffectiveFogRadius()
    {
        // Enforce large minimums so fog covers only a small outer band.
        int sum = Rows + Cols;

        if (Settings.Difficulty == Settings.Level.Medium)
        {
            // 20x45 
            return Math.Max(Settings.FogRadius, Math.Max(12, sum / 4));
        }
        else if (Settings.Difficulty == Settings.Level.Hard)
        {
            // 25x60 
            return Math.Max(Settings.FogRadius, Math.Max(10, sum / 5));
        }

        // Easy doesn’t use fog; this value is ignored when fog is inactive.
        return Settings.FogRadius;
    }

    public static void Draw(int steps, TimeSpan time, bool showSteps, bool showTime)
    {
       
        try { Console.SetCursorPosition(0, 0); }
        catch { Console.Clear(); Console.SetCursorPosition(0, 0); }

        // Fog applies ONLY on Medium & Hard
        bool fogActive = Settings.FogOn && Settings.Difficulty != Settings.Level.Easy;
        int radius = EffectiveFogRadius();

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                bool inFog = false;
                if (fogActive)
                {
                    // never hide the player, exit, or visited path
                    bool alwaysVisible =
                        (r == PlayerPos.r && c == PlayerPos.c) ||
                        (r == ExitPos.r && c == ExitPos.c) ||
                        Grid[r, c] == '.';

                    if (!alwaysVisible)
                    {
                        int md = Math.Abs(r - PlayerPos.r) + Math.Abs(c - PlayerPos.c);
                        inFog = md > radius;
                    }
                }

                if (inFog)
                {
                    Console.Write('?');
                    continue;
                }

                bool isPortal = Portals.ContainsKey((r, c));
                char ch = Grid[r, c];

                if (ch == '#') Console.Write(Settings.WallIcon);
                else if (ch == 'P') Console.Write(Settings.PlayerIcon);
                else if (isPortal && (r, c) != ExitPos) Console.Write('O'); // show portal
                else Console.Write(ch);  // ' ' or '.' or 'E'
            }
            Console.WriteLine();
        }

        
        Console.Write("HUD: ");
        Console.Write("Difficulty=" + Settings.Difficulty + "  ");
        Console.Write("Fog=" + (fogActive ? "On" : "Off") + "  ");
        if (showSteps) Console.Write("Steps=" + steps + "  ");
        if (showTime) Console.Write("Time=" + time.ToString("mm\\:ss") + "  ");
        Console.Write(" (W/A/S/D move, P pause)");

        int remaining = Math.Max(0, Console.WindowWidth - Console.CursorLeft);
        if (remaining > 0) Console.Write(new string(' ', remaining - 1));
    }

    public static bool IsWalkable(int r, int c)
    {
        if (r < 0 || r >= Rows || c < 0 || c >= Cols) return false;
        return Grid[r, c] != '#'; // walkable: ' ', '.', 'P', 'E'
    }

    public static void MovePlayer(int dr, int dc)
    {
        int targetR = PlayerPos.r + dr;
        int targetC = PlayerPos.c + dc;

        if (!IsWalkable(targetR, targetC)) return;

        // Leave '.' on the tile we vacate (unless it's the exit)
        if (Grid[PlayerPos.r, PlayerPos.c] != 'E')
            Grid[PlayerPos.r, PlayerPos.c] = '.';

        int finalR = targetR;
        int finalC = targetC;

        // Teleport if stepping onto a portal
        if (Portals.TryGetValue((finalR, finalC), out var dest))
        {
            finalR = dest.r;
            finalC = dest.c;
            if (Settings.SoundOn) { try { Console.Beep(1000, 80); } catch { } }
        }

        PlayerPos = (finalR, finalC);

        if (PlayerPos == ExitPos)
            Grid[finalR, finalC] = 'E';
        else
            Grid[finalR, finalC] = 'P';
    }

    public static bool AtExit() => PlayerPos == ExitPos;
}
