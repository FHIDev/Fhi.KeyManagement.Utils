using Fhi.Security.Cryptography.Certificates;
using Fhi.Security.Cryptography.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Fhi.Security.Cryptography.CLI.Commands.GenerateCertificate
{
    internal class GenerateCertificateCommandHandler(
        IFileHandler fileHandler,
        ILogger<GenerateCertificateCommandHandler> logger)
    {
        private readonly IFileHandler _fileHandler = fileHandler;
        private readonly ILogger<GenerateCertificateCommandHandler> _logger = logger;

        /// <summary>
        /// Generates private and public key in certificate format.
        /// Stores in executing directory if path not specified.
        /// Public Key will be named CommonName_public.pem
        /// Password protected privateKey will be named CommonName_private.pfx
        /// Certificate thumbprint will be stored to CommonName_thumbprint.txt
        /// </summary>
        /// <returns></returns>
        public void Execute(GenerateCertificateParameters parameters)
        {
            using (_logger.BeginScope("CertificateCommonName: {CertificateCommonName}", parameters.CertificateCommonName))
            {
                var certPath = parameters.CertificateDirectory ?? Environment.CurrentDirectory;
                if (!_fileHandler.PathExists(certPath))
                {
                    _logger.LogInformation("Certificate path did not exist. Creating folder {@CertPath}", certPath);
                    _fileHandler.CreateDirectory(certPath);
                }

                var certificateFiles = Certificate.CreateAsymmetricKeyPair(
                    parameters.CertificateCommonName,
                    parameters.CertificatePassword,
                    parameters.ValidityYears,
                    parameters.ValidityMonths);

                var privateCertPath = Path.Combine(certPath, $"{parameters.CertificateCommonName}_private.pfx");
                var publicCertPath = Path.Combine(certPath, $"{parameters.CertificateCommonName}_public.pem");
                var thumbprintPath = Path.Combine(certPath, $"{parameters.CertificateCommonName}_thumbprint.txt");

                _fileHandler.WriteAllBytes(privateCertPath, certificateFiles.CertificatePrivateKey.ToArray());
                _fileHandler.WriteAllText(publicCertPath, certificateFiles.CertificatePublicKey);
                _fileHandler.WriteAllText(thumbprintPath, certificateFiles.CertificateThumbprint);

                _logger.LogInformation("Private certificate saved: {@Path}", privateCertPath);
                _logger.LogInformation("Public certificate saved: {@Path}", publicCertPath);
                _logger.LogInformation("Thumbprint saved: {@Path}", thumbprintPath);
            }
        }
    }
}
