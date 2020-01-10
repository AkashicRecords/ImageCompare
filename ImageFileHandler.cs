using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ImageVerifier.MVAProxy;
using System.Configuration;

namespace ImageVerifier.MVAProxy
{
    class ImageFileHandler
    {

        #region Private fields
        /// <summary>
        /// the following 6 private fields are used to store settings from 
        /// web.config to define the behavor of ImageFileHandler class.
        /// </summary>
        private static int jpgCompressionLevel = 90;
        private static string limeLightURLPath = string.Empty;
        private static string tempImageFolder = string.Empty;
        private static string[] sanImageFolders = null;
        private static string tempStorageURLPrefix = string.Empty;
        private static int aspectRatioWarningPercentage = 3;
        private static string limeLightURLPrefix = string.Empty;

        /// <summary>
        /// The size id to use for the orignal image
        /// AJ: this should not be set-able!
        /// </summary>
        private static int originalImageSizeID;
        private static readonly Int32 customImageSizeId = -1;
        private static readonly Int32 thumbnailImageSizeId = 4;
        private static readonly Int32 podcastImageSizeId = 11;

        #endregion
        #region Properties
        public static int AspectRatioWarningPercentage
        {
            get { return ImageFileHandler.aspectRatioWarningPercentage; }
            set { ImageFileHandler.aspectRatioWarningPercentage = value; }
        }
        public static Int32 CustomImageSizeId
        {
            get { return customImageSizeId; }
        }

        public static Int32 ThumbnailImageSizeId
        {
            get { return thumbnailImageSizeId; }
        }

        public static Int32 PodcastImageSizeId
        {
            get { return podcastImageSizeId; }
        }

        public static int OriginalImageSizeID
        {
            get { return ImageFileHandler.originalImageSizeID; }
            set { ImageFileHandler.originalImageSizeID = value; }
        }
        public static string TempStorageURLPrefix
        {
            get { return ImageFileHandler.tempStorageURLPrefix; }
            set { ImageFileHandler.tempStorageURLPrefix = value; }
        }
        public static string LimeLightURLPrefix
        {
            get { return ImageFileHandler.limeLightURLPrefix; }
            set { ImageFileHandler.limeLightURLPrefix = value; }
        }



        public static string[] SanImageFolders
        {
            get
            {
                return sanImageFolders;
            }
            set
            {
                sanImageFolders = value;
            }
        }

        public static string TempImageFolder
        {
            get
            {
                return tempImageFolder;
            }
            set
            {
                tempImageFolder = value;
            }
        }

