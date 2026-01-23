using Fhi.Security.Cryptography.Certificates;
using Fhi.Security.Cryptography.CLI.Commands.Extensions;
using Microsoft.Extensions.Hosting;
using System.CommandLine;

namespace Fhi.Security.Cryptography.CLI.Commands.GenerateCertificate
{
    internal class GenerateCertificateCommandBuilder(GenerateCertificateCommandHandler commandHandler) : ICommandBuilder
    {
        private readonly GenerateCertificateCommandHandler _commandHandler = commandHandler;

        public Command Build(IHost host)
        {
            var generateCertCommand = new Command(
                GenerateCertificateParameterNames.CommandName,
                "Generate keys in PEM certificate format")
            {
                TreatUnmatchedTokensAsErrors = true
            };

            var certificateCommonNameOption = generateCertCommand.CreateStringOption(
                GenerateCertificateParameterNames.CertificateCommonName.Long,
                GenerateCertificateParameterNames.CertificateCommonName.Short,
                "Common Name (CN) for the certificate",
                isRequired: true);

            var certificatePasswordOption = generateCertCommand.CreateStringOption(
                GenerateCertificateParameterNames.CertificatePassword.Long,
                GenerateCertificateParameterNames.CertificatePassword.Short,
                "Password for the generated certificate",
                isRequired: true);

            var certificateDirectoryOption = generateCertCommand.CreateStringOption(
                GenerateCertificateParameterNames.CertificateDirectory.Long,
                GenerateCertificateParameterNames.CertificateDirectory.Short,
                "Directory to store the generated certificates",
                isRequired: false);

            var validityMonthsOption = generateCertCommand.CreateIntOption(
                GenerateCertificateParameterNames.ValidityMonths.Long,
                GenerateCertificateParameterNames.ValidityMonths.Short,
                "Number of months the certificate is valid",
                defaultValue: Certificate.DefaultValidityMonths);

            generateCertCommand.SetAction((ParseResult parseResult) =>
            {
                var certificateCommonName = parseResult.GetValue(certificateCommonNameOption);
                var certificatePassword = parseResult.GetValue(certificatePasswordOption);
                var certificateDirectory = parseResult.GetValue(certificateDirectoryOption);
                var validityMonths = parseResult.GetValue(validityMonthsOption);

                var parameters = new GenerateCertificateParameters
                {
                    // TODO: fix "may be null"
                    CertificateCommonName = certificateCommonName!,
                    CertificatePassword = certificatePassword!,
                    CertificateDirectory = certificateDirectory,
                    ValidityMonths = validityMonths
                };

                _commandHandler.Execute(parameters);
                return Task.FromResult(0);
            });

            return generateCertCommand;
        }
    }
}