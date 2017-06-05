using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using System.Diagnostics;

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
            device = devices.Where(x => (x.VendorID == 5824 && x.ProductID == 1158 && x.ProductName == "Teensyduino RawHID")).FirstOrDefault();
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
                stream = device.Open();
                stream.ReadTimeout = System.Threading.Timeout.Infinite;
                //stream.WriteTimeout = System.Threading.Timeout.Infinite;

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
            int numberOfPackets = (int)Math.Ceiling((double)data.Length / 62);
            int index = 0;
            byte[][] packets = new byte[numberOfPackets][];
            for (int packet = 0; packet < numberOfPackets; packet++)
            {
                packets[packet] = new byte[65];
                packets[packet][0] = 0;
                packets[packet][1] = (byte)packet;
                for (int i = 2; i < 65; i++)
                {
                    if (index < data.Length)
                    {
                        packets[packet][i] = data[index];
                        index++;
                    }
                }
            }
            //Debugger.Break();
            //string packet = null;
            //byte[] buffer = new byte[65];
            //buffer[0] = 0;
            //buffer[1] = 1;
            //for (int i = 2; i < 65; i++)
            //    buffer[i] = data[i-2];
            if (stream.CanWrite)
            {
                try
                {
                    //for (int i = 0; i < 65; i++)
                    //{
                    //    packets[i][1] = (byte)numberOfPackets;
                    //    packets[i][2] = (byte)i;
                    //    stream.Write(packets[i], 0, packets[i].Length);
                    //}
                    
                    //for(int i=0;i<buffer.Length;i++)
                    //{
                    //    packet += buffer[i];
                    //}
                    //Console.WriteLine(packet);
                    for(int i=0;i<numberOfPackets;i++)
                        stream.Write(packets[i], 0, packets[i].Length);
                } catch (TimeoutException e)
                {
                    Console.WriteLine(e);
                }
                return true;
            }
            else
                return false;
        }
    }
}
