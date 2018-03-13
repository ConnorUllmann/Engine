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
using Engine.Actors;

namespace Engine
{
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

        public Color4 BackgroundColor = Color4.Black;
        public bool PauseIfUnfocused = true;

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

            if (!PauseIfUnfocused || Input.Focused)
            {
                updateStopwatch.Reset();
                updateStopwatch.Start();
                Update();
                ActorGroup.World.Update();
                updateStopwatch.Stop();
                LastUpdateDurationMilliseconds = updateStopwatch.ElapsedMilliseconds;
            }

            fpsHandler.Update();
            Title = title ?? $"{titlePrefix} {Input.Mouse} {Game.Delta} FPS: {fpsHandler.fps.ToString("0.0")} Update: {LastUpdateDurationMilliseconds}ms Render: {LastRenderDurationMilliseconds}ms";
        }

        private string title = null;
        public void SetTitle(string _title) => title = _title;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            ClearBuffer();

            renderStopwatch.Reset();
            renderStopwatch.Start();
            ActorGroup.World.Render();
            Render();
            renderStopwatch.Stop();
            LastRenderDurationMilliseconds = renderStopwatch.ElapsedMilliseconds;


            SwapBuffers();
        }

        private void ClearBuffer() => ClearBuffer(BackgroundColor);
        private void ClearBuffer(Color4 color)
        {
            GL.ClearColor(color);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
