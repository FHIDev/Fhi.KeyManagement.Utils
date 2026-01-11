using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Fhi.Security.Cryptography.Certificate
{
    internal class JwkCertificateGeneratorService
    {
        private CertificateFiles GenerateCertificates(string commonName, string password)
        {
            using var rsa = RSA.Create(2048); // Or 4096?
            var request = new CertificateRequest(
                $"CN={commonName}",
                rsa,
                HashAlgorithmName.SHA256,   // Or 512?
                RSASignaturePadding.Pkcs1);

            // Evaluate certificate validity period!
            var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

            var privateKeyBytes = cert.Export(X509ContentType.Pfx, password);

            var publicKeyBytes = cert.Export(X509ContentType.Cert);
            var publicKeyBase64 = Convert.ToBase64String(publicKeyBytes, Base64FormattingOptions.InsertLineBreaks);
            var publicKey = $"-----BEGIN CERTIFICATE-----\n{publicKeyBase64}\n-----END CERTIFICATE-----";

            var thumbprint = cert.Thumbprint;
            Console.WriteLine($"Certificate thumbprint: {thumbprint}");

            return new CertificateFiles
            {
                CertificatePrivateKey = privateKeyBytes,
                CertificatePublicKey = publicKey,
                CertificateThumbprint = thumbprint
            };
        }
    }
}
