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
    /// Provides methods for creating self-signed certificates with RSA key pairs.
    /// </summary>
    public static class Certificate
    {
        /// <summary>RSA key size in bits.</summary>
        public const int DefaultKeySize = 4096;

        /// <summary>Hash algorithm used for certificate signing.</summary>
        public static readonly HashAlgorithmName DefaultHashAlgorithm = HashAlgorithmName.SHA512;

        /// <summary>RSA signature padding mode.</summary>
        public static readonly RSASignaturePadding DefaultSignaturePadding = RSASignaturePadding.Pkcs1;

        /// <summary>
        /// Create a new asymmetric key pair in certificate format.
        /// </summary>
        /// <param name="commonName">Certificate common name</param>
        /// <param name="password">Password of the private key</param>
        /// <param name="validityYears">Number of years the certificate is valid</param>
        /// <param name="validityMonths">Additional months the certificate is valid</param>
        /// <returns>A certificate key pair containing private key, public key, and thumbprint</returns>
        public static CertificateKeyPair CreateAsymmetricKeyPair(
            string commonName,
            string password,
            int validityYears,
            int validityMonths)
        {
            using var rsa = RSA.Create(DefaultKeySize);
            var request = new CertificateRequest(
                $"CN={commonName}",
                rsa,
                DefaultHashAlgorithm,
                DefaultSignaturePadding);

            var notAfter = DateTimeOffset.Now.AddYears(validityYears).AddMonths(validityMonths);
            var cert = request.CreateSelfSigned(DateTimeOffset.Now, notAfter);

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
