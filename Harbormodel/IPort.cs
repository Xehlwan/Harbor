using System;
using System.Collections.Generic;

namespace Harbor.Model
{
    public interface IPort
    {
        /// <summary>
        /// The number of boats in the port.
        /// </summary>
        public int BoatCount { get; }
        /// <summary>
        /// An enumeration of all individual boats in the port.
        /// </summary>
        public IEnumerable<Boat> Boats { get; }
        /// <summary>
        /// The number of docks in the port.
        /// </summary>
        public int DockCount { get; }
        /// <summary>
        /// An enumeration of all individual docks in the port.
        /// </summary>
        public IEnumerable<Dock> Docks { get; }
        /// <summary>
        /// An enumeration of all boats that have left today.
        /// </summary>
        public IEnumerable<Boat> LeftToday { get; }
        /// <summary>
        /// The total number of Berth spots in the port.
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// The current date and time.
        /// </summary>
        public DateTime Time { get; }
        /// <summary>
        /// The <see cref="IPort"/> data wrapped by this object, if any.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if there is no underlying data.</exception>
        public IPort UnderlyingData { get; }
        /// <summary>
        /// Increment the time one step.
        /// </summary>
        public void IncrementTime();
        /// <summary>
        /// Try to add a boat to the port.
        /// </summary>
        /// <param name="boat">The boat to add</param>
        /// <returns><see langword="true"/> if the operation succeeded, otherwise <see langword="false"/>.</returns>
        public bool TryAdd(Boat boat);
        /// <summary>
        /// Try to remove a boat from the port.
        /// </summary>
        /// <param name="boat">The boat to remove from the port.</param>
        /// <returns><see langword="true"/> if the operation succeeded, otherwise <see langword="false"/>.</returns>
        public bool TryRemove(Boat boat);
    }
}