using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace AppBIDV
{
    public class CertificateValidationService
    {
        public bool ValidateCertificate(X509Certificate2 clientCertificate)
        {
            var cert = new X509Certificate2(Path.Combine(Directory.GetCurrentDirectory(), "Keys\\cert.pem"));
            if(clientCertificate.Thumbprint == cert.Thumbprint)
            {
                return true;
            }
            return false;
        }
    }
}