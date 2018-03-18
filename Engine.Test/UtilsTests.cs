using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using OpenTK;

namespace Engine.Test
{
    public class UtilsTests
    {
        [Theory]
        [InlineData(true, 0,0, 2,0,  1,0, 3,0)] //Parallel & colinear
        [InlineData(true, -1,-1, 1,1,  -1,1, 1,-1)] //X-cross
        [InlineData(true, -1,0, 1,0,  0,-1, 0,1)] //+-cross
        [InlineData(true, -129,247, -8,-55,  -504,12, 6,-9)] //Random cross
        [InlineData(true, 0,0, 1,0,  1,0, 1,1)] //Meet at corner
        [InlineData(true, -4,-5, 2,3,  -4,-5, 2,3)] //Same line
        [InlineData(false, 0,0, 1,0,  2,0, 2,2)] //Don't meet at corner
        [InlineData(false, -1,-1, -1,1,  1,-1, 1,1)] //Parallel vertical
        [InlineData(false, -1,1, 1,1,  -1,-1, 1,-1)] //Parallel horizontal
        public void SegmentsIntersect_ReturnSucceed(bool expected, float a1x, float a1y, float a2x, float a2y, float b1x, float b1y, float b2x, float b2y)
            => Assert.Equal(expected, Utils.SegmentsIntersect(a1x, a1y, a2x, a2y, b1x, b1y, b2x, b2y));

        [Theory]
        [InlineData(true, 0,0, 5,0, 0,5,  0,0, 5,0, 0,6)]
        [InlineData(true, 0,0, 0,5, 5,0,  0,0, 0,6, 5,0)]
        [InlineData(false, 0,0, 5,0, 0,5,  -10,0, -5,0, -1,6)]
        [InlineData(true, 0,0, 5,0, 2.5f,5,  0,4, 2.5f,-1, 5,4)]
        [InlineData(false, 0,0, 1,1, 0,2,  2,1, 3,0, 3,2)]
        [InlineData(false, 0,0, 1,1, 0,2,  2,1, 3,-2, 3,4)]
        [InlineData(false, 0,0, 1,0, 0,1,  1,0, 2,0, 1,1)] //On boundary
        public void TrianglesCollide_ReturnSucceed(bool expected,
            float a1x, float a1y, float a2x, float a2y, float a3x, float a3y, 
            float b1x, float b1y, float b2x, float b2y, float b3x, float b3y)
        { //https://stackoverflow.com/questions/1585459/whats-the-most-efficient-way-to-detect-triangle-triangle-intersections
            var a = new List<Vector3>
            {
                new Vector3(a1x, a1y, 0),
                new Vector3(a2x, a2y, 0),
                new Vector3(a3x, a3y, 0)
            };
            var b = new List<Vector3>
            {
                new Vector3(b1x, b1y, 0),
                new Vector3(b2x, b2y, 0),
                new Vector3(b3x, b3y, 0)
            };
            Assert.Equal(expected, Utils.TrianglesCollide(a, b));
        }

