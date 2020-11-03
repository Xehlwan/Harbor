using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Harbor.Model
{
    public class PortControl : INotifyPropertyChanged
    {
        private readonly object autoLock = new object();
        private readonly object logLock = new object();
        private int autoBoatsPerDay;
        private int automationInterval;
        private CancellationTokenSource automationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource logCheckerTokenSource = new CancellationTokenSource();
        private int logCheckInterval;
        private string logFile = "port.log";
        private IPort port;
        private string saveFile = "port.json";

        public PortControl(IPort port)
        {
            this.port = port;
            AutoBoatsPerDay = 5;
            AutomationInterval = 5000;
            LogCheckInterval = 2500;
            logCheckerTokenSource.Cancel();
            automationTokenSource.Cancel();
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public int AutoBoatsPerDay
        {
            get => autoBoatsPerDay;
            set
            {
                autoBoatsPerDay = value;
                OnPropertyChanged();
            }
        }

        public int AutomationInterval
        {
            get => automationInterval;
            set
            {
                automationInterval = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Boat> Boats => port.Boats;

        public int BoatsInPort => port.BoatCount;
        public int BoatsLeftCount => port.LeftToday.Count();

        public IEnumerable<Boat> BoatsLeftToday => port.LeftToday;

        public double FreePercentage => FreeSpots / (double) port.Size;
        public int FreeSpots => port.Docks.SelectMany(d => d.BerthSpots).Count(b => b is null);
        public bool IsLogCheckerRunning { get; private set; }
        public bool IsSimulating { get; private set; }

        public int LogCheckInterval
        {
            get => logCheckInterval;
            set
            {
                logCheckInterval = value;
                OnPropertyChanged();
            }
        }

        public void ClearLog()
        {
            if (File.Exists(logFile))
                File.Delete(logFile);
        }

        public void ResetPort(params int[] dockSizes)
        {
            if (dockSizes.Length == 0 || dockSizes.Any(x => x <= 0))
            {
                dockSizes = new int[DockCount];
                using var enumerator = port.Docks.GetEnumerator();
                
                for (int i = 0; i < dockSizes.Length; i++)
                {
                    enumerator.MoveNext();
                    dockSizes[i] = enumerator.Current?.Size ?? 32;
                }
            }
            Port newPort = new Port(dockSizes);
            port = new PortLogger(newPort, logFile);
            OnPortChanged();
            OnBoatsChanged();
            OnLeftTodayChanged();
        }

        public int DockCount => port.DockCount;

        private Port GetBasePort()
        {
            IPort current = port;
            do
            {
                if (current is Port basePort)
                {
                    return basePort;
                }

                current = current.UnderlyingData;
            } while (!(current is null));
            throw new Exception("Couldn't get an underlying Port.");
        }

        public bool SavePortData()
        {
            try
            {
                string json = GetBasePort().Serialize();
                File.WriteAllText(saveFile, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool LoadPortData()
        {
            Port newPort;
            try
            {
                string json = File.ReadAllText(saveFile);
                newPort = Port.Deserialize(json);
            }
            catch
            {
                return false;
            }
            port = new PortLogger(newPort, logFile);
            OnPortChanged();
            OnBoatsChanged();
            OnLeftTodayChanged();
            return true;
        }

        private void OnPortChanged()
        {
            OnPropertyChanged(nameof(DockCount));
        }

        public string LogFile
        {
            get => logFile;
            set
            {
                logFile = value;
                if (port is PortLogger logger) port = new PortLogger(logger.UnderlyingData, logFile);
                OnPropertyChanged();
            }
        }

        public string LogLastLine => GetLogLast(logFile);

        public string SaveFile
        {
            get => saveFile;
            set
            {
                saveFile = value;
                OnPropertyChanged();
            }
        }

        public void AddBoat(Boat boat)
        {
            if (port.TryAdd(boat)) OnBoatsChanged();
        }

        public void IncrementTime()
        {
            port.IncrementTime();
            OnBoatsChanged();
            OnLeftTodayChanged();
        }

        public void RemoveBoat(Boat boat)
        {
            if (port.TryRemove(boat)) OnBoatsChanged();
        }

        public void StartLogChecker()
        {
            lock (logLock)
            {
                if (IsLogCheckerRunning) return;
                IsLogCheckerRunning = true;
            }

            logCheckerTokenSource = new CancellationTokenSource();
            CancellationToken token = logCheckerTokenSource.Token;
            HarborHelper
                .GetLogCheckTask(logFile, OnLogChanged, LogCheckInterval, token, () => IsLogCheckerRunning = false)
                .Start();
        }

        public void StartSimulation()
        {
            lock (autoLock)
            {
                if (IsSimulating) return;
                IsSimulating = true;
            }

            automationTokenSource = new CancellationTokenSource();
            CancellationToken token = automationTokenSource.Token;
            HarborHelper.GetHarborAutomationTask(AddBoat, IncrementTime, AutoBoatsPerDay, AutomationInterval, token,
                                                 () => IsSimulating = false)
                        .Start();
        }

        public void StopLogChecker()
        {
            logCheckerTokenSource.Cancel();
        }

        public void StopSimulation()
        {
            automationTokenSource.Cancel();
        }

        /// <summary>
        /// Called to notify subscribers that a specific property may have changed.
        /// </summary>
        /// <param name="name">The name of the property. If not provided, uses the caller's name.</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static string GetLogLast(string filePath)
        {
            return !File.Exists(filePath) ? string.Empty : File.ReadLines("port.log").Last();
        }

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

        /// <summary>
        /// Called whenever the collection of boats that left today changes.
        /// </summary>
        private void OnLeftTodayChanged()
        {
            if (port.LeftToday.Any()) OnBoatsChanged();

            OnPropertyChanged(nameof(BoatsLeftCount));
            OnPropertyChanged(nameof(BoatsLeftToday));
        }

        private void OnLogChanged()
        {
            OnPropertyChanged(nameof(LogLastLine));
        }
    }
}