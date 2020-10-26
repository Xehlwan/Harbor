using System.Collections.Generic;

namespace Marina.Model
{
    public interface IBerth
    {
        public double FreeSpace { get; }
        public int Size { get; }
        public IBerth AddBoat(Boat boat);
        public IEnumerable<(Boat boat, int berthTime)> GetBoats();
        public void IncrementTime();
        public IBerth RemoveBoat(Boat boat);
    }
}