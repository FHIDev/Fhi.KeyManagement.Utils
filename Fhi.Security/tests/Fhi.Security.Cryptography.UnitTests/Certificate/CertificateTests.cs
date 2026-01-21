using System.Security.Cryptography.X509Certificates;
using Fhi.Security.Cryptography.Certificates;

namespace Fhi.Security.Cryptography.UnitTests.Certificate
{
    public class CertificateTests
    {
        private const string TestCommonName = "TestCert";
        private const string TestPassword = "TestPassword123!";
        private static readonly TimeSpan ValidityTolerance = TimeSpan.FromSeconds(30);

        [TestCase(2, 0)]
        [TestCase(3, 0)]
        [TestCase(1, 6)]
        [TestCase(0, 18)]
        public void GIVEN_CreateAsymmetricKeyPair_WHEN_ValidParameters_THEN_CreateCertificateWithCorrectExpiration(
            int validityYears, int validityMonths)
        {
            var result = Certificates.Certificate.CreateAsymmetricKeyPair(
                TestCommonName, TestPassword, validityYears, validityMonths);

            using var cert = X509CertificateLoader.LoadPkcs12(result.CertificatePrivateKey.ToArray(), TestPassword);

            var expectedNotAfter = DateTime.UtcNow
                .AddYears(validityYears)
                .AddMonths(validityMonths);

            var rsaPublicKey = cert.GetRSAPublicKey();
            Assert.That(rsaPublicKey, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.CertificatePrivateKey, Is.Not.Empty);
                Assert.That(result.CertificatePublicKey, Is.Not.Empty);
                Assert.That(result.CertificateThumbprint, Is.Not.Empty);
                Assert.That(cert.Subject, Does.Contain($"CN={TestCommonName}"));
                Assert.That(rsaPublicKey.KeySize, Is.EqualTo(Certificates.Certificate.DefaultKeySize));
                Assert.That(cert.SignatureAlgorithm.FriendlyName, Does.Contain(Certificates.Certificate.DefaultHashAlgorithm.Name!.ToLowerInvariant()));
                Assert.That(cert.NotAfter.ToUniversalTime(), Is.EqualTo(expectedNotAfter).Within(ValidityTolerance));
            }
        }
    }
}
