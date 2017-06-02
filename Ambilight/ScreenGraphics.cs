using System;
using System.Collections.Generic;
using System.Drawing;

namespace AmbilightLib
{
    class ScreenGraphics
    {
        public static Bitmap CaptureFromScreen(Rectangle area)
        {
            Bitmap bmpScreenCapture = null;
            Graphics p = null;
            try
            {
                bmpScreenCapture = new Bitmap(area.Width, area.Height);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (bmpScreenCapture != null)
            {
                p = Graphics.FromImage(bmpScreenCapture);
            }

            try
            {
                p.CopyFromScreen(area.X,
                     area.Y,
                     0, 0,
                     area.Size,
                     CopyPixelOperation.SourceCopy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (p != null)
                p.Dispose();

            return bmpScreenCapture;
        }

        public static Color GetAverageColor(Bitmap strip, Point origin, int width, int height)
        {
            List<Color> colors = new List<Color>();
            for (int y = origin.Y; y < origin.Y + height; y++)
            {
                for (int x = origin.X; x < origin.X + width; x++)
                {
                    colors.Add(strip.GetPixel(x, y));
                }
            }
            int red = 0;
            int green = 0;
            int blue = 0;
            foreach(Color color in colors)
            {
                red += color.R;
                green += color.G;
                blue += color.B;
            }
            red /= colors.Count;
            green /= colors.Count;
            blue /= colors.Count;
            Color average = Color.FromArgb(red, green, blue);
            return average;
        }
    }
}
