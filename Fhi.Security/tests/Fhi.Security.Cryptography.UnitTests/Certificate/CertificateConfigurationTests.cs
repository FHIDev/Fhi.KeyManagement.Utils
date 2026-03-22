using System.Security.Cryptography;
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
        public void GIVEN_addPrivateKeyFromCertificateStore_WHEN_certificateIsExpired_THEN_configKeyIsAbsentAndWarningIsRaised()
        {
            using var cert = CreateCertWithValidity(DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddDays(-1));
            var store = new InMemoryCertificateStoreMock();
            store.Add("thumb", cert);

            var warnings = new List<CertificateLoadDiagnostic>();
            var config = new ConfigurationBuilder()
                .AddPrivateKeyFromCertificateStore("MyClient:PrivateKey", "thumb",
                    certStore: store, onValidationError: warnings.Add)
                .Build();

            Assert.That(config["MyClient:PrivateKey"], Is.Null);
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0].Reason, Does.Contain("expired"));
        }

        [Test]
        public void GIVEN_addPrivateKeyFromCertificateStore_WHEN_certificateIsNotYetValid_THEN_configKeyIsAbsentAndWarningIsRaised()
        {
            using var cert = CreateCertWithValidity(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(30));
            var store = new InMemoryCertificateStoreMock();
            store.Add("thumb", cert);

            var warnings = new List<CertificateLoadDiagnostic>();
            var config = new ConfigurationBuilder()
                .AddPrivateKeyFromCertificateStore("MyClient:PrivateKey", "thumb",
                    certStore: store, onValidationError: warnings.Add)
                .Build();

            Assert.That(config["MyClient:PrivateKey"], Is.Null);
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0].Reason, Does.Contain("not valid until"));
        }

        [Test]
        public void GIVEN_addPrivateKeyFromCertificateStore_WHEN_certificateKeyUsageDoesNotAllowSigning_THEN_configKeyIsAbsentAndWarningIsRaised()
        {
            using var cert = CreateCertWithKeyUsage(X509KeyUsageFlags.KeyEncipherment);
            var store = new InMemoryCertificateStoreMock();
            store.Add("thumb", cert);

            var warnings = new List<CertificateLoadDiagnostic>();
            var config = new ConfigurationBuilder()
                .AddPrivateKeyFromCertificateStore("MyClient:PrivateKey", "thumb",
                    keyUse: CertificateKeyUse.Signing,
                    certStore: store, onValidationError: warnings.Add)
                .Build();

            Assert.That(config["MyClient:PrivateKey"], Is.Null);
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0].Reason, Does.Contain("KeyUsage"));
        }

        [Test]
        public void GIVEN_addPrivateKeyFromCertificateStore_WHEN_certificateKeyUsageDoesNotAllowEncryption_THEN_configKeyIsAbsentAndWarningIsRaised()
        {
            using var cert = CreateCertWithKeyUsage(X509KeyUsageFlags.DigitalSignature);
            var store = new InMemoryCertificateStoreMock();
            store.Add("thumb", cert);

            var warnings = new List<CertificateLoadDiagnostic>();
            var config = new ConfigurationBuilder()
                .AddPrivateKeyFromCertificateStore("MyClient:PrivateKey", "thumb",
                    keyUse: CertificateKeyUse.Encryption,
                    certStore: store, onValidationError: warnings.Add)
                .Build();

            Assert.That(config["MyClient:PrivateKey"], Is.Null);
            Assert.That(warnings, Has.Count.EqualTo(1));
            Assert.That(warnings[0].Reason, Does.Contain("KeyUsage"));
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
        private static X509Certificate2 CreateCertWithValidity(DateTimeOffset notBefore, DateTimeOffset notAfter)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var pfx = request.CreateSelfSigned(notBefore, notAfter).Export(X509ContentType.Pfx);
            return X509CertificateLoader.LoadPkcs12(pfx, null, X509KeyStorageFlags.Exportable);
        }

        private static X509Certificate2 CreateCertWithKeyUsage(X509KeyUsageFlags flags)
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509KeyUsageExtension(flags, critical: true));
            var pfx = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1)).Export(X509ContentType.Pfx);
            return X509CertificateLoader.LoadPkcs12(pfx, null, X509KeyStorageFlags.Exportable);
        }
    }
}
