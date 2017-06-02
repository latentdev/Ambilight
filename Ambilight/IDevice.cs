using HidSharp;
using System.Collections.Generic;

namespace AmbilightLib
{
    public interface IDevice
    {
        IEnumerable<HidDevice> GetDeviceList();
        void ChangeDevice(string deviceName);
        bool Open();
        bool Close();
        byte[] Read();
        bool Write(byte[] data);
    }
}
