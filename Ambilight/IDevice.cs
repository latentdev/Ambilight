using HidSharp;
using System.Collections.Generic;

namespace AmbilightLib
{
    public interface IDevice
    {
        /// <summary>
        /// get list of devices
        /// </summary>
        /// <returns></returns>
        IEnumerable<HidDevice> GetDeviceList();
        /// <summary>
        /// changes currently used device
        /// </summary>
        /// <param name="deviceName">name of the device to change to</param>
        void ChangeDevice(string deviceName);
        /// <summary>
        /// opens stream
        /// </summary>
        /// <returns>true if successful</returns>
        bool Open();
        /// <summary>
        /// closes stream
        /// </summary>
        /// <returns>true if succesful</returns>
        bool Close();
        /// <summary>
        /// read packet from device
        /// </summary>
        /// <returns>returns byte array packet</returns>
        byte[] Read();
        /// <summary>
        /// writes byte array to device
        /// </summary>
        /// <param name="data">array to write</param>
        /// <returns>true if succesful</returns>
        bool Write(byte[] data);
    }
}
