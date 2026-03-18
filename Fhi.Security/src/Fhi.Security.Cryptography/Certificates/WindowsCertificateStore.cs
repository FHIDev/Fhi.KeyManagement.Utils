using System.Security.Cryptography.X509Certificates;

namespace Fhi.Security.Cryptography.Certificates
{
    /// <summary>
    /// Resolves certificates from the Windows certificate store by thumbprint.
    /// </summary>
    internal class WindowsCertificateStore(
        StoreName storeName = StoreName.My,
        StoreLocation storeLocation = StoreLocation.CurrentUser) : ICertificateStore
    {
        public X509Certificate2? GetCertificate(string identifier)
        {
            using var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            var matches = store.Certificates.Find(X509FindType.FindByThumbprint, identifier, validOnly: false);
            return matches.Count == 0 ? null : matches[0];
        }
    }
}
