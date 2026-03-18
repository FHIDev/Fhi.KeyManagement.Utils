using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using Fhi.Security.Cryptography.Certificates;

namespace Fhi.Security.Cryptography.UnitTests.Setup
{
    internal class InMemoryCertificateStoreMock : ICertificateStore
    {
        private readonly ConcurrentDictionary<string, X509Certificate2> _certificates = new();

        public void Add(string identifier, X509Certificate2 certificate)
            => _certificates[identifier] = certificate;

        public X509Certificate2? GetCertificate(string identifier)
            => _certificates.TryGetValue(identifier, out var cert) ? cert : null;
    }
}
