using System;
using System.Diagnostics;
using System.Collections.Generic;
using Craw.Shapes;
using Craw.ConsoleEngine;

namespace Craw
{
    class CrawController
    {
        public Craw.ConsoleEngine.ConsoleEngine Engine;
        public bool ShouldRun = false;

        private readonly List<Shape> Shapes = new();
        private readonly List<Shape> ShapeHistory = new();

        private Coord CursorPos;
        private Shape ActiveShape;
        private EConsolePixelColor ActiveColor = EConsolePixelColor.White;
        private EShapeKind ActiveShapeKind = EShapeKind.Rectangle;

        private float Fps;
        private double FrameTimeMs;
        private bool ToggleGrid;

        // Config Values
        private const char CursorChar = '◌'; //˟○◌⌂
        private const char SelectorChar = '⌂'; //◊∆¯^⌂
        private const string UiTitle = "ᴄᴚᴀᴡ";
        private const EConsolePixelColor UiColor = EConsolePixelColor.White;

        // Combak
        // Title alternatives: ϽЯ∆ꟿ ϽЯ∆Ш ꞆꞦѦꟿ ₡ƦȺШ ᴄᴚᴀᴡ - ϽЯᴧ₡₳∆ꞦꟿꭗףּɄШΔѦȺƦ
        // Chars:              ↕↔←↑→↓∞ █
        // TODO: Implement key [x] -> x, followed by numbers and then enter moves the cursor to the given x pos
        // TODO: Implement key [y] -> y, followed by numbers and then enter moves the cursor to the given y pos

        public CrawController()
        {
            var width = Console.WindowWidth;
            var height = Console.WindowHeight;

            CursorPos = new Coord((short)(width / 2), (short)(height / 2));

            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            Console.CursorVisible = false;

            Engine = new ConsoleEngine.ConsoleEngine((short)width, (short)height);
        }

        public void MoveCursor(short xOffset, short yOffset)
        {
            var outsideXBounds = CursorPos.X + xOffset < 1 || CursorPos.X + xOffset > Engine.Width - 1;
            var outsideYBounds = CursorPos.Y + yOffset < 1 || CursorPos.Y + yOffset > Engine.Height - 1;

            if (outsideXBounds || outsideYBounds) return;

            CursorPos.X += xOffset;
            CursorPos.Y += yOffset;
        }

        private void PutGrid(ref ConsoleFrame frame)
        {
            for (var x = 1; x < Engine.Width; x++)
            {
                for (var y = 1; y < Engine.Height; y++)
                {
                    frame[x, y].Set(x % 2 == 0 ? '─' : '┼', EConsolePixelColor.Gray);
                }
            }
        }

        private ConsoleFrame GetUiFrameForColors()
        {
            const string heading = "[C]olors";
            var colorCount = Enum.GetNames(typeof(EConsolePixelColor)).Length;
            var sectionWidth = Math.Max(heading.Length, colorCount);

            var frame = new ConsoleFrame((short)sectionWidth, 3);

            // Draw heading
            frame.InsertString(0, 0, "[C]olors", UiColor);

            // Draw Color swatches
            for (var i = 0; i < colorCount; i++)
                frame[i, 1].Set('█', (EConsolePixelColor)i);

            // Draw selector
            frame[(int)ActiveColor, 2].Set(SelectorChar, EConsolePixelColor.White);

            return frame;
        }

        private ConsoleFrame GetUiFrameForShapes()
        {
            const string heading = "[S]hapes";
            var shapeKindCount = Enum.GetNames(typeof(EShapeKind)).Length;
            var sectionWidth = Math.Max(heading.Length, shapeKindCount);

            var frame = new ConsoleFrame((short)sectionWidth, 3);

            // Draw heading
            frame.InsertString(0, 0, "[S]hapes", UiColor);

            // Draw Shape kinds
            for (var i = 0; i < shapeKindCount; i++)
            {
                var s = i switch
                {
                    (int)EShapeKind.Rectangle => '□',
                    (int)EShapeKind.Triangle => '∆',
                    (int)EShapeKind.Line => '∕',
                    (int)EShapeKind.Circle => '○',
                    _ => '?'
                };

                frame[i, 1].Set(s, UiColor);
            }

            // Draw selector
            frame[(int)ActiveShapeKind, 2].Set(SelectorChar, EConsolePixelColor.White);

            return frame;
        }

