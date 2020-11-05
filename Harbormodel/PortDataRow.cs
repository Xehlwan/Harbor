using System;
using System.Collections.Generic;
using System.Text;

namespace Harbor.Model
{
    public readonly struct PortDataRow
    {
        public PortDataRow(int dockIndex, int berthIndex, Boat boat, int? berthedFor)
        {
            DockIndex = dockIndex;
            BerthIndex = berthIndex;
            Boat = boat;
            BerthedFor = berthedFor;
        }

        public int DockIndex { get; }
        public int BerthIndex { get; }
        public Boat Boat { get; }
        public int? BerthedFor { get; }

        public int? TimeLeft => BerthedFor is null ? null : Boat.BerthTime - BerthedFor;
    }
}
