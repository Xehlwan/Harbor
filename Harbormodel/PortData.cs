using System;

namespace Harbor.Model
{
    public class PortData
    {
        public string DockChoiceAlgorithm { get; set; }
        public DockData[] Docks { get; set; }
        public DateTime Time { get; set; }
    }
}