        private ConsoleFrame GetUiFrameForOtherControls()
        {
            const string undoRedoString = "[Z]: Undo / [U]: Redo";
            const string toggleGridString = "[G]: Toggle grid";
            const string beginEndShapeString = "[Space]: Begin / End Shape";
            var sectionWidth = Math.Max(Math.Max(undoRedoString.Length, toggleGridString.Length), beginEndShapeString.Length);

            var frame = new ConsoleFrame((short)sectionWidth, 3);

            // Draw text
            frame.InsertString(0, 0, undoRedoString, UiColor);
            frame.InsertString(0, 1, toggleGridString, UiColor);
            frame.InsertString(0, 2, beginEndShapeString, UiColor);

            return frame;
        }

        private ConsoleFrame GetUiFrameForInfo()
        {
            var truncatedFps = Fps.ToString("0.00");
            truncatedFps = Fps >= 1e4 ? ">10k" : truncatedFps;

            var truncatedFrameTimeMs = FrameTimeMs.ToString("0.000");
            truncatedFrameTimeMs = truncatedFrameTimeMs.Length > 7 ? truncatedFrameTimeMs.Substring(0, 7) + "..." : truncatedFrameTimeMs;

            var fpsString = "fps: " + truncatedFps;
            var frameTimeString = "frametime [ms]: " + truncatedFrameTimeMs;
            var sectionWidth = Math.Max(fpsString.Length, frameTimeString.Length);

            var frame = new ConsoleFrame((short)sectionWidth, 3);

            // Draw text
            frame.InsertString(0, 0, fpsString, UiColor);
            frame.InsertString(0, 1, frameTimeString, UiColor);

            return frame;
        }

        private void PutUi(ref ConsoleFrame currentFrame)
        {
            // const int verticalHeight = 3;
            // var colorCount = Enum.GetNames(typeof(EConsolePixelColor)).Length;
            // var shapeKindCount = Enum.GetNames(typeof(EShapeKind)).Length;

            // var horizontalBar = new string('═', Engine.Width - 3);
            // var verticalBar = new string('║', verticalHeight);
            // var verticalSpacer = '╤' + new string('│', verticalHeight) + '╧';

            // // Draw main border
            // frame.InsertString(1, 1, '╔' + horizontalBar + '╗', UiColor);
            // frame.InsertString(1, verticalHeight + 2, '╚' + horizontalBar + '╝', UiColor);
            // frame.InsertString(1, 2, verticalBar, UiColor, true);
            // frame.InsertString(Engine.Width - 1, 2, verticalBar, UiColor, true);

            // frame.InsertString(2, 1, UiTitle, UiColor);

            var uiFrames = new ConsoleFrame[] {
                GetUiFrameForColors(),
                GetUiFrameForShapes(),
                GetUiFrameForOtherControls(),
                GetUiFrameForInfo()
            };

            // Place the frames next to each other, if we don't have enough space, we'll just
            // place them underneith each other. We need to keep track of the height of the
            // highest frame of the previous row, so we can place the next row at the correct
            // height.

            var currentX = 2;
            var currentY = 1;
            var highestFrameHeight = 0;

            foreach (var f in uiFrames)
            {
                if (currentX + f.Width >= Engine.Width)
                {
                    currentX = 2;
                    currentY += highestFrameHeight + 1;
                    highestFrameHeight = 0;
                }

                currentFrame.InsertFrame(currentX, currentY, f);
                currentX += f.Width + 1;
                highestFrameHeight = Math.Max(highestFrameHeight, f.Height);
            }

            // Draw Cursor
            currentFrame[CursorPos].Set(CursorChar, EConsolePixelColor.White);
            currentFrame.InsertString(CursorPos.X + 1, CursorPos.Y + 1, CursorPos.X + "/" + CursorPos.Y, EConsolePixelColor.Gray);
        }

