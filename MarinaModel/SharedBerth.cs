using System;
using System.Collections.Generic;
using System.Linq;

namespace Marina.Model
{
    internal class SharedBerth : IBerth
    {
        private readonly List<(Boat boat, int berthedFor)> spots = new List<(Boat boat, int berthedFor)>();

        public SharedBerth(Boat boat)
        {
            spots.Add((boat, 0));
        }

        /// <inheritdoc />
        public double FreeSpace { get; private set; }

        /// <inheritdoc />
        public int Size => 1;

        /// <inheritdoc />
        public IBerth AddBoat(Boat boat)
        {
            if (FreeSpace < boat.BerthSpace) throw new InvalidOperationException("Not enough space to add boat.");
            spots.Add((boat, 0));

            return this;
        }

        /// <inheritdoc />
        public IEnumerable<(Boat boat, int berthTime)> Occupancy => spots.AsEnumerable();

        /// <inheritdoc />
        public void IncrementTime()
        {
            for (var i = 0; i < spots.Count; i++)
            {
                (Boat boat, int berthedFor) spot = spots[i];
                spots[i] = (spot.boat, spot.berthedFor + 1);
            }
        }

        /// <inheritdoc />
        public IBerth RemoveBoat(Boat boat)
        {
            int index = spots.FindIndex(t => t.boat == boat);
            spots.RemoveAt(index);
            if (spots.Count > 0)
            {
                FreeSpace += boat.BerthSpace;

                return this;
            }

            return null;
        }
    }
}