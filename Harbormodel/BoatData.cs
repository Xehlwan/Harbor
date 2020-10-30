using System;
using System.Collections.Generic;
using System.Text;

namespace Harbor.Model
{
    public class BoatData
    {
        public string Type { get; set; }
        public char Prefix { get; set; }
        public string Code { get; set; }
        public int TopSpeed { get; set; }
        public int Weight { get; set; }
        public int Characteristic { get; set; }
    }
}
