using System;
using System.Collections.Generic;
using System.Linq;

namespace Harbor.Model
{
    public class Port : IPort
    {
        private const int defaultSize = 64;
        private readonly DockChoiceAlgorithm algorithm;
        private readonly Dock[] docks;

        public Port(params int[] dockSizes)
        {
            algorithm = DockChoiceEmptiest;
            if (dockSizes.Length >= 1)
            {
                docks = new Dock[dockSizes.Length];

                for (var i = 0; i < docks.Length; i++)
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

        public delegate IOrderedEnumerable<Dock> DockChoiceAlgorithm(Boat boat, Dock[] docks);

        public static DockChoiceAlgorithm DockChoiceEmptiest => EmptiestChoice;

        /// <inheritdoc />
        public int BoatCount => Boats.Count();

        public IEnumerable<Boat> Boats => docks.SelectMany(dock => dock.Boats);

        /// <inheritdoc />
        public int DockCount => docks.Length;

        /// <inheritdoc />
        public IEnumerable<Dock> Docks => docks.AsEnumerable();

        /// <inheritdoc />
        public IEnumerable<Boat> LeftToday => docks.SelectMany(dock => dock.LeftToday);

        public int Size { get; }

        /// <inheritdoc />
        public void IncrementTime()
        {
            foreach (Dock dock in Docks) dock.IncrementTime();
        }

        /// <summary>
        /// Attempt to add a boat to the <see cref="Port" />.
        /// </summary>
        /// <param name="boat">The <see cref="Boat" /> to be added.</param>
        /// <returns><see langword="true" /> if the <see cref="Boat" /> was accepted, otherwise <see langword="false" /></returns>
        /// <exception cref="ArgumentException">Thrown if the <see cref="Boat" /> ID is already in the port.</exception>
        public bool TryAdd(Boat boat)
        {
            if (Boats.Contains(boat))
                throw new ArgumentException("The current boat id is already in port.", nameof(boat));

            var result = false;
            foreach (Dock dock in algorithm(boat, docks))
            {
                result = dock.TryAdd(boat);

                if (result) break;
            }

            return result;
        }

        /// <inheritdoc />
        public bool TryRemove(Boat boat)
        {
            foreach (Dock dock in docks)
                if (dock.Boats.Contains(boat))
                    return dock.TryRemove(boat);

            return false;
        }

        private static IOrderedEnumerable<Dock> EmptiestChoice(Boat boat, Dock[] docks)
        {
            return docks.OrderByDescending(dock => dock.Berths.Count(berth => berth is null));
        }
    }
}