using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbilightLib
{
    public interface IAmbilight
    {
        /// <summary>
        /// holds the current state of the program:Running, Stopped
        /// </summary>
        bool state { get; set; }
        /// <summary>
        /// value for determining depth for color averaging
        /// </summary>
        int depth { get; set; }
        /// <summary>
        /// # of leds vertical
        /// </summary>
        int vertical { get; set; }
        /// <summary>
        /// # of leds horizontal
        /// </summary>
        int horizontal { get; set; }
        /// <summary>
        /// # of pixels to push the left and right sides in.
        /// </summary>
        int verticalOffset { get; set; }
        /// <summary>
        /// # of pixels to push the top and bottom in
        /// </summary>
        int horizontalOffset { get; set; }
        /// <summary>
        /// application fps
        /// </summary>
        int fps { get; set; }
        /// <summary>
        /// fps from device
        /// </summary>
        int deviceFps { get; set; }
        event EventHandler<EventArgs> DataReceived;

        /// <summary>
        /// Starts the main loop for color analysis
        /// </summary>
        /// <param name="deviceName">Name of device to send color data to</param>
        /// <returns>returns true if succesful</returns>
        bool Start(string deviceName);
        /// <summary>
        /// Stops the main loop
        /// </summary>
        /// <returns>returns true if succesful</returns>
        bool Stop();
        /// <summary>
        /// gets the current device
        /// </summary>
        /// <returns>returns handle to device</returns>
        IDevice GetDevice();
    }
}
