using System;
using System.Globalization;

namespace Marina.Model
{
    /// <summary>
    /// The abstract base-class for all types of boats seeking mooring.
    /// </summary>
    public abstract class Boat : IComparable<Boat>
    {
        private static readonly Random rng = new Random();

        /// <summary>
        /// Create an instance of a Boat.
        /// </summary>
        /// <param name="weight">The weight of the boat, in kilograms.</param>
        /// <param name="topSpeed">The top speed of the boat, in knots.</param>
        protected Boat(int weight, int topSpeed)
        {
            Weight = weight;
            TopSpeed = topSpeed;
            RegenerateCode();
        }

        /// <summary>
        /// The limits to the unique characteristics for this boat-type. Defaults to unlimited positive values.
        /// </summary>
        public static (int min, int max) CharacteristicLimits => (0, int.MaxValue);

        /// <summary>
        /// The limits to top speed for this boat-type. Defaults to unlimited positive values.
        /// </summary>
        public static (int min, int max) SpeedLimits => (0, int.MaxValue);

        /// <summary>
        /// The limits to weight for this boat-type. Defaults to unlimited positive values.
        /// </summary>
        public static (int min, int max) WeightLimits => (0, int.MaxValue);

        /// <summary>
        /// The number of berth spots this boat occupies.
        /// </summary>
        public abstract double BerthSpace { get; }

        /// <summary>
        /// The number of days this boat stays berthed, before moving on.
        /// </summary>
        public abstract int BerthTime { get; }

        /// <summary>
        /// The name of this boat-type's logged characteristic.
        /// </summary>
        public abstract string Characteristic { get; }

        /// <summary>
        /// The value of this boat-types logged characteristic.
        /// </summary>
        public abstract int CharacteristicValue { get; }

        /// <summary>
        /// The unique identity of this boat.
        /// </summary>
        public string IdentityCode => $"{Prefix}-{Code}";

        /// <summary>
        /// The maximum speed, in <b>knots</b>.
        /// </summary>
        public int TopSpeed { get; }

        /// <summary>
        /// The weight of the boat, in <b>kilograms</b>.
        /// </summary>
        public int Weight { get; }

        /// <summary>
        /// The unique prefix for this boat-type.
        /// </summary>
        protected abstract char Prefix { get; }

        private string Code { get; set; }

        /// <summary>
        /// Regenerate the unique 3-letter identifier for this boat.
        /// </summary>
        public void RegenerateCode()
        {
            const int lower = 'A';
            const int upper = 'Z' + 1;
            Code = string.Create(3, rng, (chars, rand) =>
            {
                for (var i = 0; i < chars.Length; i++) chars[i] = (char) rand.Next(lower, upper);
            });
        }

        protected static int ValidateInput(int arg, (int min, int max) limits)
        {
            (int min, int max) = limits;

            if (arg < min || arg > max)
                throw new ArgumentException($"Input was {arg}, but limits are from {min} to {max}.");

            return arg;
        }

        /// <inheritdoc />
        public int CompareTo(Boat other) =>
            string.Compare(IdentityCode, other.IdentityCode, StringComparison.OrdinalIgnoreCase);
    }
}