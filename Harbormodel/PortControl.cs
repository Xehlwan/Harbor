using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Harbor.Model
{
    public class PortControl : INotifyPropertyChanged
    {
        private string logFile = "port.log";
        private string saveFile = "port.json";
        private readonly IPort port;
        private bool logCheckerActive;
        private CancellationTokenSource cancellationSource;
        public PortControl(IPort port)
        {
            this.port = port;
        }

        public string LogLastLine => GetLogLast(logFile);

        public string LogFile
        {
            get => logFile;
            set
            {
                logFile = value;
                OnPropertyChanged();
            }
        }

        public string SaveFile
        {
            get => saveFile;
            set
            {
                saveFile = value;
                OnPropertyChanged();
            }
        }

        public void StopLogChecker() => cancellationSource.Cancel();

        public void StartLogChecker()
        {
            if (logCheckerActive) return;
            logCheckerActive = true;
            cancellationSource = new CancellationTokenSource();
            var token = cancellationSource.Token;
            var logChecker = new Task(() =>
            {
                if (!File.Exists(logFile)) return;
                logCheckerActive = true;
                DateTime lastWrite = File.GetLastWriteTimeUtc(logFile);
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        logCheckerActive = false;
                        return;
                    }
                    if (!File.Exists(logFile)) return;
                    var currentWrite = File.GetLastWriteTimeUtc(logFile);
                    if (currentWrite > lastWrite)
                    {
                        lastWrite = currentWrite;
                        OnLogChanged();
                    }
                    Task.Delay(5000, token).Wait(token);
                }
            }, token, TaskCreationOptions.LongRunning);

            logChecker.Start();
        }

        private void OnLogChanged() => OnPropertyChanged(nameof(LogLastLine));

        private static string GetLogLast(string filePath) => !File.Exists(filePath) ? string.Empty : File.ReadLines("port.log").Last();

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;


        public IEnumerable<Boat> Boats => port.Boats;

        public IEnumerable<Boat> BoatsLeftToday => port.LeftToday;

        public int BoatsInPort => port.BoatCount;
        public int BoatsLeftCount => port.LeftToday.Count();
        public int FreeSpots => port.Docks.SelectMany(d => d.BerthSpots).Count(b => b is null);

        public double FreePercentage => FreeSpots / (double) port.Size;
        public void AddBoat(Boat boat)
        {
            if (port.TryAdd(boat)) OnBoatsChanged();
        }

        public void RemoveBoat(Boat boat)
        {
            if (port.TryRemove(boat)) OnBoatsChanged();
        }

        public void IncrementTime()
        {
            port.IncrementTime();
            OnBoatsChanged();
            OnLeftTodayChanged();
        }

        /// <summary>
        /// Called whenever the collection of boats that left today changes.
        /// </summary>
        private void OnLeftTodayChanged()
        {
            if (port.LeftToday.Any()) OnBoatsChanged();

            OnPropertyChanged(nameof(BoatsLeftCount));
        }

        /// <summary>
        /// Called to notify subscribers that a specific property may have changed.
        /// </summary>
        /// <param name="name">The name of the property. If not provided, uses the caller's name.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Called whenever a change to the boats in port happens.
        /// </summary>
        private void OnBoatsChanged()
        {
            OnPropertyChanged(nameof(BoatsInPort));
            OnPropertyChanged(nameof(Boats));
            OnPropertyChanged(nameof(FreeSpots));
            OnPropertyChanged(nameof(FreePercentage));
        }
    }
}