using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using Basics;
using Engine;
using Ants.AntControllers;

namespace Ants
{
    public class AntSim : Game
    {
        private AntGrid antGrid;

        public AntSim() : base(1280, 720, "Ants") { }

        public override void Start()
        {
            var tileSize = 10;
            antGrid = new VisualAntGrid(-Width/2, -Height/2, Width, Height, tileSize);

            for (var i = 0; i < 100; i++)
            {
                var ant = new Ant(antGrid, 4);
                ant.AttachController(new AIAntController());
                ant.X = Basics.Utils.RandomInt(-Width/4, Width/4);
                ant.Y = Basics.Utils.RandomInt(-Height/4, Height/4);
                ant.Angle = (float)(Basics.Utils.RandomDouble() * Math.PI * 2);
                ant.AddToWorld();
            }

            var playerAnt = new Ant(antGrid, 8);
            playerAnt.AttachController(new KeyboardAntController());
            playerAnt.AddToWorld();
        }

        public override void Render()
        {
            antGrid.Render();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new AntSim().Run();
        }
    }
}