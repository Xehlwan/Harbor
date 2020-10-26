using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marina.Model
{
    public class Port
    {
        private const int defaultSize = 64;
        private readonly Dock[] docks;
        private DockChoiceAlgorithm algorithm;
        public int Size { get; }
        public IEnumerable<Boat> AllBoats => docks.SelectMany(dock => dock.Boats);
        public Port(params int[] dockSizes)
        {
            algorithm = DockChoiceEmptiest;
            if (dockSizes.Length >= 1)
            {
                docks = new Dock[dockSizes.Length];

                for (int i = 0; i < docks.Length; i++)
                {
                    int size = dockSizes[i];
                    docks[i] = new Dock(size, Dock.DefaultBerthing);
                    Size += size;
                }
            }
            else
            {
                docks = new Dock[1];
                docks[0] = new Dock(defaultSize, Dock.DefaultBerthing);
                Size = defaultSize;
            }
        }

        public IEnumerable<Boat> Boats => docks.SelectMany(dock => dock.Boats);

        /// <summary>
        /// Attempt to add a boat to the <see cref="Port"/>.
        /// </summary>
        /// <param name="boat">The <see cref="Boat"/> to be added.</param>
        /// <returns><see langword="true"/> if the <see cref="Boat"/> was accepted, otherwise <see langword="false"/></returns>
        /// <exception cref="ArgumentException">Thrown if the <see cref="Boat"/> ID is already in the port.</exception>
        public bool TryAdd(Boat boat)
        {
            if (AllBoats.Contains(boat))
                throw new ArgumentException("The current boat id is already in port.", nameof(boat));

            var result = false;
            foreach (Dock dock in algorithm(boat, docks))
            {
                result = dock.TryAdd(boat);

                if (result) break;
            }

            return result;
        }

        public delegate IOrderedEnumerable<Dock> DockChoiceAlgorithm(Boat boat, Dock[] docks);

        public static DockChoiceAlgorithm  DockChoiceEmptiest => EmptiestChoice;

        private static IOrderedEnumerable<Dock> EmptiestChoice(Boat boat, Dock[] docks) =>
            docks.OrderByDescending(dock => dock.Berths.Count(berth => berth is null));
    }
}
