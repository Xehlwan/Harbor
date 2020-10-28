using System;
using System.Collections.Generic;

namespace Harbor.Model
{
    public class Berth : IBerth
    {
        private readonly Boat boat;
        private int berthedFor;

        public Berth(Boat boat)
        {
            this.boat = boat;
            berthedFor = 0;
        }

        /// <inheritdoc />
        public double FreeSpace => 0;

        /// <inheritdoc />
        public IEnumerable<(Boat boat, int berthTime)> Occupancy
        {
            get { yield return (boat, berthedFor); }
        }

        /// <inheritdoc />
        public int Size => (int) Math.Ceiling(boat.BerthSpace);

        /// <exception cref="InvalidOperationException">Always thrown for this method on <see cref="Berth" />.</exception>
        /// <inheritdoc />
        public IBerth AddBoat(Boat boat)
        {
            throw new InvalidOperationException("No free space to add boat.");
        }

        /// <inheritdoc />
        public void IncrementTime()
        {
            berthedFor++;
        }

        /// <inheritdoc />
        public IBerth RemoveBoat(Boat boat)
        {
            return null;
        }
    }
}