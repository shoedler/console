using Craw.ConsoleEngine;

namespace Craw.Shapes
{
    internal class Circle : Shape
    {
        public Circle(Coord start) : base(start)
        {
        }

        public override bool NextState(Coord stop)
        {
            if (State == EShapeState.End)
                return true;

            if (State == EShapeState.Begin)
                State = EShapeState.End;

            return false;
        }

        public override Shape Update(Coord current, EConsolePixelColor color)
        {
            if (State != EShapeState.Begin) return this;

            Pixels = ShapeFunctions.Circle(Start, current, color); // 'current' is the middle of the circle

            return this;
        }
    }
}