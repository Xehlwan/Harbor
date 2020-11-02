using System;
using System.Collections.Generic;
using System.Reflection;

namespace Harbor.Model
{
    public static class HarborHelper
    {
        private static readonly List<Type> boatTypes;
        private static readonly Random rng = new Random();

        static HarborHelper()
        {
            boatTypes = new List<Type>();

            boatTypes.Add(typeof(RowingBoat));
            boatTypes.Add(typeof(MotorBoat));
            boatTypes.Add(typeof(SailingBoat));
            boatTypes.Add(typeof(CargoShip));
            boatTypes.Add(typeof(Catamaran));
        }

        public static IReadOnlyList<Type> RegisteredBoatTypes => boatTypes.AsReadOnly();

        public static Boat GetRandomBoat(Type type = null)
        {
            type ??= boatTypes[rng.Next(boatTypes.Count)];
            var weight =
                ((int min, int max)) (type.GetProperty(nameof(Boat.WeightLimits),
                                                       BindingFlags.Public | BindingFlags.Static)
                                          ?.GetValue(null) ??
                                      (0, 0));

            var speed =
                ((int min, int max)) (type.GetProperty(nameof(Boat.SpeedLimits),
                                                       BindingFlags.Public | BindingFlags.Static)
                                          ?.GetValue(null) ??
                                      (0, 0));

            var characteristic =
                ((int min, int max)) (type.GetProperty(nameof(Boat.CharacteristicLimits),
                                                       BindingFlags.Public | BindingFlags.Static)
                                          ?.GetValue(null) ??
                                      (0, 0));

            return (Boat) Activator.CreateInstance(type,
                                                   rng.Next(weight.min, weight.max + 1),
                                                   rng.Next(speed.min, speed.max + 1),
                                                   rng.Next(characteristic.min, characteristic.max + 1));
        }
    }
}