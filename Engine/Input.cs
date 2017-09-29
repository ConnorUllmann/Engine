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
    public class Input
    {
        private static Input Singleton;

        internal static void Start(Window window)
        {
            Singleton = new Input();
            Singleton.AttachToWindow(window);
        }

        internal static void Update()
        {
            Singleton.UpdateKeyboard();
        }

        private void AttachToWindow(Window window)
        {
            window.KeyUp += OnKeyUp;
            window.KeyDown += OnKeyDown;
            window.MouseEnter += OnMouseEnter;
            window.MouseLeave += OnMouseLeave;
            window.MouseDown += OnMouseDown;
            window.MouseUp += OnMouseUp;
            
            MouseHandler += () => new Vector2(window.Mouse.X * 1f / Game.PixelWidth * Game.Width, window.Mouse.Y * 1f / Game.PixelHeight * Game.Height);
        }

        #region Mouse
        private Func<Vector2> MouseHandler;
        public static Vector2 Mouse { get => Singleton.MouseHandler(); }

        public bool LeftMouseDown { get; private set; }
        public bool RightMouseDown { get; private set; }
        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            switch (args.Button)
            {
                case MouseButton.Left:
                    LeftMouseDown = true;
                    break;
                case MouseButton.Right:
                    RightMouseDown = true;
                    break;
                default: break;
            }
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            switch (args.Button)
            {
                case MouseButton.Left:
                    LeftMouseDown = false;
                    break;
                case MouseButton.Right:
                    RightMouseDown = false;
                    break;
                default: break;
            }
        }

        public bool Focused { get; private set; }
        private void OnMouseEnter(object sender, EventArgs args) => Focused = true;
        private void OnMouseLeave(object sender, EventArgs args) => Focused = false;
        #endregion

        #region Keyboard
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

        public static bool KeyPressed(Key key) => Singleton.KeysPressed.Contains(key);
        public static bool KeyReleased(Key key) => Singleton.KeysReleased.Contains(key);
        public static bool KeyDown(Key key) => Singleton.KeysDown.Contains(key);
        #endregion
    }
}
