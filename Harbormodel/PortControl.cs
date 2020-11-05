using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private string turnedAwayFile = "turnedAway.json";
        private IPort port;
        private string saveFile = "port.json";
        private List<Boat> turnedAway = new List<Boat>();

        public PortControl(IPort port) : this()
        {
            this.port = port;
        }

        public int TotalWeight => Boats.Select(b => b.Weight).Sum();
        public double AverageSpeed => Boats.Select(b => b.TopSpeed).Sum() / (double) BoatsInPort;
        public PortControl()
        {
            if (port is null)
            {
                port = new PortLogger(new Port(32, 32), logFile);
            }
            AutoBoatsPerDay = 5;
            AutomationInterval = 5000;
            LogCheckInterval = 2500;
            logCheckerTokenSource.Cancel();
            automationTokenSource.Cancel();
        }

        public IEnumerable<string> BoatTypes => HarborHelper.RegisteredBoatTypes.Keys;

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
        public string Today => port.Time.ToShortDateString();
        public IEnumerable<PortDataRow> Berths
        {
            get
            {
                using var docks = port.Docks.GetEnumerator();
                for (int dockIndex = 0; dockIndex < port.DockCount; dockIndex++)
                {
                    docks.MoveNext();
                    var dock = docks.Current;
                    if (dock is null) break; 
                    for (int i = 0; i < dock.BerthSpots.Length; i++)
                    {
                        IEnumerable<(Boat boat, int berthTime)> occupancy = dock.BerthSpots[i]?.Occupancy;
                        if (!(occupancy is null))
                        {
                            foreach ((Boat boat, int berthTime) in occupancy)
                            {
                                yield return new PortDataRow(dockIndex + 1, i + 1, boat, berthTime);
                            }
                        }
                        else
                        {
                            yield return new PortDataRow(dockIndex + 1, i + 1, null, null);
                        }
                        
                    }
                }
            }
        }
        public IEnumerable<Boat> TurnedAway => turnedAway.AsEnumerable();
        public int TurnedAwayTotal => turnedAway.Count;
        public int BoatsInPort => port.BoatCount;
        public int BoatsLeftCount => port.LeftToday.Count();
        public IEnumerable<Boat> BoatsLeftToday => port.LeftToday;
        public int DockCount => port.DockCount;
        public double FreePercentage => (double)FreeSpots / port.Size;
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

        public int Size => port.Size;

        public void AddBoat(Boat boat)
        {
            if (port.TryAdd(boat)) OnBoatsChanged();
            else
            {
                turnedAway.Add(boat);
                OnPropertyChanged(nameof(TurnedAwayTotal));
                OnPropertyChanged(nameof(TurnedAway));
            }
        }

        public void ClearLog()
        {
            if (File.Exists(logFile)) File.Delete(logFile);
        }

        public void IncrementTime()
        {
            port.IncrementTime();
            OnBoatsChanged();
            OnLeftTodayChanged();
            OnPropertyChanged(nameof(Today));
        }

        public bool LoadPortData()
        {
            Port newPort;
            try
            {
                string json = File.ReadAllText(saveFile);
                newPort = Port.Deserialize(json);

                json = File.ReadAllText(turnedAwayFile);
                var collection = JsonSerializer.Deserialize(json, typeof(List<BoatData>)) as List<BoatData>;
                turnedAway.Clear();
                if (collection != null)
                    turnedAway.AddRange(collection.Select(Boat.FromData));
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

        public void RemoveBoat(Boat boat)
        {
            if (port.TryRemove(boat)) OnBoatsChanged();
        }

        public void ResetPort(params int[] dockSizes)
        {
            if (dockSizes.Length == 0 || dockSizes.Any(x => x <= 0))
            {
                dockSizes = new int[DockCount];
                using IEnumerator<Dock> enumerator = port.Docks.GetEnumerator();

                for (var i = 0; i < dockSizes.Length; i++)
                {
                    enumerator.MoveNext();
                    dockSizes[i] = enumerator.Current?.Size ?? 32;
                }
            }

            var newPort = new Port(dockSizes);
            port = new PortLogger(newPort, logFile);
            OnPortChanged();
            OnBoatsChanged();
            OnLeftTodayChanged();
        }

        public bool SavePortData()
        {
            try
            {
                string json = GetBasePort().Serialize();
                File.WriteAllText(saveFile, json);

                var turnedAwayData = new List<BoatData>();
                foreach (Boat boat in turnedAway)
                {
                    turnedAwayData.Add(boat.AsData());
                }

                json = JsonSerializer.Serialize(turnedAwayData);
                File.WriteAllText(turnedAwayFile, json);
                return true;
            }
            catch
            {
                return false;
            }
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
            try
            {
                return !File.Exists(filePath) ? string.Empty : File.ReadLines("port.log").Last();
            }
            catch
            {
                return string.Empty;
            }
        }

        private Port GetBasePort()
        {
            IPort current = port;
            do
            {
                if (current is Port basePort) return basePort;

                current = current.UnderlyingData;
            } while (!(current is null));

            throw new Exception("Couldn't get an underlying Port.");
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
            OnPropertyChanged(nameof(Berths));
            OnPropertyChanged(nameof(TotalWeight));
            OnPropertyChanged(nameof(AverageSpeed));
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

        private void OnPortChanged()
        {
            OnPropertyChanged(nameof(DockCount));
        }

        public void Update()
        {
            OnLeftTodayChanged();
            OnBoatsChanged();
            OnLogChanged();
            OnPortChanged();
        }
    }
}