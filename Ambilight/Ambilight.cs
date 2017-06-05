﻿using System;
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
        bool state = false;
        Color[][] sides = new Color[4][];
        int depth;
        int vertical;
        int horizontal;
        int verticalOffset;
        int horizontalOffset;
        int screenWidth;
        int screenHeight;
        CancellationTokenSource cts = null;
        byte[] buffer;
        //BackgroundWorker backgroundWorker;

        public Ambilight()
        {
            device = USBDevice.Instance;
            vertical = 52;
            horizontal = 90;
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
            depth = 20;
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
            if (success)
            {
                //if (cts == null || cts.IsCancellationRequested)
                //    cts = new CancellationTokenSource();

                //ThreadPool.QueueUserWorkItem(new WaitCallback(Loop), cts.Token);
                //backgroundWorker = new BackgroundWorker
                //{
                //    WorkerSupportsCancellation = true
                //};
                //backgroundWorker.DoWork += BackgroundWorkerOnDoWork;
                Thread t = new Thread(Loop) { IsBackground = true };
                t.Start();
                state = true;
            }
            return success;
        }

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                Loop();
                //Do your stuff here
                //worker.ReportProgress(0, "AN OBJECT TO PASS TO THE UI-THREAD");
            }
        }

        public bool Stop()
        {
            bool success = device.Close();
            if (success)
            {
                state = false;
            }
            return success;
        }

        public void Loop()
        {
            while (state)
            {
                GetFrame();
                WriteFrame();
            }
        }
        public void GetFrame()
        {
            Rectangle rect = new Rectangle(0+horizontalOffset, 0, depth, screenHeight);
            sides[0] = VerticalColors(sides[0], ScreenGraphics.CaptureFromScreen(rect));
            sides[0] = sides[0].Reverse().ToArray();
            rect = new Rectangle(0, 0+verticalOffset, screenWidth, depth);
            sides[1] = HorizontalColors(sides[1], ScreenGraphics.CaptureFromScreen(rect));
            rect = new Rectangle(screenWidth - depth-horizontalOffset, 0, depth, screenHeight);
            sides[2] = VerticalColors(sides[2], ScreenGraphics.CaptureFromScreen(rect));
            rect = new Rectangle(0, screenHeight - depth-verticalOffset, screenWidth, depth);
            sides[3] = HorizontalColors(sides[3], ScreenGraphics.CaptureFromScreen(rect));
            sides[3] = sides[3].Reverse().ToArray();
        }
        public void WriteFrame()
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
        public Color[] HorizontalColors(Color[] pixels, Bitmap bitmap)
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
        public Color[] VerticalColors(Color [] pixels, Bitmap bitmap)
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

    }
}
