using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace AlgoCombat
{
    // ==========================================
    // MAIN ENTRY POINT & UI
    // ==========================================
    class Program
    {
        static void Main(string[] args)
        {
            // Set console size to support the wide symmetrical bracket
            try
            {
                Console.SetWindowSize(130, 40);
            }
            catch { } // Ignore if running in a restricted environment

            Console.Title = "ALGOCOMBAT - Data Structures & Algorithms Lab Project";
            Console.CursorVisible = false;

            Maze currentMaze = new Maze(45, 21); // Slightly wider maze
            // Default generation strategy
            currentMaze.GenerateRecursiveBacktracker();

            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(@"
    _    _             ____                  _           _   
   / \  | | __ _  ___ / ___|___  _ __ ___ | |__   __ _| |_ 
  / _ \ | |/ _` |/ _ \ |   / _ \| '_ ` _ \| '_ \ / _` | __|
 / ___ \| | (_| | (_) | |__| (_) | | | | | | |_) | (_| | |_ 
/_/   \_\_|\__, |\___/ \____\___/|_| |_| |_|_.__/ \__,_|\__|
           |___/                                            
");
                Console.ResetColor();
                Console.WriteLine("---------------------------------------------------------------------------------");
                Console.WriteLine(" Project By: Arbaz Khan Orakzai , Muhammad Ahmed, Abdul Rehman, Rana Mudassir");
                Console.WriteLine("---------------------------------------------------------------------------------");
                Console.WriteLine(" --- MAZE SETUP ---");
                Console.WriteLine(" 1. Generate New Maze (Recursive Backtracker)");
                Console.WriteLine(" 2. Generate New Maze (Prim's Algorithm)");
                Console.WriteLine(" --- MODE SELECTION ---");
                Console.WriteLine(" 3. Run Individual Algorithm (Practice Mode)");
                Console.WriteLine(" 4. START TOURNAMENT (Symmetrical Bracket)");
                Console.WriteLine(" 5. Exit");
                Console.WriteLine("------------------------------------------------------------");
                Console.Write(" Select Option: ");

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.D1)
                {
                    currentMaze.GenerateRecursiveBacktracker();
                    ShowGenerationMessage("Recursive Backtracker");
                }
                else if (key == ConsoleKey.D2)
                {
                    currentMaze.GeneratePrims();
                    ShowGenerationMessage("Prim's Algorithm");
                }
                else if (key == ConsoleKey.D3)
                {
                    RunIndividualMenu(currentMaze);
                }
                else if (key == ConsoleKey.D4)
                {
                    TournamentEngine.RunBracketTournament(currentMaze);
                }
                else if (key == ConsoleKey.D5) break;
            }
        }

        static void ShowGenerationMessage(string type)
        {
            Console.WriteLine($"\n New Maze Generated using {type}!");
            Thread.Sleep(800);
        }

        static void RunIndividualMenu(Maze maze)
        {
            Console.Clear();
            Console.WriteLine("--- SELECT ALGORITHM ---");
            Console.WriteLine(" 1. BFS");
            Console.WriteLine(" 2. DFS");
            Console.WriteLine(" 3. Dijkstra");
            Console.WriteLine(" 4. A*");
            Console.WriteLine(" 5. Greedy Best-First");
            Console.WriteLine(" 6. Monte Carlo");
            Console.WriteLine(" 7. Bidirectional BFS");
            Console.WriteLine(" 8. Smart A* (Weighted)");

            var key = Console.ReadKey(true).Key;
            IPathfinder algo = null;

            switch (key)
            {
                case ConsoleKey.D1: algo = new BfsPathfinder(); break;
                case ConsoleKey.D2: algo = new DfsPathfinder(); break;
                case ConsoleKey.D3: algo = new DijkstraPathfinder(); break;
                case ConsoleKey.D4: algo = new AStarPathfinder(); break;
                case ConsoleKey.D5: algo = new GreedyBestFirstPathfinder(); break;
                case ConsoleKey.D6: algo = new MonteCarloPathfinder(); break;
                case ConsoleKey.D7: algo = new BidirectionalBfsPathfinder(); break;
                case ConsoleKey.D8: algo = new WeightedAStarPathfinder(); break;
            }

            if (algo != null) RunVisualizer(algo, maze);
        }

        public static void RunVisualizer(IPathfinder algo, Maze maze)
        {
            Console.Clear();
            maze.Draw();
            Console.SetCursorPosition(0, maze.Height + 2);
            Console.WriteLine($"Running: {algo.Name}...");

            int delay = algo.Name.Contains("Monte") ? 5 : 20;
            var result = algo.Solve(maze, maze.Start, maze.End, delayMs: delay);

            Console.SetCursorPosition(0, maze.Height + 4);
            if (result.Success)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Path Found! Length: {result.PathLength}, Visited: {result.NodesVisited}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Path Found (or Timeout).");
            }
            Console.ResetColor();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    // ==========================================
    // TOURNAMENT ENGINE (SYMMETRICAL BRACKET)
    // ==========================================
    public static class TournamentEngine
    {
        public static void RunBracketTournament(Maze maze)
        {
            var p = new List<IPathfinder>
            {
                new BfsPathfinder(), new DfsPathfinder(), new DijkstraPathfinder(),
                new AStarPathfinder(), new GreedyBestFirstPathfinder(), new MonteCarloPathfinder(),
                new BidirectionalBfsPathfinder(), new WeightedAStarPathfinder()
            };

            Random rnd = new Random();
            p = p.OrderBy(x => rnd.Next()).ToList();

            // Symmetrical Layout Data Mapping:
            // Left Bracket: P0, P1, P2, P3
            // Right Bracket: P4, P5, P6, P7

            // --- ROUND 1 (Quarter Finals) ---
            var w_q1 = PlayMatch("Quarter-Final 1 (Left)", p[0], p[1], maze, p, 0);
            var w_q2 = PlayMatch("Quarter-Final 2 (Left)", p[2], p[3], maze, p, 1, w_q1);
            var w_q3 = PlayMatch("Quarter-Final 3 (Right)", p[4], p[5], maze, p, 2, w_q1, w_q2);
            var w_q4 = PlayMatch("Quarter-Final 4 (Right)", p[6], p[7], maze, p, 3, w_q1, w_q2, w_q3);

            // --- ROUND 2 (Semi Finals) ---
            var w_s1 = PlayMatch("Semi-Final 1 (Left Final)", w_q1, w_q2, maze, p, 4, w_q1, w_q2, w_q3, w_q4);
            var w_s2 = PlayMatch("Semi-Final 2 (Right Final)", w_q3, w_q4, maze, p, 5, w_q1, w_q2, w_q3, w_q4, w_s1);

            // --- ROUND 3 (Grand Final) ---
            var champion = PlayMatch("GRAND FINAL", w_s1, w_s2, maze, p, 6, w_q1, w_q2, w_q3, w_q4, w_s1, w_s2);

            // Winner Screen
            DrawSymmetricalBracket(p, w_q1, w_q2, w_q3, w_q4, w_s1, w_s2, champion);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n\n >>> THE CHAMPION IS: {champion.Name.ToUpper()} <<<");
            Console.ResetColor();
            Console.WriteLine(" Press any key to return to menu...");
            Console.ReadKey();
        }

        private static IPathfinder PlayMatch(string matchName, IPathfinder a, IPathfinder b, Maze maze,
            List<IPathfinder> p, int stage,
            IPathfinder w1 = null, IPathfinder w2 = null, IPathfinder w3 = null, IPathfinder w4 = null,
            IPathfinder s1 = null, IPathfinder s2 = null)
        {
            DrawSymmetricalBracket(p, w1, w2, w3, w4, s1, s2, null);
            Console.SetCursorPosition(0, 0); // Reset to ensure bracket is visible
            Console.SetCursorPosition(0, 18); // Move below bracket

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n UP NEXT: {matchName}");
            Console.WriteLine($" {a.Name} VS {b.Name}");
            Console.ResetColor();
            Console.WriteLine(" Press any key to start match...");
            Console.ReadKey();

            // Run A
            Console.Clear();
            maze.Draw();
            Console.SetCursorPosition(0, maze.Height + 2);
            Console.WriteLine($" Match: {a.Name} (Running...) vs {b.Name} (Waiting)");
            int delay = a.Name.Contains("Monte") ? 2 : 10;
            var resA = a.Solve(maze, maze.Start, maze.End, delay);
            Thread.Sleep(500);

            // Run B
            maze.Draw();
            Console.SetCursorPosition(0, maze.Height + 2);
            Console.WriteLine($" Match: {a.Name} (Finished) vs {b.Name} (Running...)");
            delay = b.Name.Contains("Monte") ? 2 : 10;
            var resB = b.Solve(maze, maze.Start, maze.End, delay);

            IPathfinder winner;
            if (!resA.Success && !resB.Success) winner = a;
            else if (resA.Success && !resB.Success) winner = a;
            else if (!resA.Success && resB.Success) winner = b;
            else
            {
                if (resA.PathLength < resB.PathLength) winner = a;
                else if (resB.PathLength < resA.PathLength) winner = b;
                else
                {
                    if (resA.NodesVisited < resB.NodesVisited) winner = a;
                    else winner = b;
                }
            }

            // Stats
            Console.SetCursorPosition(0, maze.Height + 4);
            Console.WriteLine(" MATCH RESULTS:");
            Console.WriteLine(" -------------------------------------------------------------");
            Console.Write(String.Format(" {0,-15} | ", "METRIC"));
            if (winner == a) Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(String.Format("{0,-20}", a.Name));
            Console.ResetColor();
            Console.Write(" | ");
            if (winner == b) Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(String.Format("{0,-20}", b.Name));
            Console.ResetColor();
            Console.WriteLine(" -------------------------------------------------------------");
            PrintStatRow("Path Length", resA.PathLength, resB.PathLength, true);
            PrintStatRow("Visited Nodes", resA.NodesVisited, resB.NodesVisited, true);
            PrintStatRow("Time (ms)", (int)resA.ElapsedMilliseconds, (int)resB.ElapsedMilliseconds, true);
            Console.WriteLine(" -------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($" >> WINNER: {winner.Name.ToUpper()}");
            Console.ResetColor();
            Console.WriteLine("\n Press any key to update bracket...");
            Console.ReadKey();
            return winner;
        }

        private static void PrintStatRow(string label, int valA, int valB, bool lowerIsBetter)
        {
            Console.Write(String.Format(" {0,-15} | ", label));
            bool aIsBetter = lowerIsBetter ? valA < valB : valA > valB;
            bool tie = valA == valB;
            if (aIsBetter && !tie) Console.ForegroundColor = ConsoleColor.Green;
            else if (!tie) Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(String.Format("{0,-20}", valA));
            Console.ResetColor();
            Console.Write(" | ");
            if (!aIsBetter && !tie) Console.ForegroundColor = ConsoleColor.Green;
            else if (!tie) Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(String.Format("{0,-20}", valB));
            Console.ResetColor();
        }

        // --- THE NEW SYMMETRICAL DRAWING ENGINE ---
        private static void DrawSymmetricalBracket(List<IPathfinder> p,
            IPathfinder q1, IPathfinder q2, IPathfinder q3, IPathfinder q4,
            IPathfinder s1, IPathfinder s2, IPathfinder champ)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================ ALGOCOMBAT TOURNAMENT ================================================");
            Console.ResetColor();
            Console.WriteLine();

            // FORMAT: LeftName (15) --+                                                                            +-- RightName (15)
            //                        |-- QWinner (15) --+                          +-- QWinner (15) --|
            // LeftName (15) --+                         |                          |                         +-- RightName (15)
            //                                           |-- SF1 (15) -- CHAMP -- SF2 (15) --|

            string fmtP = "{0,-15}";
            string fmtR = "{0,15}";
            string spaceMid = new string(' ', 44);
            string spaceInner = new string(' ', 10);

            // Row 1: P0 & P4
            PrintL(p[0], q1); Console.Write("---+");
            Console.Write(new string(' ', 92));
            Console.Write("+---"); PrintR(p[4], q3); Console.WriteLine();

            // Row 2: Q1 & Q3 Connectors
            Console.Write(new string(' ', 19)); Console.Write("|---"); PrintL(q1, s1); Console.Write("---+");
            Console.Write(spaceMid);
            Console.Write("+---"); PrintR(q3, s2); Console.Write("---|"); Console.WriteLine();

            // Row 3: P1 & P5
            PrintL(p[1], q1); Console.Write("---+"); Console.Write(new string(' ', 19)); Console.Write("|");
            Console.Write(spaceMid);
            Console.Write("|"); Console.Write(new string(' ', 19)); Console.Write("+---"); PrintR(p[5], q3); Console.WriteLine();

            // Row 4: S1 & S2 Connectors to Champ
            Console.Write(new string(' ', 38)); Console.Write("|---"); PrintC(s1, champ); Console.Write("---");
            PrintChamp(champ);
            Console.Write("---"); PrintC(s2, champ); Console.Write("---|"); Console.WriteLine();

            // Row 5: P2 & P6
            PrintL(p[2], q2); Console.Write("---+"); Console.Write(new string(' ', 19)); Console.Write("|");
            Console.Write(spaceMid);
            Console.Write("|"); Console.Write(new string(' ', 19)); Console.Write("+---"); PrintR(p[6], q4); Console.WriteLine();

            // Row 6: Q2 & Q4 Connectors
            Console.Write(new string(' ', 19)); Console.Write("|---"); PrintL(q2, s1); Console.Write("---+");
            Console.Write(spaceMid);
            Console.Write("+---"); PrintR(q4, s2); Console.Write("---|"); Console.WriteLine();

            // Row 7: P3 & P7
            PrintL(p[3], q2); Console.Write("---+");
            Console.Write(new string(' ', 92));
            Console.Write("+---"); PrintR(p[7], q4); Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("=======================================================================================================================");
        }

        static void PrintL(IPathfinder p, IPathfinder winnerOfNext)
        {
            SetColor(p, winnerOfNext);
            Console.Write("{0,-15}", p != null ? Truncate(p.Name, 15) : "TBD");
            Console.ResetColor();
        }

        static void PrintR(IPathfinder p, IPathfinder winnerOfNext)
        {
            SetColor(p, winnerOfNext);
            Console.Write("{0,15}", p != null ? Truncate(p.Name, 15) : "TBD");
            Console.ResetColor();
        }

        static void PrintC(IPathfinder p, IPathfinder winnerOfNext)
        {
            SetColor(p, winnerOfNext);
            Console.Write("{0,-15}", p != null ? Truncate(p.Name, 15) : "TBD");
            Console.ResetColor();
        }

        static void PrintChamp(IPathfinder p)
        {
            if (p != null) Console.ForegroundColor = ConsoleColor.Yellow;
            else Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[ {0,-10} ]", p != null ? Truncate(p.Name, 10) : "CHAMPION");
            Console.ResetColor();
        }

        static void SetColor(IPathfinder p, IPathfinder winnerOfNext)
        {
            if (p == null) Console.ForegroundColor = ConsoleColor.DarkGray;
            else if (winnerOfNext == null) Console.ForegroundColor = ConsoleColor.White;
            else if (winnerOfNext == p) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ForegroundColor = ConsoleColor.Red;
        }

        static string Truncate(string s, int len) => s.Length <= len ? s : s.Substring(0, len - 1) + ".";
    }

    // ==========================================
    // MAZE & DATA STRUCTURES (Unchanged)
    // ==========================================

    public struct Point { public int X, Y; public Point(int x, int y) { X = x; Y = y; } public static Point Empty => new Point(-1, -1); public static bool operator ==(Point a, Point b) => a.X == b.X && a.Y == b.Y; public static bool operator !=(Point a, Point b) => !(a == b); public override bool Equals(object obj) => obj is Point p && this == p; public override int GetHashCode() => (X * 397) ^ Y; }
    public class Node { public Point Position; public bool IsWall; public int Weight; public Node(int x, int y, bool isWall) { Position = new Point(x, y); IsWall = isWall; Weight = 1; } }

    public class Maze
    {
        public Node[,] Grid; public int Width { get; private set; }
        public int Height { get; private set; }
        public Point Start { get; private set; }
        public Point End { get; private set; }
        private Random _rnd = new Random();
        public Maze(int width, int height) { Width = width; Height = height; Grid = new Node[width, height]; }
        private void InitializeGrid() { for (int x = 0; x < Width; x++) for (int y = 0; y < Height; y++) Grid[x, y] = new Node(x, y, true); }
        public void GenerateRecursiveBacktracker() { InitializeGrid(); Stack<Point> stack = new Stack<Point>(); Point start = new Point(1, 1); Grid[start.X, start.Y].IsWall = false; stack.Push(start); while (stack.Count > 0) { Point current = stack.Peek(); var neighbors = GetUnvisitedWallNeighbors(current, 2); if (neighbors.Count > 0) { Point next = neighbors[_rnd.Next(neighbors.Count)]; int wallX = (current.X + next.X) / 2; int wallY = (current.Y + next.Y) / 2; Grid[wallX, wallY].IsWall = false; Grid[next.X, next.Y].IsWall = false; stack.Push(next); } else stack.Pop(); } FinalizeMaze(); }
        public void GeneratePrims() { InitializeGrid(); List<Point> walls = new List<Point>(); Point start = new Point(1, 1); Grid[start.X, start.Y].IsWall = false; AddWallsToList(start, walls); while (walls.Count > 0) { int index = _rnd.Next(walls.Count); Point wall = walls[index]; walls.RemoveAt(index); Point c1 = Point.Empty, c2 = Point.Empty; if (wall.X > 0 && wall.X < Width - 1 && Grid[wall.X - 1, wall.Y].IsWall != Grid[wall.X + 1, wall.Y].IsWall) { c1 = new Point(wall.X - 1, wall.Y); c2 = new Point(wall.X + 1, wall.Y); } else if (wall.Y > 0 && wall.Y < Height - 1 && Grid[wall.X, wall.Y - 1].IsWall != Grid[wall.X, wall.Y + 1].IsWall) { c1 = new Point(wall.X, wall.Y - 1); c2 = new Point(wall.X, wall.Y + 1); } if (c1 != Point.Empty) { Point unvisited = Grid[c1.X, c1.Y].IsWall ? c1 : c2; Grid[wall.X, wall.Y].IsWall = false; Grid[unvisited.X, unvisited.Y].IsWall = false; AddWallsToList(unvisited, walls); } } FinalizeMaze(); }
        private void AddWallsToList(Point p, List<Point> walls) { int[] dx = { 0, 0, 1, -1 }; int[] dy = { 1, -1, 0, 0 }; for (int i = 0; i < 4; i++) { int nx = p.X + dx[i]; int ny = p.Y + dy[i]; if (nx > 0 && nx < Width - 1 && ny > 0 && ny < Height - 1 && Grid[nx, ny].IsWall) if (!walls.Contains(new Point(nx, ny))) walls.Add(new Point(nx, ny)); } }
        private void FinalizeMaze() { Start = new Point(1, 1); End = new Point(Width - 2, Height - 2); Grid[Start.X, Start.Y].IsWall = false; Grid[End.X, End.Y].IsWall = false; for (int i = 0; i < Width * Height / 30; i++) { int rx = _rnd.Next(1, Width - 1); int ry = _rnd.Next(1, Height - 1); Grid[rx, ry].IsWall = false; } }
        private List<Point> GetUnvisitedWallNeighbors(Point p, int step) { List<Point> list = new List<Point>(); int[] dx = { 0, 0, step, -step }; int[] dy = { step, -step, 0, 0 }; for (int i = 0; i < 4; i++) { int nx = p.X + dx[i]; int ny = p.Y + dy[i]; if (nx > 0 && nx < Width - 1 && ny > 0 && ny < Height - 1 && Grid[nx, ny].IsWall) list.Add(new Point(nx, ny)); } return list; }
        public List<Node> GetNeighbors(Node node) { List<Node> neighbors = new List<Node>(); int[] dx = { 0, 0, 1, -1 }; int[] dy = { 1, -1, 0, 0 }; for (int i = 0; i < 4; i++) { int nx = node.Position.X + dx[i]; int ny = node.Position.Y + dy[i]; if (nx >= 0 && nx < Width && ny >= 0 && ny < Height && !Grid[nx, ny].IsWall) neighbors.Add(Grid[nx, ny]); } return neighbors; }
        public void Draw() { Console.SetCursorPosition(0, 0); for (int y = 0; y < Height; y++) { for (int x = 0; x < Width; x++) { if (x == Start.X && y == Start.Y) { Console.BackgroundColor = ConsoleColor.Green; Console.Write("S"); } else if (x == End.X && y == End.Y) { Console.BackgroundColor = ConsoleColor.Red; Console.Write("E"); } else if (Grid[x, y].IsWall) { Console.BackgroundColor = ConsoleColor.Gray; Console.Write(" "); } else { Console.BackgroundColor = ConsoleColor.Black; Console.Write(" "); } } Console.ResetColor(); Console.WriteLine(); } }
    }

    public class PathResult { public bool Success; public int NodesVisited; public int PathLength; public long ElapsedMilliseconds; }
    public interface IPathfinder { string Name { get; } PathResult Solve(Maze maze, Point start, Point end, int delayMs); }
    public class BfsPathfinder : IPathfinder { public string Name => "BFS"; public PathResult Solve(Maze maze, Point start, Point end, int delayMs) { return PathUtils.SolveGenericBFS(maze, start, end, delayMs, false); } }
    public class DfsPathfinder : IPathfinder { public string Name => "DFS"; public PathResult Solve(Maze maze, Point start, Point end, int delayMs) { return PathUtils.SolveGenericDFS(maze, start, end, delayMs); } }
    public class DijkstraPathfinder : IPathfinder { public string Name => "Dijkstra"; public PathResult Solve(Maze maze, Point start, Point end, int delayMs) { return PathUtils.SolveGenericDijkstra(maze, start, end, delayMs); } }
    public class AStarPathfinder : IPathfinder { public string Name => "A*"; public PathResult Solve(Maze maze, Point start, Point end, int delayMs) { return PathUtils.SolveGenericAStar(maze, start, end, delayMs, 1.0); } }
    public class GreedyBestFirstPathfinder : IPathfinder { public string Name => "Greedy Best-First"; public PathResult Solve(Maze maze, Point start, Point end, int delayMs) { return PathUtils.SolveGenericGreedy(maze, start, end, delayMs); } }
    public class MonteCarloPathfinder : IPathfinder { public string Name => "Monte Carlo (Random)"; public PathResult Solve(Maze maze, Point start, Point end, int delayMs) { return PathUtils.SolveMonteCarlo(maze, start, end, delayMs); } }
    public class BidirectionalBfsPathfinder : IPathfinder { public string Name => "Bidirectional BFS"; public PathResult Solve(Maze maze, Point start, Point end, int delayMs) { return PathUtils.SolveBiBFS(maze, start, end, delayMs); } }
    public class WeightedAStarPathfinder : IPathfinder { public string Name => "Smart A* (Weighted)"; public PathResult Solve(Maze maze, Point start, Point end, int delayMs) { return PathUtils.SolveGenericAStar(maze, start, end, delayMs, 2.5); } }

    public static class PathUtils
    {
        public static PathResult SolveGenericBFS(Maze maze, Point start, Point end, int delayMs, bool useStack) { Stopwatch sw = Stopwatch.StartNew(); var visited = new HashSet<Point>(); var parent = new Dictionary<Point, Point>(); dynamic collection = useStack ? (object)new Stack<Node>() : new Queue<Node>(); if (useStack) collection.Push(maze.Grid[start.X, start.Y]); else collection.Enqueue(maze.Grid[start.X, start.Y]); visited.Add(start); int vCount = 0; while (collection.Count > 0) { Node curr = useStack ? collection.Pop() : collection.Dequeue(); vCount++; if (delayMs > 0 && curr.Position != start && curr.Position != end) { Visualizer.DrawCell(curr.Position, useStack ? ConsoleColor.Magenta : ConsoleColor.Cyan, "."); Thread.Sleep(delayMs); } if (curr.Position == end) { sw.Stop(); if (delayMs > 0) Visualizer.DrawPath(parent, end, start); return new PathResult { Success = true, NodesVisited = vCount, PathLength = Visualizer.GetPathLength(parent, end), ElapsedMilliseconds = sw.ElapsedMilliseconds }; } foreach (Node n in maze.GetNeighbors(curr)) { if (!visited.Contains(n.Position)) { visited.Add(n.Position); parent[n.Position] = curr.Position; if (useStack) collection.Push(n); else collection.Enqueue(n); } } } return new PathResult { Success = false, NodesVisited = vCount, ElapsedMilliseconds = sw.ElapsedMilliseconds }; }
        public static PathResult SolveGenericDFS(Maze maze, Point start, Point end, int delayMs) => SolveGenericBFS(maze, start, end, delayMs, true);
        public static PathResult SolveGenericDijkstra(Maze maze, Point start, Point end, int delayMs) { Stopwatch sw = Stopwatch.StartNew(); var dist = new Dictionary<Point, int>(); var parent = new Dictionary<Point, Point>(); var pq = new SimplePriorityQueue<Node>(); var visited = new HashSet<Point>(); dist[start] = 0; pq.Enqueue(maze.Grid[start.X, start.Y], 0); int vCount = 0; while (pq.Count > 0) { Node curr = pq.Dequeue(); if (visited.Contains(curr.Position)) continue; visited.Add(curr.Position); vCount++; if (delayMs > 0 && curr.Position != start && curr.Position != end) { Visualizer.DrawCell(curr.Position, ConsoleColor.DarkYellow, "."); Thread.Sleep(delayMs); } if (curr.Position == end) { sw.Stop(); if (delayMs > 0) Visualizer.DrawPath(parent, end, start); return new PathResult { Success = true, NodesVisited = vCount, PathLength = dist[end], ElapsedMilliseconds = sw.ElapsedMilliseconds }; } foreach (Node n in maze.GetNeighbors(curr)) { if (visited.Contains(n.Position)) continue; int newDist = dist[curr.Position] + n.Weight; if (!dist.ContainsKey(n.Position) || newDist < dist[n.Position]) { dist[n.Position] = newDist; parent[n.Position] = curr.Position; pq.Enqueue(n, newDist); } } } return new PathResult { Success = false, NodesVisited = vCount, ElapsedMilliseconds = sw.ElapsedMilliseconds }; }
        public static PathResult SolveGenericAStar(Maze maze, Point start, Point end, int delayMs, double weight) { Stopwatch sw = Stopwatch.StartNew(); var gScore = new Dictionary<Point, int>(); var parent = new Dictionary<Point, Point>(); var pq = new SimplePriorityQueue<Node>(); var visited = new HashSet<Point>(); gScore[start] = 0; pq.Enqueue(maze.Grid[start.X, start.Y], 0); int vCount = 0; while (pq.Count > 0) { Node curr = pq.Dequeue(); if (curr.Position == end) { sw.Stop(); if (delayMs > 0) Visualizer.DrawPath(parent, end, start); return new PathResult { Success = true, NodesVisited = vCount, PathLength = gScore[end], ElapsedMilliseconds = sw.ElapsedMilliseconds }; } if (visited.Contains(curr.Position)) continue; visited.Add(curr.Position); vCount++; if (delayMs > 0 && curr.Position != start && curr.Position != end) { Visualizer.DrawCell(curr.Position, ConsoleColor.Blue, "*"); Thread.Sleep(delayMs); } foreach (Node n in maze.GetNeighbors(curr)) { int tG = gScore[curr.Position] + n.Weight; if (!gScore.ContainsKey(n.Position) || tG < gScore[n.Position]) { gScore[n.Position] = tG; int f = tG + (int)(Heuristic(n.Position, end) * weight); parent[n.Position] = curr.Position; pq.Enqueue(n, f); } } } return new PathResult { Success = false, NodesVisited = vCount, ElapsedMilliseconds = sw.ElapsedMilliseconds }; }
        public static PathResult SolveGenericGreedy(Maze maze, Point start, Point end, int delayMs) { Stopwatch sw = Stopwatch.StartNew(); var parent = new Dictionary<Point, Point>(); var pq = new SimplePriorityQueue<Node>(); var visited = new HashSet<Point>(); pq.Enqueue(maze.Grid[start.X, start.Y], Heuristic(start, end)); int vCount = 0; while (pq.Count > 0) { Node curr = pq.Dequeue(); if (curr.Position == end) { sw.Stop(); if (delayMs > 0) Visualizer.DrawPath(parent, end, start); return new PathResult { Success = true, NodesVisited = vCount, PathLength = Visualizer.GetPathLength(parent, end), ElapsedMilliseconds = sw.ElapsedMilliseconds }; } if (visited.Contains(curr.Position)) continue; visited.Add(curr.Position); vCount++; if (delayMs > 0 && curr.Position != start && curr.Position != end) { Visualizer.DrawCell(curr.Position, ConsoleColor.DarkGreen, "?"); Thread.Sleep(delayMs); } foreach (Node n in maze.GetNeighbors(curr)) { if (!visited.Contains(n.Position) && !parent.ContainsKey(n.Position)) { parent[n.Position] = curr.Position; pq.Enqueue(n, Heuristic(n.Position, end)); } } } return new PathResult { Success = false, NodesVisited = vCount, ElapsedMilliseconds = sw.ElapsedMilliseconds }; }
        public static PathResult SolveMonteCarlo(Maze maze, Point start, Point end, int delayMs) { Stopwatch sw = Stopwatch.StartNew(); Random rnd = new Random(); Node current = maze.Grid[start.X, start.Y]; int steps = 0; while (current.Position != end && steps < 5000) { steps++; if (delayMs > 0) { Visualizer.DrawCell(current.Position, ConsoleColor.White, "@"); Thread.Sleep(delayMs); if (current.Position != start) Visualizer.DrawCell(current.Position, ConsoleColor.DarkGray, "."); } var n = maze.GetNeighbors(current); if (n.Count > 0) current = n[rnd.Next(n.Count)]; } sw.Stop(); return new PathResult { Success = current.Position == end, NodesVisited = steps, PathLength = steps, ElapsedMilliseconds = sw.ElapsedMilliseconds }; }
        public static PathResult SolveBiBFS(Maze maze, Point start, Point end, int delayMs) { Stopwatch sw = Stopwatch.StartNew(); var qS = new Queue<Node>(); var qE = new Queue<Node>(); var vS = new Dictionary<Point, Point>(); var vE = new Dictionary<Point, Point>(); qS.Enqueue(maze.Grid[start.X, start.Y]); vS[start] = start; qE.Enqueue(maze.Grid[end.X, end.Y]); vE[end] = end; int vC = 0; while (qS.Count > 0 && qE.Count > 0) { if (Exp(qS, vS, vE, maze, delayMs, ConsoleColor.Cyan, out int lS)) return Res(sw, vC, lS + GetPath(vE, vE.Keys.Last(), end) - 1); vC++; if (Exp(qE, vE, vS, maze, delayMs, ConsoleColor.Blue, out int lE)) return Res(sw, vC, lE + GetPath(vS, vS.Keys.Last(), start) - 1); vC++; } return Res(sw, vC, 0, false); }
        private static bool Exp(Queue<Node> q, Dictionary<Point, Point> my, Dictionary<Point, Point> other, Maze m, int d, ConsoleColor c, out int l) { l = 0; if (q.Count == 0) return false; Node curr = q.Dequeue(); if (other.ContainsKey(curr.Position)) { if (d > 0) Visualizer.DrawCell(curr.Position, ConsoleColor.Yellow, "X"); l = GetPath(my, curr.Position, my.First().Value); return true; } if (d > 0) { Visualizer.DrawCell(curr.Position, c, "."); Thread.Sleep(d); } foreach (var n in m.GetNeighbors(curr)) if (!my.ContainsKey(n.Position)) { my[n.Position] = curr.Position; q.Enqueue(n); } return false; }
        private static int GetPath(Dictionary<Point, Point> p, Point c, Point s) { int i = 0; while (p.ContainsKey(c) && c != s) { c = p[c]; i++; } return i; }
        private static PathResult Res(Stopwatch sw, int v, int l, bool s = true) { sw.Stop(); return new PathResult { Success = s, NodesVisited = v, PathLength = l, ElapsedMilliseconds = sw.ElapsedMilliseconds }; }
        private static int Heuristic(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }

    public class SimplePriorityQueue<T> { private List<KeyValuePair<T, int>> elements = new List<KeyValuePair<T, int>>(); public int Count => elements.Count; public void Enqueue(T item, int priority) => elements.Add(new KeyValuePair<T, int>(item, priority)); public T Dequeue() { int bestIndex = 0; for (int i = 0; i < elements.Count; i++) if (elements[i].Value < elements[bestIndex].Value) bestIndex = i; T bestItem = elements[bestIndex].Key; elements.RemoveAt(bestIndex); return bestItem; } }
    public static class Visualizer { public static void DrawCell(Point p, ConsoleColor color, string symbol) { try { Console.SetCursorPosition(p.X, p.Y); Console.BackgroundColor = color; Console.Write(symbol); Console.ResetColor(); } catch { } } public static void DrawPath(Dictionary<Point, Point> parents, Point end, Point start) { Point curr = end; while (parents.ContainsKey(curr)) { curr = parents[curr]; if (curr == start) break; DrawCell(curr, ConsoleColor.Yellow, "•"); Thread.Sleep(10); } } public static int GetPathLength(Dictionary<Point, Point> parents, Point end) { int count = 0; Point curr = end; while (parents.ContainsKey(curr)) { count++; curr = parents[curr]; } return count; } }
}