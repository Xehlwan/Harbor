using System;

namespace Harbor.Model
{
    public class PortData
    {
        public DockData[] Docks { get; set; }
        public string DockChoiceAlgorithm { get; set; }
        public DateTime Time { get; set; }
    }
}