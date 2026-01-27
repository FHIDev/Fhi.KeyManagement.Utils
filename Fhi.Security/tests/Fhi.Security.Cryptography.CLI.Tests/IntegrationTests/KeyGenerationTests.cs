using System.Text;
using System.Text.Json;
using Fhi.Security.Cryptography.CLI.Commands.GenerateJsonWebKey;
using Fhi.Security.Cryptography.CLI.IntegrationTests.Setup;
using Fhi.Security.Cryptography.Jwks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Fhi.Security.Cryptography.CLI.IntegrationTests
{
    public class KeyGenerationTests
    {
        [TestCase("--KeyFileNamePrefix", "--KeyDirectory", "")]
        [TestCase("--KeyFileNamePrefix", "--KeyDirectory", "--KeyCustomKid")]
        [TestCase("-n", "-d", "")]
        [TestCase("-n", "-d", "-k")]
        public async Task GIVEN_GenerateJsonWebKeys_WHEN_ValidParameters_THEN_GenerateValidKeyPair(string prefixOption, string directoryPathOption, string? customKidOption)
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();

            var prefixName = "integration_test";
            var directoryPath = "c:\\temp";

            var args = new List<string>
            {
                GenerateJsonWebKeyParameterNames.CommandName,
                $"{prefixOption}",
                prefixName,
                $"{directoryPathOption}",
                directoryPath
            };

            if (!string.IsNullOrWhiteSpace(customKidOption))
            {
                args.Add($"{customKidOption}");
                args.Add("customKidTest");
            }

            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(args.ToArray())
                .WithFileHandler(fileHandlerMock)
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);

            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(fileHandlerMock.Files, Has.Count.EqualTo(2));
                Assert.That(exitCode, Is.Zero);

                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain($"Private key saved: {Path.Combine(directoryPath, prefixName)}_private.json"));
                Assert.That(logs!, Does.Contain($"Public key saved: {Path.Combine(directoryPath, prefixName)}_public.json"));
            }
        }

        [Test]
        [Ignore("TODO: Parsing errors cannot be tested on command level, needs to be tested on program level")]
        public async Task GIVEN_GenerateJsonWebKeys_WHEN_InvalidParameterAsync_THEN_ExitWithExitCode1()
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();

            var prefixName = "integration_test";
            var directoryPath = "c:\\temp";

            var args = new[]
            {
                GenerateJsonWebKeyParameterNames.CommandName,
                $"--{GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Long}",
                prefixName,
                $"--{GenerateJsonWebKeyParameterNames.KeyDirectory.Long}",
                directoryPath,
                "--invalidparameter", "integration_test"
            };
            var rootCommandBuilder = new RootCommandBuilder()
               .WithArgs(args)
               .WithFileHandler(fileHandlerMock)
               .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);

            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();

                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(logs, Does.Contain("Unrecognized command or argument '--invalidparameter'."));
                Assert.That(logs!, !Does.Contain(@"Private key saved: c:\temp\integration_test_private.json"));
                Assert.That(logs!, !Does.Contain(@"Public key saved: c:\temp\integration_test_public.json"));
            }
        }

        [Test]
        public async Task GIVEN_GenerateJsonWebKeys_WHEN_PathIsEmpty_THEN_UseCurrentDirectory()
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();
            var args = new[]
            {
                GenerateJsonWebKeyParameterNames.CommandName,
                $"--{GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Long}", "TestClient",
                $"--{GenerateJsonWebKeyParameterNames.KeyCustomKid.Long}", "TESSTSTST"
            };
            var rootCommandBuilder = new RootCommandBuilder()
              .WithArgs(args)
              .WithFileHandler(fileHandlerMock)
              .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);

            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                var expectedPublicKeyPath = Path.Combine(Environment.CurrentDirectory, "TestClient_public.json");
                var expectedPrivateKeyPath = Path.Combine(Environment.CurrentDirectory, "TestClient_private.json");
                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(exitCode, Is.Zero);

                Assert.That(logs!, Does.Contain($"Private key saved: {expectedPrivateKeyPath}"));
                Assert.That(logs!, Does.Contain($"Public key saved: {expectedPublicKeyPath}"));
            }
        }

        [TestCase("--OutputFormat", OutputFormats.Base64)]
        [TestCase("-of", OutputFormats.Base64)]
        public async Task GIVEN_GenerateJsonWebKeys_WHEN_Base64OutputFormat_THEN_OutputBase64EncodedContent(string formatOption, string formatValue)
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();

            var prefixName = "base64_test";
            var directoryPath = "c:\\temp";

            var args = new[]
            {
                GenerateJsonWebKeyParameterNames.CommandName,
                $"--{GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Long}", prefixName,
                $"--{GenerateJsonWebKeyParameterNames.KeyDirectory.Long}", directoryPath,
                formatOption, formatValue
            };

            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(args)
                .WithFileHandler(fileHandlerMock)
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);

            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(fileHandlerMock.Files, Has.Count.EqualTo(2));

                var expectedPrivateKeyPath = Path.Combine(directoryPath, $"{prefixName}_private.txt");
                var expectedPublicKeyPath = Path.Combine(directoryPath, $"{prefixName}_public.txt");

                var logs = fakeLogProvider.Collector?.GetSnapshot().Select(x => x.Message).ToList();
                Assert.That(logs!, Does.Contain($"Private key saved: {expectedPrivateKeyPath}"));
                Assert.That(logs!, Does.Contain($"Public key saved: {expectedPublicKeyPath}"));

                // Verify the content is valid base64 that decodes to valid JSON
                var privateKeyBase64 = fileHandlerMock.Files[expectedPrivateKeyPath];
                var publicKeyBase64 = fileHandlerMock.Files[expectedPublicKeyPath];

                var privateKeyJson = Encoding.UTF8.GetString(Convert.FromBase64String(privateKeyBase64));
                var publicKeyJson = Encoding.UTF8.GetString(Convert.FromBase64String(publicKeyBase64));

                // Verify it's valid JSON by parsing
                using var privateKeyDoc = JsonDocument.Parse(privateKeyJson);
                using var publicKeyDoc = JsonDocument.Parse(publicKeyJson);

                // Verify JWK structure
                Assert.That(privateKeyDoc.RootElement.TryGetProperty("kty", out _), Is.True);
                Assert.That(publicKeyDoc.RootElement.TryGetProperty("kty", out _), Is.True);
                Assert.That(privateKeyDoc.RootElement.TryGetProperty("d", out _), Is.True);
                Assert.That(publicKeyDoc.RootElement.TryGetProperty("d", out _), Is.False);
                Assert.That(publicKeyDoc.RootElement.TryGetProperty("n", out _), Is.True);
                Assert.That(publicKeyDoc.RootElement.TryGetProperty("e", out _), Is.True);
            }
        }

        [Test]
        public async Task GIVEN_GenerateJsonWebKeys_WHEN_OutputFormatOmitted_THEN_DefaultsToJsonOutput()
        {
            var fileHandlerMock = new FileHandlerMock();
            var fakeLogProvider = new FakeLoggerProvider();

            var prefixName = "json_default_test";
            var directoryPath = "c:\\temp";

            var args = new[]
            {
                GenerateJsonWebKeyParameterNames.CommandName,
                $"--{GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Long}", prefixName,
                $"--{GenerateJsonWebKeyParameterNames.KeyDirectory.Long}", directoryPath
            };

            var rootCommandBuilder = new RootCommandBuilder()
                .WithArgs(args)
                .WithFileHandler(fileHandlerMock)
                .WithLoggerProvider(fakeLogProvider, LogLevel.Trace);

            var rootCommand = rootCommandBuilder.Build();
            var parseResult = rootCommand.Parse(rootCommandBuilder.Args);

            var exitCode = await CommandLineBuilder.CommandLineBuilderInvokerAsync(parseResult);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(fileHandlerMock.Files, Has.Count.EqualTo(2));

                var expectedPrivateKeyPath = Path.Combine(directoryPath, $"{prefixName}_private.json");
                var expectedPublicKeyPath = Path.Combine(directoryPath, $"{prefixName}_public.json");

                Assert.That(fileHandlerMock.Files.ContainsKey(expectedPrivateKeyPath), Is.True);
                Assert.That(fileHandlerMock.Files.ContainsKey(expectedPublicKeyPath), Is.True);

                // Verify the content is valid JSON
                using var privateKeyDoc = JsonDocument.Parse(fileHandlerMock.Files[expectedPrivateKeyPath]);
                using var publicKeyDoc = JsonDocument.Parse(fileHandlerMock.Files[expectedPublicKeyPath]);

                Assert.That(privateKeyDoc.RootElement.TryGetProperty("kty", out _), Is.True);
                Assert.That(publicKeyDoc.RootElement.TryGetProperty("kty", out _), Is.True);
            }
        }
    }
}
