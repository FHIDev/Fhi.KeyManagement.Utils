using System.Security.Cryptography.X509Certificates;
using Fhi.Security.Cryptography.Certificates;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extension methods for loading private keys from a certificate store into IConfiguration.
    /// </summary>
    public static class CertificateConfigurationExtensions
    {
        /// <summary>
        /// Reads a private key from the Windows certificate store and injects it as a PEM string
        /// at <paramref name="configKey"/> in IConfiguration.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configKey">name of config parameter</param>
        /// <param name="thumbprint">Certificate thumbprint</param>
        /// <param name="keyUse">Intended use of private key</param>
        /// <param name="storeLocation">Certificate location</param>
        /// <param name="storeName">The Certificate store to lead the certificate from</param>
        /// <param name="certStore">To implement another certificate store</param>
        /// <param name="onValidationError">
        /// Optional callback invoked when a certificate is skipped (not found, expired, missing private key, etc.).
        /// Use this to connect to your application's logger:
        /// <code>onWarning: d => logger.LogWarning("Certificate skipped [{Key}] {Id}: {Reason}", d.ConfigKey, d.Identifier, d.Reason)</code>
        /// </param>
        public static IConfigurationBuilder AddPrivateKeyFromCertificateStore(
            this IConfigurationBuilder builder,
            string configKey,
            string thumbprint,
            CertificateKeyUse keyUse = CertificateKeyUse.Signing,
            StoreLocation storeLocation = StoreLocation.CurrentUser,
            StoreName storeName = StoreName.My,
            ICertificateStore? certStore = null,
            Action<CertificateLoadDiagnostic>? onValidationError = null)
        {
            var store = certStore ?? new WindowsCertificateStore(storeName, storeLocation);
            var source = new PrivateKeyCertificateStoreConfigurationSource(store, onValidationError);
            source.Add(configKey, thumbprint, keyUse);
            return builder.Add(source);
        }

        /// <summary>
        /// Can reads multiple private keys from a certificate store into IConfiguration.
        /// Use <paramref name="configure"/> to map config keys to certificate identifiers.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="certStore"></param>
        /// <param name="onWarning">
        /// Optional callback invoked when a certificate is skipped. See single-entry overload for usage example.
        /// </param>
        public static IConfigurationBuilder AddPrivateKeyFromCertificateStore(
            this IConfigurationBuilder builder,
            Action<PrivateKeyCertificateStoreConfigurationSource> configure,
            ICertificateStore? certStore = null,
            Action<CertificateLoadDiagnostic>? onWarning = null)
        {
            var store = certStore ?? new WindowsCertificateStore();
            var source = new PrivateKeyCertificateStoreConfigurationSource(store, onWarning);
            configure(source);
            return builder.Add(source);
        }
    }
}
