using System;

namespace Harbor.Model
{
    public class RowingBoat : Boat
    {
        /// <summary>Create an instance of a rowing boat.</summary>
        /// <param name="weight">The weight of the boat, in kilograms. </param>
        /// <param name="topSpeed">The top speed of the boat, in knots.</param>
        /// <param name="passengers">The maximum number of passengers.</param>
        public RowingBoat(int weight, int topSpeed, int passengers) :
            base(ValidateInput(weight, WeightLimits), ValidateInput(topSpeed, SpeedLimits))
        {
            MaxPassengers = ValidateInput(passengers, CharacteristicLimits);
        }

        public RowingBoat(BoatData boatData) : base(boatData)
        {
            if (boatData.Prefix != CodePrefix) throw new InvalidCastException("The prefix doesn't match this type.");
            MaxPassengers = boatData.Characteristic;
        }

        /// <inheritdoc cref="Boat.CharacteristicLimits" />
        public new static (int min, int max) CharacteristicLimits => (1, 6);

        public static char CodePrefix { get; } = 'R';

        /// <inheritdoc cref="Boat.SpeedLimits" />
        public new static (int min, int max) SpeedLimits => (1, 3);

        /// <inheritdoc cref="Boat.WeightLimits" />
        public new static (int min, int max) WeightLimits => (100, 300);

        /// <inheritdoc />
        public override double BerthSpace { get; } = 0.5;

        /// <inheritdoc />
        public override int BerthTime { get; } = 1;

        /// <inheritdoc />
        public override string Characteristic { get; } = nameof(MaxPassengers);

        /// <inheritdoc />
        public override int CharacteristicValue => MaxPassengers;

        /// <summary>
        /// The maximum number of passengers this rowing boat can take.
        /// </summary>
        public int MaxPassengers { get; }

        /// <inheritdoc />
        protected override char Prefix => CodePrefix;
    }
}