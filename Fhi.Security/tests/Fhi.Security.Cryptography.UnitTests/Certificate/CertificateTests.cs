using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fhi.Security.Cryptography.Certificates;

namespace Fhi.Security.Cryptography.UnitTests.Certificate
{
    public class CertificateTests
    {
        private const string TestCommonName = "TestCert";
        private const string TestPassword = "TestPassword123!";
        private static readonly TimeSpan ValidityTolerance = TimeSpan.FromSeconds(30);

        [TestCase(24)]
        [TestCase(36)]
        [TestCase(18)]
        [TestCase(6)]
        public void GIVEN_CreateAsymmetricKeyPair_WHEN_ValidParameters_THEN_CreateCertificateWithCorrectExpiration(
            int validityMonths)
        {
            var result = Certificates.Certificate.CreateAsymmetricKeyPair(
                TestCommonName, TestPassword, validityMonths);

            using var cert = X509CertificateLoader.LoadPkcs12(result.CertificatePrivateKey.ToArray(), TestPassword);

            var expectedNotAfter = DateTime.UtcNow
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

        [Test]
        public void GIVEN_CreateAsymmetricKeyPair_WHEN_NoValiditySpecified_THEN_UseDefault24Months()
        {
            var result = Certificates.Certificate.CreateAsymmetricKeyPair(
                TestCommonName, TestPassword);

            using var cert = X509CertificateLoader.LoadPkcs12(result.CertificatePrivateKey.ToArray(), TestPassword);

            var expectedNotAfter = DateTime.UtcNow.AddMonths(24);

            Assert.That(cert.NotAfter.ToUniversalTime(), Is.EqualTo(expectedNotAfter).Within(ValidityTolerance));
        }

        [TestCase(2048)]
        [TestCase(3072)]
        public void GIVEN_CreateAsymmetricKeyPair_WHEN_CustomKeySize_THEN_CreateCertificateWithSpecifiedKeySize(int keySize)
        {
            var result = Certificates.Certificate.CreateAsymmetricKeyPair(
                TestCommonName, TestPassword, keySize: keySize);

            using var cert = X509CertificateLoader.LoadPkcs12(result.CertificatePrivateKey.ToArray(), TestPassword);
            var rsaPublicKey = cert.GetRSAPublicKey();

            Assert.That(rsaPublicKey, Is.Not.Null);
            Assert.That(rsaPublicKey!.KeySize, Is.EqualTo(keySize));
        }

        [Test]
        public void GIVEN_CreateAsymmetricKeyPair_WHEN_CustomHashAlgorithm_THEN_CreateCertificateWithSpecifiedAlgorithm()
        {
            var hashAlgorithm = HashAlgorithmName.SHA256;

            var result = Certificates.Certificate.CreateAsymmetricKeyPair(
                TestCommonName, TestPassword, hashAlgorithm: hashAlgorithm);

            using var cert = X509CertificateLoader.LoadPkcs12(result.CertificatePrivateKey.ToArray(), TestPassword);

            Assert.That(cert.SignatureAlgorithm.FriendlyName, Does.Contain(hashAlgorithm.Name!.ToLowerInvariant()));
        }

        [Test]
        public void GIVEN_CreateAsymmetricKeyPair_WHEN_PssSignaturePadding_THEN_CreateCertificateWithPssPadding()
        {
            var signaturePadding = RSASignaturePadding.Pss;

            var result = Certificates.Certificate.CreateAsymmetricKeyPair(
                TestCommonName, TestPassword, signaturePadding: signaturePadding);

            using var cert = X509CertificateLoader.LoadPkcs12(result.CertificatePrivateKey.ToArray(), TestPassword);

            Assert.That(cert.SignatureAlgorithm.FriendlyName, Does.Contain("PSS").IgnoreCase);
        }
    }
}
