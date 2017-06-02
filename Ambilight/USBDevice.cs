using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;

namespace AmbilightLib
{
    public class USBDevice : IDevice
    {
        private static USBDevice instance;
        private HidDevice device;
        private HidStream stream;

        private USBDevice()
        {
            HidDeviceLoader deviceLoader = new HidDeviceLoader();
            IEnumerable<HidDevice> devices = deviceLoader.GetDevices();
            device = devices.Where(x => (x.VendorID == 5824 && x.ProductID == 1158)).FirstOrDefault();
        }

        public static USBDevice Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new USBDevice();
                }
                return instance;
            }
        }

        public void ChangeDevice(string deviceName)
        {
            HidDeviceLoader deviceLoader = new HidDeviceLoader();
            IEnumerable<HidDevice> devices = deviceLoader.GetDevices();
            device = devices.Where(x => (x.ProductName==deviceName)).FirstOrDefault();
        }


        public bool Close()
        {
            try
            {
                stream.Close();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public IEnumerable<HidDevice> GetDeviceList()
        {
            HidDeviceLoader deviceLoader = new HidDeviceLoader();
            return deviceLoader.GetDevices();
        }

        public bool Open()
        {
            try
            {
                stream.ReadTimeout = System.Threading.Timeout.Infinite;
                stream.WriteTimeout = System.Threading.Timeout.Infinite;
                stream = device.Open();
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public byte[] Read()
        {
            if (stream.CanRead)
            {
                byte[] byteArray = stream.Read();
                return byteArray;
            }
            else
                return null;
        }

        public bool Write(byte[] data)
        {
            if (stream.CanWrite)
            {
                stream.Write(data,0,data.Length);
                return true;
            }
            else
                return false;
        }
    }
}
