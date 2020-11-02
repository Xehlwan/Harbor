using System;
using System.IO;
using Harbor.Model;

namespace Harbor.Control
{
    public static class App
    {
        private static Port port;

        public static void Initialize(string filePath)
        {
            var json = File.ReadAllText(filePath);
            port = Port.Deserialize(json);
        }

        public static void Initialize(params int[] dockSizes)
        {
            port = new Port(dockSizes);
        }
    }
}
