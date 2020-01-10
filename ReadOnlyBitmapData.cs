using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageVerifier.ImageManagement
{
    public unsafe class ReadOnlyBitmapData
    {
        private Int32[] imageData;
        private int width;
        private int height;

        public ReadOnlyBitmapData(Bitmap b)
        {
            Bitmap toDispose = null;
            try
            {
                if (b.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    Bitmap input = b;
                    b = new Bitmap(b.Width, b.Height, PixelFormat.Format32bppArgb);
                    toDispose = b;
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        g.DrawImage(input, 0, 0);
                    }
                }

                width = b.Width;
                height = b.Height;

                BitmapData lockData = b.LockBits(
                    new Rectangle(0, 0, width, height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                // Create an array to store image data
                imageData = new Int32[width * height];

                // Use the Marshal class to copy image data
                System.Runtime.InteropServices.Marshal.Copy(
                    lockData.Scan0, imageData, 0, imageData.Length);

                b.UnlockBits(lockData);
            }
            finally
            {
                if (toDispose != null)
                {
                    toDispose.Dispose();
                }
            }
        }

        public Color GetPixel(int x, int y)
        {
            int pixelValue = imageData[y * width + x];
            return Color.FromArgb(pixelValue);
        }

        public Color GetPixel(int x, int y, Color defColor)
        {
            if (x >= width || y >= height)
            {
                return defColor;
            }
            int pixelValue = imageData[y * width + x];
            return Color.FromArgb(pixelValue);
        }

        public int GetRawARGBValue(int x, int y, int defValue)
        {
            if (x >= width || y >= height)
            {
                return defValue;
            }
            return imageData[y * width + x];
        }
    }
}
