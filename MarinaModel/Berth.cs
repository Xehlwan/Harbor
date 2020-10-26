using System;
using System.Collections.Generic;

namespace Marina.Model
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
        public int Size => (int) Math.Ceiling(boat.BerthSpace);

        /// <inheritdoc />
        public IBerth AddBoat(Boat boat)
        {
            throw new InvalidOperationException("No free space to add boat.");
        }

        /// <inheritdoc />
        public IEnumerable<(Boat boat, int berthTime)> GetBoats()
        {
            yield return (boat, berthedFor);
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