        public static int JpgCompressionLevel
        {
            get
            {
                return jpgCompressionLevel;
            }
            set
            {
                jpgCompressionLevel = value;
            }
        }
        #endregion
        #region Public Method
        /// <summary>
        /// LimeLightURLPath used to hold image content.
        /// </summary>
        public static string LimeLightURLPath
        {
            get
            {
                return ImageFileHandler.limeLightURLPath;
            }
            set
            {
                ImageFileHandler.limeLightURLPath = value;
            }
        }
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImageFileHandler()
        {

        }

 
           public static void GetImageFileProperty(Guid ImageInstanceID, out int width, out int height, out int fileSize, out string fileExtension)
        {
           // string[] ImageInstanceFileName = new string[];

            SanImageFolders = new string[] { @"\\vidlabfile01\IMAGES", @"\\i\DRMW\IMAGES" };
            string originalSizeImageFolderPath = SanImageFolders[0] + @"\" + ImageInstanceID.ToString().Substring(ImageInstanceID.ToString().Length - 2, 2) + @"\";
            //string originalSizeImageFolderPath = SanImageFolders[0] + ImageFileHandler.limeLightURLPath.Trim("/".ToCharArray()) + @"//" + ImageInstanceID.ToString().Substring(ImageInstanceID.ToString().Length - 2, 2) + @"\";
            try
            {
             string[] InstanceFileName = Directory.GetFiles(originalSizeImageFolderPath, "*" + ImageInstanceID.ToString() + "*");
             //foreach (string file in InstanceFileName)
             //    Console.WriteLine(Path.GetFileName(file));
             if (InstanceFileName.Length!=0)
             {
                 string ImageInstanceFileName = Directory.GetFiles(originalSizeImageFolderPath, "*" + ImageInstanceID.ToString() + "*")[0]; 
            FileInfo fileInfo = new FileInfo(ImageInstanceFileName);
            System.Drawing.Image originalImage = System.Drawing.Image.FromFile(ImageInstanceFileName);
            fileExtension = fileInfo.Extension;
            fileSize = (int)fileInfo.Length;
            width = originalImage.Width;
            height = originalImage.Height;
            originalImage.Dispose();
            }
            else
            {
                fileExtension = Path.Combine(originalSizeImageFolderPath, "*" + ImageInstanceID.ToString() + "*");
                fileSize = 0;
                width = 0;
                height = 0;
               
            }
            }
            catch (Exception ex)
            {
                throw new IOException(ex.Message);
            }
          
        }
        /// <summary>
        /// Prop images to lime light network
        /// </summary>
        /// <param name="image">image object to be proped</param>
        /// <param name="sizeTable">Hashtable,the key is the guid of the image instance, and the element 
        /// is ImageSize object which contains the size ID and the width& height the image to be resized to </param>
        /// <param name="originalSizeImageInstanceID">The image instance that contains the original image to be used
        /// as a source for resizing operation</param>
        public static Dictionary<Guid, System.Drawing.Image> PropImage(ImageVerifier.MVAProxy.Image image, Hashtable sizeTable, Guid originalSizeImageInstanceID)
           {
            string ImageInstanceFileName = string.Empty;
            Dictionary<Guid, System.Drawing.Image> dictDestImages = new Dictionary<Guid, System.Drawing.Image>();
            SanImageFolders = new string[] { @"\\vidlabfile01\IMAGES", @"\\i\DRMW\IMAGES" };
            string originalSizeImageFolderPath = SanImageFolders[0] + @"\" + originalSizeImageInstanceID.ToString().Substring(originalSizeImageInstanceID.ToString().Length - 2, 2) + @"\";

            string OriginalImageFileName;
            System.Drawing.Image originalImage;
            System.Drawing.Image destImage;
           // System.Drawing.Image cloneImage;
            string originalImageFileExtension;
            string destImageFileFullPath;
            MemoryStream destFileStream;

            try
            {
                OriginalImageFileName = Directory.GetFiles(originalSizeImageFolderPath, "*" + originalSizeImageInstanceID.ToString() + "*")[0];
            }
            catch
            {
                throw new IOException("Original file with ID: " + originalSizeImageInstanceID.ToString() + " does not exist.");
            }
            originalImageFileExtension = new FileInfo(OriginalImageFileName).Extension;
            originalImage = System.Drawing.Image.FromFile(OriginalImageFileName);
                     

            try
            {
                #region resize
                foreach (Guid imInstID in sizeTable.Keys)
                {
                    bool foundmatch = false;
                    MVAProxy.ImageInstance currentImInst = null;
                    foreach (MVAProxy.ImageInstance imInst in image.Instances)
                    {
                        if (imInst.Id == imInstID)
                        {
                            currentImInst = imInst;
                            foundmatch = true;
                            break;
                        }
                    }

                    if (!foundmatch)
                        throw new ArgumentException("Key: " + imInstID.ToString() + " in size table not in image instances of the image");
                    destImageFileFullPath = tempImageFolder + @"\" + imInstID.ToString().Substring(imInstID.ToString().Length - 2, 2) + @"\" + imInstID + originalImageFileExtension;

                    if (originalImage.Width == ((ImageSize)(sizeTable[imInstID])).Width && originalImage.Height == ((ImageSize)(sizeTable[imInstID])).Height)
                    {
                        destFileStream = new MemoryStream(File.ReadAllBytes(OriginalImageFileName));
                    }
                    else
                    {
                        if (originalImage.PixelFormat == PixelFormat.Indexed
                            || originalImage.PixelFormat == PixelFormat.Format8bppIndexed
                            || originalImage.PixelFormat == PixelFormat.Format4bppIndexed
                            || originalImage.PixelFormat == PixelFormat.Format1bppIndexed
                            )
                        {
                            destImage = new Bitmap(((ImageSize)(sizeTable[imInstID])).Width, ((ImageSize)(sizeTable[imInstID])).Height, PixelFormat.Format24bppRgb);
                        }
                        else
                        {
                            destImage = new Bitmap(((ImageSize)(sizeTable[imInstID])).Width, ((ImageSize)(sizeTable[imInstID])).Height, originalImage.PixelFormat);
                        }
                        Graphics graphic = Graphics.FromImage(destImage);
                        graphic.CompositingQuality = CompositingQuality.HighQuality;
                        graphic.SmoothingMode = SmoothingMode.HighQuality;
                        graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphic.DrawImage(originalImage,
                            new Rectangle(0, 0, ((ImageSize)(sizeTable[imInstID])).Width, ((ImageSize)(sizeTable[imInstID])).Height)
                            );
                        try
                        {
                            destFileStream = new MemoryStream();

                            if (originalImageFileExtension.ToLower().EndsWith("jpg") || originalImageFileExtension.ToLower().EndsWith("jpeg"))
                            {
                                System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                                EncoderParameter ratio = new EncoderParameter(qualityEncoder, jpgCompressionLevel);
                                // Add the quality parameter to the list
                                EncoderParameters codecParams = new EncoderParameters(1);
                                codecParams.Param[0] = ratio;
                                ImageCodecInfo codecInfo = GetEncoderInfo("image/jpeg");
                                destImage.Save(destFileStream, codecInfo, codecParams);
                                destImage.Dispose();
                                dictDestImages.Add(imInstID, System.Drawing.Image.FromStream(destFileStream));
                                destFileStream.Close();
                                
                            }
                            else if (originalImageFileExtension.ToLower().EndsWith("png"))
                            {

                                dictDestImages.Add(imInstID, System.Drawing.Image.FromStream(destFileStream));
                                destImage.Dispose();
                                destFileStream.Close();
                          
                            }
                            else
                            {
                                destImage.Dispose();
                                destFileStream.Close();
                                throw new ArgumentException("The original Image's format: " + originalImageFileExtension + " is not supported.");
                               
                            }
                        }
                        catch (IOException ex)
                        {
                            
                            throw new IOException("IO error while writing image with ID: " + originalSizeImageInstanceID.ToString() + ".", ex);
                        }

                    }
                   // destImage.Dispose();
                    destFileStream.Close();
                    
                }
                
                #endregion
            }
            catch (Exception)
            {
                if (originalImage != null)
                    originalImage.Dispose();
              
                throw;
            }
           //  destFileStream.Close();
           
            originalImage.Dispose();
            return dictDestImages;
        }
        #endregion
        #region Private Methods
           
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; j++)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            } return null;
        }
        #endregion
    }
}