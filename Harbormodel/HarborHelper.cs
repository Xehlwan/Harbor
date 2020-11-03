using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Harbor.Model
{
    public static class HarborHelper
    {
        private static readonly Dictionary<string, Dock.BerthingAlgorithm> berthingAlgorithms;
        private static readonly Dictionary<string, Type> boatTypes;
        private static readonly Dictionary<string, Port.DockChoiceAlgorithm> dockAlgorithms;
        private static readonly ReadOnlyDictionary<string, Dock.BerthingAlgorithm> readOnlyBerthingAlgorithms;
        private static readonly ReadOnlyDictionary<string, Type> readOnlyBoatTypes;
        private static readonly ReadOnlyDictionary<string, Port.DockChoiceAlgorithm> readOnlyDockAlgorithms;
        private static readonly Random rng = new Random();

        static HarborHelper()
        {
            boatTypes = new Dictionary<string, Type>();
            dockAlgorithms = new Dictionary<string, Port.DockChoiceAlgorithm>();
            berthingAlgorithms = new Dictionary<string, Dock.BerthingAlgorithm>();
            readOnlyBoatTypes = new ReadOnlyDictionary<string, Type>(boatTypes);
            readOnlyDockAlgorithms = new ReadOnlyDictionary<string, Port.DockChoiceAlgorithm>(dockAlgorithms);
            readOnlyBerthingAlgorithms = new ReadOnlyDictionary<string, Dock.BerthingAlgorithm>(berthingAlgorithms);

            GetLocalAssemblyTypes();
            PopulateStaticDockAlgorithms();
            PopulateStaticBerthingAlgorithms();
        }

        private static void PopulateStaticDockAlgorithms()
        {
            var algorithms = typeof(Port).GetProperties(BindingFlags.Public | BindingFlags.Static)
                                         .Where(p => p.GetMethod?.ReturnType == typeof(Port.DockChoiceAlgorithm));

            foreach (PropertyInfo info in algorithms)
            {
                if (!(info.GetValue(null) is Port.DockChoiceAlgorithm algorithm)) continue;
                dockAlgorithms.TryAdd(info.Name, algorithm);
            }
        }

        private static void PopulateStaticBerthingAlgorithms()
        {
            var algorithms = typeof(Dock).GetProperties(BindingFlags.Public | BindingFlags.Static)
                                         .Where(p => p.GetMethod?.ReturnType == typeof(Dock.BerthingAlgorithm));

            foreach (PropertyInfo info in algorithms)
            {
                if (!(info.GetValue(null) is Dock.BerthingAlgorithm algorithm)) continue;
                berthingAlgorithms.TryAdd(info.Name, algorithm);
            }
        }

        public static IReadOnlyDictionary<string, Dock.BerthingAlgorithm> RegisteredBerthingAlgorithms =>
            readOnlyBerthingAlgorithms;

        public static IReadOnlyDictionary<string, Type> RegisteredBoatTypes => readOnlyBoatTypes;

        public static IReadOnlyDictionary<string, Port.DockChoiceAlgorithm> RegisteredDockAlgorithms =>
            readOnlyDockAlgorithms;

        public static Task GetHarborAutomationTask(Action<Boat> addBoatAction, Action timeTickAction, int boatsPerDay,
                                                   int interval, CancellationToken token, Action endCallback)
        {
            return new Task(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    for (var i = 0; i < boatsPerDay; i++) addBoatAction(GetRandomBoat().boat);

                    try
                    {
                        await Task.Delay(interval, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    timeTickAction();
                }

                endCallback();
            }, token, TaskCreationOptions.LongRunning);
        }

        public static Task GetLogCheckTask(string logFile, Action notify, int interval, CancellationToken token,
                                           Action endCallback)
        {
            return new Task(async () =>
            {
                if (!File.Exists(logFile)) return;
                DateTime lastWrite = File.GetLastWriteTimeUtc(logFile);
                while (!token.IsCancellationRequested)
                {
                    if (!File.Exists(logFile)) return;
                    DateTime currentWrite = File.GetLastWriteTimeUtc(logFile);
                    if (currentWrite > lastWrite)
                    {
                        lastWrite = currentWrite;
                        notify.Invoke();
                    }

                    try
                    {
                        await Task.Delay(interval, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }

                endCallback();
            }, token, TaskCreationOptions.LongRunning);
        }

        public static (string name, Boat boat) GetRandomBoat(Type type = null, string name = null)
        {
            bool unknownType = type is null || !type.IsSubclassOf(typeof(Boat));
            int index = rng.Next(boatTypes.Count);
            if (unknownType) type = boatTypes.ElementAt(index).Value;
            if (string.IsNullOrEmpty(name))
            {
                if (unknownType)
                    name = boatTypes.ElementAt(index).Key;
                else
                    name = type.Name;
            }

            (int weightMin, int weightMax) = ((int, int)) (type.GetProperty(nameof(Boat.WeightLimits),
                                                                            BindingFlags.Public | BindingFlags.Static)
                                                               ?.GetValue(null) ?? (0, 0));

            (int speedMin, int speedMax) = ((int, int)) (type.GetProperty(nameof(Boat.SpeedLimits),
                                                                          BindingFlags.Public | BindingFlags.Static)
                                                             ?.GetValue(null) ?? (0, 0));

            (int uniqueMin, int uniqueMax) = ((int, int)) (type.GetProperty(nameof(Boat.CharacteristicLimits),
                                                                            BindingFlags.Public | BindingFlags.Static)
                                                               ?.GetValue(null) ?? (0, 0));

            var boat = (Boat) Activator.CreateInstance(type,
                                                       rng.Next(weightMin, weightMax + 1),
                                                       rng.Next(speedMin, speedMax + 1),
                                                       rng.Next(uniqueMin, uniqueMax + 1));

            return (name, boat);
        }

        public static void RegisterBerthingAlgorithm(Dock.BerthingAlgorithm algorithm, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Invalid name string.", nameof(name));
            if (!berthingAlgorithms.TryAdd(name, algorithm))
                throw new InvalidOperationException("Algorithm with this name already registered.");
        }

        public static void RegisterBoatType(Type boatType, string name = null)
        {
            if (!boatType.IsSubclassOf(typeof(Boat))) throw new ArgumentException("Not a boat type.", nameof(boatType));
            if (string.IsNullOrEmpty(name)) name = boatType.Name;

            if (!boatTypes.TryAdd(name, boatType))
                throw new InvalidOperationException("Type with this name already registered.");
        }

        public static void RegisterDockAlgorithm(Port.DockChoiceAlgorithm algorithm, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Invalid name string.", nameof(name));
            if (!dockAlgorithms.TryAdd(name, algorithm))
                throw new InvalidOperationException("Algorithm with this name already registered.");
        }

        private static void GetLocalAssemblyTypes()
        {
            IEnumerable<Type> types = typeof(HarborHelper).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Boat)));
            foreach (Type type in types)
                if (!boatTypes.TryAdd(type.Name, type))
                    Debug.Fail($"Couldn't register assembly type: {type.FullName}.");
        }
    }
}