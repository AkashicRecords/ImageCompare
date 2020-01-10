namespace ImageVerifier.MVAProxy
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;


    public partial class CatalogServices
    {
        public static CatalogServices Catalog;

        static CatalogServices()
        {
            Catalog = new CatalogServices();
        }
    }
}