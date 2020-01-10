using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using ImageDraw = System.Drawing.Image;
using System.Net;

namespace ImageVerifier.ImageManagement
{
    public class ImageUtil
    {

      public static ImageSet Diff(Bitmap src1, Bitmap src2, IEnumerable<Rectangle> regionsToExclude, double tolerance)
        {
            int width = (src1.Width > src2.Width) ? src1.Width : src2.Width;
            int height = (src1.Height > src2.Height) ? src1.Height : src2.Height;

            Bitmap diffBM = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Bitmap annotated1 = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Bitmap annotated2 = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            Graphics gfx1 = Graphics.FromImage(annotated1);
            Graphics gfx2 = Graphics.FromImage(annotated2);
            gfx1.DrawImage(src1, new Point(0, 0));
            gfx2.DrawImage(src2, new Point(0, 0));

            Graphics gfxDiff = Graphics.FromImage(diffBM);

            gfxDiff.DrawImage(src1, new Rectangle(0, 0, src1.Width, src1.Height));

            //Convert Image2 to negative
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
                new float[]{-1, 0,  0,  0,      0},
                new float[]{0,  -1, 0,  0,      0},
                new float[]{0,  0,  -1, 0,      0},
                new float[]{0,  0,  0,  0.5f,    0},
                new float[]{1,  1,  1,  0,      1}
            });
            ImageAttributes ia = new ImageAttributes();
            ia.SetColorMatrix(cm);

            //draw the negative of image2 ontop of image1 to create diff
            gfxDiff.DrawImage(src2, new Rectangle(0, 0, src2.Width, src2.Height)
                , 0, 0, src2.Width, src2.Height, GraphicsUnit.Pixel, ia);

            bool areDifferent = false;

            Color defColor = Color.White;
            int defColorValue = defColor.ToArgb();

            ReadOnlyBitmapData src1Data = new ReadOnlyBitmapData(src1);
            ReadOnlyBitmapData src2Data = new ReadOnlyBitmapData(src2);
            SolidBrush highlight = new SolidBrush(Color.FromArgb(20, Color.Yellow));

            Dictionary<Int32, Boolean> excludedPixels = new Dictionary<Int32, Boolean>();
            foreach (Rectangle excludedRectangle in regionsToExclude)
            {
                for (int x = excludedRectangle.X; x < excludedRectangle.X + excludedRectangle.Width; x++)
                {
                    for (int y = excludedRectangle.Y; y < excludedRectangle.Y + excludedRectangle.Height; y++)
                    {
                        int uniqueId = x << 0x10 | (y & 0xffff);
                        excludedPixels[uniqueId] = true;
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int uniqueId = x << 0x10 | (y & 0xffff);
                    if (!excludedPixels.ContainsKey(uniqueId))
                    {
                        int pt1Raw = src1Data.GetRawARGBValue(x, y, defColorValue);
                        int pt2Raw = src2Data.GetRawARGBValue(x, y, defColorValue);
                    //    Console.WriteLine("Comparing"+(pt1Raw)+"and"+(pt2Raw));
                        if (pt1Raw != pt2Raw)
                        {
                            Color pt1 = Color.FromArgb(pt1Raw);
                            Color pt2 = Color.FromArgb(pt2Raw);
                            double colorDifference = Math.Sqrt(Math.Pow(pt2.R - pt1.R, 2) + Math.Pow(pt2.G - pt1.G, 2) + Math.Pow(pt2.B - pt1.B, 2));
                            if (colorDifference > tolerance)
                            {
                                areDifferent = true;
                                gfx1.FillRectangle(highlight, new Rectangle(x - 1, y - 1, 3, 3));
                                gfx2.FillRectangle(highlight, new Rectangle(x - 1, y - 1, 3, 3));
                            }
                        }
                    }
                }
            }
            return new ImageSet(diffBM, annotated1, annotated2, areDifferent);
        }

      public static bool DiffImages(System.Drawing.Image sourceIm, System.Drawing.Image targetIm)
      {
          ImageDraw sourceScreenshotRaw = sourceIm;
          ImageDraw targetScreenshotRaw = targetIm;
          Bitmap sourceImage = new Bitmap(sourceScreenshotRaw.Width, sourceScreenshotRaw.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
          using (Graphics g = Graphics.FromImage(sourceImage))
          {
              g.DrawImage(sourceScreenshotRaw, 0, 0);
          }
          Bitmap targetImage = new Bitmap(targetScreenshotRaw.Width, targetScreenshotRaw.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
          using (Graphics g = Graphics.FromImage(targetImage))
          {
              g.DrawImage(targetScreenshotRaw, 0, 0);
          }
          ImageSet result = Diff(sourceImage, targetImage, new Rectangle[0], 50);
          if (result.AreDifferent)
          {
              return false;
          }
          return true;
      }


      /// <summary>
      /// Initializes a new instance of the System.Net.WebClient class. Downloads Images in to mamaory based on fileURL.
      /// </summary>
      /// <param name="url">Represents fileUrl of imageInastance</param>
      /// 
      public static System.Drawing.Image DownloadImage(string url)
      {
          WebClient wc = new WebClient();
          byte[] bytes = wc.DownloadData(url);
          MemoryStream ms = new MemoryStream(bytes);
          return System.Drawing.Image.FromStream(ms);
      }
        
    }
}
