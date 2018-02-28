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
        private AntWorld world;
        private Ant player;

        public AntSim() : base(1280, 720, "Ants") { }

        public override void Start()
        {
            GenerateGrids();
            //GenerateAnts(1);
            GenerateAntPlayer();
            GenerateResources(25);
        }

        private void GenerateGrids()
        {
            world = new AntWorld(Width, Height, 10);
        }
        
        private void GenerateAnt()
        {
            var ant = new Ant(world, WorldType.Underworld);
            ant.AttachController(new AIAntController());
            ant.Angle = (float)(Basics.Utils.RandomDouble() * Math.PI * 2);
            ant.AddToWorld();
        }

        private void GenerateAnts(int _count)
        {
            for (var i = 0; i < _count; i++)
                GenerateAnt();
        }

        private void GenerateAntPlayer()
        {
            player = new Ant(AntType.Digger, world, WorldType.Underworld, 0, 0, 25);
            player.AttachController(new KeyboardAntController());
            player.AddToWorld();
        }

        private void GenerateResource()
        {
            var randomX = Basics.Utils.RandomInt(-Width / 2, Width / 2);
            var randomY = Basics.Utils.RandomInt(-Height / 2, Height / 2);
            var resource = new Resource(randomX, randomY, Resource.RandomType(), 1);
            resource.AddToWorld();
        }

        private void GenerateResources(int _count)
        {
            for (var i = 0; i < _count; i++)
                GenerateResource();
        }

        public override void Update()
        {
            world.Update();
        }

        public override void Render()
        {
            world.OverworldVisible = player.Location == WorldType.Overworld;
            world.UnderworldVisible = player.Location == WorldType.Underworld;
            world.Render();
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