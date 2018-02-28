using System;
using System.Collections.Generic;
using System.Text;
using Basics;
using Engine;
using Basics.QuadTree;
using OpenTK.Graphics;
using System.Linq;
using Ants.AntControllers;

namespace Ants
{
    public class Tile : ISolidTile
    {
        public AntGrid Grid;
        public int Size => Grid.TileSize;
        public bool Solid { get; set; } = false;

        public Dictionary<SignalType, List<Signal>> Signals = new Dictionary<SignalType, List<Signal>>();
        public bool HasSignal(SignalType _type) => Signals.ContainsKey(_type);

        //Indices within Grid's tiles
        public int I;
        public int J;

        //Position in real world space
        public float X { get => Grid.GetX(I); }
        public float Y { get => Grid.GetY(J); }

        public Tile(AntGrid _grid, int _i, int _j)
        {
            Grid = _grid;
            I = _i;
            J = _j;
        }

        public void Update()
        {
            UpdateSignals();
        }

        public void UpdateSignals()
        {
            var typesToRemove = new HashSet<SignalType>();
            foreach (var signalType in Signals.Keys)
            {
                var signals = Signals[signalType];
                var signalsToRemove = new HashSet<Signal>();
                foreach (var signal in signals)
                {
                    signal.Update();
                    if (signal.Amount <= 0)
                        signalsToRemove.Add(signal);
                }
                foreach (var signal in signalsToRemove)
                    signals.Remove(signal);
                if (signals.Count == 0)
                    typesToRemove.Add(signalType);
            }
            typesToRemove.ForEach(x => Signals.Remove(x));
        }

        public void AddSignal(SignalType _type, float _amount, float _angle)
        {
            if (!Signals.ContainsKey(_type))
                Signals[_type] = new List<Signal>();
            Signals[_type].Add(new Signal(_type, _amount, _angle));
        }

        public float? GetSignalAngleForResourceType(SignalType _type)
        {
            if (!Signals.TryGetValue(_type, out var signals) || signals.Count == 0)
                return null;
            var weightedAngleSum = signals.Select(o => o.Amount * o.Angle).Sum();
            var weightSum = signals.Select(o => o.Amount).Sum();
            return weightedAngleSum / weightSum; // Radians
        }
    }
}
