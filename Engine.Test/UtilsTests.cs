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
            var result = Utils.Rotate(input, radians, center);
            Assert.Equal(Math.Round(x, 3), Math.Round(result.X, 3));
            Assert.Equal(Math.Round(y, 3), Math.Round(result.Y, 3));
            Assert.Equal(Math.Round(z, 3), Math.Round(result.Z, 3));
        }
    }
}
