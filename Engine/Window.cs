using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

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
        public static bool KeyDown(Key key) => game.window.Key_Down(key);
        public static bool KeyPressed(Key key) => game.window.Key_Pressed(key);
        public static bool KeyReleased(Key key) => game.window.Key_Released(key);

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
            InitKeyboard();
            fpsHandler = new FPSHandler(this);
            camera = new Camera(Width, Height);
            ColoredVertexArray.Start();
            Start();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            UpdateKeyboard();

            Update();
            ActorGroup.World.Update();

            fpsHandler.Update();
            Title = $"{titlePrefix} FPS: {fpsHandler.fps.ToString("0.0")}";
        }

        private void InitKeyboard()
        {
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
        }
        
        private void UpdateKeyboard()
        {
            KeysPressed = KeysPressedAsync;
            KeysReleased = KeysReleasedAsync;
            KeysPressedAsync = new List<Key>();
            KeysReleasedAsync = new List<Key>();
        }

        private List<Key> KeysPressed = new List<Key>();
        private List<Key> KeysReleased = new List<Key>();

        private List<Key> KeysPressedAsync = new List<Key>();
        private List<Key> KeysReleasedAsync = new List<Key>();
        private List<Key> KeysDown = new List<Key>();

        private void OnKeyUp(object sender, KeyboardKeyEventArgs args)
        {
            KeysDown.Remove(args.Key);
            if (!KeysReleasedAsync.Contains(args.Key))
                KeysReleasedAsync.Add(args.Key);
        }
        private void OnKeyDown(object sender, KeyboardKeyEventArgs args)
        {
            if (!KeysPressedAsync.Contains(args.Key) && !KeysDown.Contains(args.Key))
                KeysPressedAsync.Add(args.Key);
            if (!KeysDown.Contains(args.Key))
                KeysDown.Add(args.Key);
        }

        public bool Key_Pressed(Key key) => KeysPressed.Contains(key);
        public bool Key_Released(Key key) => KeysReleased.Contains(key);
        public bool Key_Down(Key key) => KeysDown.Contains(key);

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
