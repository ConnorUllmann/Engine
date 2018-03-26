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
        public readonly Polygon Polygon;

        public PolygonFillRenderer FillRenderer;
        public readonly PolygonOutlineRenderer OutlineRenderer;

        public bool FillVisible = true;
        public bool OutlineVisible = true;

        public PolygonActor(Polygon _polygon, float _x = 0, float _y = 0, bool _center = true, bool _filled=true, bool _outlined=true) 
            : base(_x, _y)
        {
            Polygon = _polygon;
            if (_center)
                center();
            if(_filled)
                FillRenderer = new PolygonFillRenderer(Polygon, X, Y, ColorExtensions.RandomColor());
            if(_outlined)
                OutlineRenderer = new PolygonOutlineRenderer(Polygon, X, Y, ColorExtensions.RandomColor());
        }

        public void UpdateBoundingBoxToMatchPolygon() => BoundingBox.MatchPositionAndDimensions(Polygon.BoundingRectangle());

        public Vector3 CenterOfMassRelative => Polygon.CenterOfMass;
        public Vector3 CenterOfMassAbsolute => Polygon.CenterOfMass + new Vector3(X, Y, 0);

        protected void center()
        {
            var com = CenterOfMassRelative;
            X += com.X;
            Y += com.Y;
            Polygon.MoveRelative(-com);
        }

        /// <summary>
        /// Rotates the PolygonActor around its current position (offset by the given offse, or Vector3.Zero if nothing)
        /// </summary>
        /// <param name="_angleRad">angle by which to rotate</param>
        /// <param name="_offset">position relative to the PolygonActor around which it should rotate</param>
        public void Rotate(float _angleRad, Vector3? _offset = null)
        {
            if (_angleRad % (2 * Math.PI) == 0)
                return;

            var realOffset = _offset ?? Vector3.Zero;
            Polygon.RotateRelative(_angleRad, realOffset);
            FillRenderer?.RotateRelative(_angleRad, realOffset);
            OutlineRenderer?.RotateRelative(_angleRad, realOffset);
        }

        public IEnumerable<Polygon> SplitAlongLine(Vector3 _a, Vector3 _b)
        {
            var v = new Vector3(X, Y, 0);
            var ret = Polygon.SplitAlongLine(_a - v, _b - v).ToList();
            ret.ForEach(o => o.MoveRelative(v));
            return ret;
        }
        
        public override void Render()
        {
            if (FillVisible)
            {
                FillRenderer?.MoveAbsolute(X, Y);
                FillRenderer?.Render();
            }
            if (OutlineVisible)
            {
                OutlineRenderer?.MoveAbsolute(X, Y);
                OutlineRenderer?.Render();
            }
        }
    }

    public class RocketPolygonActor : PolygonActor
    {
        private Rocket rocket;
        public float Angle
        {
            get => rocket.Angle;
            protected set => rocket.Angle = value;
        }
        public float Speed
        {
            get => rocket.Speed;
            protected set => rocket.Speed = value;
        }
        public float SpeedMax
        {
            get => rocket.SpeedMax;
            protected set => rocket.SpeedMax = value;
        }
        public float SpeedMin
        {
            get => rocket.SpeedMin;
            protected set => rocket.SpeedMin = value;
        }

        public RocketPolygonActor(Polygon _polygon, float _x = 0, float _y = 0, bool _center = true, bool _filled = true, bool _outlined = true)
            : base(_polygon, _x, _y, _center, _filled, _outlined)
        {
            rocket = new Rocket((_a) => Rotate(_a));
        }

        private void updatePosition() => Position += rocket.DeltaPosition();

        public override void Update()
        {
            base.Update();
            updatePosition();
        }
    }
}
