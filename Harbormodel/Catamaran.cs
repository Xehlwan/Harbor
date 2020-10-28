namespace Harbor.Model
{
    public class Catamaran : Boat
    {
        /// <summary>Create an instance of a catamaran.</summary>
        /// <param name="weight">The weight of the boat, in kilograms. </param>
        /// <param name="topSpeed">The top speed of the boat, in knots.</param>
        /// <param name="bedCount">The maximum number of beds.</param>
        public Catamaran(int weight, int topSpeed, int bedCount) : base(ValidateInput(weight, WeightLimits),
                                                                        ValidateInput(topSpeed, SpeedLimits))
        {
            BedCount = ValidateInput(bedCount, CharacteristicLimits);
        }

        /// <inheritdoc cref="Boat.CharacteristicLimits" />
        public new static (int min, int max) CharacteristicLimits => (1, 4);

        /// <inheritdoc cref="Boat.SpeedLimits" />
        public new static (int min, int max) SpeedLimits => (1, 12);

        /// <inheritdoc cref="Boat.WeightLimits" />
        public new static (int min, int max) WeightLimits => (1200, 8000);

        /// <summary>
        /// Number of beds on this catamaran.
        /// </summary>
        public int BedCount { get; }

        /// <inheritdoc />
        public override double BerthSpace => 3;

        /// <inheritdoc />
        public override int BerthTime => 3;

        /// <inheritdoc />
        public override string Characteristic => nameof(BedCount);

        /// <inheritdoc />
        public override int CharacteristicValue => BedCount;

        /// <inheritdoc />
        protected override char Prefix => 'K';
    }
}