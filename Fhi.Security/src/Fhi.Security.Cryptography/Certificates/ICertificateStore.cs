using System.Security.Cryptography.X509Certificates;

namespace Fhi.Security.Cryptography.Certificates
{
    /// <summary>
    /// Abstraction over a certificate source (Windows certificate store, file system, etc.).
    /// </summary>
    public interface ICertificateStore
    {
        /// <summary>
        /// Returns the certificate for the given identifier, or null if not found.
        /// The caller is responsible for disposing the returned certificate.
        /// </summary>
        X509Certificate2? GetCertificate(string identifier);
    }

    /// <summary>
    /// Intended use of the private key extracted from the certificate.
    /// </summary>
    public enum CertificateKeyUse
    {
        /// <summary>Key is used for signing (e.g. client assertion JWTs). Requires DigitalSignature KeyUsage.</summary>
        Signing,
        /// <summary>Key is used for decrypting data at rest or in transit. Requires KeyEncipherment KeyUsage.</summary>
        Encryption
    }

    /// <summary>
    /// Describes why a certificate entry was skipped during configuration load.
    /// Passed to the <c>onValidationError</c> callback on <see cref="Microsoft.Extensions.Configuration.CertificateConfigurationExtensions"/>.
    /// </summary>
    public record CertificateLoadDiagnostic(string ConfigKey, string Identifier, string Reason);

    internal record CertificateSecretEntry(
        string ConfigKey,
        string Identifier,
        CertificateKeyUse KeyUse);
}
