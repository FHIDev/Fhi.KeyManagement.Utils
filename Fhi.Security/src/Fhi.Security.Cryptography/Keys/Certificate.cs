using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace Fhi.Security.Cryptography.Certificates
{
    /// <summary>
    /// Represents a pair of cryptographic keys and the associated certificate thumbprint.
    /// </summary>
    public record CertificateKeyPair(
        ImmutableArray<byte> CertificatePrivateKey,
        string CertificatePublicKey,
        string CertificateThumbprint
    );

    /// <summary>
    /// 
    /// </summary>
    public static class Certificate
    {
        /// <summary>
        /// Create a new asymmetric key pair in certificate format.
        /// </summary>
        /// <param name="commonName">Certificate common name</param>
        /// <param name="password">Password of the private key</param>
        /// <returns></returns>
        public static CertificateKeyPair CreateAsymmetricKeyPair(
            string commonName,
            string password)
        {
            return GenerateRSACertificate(commonName, password);
        }
        private static CertificateKeyPair GenerateRSACertificate(string commonName, string password)
        {
            using var rsa = RSA.Create(4096);
            var request = new CertificateRequest(
                $"CN={commonName}",
                rsa,
                HashAlgorithmName.SHA512,
                RSASignaturePadding.Pkcs1);

            // Evaluate certificate validity period!
            var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

            var privateKeyBytes = cert.Export(X509ContentType.Pfx, password);

            var publicKeyBytes = cert.Export(X509ContentType.Cert);
            var publicKeyBase64 = Convert.ToBase64String(publicKeyBytes, Base64FormattingOptions.InsertLineBreaks);
            var publicKey = $"-----BEGIN CERTIFICATE-----\n{publicKeyBase64}\n-----END CERTIFICATE-----";

            var thumbprint = cert.Thumbprint;
            Console.WriteLine($"Certificate thumbprint: {thumbprint}");

            return new CertificateKeyPair(privateKeyBytes.ToImmutableArray(), publicKey, thumbprint);
        }
    }
}
