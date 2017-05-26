using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading.Tasks;

namespace MVCFrontend.Helpers
{
    static class Security
    {
        public static X509Certificate2 GetCertificateFromStore(string name)
        {
            X509Certificate2 result = null;
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            try
            {
                var certCollection = store.Certificates;

                foreach (var cert in certCollection)
                {
                    //Console.WriteLine("cert name: {0}", cert.Subject);
                    if (cert.Subject.Contains(name))
                    {
                        result = cert;
                        break;
                    }
                }
            }
            finally
            {
                store.Close();
            }
            if (result == null) throw new Exception("Unable to get certificate from store containing " + name);
            return result;
        }

        public static X509Certificate2 GetCertificateFromFile()
        {
            X509Certificate2 result = null;
            var certificatePath = string.Empty;
            certificatePath = Appsettings.OnAzure()
                ? Path.Combine(Environment.GetEnvironmentVariable("HOME"), "site\\wwwroot\\App_Data\\aws_certificate.cer")
                : Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~\\App_Data"),"aws_certificate.cer");

            X509Certificate cert = X509Certificate.CreateFromCertFile(certificatePath);
            result = new X509Certificate2(cert);
            return result;
        }


    }
}
