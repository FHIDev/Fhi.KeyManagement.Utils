using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Fhi.Security.Cryptography.Certificates
{
    internal sealed class PrivateKeyCertificateStoreConfigurationProvider(
        IReadOnlyList<CertificateSecretEntry> entries,
        ICertificateStore certificateStore,
        Action<CertificateLoadDiagnostic>? onWarning = null) : ConfigurationProvider
    {
        public override void Load()
        {
            var utcNow = TimeProvider.System.GetUtcNow().UtcDateTime;

            foreach (var entry in entries)
            {
                try
                {
                    var cert = certificateStore.GetCertificate(entry.Identifier);

                    if (cert == null)
                    {
                        Warn(entry, "Certificate not found in store.");
                        continue;
                    }

                    if (!cert.HasPrivateKey)
                    {
                        Warn(entry, "Certificate does not have a private key.");
                        continue;
                    }

                    var validationReason = GetValidationFailureReason(cert, entry.KeyUse, utcNow);
                    if (validationReason != null)
                    {
                        Warn(entry, validationReason);
                        continue;
                    }

                    var privateKey = cert.GetRSAPrivateKey()?.ExportRSAPrivateKeyPem();
                    if (privateKey == null)
                    {
                        Warn(entry, "Certificate does not contain a supported private key algorithm (RSA expected).");
                        continue;
                    }

                    Data[entry.ConfigKey] = privateKey;
                }
                catch (Exception ex)
                {
                    Warn(entry, $"Unexpected error: {ex.Message}");
                }
            }
        }

        private void Warn(CertificateSecretEntry entry, string reason)
            => onWarning?.Invoke(new CertificateLoadDiagnostic(entry.ConfigKey, entry.Identifier, reason));

        private static string? GetValidationFailureReason(X509Certificate2 cert, CertificateKeyUse keyUse, DateTime utcNow)
        {
            if (cert.NotAfter.ToUniversalTime() < utcNow)
                return $"Certificate expired on {cert.NotAfter:yyyy-MM-dd}.";

            if (cert.NotBefore.ToUniversalTime() > utcNow)
                return $"Certificate is not valid until {cert.NotBefore:yyyy-MM-dd}.";

            var keyUsageExt = cert.Extensions.OfType<X509KeyUsageExtension>().FirstOrDefault();
            if (keyUsageExt != null)
            {
                var required = keyUse == CertificateKeyUse.Signing
                    ? X509KeyUsageFlags.DigitalSignature
                    : X509KeyUsageFlags.KeyEncipherment;

                if (!keyUsageExt.KeyUsages.HasFlag(required))
                    return $"Certificate is missing required KeyUsage: {required}.";
            }

            return null;
        }
    }
}
