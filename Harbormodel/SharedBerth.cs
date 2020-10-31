using System;
using System.Collections.Generic;
using System.Linq;

namespace Harbor.Model
{
    internal class SharedBerth : IBerth
    {
        private readonly List<(Boat boat, int berthedFor)> spots = new List<(Boat boat, int berthedFor)>();

        public SharedBerth(Boat boat)
        {
            spots.Add((boat, 0));
            FreeSpace = 1.0 - boat.BerthSpace;
        }

        /// <inheritdoc />
        public double FreeSpace { get; private set; }

        /// <inheritdoc />
        public IEnumerable<(Boat boat, int berthTime)> Occupancy => spots.AsEnumerable();

        /// <inheritdoc />
        public int Size => 1;

        /// <inheritdoc />
        public IBerth AddBoat(Boat boat, int berthedFor)
        {
            if (FreeSpace < boat.BerthSpace) throw new InvalidOperationException("Not enough space to add boat.");
            spots.Add((boat, berthedFor));
            FreeSpace -= boat.BerthSpace;

            return this;
        }

        /// <inheritdoc />
        public void IncrementTime()
        {
            for (var i = 0; i < spots.Count; i++)
            {
                (Boat boat, int berthedFor) = spots[i];
                spots[i] = (boat, berthedFor + 1);
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