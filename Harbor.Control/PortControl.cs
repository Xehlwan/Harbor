using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Harbor.Model;

namespace Harbor.Control
{
    public class PortControl : INotifyPropertyChanged
    {
        private IPort port;

        public PortControl(IPort port) => this.port = port;

        public List<Boat> Boats => port.Boats.ToList();

        public void AddBoat(Boat boat)
        {
            if(port.TryAdd(boat)) OnPropertyChanged(nameof(Boats));
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
