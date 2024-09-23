using System;
using System.Collections.Generic;
using System.Numerics;

namespace cnake
{
    class Program
    {
        public static Snake S;
        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 20);
            Console.CursorVisible = false;

            S = new Snake();
            while (S.alive) { S.Run(); }
        }
    }

    class Snake
    {
        public static List<Vector2> LBody = new List<Vector2>();
        public static Vector2 vDir = new Vector2(2, 0);
        public static Vector2 vFood = Food();
        public bool alive = true;

        static Snake() { for (int i = 3; i >= 0; i--) { LBody.Add(new Vector2(i, 0)); } }

        public static Vector2 Food()
        {
            Random R = new Random();
            int X = R.Next(0, Console.WindowWidth);
            while (X % 2 == 0) { X = R.Next(0, Console.WindowWidth); }
            return new Vector2(X, R.Next(0, Console.WindowHeight));
        }

        public void Run()
        {

            /* Move Body pieces to their next position */
            for (int i = LBody.Count - 1; i > 0; i--) { LBody[i] = LBody[i - 1]; }

            /* Update head position */
            LBody[0] = new Vector2(Mod((LBody[0].X + vDir.X), Console.WindowWidth), Mod((LBody[0].Y + vDir.Y), Console.WindowHeight));

            /* Render Body pieces at their respective position */
            LBody.ForEach(b => { DrawPiece(b, ConsoleColor.White); });

            /* Clear Trail by drawing last piece in black (This way Console.Clear is not needed), draw Food */
            DrawPiece(LBody[LBody.Count - 1], ConsoleColor.Black);
            DrawPiece(vFood, ConsoleColor.Red);

            /* Check game rules */
            for (int i = 0; i < LBody.Count; i++) 
            { 
                for (int j = 0; j < LBody.Count; j++) 
                { 
                    if (i != j && Vector2.Equals(LBody[i], LBody[j])) { alive = false; } 
                } 
            } 

            /* Check if Food was eaten */
            if ((LBody[0].X == vFood.X || LBody[0].X == vFood.X + 1) && (LBody[0].Y == vFood.Y))
            {
                LBody.Add(LBody[LBody.Count - 1]);
                vFood = Food();
            }

            /* Delay the next frame */
            DateTime dtThen = DateTime.Now;
            bool xNewInput = false;
            do { if (!xNewInput) { xNewInput = Controls(); } } while (dtThen.AddMilliseconds(100) > DateTime.Now);
        }

        public static bool Controls()
        {
            if (Console.KeyAvailable)
            {
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.LeftArrow:  case ConsoleKey.A: if (vDir.Length() != 2) { vDir = new Vector2(-2,  0); } return true;
                    case ConsoleKey.UpArrow:    case ConsoleKey.W: if (vDir.Length() != 1) { vDir = new Vector2( 0, -1); } return true;
                    case ConsoleKey.RightArrow: case ConsoleKey.D: if (vDir.Length() != 2) { vDir = new Vector2( 2,  0); } return true;
                    case ConsoleKey.DownArrow:  case ConsoleKey.S: if (vDir.Length() != 1) { vDir = new Vector2( 0,  1); } return true;
                }
            }
            return false;
        }

        public float Mod(float a, float b) { return (a - b* (float) Math.Floor(a / b)); }

        public void DrawPiece(Vector2 v, ConsoleColor c)
        {
            Console.ForegroundColor = c;
            Console.SetCursorPosition(Convert.ToInt16(v.X), Convert.ToInt16(v.Y));
            Console.Write("██");
        }
    }
}
