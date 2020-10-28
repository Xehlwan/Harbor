using System.Linq;
using Harbor.Model;
using static System.Console;

namespace Harbor.ConsoleUI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var marina = new Model.Port(32, 32);

            Print.Value(marina.Size, "Marina Size");
            Print.Value(marina.Boats.Count());
            var boat = new RowingBoat(RowingBoat.WeightLimits.min, RowingBoat.SpeedLimits.min,
                                      RowingBoat.CharacteristicLimits.min);
            var result = marina.TryAdd(boat);
            Print.Value(result, "Could add boat");
            Print.Value(marina.Boats.Count());
            
            boat = new RowingBoat(RowingBoat.WeightLimits.min, RowingBoat.SpeedLimits.min,
                                  RowingBoat.CharacteristicLimits.min);
            result = marina.TryAdd(boat);
            Print.Value(result, "Could add boat");
            Print.Value(marina.Boats.Count());

            foreach (Boat b in marina.Boats)
            {
                WriteLine($"{b.GetType().Name}[{b.IdentityCode}]: Weight {b.Weight}, Speed {b.TopSpeed}, {b.Characteristic} {b.CharacteristicValue}");
            }
            

        }
    }
}
