using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;
using OpenTK.Graphics;

namespace Ants
{
    public enum ResourceType
    {
        Food,
        Dirt
    }

    public class Resource : Actor
    {
        public readonly static int TypesCount = Enum.GetNames(typeof(ResourceType)).Length;
        public static ResourceType RandomType() => (ResourceType)Basics.Utils.RandomInt(0, TypesCount);

        public static Dictionary<ResourceType, Color4> Colors = new Dictionary<ResourceType, Color4>
        {
            [ResourceType.Food] = Color4.Green,
            [ResourceType.Dirt] = Color4.Brown
        };

        public ResourceAccount Account;

        private PolygonRenderer renderer;
        
        public Resource(float _x, float _y, ResourceType _type, float _amount=0) : base(_x, _y)
        {
            Account = new ResourceAccount(_type);
            Account.Deposit(_amount);
            renderer = new PolygonFillRenderer(ConvexPolygon.Square(8), Colors[_type]);
        }

        public override void Update()
        {
            if (Account.IsEmpty)
                Destroy();
        }

        public override void Render() => renderer?.Render(X, Y);
    }
}
