using System.IO;
using System.Linq;
using System.Text.Json;
using Harbor.Model;
using static System.Console;

namespace Harbor.ConsoleUI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Init
            BufferHeight = 300;
            IPort port = new Port(32, 32);
            port =  new PortLogger(port, "test.log", true);

            // Print port stats
            Print.Value(port.Size, "Marina Size");
            Print.Value(port.Boats.Count());

            // Add rowing boat
            Boat boat = new RowingBoat(RowingBoat.WeightLimits.min, RowingBoat.SpeedLimits.min,
                                      RowingBoat.CharacteristicLimits.min);
            var result = port.TryAdd(boat);
            Print.Value(result, "Could add boat");
            Print.Value(port.Boats.Count());

            // Add Rowing boat
            boat = new RowingBoat(RowingBoat.WeightLimits.min, RowingBoat.SpeedLimits.min,
                                  RowingBoat.CharacteristicLimits.min);
            result = port.TryAdd(boat);
            Print.Value(result, "Could add boat");
            Print.Value(port.Boats.Count());

            // Add Catamaran
            boat = new Catamaran(Catamaran.WeightLimits.min, Catamaran.SpeedLimits.min,
                                  Catamaran.CharacteristicLimits.min);
            result = port.TryAdd(boat);
            Print.Value(result, "Could add boat");
            Print.Value(port.Boats.Count());

            // Add Catamaran
            boat = new Catamaran(Catamaran.WeightLimits.min, Catamaran.SpeedLimits.min,
                                 Catamaran.CharacteristicLimits.min);
            result = port.TryAdd(boat);
            Print.Value(result, "Could add boat");
            Print.Value(port.Boats.Count());

            // Print boats
            foreach (Boat b in port.Boats)
            {
                WriteLine($"{b.GetType().Name}[{b.IdentityCode}]: Weight {b.Weight}, Speed {b.TopSpeed}, {b.Characteristic} {b.CharacteristicValue}");
            }

            // Serialize
            WriteLine("Serializing to JSON...");
            var json = ((Port)port.UnderlyingData).Serialize();

            // Remove and increment time.
            port.TryRemove(boat);
            foreach (Boat b in port.Boats)
            {
                WriteLine($"{b.GetType().Name}[{b.IdentityCode}]: Weight {b.Weight}, Speed {b.TopSpeed}, {b.Characteristic} {b.CharacteristicValue}");
            }
            port.IncrementTime();
            port.IncrementTime();

            // Print boats
            foreach (Boat b in port.Boats)
            {
                WriteLine($"{b.GetType().Name}[{b.IdentityCode}]: Weight {b.Weight}, Speed {b.TopSpeed}, {b.Characteristic} {b.CharacteristicValue}");
            }
            Print.Value(port.Boats.Count());

            // Deserializing
            WriteLine("Deserializing...");

            port = new PortLogger(Port.Deserialize(json), "test.log");

            // Print boats
            foreach (Boat b in port.Boats)
            {
                WriteLine($"{b.GetType().Name}[{b.IdentityCode}]: Weight {b.Weight}, Speed {b.TopSpeed}, {b.Characteristic} {b.CharacteristicValue}");
            }
            Print.Value(port.Boats.Count());
        }
    }
}
