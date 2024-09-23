using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CGOL
{
    class Program
    {
        public static GOL Game;
        static void Main()
        {
            Game = new GOL();
            while (true) { Game.Run(); }
        }
    }

    class GOL
    {
        public static int Width = 50;
        public static int Height = 50;
        public static List<List<bool>> Grid = [];
        public static List<List<bool>> Next = [];

        public GOL()
        {
            Console.SetWindowSize(Width * 2, Height);
            Console.CursorVisible = false;

            // Initialize the Cells
            for (var x = 0; x < Width; x++)
            {
                var Row = new List<bool>();
                for (var y = 0; y < Height; y++) { Row.Add(false); }

                // Use a Deepcopy for each List to mitigate Crossreferencing
                Grid.Add(new List<bool>(Row));
                Next.Add(new List<bool>(Row));
            }

            // Initialize grid with the famous 'glider' - or 'ant' as John Conway 
            // regretably didn't name it.
            var Glider = new List<int>() { 0, 0, 0, 1, 0, 2, 1, 2, 2, 1 };

            Insert(Width - 5, 2, Glider);
            Insert(Width - 8, 6, Glider);
        }

        public void Run()
        {
            // Reset 'Next' Iteration
            Next.ForEach(Row => Row.ForEach(Cell => Cell = false));
          
            for (var x = 0; x < Grid.Count; x++)
            {
                for (var y = 0; y < Grid[x].Count; y++)
                {
                    // Render Cell
                    DrawCell(x, y, Grid[x][y]);

                    var neighbors = 0;

                    // Get Neighbor count
                    neighbors += Grid[(x + -1 + Width) % Width][(y + -1 + Height) % Height] == true ? 1 : 0;
                    neighbors += Grid[(x + -1 + Width) % Width][(y +  0 + Height) % Height] == true ? 1 : 0;
                    neighbors += Grid[(x + -1 + Width) % Width][(y +  1 + Height) % Height] == true ? 1 : 0;
                    neighbors += Grid[(x +  0 + Width) % Width][(y + -1 + Height) % Height] == true ? 1 : 0;
                    neighbors += Grid[(x +  0 + Width) % Width][(y +  1 + Height) % Height] == true ? 1 : 0;
                    neighbors += Grid[(x +  1 + Width) % Width][(y + -1 + Height) % Height] == true ? 1 : 0;
                    neighbors += Grid[(x +  1 + Width) % Width][(y +  0 + Height) % Height] == true ? 1 : 0;
                    neighbors += Grid[(x +  1 + Width) % Width][(y +  1 + Height) % Height] == true ? 1 : 0;

                    // Evaluate Game Of Life Rules
                    Next[x][y] = neighbors == 3 || Grid[x][y];
                    Next[x][y] = neighbors >= 2 && neighbors <= 3 && Next[x][y];
                }
            }   

            // Get Next Iteration (Deep Copy)
            Grid = Next.Select(x => x.ToList()).ToList();
        }

        public static void Insert(int StartX, int StartY, List<int> Shape)
        {
            for (var i = 0; i < Shape.Count; i += 2)
            {
                Grid[StartX + Shape[i]][StartY + Shape[i + 1]] = true;
            }
        }

        public static void DrawCell(int X, int Y, bool Life)
        {

            Console.SetCursorPosition(Convert.ToInt16(X * 2), Convert.ToInt16(Y));
            Console.ForegroundColor = Life ? ConsoleColor.Green : ConsoleColor.Black;
            Console.Write("██");
        }
    }
}