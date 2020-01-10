namespace ImageVerifier.MVAProxy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Holds information about an image size.
    /// Partial. Other code is auto-generated from the WebService wsdl.
    /// </summary>
    public partial class ImageSize : IProxyKey<Int32>
    {
        private static ProxyCache<Int32, ImageSize> cache = new ProxyCache<Int32, ImageSize>(
            new ProxyCache<Int32, ImageSize>.DataLoaderDelegate(CatalogServices.Catalog.GetImageSize),
                                                               Settings.GetCacheExpirationPeriod());
        /// <summary>
        /// Retirieves the list of all ImageSizes known to the system
        /// </summary>
        public static List<ImageSize> Get()
        {
            return cache.Get();
        }

        /// <summary>
        /// Retrieves the ImageSize information for an image size
        /// <summary>
        /// <param name="imageSizeId">The id of the image size to retrieve</param>
        /// <returns>The ImageSize object matching the id if one exists, otherwise null</returns>
        public static ImageSize Get(Int32 imageSizeId)
        {
            return cache.Get(imageSizeId);
        }

        /// <summary>
        /// Refreshes the ClientSide Cache
        /// </summary>
        public static void RefreshCache()
        {
            cache.Refresh();
        }
    }
}

