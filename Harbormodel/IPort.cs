using System;
using System.Collections.Generic;

namespace Harbor.Model
{
    public interface IPort
    {
        public int BoatCount { get; }
        public IEnumerable<Boat> Boats { get; }
        public int DockCount { get; }
        public IEnumerable<Dock> Docks { get; }
        public IEnumerable<Boat> LeftToday { get; }
        public DateTime Time { get; }
        public int Size { get; }
        public void IncrementTime();
        public bool TryAdd(Boat boat);
        public bool TryRemove(Boat boat);
    }
}