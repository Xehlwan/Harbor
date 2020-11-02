using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Harbor.Model;

namespace Harbor.Console
{
    public class PortControl : INotifyPropertyChanged
    {
        private const string logFile = "port.log";
        private readonly IPort port;
        private bool logCheckerActive;
        private CancellationTokenSource cancellationSource;
        public PortControl(IPort port)
        {
            this.port = port;
        }

        public string LogLastLine => GetLogLast(logFile);

        public void StopLogChecker() => cancellationSource.Cancel();

        public void StartLogChecker()
        {
            if (logCheckerActive) return;
            logCheckerActive = true;
            cancellationSource = new CancellationTokenSource();
            var token = cancellationSource.Token;
            Task logChecker = new Task(() =>
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

        private void OnLogChanged()
        {
            OnPropertyChanged(nameof(LogLastLine));
        }

        private static string GetLogLast(string filePath) => !File.Exists(filePath) ? string.Empty : File.ReadLines("port.log").Last();

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        public IEnumerable<Boat> Boats => port.Boats;

        public IEnumerable<Boat> BoatsLeft => port.LeftToday;

        public int BoatsInPort => port.BoatCount;
        public int BoatsLeftCount => port.LeftToday.Count();

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

        private void OnLeftTodayChanged()
        {
            if (port.LeftToday.Any()) OnBoatsChanged();

            OnPropertyChanged(nameof(BoatsLeftCount));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void OnBoatsChanged()
        {
            OnPropertyChanged(nameof(BoatsInPort));
            OnPropertyChanged(nameof(Boats));
        }
    }
}