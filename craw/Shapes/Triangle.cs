using Craw.ConsoleEngine;

namespace Craw.Shapes
{
    internal class Triangle : Shape
    {
        private Coord Middle;
        public Triangle(Coord start) : base(start) { }
        
        public override bool NextState(Coord stop)
        {
            if (State == EShapeState.End)
                return true;

            if (State == EShapeState.State1)
                State = EShapeState.End;

            if (State == EShapeState.Begin)
                State = EShapeState.State1;

            return false;
        }

        public override Shape Update(Coord current, EConsolePixelColor color)
        {
            switch (State)
            {
                case EShapeState.Begin:
                    Middle = current;
                    Pixels = ShapeFunctions.Line(Start, current, color);
                    break;
                case EShapeState.State1:
                {
                    var line1 = ShapeFunctions.Line(Start,   Middle, color);
                    var line2 = ShapeFunctions.Line(current, Middle, color);
                    var line3 = ShapeFunctions.Line(current, Start,  color);

                    Pixels = new Pixel[line1.Length + line2.Length + line3.Length + 3];

                    for (var i = 0; i < line1.Length; i++) Pixels[i] = line1[i];
                    for (var i = 0; i < line2.Length; i++) Pixels[i + line1.Length] = line2[i];
                    for (var i = 0; i < line3.Length; i++) Pixels[i + line1.Length + line2.Length] = line3[i];
                    break;
                }
            }

            return this;
        }
    }

}