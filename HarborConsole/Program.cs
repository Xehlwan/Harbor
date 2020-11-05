using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Harbor.Model;
using static System.Console;

namespace Harbor.Console
{
    internal class Program
    {
        private static PortControl portControl;
        private static bool windowOnly = false;
        private static bool isRunning = true;

        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Any(x => x == "-window")) windowOnly = true;
            Initialize();
            //LoadWindow();
            CursorVisible = false;
            while (isRunning)
            {
                PortMenu();
                ConsoleCommand();
            }
        }

        private static void ConsoleCommand()
        {
            bool inMenu = true;
            do
            {
                if (KeyAvailable)
                {
                    switch (ReadKey(true).Key)
                    {
                        case ConsoleKey.Escape:
                            inMenu = false;
                            break;
                        case ConsoleKey.W:
                            LoadWindow();
                            return;
                        case ConsoleKey.R:
                            portControl.AddBoat(HarborHelper.GetRandomBoat().boat);
                            PortMenu();
                            break;
                        case ConsoleKey.D:
                            portControl.IncrementTime();
                            PortMenu();
                            break;
                        case ConsoleKey.S:
                            portControl.SavePortData();
                            break;
                        case ConsoleKey.L:
                            portControl.LoadPortData();
                            PortMenu();
                            break;
                        case ConsoleKey.C:
                            portControl.ResetPort();
                            PortMenu();
                            break;
                    }
                }
            } while (inMenu);
        }

        private static void PortMenu()
        {
            Clear();
            int desiredHeight = portControl.Berths.Count() + (3 + portControl.DockCount * 3);
            BufferHeight = desiredHeight;
            SetCursorPosition(0,0);
            WriteLine("     HARBOR");
            WriteLine("----------------");
            for (int i = 1; i <= portControl.DockCount; i++)
            {
                WriteLine();
                WriteLine($"Dock [{i}]");
                WriteLine("----");
                foreach (var row in portControl.Berths.Where(b => b.DockIndex == i))
                {
                    var boat = row.Boat;
                    if (boat is null)
                    {
                        if (ForegroundColor != ConsoleColor.Green) ForegroundColor = ConsoleColor.Green;
                        WriteLine($"[{row.BerthIndex:00}] -FREE-");
                    }
                    else
                    {
                        if (ForegroundColor != ConsoleColor.Gray) ForegroundColor = ConsoleColor.Gray;
                        WriteLine($"[{row.BerthIndex:00}] {boat.TypeName} ({boat.IdentityCode}): {boat.Weight}kg, {boat.TopSpeed * 1.852}km/h, {boat.Characteristic}: {boat.CharacteristicValue}");
                    }
                    if (ForegroundColor != ConsoleColor.Gray) ForegroundColor = ConsoleColor.Gray;
                }
            }
            
        }

        private static void Initialize()
        {
            portControl = new PortControl();
            portControl.LoadPortData();
        }

        private static void LoadWindow()
        {
            Interop.HideConsole();
            Window main = new Wpf.MainWindow(portControl);
            main.ShowDialog();
            if (!windowOnly)
            {
                Interop.ShowConsole();
            }
        }
    }
}