using System;
using System.Collections.Generic;
using System.Text;

namespace Ants
{
    public enum WorldType
    {
        Underworld,
        Overworld
    }

    public class AntWorld
    {
        public AntGrid Underworld;
        public AntGrid Overworld;

        public bool UnderworldVisible = true;
        public bool OverworldVisible = true;

        public AntWorld(int _width, int _height, int _tileSize)
        {
            Underworld = new AntGrid(-_width / 2, -_height / 2, _width, _height, _tileSize);
            Overworld = new AntGrid(-_width / 2, -_height / 2, _width, _height, _tileSize);
        }

        public bool AddAnt(Ant _ant) => WorldFromType(_ant.Location).AddAnt(_ant);
        public bool RemoveAnt(Ant _ant) => WorldFromType(_ant.Location).RemoveAnt(_ant);
        public void MoveAnt(Ant _ant, WorldType _destination)
        {
            if (_ant.Location == _destination)
                return;
            RemoveAnt(_ant);
            WorldFromType(_destination).AddAnt(_ant);
            _ant.Location = _destination;
        }

        public void Update()
        {
            Underworld.Update();
            Overworld.Update();
        }

        public void Render()
        {
            if (UnderworldVisible) Underworld.Render();
            if (OverworldVisible) Overworld.Render();
        }

        public AntGrid WorldFromType(WorldType _type) => _type == WorldType.Overworld ? Overworld : Underworld;
    }
}
