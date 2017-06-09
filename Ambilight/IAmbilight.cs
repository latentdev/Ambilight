using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbilightLib
{
    public interface IAmbilight
    {
        bool state { get; set; }
        int depth { get; set; }
        int vertical { get; set; }
        int horizontal { get; set; }
        int verticalOffset { get; set; }
        int horizontalOffset { get; set; }
        int fps { get; set; }
        event EventHandler<EventArgs> DataReceived;

        bool Start(string deviceName);
        bool Stop();
        IDevice GetDevice();
    }
}