        public void Run()
        {
            var sw = new Stopwatch();
            const int fpsFreqMs = 100; // Recalculate fps every `fpsFreqMs` miliseconds
            var fpsAccumulator = 0d;

            while (ShouldRun)
            {
                try
                {
                    sw.Restart();

                    if (Console.KeyAvailable)
                    {
                        var action = Console.ReadKey();
                        switch (action.Key)
                        {
                            case ConsoleKey.LeftArrow: MoveCursor(-1, 0); break;
                            case ConsoleKey.RightArrow: MoveCursor(1, 0); break;
                            case ConsoleKey.UpArrow: MoveCursor(0, -1); break;
                            case ConsoleKey.DownArrow: MoveCursor(0, 1); break;
                            case ConsoleKey.Spacebar: CursorAction(); break;
                            case ConsoleKey.C: ChangeColor(); break;
                            case ConsoleKey.S: ChangeShapeKind(); break;
                            case ConsoleKey.Z: UndoShape(); break;
                            case ConsoleKey.U: RedoShape(); break;
                            case ConsoleKey.G: ToggleGrid = !ToggleGrid; break;
                            default: break;
                        }
                    }

                    Render();

                    FrameTimeMs = sw.Elapsed.TotalMilliseconds;
                    fpsAccumulator += FrameTimeMs;

                    if (!(fpsAccumulator > fpsFreqMs)) continue;

                    Fps = 1000 * (float)fpsAccumulator / fpsFreqMs;
                    fpsAccumulator = 0;
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.WriteLine("An Error occured during Runtime");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("\n");
                    Console.WriteLine("Press any key to restart...");
                    Console.ReadLine();
                }
            }
        }

        private void CursorAction()
        {
            // Create / Finish ActiveShape
            if (ActiveShape != null)
            {
                ActiveShape.NextState(CursorPos);

                if (ActiveShape.State != EShapeState.End) return;

                Shapes.Add(ActiveShape);
                ActiveShape = null;
            }
            else
            {
                ActiveShape = ActiveShapeKind switch
                {
                    EShapeKind.Triangle => new Triangle(CursorPos),
                    EShapeKind.Rectangle => new Rectangle(CursorPos),
                    EShapeKind.Circle => new Circle(CursorPos),
                    EShapeKind.Line => new Line(CursorPos),
                    _ => new Rectangle(CursorPos)
                };
            }
        }

        private void ChangeColor()
        {
            var currentColor = (int)ActiveColor;
            currentColor += 1;
            currentColor %= Enum.GetNames(typeof(EConsolePixelColor)).Length;
            ActiveColor = (EConsolePixelColor)currentColor;
        }

        private void ChangeShapeKind()
        {
            var currentShapeKind = (int)ActiveShapeKind;
            currentShapeKind++;
            currentShapeKind %= Enum.GetNames(typeof(EShapeKind)).Length;
            ActiveShapeKind = (EShapeKind)currentShapeKind;
        }

        private void UndoShape()
        {
            if (Shapes.Count <= 0) return;

            var s = Shapes[^1];
            Shapes.Remove(s);
            ShapeHistory.Add(s);
        }

        private void RedoShape()
        {
            if (ShapeHistory.Count <= 0) return;

            var s = ShapeHistory[^1];
            ShapeHistory.Remove(s);
            Shapes.Add(s);
        }

        private void Render()
        {
            var frame = Engine.CreateFrame();

            // Draw the grid first, if it's toggled on
            if (ToggleGrid)
            {
                PutGrid(ref frame);
            }

            // Fill frame buffer with Data from all Recipes - implicitly prioritizing the
            // higher indices
            Shapes.ForEach(recipe => recipe.Put(ref frame));

            // Fill frame buffer with Data from the Active shape
            if (ActiveShape != null)
            {
                ActiveShape.Update(CursorPos, ActiveColor); // Update Active Shape with current Cursor Position
                ActiveShape.Put(ref frame);
            }

            // Fill frame buffer with Data from UI Layer
            PutUi(ref frame);

            Engine.Write(frame);
        }
    }
}
