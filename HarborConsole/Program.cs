using System.Linq;
using Harbor.Model;
using static System.Console;

namespace Harbor.ConsoleUI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var port = new Port(32, 32);

            Print.Value(port.Size, "Marina Size");
            Print.Value(port.Boats.Count());
            var boat = new RowingBoat(RowingBoat.WeightLimits.min, RowingBoat.SpeedLimits.min,
                                      RowingBoat.CharacteristicLimits.min);
            var result = port.TryAdd(boat);
            Print.Value(result, "Could add boat");
            Print.Value(port.Boats.Count());
            
            boat = new RowingBoat(RowingBoat.WeightLimits.min, RowingBoat.SpeedLimits.min,
                                  RowingBoat.CharacteristicLimits.min);
            result = port.TryAdd(boat);
            Print.Value(result, "Could add boat");
            Print.Value(port.Boats.Count());

            foreach (Boat b in port.Boats)
            {
                WriteLine($"{b.GetType().Name}[{b.IdentityCode}]: Weight {b.Weight}, Speed {b.TopSpeed}, {b.Characteristic} {b.CharacteristicValue}");
            }
            

        }
    }
}
