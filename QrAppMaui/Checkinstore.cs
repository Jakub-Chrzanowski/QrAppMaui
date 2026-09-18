using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrAppMaui
{
    internal class Checkinstore
    {
        public static ObservableCollection<Checkinentry> Entries { get; } = new();
    }
}
