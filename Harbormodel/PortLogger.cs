using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Harbor.Model
{
    public class PortLogger : IPort
    {
        private readonly IPort port;
        private FileInfo fileInfo;

        public PortLogger(IPort port, string path) : this(port, path, false)
        {
        }
        public PortLogger(IPort port, string path, bool overwrite)
        {
            this.port = port;
            fileInfo = new FileInfo(path);
            if (overwrite && fileInfo.Exists)
            {
                fileInfo.Delete();
            }
        }

        public IPort Underlying => port;

        /// <inheritdoc />
        public int BoatCount => port.BoatCount;

        /// <inheritdoc />
        public IEnumerable<Boat> Boats => port.Boats;

        /// <inheritdoc />
        public int DockCount => port.DockCount;

        /// <inheritdoc />
        public IEnumerable<Dock> Docks => port.Docks;

        /// <inheritdoc />
        public IEnumerable<Boat> LeftToday => port.LeftToday;

        /// <inheritdoc />
        public DateTime Time => port.Time;

        /// <inheritdoc />
        public int Size => port.Size;

        /// <inheritdoc />
        public void IncrementTime()
        {
            var prevTime = port.Time;
            port.IncrementTime();
            Log($"Time incremented: [{prevTime:d}] => [{port.Time:d}]");
            foreach (Boat boat in LeftToday)
            {
                LogWithDate("left the port.", boat);
            }
        }

        /// <inheritdoc />
        public bool TryAdd(Boat boat)
        {
            var success = port.TryAdd(boat);
            if (success)
                LogWithDate("found space at the harbor.", boat);
            else
                LogWithDate("was turned away due to lack of space.", boat);

            return success;
        }

        /// <inheritdoc />
        public bool TryRemove(Boat boat)
        {
            var success = port.TryRemove(boat);
            if (success) LogWithDate("was removed from the harbor.", boat);
            else LogWithDate("could not be found and removed from the harbor.", boat);

            return success;
        }

        private void LogWithDate(string str, Boat boat)
        {
            var boatType = boat.GetType().Name;
            var code = boat.IdentityCode;
            LogWithDate($"{boatType} with ID:({code}) {str}");
        }

        private void LogWithDate(string str)
        {
            using StreamWriter sw = fileInfo.AppendText();
            sw.WriteLine($"[{Time:d}] {str}");
        }

        private void Log(string str)
        {
            using StreamWriter sw = fileInfo.AppendText();
            sw.WriteLine(str);
        }
    }
}
