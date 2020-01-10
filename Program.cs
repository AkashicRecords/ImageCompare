using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using ImageDraw = System.Drawing.Image;
using System.Threading;
using ImageVerifier.MVAProxy;
using System.Net;
using ImageVerifier.ImageManagement;
using System.Web;
using System.Collections;


namespace ImageVerifier
{
    class Program
    {
        /// <summary>
        /// Main entry point for ImageVerifier application
        /// </summary>
        /// <param name="args">Represents path to tab delimited file or a single ImageID GUID</param>
        /// **TBD** Needs to be refactored to several classes. Option to compare images for a single imageId
        /// 
        static void Main(string[] args)
        {
            List<string> imageIDs = new List<string>();
            string line = null;
            //using (StreamReader file = new StreamReader(@"C:\Users\V-saeld\Desktop\Image\ImageID.txt"))
            using (StreamReader file = new StreamReader(@"..\..\999.txt"))
            {
                while ((line = file.ReadLine()) != null)
                {
                    char[] delimiters = new char[] { '\t' };
                    string[] parts = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < parts.Length; i++)
                    {
                        imageIDs.Add(parts[i]);
                    }
                }
                file.Close();
            }
            string results = @"C:\Users\v-saeld\Desktop\results.log";
            using (StreamWriter logWriter = new StreamWriter(results))
            {
                foreach (string imageID in imageIDs)
                {
                    logWriter.WriteLine("ImageID: {0}", imageID);
                    Console.WriteLine("ImageID: {0}", imageID);
                    CatalogServices cs = Constants.Proxy;
                    ImageSearchRequest imageSearchRequest = new ImageSearchRequest();
                    List<ImageVerifier.ImInstDisplay> list = new List<ImageVerifier.ImInstDisplay>();
                    Dictionary<Guid, string> dictUrl = new Dictionary<Guid, string>();
                    ImageVerifier.MVAProxy.Image image = null;
                    ImageVerifier.ImInstDisplay im = new ImageVerifier.ImInstDisplay();
                    im.ImageID = new Guid(imageID);
                    image = cs.GetImage(im.ImageID);
                    int OriginalImageSizeID = 0;
                    int ThumbnailImageSizeID = 4;
                    List<ImageVerifier.MVAProxy.ImageSize> imageSizes = ImageVerifier.MVAProxy.ImageSize.Get();
                    ImageVerifier.ImInstDisplay imInstDisplay = new ImageVerifier.ImInstDisplay();
                    int originalImageFileWidth = 0, originalImageFileHeight = 0, originalImageFileSize = 0;
                    string originalImageFileExtension = string.Empty;
                    bool originalFileAvailable = false;
                    bool originalFileinSanAvailable = true;
                    foreach (ImageInstance iminst in image.Instances)
                    {
                        if (iminst.ImageSizeId == OriginalImageSizeID)
                        {
                            im.OriginalFileGuid = iminst.Id;
                            ImageFileHandler.GetImageFileProperty(iminst.Id, out originalImageFileWidth, out originalImageFileHeight, out originalImageFileSize, out  originalImageFileExtension);
                            if (originalImageFileWidth == 0 && originalImageFileHeight == 0 && originalImageFileSize == 0)
                            {
                                Console.WriteLine("ImageID: {0} - There is no image in San folder-{1}.", imageID, originalImageFileExtension);
                                logWriter.WriteLine("ImageID: {0} - There is no image in San folder-{1}.", imageID, originalImageFileExtension);
                                originalFileinSanAvailable = false;
                                break;
                            }
                            else
                            {
                                originalFileAvailable = true;
                                continue;
                            }
                        }
                        else if (iminst.ImageSizeId == ThumbnailImageSizeID)
                        {
                            continue;
                        }
                        else
                        {
                            imInstDisplay.LiveURL = iminst.FileUrl;
                            dictUrl.Add(iminst.Id, imInstDisplay.LiveURL.ToString());
                        }
                    }
                    Hashtable propImageList = new Hashtable();
                    foreach (ImageInstance iminst in image.Instances)
                    {
                        if (!originalFileinSanAvailable)
                        { break; }
                        string sourceImageFileExtension = string.Empty;
                        int sourceImageFileWidth = 0, sourceImageFileHeight = 0, sourceImageFileSize = 0;
                        int thumbNailImageSizeID = 4;
                        if (!originalFileAvailable)
                        {
                            logWriter.WriteLine("ImageID: {0}-There is no original image.", imageID);
                            Console.WriteLine("ImageID: {0}-There is no original image.", imageID);
                            break;
                        }
                            ImageFileHandler.GetImageFileProperty(iminst.Id, out sourceImageFileWidth, out sourceImageFileHeight, out sourceImageFileSize, out sourceImageFileExtension);
                            if (iminst.ImageSizeId == OriginalImageSizeID || iminst.ImageSizeId == thumbNailImageSizeID)
                            { continue; }
                            ImageSize imSize1 = new ImageSize();
                            imSize1.Id = iminst.ImageSizeId;
                            imSize1.Width = sourceImageFileWidth;
                            imSize1.Height = sourceImageFileHeight;
                            propImageList.Add(iminst.Id, imSize1);
                    }
                    if (originalFileAvailable && originalFileinSanAvailable)
                    {
                        Dictionary<Guid, System.Drawing.Image> dictDestImages = ImageFileHandler.PropImage(image, propImageList, im.OriginalFileGuid);
                        Dictionary<Guid, System.Drawing.Image> dictImages = new Dictionary<Guid, System.Drawing.Image>();
                        foreach (var pair in dictUrl)
                        {
                            System.Drawing.Image ima = ImageUtil.DownloadImage(pair.Value);
                            dictImages.Add(pair.Key, ima);
                        }
                        foreach (KeyValuePair<Guid, System.Drawing.Image> Origkvp in dictImages)
                        {
                            foreach (KeyValuePair<Guid, System.Drawing.Image> kvp in dictDestImages)
                            {
                                if (Origkvp.Key == kvp.Key)
                                {
                                    bool IsImageSame = ImageUtil.DiffImages(kvp.Value, Origkvp.Value);
                                    if (!IsImageSame)
                                    {
                                        logWriter.WriteLine("InstanceID: {0} Different image.", kvp.Key);
                                        Console.WriteLine("InstanceID: {0} Different image.", kvp.Key);
                                    }
                                    else
                                    {
                                        logWriter.WriteLine("InstanceID: {0} Same image.", kvp.Key);
                                        Console.WriteLine("InstanceID: {0} Same image.", kvp.Key);
                                    }
                                }
                            }
                        }
                    }
                }
                Console.ReadLine();
            }
        }
       
    }
}
