using System;
using System.Collections.Generic;
using Craw.ConsoleEngine;

namespace Craw.Shapes
{
    internal class Pixel
    {
        public Coord Coordinate;
        public CharItem Data;

        public Pixel(short x, short y, int unicode, short attr = 15)
        {
            Coordinate = new Coord(x, y);
            Data = new CharItem()
            {
                Char = { UnicodeChar = (char)unicode },
                Attributes = attr
            };
        }
    }

    public enum EShapeKind
    {
        Rectangle = 0,
        Triangle = 1,
        Circle = 2,
        Line = 3,
    }

    public enum EShapeState
    {
        Begin  = 0,
        State1 = 1,
        State2 = 2,
        State3 = 3,
        State4 = 4,
        State5 = 5,
        End    = 98,
        Error  = 99,
    }

    internal abstract class Shape
    {
        public Coord Start;
        public Pixel[] Pixels;
        public EShapeState State = EShapeState.Begin;

        internal Shape(Coord start)
        {
            Start = start;
            Pixels = new Pixel[1];
            Pixels[0] = new Pixel(start.X, start.Y, 'X');
        }

        public abstract Shape Update(Coord current, EConsolePixelColor color);
        public abstract bool NextState(Coord stop);

        public void Put(ref ConsoleFrame frame)
        {
            foreach (var p in Pixels)
            {
                if (p != null)
                    frame[p.Coordinate].Set(p.Data.Character, p.Data.Color);
            }
        }
    }

    internal static class ShapeFunctions 
    {
        public static char GetAngleChar(double angle)
        {
            if (angle > 180)
                angle -= 180;

            const double inc = 22.5;

            return angle switch
            {
                >= inc and <= 90 - inc or < -135 + inc and > -180 + inc => '╲',
                <= -inc and >= -90 + inc or >= 135 - inc and <= 180 - inc => '╱',
                >= 90 - inc and <= 135 - inc or <= -90 + inc and >= -135 + inc => '│',
                _ => '─'
            };
        }

        public static Pixel[] Circle(Coord center, Coord perimeter, EConsolePixelColor color)
        {
            var dist = (int)Math.Ceiling(center.DistanceTo(perimeter));

            List<Pixel> pixels = new();

            if (dist < 2)
                return pixels.ToArray();

            var inc = dist < 5 ? 0.3d : 0.1d;
            
            int x0 = center.X;
            int y0 = center.Y;

            for (var i = 0.0d; i < Math.PI * 2; i += inc)
            {
                var x1 = (int)(x0 + dist * Math.Cos(i));

                // Use twice the x value since the cells are 2:1
                x1 += x1;
                x1 -= x0;

                var y1 = (int)(y0 + dist * Math.Sin(i));

                pixels.Add(new Pixel((short)x1, (short)y1, GetAngleChar(i * 180 / Math.PI), (short)color));
            }

            return pixels.ToArray();
        }


        public static Pixel[] Line(Coord A, Coord B, EConsolePixelColor color)
        {
            var dist = (int)Math.Ceiling(A.DistanceTo(B));

            var pixels = new Pixel[dist + 1];

            if (dist == 0)
                return pixels;

            int x0 = A.X; int x1 = B.X;
            int y0 = A.Y; int y1 = B.Y;

            var angle = A.AngleTo(B);
            var line = GetAngleChar(angle);

            // https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
            var dx =  Math.Abs(x1 - x0);
            var dy = -Math.Abs(y1 - y0);

            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;

            var err = dx + dy;  
            var c = 0;

            while (true) 
            {
                pixels[c++] = new Pixel((short)x0, (short)y0, line, (short)color);

                if (x0 == x1 && y0 == y1) 
                    return pixels;
                    
                var e2 = 2*err;

                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }

                if (e2 > dx) continue;

                err += dx;
                y0 += sy;
            }
        }
    }
}
