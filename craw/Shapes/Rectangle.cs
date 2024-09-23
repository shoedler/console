using Craw;
using Craw.ConsoleEngine;

namespace Craw.Shapes
{
    internal class Rectangle : Shape
    {
        public Rectangle(Coord start) : base(start) { }

        public override bool NextState(Coord stop)
        {
            if (State == EShapeState.End)
                return true;

            State = EShapeState.End;
            return false;
        }

        public override Shape Update(Coord current, EConsolePixelColor color)
        {
            // Find top left and bottom right coordinates of the current Rectangle (Start to stop)
            var xLeft = Start.X < current.X ? Start.X : current.X;
            var xRight = current.X > Start.X ? current.X : Start.X;
            var yTop = Start.Y < current.Y ? Start.Y : current.Y;
            var yBottom = current.Y > Start.Y ? current.Y : Start.Y;

            var width = xRight - xLeft;
            var height = yBottom - yTop;

            if (width < 1 || height < 1) 
                return null;

            Pixels = new Pixel[(width + 1) * (height + 1)];

            var pixelCount = 0;
            var pixelChar = ' ';

            for (var x = xLeft; x <= xRight; x++)
            {
                for (var y = yTop; y <= yBottom; y++)
                {
                    if (x == xLeft)
                        pixelChar = (y == yTop) ? '┌' : (y == yBottom) ? '└' : '│';
                    else if (x == xRight)
                        pixelChar = (y == yTop) ? '┐' : (y == yBottom) ? '┘' : '│';
                    else if (y == yTop || y == yBottom)
                        pixelChar = '─';
                    else
                        pixelChar = ' ';

                    Pixels[pixelCount++] = new Pixel(x, y, pixelChar, (short)color);
                }
            }

            return this;
        }
    }
}