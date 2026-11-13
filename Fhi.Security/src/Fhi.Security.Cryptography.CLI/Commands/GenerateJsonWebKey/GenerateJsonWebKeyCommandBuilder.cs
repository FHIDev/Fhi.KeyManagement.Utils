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
                $"Output transform for both keys: '{OutputTransformType.JsonEscape.ToCamelCase()}' (default), '{OutputTransformType.Base64.ToCamelCase()}' for base64-encoded content or '{OutputTransformType.None.ToCamelCase()}' for raw JSON",
                isRequired: false);
            outputTransformOption.AcceptOnlyFromAmong(AllowedOutputTransforms);

            var privateKeyOutputTransformOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.PrivateKeyOutputTransform.Long,
                GenerateJsonWebKeyParameterNames.PrivateKeyOutputTransform.Short,
                $"Output transform for the private key. Overrides --{GenerateJsonWebKeyParameterNames.OutputTransform.Long}",
                isRequired: false);
            privateKeyOutputTransformOption.AcceptOnlyFromAmong(AllowedOutputTransforms);

            var publicKeyOutputTransformOption = generateJsonWebKeyCommand.CreateStringOption(
                GenerateJsonWebKeyParameterNames.PublicKeyOutputTransform.Long,
                GenerateJsonWebKeyParameterNames.PublicKeyOutputTransform.Short,
                $"Output transform for the public key. Overrides --{GenerateJsonWebKeyParameterNames.OutputTransform.Long}",
                isRequired: false);
            publicKeyOutputTransformOption.AcceptOnlyFromAmong(AllowedOutputTransforms);

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
                var outputTransform = ParseOutputTransform(parseResult.GetValue(outputTransformOption)) ?? OutputTransformType.JsonEscape;
                var privateKeyOutputTransform = ParseOutputTransform(parseResult.GetValue(privateKeyOutputTransformOption)) ?? outputTransform;
                var publicKeyOutputTransform = ParseOutputTransform(parseResult.GetValue(publicKeyOutputTransformOption)) ?? outputTransform;

                var parameters = new GenerateJsonWebKeyParameters
                {
                    // TODO: fix "may be null"
                    KeyFileNamePrefix = keyFileNamePrefix!,
                    KeyDirectory = keyDirectory,
                    KeyCustomKid = keyCustomKid,
                    PrivateKeyOutputTransform = privateKeyOutputTransform,
                    PublicKeyOutputTransform = publicKeyOutputTransform,
                    SigningAlgorithm = parseResult.GetValue(signingAlgorithmOption)!
                };

                _commandHandler.Execute(parameters);
                return Task.FromResult(0);
            });

            return generateJsonWebKeyCommand;
        }

        private static readonly string[] AllowedOutputTransforms =
            [.. Enum.GetValues<OutputTransformType>().Select(type => type.ToCamelCase())];

        private static OutputTransformType? ParseOutputTransform(string? value) =>
            string.IsNullOrEmpty(value) ? null : Enum.Parse<OutputTransformType>(value, ignoreCase: true);
    }
}