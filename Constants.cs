using System;
using System.Data;
using System.Diagnostics;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using ImageVerifier.MVAProxy;
using System.Data.SqlTypes;

/// <summary>
/// Summary description for Constants
/// </summary> destFileStream.Close();
public class Constants
{
    
    private static CatalogServices _proxy = null;
    private static Object _proxyLockObject = new Object();

    public static CatalogServices Proxy
    {
        get
        {
            if (_proxy == null)
            {
                lock (_proxyLockObject)
                {
                    if (_proxy == null)
                    {
                        _proxy = new CatalogServices();
                        //_proxy.Url = System.Configuration.ConfigurationManager.AppSettings["CatalogWebServiceUrl"];
                        _proxy.Url = @"http://vsvaltstweb01/EmsMetaDataTools/CatalogServices.asmx";
                        //string webProxy =@"http://itgproxy";
                        //string webProxy = ConfigurationManager.AppSettings["WebProxy"];
                        //if (!string.IsNullOrEmpty(webProxy))
                        //{
                        //    _proxy.Proxy = new System.Net.WebProxy(webProxy);
                        //}

                        _proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                        //String certNameToFind = ConfigurationManager.AppSettings["CatalogWebServiceCertName"];
                        String certNameToFind = "int2.CatSvcWEB.rdw.001";
                        X509Store store = new X509Store("My", StoreLocation.LocalMachine);
                        try
                        {
                            store.Open(OpenFlags.ReadOnly);
                            X509Certificate2Collection certs = store.Certificates;
                            foreach (X509Certificate2 cert in certs)
                            {
                                String[] subjectAttributes = cert.Subject.Split(',');
                                String name = null;
                                foreach (String subjectAttribute in subjectAttributes)
                                {
                                    if (subjectAttribute.Trim().StartsWith("CN="))
                                    {
                                        name = subjectAttribute.Trim().Substring(3);
                                        if (name == certNameToFind)
                                        {
                                            _proxy.ClientCertificates.Add(cert);
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            store.Close();
                        }
                        CatalogServices.Catalog = _proxy;
                    }
                }
            }
            return _proxy;
        }
    }
}

  
