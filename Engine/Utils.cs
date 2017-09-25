using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace Engine
{
    public static class Utils
    {
        public static void Rotate(this List<Vector3> _vertices, float _angleRad, Vector3 _center)
        {
            for (var i = 0; i < _vertices.Count; i++)
                _vertices[i] = Rotate(_vertices[i], _angleRad, _center);
        }

        public static Vector3 Rotate(Vector3 _vertex, float _angle, Vector3 _center)
        {
            var diff = _vertex - _center;
            diff = new Vector3(
                (float)(diff.X * Math.Cos(_angle) - diff.Y * Math.Sin(_angle)),
                (float)(diff.Y * Math.Cos(_angle) + diff.X * Math.Sin(_angle)),
                diff.Z);
            return diff + _center;
        }
    }
}
