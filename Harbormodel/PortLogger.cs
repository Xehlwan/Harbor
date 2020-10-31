using System;
using System.Collections.Generic;
using System.IO;

namespace Harbor.Model
{
    public class PortLogger : IPort
    {
        private FileInfo fileInfo;

        public PortLogger(IPort port, string path) : this(port, path, false)
        {
        }

        public PortLogger(IPort port, string path, bool overwrite)
        {
            UnderlyingData = port;
            fileInfo = new FileInfo(path);
            if (overwrite && fileInfo.Exists) fileInfo.Delete();
        }

        /// <inheritdoc />
        public int BoatCount => UnderlyingData.BoatCount;

        /// <inheritdoc />
        public IEnumerable<Boat> Boats => UnderlyingData.Boats;

        /// <inheritdoc />
        public int DockCount => UnderlyingData.DockCount;

        /// <inheritdoc />
        public IEnumerable<Dock> Docks => UnderlyingData.Docks;

        /// <inheritdoc />
        public IEnumerable<Boat> LeftToday => UnderlyingData.LeftToday;

        /// <inheritdoc />
        public int Size => UnderlyingData.Size;

        /// <inheritdoc />
        public DateTime Time => UnderlyingData.Time;

        /// <inheritdoc />
        public IPort UnderlyingData { get; }

        public string GetFileLocation()
        {
            return fileInfo.FullName;
        }

        /// <inheritdoc />
        public void IncrementTime()
        {
            DateTime prevTime = UnderlyingData.Time;
            UnderlyingData.IncrementTime();
            Log($"Time incremented: [{prevTime:d}] => [{UnderlyingData.Time:d}]");
            foreach (Boat boat in LeftToday) LogWithDate("left the port.", boat);
        }

        public void SetFileLocation(string filePath)
        {
            fileInfo = new FileInfo(filePath);
        }

        /// <inheritdoc />
        public bool TryAdd(Boat boat)
        {
            bool success = UnderlyingData.TryAdd(boat);
            if (success)
                LogWithDate("found space at the harbor.", boat);
            else
                LogWithDate("was turned away due to lack of space.", boat);

            return success;
        }

        /// <inheritdoc />
        public bool TryRemove(Boat boat)
        {
            bool success = UnderlyingData.TryRemove(boat);
            if (success)
                LogWithDate("was removed from the harbor.", boat);
            else
                LogWithDate("could not be found and removed from the harbor.", boat);

            return success;
        }

        private void Log(string str)
        {
            using StreamWriter sw = fileInfo.AppendText();
            sw.WriteLine(str);
        }

        private void LogWithDate(string str, Boat boat)
        {
            string boatType = boat.GetType().Name;
            string code = boat.IdentityCode;
            LogWithDate($"{boatType} with ID:({code}) {str}");
        }

        private void LogWithDate(string str)
        {
            using StreamWriter sw = fileInfo.AppendText();
            sw.WriteLine($"[{Time:d}] {str}");
        }
    }
}