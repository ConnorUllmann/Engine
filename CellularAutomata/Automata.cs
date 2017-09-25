using System;
using System.Collections.Generic;
using System.Text;
using Engine;

namespace CellularAutomata
{
    class Automata : Game
    {
        private CustomCellGrid cellGrid;

        public Automata(int width, int height) : base(width, height, "Cellular Automata") { }

        public override void Start()
        {
            var width = 100;
            int height = 100;
            var size = 8;
            cellGrid = new CustomCellGrid(-width/2*size, -height/2*size, width, height, size);
            cellGrid.Start();
        }

        public override void Update()
        {
            cellGrid.Update();
        }
    }
}
