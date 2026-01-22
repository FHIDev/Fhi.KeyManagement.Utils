using System.Text;
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
        /// Private key will be named FileName_private.json (or .txt if base64 encoded)
        /// Public key will be named FileName_public.json (or .txt if base64 encoded)
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

            var privateKeyContent = parameters.KeyBase64
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes(keyPair.PrivateKey))
                : keyPair.PrivateKey;

            var publicKeyContent = parameters.KeyBase64
                ? Convert.ToBase64String(Encoding.UTF8.GetBytes(keyPair.PublicKey))
                : keyPair.PublicKey;

            var fileExtension = parameters.KeyBase64 ? "txt" : "json";
            var privateKeyPath = Path.Combine(keyPath, $"{parameters.KeyFileNamePrefix}_private.{fileExtension}");
            var publicKeyPath = Path.Combine(keyPath, $"{parameters.KeyFileNamePrefix}_public.{fileExtension}");

            _fileHandler.WriteAllText(privateKeyPath, privateKeyContent);
            _fileHandler.WriteAllText(publicKeyPath, publicKeyContent);

            _logger.LogInformation("Private key saved: {@PrivateKeyPath}", privateKeyPath);
            _logger.LogInformation("Public key saved: {@PublicKeyPath}", publicKeyPath);
        }
    }
}
