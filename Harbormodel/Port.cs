using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Harbor.Model
{
    public class Port : IPort
    {
        private const int defaultSize = 64;
        private readonly Dock[] docks;
        private DockChoiceAlgorithm algorithm;
        private string algorithmName;

        public Port(params int[] dockSizes)
        {
            Time = DateTime.Now;
            SetAlgorithm(DockChoiceEmptiest, nameof(DockChoiceEmptiest));

            if (dockSizes.Length >= 1)
            {
                docks = new Dock[dockSizes.Length];

                for (var i = 0; i < docks.Length; i++)
                {
                    int size = dockSizes[i];
                    docks[i] = new Dock(size);
                    Size += size;
                }
            }
            else
            {
                docks = new Dock[1];
                docks[0] = new Dock(defaultSize);
                Size = defaultSize;
            }
        }

        private Port(PortData data, Dictionary<string, DockChoiceAlgorithm> dockChoices,
                     Dictionary<string, Dock.BerthingAlgorithm> berthingChoices)
        {
            Time = data.Time;
            if (dockChoices.TryGetValue(data.DockChoiceAlgorithm, out DockChoiceAlgorithm dockChoice))
                SetAlgorithm(dockChoice, data.DockChoiceAlgorithm);
            else
                SetAlgorithm(DockChoiceDefault, nameof(DockChoiceDefault));

            docks = new Dock[data.Docks.Length];

            for (var i = 0; i < docks.Length; i++) docks[i] = Dock.FromData(data.Docks[i], berthingChoices);
        }

        public delegate IOrderedEnumerable<Dock> DockChoiceAlgorithm(Boat boat, Dock[] docks);

        public static DockChoiceAlgorithm DockChoiceDefault => EmptiestChoice;

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

        /// <inheritdoc />
        public int Size { get; }

        /// <inheritdoc />
        public DateTime Time { get; private set; }

        /// <inheritdoc />
        public IPort UnderlyingData => throw new NotSupportedException("There is no underlying data for this type.");

        public static Port Deserialize(string portJsonString)
        {
            var dockChoices = new Dictionary<string, DockChoiceAlgorithm>();
            var berthingChoices = new Dictionary<string, Dock.BerthingAlgorithm>();

            return Deserialize(portJsonString, dockChoices, berthingChoices);
        }

        public static Port Deserialize(string portJsonString, Dictionary<string, DockChoiceAlgorithm> dockChoices,
                                       Dictionary<string, Dock.BerthingAlgorithm> berthingChoices)
        {
            var data = JsonSerializer.Deserialize<PortData>(portJsonString);

            return new Port(data, dockChoices, berthingChoices);
        }

        /// <inheritdoc />
        public void IncrementTime()
        {
            foreach (Dock dock in Docks) dock.IncrementTime();
            Time = Time.AddDays(1);
        }

        public string Serialize()
        {
            var data = new PortData();
            data.Time = Time;
            data.DockChoiceAlgorithm = algorithmName;
            data.Docks = new DockData[docks.Length];
            for (var i = 0; i < docks.Length; i++) data.Docks[i] = docks[i].AsData();

            return JsonSerializer.Serialize(data, new JsonSerializerOptions {WriteIndented = true});
        }

        public void SetAlgorithm(DockChoiceAlgorithm algorithm, string name)
        {
            algorithmName = name;
            this.algorithm = algorithm;
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