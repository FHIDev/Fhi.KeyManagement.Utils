using Fhi.Security.Cryptography.Jwks;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.Security.Cryptography.CLI.Commands.GenerateJsonWebKey
{
    /// <summary>
    /// Represents long and short option names for command-line arguments.
    /// </summary>
    /// <param name="Long">The long form option name</param>
    /// <param name="Short">The short form option name</param>
    internal record GenerateJsonWebKeyOptionNames(string Long, string Short);

    internal static class GenerateJsonWebKeyParameterNames
    {
        public const string CommandName = "generatejsonwebkey";
        public static readonly GenerateJsonWebKeyOptionNames KeyFileNamePrefix = new("KeyFileNamePrefix", "n");
        public static readonly GenerateJsonWebKeyOptionNames KeyDirectory = new("KeyDirectory", "d");
        public static readonly GenerateJsonWebKeyOptionNames KeyCustomKid = new("KeyCustomKid", "k");
        public static readonly GenerateJsonWebKeyOptionNames OutputTransform = new("OutputTransform", "ot");
        public static readonly GenerateJsonWebKeyOptionNames PrivateKeyOutputTransform = new("PrivateKeyOutputTransform", "pot");
        public static readonly GenerateJsonWebKeyOptionNames PublicKeyOutputTransform = new("PublicKeyOutputTransform", "put");
        public static readonly GenerateJsonWebKeyOptionNames SigningAlgorithm = new("SigningAlgorithm", "a");
    }

    /// <summary>
    /// Parameters for generating Client private and public Json web keys
    /// </summary>
    internal class GenerateJsonWebKeyParameters
    {
        /// <summary>
        /// Prefix name of the file
        /// </summary>
        public required string KeyFileNamePrefix { get; set; }

        /// <summary>
        /// Directory where public and private file will be stored
        /// </summary>
        public string? KeyDirectory { get; set; }

        /// <summary>
        /// Custom Kid thats present on both public and private key
        /// </summary>
        public string? KeyCustomKid { get; set; }

        /// <summary>
        /// Output transform for the private key: JsonEscape (default), Base64 or None
        /// </summary>
        public OutputTransformType PrivateKeyOutputTransform { get; set; } = OutputTransformType.JsonEscape;

        /// <summary>
        /// Output transform for the public key: JsonEscape (default), Base64 or None
        /// </summary>
        public OutputTransformType PublicKeyOutputTransform { get; set; } = OutputTransformType.JsonEscape;

        /// <summary>
        /// Signing algorithm, one of <see cref="JWK.SupportedSigningAlgorithms"/>
        /// </summary>
        public string SigningAlgorithm { get; set; } = SecurityAlgorithms.RsaSsaPssSha512;
    };
}
