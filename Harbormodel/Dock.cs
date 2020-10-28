using System;
using System.Collections.Generic;
using System.Linq;

namespace Harbor.Model
{
    public class Dock
    {
        private readonly IBerth[] berths;
        private readonly List<Boat> leftToday = new List<Boat>();
        private BerthingAlgorithm berthingAlgorithm;

        public Dock(int size, BerthingAlgorithm algorithm)
        {
            berths = new IBerth[size];
            berthingAlgorithm = algorithm;
        }

        /// <summary>
        /// An algorithm for finding a place to make berth.
        /// </summary>
        /// <param name="boat">The boat seeking berth</param>
        /// <param name="freeBerths">An array for all spots where occupied spots are marked with <see langword="false" />.</param>
        /// <returns>The index for the best berth, or a negative value if none is found.</returns>
        public delegate int BerthingAlgorithm(Boat boat, IBerth[] berths);

        public static BerthingAlgorithm DefaultBerthing => FirstFitAlgorithm;

        public IEnumerable<IBerth> Berths =>
            berths.Where(berth => berth != null)
                  .GroupBy(berth => berth)
                  .Select(group => group.First());

        public int BoatCount => Boats.Count();

        public IEnumerable<Boat> Boats => Berths.SelectMany(berth => berth.Occupancy.Select(item => item.boat));

        public IEnumerable<Boat> LeftToday => leftToday.AsEnumerable();
        public int Size => berths.Length;

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

        public void SetBerthingAlgorithm(BerthingAlgorithm algorithm)
        {
            berthingAlgorithm = algorithm;
        }

        public bool TryAdd(Boat boat)
        {
            return TryAdd(boat, berthingAlgorithm);
        }

        public bool TryAdd(Boat boat, BerthingAlgorithm algorithm)
        {
            if (Boats.Contains(boat))
                throw new ArgumentException("A boat with the same id is already berthed.", nameof(boat));

            int index = algorithm(boat, berths);

            if (index < 0) return false;
            AddBoat(boat, index);

            return true;
        }

        public bool TryRemove(Boat boat)
        {
            for (var i = 0; i < berths.Length; i++)
            {
                if (berths[i] is null) continue;
                if (berths[i].Occupancy.Select(o => o.boat).Contains(boat))
                {
                    int size = berths[i].Size;
                    IBerth berth = berths[i].RemoveBoat(boat);
                    for (var j = 0; j < size; j++) berths[i + j] = berth;

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

            for (var i = 0; i < berths.Length; i++)
                if (freeBerths[i])
                    return i;

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

        private void AddBoat(Boat boat, int index)
        {
            if (berths[index] is null)
            {
                if (boat.BerthSpace < 1)
                {
                    berths[index] = new SharedBerth(boat);
                }
                else
                {
                    var berth = new Berth(boat);
                    for (var offset = 0; offset < boat.BerthSpace; offset++) berths[index + offset] = berth;
                }
            }
            else
            {
                berths[index].AddBoat(boat);
            }
        }
    }
}