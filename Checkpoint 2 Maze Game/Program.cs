// Escape the Maze - Console .NET Framework (C#)
// Rubric coverage:
//   Tier1: state machine, rendering, movement/collision, pause, scoring (latest 10)
//   Tier2: difficulty, countdown, step counter, customization
//   Tier3: sound settings, trail (visited '.')
// Extras: light Fog of War (Medium/Hard only), teleport portals
// Controls: WASD move | P pause/resume | M/Esc (from pause) to menu
// Notes: Uses Console.SetCursorPosition(0,0) to avoid flicker during Draw.
using System;
using System.Threading; 

enum GameState
{
    Menu,
    Playing,
    Paused,
    Ended
}

class Program
{
    static GameState State = GameState.Menu;

    static void Main()
    {
        while (true)
        {
            switch (State)
            {
                case GameState.Menu: ShowMainMenu(); break;
                case GameState.Playing: RunGameLoop(); break;
                case GameState.Paused: ShowPause(); break;
                case GameState.Ended: ShowEndMenu(); break;
            }
        }
    }

    static void ShowMainMenu()
    {
        Console.CursorVisible = true;
        Console.Clear();
        Console.WriteLine("=== Escape the Maze ===");
        Console.WriteLine("1) Start Game");
        Console.WriteLine("2) View High Scores (latest 10)");
        Console.WriteLine("3) Exit");
        Console.WriteLine("4) Customize Icons/Skin");
        Console.WriteLine($"5) Sound: {(Settings.SoundOn ? "On" : "Off")}");
        Console.WriteLine($"6) Fog of War: {(Settings.FogOn ? "On" : "Off")} (r={Settings.FogRadius})");
        Console.Write("Select: ");
        var key = Console.ReadKey(true).KeyChar;

        if (key == '1')
        {
            // Difficulty selection
            Console.Clear();
            Console.WriteLine("Difficulty: 1) Easy  2) Medium  3) Hard");
            var d = Console.ReadKey(true).KeyChar;

            if (d == '1') Settings.Difficulty = Settings.Level.Easy;
            else if (d == '3') Settings.Difficulty = Settings.Level.Hard;
            else Settings.Difficulty = Settings.Level.Medium;

            // Sizes tuned for typical console widths
            (int rows, int cols) size =
                Settings.Difficulty == Settings.Level.Easy ? (15, 30) :
                Settings.Difficulty == Settings.Level.Hard ? (25, 60) :
                                                                (20, 45); // Medium

            // 3..2..1 Countdown
            for (int i = 3; i >= 1; i--)
            {
                Console.Clear();
                Console.WriteLine($"Starting in {i}...");
                Thread.Sleep(1000);
            }

            bool randomize = Settings.Difficulty != Settings.Level.Easy;
            Maze.LoadOrGenerate(size.rows, size.cols, randomize);
            Player.Reset();
            Console.Clear();
            Console.CursorVisible = false;
            State = GameState.Playing;
        }
        else if (key == '2')
        {
            Console.Clear();
            Console.WriteLine("High Scores (latest 10)");
            Console.WriteLine("-----------------------");
            var lines = ScoreService.ReadLatest(10);
            if (lines.Count == 0) Console.WriteLine("(no scores yet)");
            else foreach (var l in lines) Console.WriteLine(l);
            Console.WriteLine();
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
        }
        else if (key == '3')
        {
            Environment.Exit(0);
        }
        else if (key == '4')
        {
            Console.Clear();
            Console.WriteLine("=== Customize Icons/Skin ===");
            Console.WriteLine($"Current Player icon: {Settings.PlayerIcon}");
            Console.WriteLine($"Current Wall   icon: {Settings.WallIcon}");
            Console.WriteLine();
            Console.Write("Enter new Player icon (single char) or press Enter to keep: ");
            var p = Console.ReadKey(true);
            if (p.Key != ConsoleKey.Enter && !char.IsWhiteSpace(p.KeyChar)) Settings.PlayerIcon = p.KeyChar;

            Console.Write("Enter new Wall icon (single char) or press Enter to keep: ");
            var w = Console.ReadKey(true);
            if (w.Key != ConsoleKey.Enter && !char.IsWhiteSpace(w.KeyChar)) Settings.WallIcon = w.KeyChar;

            Console.WriteLine();
            Console.WriteLine($"Saved! Player={Settings.PlayerIcon}, Wall={Settings.WallIcon}");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
        }
        else if (key == '5')
        {
            Settings.SoundOn = !Settings.SoundOn;
            Console.Clear();
            Console.WriteLine($"Sound is now: {(Settings.SoundOn ? "On" : "Off")}");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
        }
        else if (key == '6')
        {
            Settings.FogOn = !Settings.FogOn;
            Console.Clear();
            Console.WriteLine($"Fog of War is now: {(Settings.FogOn ? "On" : "Off")} (radius {Settings.FogRadius})");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
        }
    }

