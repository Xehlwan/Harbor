using System.Collections.Generic;

namespace Harbor.Model
{
    public interface IBerth
    {
        public double FreeSpace { get; }
        public IEnumerable<(Boat boat, int berthTime)> Occupancy { get; }
        public int Size { get; }
        public IBerth AddBoat(Boat boat, int berthedFor);
        public void IncrementTime();
        public IBerth RemoveBoat(Boat boat);
    }
}