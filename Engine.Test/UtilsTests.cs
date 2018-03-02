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
        [InlineData(true, 0,0, 5,0, 0,5,  0,0, 5,0, 0,6)]
        [InlineData(true, 0,0, 0,5, 5,0,  0,0, 0,6, 5,0)]
        [InlineData(false, 0,0, 5,0, 0,5,  -10,0, -5,0, -1,6)]
        [InlineData(true, 0,0, 5,0, 2.5f,5,  0,4, 2.5f,-1, 5,4)]
        [InlineData(false, 0,0, 1,1, 0,2,  2,1, 3,0, 3,2)]
        [InlineData(false, 0,0, 1,1, 0,2,  2,1, 3,-2, 3,4)]
        [InlineData(true, 0,0, 1,0, 0,1,  1,0, 2,0, 1,1)] //On boundary
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
    }
}
