namespace ImageVerifier.MVAProxy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;

    /// <summary>
    /// Containd the default settings for the application
    /// </summary>
    public class Settings
    {
        private static NameValueCollection settings;

        /// <summary>
        /// If no cache expiration period can be locaed or parsed, the defult of 15mins is used.
        /// </summary>
        public static readonly TimeSpan DefaultCacheExpirationPeriod = new TimeSpan(0, 15, 0);

        //static Settings()
        //{
        //    try
        //    {
        //        settings = ConfigurationManager.AppSettings;
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        /// <summary>
        /// Returns the cache expiration period. If a configured value could not be localted
        /// the default value is used.
        /// </summary>
        public static TimeSpan GetCacheExpirationPeriod()
        {
            if (settings != null)
            {
                try
                {
                    if (settings["CacheExpirationPeriod"] != null)
                        return TimeSpan.Parse(settings["CacheExpirationPeriod"]);
                }
                catch (Exception)
                {
                }
            }

            return DefaultCacheExpirationPeriod;
        }
    }
}
