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

            var position = new Vector2(-40, -80);

            game.StartHandler += () =>
            {
            };

            game.UpdateHandler += () =>
            {
                if (Input.LeftMouseDown)
                    position = Input.Mouse;
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

                var mouse = Input.Mouse;

                var collisionPoint = Engine.Utils.SlideAgainstRectangles(rectangles, position, mouse);
                Engine.Debug.Draw.Circle(collisionPoint, 2, Color4.Red);

                foreach (var rectangle in rectangles)
                    Engine.Debug.Draw.Rectangle(rectangle, _color: Color4.Green, _filled: false);

                Engine.Debug.Draw.Line(position, mouse, Color4.Blue);
            };

            game.Run();

        }
    }
}
