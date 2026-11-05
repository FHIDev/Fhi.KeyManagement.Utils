using Fhi.Security.Cryptography.Jwks;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Fhi.Security.Cryptography.UnitTests
{
    public class JwkGeneratorTests
    {
        /// <summary>
        /// Generate JWK with default parameters and validate key structure.
        /// </summary>
        [Test]
        public void GIVEN_CreateRsaJwk_WHEN_OnlyDefaultValues_THEN_CreateValidKeyPair()
        {
            var keys = JWK.Create();

            var publicJwk = JsonSerializer.Deserialize<JsonWebKey>(keys.PublicKey);
            var privateJwk = JsonSerializer.Deserialize<JsonWebKey>(keys.PrivateKey);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(keys.PublicKey, Is.Not.Empty, "Missing Public Key.");
                Assert.That(keys.PrivateKey, Is.Not.Empty, "Missing Private Key.");

                Assert.That(publicJwk?.Kty, Is.EqualTo("RSA"), "Wrong/ missing kty.");
                Assert.That(publicJwk?.Use, Is.EqualTo("sig"), "Wrong/ missing use.");
                Assert.That(publicJwk?.Kid, Is.EqualTo(privateJwk?.Kid), "Kid does not match.");
                Assert.That(publicJwk?.N, Is.EqualTo(privateJwk?.N), "N does not match.");
                Assert.That(publicJwk?.E, Is.EqualTo(privateJwk?.E), "E does not match.");
            }
        }

        /// <summary>
        /// Generate JWK with custom kid and verify it is applied.
        /// </summary>
        [Test]
        public void GIVEN_CreateRsaJwk_WHEN_CustomKidValue_THEN_CreateValidKeyPairWithCustomKid()
        {
            var customKid = "custom-key-id";
            var keys = JWK.Create(kid: customKid);

            var publicJwk = JsonSerializer.Deserialize<JsonWebKey>(keys.PublicKey);
            var privateJwk = JsonSerializer.Deserialize<JsonWebKey>(keys.PrivateKey);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(publicJwk?.Kid, Is.EqualTo(customKid), "Incorrect kid.");
                Assert.That(privateJwk?.Kid, Is.EqualTo(customKid), "Incorrect kid.");
            }
        }

        /// <summary>
        /// Generate JWK with invalid key use and expect exception.
        /// </summary>
        [Test]
        public void GIVEN_CreateRsaJwk_WHEN_InvalidKeyUseType_THEN_ThrowException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                JWK.Create(keyUse: "invalid");
            });

            Assert.That(ex.Message, Does.Contain("Invalid key use"), "Wrong or missing exception.");
        }

        /// <summary>
        /// Generate JWK with invalid signing algorithm and expect exception.
        /// </summary>
        [Test]
        public void GIVEN_CreateRsaJwk_WHEN_InvalidSigningAlgorithm_THEN_ThrowException()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                JWK.Create(signingAlgorithm: "none");
            });

            Assert.That(ex.Message, Does.Contain("Invalid signing algorithm"), "Wrong or missing exception.");
        }

        [TestCase(SecurityAlgorithms.RsaSsaPssSha256)]
        [TestCase(SecurityAlgorithms.RsaSsaPssSha384)]
        [TestCase(SecurityAlgorithms.RsaSsaPssSha512)]
        [TestCase(SecurityAlgorithms.RsaSha256)]
        [TestCase(SecurityAlgorithms.RsaSha384)]
        [TestCase(SecurityAlgorithms.RsaSha512)]
        public void GIVEN_Create_WHEN_RsaSigningAlgorithm_THEN_CreateRsaKeyPair(string signingAlgorithm)
        {
            var keys = JWK.Create(signingAlgorithm);

            using var publicJwk = JsonDocument.Parse(keys.PublicKey);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(publicJwk.RootElement.GetProperty("kty").GetString(), Is.EqualTo("RSA"));
                Assert.That(publicJwk.RootElement.GetProperty("alg").GetString(), Is.EqualTo(signingAlgorithm));
                Assert.That(publicJwk.RootElement.TryGetProperty("n", out _), Is.True);
                Assert.That(publicJwk.RootElement.TryGetProperty("e", out _), Is.True);
                Assert.That(publicJwk.RootElement.TryGetProperty("d", out _), Is.False);
            }
        }

        [TestCase(SecurityAlgorithms.EcdsaSha256, "P-256")]
        [TestCase(SecurityAlgorithms.EcdsaSha384, "P-384")]
        [TestCase(SecurityAlgorithms.EcdsaSha512, "P-521")]
        public void GIVEN_Create_WHEN_EcSigningAlgorithm_THEN_CreateEcKeyPair(string signingAlgorithm, string expectedCurve)
        {
            var keys = JWK.Create(signingAlgorithm);

            using var publicJwk = JsonDocument.Parse(keys.PublicKey);
            using var privateJwk = JsonDocument.Parse(keys.PrivateKey);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(publicJwk.RootElement.GetProperty("kty").GetString(), Is.EqualTo("EC"));
                Assert.That(publicJwk.RootElement.GetProperty("alg").GetString(), Is.EqualTo(signingAlgorithm));
                Assert.That(publicJwk.RootElement.GetProperty("crv").GetString(), Is.EqualTo(expectedCurve));
                Assert.That(publicJwk.RootElement.TryGetProperty("x", out _), Is.True);
                Assert.That(publicJwk.RootElement.TryGetProperty("y", out _), Is.True);
                Assert.That(publicJwk.RootElement.TryGetProperty("d", out _), Is.False);
                Assert.That(privateJwk.RootElement.TryGetProperty("d", out _), Is.True);
            }
        }

        [TestCaseSource(typeof(JWK), nameof(JWK.SupportedSigningAlgorithms))]
        public async Task GIVEN_Create_WHEN_SigningTokenWithPrivateKey_THEN_PublicKeyValidatesToken(string signingAlgorithm)
        {
            var keys = JWK.Create(signingAlgorithm);
            var handler = new JsonWebTokenHandler();

            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = "issuer",
                SigningCredentials = new SigningCredentials(new Microsoft.IdentityModel.Tokens.JsonWebKey(keys.PrivateKey), signingAlgorithm)
            });

            var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidIssuer = "issuer",
                ValidateAudience = false,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.JsonWebKey(keys.PublicKey)
            });

            Assert.That(result.IsValid, Is.True, result.Exception?.Message);
        }
    }
}