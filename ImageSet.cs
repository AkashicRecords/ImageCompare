using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImageVerifier.ImageManagement
{
    public class ImageSet : IDisposable
    {
        public Image Diff;
        public Image Image1Annotated;
        public Image Image2Annotated;
        public bool AreDifferent;
        public ImageSet(Image diff, Image image1diff, Image image2diff, bool areDifferent)
        {
            Diff = diff;
            Image1Annotated = image1diff;
            Image2Annotated = image2diff;
            AreDifferent = areDifferent;
        }

        public void Dispose()
        {
            if (Diff != null)
            {
                Diff.Dispose();
            }
            if (Image1Annotated != null)
            {
                Image1Annotated.Dispose();
            }
            if (Image2Annotated != null)
            {
                Image2Annotated.Dispose();
            }
        }
    }
}
