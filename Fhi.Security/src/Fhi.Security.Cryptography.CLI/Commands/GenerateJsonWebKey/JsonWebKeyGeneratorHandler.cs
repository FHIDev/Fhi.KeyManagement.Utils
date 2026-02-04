using System.Text;
using System.Text.Json;
using Fhi.Security.Cryptography.CLI.Services;
using Fhi.Security.Cryptography.Jwks;
using Microsoft.Extensions.Logging;

namespace Fhi.Security.Cryptography.CLI.Commands.GenerateJsonWebKey
{
    internal class JsonWebKeyGeneratorHandler(IFileHandler fileHandler, ILogger<JsonWebKeyGeneratorHandler> logger)
    {
        private readonly IFileHandler _fileHandler = fileHandler;
        private readonly ILogger<JsonWebKeyGeneratorHandler> _logger = logger;

        /// <summary>
        /// Generates private and public key.
        /// Stores in executing directory if path not specified.
        /// File extension is .json for jsonEscape transform, .txt for base64 transform.
        /// </summary>
        public void Execute(GenerateJsonWebKeyParameters parameters)
        {
            var keyPath = parameters.KeyDirectory ?? Environment.CurrentDirectory;
            if (!_fileHandler.PathExists(keyPath))
            {
                _logger.LogInformation("Key path did not exist. Creating folder {@KeyPath}", keyPath);
                _fileHandler.CreateDirectory(keyPath);
            }

            var keyPair = JWK.Create(kid: parameters.KeyCustomKid);

            var privateKeyContent = ApplyTransform(keyPair.PrivateKey, parameters.OutputTransform);
            var publicKeyContent = ApplyTransform(keyPair.PublicKey, parameters.OutputTransform);

            var fileExtension = parameters.OutputTransform == OutputTransformType.Base64 ? "txt" : "json";
            var privateKeyPath = Path.Combine(keyPath, $"{parameters.KeyFileNamePrefix}_private.{fileExtension}");
            var publicKeyPath = Path.Combine(keyPath, $"{parameters.KeyFileNamePrefix}_public.{fileExtension}");

            _fileHandler.WriteAllText(privateKeyPath, privateKeyContent);
            _fileHandler.WriteAllText(publicKeyPath, publicKeyContent);

            _logger.LogInformation("Private key saved: {@PrivateKeyPath}", privateKeyPath);
            _logger.LogInformation("Public key saved: {@PublicKeyPath}", publicKeyPath);
        }

        private static string ApplyTransform(string content, OutputTransformType transform)
        {
            return transform switch
            {
                OutputTransformType.Base64 => Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
                OutputTransformType.JsonEscape => JsonSerializer.Serialize(content),
                _ => throw new ArgumentOutOfRangeException(nameof(transform), transform, "Unknown output transform")
            };
        }
    }
}
