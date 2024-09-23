using System;
using System.Collections.Generic;
using System.Linq;

namespace CGOL
{
    class Program
    {
        public static GOL Game;
        static void Main(string[] args)
        {
            Game = new GOL();
            while (true) { Game.Run(); }
        }
    }

    class GOL
    {
        public static int Width = 50;
        public static int Height = 50;
        public static List<List<bool>> Grid = new List<List<bool>>();
        public static List<List<bool>> Next = new List<List<bool>>();

        public GOL()
        {
            Console.SetWindowSize(Width * 2, Height);
            Console.CursorVisible = false;

            // Initialize the Cells
            for (int x = 0; x < Width; x++)
            {
                List<bool> Row = new List<bool>();
                for (int y = 0; y < Height; y++) { Row.Add(false); }

                // Use a Deepcopy for each List to mitigate Crossreferencing
                Grid.Add(new List<bool>(Row));
                Next.Add(new List<bool>(Row));
            }

            // Initialize grid with the famous 'glider' - or 'ant' as John Conway 
            // regretably didn't name it.
            List<int> Glider = new List<int>() { 0, 0, 0, 1, 0, 2, 1, 2, 2, 1 };

            Insert(Width - 5, 2, Glider);
            Insert(Width - 8, 6, Glider);
        }

        public void Run()
        {
            // Reset 'Next' Iteration
            Next.ForEach(Row => Row.ForEach(Cell => Cell = false));

            for (int x = 0; x < Grid.Count; x++)
            {
                for (int y = 0; y < Grid[x].Count; y++)
                {
                    // Render Cell
                    DrawCell(x, y, Grid[x][y]);

                    int Neighbors = 0;

                    // Get Neighbor count
                    Neighbors += Grid[(x + -1 + Width) % Width][(y + -1 + Height) % Height] == true ? 1 : 0;
                    Neighbors += Grid[(x + -1 + Width) % Width][(y +  0 + Height) % Height] == true ? 1 : 0;
                    Neighbors += Grid[(x + -1 + Width) % Width][(y +  1 + Height) % Height] == true ? 1 : 0;
                    Neighbors += Grid[(x +  0 + Width) % Width][(y + -1 + Height) % Height] == true ? 1 : 0;
                    Neighbors += Grid[(x +  0 + Width) % Width][(y +  1 + Height) % Height] == true ? 1 : 0;
                    Neighbors += Grid[(x +  1 + Width) % Width][(y + -1 + Height) % Height] == true ? 1 : 0;
                    Neighbors += Grid[(x +  1 + Width) % Width][(y +  0 + Height) % Height] == true ? 1 : 0;
                    Neighbors += Grid[(x +  1 + Width) % Width][(y +  1 + Height) % Height] == true ? 1 : 0;

                    // Evaluate Game Of Life Rules
                    Next[x][y] = Neighbors == 3 ? true : Grid[x][y];
                    Next[x][y] = Neighbors < 2 || Neighbors > 3 ? false : Next[x][y];
                }
            }

            // Get Next Iteration (Deep Copy of 2D List: https://stackoverflow.com/a/16068713)
            Grid = Next.Select(x => x.ToList()).ToList();
        }

        public void Insert(int StartX, int StartY, List<int> Shape)
        {
            for (int i = 0; i < Shape.Count; i += 2)
            {
                Grid[StartX + Shape[i]][StartY + Shape[i + 1]] = true;
            }
        }

        public void DrawCell(int X, int Y, bool Life)
        {
            ConsoleColor C = Life ? ConsoleColor.White : ConsoleColor.Black;
   
            Console.ForegroundColor = C;
            Console.SetCursorPosition(Convert.ToInt16(X * 2), Convert.ToInt16(Y));
            Console.Write("██");
        }
    }
}