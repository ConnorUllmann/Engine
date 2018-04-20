﻿using System;
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
        private FPSTracker fpsTracker;
        private GarbageCollectionTracker gcTracker;
        public Camera camera;

        public static float TimeSinceStart => FPSTracker.MillisecondsSinceStart;
        public static float FPS => FPSTracker.FPS;
        public static float Delta => FPSTracker.Delta;
        public static bool GarbageCollected { get; private set; }

        public Action Start;
        public Action Update;
        public Action PreRender;
        public Action PostRender;

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
            fpsTracker = new FPSTracker();
            gcTracker = new GarbageCollectionTracker();
            camera = new Camera(Width, Height);
            ColoredVertexArray.Start();
            Start();

            //Refresh projection matrix after loading
            camera.RefreshProjectionMatrix();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Input.Update();

            if (!PauseIfUnfocused || Input.Focused)
            {
                updateStopwatch.Reset();
                updateStopwatch.Start();
                Update();
                Game.Group.Update();
                updateStopwatch.Stop();
                LastUpdateDurationMilliseconds = updateStopwatch.ElapsedMilliseconds;
            }

            gcTracker.Update();
            GarbageCollected = gcTracker.GarbageCollectionsByGeneration(2); //2nd generation polling gives best indication of lag

            fpsTracker.Update();
            Title = title ?? $"{titlePrefix} ({(int)Input.Mouse.X}, {(int)Input.Mouse.Y}) FPS: {fpsTracker.fps.ToString("0")} Total: {(Game.Delta * 1000).ToString("0")}ms Update: {LastUpdateDurationMilliseconds}ms Render: {LastRenderDurationMilliseconds}ms";
        }

        private string title = null;
        public void SetTitle(string _title) => title = _title;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            ClearBuffer();

            renderStopwatch.Reset();
            renderStopwatch.Start();

            PreRender();
            Game.Group.Render();
            PostRender();

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
