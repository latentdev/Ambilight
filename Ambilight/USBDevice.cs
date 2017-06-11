using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;
using System.Diagnostics;
using System.IO;

namespace AmbilightLib
{
    /// <summary>
    /// class for using a USB protocol for communication with ambient light display device.
    /// </summary>
    public class USBDevice : IDevice
    {
        private static USBDevice instance;
        private HidDevice device;
        private HidStream stream;

        /// <summary>
        /// private Default constructor. Automatically sets device to the Teensyduino RawHID Device.
        /// 
        /// </summary>
        private USBDevice()
        {
            HidDeviceLoader deviceLoader = new HidDeviceLoader();
            IEnumerable<HidDevice> devices = deviceLoader.GetDevices();
            device = devices.Where(x => (x.VendorID == 5824 && x.ProductID == 1158 && x.ProductName == "Teensyduino RawHID")).FirstOrDefault();
        }

        /// <summary>
        /// get instance of USB device. We only want one instance of the device to exist so the constructor is private.
        /// </summary>
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

        /// <summary>
        /// Changes device being used.
        /// </summary>
        /// <param name="deviceName">String containing device Product Name</param>
        public void ChangeDevice(string deviceName)
        {
            HidDeviceLoader deviceLoader = new HidDeviceLoader();
            IEnumerable<HidDevice> devices = deviceLoader.GetDevices();
            device = devices.Where(x => (x.ProductName==deviceName)).FirstOrDefault();
        }

        /// <summary>
        /// Closes stream.
        /// </summary>
        /// <returns>true if stream was successfully closed. false if not.</returns>
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

        /// <summary>
        /// Gets the list of USB devices currently connected to the computer.
        /// </summary>
        /// <returns>Returns an enumerable list of devices</returns>
        public IEnumerable<HidDevice> GetDeviceList()
        {
            HidDeviceLoader deviceLoader = new HidDeviceLoader();
            return deviceLoader.GetDevices();
        }
        /// <summary>
        /// Opens a stream to the selected USB device.
        /// </summary>
        /// <returns>Returns true if stream was successfully opened. False if stream failed to open.</returns>
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
        /// <summary>
        /// Reads a packet in from the device and stores it in a byte array and returns it.
        /// </summary>
        /// <returns>Returns a byte array containing packet read from device</returns>
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
        /// <summary>
        /// breaks the passed in byte array into 65 byte packets and sends them to the device
        /// </summary>
        /// <param name="data">passed in byte array containing color data for the leds connected to the device</param>
        /// <returns>true if successful. false if it fails</returns>
        public bool Write(byte[] data)
        {
            //determine the number of packets needed to send whole byte array.
            int numberOfPackets = (int)Math.Ceiling((double)data.Length / 63);
            //keeps our location in the data array
            int index = 0;
            //create array of packets
            byte[][] packets = new byte[numberOfPackets][];
            for (int packet = 0; packet < numberOfPackets; packet++)
            {
                //create a new packet
                packets[packet] = new byte[65];
                //set report id
                packets[packet][0] = 0;
                //set the number of the packet. this is used on the device side to determine location to place packet in led array.
                packets[packet][1] = (byte)packet;
                //copy data from data array into the rest of the packet.
                for (int i = 2; i < 65; i++)
                {
                    if (index < data.Length)
                    {
                        packets[packet][i] = data[index];
                        index++;
                    }
                }
            }

            if (stream.CanWrite)
            {
                try
                {
                    //write our packets to the device
                    for(int i=0;i<numberOfPackets;i++)
                        stream.Write(packets[i], 0, packets[i].Length);
                } catch (Exception e)
                {
                    if (e is TimeoutException ||
                       e is InvalidOperationException ||
                       e is IOException)
                    {
                        Console.WriteLine(e);
                        return false;
                    }
                    throw;
                }
                return true;
            }
            else
                return false;
        }
    }
}
 