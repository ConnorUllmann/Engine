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

        public static bool KeyPressed(Key key) => Singleton.KeysPressed.Contains(key);
        public static bool KeyReleased(Key key) => Singleton.KeysReleased.Contains(key);
        public static bool KeyDown(Key key) => Singleton.KeysDown.Contains(key);
    }
}
