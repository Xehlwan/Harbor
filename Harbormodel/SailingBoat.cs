namespace Harbor.Model
{
    public class SailingBoat : Boat
    {
        /// <summary>Create an instance of a sailing boat.</summary>
        /// <param name="weight">The weight of the boat, in kilograms. </param>
        /// <param name="topSpeed">The top speed of the boat, in knots.</param>
        /// <param name="length">The length of the sailing boat.</param>
        public SailingBoat(int weight, int topSpeed, int length) :
            base(ValidateInput(weight, WeightLimits), ValidateInput(topSpeed, SpeedLimits))
        {
            Length = ValidateInput(length, CharacteristicLimits);
        }

        /// <inheritdoc cref="Boat.CharacteristicLimits" />
        public new static (int min, int max) CharacteristicLimits => (10, 60);

        /// <inheritdoc cref="Boat.SpeedLimits" />
        public new static (int min, int max) SpeedLimits => (1, 12);

        /// <inheritdoc cref="Boat.WeightLimits" />
        public new static (int min, int max) WeightLimits => (800, 6000);

        /// <inheritdoc />
        public override double BerthSpace { get; } = 2;

        /// <inheritdoc />
        public override int BerthTime { get; } = 4;

        /// <inheritdoc />
        public override string Characteristic { get; } = nameof(Length);

        /// <inheritdoc />
        public override int CharacteristicValue => Length;

        public int Length { get; }

        /// <inheritdoc />
        protected override char Prefix { get; } = 'S';
    }
}