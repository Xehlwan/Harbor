using System;

namespace Harbor.Model
{
    public class MotorBoat : Boat
    {
        /// <summary>Create an instance of a motor boat.</summary>
        /// <param name="weight">The weight of the boat, in kilograms. </param>
        /// <param name="topSpeed">The top speed of the boat, in knots.</param>
        /// <param name="horsePower">The horse-power of the engine..</param>
        public MotorBoat(int weight, int topSpeed, int horsePower) :
            base(ValidateInput(weight, WeightLimits), ValidateInput(topSpeed, SpeedLimits))
        {
            HorsePower = ValidateInput(horsePower, CharacteristicLimits);
        }

        public MotorBoat(BoatData boatData) : base(boatData)
        {
            if (boatData.Prefix != CodePrefix) throw new InvalidCastException("The prefix doesn't match this type.");
            HorsePower = boatData.Characteristic;
        }

        /// <inheritdoc cref="Boat.CharacteristicLimits" />
        public new static (int min, int max) CharacteristicLimits => (10, 1000);

        public static char CodePrefix { get; } = 'M';

        /// <inheritdoc cref="Boat.SpeedLimits" />
        public new static (int min, int max) SpeedLimits => (1, 60);

        /// <inheritdoc cref="Boat.WeightLimits" />
        public new static (int min, int max) WeightLimits => (200, 3000);

        /// <inheritdoc />
        public override double BerthSpace { get; } = 1;

        /// <inheritdoc />
        public override int BerthTime { get; } = 3;

        /// <inheritdoc />
        public override string Characteristic { get; } = nameof(HorsePower);

        /// <inheritdoc />
        public override int CharacteristicValue => HorsePower;

        public int HorsePower { get; }

        /// <inheritdoc />
        protected override char Prefix => CodePrefix;
    }
}