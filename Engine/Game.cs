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
using Basics;
using Engine.Actors;

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

        public static string Title { set => game.window.SetTitle(value); }

        //TODO: make this a configurable option instead of a const string
        private const string logPath = "..\\Logs\\basics.log";
        private Log log;
        private ActorGroup group;
        public static ActorGroup Group => game.group;

        public static Color4 BackgroundColor
        {
            get => game.backgroundColor;
            set => game.backgroundColor = value;
        }
        private Color4 backgroundColor
        {
            get => window.BackgroundColor;
            set => window.BackgroundColor = value;
        }

        private Window window;

        public Game(int width, int height, string title = "", Log.Level _threshold=Log.Level.Error)
        {
            game = this;
            Width = width;
            Height = height;
            
            log = new Log(logPath, false, true, _threshold);
            group = new ActorGroup(log);

            initializeWindow(title);
        }

        private void initializeWindow(string title)
        {
            //TODO: Figure out why, on some computers, the dimensions are inflated (e.g. by 1.5x, so the width/height must be divided by 1.5)
            window = new Window(Width, Height, title ?? "");
            window.Start += Start;
            window.Update += PreUpdate;
            window.Update += Update;
            window.Render += Render;
            window.Closing += Closing;
        }

        private void Closing(object sender, System.ComponentModel.CancelEventArgs e) => LogFlush();

        public static Log.Level LogThreshold { get => game.log.Threshold; set => game.log.Threshold = value; }
        public static bool LogShouldPrintToFile { get => game.log.ShouldPrintToFile; set => game.log.ShouldPrintToFile = value; }
        public static bool LogShouldPrintToConsole { get => game.log.ShouldPrintToConsole; set => game.log.ShouldPrintToConsole = value; }
        public static void LogDebug(string line) => game.log.Debug(line);
        public static void LogInfo(string line) => game.log.Info(line);
        public static void LogWarning(string line) => game.log.Warning(line);
        public static void LogError(string line) => game.log.Error(line);
        public static void LogCritical(string line) => game.log.Critical(line);
        public static void LogFlush() => game.log.Flush();

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
            => new Vector2(RandomX(_horizontalMargin), RandomY(_verticalMargin));

        /// <summary>
        /// Returns a random x-coordinate on the screen
        /// </summary>
        /// <param name="_horizontalMargin">Distance from the left & right sides of the screen that the coordinate must be</param>
        /// <returns></returns>
        public static float RandomX(float _horizontalMargin = 0)
            => (float)((Basics.Utils.RandomDouble() - 0.5) * (Width - 2 * _horizontalMargin));

        /// <summary>
        /// Returns a random y-coordinate on the screen
        /// </summary>
        /// <param name="_verticalMargin">Distance from the top & bottom of the screen that the coordinate must be</param>
        /// <returns></returns>
        public static float RandomY(float _verticalMargin = 0)
            => (float)((Basics.Utils.RandomDouble() - 0.5) * (Height - 2 * _verticalMargin));

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