        [Fact]
        public void TrianglesCollide_ReturnFails_FirstArgumentTooMany()
        {
            var tooMany = new List<Vector3> { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
            var justRight = new List<Vector3> { Vector3.Zero, Vector3.Zero, Vector3.Zero };
            Assert.Throws<ArgumentException>(() => Utils.TrianglesCollide(tooMany, justRight));
        }
        [Fact]
        public void TrianglesCollide_ReturnFails_SecondArgumentTooMany()
        {
            var tooMany = new List<Vector3> { Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
            var justRight = new List<Vector3> { Vector3.Zero, Vector3.Zero, Vector3.Zero };
            Assert.Throws<ArgumentException>(() => Utils.TrianglesCollide(justRight, tooMany));
        }

        [Fact]
        public void TrianglesCollide_ReturnFails_FirstArgumentTooFew()
        {
            var tooFew = new List<Vector3> { Vector3.Zero, Vector3.Zero };
            var justRight = new List<Vector3> { Vector3.Zero, Vector3.Zero, Vector3.Zero };
            Assert.Throws<ArgumentException>(() => Utils.TrianglesCollide(tooFew, justRight));
        }
        [Fact]
        public void TrianglesCollide_ReturnFails_SecondArgumentTooFew()
        {
            var tooFew = new List<Vector3> { Vector3.Zero, Vector3.Zero };
            var justRight = new List<Vector3> { Vector3.Zero, Vector3.Zero, Vector3.Zero };
            Assert.Throws<ArgumentException>(() => Utils.TrianglesCollide(justRight, tooFew));
        }

        [Fact]
        public void TrianglesCollide_ReturnFails_FirstArgumentNull()
            => Assert.False(Utils.TrianglesCollide(null, new List<Vector3> { Vector3.Zero, Vector3.Zero, Vector3.Zero }));
        [Fact]
        public void TrianglesCollide_ReturnFails_SecondArgumentNull()
            => Assert.False(Utils.TrianglesCollide(new List<Vector3> { Vector3.Zero, Vector3.Zero, Vector3.Zero }, null));

        [Theory]
        [InlineData(2,0,0, Math.PI, 0,0,0, -2,0,0)]
        [InlineData(-2,0,0, Math.PI, 0,0,0, 2,0,0)]
        [InlineData(2,0,0, Math.PI/2, 0,0,0, 0,2,0)]
        [InlineData(0,2,0, Math.PI/2, 0,0,0, -2,0,0)]
        [InlineData(-2,0,0, Math.PI/2, 0,0,0, 0,-2,0)]
        [InlineData(0,-2,0, Math.PI/2, 0,0,0, 2,0,0)]
        [InlineData(2,0,0, -Math.PI/2, 0,0,0, 0,-2,0)]
        [InlineData(2,0,0, 20*Math.PI, 0,0,0, 2,0,0)]
        [InlineData(7,-2,0, 0, 5,-5,0, 7,-2,0)]
        [InlineData(7,-2,0, 2*Math.PI, 5,-5,0, 7,-2,0)]
        [InlineData(2,0,0, Math.PI/4, 0,0,0, 1.414,1.414,0)]
        [InlineData(2,0,0, Math.PI, 1,0,0, 0,0,0)]
        [InlineData(2,0,0, Math.PI/2, 1,0,0, 1,1,0)]
        [InlineData(2,3,0, Math.PI/2, 3,1,0, 1,0,0)]
        public void Rotate_ReturnSucceed(float a, float b, float c, float radians, float xcenter, float ycenter, float zcenter, float x, float y, float z)
        {
            var input = new Vector3(a, b, c);
            var center = new Vector3(xcenter, ycenter, zcenter);
            var result = input.Rotate(radians, center);
            Assert.Equal(Math.Round(x, 3), Math.Round(result.X, 3));
            Assert.Equal(Math.Round(y, 3), Math.Round(result.Y, 3));
            Assert.Equal(Math.Round(z, 3), Math.Round(result.Z, 3));
        }

        [Theory]
        [InlineData(0, 2, 0, 0, 2, 2, 1, 1)]
        [InlineData(2, 0, -2, 2, 2, -2, 1, -1)]
        [InlineData(-1, 0, -1, 0, 1, 0, -1, 0)]
        [InlineData(0, 0, -1, 0, 1, 0, 0, 0)]
        [InlineData(2, 2, -1, 3, 2, -3, 0, 1)]
        [InlineData(-2, 0, -1, 3, 2, -3, 0, 1)]
        public void PointOnLineNearestPoint_ReturnSucceed(float px, float py, float ax, float ay, float bx, float by, float rx, float ry)
        {
            var point = Utils.PointOnLineNearestPoint(px, py, ax, ay, bx, by);
            Assert.Equal(rx, point.X);
            Assert.Equal(ry, point.Y);
        }

        [Theory]
        [InlineData(1, 0, -1, 0, 1, 0, 1)]
        [InlineData(-1, 0, -1, 0, 1, 0, 0)]
        [InlineData(1, 3, -3, -5, 3, 7, 2f / 3)]
        [InlineData(6, 13, -3, -5, 3, 7, 1.5f)]
        [InlineData(-6, -11, -3, -5, 3, 7, -0.5f)]
        [InlineData(-12, -5, 6, -1, 6, -1, 0)]
        [InlineData(0, 0, 0, 0, 0, 0, 0)]
        public void PercentAlongLine_ReturnSucceed(float px, float py, float ax, float ay, float bx, float by, float v)
            => Assert.Equal(v, Utils.PercentAlongLine(px, py, ax, ay, bx, by));

        [Theory]
        [InlineData(1235, -12385, 443225, 3452)]
        [InlineData(-124905, 1231224, 823786, 919208)]
        public void PercentAlongLine_ReturnSucceed_Midpoint(float ax, float ay, float bx, float by)
        {
            var a = new Vector2(ax, ay);
            var b = new Vector2(bx, by);
            Assert.Equal(0.5f, Utils.PercentAlongLine(a.Midpoint(b), a, b));
        }
    }
}
