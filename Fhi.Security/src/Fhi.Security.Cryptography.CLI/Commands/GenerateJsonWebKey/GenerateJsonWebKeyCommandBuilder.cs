using System.CommandLine;
using Fhi.Security.Cryptography.CLI.Commands.Extensions;
using Fhi.Security.Cryptography.Jwks;
using Microsoft.Extensions.Hosting;

namespace Fhi.Security.Cryptography.CLI.Commands.GenerateJsonWebKey
{
    internal class GenerateJsonWebKeyCommandBuilder(JsonWebKeyGeneratorHandler commandHandler) : ICommandBuilder
    {
        private readonly JsonWebKeyGeneratorHandler _commandHandler = commandHandler;

        public Command Build(IHost host)
        {
            var generateJsonWebKeyCommand = new Command(
                GenerateJsonWebKeyParameterNames.CommandName,
                "Generate a new RSA key pair")
            {
                TreatUnmatchedTokensAsErrors = true
            };

            var keyFileNamePrefixOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Long,
                GenerateJsonWebKeyParameterNames.KeyFileNamePrefix.Short,
                "Prefix for the key file names",
                isRequired: true);

            var keyDirectoryOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.KeyDirectory.Long,
                GenerateJsonWebKeyParameterNames.KeyDirectory.Short,
                "Directory to store the generated keys",
                isRequired: false);

            var keyCustomKidOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.KeyCustomKid.Long,
                GenerateJsonWebKeyParameterNames.KeyCustomKid.Short,
                "Custom Kid value to use in the generated keys",
                isRequired: false);

            var outputFormatOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.OutputFormat.Long,
                GenerateJsonWebKeyParameterNames.OutputFormat.Short,
                "Output format: 'json' (default) or 'base64' for base64-encoded content",
                isRequired: false);
            outputFormatOption.AcceptOnlyFromAmong(OutputFormats.Json, OutputFormats.Base64);

            generateJsonWebKeyCommand.SetAction((ParseResult parseResult) =>
            {
                var keyFileNamePrefix = parseResult.GetValue(keyFileNamePrefixOption);
                var keyDirectory = parseResult.GetValue(keyDirectoryOption);
                var keyCustomKid = parseResult.GetValue(keyCustomKidOption);
                var outputFormat = parseResult.GetValue(outputFormatOption);

                var parameters = new GenerateJsonWebKeyParameters
                {
                    // TODO: fix "may be null"
                    KeyFileNamePrefix = keyFileNamePrefix!,
                    KeyDirectory = keyDirectory,
                    KeyCustomKid = keyCustomKid,
                    OutputFormat = outputFormat ?? OutputFormats.Json
                };

                _commandHandler.Execute(parameters);
                return Task.FromResult(0);
            });

            return generateJsonWebKeyCommand;
        }
    }
}