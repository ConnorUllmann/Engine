using System;
using System.Linq;
using System.Collections.Generic;
using Basics;
using Basics.QuadTree;
using Engine;
using Engine.Actors;
using OpenTK;
using OpenTK.Graphics;
using Rectangle = Basics.Rectangle;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //SwarmTest.Run();

            var game = new ShellGame(600, 600);

            var position = new Vector2(-2, -4);

            var rectangles = new[]
            {
                new Rectangle(0, 0, 3, 3),
                new Rectangle(0, -3, 3, 3),
                new Rectangle(-3, 0, 3, 3),
                new Rectangle(-6, -6, 3, 3)
            };


            game.StartHandler += () =>
            {
            };

            game.UpdateHandler += () =>
            {
            };

            game.RenderHandler += () =>
            {
                const int multiplier = 20;

                var mouse = Input.Mouse;
                var mouseRectangle = new Rectangle(mouse.X - 10, mouse.Y - 40, 70, 50);

                var pt = Engine.Utils.RaycastAgainstRectangles(rectangles, position, (mouse - multiplier * position).Radians() ?? 0);
                if(pt.HasValue)
                    Engine.Debug.Draw.Circle(multiplier * pt.Value, 2, Color4.Red);
                //foreach (var point in Engine.Utils.PointsRectanglesCollideLine(rectangles, position, mouse / multiplier))
                //    Engine.Debug.Draw.Circle(multiplier * point, 2, Color4.Red);

                foreach (var rectangle in rectangles)
                {
                    Engine.Debug.Draw.Rectangle(rectangle * multiplier, _color: Color4.Green, _filled: false);
                    
                    //foreach (var point in Engine.Utils.PointsRectangleCollidesRectangle(rectangle, mouseRectangle / multiplier))
                    //    Engine.Debug.Draw.Circle(multiplier * point, 2, Color4.Red);
                }

                //Engine.Debug.Draw.Rectangle(mouseRectangle, 0, 0, Color4.Purple, false);

                Engine.Debug.Draw.Line(multiplier * position, mouse, Color4.Blue);
            };

            game.Run();

        }
    }
}
