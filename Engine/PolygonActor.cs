using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Linq;
using Engine.Actors;

namespace Engine
{
    public class PolygonActor : Actor
    {
        public Polygon Polygon;

        public PolygonFillRenderer FillRenderer;
        public PolygonOutlineRenderer OutlineRenderer;

        public bool FillVisible = true;
        public bool OutlineVisible = true;

        public PolygonActor(Polygon _polygon, float _x = 0, float _y = 0, bool _center = true, bool _filled=true, bool _outlined=true) 
            : base(_x, _y)
        {
            Polygon = _polygon;
            if (_center)
                Center();
            if(_filled)
                FillRenderer = new PolygonFillRenderer(Polygon, X, Y, ColorExtensions.RandomColor());
            if(_outlined)
                OutlineRenderer = new PolygonOutlineRenderer(Polygon, X, Y, ColorExtensions.RandomColor());
        }

        public void UpdateBoundingBoxToMatchPolygon() => BoundingBox.MatchPositionAndDimensions(Polygon.BoundingRectangle());

        private void Center()
        {
            var com = Polygon.CenterOfMass;
            X += com.X;
            Y += com.Y;
            Polygon.Move(-com);
        }

        public void Rotate(float _angleRad, Vector3? _center = null)
        {
            var centerRelative = _center.HasValue
                ? _center.Value - new Vector3(X, Y, 0)
                : Vector3.Zero;
            var centerAbsolute = _center.HasValue
                ? _center.Value
                : Vector3.Zero;
            Polygon.Rotate(_angleRad, centerRelative);
            FillRenderer?.Rotate(_angleRad, centerAbsolute);
            OutlineRenderer?.Rotate(_angleRad, centerAbsolute);
        }

        public IEnumerable<Polygon> SplitAlongLine(Vector3 _a, Vector3 _b)
        {
            var v = new Vector3(X, Y, 0);
            var ret = Polygon.SplitAlongLine(_a - v, _b - v).ToList();
            ret.ForEach(o => o.Move(v));
            return ret;
        }

        public override void PostUpdate()
        {
            FillRenderer?.Update(X, Y);
            OutlineRenderer?.Update(X, Y);
        }

        public override void Render()
        {
            if (FillVisible)
                FillRenderer?.Render();
            if (OutlineVisible)
                OutlineRenderer?.Render();
        }
    }
}
