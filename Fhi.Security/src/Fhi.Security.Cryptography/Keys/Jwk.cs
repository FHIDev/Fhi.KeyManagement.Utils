using Fhi.Security.Cryptography.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;

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
        /// <summary>
        /// Create a new JWK key pair.
        /// </summary>
        /// <param name="signingAlgorithm">Signing algorithm (default: RSA SHA-512)</param>
        /// <param name="keyUse">Key use (default: "sig")</param>
        /// <param name="kid">Optional key ID (default: computed thumbprint)</param>
        /// <returns>A JwkKeyPair containing public and private JWK JSON</returns>
        public static JwkKeyPair Create(
            string signingAlgorithm = SecurityAlgorithms.RsaSha512,
            string keyUse = "sig",
            string? kid = null)
        {
            return CreateRsaJwk(signingAlgorithm, keyUse, kid);
        }

        /// <summary>
        /// Generate a Json web key with RSA signing algorithm. Returns both private key and public key 
        /// Following key requirement https://utviklerportal.nhn.no/informasjonstjenester/helseid/protokoller-og-sikkerhetsprofil/sikkerhetsprofil/docs/vedlegg/krav_til_kryptografi_enmd
        /// </summary>
        /// <param name="signingAlgorithm"></param>
        /// <param name="keyUse"></param>
        /// <param name="kid"></param>
        /// <returns></returns>
        private static JwkKeyPair CreateRsaJwk(
            string signingAlgorithm,
            string keyUse,
            string? kid)
        {
            var allowedAlgorithms = new[]
            {
                // TODO: add support for multiple algorithms
                //SecurityAlgorithms.RsaSha256,
                //SecurityAlgorithms.RsaSha384,
                SecurityAlgorithms.RsaSha512
            };
            if (!allowedAlgorithms.Contains(signingAlgorithm))
            {
                throw new ArgumentException($"Invalid signing algorithm: '{signingAlgorithm}'. Expected one of: {string.Join(", ", allowedAlgorithms)}", nameof(signingAlgorithm));
            }

            var allowedKeyUses = new[] { "sig", "enc" };
            if (!allowedKeyUses.Contains(keyUse))
            {
                throw new ArgumentException($"Invalid key use: '{keyUse}'. Expected one of: {string.Join(", ", allowedKeyUses)}", nameof(keyUse));
            }

            using var rsa = RSA.Create(4096);
            var rsaParameters = rsa.ExportParameters(true);

            // Fully qualify JsonWebKey to avoid ambiguity with namespace
            var privateJwk = new JsonWebKey
            {
                Alg = signingAlgorithm,
                Kty = "RSA",
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

            privateJwk.Kid = string.IsNullOrWhiteSpace(kid)
                ? Base64UrlEncoder.Encode(privateJwk.ComputeJwkThumbprint())
                : kid;

            var publicOptions = new JsonSerializerOptions
            {
                // Converter ensures only public key values are added to the public key
                Converters = { new PublicJsonWebKeyConverter() },
                WriteIndented = false
            };

            string publicJwkJson = JsonSerializer.Serialize(privateJwk, publicOptions);
            string privateJwkJson = JsonSerializer.Serialize(privateJwk);

            return new JwkKeyPair(publicJwkJson, privateJwkJson);
        }
    }
}