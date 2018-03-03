using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using OpenTK;

namespace Engine
{
    public static class Vector3Pool
    {
        private static Pool<Vector3> Global = new Pool<Vector3>(() => Vector3.Zero);

        public static Vector3 Get(float _x, float _y, float _z)
        {
            var v = Global.Get();
            v.X = _x;
            v.Y = _y;
            v.Z = _z;
            return v;
        }

        public static void Add(Vector3 _o) => Global.Add(_o);
    }
}
