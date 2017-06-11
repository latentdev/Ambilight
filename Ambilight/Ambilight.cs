using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AmbilightLib
{
    public class Ambilight: IAmbilight
    {
        /// <summary>
        /// current device
        /// </summary>
        public IDevice device;
        /// <summary>
        /// holds the state of the main loop: Running, Stopped
        /// </summary>
        public bool state { get; set; }
        /// <summary>
        /// 2d array of color data for the current frame.
        /// </summary>
        Color[][] sides = new Color[4][];
        /// <summary>
        /// the amount of pixels deep to use for color averaging
        /// </summary>
        public int depth { get; set; }
        /// <summary>
        /// number of leds vertical
        /// </summary>
        public int vertical { get; set; }
        /// <summary>
        /// # of leds horizontal
        /// </summary>
        public int horizontal { get; set; }
        /// <summary>
        /// # of pixels to push the left and right sides in by
        /// </summary>
        public int verticalOffset { get; set; }
        /// <summary>
        /// # of pixels to push the top and bottom in by
        /// </summary>
        public int horizontalOffset { get; set; }
        /// <summary>
        /// FPS of application
        /// </summary>
        public int fps { get; set; }
        /// <summary>
        /// FPS reported by device
        /// </summary>
        public int deviceFps { get; set; }
        /// <summary>
        /// width of screen in pixels
        /// </summary>
        int screenWidth;
        /// <summary>
        /// height of screen in pixels
        /// </summary>
        int screenHeight;
        /// <summary>
        /// byte array that RGB values are copied into. used to write to device.
        /// </summary>
        byte[] buffer;

        public event EventHandler<EventArgs> DataReceived;

        /// <summary>
        /// Default constructor set up for driving the leds on the circuit playground running the teensy core.
        /// </summary>
        public Ambilight()
        {
            device = USBDevice.Instance;
            state = false;
            vertical = 3;
            horizontal = 4;
            verticalOffset = 0;
            horizontalOffset = 0;
            //left
            sides[0] = new Color[vertical];
            //top
            sides[1] = new Color[horizontal];
            //right
            sides[2] = new Color[vertical];
            //bottom
            sides[3] = new Color[horizontal];
            //pixel depth. smaller is more efficient, but less accurate.
            depth = 1;
            screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            //create a buffer large enough to hold all our RGB values.changed horizontal to 1 for just the top of screen, 2 for both top and bottom.
            buffer = new byte[((vertical * 2) * 3) + ((horizontal * 1) * 3)];
        }
        /// <summary>
        /// Changes the device used to the passed in device
        /// </summary>
        /// <param name="deviceName">Name of device to change to</param>
        public void SetDevice(string deviceName)
        {
            device.ChangeDevice(deviceName);
        }
        /// <summary>
        /// redefines the number of leds we need to gather data for.
        /// </summary>
        /// <param name="vertical"></param>
        /// <param name="horizontal"></param>
        public void SetDimensions(int vertical, int horizontal)
        {
            sides[0] = new Color[vertical];
            sides[1] = new Color[horizontal];
            sides[2] = new Color[vertical];
            sides[3] = new Color[horizontal];
            buffer = new byte[((vertical * 2) * 3) + ((horizontal * 1) * 3)];
        }

        /// <summary>
        /// Starts the main loop. Opens device stream and sets the dimensions. Runs the loop in a new background thread.
        /// </summary>
        /// <param name="deviceName">Name of device to use</param>
        /// <returns>true if successful</returns>
        public bool Start(string deviceName)
        {
            SetDevice(deviceName);
            bool success = device.Open();
            SetDimensions(vertical, horizontal);
            if (success)
            {
                Thread t = new Thread(Loop) { IsBackground = true };
                t.Start();
                state = true;
            }
            return success;
        }
        /// <summary>
        /// stops main loop. bool is flipped and the thread is allowed to finish current loop.
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            state = false;
            return true;
        }
        /// <summary>
        /// Main loop. Gets the frame data and writes it to the device. Also calculates application fps and device fps.
        /// </summary>
        private void Loop()
        {
            DateTime current;
            while (state)
            {
                current = DateTime.Now;
                GetFrame();
                WriteFrame();
                GetDeviceFPS();
                int span = (DateTime.Now-current).Milliseconds;
                fps = 1000 / span;
                //update UI
                DataReceived?.Invoke(this, new EventArgs());
            }
            // fill the color array with black and write to device to turn off the lights when program is stopped.
            FillEmpty();
            WriteFrame();
            // close device stream.
            device.Close();
            fps = 0;
            deviceFps = 0;
            //update UI
            DataReceived?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Gets FPS from device
        /// </summary>
        private void GetDeviceFPS()
        {
            try
            {
                byte[] packet = device.Read();
                deviceFps = BitConverter.ToUInt16(packet,1);
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
        /// <summary>
        /// fills the frame with black
        /// </summary>
        private void FillEmpty()
        {
            for(int i=0;i<sides.Length;i++)
            {
                for(int m=0;m<sides[i].Length;m++)
                {
                    sides[i][m] = Color.Black;
                }
            }
        }
        /// <summary>
        /// grabs color data from screen and stores it in sides array
        /// </summary>
        private void GetFrame()
        {
            //define rectangle for the area of the screen we want to sample
            Rectangle rect = new Rectangle(0+horizontalOffset, 0, depth, screenHeight);
            //get color data from that section of the screen
            sides[0] = VerticalColors(sides[0], ScreenGraphics.CaptureFromScreen(rect));
            //reverse
            sides[0] = sides[0].Reverse().ToArray();
            //repeat for each side
            rect = new Rectangle(0, 0+verticalOffset, screenWidth, depth);
            sides[1] = HorizontalColors(sides[1], ScreenGraphics.CaptureFromScreen(rect));
            rect = new Rectangle(screenWidth - depth-horizontalOffset, 0, depth, screenHeight);
            sides[2] = VerticalColors(sides[2], ScreenGraphics.CaptureFromScreen(rect));

            //this is the bottom of the screen. un-comment to get data on the bottom of the screen.

            //rect = new Rectangle(0, screenHeight - depth-verticalOffset, screenWidth, depth);
            //sides[3] = HorizontalColors(sides[3], ScreenGraphics.CaptureFromScreen(rect));
            //sides[3] = sides[3].Reverse().ToArray();
        }
        /// <summary>
        /// write frame to device
        /// </summary>
        private void WriteFrame()
        {
            int index = 0;
            //Convert Color arrays to one long byte array.Values for my string of lights are Green, Red, Blue //changed to 3 sides(left,top,right) from 4(left,top,right,bottom)
            for (int side = 0; side < 3; side++)
            {
                for (int i = 0; i < sides[side].Count(); i++)
                {

                    buffer[index] = sides[side][i].R;
                    buffer[index + 1] = sides[side][i].G;

                    buffer[index + 2] = sides[side][i].B;
                    index += 3;
                }
            }
            //write buffer to device
            device.Write(buffer);
        }
        /// <summary>
        /// sample colors from a horizontal strip
        /// </summary>
        /// <param name="pixels">color array to store data in</param>
        /// <param name="bitmap">image to sample colors from</param>
        /// <returns>color array containing data from bitmap</returns>
        private Color[] HorizontalColors(Color[] pixels, Bitmap bitmap)
        {
            int count = pixels.Count();
            int multiplier = bitmap.Width / count;
            for (int i = 0; i < count; i++)
            {
                Point origin = new Point(i*multiplier, 0);
                pixels[i] = ScreenGraphics.GetAverageColor(bitmap, origin, multiplier, depth);
            }
            return pixels;
        }
        /// <summary>
        /// sample colors from a vertical strip
        /// </summary>
        /// <param name="pixels">color array to store data in</param>
        /// <param name="bitmap">image to sample colors from</param>
        /// <returns>color array containing data from bitmap</returns>
        private Color[] VerticalColors(Color [] pixels, Bitmap bitmap)
        {
            int count = pixels.Count();
            int multiplier = bitmap.Height / count;
            for (int i=0;i<count;i++)
            {
                Point origin = new Point(0, i * multiplier);
                pixels[i] = ScreenGraphics.GetAverageColor(bitmap, origin, depth, multiplier);
            }
            return pixels;
        }
        /// <summary>
        /// Get a handle to the device
        /// </summary>
        /// <returns>returns currently used device</returns>
        public IDevice GetDevice()
        {
            return device;
        }
    }
}
