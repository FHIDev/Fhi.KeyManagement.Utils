using System.CommandLine;
using Fhi.Security.Cryptography.CLI.Commands.Extensions;
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

            var keyBase64Option = generateJsonWebKeyCommand.CreateBoolOption(
                GenerateJsonWebKeyParameterNames.KeyBase64.Long,
                GenerateJsonWebKeyParameterNames.KeyBase64.Short,
                "Output the JWK content as base64-encoded string");

            generateJsonWebKeyCommand.SetAction((ParseResult parseResult) =>
            {
                var keyFileNamePrefix = parseResult.GetValue(keyFileNamePrefixOption);
                var keyDirectory = parseResult.GetValue(keyDirectoryOption);
                var keyCustomKid = parseResult.GetValue(keyCustomKidOption);
                var keyBase64 = parseResult.GetValue(keyBase64Option);

                var parameters = new GenerateJsonWebKeyParameters
                {
                    // TODO: fix "may be null"
                    KeyFileNamePrefix = keyFileNamePrefix!,
                    KeyDirectory = keyDirectory,
                    KeyCustomKid = keyCustomKid,
                    KeyBase64 = keyBase64
                };

                _commandHandler.Execute(parameters);
                return Task.FromResult(0);
            });

            return generateJsonWebKeyCommand;
        }
    }
}