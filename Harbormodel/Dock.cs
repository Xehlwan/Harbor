using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Harbor.Model
{
    public class Dock
    {
        private readonly IBerth[] berthSpots;
        private readonly List<Boat> leftToday = new List<Boat>();
        private string algorithmName;
        private BerthingAlgorithm berthingAlgorithm;

        public Dock(int size)
        {
            berthSpots = new IBerth[size];
            SetBerthingAlgorithm(DefaultBerthing, nameof(DefaultBerthing));
        }

        private Dock(DockData data, Dictionary<string, BerthingAlgorithm> berthingChoices)
        {
            berthSpots = new IBerth[data.Size];
            for (var i = 0; i < data.Boats.Length; i++) AddBoat(data.Boats[i], data.Indices[i], data.BerthTimes[i]);
            foreach (BoatData boatData in data.LeftToday) leftToday.Add(Boat.FromData(boatData));

            if (berthingChoices.TryGetValue(data.BerthingChoiceAlgorithm, out BerthingAlgorithm berthingChoice))
                SetBerthingAlgorithm(berthingChoice, data.BerthingChoiceAlgorithm);
            else
                SetBerthingAlgorithm(DefaultBerthing, nameof(DefaultBerthing));
        }

        /// <summary>
        /// An algorithm for finding a place to make berth.
        /// </summary>
        /// <param name="boat">The boat seeking berth</param>
        /// <param name="berths">An array for all spots where occupied spots are marked with <see langword="false" />.</param>
        /// <returns>The index for the best berth, or a negative value if none is found.</returns>
        public delegate int BerthingAlgorithm(Boat boat, IBerth[] berths);

        public static BerthingAlgorithm DefaultBerthing => FirstFitAlgorithm;

        public IEnumerable<IBerth> Berths =>
            berthSpots.Where(berth => berth != null)
                      .GroupBy(berth => berth)
                      .Select(group => group.First());

        public ImmutableArray<IBerth> BerthSpots => berthSpots.ToImmutableArray();

        public int BoatCount => Boats.Count();

        public IEnumerable<Boat> Boats => Berths.SelectMany(berth => berth.Occupancy.Select(item => item.boat));

        public IEnumerable<Boat> LeftToday => leftToday.AsEnumerable();
        public int Size => berthSpots.Length;

        public static Dock FromData(DockData dockData, Dictionary<string, BerthingAlgorithm> berthingChoices)
        {
            return new Dock(dockData, berthingChoices);
        }

        public DockData AsData()
        {
            int count = Boats.Count();
            var data = new DockData
            {
                Size = Size,
                BerthingChoiceAlgorithm = algorithmName,
                Boats = new BoatData[count],
                Indices = new int[count],
                BerthTimes = new int[count],
                LeftToday = leftToday.Select(b => b.AsData()).ToArray()
            };

            IBerth prev = null;
            for (int source = 0, target = 0; source < berthSpots.Length; source++)
            {
                IBerth spot = berthSpots[source];

                if (spot is null || spot == prev) continue;
                foreach ((Boat boat, int berthTime) in spot.Occupancy)
                {
                    data.Indices[target] = source;
                    data.Boats[target] = boat.AsData();
                    data.BerthTimes[target] = berthTime;
                    target++;
                }

                prev = spot;
            }

            return data;
        }

        public void IncrementTime()
        {
            leftToday.Clear();
            foreach (IBerth berth in Berths)
            {
                berth.IncrementTime();
                foreach ((Boat boat, int berthTime) in berth.Occupancy)
                    if (berthTime >= boat.BerthTime)
                        leftToday.Add(boat);
            }

            foreach (Boat boat in leftToday) TryRemove(boat);
        }

        public void SetBerthingAlgorithm(BerthingAlgorithm algorithm, string algorithmName)
        {
            berthingAlgorithm = algorithm;
            this.algorithmName = algorithmName;
        }

        public bool TryAdd(Boat boat)
        {
            return TryAdd(boat, berthingAlgorithm);
        }

        public bool TryAdd(Boat boat, BerthingAlgorithm algorithm)
        {
            if (Boats.Contains(boat))
                throw new ArgumentException("A boat with the same id is already berthed.", nameof(boat));

            int index = algorithm(boat, berthSpots);

            if (index < 0) return false;
            AddBoat(boat, index);

            return true;
        }

        public bool TryRemove(Boat boat)
        {
            for (var i = 0; i < berthSpots.Length; i++)
            {
                if (berthSpots[i] is null) continue;
                if (berthSpots[i].Occupancy.Select(o => o.boat).Contains(boat))
                {
                    int size = berthSpots[i].Size;
                    IBerth berth = berthSpots[i].RemoveBoat(boat);
                    for (var j = 0; j < size; j++) berthSpots[i + j] = berth;

                    return true;
                }
            }

            return false;
        }

        private static int FirstFitAlgorithm(Boat boat, IBerth[] berths)
        {
            if (boat.BerthSpace < 1)
                for (var i = 0; i < berths.Length; i++)
                {
                    bool free = berths[i]?.FreeSpace > boat.BerthSpace;

                    if (free) return i;
                }

            bool[] freeBerths = GetFreeBerths(boat.BerthSpace, berths);

            for (var spot = 0; spot + boat.BerthSpace <= berths.Length; spot++)
            {
                var free = true;
                for (var offset = 0; offset < boat.BerthSpace; offset++)
                {
                    free = freeBerths[spot + offset];

                    if (!free) break;
                }

                if (free) return spot;
            }

            return -1;
        }

        private static bool[] GetFreeBerths(double berthSpace, IBerth[] berths)
        {
            var freeSpots = new bool[berths.Length];
            if (berthSpace >= 1)
                for (var i = 0; i < berths.Length; i++)
                    freeSpots[i] = berths[i] is null;
            else
                for (var i = 0; i < berths.Length; i++)
                {
                    IBerth spot = berths[i];
                    if (spot is null || spot.FreeSpace >= berthSpace) freeSpots[i] = true;
                }

            return freeSpots;
        }

        private void AddBoat(BoatData boatData, int index, int berthedFor = 0)
        {
            var boat = Boat.FromData(boatData);
            AddBoat(boat, index, berthedFor);
        }

        private void AddBoat(Boat boat, int index, int berthedFor = 0)
        {
            if (berthSpots[index] is null)
            {
                if (boat.BerthSpace < 1)
                {
                    berthSpots[index] = new SharedBerth(boat);
                }
                else
                {
                    var berth = new Berth(boat, berthedFor);
                    for (var offset = 0; offset < boat.BerthSpace; offset++) berthSpots[index + offset] = berth;
                }
            }
            else
            {
                berthSpots[index].AddBoat(boat, berthedFor);
            }
        }
    }
}