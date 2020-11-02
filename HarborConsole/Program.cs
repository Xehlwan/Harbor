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
        private const bool debugWindowed = true;
        private const bool debugModelTest = false;
        private const string logPath = "port.log";
        private const string savePath = "port.json";
        private static IPort port;
        private static FileInfo saveFile;
        private static PortControl portControl;

        [STAThread]
        private static void Main(string[] args)
        {
            Initialize();
            LoadWindow();
        }

        private static void Initialize()
        {
            saveFile = new FileInfo(savePath);
            if (saveFile.Exists)
            {
                using StreamReader sr = saveFile.OpenText();
                string json = sr.ReadToEnd();
                port = Port.Deserialize(json);
            }
            else
            {
                port = new Port(32, 32);
            }

            port = new PortLogger(port, logPath);
            portControl = new PortControl(port);
        }

        private static void LoadWindow()
        {
            Window main = new Wpf.MainWindow(portControl);
            main.ShowDialog();
            WriteLine("Window closed!");
        }
    }
}