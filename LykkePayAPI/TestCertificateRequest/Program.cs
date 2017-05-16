using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace TestCertificateRequest
{
    class Program
    {
        static void Main(string[] args)
        {
            #region old
            ////  Note: Requires Chilkat v9.5.0.65 or greater.

            ////  This requires the Chilkat API to have been previously unlocked.
            ////  See Global Unlock Sample for sample code.

            ////  First generate an RSA private key.
            //Chilkat.Rsa rsa = new Chilkat.Rsa();

            ////  Generate a random 2048-bit RSA key.
            //bool success = rsa.GenerateKey(2048);
            //if (success != true)
            //{
            //    Console.WriteLine(rsa.LastErrorText);
            //    return;
            //}

            ////  Get the private key
            //Chilkat.PrivateKey privKey = rsa.ExportPrivateKeyObj();

            ////  Create the CSR object and set properties.
            //Chilkat.Csr csr = new Chilkat.Csr();

            ////  Specify the Common Name.  This is the only required property.
            ////  For SSL/TLS certificates, this would be the domain name.
            ////  For email certificates this would be the email address.
            //csr.CommonName = "mysubdomain.mydomain.com";

            ////  Country Name (2 letter code)
            //csr.Country = "CH";

            ////  State or Province Name (full name)
            //csr.State = "Geneva";

            ////  Locality Name (eg, city)
            //csr.Locality = "Geneva";

            ////  Organization Name (eg, company)
            //csr.Company = "Lykke Ltd";

            ////  Organizational Unit Name (eg, secion/division)
            //csr.CompanyDivision = "IT";

            ////  Email address
            //csr.EmailAddress = "support@mydomain.com";

            ////  Create the CSR using the private key.
            //string pemStr = csr.GenCsrPem(privKey);
            //if (csr.LastMethodSuccess != true)
            //{
            //    Console.WriteLine(csr.LastErrorText);

            //    return;
            //}

            ////  Save the private key and CSR to a files.
            //privKey.SavePkcs8EncryptedPemFile("password", "qa_output/privKey1.pem");

            //Chilkat.FileAccess fac = new Chilkat.FileAccess();
            //fac.WriteEntireTextFile("qa_output/csr1.pem", pemStr, "utf-8", false);

            ////  Show the CSR.
            //Console.WriteLine(pemStr);

            #endregion

            X509Certificate2
            using (RSA rsa = cert.GetRSAPublicKey())
            {
                DoStuffWithThePublicKey(rsa);
            }
        }
    }
}