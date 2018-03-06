using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Engine.OpenGL.Colored;
using OpenTK.Input;
using System.Diagnostics;

namespace Engine
{
    public class Game
    {
        private static Game game;

        public static Camera Camera => game.window.camera;
        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static int PixelWidth => game.window.Width;
        public static int PixelHeight => game.window.Height;
        public static bool PauseIfUnfocused { get => game.window.PauseIfUnfocused; set => game.window.PauseIfUnfocused = value; }
        public static float LastUpdateDurationMilliseconds => game.window.LastUpdateDurationMilliseconds;
        public static float LastRenderDurationMilliseconds => game.window.LastRenderDurationMilliseconds;

        public static float FramesSinceStart => FPSHandler.FramesSinceStart;
        public static float MillisecondsSinceStart => FPSHandler.MillisecondsSinceStart;
        public static float FPS => FPSHandler.FPS;
        public static float Delta => FPSHandler.Delta;

        private Window window;

        public Game(int width, int height, string title = "")
        {
            game = this;
            Width = width;
            Height = height;
            //Not sure why, but the dimensions always seem to inflate by 1.5x... these divisions are done to offset that
            window = new Window((int)(width / 1.5f), (int)(height / 1.5f), title);
            window.Start += Start;
            window.Update += PreUpdate;
            window.Update += Update;
            window.Render += Render;
        }

        public void Run() => window.Run();

        public virtual void Start() { }
        public virtual void PreUpdate() { }
        public virtual void Update() { }
        public virtual void Render() { }

        /// <summary>
        /// Returns a random position on the screen
        /// </summary>
        /// <param name="_horizontalMargin">Distance from the sides of the screen that the point must be</param>
        /// <param name="_verticalMargin">Distance from the top & bottom edges of the screen that the point must be</param>
        /// <returns></returns>
        public static Vector2 RandomPosition(float _horizontalMargin = 0, float _verticalMargin = 0)
            => new Vector2((float)((Basics.Utils.RandomDouble() - 0.5) * (Width - 2 * _horizontalMargin)),
                           (float)((Basics.Utils.RandomDouble() - 0.5) * (Height - 2 * _verticalMargin)));

        /// <summary>
        /// Returns the position to place _position after it has been wrapped around the screen. _margin specifies how much space outside the screen to include (negative values work)
        /// </summary>
        /// <param name="_position">position to wrap</param>
        /// <param name="_margin">pixels of extra space to include outside the screen</param>
        /// <returns></returns>
        public static Vector2 ScreenWrap(Vector2 _position, float _margin = 0) => ScreenWrap(_position, _margin, _margin);
        public static Vector2 ScreenWrap(Vector2 _position, float _marginX, float _marginY)
            => new Vector2((_position.X + Width / 2 + _marginX) % (Width + 2 * _marginX) - Width / 2 - _marginX,
                           (_position.Y + Height / 2 + _marginY) % (Height + 2 * _marginY) - Height / 2 - _marginY);

        /// <summary>
        /// Returns the position to place _position after it has been clamped to the screen. _margin specifies how much space outside the screen to include (negative values work)
        /// </summary>
        /// <param name="_position">position to wrap</param>
        /// <param name="_margin">pixels of extra space to include outside the screen</param>
        /// <returns></returns>
        public static Vector2 ScreenClamp(Vector2 _position, float _margin = 0) => ScreenClamp(_position, _margin, _margin);
        public static Vector2 ScreenClamp(Vector2 _position, float _marginX, float _marginY)
            => new Vector2(Basics.Utils.Clamp(_position.X, -Width / 2 - _marginX, Width / 2 + _marginX),
                           Basics.Utils.Clamp(_position.Y, -Height / 2 - _marginY, Height / 2 + _marginY));
    }

    public class ShellGame : Game
    {
        public Action StartHandler;
        public Action UpdateHandler;
        public Action RenderHandler;

        public ShellGame(int width, int height) : base(width, height, "Test Game") { }

        public override void Start() => StartHandler?.Invoke();
        public override void Update() => UpdateHandler?.Invoke();
        public override void Render() => RenderHandler?.Invoke();
    }
}
