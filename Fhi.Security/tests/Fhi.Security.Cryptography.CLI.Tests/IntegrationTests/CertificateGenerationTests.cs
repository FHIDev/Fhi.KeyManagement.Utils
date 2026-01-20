using System.Security.Cryptography.X509Certificates;
using Fhi.Security.Cryptography.CLI.Commands.GenerateCertificate;
using Fhi.Security.Cryptography.CLI.IntegrationTests.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Fhi.Security.Cryptography.CLI.IntegrationTests
{
    public class CertificateGenerationTests
    {
        private const string TestPassword = "TestPassword123!";
        private static readonly TimeSpan ValidityTolerance = TimeSpan.FromSeconds(30);

        private FileHandlerMock _fileHandlerMock = null!;
        private FakeLoggerProvider _fakeLogProvider = null!;

        [SetUp]
        public void SetUp()
        {
            _fileHandlerMock = new FileHandlerMock();
            _fakeLogProvider = new FakeLoggerProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _fakeLogProvider.Dispose();
        }

        private async Task<int> ExecuteCommandAsync(params string[] args)
        {
            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(args)
                .WithFileHandler(_fileHandlerMock)
                .WithLoggerProvider(_fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            return await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);
        }

        private X509Certificate2 LoadGeneratedCertificate(string certName, string password)
        {
            var pfxFile = _fileHandlerMock.ByteFiles
                .First(f => f.Key.EndsWith($"{certName}_private.pfx"));
            return X509CertificateLoader.LoadPkcs12(pfxFile.Value, password);
        }

        private List<string> GetLogMessages() =>
            _fakeLogProvider.Collector.GetSnapshot().Select(x => x.Message).ToList();

        [TestCase("--CertificateCommonName", "--CertificatePassword", "--CertificateDirectory")]
        [TestCase("-cn", "-pwd", "-dir")]
        public async Task GIVEN_GenerateCertificates_WHEN_ValidOptions_THEN_CreateCertificateFiles(
            string commonNameOption, string passwordOption, string directoryOption)
        {
            var certName = "integration_test";
            var directoryPath = "c:\\temp";
            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                commonNameOption, certName,
                directoryOption, directoryPath,
                passwordOption, TestPassword
            };

            var exitCode = await ExecuteCommandAsync(args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(_fileHandlerMock.Files, Has.Count.EqualTo(3));
                Assert.That(exitCode, Is.Zero);

                var logs = GetLogMessages();
                Assert.That(logs, Does.Contain($"Private certificate saved: {Path.Combine(directoryPath, certName)}_private.pfx"));
                Assert.That(logs, Does.Contain($"Public certificate saved: {Path.Combine(directoryPath, certName)}_public.pem"));
                Assert.That(logs, Does.Contain($"Thumbprint saved: {Path.Combine(directoryPath, certName)}_thumbprint.txt"));
            }
        }

        [Test]
        public async Task GIVEN_GenerateCertificates_WHEN_EmptyPath_THEN_UseCurrentDirectory()
        {
            var certName = "TestCert";
            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                $"--{GenerateCertificateParameterNames.CertificateCommonName.Long}", certName,
                $"--{GenerateCertificateParameterNames.CertificatePassword.Long}", TestPassword
            };

            var expectedPublicCertPath = Path.Combine(Environment.CurrentDirectory, $"{certName}_public.pem");
            var expectedPrivateCertPath = Path.Combine(Environment.CurrentDirectory, $"{certName}_private.pfx");
            var expectedThumbprintPath = Path.Combine(Environment.CurrentDirectory, $"{certName}_thumbprint.txt");

            var exitCode = await ExecuteCommandAsync(args);

            using (Assert.EnterMultipleScope())
            {
                var logs = GetLogMessages();
                Assert.That(exitCode, Is.Zero);
                Assert.That(logs, Does.Contain($"Private certificate saved: {expectedPrivateCertPath}"));
                Assert.That(logs, Does.Contain($"Public certificate saved: {expectedPublicCertPath}"));
                Assert.That(logs, Does.Contain($"Thumbprint saved: {expectedThumbprintPath}"));
            }
        }

        [Test]
        public async Task GIVEN_GenerateCertificates_WHEN_MissingPassword_THEN_ReturnError()
        {
            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                $"--{GenerateCertificateParameterNames.CertificateCommonName.Long}", "TestName",
            };

            using var errorWriter = new StringWriter();
            var config = new System.CommandLine.InvocationConfiguration { Error = errorWriter };

            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(args)
                .WithFileHandler(_fileHandlerMock)
                .WithLoggerProvider(_fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);
            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult, config);

            using (Assert.EnterMultipleScope())
            {
                var errorOutput = errorWriter.ToString();
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(errorOutput, Does.Contain("Option '--CertificatePassword' is required"));
            }
        }

        [Test]
        public async Task GIVEN_GenerateCertificates_WHEN_DefaultValidity_THEN_UseDefaultExpiration()
        {
            var certName = "default_validity";
            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                $"--{GenerateCertificateParameterNames.CertificateCommonName.Long}", certName,
                $"--{GenerateCertificateParameterNames.CertificatePassword.Long}", TestPassword
            };

            var exitCode = await ExecuteCommandAsync(args);
            using var cert = LoadGeneratedCertificate(certName, TestPassword);
            var expectedNotAfter = DateTime.UtcNow.AddYears(2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(_fileHandlerMock.Files, Has.Count.EqualTo(3));
                Assert.That(cert.NotAfter.ToUniversalTime(), Is.EqualTo(expectedNotAfter).Within(ValidityTolerance));
            }
        }

        [Test]
        public async Task GIVEN_GenerateCertificates_WHEN_ValidityYearsOnly_THEN_CertificateHasCorrectExpiration()
        {
            var certName = "years_only";
            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                $"--{GenerateCertificateParameterNames.CertificateCommonName.Long}", certName,
                $"--{GenerateCertificateParameterNames.CertificatePassword.Long}", TestPassword,
                "--ValidityYears", "3"
            };

            var exitCode = await ExecuteCommandAsync(args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(_fileHandlerMock.Files, Has.Count.EqualTo(3));

                using var cert = LoadGeneratedCertificate(certName, TestPassword);
                var expectedNotAfter = DateTime.UtcNow.AddYears(3);

                Assert.That(cert.NotAfter.ToUniversalTime(), Is.EqualTo(expectedNotAfter).Within(ValidityTolerance));
            }
        }

        [TestCase("--ValidityYears", "--ValidityMonths")]
        [TestCase("-vy", "-vm")]
        public async Task GIVEN_GenerateCertificates_WHEN_ValidityOptions_THEN_CertificateHasCorrectExpiration(
            string yearsOption, string monthsOption)
        {
            var certName = $"validity_{yearsOption.TrimStart('-')}";
            var args = new[]
            {
                GenerateCertificateParameterNames.CommandName,
                $"--{GenerateCertificateParameterNames.CertificateCommonName.Long}", certName,
                $"--{GenerateCertificateParameterNames.CertificatePassword.Long}", TestPassword,
                yearsOption, "1",
                monthsOption, "6"
            };

            var exitCode = await ExecuteCommandAsync(args);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(_fileHandlerMock.Files, Has.Count.EqualTo(3));

                using var cert = LoadGeneratedCertificate(certName, TestPassword);
                var expectedNotAfter = DateTime.UtcNow.AddYears(1).AddMonths(6);

                Assert.That(cert.NotAfter.ToUniversalTime(), Is.EqualTo(expectedNotAfter).Within(ValidityTolerance));
            }
        }
    }
}
