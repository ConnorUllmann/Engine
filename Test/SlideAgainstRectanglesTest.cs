using System;
using System.Collections.Generic;
using System.Linq;
using Basics;
using Engine.Actors;
using Basics.QuadTree;
using Engine;
using OpenTK;
using OpenTK.Graphics;
using Rectangle = Basics.Rectangle;

namespace Test
{
    public class SlideAgainstRectanglesTest
    {
        //Regular expressions for converting tests into list
        //\[InlineData\((.*), (.*), (.*), (.*), (.*), (.*)\)\] //(.*)
        //\($1f, $2f, $3f, $4f, $5f, $6f, "$7"\),
        private static readonly List<(float ax, float ay, float bx, float by, float expectedX, float expectedY, string comment)> Tests =
            new List<(float ax, float ay, float bx, float by, float expectedX, float expectedY, string comment)>
            {
                (-57f, -33f, -40f, 25f, -21.27584f, 0f, "Automatically generated from working algorithm"),
                (-106f, 21f, -40f, 25f, -60f, 43.82458f, "Automatically generated from working algorithm"),
                (-197f, -177f, 30f, 76f, -26.30333f, 97.5562f, "Automatically generated from working algorithm"),
                (-178f, -153f, 0f, 24f, -10.8501f, 0f, "Automatically generated from working algorithm"),
                (175f, 41f, -47.00002f, -16.99997f, 21.65067f, -70.01919f, "Automatically generated from working algorithm"),
                (175f, 41f, 8f, 30f, 60f, -18.68753f, "Automatically generated from working algorithm"),
                (175f, 41f, -22f, -84f, 1.66798f, -97.0127f, "Automatically generated from working algorithm"),
                (175f, 41f, -107f, 71f, -100.272f, 77.05021f, "Automatically generated from working algorithm"),
                (-12f, -187f, 7f, 21f, 0f, 0f, "Automatically generated from working algorithm"),
                (0f, 0f, -30f, 0f, -30f, 0f, "From corner vertical"),
                (0f, 0f, 0f, -30f, 0f, -30f, "From corner horizontal"),
                (0f, -30f, 0f, 30f, 0f, 0f, "Into wall vertical"),
                (-30f, 0f, 30f, 0f, 0f, 0f, "Into wall horizontal"),
                (0f, 90f, 0f, 30f, -30f, 60f, "Into crease vertical"),
                (90f, 0f, 30f, 0f, 60f, 30f, "Into crease horizontal"),
                (-90f, -30f, -30f, -30f, -30f, -30f, "Open space"),
                (-30f, -45f, -30f, -15f, -30f, -15f, "Open space vertical"),
                (-45f, -30f, -15f, -30f, -15f, -30f, "Open space horizontal"),
                (0f, 0f, 0f, 0f, 0f, 0f, "Length zero"),
                (0f, -60f, 20f, 200f, 0f, 0f, "Corner into wall"),
                (0f, -60f, -30f, -90f, -30f, -90f, "Corner away from wall"),
                (0f, -30f, 20f, 200f, 0f, 0f, "On wall into wall upward"),
                (-30f, 0f, 200f, 20f, 0f, 0f, "On wall into right wall"),
            };
        private static int visibleTestIndexPrevious = int.MinValue;
        private static int visibleTestIndex = 0;

        public static void Run()
        {
            var game = new ShellGame(600, 600);

            var first = new Vector2(0, 0);
            Vector2? second = new Vector2(0, -30);

            game.StartHandler += () =>
            {
            };

            game.UpdateHandler += () =>
            {
                if (Input.RightMouseDown)
                    first = Input.Mouse;
            };

            game.RenderHandler += () =>
            {
                var rectangles = new[]
                {
                    new Rectangle(0, 0, 60, 60),
                    new Rectangle(0, -60, 60, 60),
                    new Rectangle(-60, 0, 60, 60),
                    new Rectangle(-120, -120, 60, 60)
                };

                if (Input.LeftMousePressed)
                {
                    if (second == null)
                        second = Input.Mouse;
                    else
                        second = null;
                }

                if (Input.KeyPressed(OpenTK.Input.Key.Left))
                    visibleTestIndex = (visibleTestIndex - 1 + Tests.Count) % Tests.Count;
                if (Input.KeyPressed(OpenTK.Input.Key.Right))
                    visibleTestIndex = (visibleTestIndex + 1) % Tests.Count;
                var test = Tests[visibleTestIndex];
                if (visibleTestIndexPrevious != visibleTestIndex)
                {
                    first = new Vector2(test.ax, test.ay);
                    second = new Vector2(test.bx, test.by);
                    visibleTestIndexPrevious = visibleTestIndex;

                    var displayFormat = "0.";
                    Game.LogInfo($"[{visibleTestIndex}] {test.comment}");
                    Game.LogInfo($"   start: ({test.ax.ToString(displayFormat)}, {test.ay.ToString(displayFormat)})", ConsoleColor.DarkYellow);
                    Game.LogInfo($"     end: ({test.bx.ToString(displayFormat)}, {test.by.ToString(displayFormat)})", ConsoleColor.DarkYellow);
                    Game.LogInfo($"expected: ({test.expectedX.ToString(displayFormat)}, {test.expectedY.ToString(displayFormat)})", ConsoleColor.DarkYellow);
                    var actual = Engine.Utils.SlideAgainstRectangles(rectangles, first, second.Value);
                    Game.LogInfo($"  actual: ({actual.X.ToString(displayFormat)}, {actual.Y.ToString(displayFormat)})", ConsoleColor.Magenta);
                    var correct = actual.X.ToString("0.00") == test.expectedX.ToString("0.00") && actual.Y.ToString("0.00") == test.expectedY.ToString("0.00");
                    Game.LogInfo($"{(correct ? "Correct" : "Incorrect")}", correct ? ConsoleColor.Green : ConsoleColor.Red);
                    Game.LogInfo(string.Empty);
                }

                var tempSecond = second ?? Input.Mouse;
                var collisionPoint = Engine.Utils.SlideAgainstRectangles(rectangles, first, tempSecond);

                //Save line for test case to the log
                if (Input.KeyReleased(OpenTK.Input.Key.T))
                    Game.LogInfo($"[InlineData({first.X}, {first.Y}, {tempSecond.X}, {tempSecond.Y}, {collisionPoint.X}, {collisionPoint.Y})] //Automatically generated from working algorithm");

                foreach (var rectangle in rectangles)
                    Engine.Debug.Draw.Rectangle(rectangle, _color: Color4.Blue, _filled: false);

                Engine.Debug.Draw.Arrow(first, tempSecond, Color4.Gold);
                Engine.Debug.Draw.Circle(test.expectedX, test.expectedY, 3, Color4.Gold);
                Engine.Debug.Draw.Circle(collisionPoint, 4, Color4.Fuchsia);
            };

            game.Run();

        }
    }
}
