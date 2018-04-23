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
            Singleton.UpdateMouse();
        }

        private void AttachToWindow(Window window)
        {
            window.KeyUp += OnKeyUp;
            window.KeyDown += OnKeyDown;

            window.FocusedChanged += (object sender, EventArgs e) => focused = window.Focused;
            focused = window.Focused;

            //window.MouseEnter += OnMouseEnter;
            //window.MouseLeave += OnMouseLeave;

            window.MouseDown += OnMouseDown;
            window.MouseUp += OnMouseUp;

            MouseHandler += () => new Vector2(window.Mouse.X * 1f / Game.PixelWidth * Game.Width - Game.Width / 2f, Game.Height - (window.Mouse.Y * 1f / Game.PixelHeight * Game.Height) - Game.Height / 2f);
        }

        public static bool Focused => Singleton.focused;
        private bool focused;

        #region Mouse
        private Func<Vector2> MouseHandler;
        /// <summary>
        /// Position of the mouse with respect to the screen ((0, 0) is the center)
        /// </summary>
        public static Vector2 Mouse { get => Singleton.MouseHandler(); }
        public static float AngleToMouse(float _x, float _y)
        {
            var mouse = Mouse;
            return (float) Math.Atan2(mouse.Y - _y, mouse.X - _x);
        }

        private bool leftMousePressedAsync;
        private bool leftMousePressed;
        private bool leftMouseReleasedAsync;
        private bool leftMouseReleased;
        private bool leftMouseDownAsync;
        private bool leftMouseDown;

        private bool rightMousePressedAsync;
        private bool rightMousePressed;
        private bool rightMouseReleasedAsync;
        private bool rightMouseReleased;
        private bool rightMouseDownAsync;
        private bool rightMouseDown;

        public static bool LeftMouseDown { get => Singleton.leftMouseDown; }
        public static bool LeftMousePressed { get => Singleton.leftMousePressed; }
        public static bool LeftMouseReleased { get => Singleton.leftMouseReleased; }

        public static bool RightMouseDown { get => Singleton.rightMouseDown; }
        public static bool RightMousePressed { get => Singleton.rightMousePressed; }
        public static bool RightMouseReleased { get => Singleton.rightMouseReleased; }

        private void UpdateMouse()
        {
            leftMouseDown = leftMouseDownAsync;
            leftMousePressed = leftMousePressedAsync;
            leftMouseReleased = leftMouseReleasedAsync;
            leftMousePressedAsync = false;
            leftMouseReleasedAsync = false;

            rightMouseDown = rightMouseDownAsync;
            rightMousePressed = rightMousePressedAsync;
            rightMouseReleased = rightMouseReleasedAsync;
            rightMousePressedAsync = false;
            rightMouseReleasedAsync = false;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs args)
        {
            switch (args.Button)
            {
                case MouseButton.Left:
                    leftMouseDownAsync = true;
                    leftMousePressedAsync = true;
                    break;
                case MouseButton.Right:
                    rightMouseDownAsync = true;
                    rightMousePressedAsync = true;
                    break;
                default: break;
            }
        }
        private void OnMouseUp(object sender, MouseButtonEventArgs args)
        {
            switch (args.Button)
            {
                case MouseButton.Left:
                    leftMouseDownAsync = false;
                    leftMouseReleasedAsync = true;
                    break;
                case MouseButton.Right:
                    rightMouseDownAsync = false;
                    rightMouseReleasedAsync = true;
                    break;
                default: break;
            }
        }
        
        #endregion

        #region Keyboard
        private List<Key> KeysPressed = new List<Key>();
        private List<Key> KeysReleased = new List<Key>();

        private List<Key> KeysPressedAsync = new List<Key>();
        private List<Key> KeysReleasedAsync = new List<Key>();
        private List<Key> KeysDown = new List<Key>();

        public static bool KeyPressed(Key key) => Singleton.KeysPressed.Contains(key);
        public static bool KeyReleased(Key key) => Singleton.KeysReleased.Contains(key);
        public static bool KeyDown(Key key) => Singleton.KeysDown.Contains(key);

        private void UpdateKeyboard()
        {
            KeysPressed = KeysPressedAsync;
            KeysReleased = KeysReleasedAsync;
            KeysPressedAsync = new List<Key>();
            KeysReleasedAsync = new List<Key>();
        }

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
        #endregion
    }
}
