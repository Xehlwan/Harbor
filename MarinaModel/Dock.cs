using System;
using System.Collections.Generic;
using System.Linq;

namespace Marina.Model
{
    public class Dock
    {
        /// <summary>
        /// An algorithm for finding a place to make berth.
        /// </summary>
        /// <param name="boat">The boat seeking berth</param>
        /// <param name="freeBerths">An array for all spots where occupied spots are marked with <see langword="false"/>.</param>
        /// <returns>The index for the best berth, or a negative value if none is found.</returns>
        public delegate int BerthingAlgorithm(Boat boat, IBerth[] berths);
        private readonly IBerth[] berths;
        private BerthingAlgorithm berthingAlgorithm;
        public int Size => berths.Length;

        public Dock(int size, BerthingAlgorithm algorithm)
        {
            berths = new IBerth[size];
            berthingAlgorithm = algorithm;
        }

        public void SetBerthingAlgorithm(BerthingAlgorithm algorithm) => berthingAlgorithm = algorithm;

        public bool TryAdd(Boat boat) => TryAdd(boat, berthingAlgorithm);

        public bool TryAdd(Boat boat, BerthingAlgorithm algorithm)
        {
            if (Boats.Contains(boat))
                throw new ArgumentException("A boat with the same id is already berthed.", nameof(boat));

            var index = algorithm(boat, berths);

            if (index < 0) return false;
            AddBoat(boat, index);

            return true;
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
                    for (int offset = 0; offset < boat.BerthSpace; offset++)
                    {
                        berths[index + offset] = berth;
                    }
                }
            }
            else
            {
                berths[index].AddBoat(boat);
            }
        }

        private static bool[] GetFreeBerths(double berthSpace, IBerth[] berths)
        {
            var freeSpots = new bool[berths.Length];
            if (berthSpace >= 1)
            {
                for (int i = 0; i < berths.Length; i++)
                {
                    freeSpots[i] = berths[i] is null;
                }
            }
            else
            {
                for (int i = 0; i < berths.Length; i++)
                {
                    var spot = berths[i];
                    if (spot is null || spot.FreeSpace >= berthSpace)
                    {
                        freeSpots[i] = true;
                    }
                }
            }

            return freeSpots;
        }

        public static BerthingAlgorithm DefaultBerthing => FirstFitAlgorithm;
        private static int FirstFitAlgorithm(Boat boat, IBerth[] berths)
        {
            if (boat.BerthSpace < 1)
            {
                for (int i = 0; i < berths.Length; i++)
                {
                    bool free = berths[i]?.FreeSpace > boat.BerthSpace;

                    if (free) return i;
                }
            }

            var freeBerths = GetFreeBerths(boat.BerthSpace, berths);

            for (int i = 0; i < berths.Length; i++)
            {
                if (freeBerths[i]) return i;
            }

            return -1;
        }

        public IEnumerable<IBerth> Berths =>
            berths.Where(berth => berth != null)
                      .GroupBy(berth => berth)
                      .Select(group => group.First());

        public IEnumerable<Boat> Boats => Berths.SelectMany(berth => berth.Occupancy.Select(item => item.boat));
    }
}