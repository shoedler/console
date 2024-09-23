using System;
using System.Collections.Generic;
using System.Numerics;

namespace cnake
{
    class Program
    {
        public static Snake S;
        static void Main()
        {
            Console.SetWindowSize(80, 20);
            Console.CursorVisible = false;

            S = new Snake();
            while (S.Alive) { S.Run(); }
        }
    }

    class Snake
    {
        public static List<Vector2> Body = [];
        public static Vector2 Dir = new(2, 0);
        public static Vector2 Food = NewFood();
        public bool Alive = true;

        static Snake() { for (var i = 3; i >= 0; i--) { Body.Add(new Vector2(i, 0)); } }

        public static Vector2 NewFood()
        {
            Random r = new();
            var x = r.Next(0, Console.WindowWidth);
            while (x % 2 == 0) { x = r.Next(0, Console.WindowWidth); }
            return new Vector2(x, r.Next(0, Console.WindowHeight));
        }

        public void Run()
        {

            // Move Body pieces to their next position
            for (var i = Body.Count - 1; i > 0; i--) { Body[i] = Body[i - 1]; }

            // Update head position
            Body[0] = new Vector2(Mod((Body[0].X + Dir.X), Console.WindowWidth), Mod((Body[0].Y + Dir.Y), Console.WindowHeight));

            // Render Body pieces at their respective position
            Body.ForEach(b => { DrawPiece(b, ConsoleColor.White); });

            // Clear Trail by drawing last piece in black (This way Console.Clear is not needed), draw Food
            DrawPiece(Body[^1], ConsoleColor.Black);
            DrawPiece(Food, ConsoleColor.Red);

            // Check game rules
            for (var i = 0; i < Body.Count; i++) 
            { 
                for (var j = 0; j < Body.Count; j++) 
                { 
                    if (i != j && Vector2.Equals(Body[i], Body[j])) { Alive = false; } 
                } 
            }

            // Check if Food was eaten
            if ((Body[0].X == Food.X || Body[0].X == Food.X + 1) && (Body[0].Y == Food.Y))
            {
                Body.Add(Body[^1]);
                Food = NewFood();
            }

            // Delay the next frame
            var start = DateTime.Now;
            var newInput = false;
            do { if (!newInput) { newInput = Controls(); } } while (start.AddMilliseconds(100) > DateTime.Now);
        }

        public static bool Controls()
        {
            if (Console.KeyAvailable)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.LeftArrow:  case ConsoleKey.A: if (Dir.Length() != 2) { Dir = new Vector2(-2,  0); } return true;
                    case ConsoleKey.UpArrow:    case ConsoleKey.W: if (Dir.Length() != 1) { Dir = new Vector2( 0, -1); } return true;
                    case ConsoleKey.RightArrow: case ConsoleKey.D: if (Dir.Length() != 2) { Dir = new Vector2( 2,  0); } return true;
                    case ConsoleKey.DownArrow:  case ConsoleKey.S: if (Dir.Length() != 1) { Dir = new Vector2( 0,  1); } return true;
                }
            }
            return false;
        }

        public static float Mod(float a, float b) { return (a - b* (float) Math.Floor(a / b)); }

        public static void DrawPiece(Vector2 v, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.SetCursorPosition(Convert.ToInt16(v.X), Convert.ToInt16(v.Y));
            Console.Write("██");
        }
    }
}
