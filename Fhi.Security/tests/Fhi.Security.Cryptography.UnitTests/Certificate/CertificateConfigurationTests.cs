using System.Security.Cryptography.X509Certificates;
using Fhi.Security.Cryptography.Certificates;
using Fhi.Security.Cryptography.UnitTests.Setup;
using Microsoft.Extensions.Configuration;

namespace Fhi.Security.Cryptography.UnitTests.Certificate
{
    public class CertificateConfigurationTests
    {
        [Test]
        public void GIVEN_addPrivateKeyFromCertificateStore_WHEN_thumbprintNotFound_THEN_configKeyIsAbsent()
        {
            var warnings = new List<CertificateLoadDiagnostic>();
            var config = new ConfigurationBuilder()
                .AddPrivateKeyFromCertificateStore("MyClient:PrivateKey", "AABBCCDD",
                    certStore: new InMemoryCertificateStoreMock(),
                    onValidationError: warnings.Add)
                .Build();

            Assert.That(config["MyClient:PrivateKey"], Is.Null);
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0].Reason, Does.Contain("Certificate not found in store."));
        }

        [Test]
        public void GIVEN_addPrivateKeyFromCertificateStore_WHEN_certificateHasNoPrivateKey_THEN_configKeyIsAbsentAndWarningIsRaised()
        {
            var keyPair = Certificates.Certificate.CreateAsymmetricKeyPair("CN", "PWD", 1);
            using var publicOnlyCert = X509Certificate2.CreateFromPem(keyPair.CertificatePublicKey);

            var store = new InMemoryCertificateStoreMock();
            store.Add("thumb", publicOnlyCert);

            var warnings = new List<CertificateLoadDiagnostic>();
            var config = new ConfigurationBuilder()
                .AddPrivateKeyFromCertificateStore("MyClient:PrivateKey", "thumb",
                    certStore: store, onValidationError: warnings.Add)
                .Build();

            Assert.That(config["MyClient:PrivateKey"], Is.Null);
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0].Reason, Does.Contain("private key"));
        }

        [Test]
        public void GIVEN_addPrivateKeyFromCertificateStore_WHEN_certificateFoundAndIsValid_THEN_privateKeyIsPopulated()
        {
            var keyPair = Certificates.Certificate.CreateAsymmetricKeyPair("CN", "PWD", 1);
            using var cert = X509CertificateLoader.LoadPkcs12(keyPair.CertificatePrivateKey.ToArray(), "PWD", X509KeyStorageFlags.Exportable);

            var store = new InMemoryCertificateStoreMock();
            store.Add("any-thumb", cert);

            var config = new ConfigurationBuilder()
                .AddPrivateKeyFromCertificateStore("MyClient:PrivateKey", "any-thumb", certStore: store)
                .Build();

            Assert.That(config["MyClient:PrivateKey"], Is.Not.Null.And.Not.Empty);
            Assert.That(config["MyClient:PrivateKey"], Does.StartWith("-----BEGIN RSA"));
        }

        [Test]
        public void GIVEN_addPrivateKeyFromCertificateStore_WHEN_multipleEntries_THEN_allConfigKeysArePopulated()
        {
            var keyPairA = Certificates.Certificate.CreateAsymmetricKeyPair("CN=A", "PWD", 1);
            var keyPairB = Certificates.Certificate.CreateAsymmetricKeyPair("CN=B", "PWD", 1);

            using var certA = X509CertificateLoader.LoadPkcs12(keyPairA.CertificatePrivateKey.ToArray(), "PWD", X509KeyStorageFlags.Exportable);
            using var certB = X509CertificateLoader.LoadPkcs12(keyPairB.CertificatePrivateKey.ToArray(), "PWD", X509KeyStorageFlags.Exportable);

            var store = new InMemoryCertificateStoreMock();
            store.Add("AABBCCDD", certA);
            store.Add("EEFF0011", certB);

            var config = new ConfigurationBuilder()
                .AddPrivateKeyFromCertificateStore(source =>
                {
                    source.Add("ClientA:PrivateKey", "AABBCCDD");
                    source.Add("ClientB:PrivateKey", "EEFF0011");
                }, certStore: store)
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(config["ClientA:PrivateKey"], Is.Not.Null.And.Not.Empty);
                Assert.That(config["ClientA:PrivateKey"], Does.StartWith("-----BEGIN RSA"));
                Assert.That(config["ClientB:PrivateKey"], Is.Not.Null.And.Not.Empty);
                Assert.That(config["ClientB:PrivateKey"], Does.StartWith("-----BEGIN RSA"));
            });
        }
    }
}
