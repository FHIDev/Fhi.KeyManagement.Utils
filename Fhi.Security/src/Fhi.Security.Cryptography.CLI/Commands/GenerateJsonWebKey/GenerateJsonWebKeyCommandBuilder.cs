using System.CommandLine;
using Fhi.Security.Cryptography.CLI.Commands.Extensions;
using Fhi.Security.Cryptography.Jwks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

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

            var outputTransformOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.OutputTransform.Long,
                GenerateJsonWebKeyParameterNames.OutputTransform.Short,
                $"Output transform: '{OutputTransformType.JsonEscape.ToCamelCase()}' (default) or '{OutputTransformType.Base64.ToCamelCase()}' for base64-encoded content",
                isRequired: false);
            outputTransformOption.AcceptOnlyFromAmong(
                OutputTransformType.JsonEscape.ToCamelCase(),
                OutputTransformType.Base64.ToCamelCase());

            var signingAlgorithmOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.SigningAlgorithm.Long,
                GenerateJsonWebKeyParameterNames.SigningAlgorithm.Short,
                $"Signing algorithm (default: {SecurityAlgorithms.RsaSsaPssSha512}). One of: {string.Join(", ", JWK.SupportedSigningAlgorithms)}",
                isRequired: false);
            signingAlgorithmOption.AcceptOnlyFromAmong([.. JWK.SupportedSigningAlgorithms]);
            signingAlgorithmOption.DefaultValueFactory = _ => SecurityAlgorithms.RsaSsaPssSha512;

            generateJsonWebKeyCommand.SetAction((ParseResult parseResult) =>
            {
                var keyFileNamePrefix = parseResult.GetValue(keyFileNamePrefixOption);
                var keyDirectory = parseResult.GetValue(keyDirectoryOption);
                var keyCustomKid = parseResult.GetValue(keyCustomKidOption);
                var outputTransformString = parseResult.GetValue(outputTransformOption);
                var outputTransform = string.IsNullOrEmpty(outputTransformString)
                    ? OutputTransformType.JsonEscape
                    : Enum.Parse<OutputTransformType>(outputTransformString, ignoreCase: true);

                var parameters = new GenerateJsonWebKeyParameters
                {
                    // TODO: fix "may be null"
                    KeyFileNamePrefix = keyFileNamePrefix!,
                    KeyDirectory = keyDirectory,
                    KeyCustomKid = keyCustomKid,
                    OutputTransform = outputTransform,
                    SigningAlgorithm = parseResult.GetValue(signingAlgorithmOption)!
                };

                _commandHandler.Execute(parameters);
                return Task.FromResult(0);
            });

            return generateJsonWebKeyCommand;
        }
    }
}