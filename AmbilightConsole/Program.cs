using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmbilightLib;
using HidSharp;
using System.Threading;

namespace AmbilightConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Ambilight lights = new Ambilight();
            HidDevice [] list = (HidDevice[])lights.device.GetDeviceList();
            int i = 0;
            foreach (var device in list)
            {

                Console.WriteLine("{0}. {1}", i, device.ProductName);
                i++;
            }
            Console.Write("Enter number of selection: ");
            i=Convert.ToInt32(Console.ReadLine());
            lights.Start(list[i].ProductName);
            Thread.Sleep(10);
            //Console.WriteLine(lights.device.Read());
            Console.Write("Press any key to quit: ");
            Console.ReadKey();
        }
    }
}
