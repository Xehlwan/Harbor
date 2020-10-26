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
        public delegate int BerthingAlgorithm(Boat boat, bool[] freeBerths);
        private readonly IBerth[] berthSpots;
        private BerthingAlgorithm berthingAlgorithm;
        public int Size => berthSpots.Length;

        public Dock(int size, BerthingAlgorithm algorithm)
        {
            berthSpots = new IBerth[size];
            berthingAlgorithm = algorithm;
        }

        public void SetBerthingAlgorithm(BerthingAlgorithm algorithm) => berthingAlgorithm = algorithm;


        public bool Berth(Boat boat) => Berth(boat, berthingAlgorithm);

        public bool Berth(Boat boat, BerthingAlgorithm algorithm)
        {
            var index = algorithm(boat, GetFreeSpots(boat.BerthSpace));

            if (index < 0) return false;
            Add(boat, index);

            return true;
        }

        private void Add(Boat boat, int index)
        {
            if (berthSpots[index] is null)
            {
                if (boat.BerthSpace < 1)
                {
                    berthSpots[index] = new SharedBerth(boat);
                }
                else
                {
                    var berth = new Berth(boat);
                    for (int offset = 0; offset < boat.BerthSpace; offset++)
                    {
                        berthSpots[index + offset] = berth;
                    }
                }
            }
            else
            {
                berthSpots[index].AddBoat(boat);
            }
        }

        private bool[] GetFreeSpots(double berthSpace)
        {
            var freeSpots = new bool[berthSpots.Length];
            if (berthSpace >= 1)
            {
                for (int i = 0; i < berthSpots.Length; i++)
                {
                    freeSpots[i] = berthSpots[i] is null;
                }
            }
            else
            {
                for (int i = 0; i < berthSpots.Length; i++)
                {
                    var spot = berthSpots[i];
                    if (spot is null || spot.FreeSpace >= berthSpace)
                    {
                        freeSpots[i] = true;
                    }
                }
            }

            return freeSpots;
        }
    }
}