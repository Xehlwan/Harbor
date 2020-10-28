namespace Harbor.Model
{
    public class CargoShip : Boat
    {
        /// <summary>Create an instance of a cargo ship.</summary>
        /// <param name="weight">The weight of the boat, in kilograms. </param>
        /// <param name="topSpeed">The top speed of the boat, in knots.</param>
        /// <param name="cargo">The current number of containers on the ship.</param>
        public CargoShip(int weight, int topSpeed, int cargo) :
            base(ValidateInput(weight, WeightLimits), ValidateInput(topSpeed, SpeedLimits))
        {
            Cargo = ValidateInput(cargo, CharacteristicLimits);
        }

        /// <inheritdoc cref="Boat.CharacteristicLimits" />
        public new static (int min, int max) CharacteristicLimits => (0, 500);

        /// <inheritdoc cref="Boat.SpeedLimits" />
        public new static (int min, int max) SpeedLimits => (1, 20);

        /// <inheritdoc cref="Boat.WeightLimits" />
        public new static (int min, int max) WeightLimits => (3000, 20000);

        /// <inheritdoc />
        public override double BerthSpace { get; } = 4;

        /// <inheritdoc />
        public override int BerthTime { get; } = 6;

        /// <summary>
        /// The number of cargo containers on this ship.
        /// </summary>
        public int Cargo { get; set; }

        /// <inheritdoc />
        public override string Characteristic { get; } = nameof(Cargo);

        /// <inheritdoc />
        public override int CharacteristicValue => Cargo;

        /// <inheritdoc />
        protected override char Prefix { get; } = 'L';
    }
}