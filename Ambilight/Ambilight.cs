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
        public IDevice device;
        public bool state { get; set; }
        Color[][] sides = new Color[4][];
        public int depth { get; set; }
        public int vertical { get; set; }
        public int horizontal { get; set; }
        public int verticalOffset { get; set; }
        public int horizontalOffset { get; set; }
        public int fps { get; set; }
        int screenWidth;
        int screenHeight;
        byte[] buffer;

        public event EventHandler<EventArgs> DataReceived;

        public Ambilight()
        {
            device = USBDevice.Instance;
            state = false;
            vertical = 3;
            horizontal = 4;
            verticalOffset = 100;
            horizontalOffset = 0;
            //left
            sides[0] = new Color[vertical];
            //top
            sides[1] = new Color[horizontal];
            //right
            sides[2] = new Color[vertical];
            //bottom
            sides[3] = new Color[horizontal];
            depth = 10;
            screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            //changed horizontal to 1 for just the top
            buffer = new byte[((vertical * 2) * 3) + ((horizontal * 1) * 3)];
        }

        public void SetDevice(string deviceName)
        {
            device.ChangeDevice(deviceName);
        }

        public void SetDimensions(int vertical, int horizontal)
        {
            sides[0] = new Color[vertical];
            sides[1] = new Color[horizontal];
            sides[2] = new Color[vertical];
            sides[3] = new Color[horizontal];
        }

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

        public bool Stop()
        {
            bool success = device.Close();
            fps = 0;
            DataReceived?.Invoke(this, new EventArgs());
            if (success)
            {
                state = false;
            }
            return success;
        }

        private void Loop()
        {
            DateTime current;
            while (state)
            {
                current = DateTime.Now;
                GetFrame();
                WriteFrame();
                int span = (DateTime.Now-current).Milliseconds;
                fps = 1000 / span;
                DataReceived?.Invoke(this, new EventArgs());
            }
            fps = 0;
            DataReceived?.Invoke(this, new EventArgs());
        }
        private void GetFrame()
        {
            Rectangle rect = new Rectangle(0+horizontalOffset, 0, depth, screenHeight);
            sides[0] = VerticalColors(sides[0], ScreenGraphics.CaptureFromScreen(rect));
            sides[0] = sides[0].Reverse().ToArray();
            rect = new Rectangle(0, 0+verticalOffset, screenWidth, depth);
            sides[1] = HorizontalColors(sides[1], ScreenGraphics.CaptureFromScreen(rect));
            rect = new Rectangle(screenWidth - depth-horizontalOffset, 0, depth, screenHeight);
            sides[2] = VerticalColors(sides[2], ScreenGraphics.CaptureFromScreen(rect));

            //this is the bottom of the screen. un-comment to get data on the bottom of the screen.

            //rect = new Rectangle(0, screenHeight - depth-verticalOffset, screenWidth, depth);
            //sides[3] = HorizontalColors(sides[3], ScreenGraphics.CaptureFromScreen(rect));
            //sides[3] = sides[3].Reverse().ToArray();
        }
        private void WriteFrame()
        {
            int index = 0;
            //Values for my string of lights are Green, Red, Blue //changed to 3 from 4
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
            device.Write(buffer);
        }
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

        public IDevice GetDevice()
        {
            return device;
        }
    }
}