    static void RunGameLoop()
    {
        Maze.Draw(Player.Steps, Player.Elapsed(), showSteps: true, showTime: true);
        var key = Console.ReadKey(true).Key;

        // Toggle pause
        if (key == ConsoleKey.P)
        {
            State = GameState.Paused;
            return;
        }

        int dr = 0, dc = 0;
        if (key == ConsoleKey.W) dr = -1;
        else if (key == ConsoleKey.S) dr = +1;
        else if (key == ConsoleKey.A) dc = -1;
        else if (key == ConsoleKey.D) dc = +1;
        else return; // ignore other keys

        var nr = Maze.PlayerPos.r + dr;
        var nc = Maze.PlayerPos.c + dc;

        if (!Maze.IsWalkable(nr, nc))
        {
            Beep(400, 60); // invalid move
            return;
        }

        Maze.MovePlayer(dr, dc);
        Player.BumpStep();
        Beep(700, 40); // valid move

        if (Maze.AtExit())
        {
            Beep(900, 120);
            Beep(1200, 160);

            Console.Clear();
            Console.CursorVisible = true;
            Console.WriteLine("🎉 You escaped the maze!");
            Console.WriteLine($"Steps: {Player.Steps}");
            Console.WriteLine($"Time: {Player.Elapsed():mm\\:ss}");
            Console.WriteLine("Press any key...");
            Console.ReadKey(true);
            State = GameState.Ended;
        }
    }

    //  pause/resume with the SAME key (P). M/Esc returns to menu.
    static void ShowPause()
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.WriteLine("=== Game Paused ===");
        Console.WriteLine("Press P to resume");
        Console.WriteLine("Press M (or Esc) to return to Main Menu");

        while (true)
        {
            var k = Console.ReadKey(true).Key;
            if (k == ConsoleKey.P)
            {
                Console.Clear();
                Console.CursorVisible = false;
                State = GameState.Playing;   // resume with same key
                return;
            }
            if (k == ConsoleKey.M || k == ConsoleKey.Escape)
            {
                State = GameState.Menu;      // exit to menu
                return;
            }
            
        }
    }

    static void ShowEndMenu()
    {
        // Save score for the run
        var secs = (int)Player.Elapsed().TotalSeconds;
        ScoreService.AppendLatest10(new Score(DateTime.Now, Player.Steps, secs));

        Console.CursorVisible = true;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== End Game Menu ===");
            Console.WriteLine("1) Play Again");
            Console.WriteLine("2) View High Scores");
            Console.WriteLine("3) Exit");
            var key = Console.ReadKey(true).KeyChar;

            if (key == '1')
            {
                Console.Clear();
                Console.WriteLine("Difficulty: 1) Easy  2) Medium  3) Hard");
                var d = Console.ReadKey(true).KeyChar;

                if (d == '1') Settings.Difficulty = Settings.Level.Easy;
                else if (d == '3') Settings.Difficulty = Settings.Level.Hard;
                else Settings.Difficulty = Settings.Level.Medium;

                (int rows, int cols) size =
                    Settings.Difficulty == Settings.Level.Easy ? (15, 30) :
                    Settings.Difficulty == Settings.Level.Hard ? (25, 60) :
                                                                    (20, 45);

                for (int i = 3; i >= 1; i--)
                {
                    Console.Clear();
                    Console.WriteLine($"Starting in {i}...");
                    Thread.Sleep(1000);
                }

                bool randomize = Settings.Difficulty != Settings.Level.Easy;
                Maze.LoadOrGenerate(size.rows, size.cols, randomize);
                Player.Reset();
                Console.Clear();
                Console.CursorVisible = false;
                State = GameState.Playing;
                return;
            }
            else if (key == '2')
            {
                Console.Clear();
                Console.WriteLine("High Scores (latest 10)");
                Console.WriteLine("-----------------------");
                var lines = ScoreService.ReadLatest(10);
                if (lines.Count == 0) Console.WriteLine("(no scores yet)");
                else foreach (var l in lines) Console.WriteLine(l);
                Console.WriteLine();
                Console.WriteLine("Press any key to return...");
                Console.ReadKey(true);
            }
            else if (key == '3')
            {
                Environment.Exit(0);
            }
        }
    }

    // Beep wrapper that respects sound setting
    static void Beep(int freq, int durMs)
    {
        if (!Settings.SoundOn) return;
        try { Console.Beep(freq, durMs); } catch { }
    }
}
