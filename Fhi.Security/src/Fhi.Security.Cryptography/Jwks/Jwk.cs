using System.Security.Cryptography;
using System.Text.Json;
using Fhi.Security.Cryptography.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace Fhi.Security.Cryptography.Jwks
{
    /// <summary>
    /// Generate Json Web Keys
    /// </summary>
    /// <param name="PublicKey">Public key</param>
    /// <param name="PrivateKey">Public and private key</param>
    public record JwkKeyPair(string PublicKey, string PrivateKey);

    /// <summary>
    /// Output transform types for JWK key pair serialization.
    /// </summary>
    public enum OutputTransformType
    {
        JsonEscape,
        Base64
    }

    public static class OutputTransformTypeExtensions
    {
        public static string ToCamelCase(this OutputTransformType type)
        {
            var name = type.ToString();
            return char.ToLowerInvariant(name[0]) + name[1..];
        }
    }

    /// <summary>
    /// Public class that exposes methods to generate Jwk
    /// </summary>
    public static class JWK
    {
        private static readonly string[] AllowedKeyUses = ["sig", "enc"];

        private static readonly Dictionary<string, string> EcCurvesBySigningAlgorithm = new()
        {
            [SecurityAlgorithms.EcdsaSha256] = JsonWebKeyECTypes.P256,
            [SecurityAlgorithms.EcdsaSha384] = JsonWebKeyECTypes.P384,
            [SecurityAlgorithms.EcdsaSha512] = JsonWebKeyECTypes.P521,
        };

        /// <summary>
        /// Signing algorithms supported by <see cref="Create"/>.
        /// HelseID rejects RS* from 11 January 2027; RS* is kept for other providers.
        /// </summary>
        public static IReadOnlyList<string> SupportedSigningAlgorithms { get; } =
        [
            SecurityAlgorithms.RsaSsaPssSha256,
            SecurityAlgorithms.RsaSsaPssSha384,
            SecurityAlgorithms.RsaSsaPssSha512,
            SecurityAlgorithms.EcdsaSha256,
            SecurityAlgorithms.EcdsaSha384,
            SecurityAlgorithms.EcdsaSha512,
            SecurityAlgorithms.RsaSha256,
            SecurityAlgorithms.RsaSha384,
            SecurityAlgorithms.RsaSha512,
        ];

        /// <summary>
        /// Create a new JWK key pair.
        /// Following key requirement https://utviklerportal.nhn.no/informasjonstjenester/helseid/protokoller-og-sikkerhetsprofil/sikkerhetsprofil/docs/vedlegg/krav_til_kryptografi_enmd
        /// </summary>
        /// <param name="signingAlgorithm">Signing algorithm, one of <see cref="SupportedSigningAlgorithms"/> (default: PS512)</param>
        /// <param name="keyUse">Key use (default: "sig")</param>
        /// <param name="kid">Optional key ID (default: computed thumbprint)</param>
        /// <returns>A JwkKeyPair containing public and private JWK JSON</returns>
        public static JwkKeyPair Create(
            string signingAlgorithm = SecurityAlgorithms.RsaSsaPssSha512,
            string keyUse = "sig",
            string? kid = null)
        {
            if (!SupportedSigningAlgorithms.Contains(signingAlgorithm))
            {
                throw new ArgumentException($"Invalid signing algorithm: '{signingAlgorithm}'. Expected one of: {string.Join(", ", SupportedSigningAlgorithms)}", nameof(signingAlgorithm));
            }

            if (!AllowedKeyUses.Contains(keyUse))
            {
                throw new ArgumentException($"Invalid key use: '{keyUse}'. Expected one of: {string.Join(", ", AllowedKeyUses)}", nameof(keyUse));
            }

            var privateJwk = EcCurvesBySigningAlgorithm.TryGetValue(signingAlgorithm, out var curve)
                ? CreateEcJwk(signingAlgorithm, curve, keyUse)
                : CreateRsaJwk(signingAlgorithm, keyUse);

            return ToKeyPair(privateJwk, kid);
        }

        private static JsonWebKey CreateEcJwk(string signingAlgorithm, string curve, string keyUse)
        {
            using var ecdsa = ECDsa.Create(ToECCurve(curve));
            var ecParameters = ecdsa.ExportParameters(true);

            return new JsonWebKey
            {
                Alg = signingAlgorithm,
                Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
                Crv = curve,
                X = Base64UrlEncoder.Encode(ecParameters.Q.X),
                Y = Base64UrlEncoder.Encode(ecParameters.Q.Y),
                D = Base64UrlEncoder.Encode(ecParameters.D),
                Use = keyUse,
            };
        }

        private static ECCurve ToECCurve(string curve) => curve switch
        {
            JsonWebKeyECTypes.P256 => ECCurve.NamedCurves.nistP256,
            JsonWebKeyECTypes.P384 => ECCurve.NamedCurves.nistP384,
            JsonWebKeyECTypes.P521 => ECCurve.NamedCurves.nistP521,
            _ => throw new ArgumentOutOfRangeException(nameof(curve), curve, "Unsupported curve")
        };

        private static JsonWebKey CreateRsaJwk(string signingAlgorithm, string keyUse)
        {
            using var rsa = RSA.Create(4096);
            var rsaParameters = rsa.ExportParameters(true);

            return new JsonWebKey
            {
                Alg = signingAlgorithm,
                Kty = JsonWebAlgorithmsKeyTypes.RSA,
                N = Base64UrlEncoder.Encode(rsaParameters.Modulus),
                E = Base64UrlEncoder.Encode(rsaParameters.Exponent),
                D = Base64UrlEncoder.Encode(rsaParameters.D),
                P = Base64UrlEncoder.Encode(rsaParameters.P),
                Q = Base64UrlEncoder.Encode(rsaParameters.Q),
                DP = Base64UrlEncoder.Encode(rsaParameters.DP),
                DQ = Base64UrlEncoder.Encode(rsaParameters.DQ),
                QI = Base64UrlEncoder.Encode(rsaParameters.InverseQ),
                Use = keyUse,
            };
        }

        private static JwkKeyPair ToKeyPair(JsonWebKey privateJwk, string? kid)
        {
            privateJwk.Kid = string.IsNullOrWhiteSpace(kid)
                ? Base64UrlEncoder.Encode(privateJwk.ComputeJwkThumbprint())
                : kid;

            var publicOptions = new JsonSerializerOptions
            {
                Converters = { new PublicJsonWebKeyConverter() },
                WriteIndented = false
            };

            string publicJwkJson = JsonSerializer.Serialize(privateJwk, publicOptions);
            string privateJwkJson = JsonSerializer.Serialize(privateJwk);

            return new JwkKeyPair(publicJwkJson, privateJwkJson);
        }
    }
}