using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

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
        private Window window;

        public static Camera Camera => game.window.camera;
        public static int Width => game.window.Width;
        public static int Height => game.window.Height;

        public static float MillisecondsSinceStart => FPSHandler.MillisecondsSinceStart;
        public static float FPS => FPSHandler.FPS;
        public static float Delta => FPSHandler.Delta;

        public Game(int width, int height, string title="")
        {
            game = this;
            window = new Window(width, height, title);
            window.Start += Start;
            window.Update += Update;
            window.Render += Render;
        }

        public void Run() => window.Run();

        public virtual void Start() { }
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

        private string titlePrefix;

        public Window(int _width, int _height, string _title="") : 
            base(_width, _height, GraphicsMode.Default, _title, GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            titlePrefix = _title;
        }

        protected override void OnResize(EventArgs e) => GL.Viewport(0, 0, Width, Height);

        protected override void OnLoad(EventArgs e)
        {
            fpsHandler = new FPSHandler(this);
            camera = new Camera(Width, Height);
            ColoredVertexArray.Start();
            Start();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Update();
            ActorGroup.World.Update();
            fpsHandler.Update();
            Title = $"{titlePrefix} FPS: {fpsHandler.fps.ToString("0.0")}";
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            ClearBuffer();
            Render();
            ActorGroup.World.Render();
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
