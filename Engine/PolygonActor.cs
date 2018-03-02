using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using System.Linq;

namespace Engine
{
    public class PolygonActor : Actor
    {
        public Polygon Polygon;

        public PolygonFillRenderer FillRenderer;
        public PolygonOutlineRenderer OutlineRenderer;

        public bool FillVisible = true;
        public bool OutlineVisible = true;

        public PolygonActor(Polygon _polygon, float _x = 0, float _y = 0, bool _center = true) : base(_x, _y)
        {
            Polygon = _polygon;
            if (_center) Center();
            FillRenderer = new PolygonFillRenderer(Polygon, X, Y, ColorExtensions.RandomColor());
            OutlineRenderer = new PolygonOutlineRenderer(Polygon, X, Y, ColorExtensions.RandomColor());
        }

        private void UpdateBounds() => BoundingBox.MatchPositionAndDimensions(Polygon.BoundingRectangle());

        private void Center()
        {
            var com = Polygon.CenterOfMass;
            X += com.X;
            Y += com.Y;
            Polygon.Move(-com);
            UpdateBounds();
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
            FillRenderer.Rotate(_angleRad, centerAbsolute);
            OutlineRenderer.Rotate(_angleRad, centerAbsolute);
            UpdateBounds();
        }

        public IEnumerable<Polygon> SplitAlongLine(Vector3 pointA, Vector3 pointB)
        {
            var v = new Vector3(X, Y, 0);
            var ret = Polygon.SplitAlongLine(pointA - v, pointB - v).ToList();
            ret.ForEach(o => o.Move(v));
            return ret;
        }

        public override void Render()
        {
            if (FillVisible)
                FillRenderer.Render(X, Y);
            if (OutlineVisible)
                OutlineRenderer.Render(X, Y);
        }
    }
}
