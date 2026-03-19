using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace Fhi.Security.Cryptography.Certificates
{
    internal sealed class PrivateKeyCertificateStoreConfigurationProvider(
        IReadOnlyList<CertificateSecretEntry> entries,
        ICertificateStore certificateStore,
        Action<CertificateLoadDiagnostic>? onValidationError = null) : ConfigurationProvider
    {
        public override void Load()
        {
            var utcNow = TimeProvider.System.GetUtcNow().UtcDateTime;

            foreach (var entry in entries)
            {
                try
                {
                    var cert = certificateStore.GetCertificate(entry.Identifier);
                    if (cert != null)
                    {
                        var validationReason = Validate(cert, entry.KeyUse, utcNow);
                        if (validationReason != null)
                        {
                            Diagnostic(entry, validationReason);
                            continue;
                        }

                        Data[entry.ConfigKey] = cert.GetRSAPrivateKey()?.ExportRSAPrivateKeyPem();
                    }
                    Diagnostic(entry, "Certificate not found in store.");

                }
                catch (Exception ex)
                {
                    Diagnostic(entry, $"Unexpected error: {ex.Message}");
                }
            }
        }

        private void Diagnostic(CertificateSecretEntry entry, string reason)
            => onValidationError?.Invoke(new CertificateLoadDiagnostic(entry.ConfigKey, entry.Identifier, reason));

        private static string? Validate(X509Certificate2 cert, CertificateKeyUse keyUse, DateTime utcNow)
        {
            if (!cert.HasPrivateKey && cert.GetRSAPrivateKey() == null)
                return "Certificate does not contain a supported private key algorithm (RSA expected).";
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
