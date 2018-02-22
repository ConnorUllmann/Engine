using System;
using System.Collections.Generic;
using System.Text;
using Engine;

namespace Ants
{
    public enum SignalType
    {
        Food,
        Dirt,
        Home,
        Enemy
    }

    public class Signal
    {
        private static readonly Dictionary<ResourceType, SignalType> signalPerResource = new Dictionary<ResourceType, SignalType>
        {
            [ResourceType.Food] = SignalType.Food,
            [ResourceType.Dirt] = SignalType.Dirt
        };
        //The kind of signal that corresponds to the given type of resource we want to find
        public static SignalType ForResource(ResourceType _type) => signalPerResource[_type];

        private const float DecayRate = 6;

        public SignalType Type;
        private Account account;
        public float Amount => account.Amount;
        public float Angle; //Radians

        public Signal(SignalType _type, float _amount, float _angle)
        {
            Type = _type;
            account = new Account();
            account.Deposit(_amount);
            Angle = _angle;
        }

        public void Update()
        {
            account.Withdraw(DecayRate * Game.Delta);
        }
    }
}
