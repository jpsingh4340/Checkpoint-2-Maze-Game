using System;
using System.Threading; // for countdown sleep

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
                case GameState.Menu:
                    ShowMainMenu();
                    break;
                case GameState.Playing:
                    RunGameLoop();
                    break;
                case GameState.Paused:
                    ShowPause();
                    break;
                case GameState.Ended:
                    ShowEndMenu();
                    break;
            }
        }
    }

    static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Escape the Maze ===");
        Console.WriteLine("1) Start Game");
        Console.WriteLine("2) View High Scores (latest 10)");
        Console.WriteLine("3) Exit");
        Console.WriteLine("4) Customize Icons/Skin");
        Console.WriteLine($"5) Sound: {(Settings.SoundOn ? "On" : "Off")}");   // T3(d)
        Console.Write("Select: ");
        var key = Console.ReadKey(true).KeyChar;

        if (key == '1')
        {
            // Difficulty selection (Tier 2)
            Console.Clear();
            Console.WriteLine("Difficulty: 1) Easy  2) Medium  3) Hard");
            var d = Console.ReadKey(true).KeyChar;
            (int rows, int cols) size = d == '1' ? (15, 30) : d == '3' ? (25, 70) : (20, 50);

            // Countdown (Tier 2)
            for (int i = 3; i >= 1; i--)
            {
                Console.Clear();
                Console.WriteLine($"Starting in {i}...");
                Thread.Sleep(1000);
            }

            bool randomize = d == '3';
            Maze.LoadOrGenerate(size.rows, size.cols, randomize);
            Player.Reset();
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
            if (p.Key != ConsoleKey.Enter) Settings.PlayerIcon = p.KeyChar;

            Console.Write("Enter new Wall icon (single char) or press Enter to keep: ");
            var w = Console.ReadKey(true);
            if (w.Key != ConsoleKey.Enter) Settings.WallIcon = w.KeyChar;

            Console.WriteLine();
            Console.WriteLine($"Saved! Player={Settings.PlayerIcon}, Wall={Settings.WallIcon}");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
        }
        else if (key == '5') // T3(d): Sound toggle
        {
            Settings.SoundOn = !Settings.SoundOn;
            Console.Clear();
            Console.WriteLine($"Sound is now: {(Settings.SoundOn ? "On" : "Off")}");
            Console.WriteLine("Press any key to return...");
            Console.ReadKey(true);
        }
    }

    static void RunGameLoop()
    {
        Maze.Draw(Player.Steps, Player.Elapsed(), showSteps: true, showTime: true);
        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.P)
        {
            State = GameState.Paused;
            return;
        }

        // (Optional future) Hint key 'H' can be wired here for Tier 2(a)

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
            Beep(400, 60); // invalid move (guarded by Settings.SoundOn)
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
            Console.WriteLine("🎉 You escaped the maze!");
            Console.WriteLine($"Steps: {Player.Steps}");
            Console.WriteLine($"Time: {Player.Elapsed():mm\\:ss}");
            Console.WriteLine("Press any key...");
            Console.ReadKey(true);
            State = GameState.Ended;
        }
    }

    static void ShowPause()
    {
        Console.Clear();
        Console.WriteLine("=== Game Paused ===");
        Console.WriteLine("1) Resume");
        Console.WriteLine("2) Exit to Menu");

        var key = Console.ReadKey(true).KeyChar;
        if (key == '1') State = GameState.Playing;
        else if (key == '2') State = GameState.Menu;
    }

    static void ShowEndMenu()
    {
        // Save both steps and actual elapsed seconds
        var secs = (int)Player.Elapsed().TotalSeconds;
        ScoreService.AppendLatest10(new Score(DateTime.Now, Player.Steps, secs));

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
                (int rows, int cols) size = d == '1' ? (15, 30) : d == '3' ? (25, 70) : (20, 50);

                for (int i = 3; i >= 1; i--)
                {
                    Console.Clear();
                    Console.WriteLine($"Starting in {i}...");
                    Thread.Sleep(1000);
                }

                bool randomize = d == '3';
                Maze.LoadOrGenerate(size.rows, size.cols, randomize);
                Player.Reset();
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

    // ----- T3(d): central beep that respects Settings.SoundOn -----
    static void Beep(int freq, int durMs)
    {
        if (!Settings.SoundOn) return;
        try { Console.Beep(freq, durMs); } catch { /* environments may block beep */ }
    }
}
