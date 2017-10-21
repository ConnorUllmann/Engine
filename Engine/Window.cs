using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Diagnostics;

namespace Engine
{
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

    public class Game
    {
        private static Game game;

        public static Camera Camera => game.window.camera;
        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static int PixelWidth => game.window.Width;
        public static int PixelHeight => game.window.Height;
        public static float LastUpdateDurationMilliseconds => game.window.LastUpdateDurationMilliseconds;
        public static float LastRenderDurationMilliseconds => game.window.LastRenderDurationMilliseconds;

        public static float FramesSinceStart => FPSHandler.FramesSinceStart;
        public static float MillisecondsSinceStart => FPSHandler.MillisecondsSinceStart;
        public static float FPS => FPSHandler.FPS;
        public static float Delta => FPSHandler.Delta;

        private Window window;

        public Game(int width, int height, string title="")
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
    }

    internal class Window : GameWindow
    {
        private FPSHandler fpsHandler;
        public Camera camera;

        public static float TimeSinceStart => FPSHandler.MillisecondsSinceStart;
        public static float FPS => FPSHandler.FPS;
        public static float Delta => FPSHandler.Delta;

        public Action Start;
        public Action Update;
        public Action Render;
        
        private Stopwatch updateStopwatch;
        private Stopwatch renderStopwatch;
        public long LastUpdateDurationMilliseconds { get; private set; }
        public long LastRenderDurationMilliseconds { get; private set; }

        private string titlePrefix;

        public Window(int _width, int _height, string _title="") : 
            base(_width, _height, GraphicsMode.Default, _title, GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            titlePrefix = _title;
            updateStopwatch = new Stopwatch();
            renderStopwatch = new Stopwatch();
        }

        protected override void OnResize(EventArgs e) => GL.Viewport(0, 0, Width, Height);

        protected override void OnLoad(EventArgs e)
        {
            Input.Start(this);
            fpsHandler = new FPSHandler(this);
            camera = new Camera(Width, Height);
            ColoredVertexArray.Start();
            Start();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Input.Update();

            updateStopwatch.Reset();
            updateStopwatch.Start();
            Update();
            ActorGroup.World.Update();
            updateStopwatch.Stop();
            LastUpdateDurationMilliseconds = updateStopwatch.ElapsedMilliseconds;

            fpsHandler.Update();
            Title = $"{titlePrefix} {Input.Mouse} FPS: {fpsHandler.fps.ToString("0.0")} Update: {LastUpdateDurationMilliseconds}ms Render: {LastRenderDurationMilliseconds}ms";
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            ClearBuffer();

            renderStopwatch.Reset();
            renderStopwatch.Start();
            Render();
            ActorGroup.World.Render();
            renderStopwatch.Stop();
            LastRenderDurationMilliseconds = renderStopwatch.ElapsedMilliseconds;

            SwapBuffers();
        }

        private void ClearBuffer() => ClearBuffer(Color4.Black);
        private void ClearBuffer(Color4 color)
        {
            GL.ClearColor(color);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
