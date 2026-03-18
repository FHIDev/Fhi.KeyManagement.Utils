using Microsoft.Extensions.Configuration;

namespace Fhi.Security.Cryptography.Certificates
{
    /// <summary>
    /// IConfigurationSource that loads private keys from a certificate store.
    /// Entries are added via <see cref="Add"/>.
    /// </summary>
    public class PrivateKeyCertificateStoreConfigurationSource(
        ICertificateStore certificateStore,
        Action<CertificateLoadDiagnostic>? onWarning = null) : IConfigurationSource
    {
        private readonly List<CertificateSecretEntry> _entries = new();

        /// <summary>Maps <paramref name="configKey"/> to a certificate by <paramref name="identifier"/>.</summary>
        public PrivateKeyCertificateStoreConfigurationSource Add(
            string configKey,
            string identifier,
            CertificateKeyUse keyUse = CertificateKeyUse.Signing)
        {
            _entries.Add(new(configKey, identifier, keyUse));
            return this;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
            => new PrivateKeyCertificateStoreConfigurationProvider(_entries.AsReadOnly(), certificateStore, onWarning);
    }
}
