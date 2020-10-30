using System.Text.Json.Serialization;

namespace Harbor.Model
{
    public class DockData
    {
        public string BerthingChoiceAlgorithm { get; set; }
        public int[] BerthTimes { get; set; }
        public BoatData[] Boats { get; set; }
        public int[] Indices { get; set; }
        public BoatData[] LeftToday { get; set; }
        public int Size { get; set; }
    }
}