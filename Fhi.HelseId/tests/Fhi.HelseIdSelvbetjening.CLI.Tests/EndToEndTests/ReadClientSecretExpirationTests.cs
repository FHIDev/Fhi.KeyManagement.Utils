using Fhi.HelseIdSelvbetjening.CLI.Commands.ReadClientSecretExpiration;

namespace Fhi.HelseIdSelvbetjening.CLI.AcceptanceTests
{
    /// <summary>
    /// Manual acceptance tests for the CLI. These tests should be run against test environment.
    /// </summary>
    [TestFixture, Explicit]
    public class ReadClientSecretExpirationTests
    {
        /// <summary>
        /// In order to run this test:
        /// 1. Set directory to where existing (old) keys is stored
        /// 2. Set clientId to a valid test client
        /// 
        /// Note: In order to read secret expiration the nhn:selvbetjening/client scope must be set on the client
        /// 
        /// Setup Instructions:
        /// 1. Configure Test Client ID in this file (replace the clientId below)
        /// 2. Create a TestData directory in the test project root (not in bin folder)
        /// 3. Add your private key file as TestData/oldkey.json
        /// 4. The test will automatically set DOTNET_ENVIRONMENT=Test
        /// 
        /// Expected Results:
        /// - Success: Exit code 0, Output contains "Reading client secret expiration for client"
        /// - Failure: Non-zero exit code with error message
        /// </summary>
        /// <returns></returns>        
        [Test, Explicit("This test reads client secret expiration and should be run manually against test environment.")]
        public async Task ReadClientSecretExpiration_FromPath()
        {
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            /******************************************************************************************
             * Read Client Secret Expiration
             *****************************************************************************************/
            var testProjectDirectory = GetTestProjectDirectory();
            var existingKeyPath = Path.Combine(testProjectDirectory, "AcceptanceTests", "TestData", "oldkey.json");
            var clientId = "88d474a8-07df-4dc4-abb0-6b759c2b99ec"; // Replace with your test client ID
            if (!File.Exists(existingKeyPath))
            {
                Assert.Fail($"Test key file not found at: {existingKeyPath}\n" +
                           $"Please ensure your test client's private key is available at TestData/oldkey.json.\n" +
                           $"Test project directory: {testProjectDirectory}");
            }
            int exitCode = await Program.Main(
            [
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwkPath.Long}", existingKeyPath,
                $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no",
            ]);

            var output = stringWriter.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Reading client secret expiration succeeded");
                Assert.That(output, Does.Contain("Reading client secret expiration for client"), "Output contains expected message");
            }
        }

        /// <summary>
        /// In order to run this test:
        /// 1. Set clientId to a valid test client
        /// 2. Set existingPrivateJwk to a valid private key JSON
        /// 
        /// Note: In order to read secret expiration the nhn:selvbetjening/client scope must be set on the client
        /// 
        /// Setup Instructions:
        /// 1. Configure Test Client ID in this file (replace the clientId below)
        /// 2. Replace the existingPrivateJwk with a valid private key JSON
        /// 3. The test will automatically set DOTNET_ENVIRONMENT=Test
        /// 
        /// Format for the private key:
        /// {
        ///   "kty": "RSA",
        ///   "d": "...",
        ///   "n": "...",
        ///   "e": "AQAB",
        ///   "use": "sig",
        ///   "kid": "..."
        /// }
        /// 
        /// Expected Results:
        /// - Success: Exit code 0, Output contains "Reading client secret expiration for client"
        /// - Failure: Non-zero exit code with error message
        /// </summary>
        /// <returns></returns>
        [Test, Explicit("This test reads client secret expiration using direct key value and should be run manually against test environment.")]
        public async Task ReadClientSecretExpiration_FromDirectKey()
        {
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            /******************************************************************************************
             * Read Client Secret Expiration using direct key value
             *****************************************************************************************/
            var clientId = "20cfbb73-4cb2-4b20-b801-e805da5c14d5"; // Replace with valid test client ID
            var existingPrivateJwk = "{\"alg\":\"PS512\",\"d\":\"GROJzz8Sxj4cZJ8vj8-cvSEM3L4f3Vnr-qd9fBYVdSCdByCxR3IZHBG6rmP4vHeQRFmq5-Y0apSPUiWsj0Dc05W3Gg_ays-hqMSpe7kCrgHTPb2aDIB4l-8rxLokAc7E4_EYbuNQpZCPS_uR2xPPOBzNzjMgV-QPEaXHGkkZcCavlu_o6nhriNjWqm3ubzVkHZNTpA-GFSsm180lgniJvdeedKNKCjxlEGqEPa8sg4fRyg_h6U7heUCv8L3jQHKQvcWDUwfdX_gi4NEYiXjTX-sgmnQfpCsaKscqdZ0Qiyos8WCWcWkYTgkIxm_XectybW5LtK_pwnQPCi93JyQBwML3u3Hx_f4K77Tg_GkfXCC7UO7dyykputOhEPLG44zfPgzy31lw2GHg1bhxP8wJQLFRwmj6Nfu7WvjfDq1MexftJxPvjjpssh4TS6-5hMti0FllGXlcesmfeW6PjBrqbnEi7yXzECuyddjPEEEMPLFuHr5P2FR25YgaSm29-UFTmrvy1gDLXtnaxi1-QEJsCyw2lA7sYvaeTawY18tOoT98uKj93vg7GGVOteCwesDmnZB0iggmUtaykvuSqrMk95_qVpi4ZGuVRLzNUVr06xxqeSzVEOFVdOP_gU-ZGRWIqYSnwZhBHQCdIVpKM-GXUvKYMt9QKjzQTGNeIMzr7VE\",\"dp\":\"hvXJ5U9GDXO4cvgYJN8kP6QT19rQ5HE5xne_YvY1XmKNHHNr--iNi1_2ravSLJfz3sgaOK7Y0q_6ZF9FOXu7Ii1_CssjLyE3OdctOtrztV-0cFysx3gMmUSKdt6O3CT8IS4tDxkUY1r9_cfH5M3MoG5Y35Y-G8kPSoa7lMCaDGLrxbB60NiyxIP2wiGM85IFj63xUCWbyq069RtFCBFWlZayOYTNIW5agMB7EMmF05kUe2N6PoC4Vderv96oYJlh3E-v0gOGsgE0q0tu_JpqlyddW88mwmFrser25-YVpsDH1Jwnh1J5DYVb9ys4oGhmpyj_rH8rvrRgt6Dr71JzCw\",\"dq\":\"moLAljzewjrNbz5hQZveTFFn578CNrn2aES1CPiYheC6uf8kd_XqLsRJeEQlmrugIkOoRKDDevxTmvaYDmCIGl9TRScR_hV9v_9oLqUJzsj5tOZMx6S4rFsYsieJz2pIV6JCS4bNjOz7ZFInrinb3vAgbfEbxWqSFU989MF-6JJEjNWUc6KiauEMiR77Lb0zWiDVNvA9PprSDvIJmS3Nmy0YF7KOiieeT55Prvyv_rwBRQPxlGaDo3OqSQlaksVk09VCs9z2OrE6hh3YsVDFeJf_tKkn2-SPX6tjtxlem_0KVtLwjcnb5pyPPwCp_QyV4Mxo3iSTU9BxhbLjS4uZBw\",\"e\":\"AQAB\",\"kty\":\"RSA\",\"n\":\"whMxivhxWOhB3cPnyNU1eLzyRMazE0f9uEZiDtxfoOTbWgVVQcPLtGLhKuNQbc9KAQag4PSke52ftDWm59uajocI0iM3GhKEECc_f2kJSBZJwGsiSR2J_Rc1cjYybKzidIL5WTxT8vn60QvoBAp4E7YsG5lpLP8fQB3FO2F1NwZmtxELzUBkpgreXEjSzuYZ98KbQBDAp-HjyunKDjXRg_ih7W4fm5WQaPNxjjDrpTHcgTMHp4GYfdQE4VcRAA4QNtj5XZJ0FXMWKIJmI5fokBkrGO7FXBuFUS55EgDpZNA8z-r0FrhXxA-vNJ8TRtR5MbjuEn15XDNRVHtigy3boaSpXInnNsuaJVmiKmfJeVNvTEqJazOuI3T97IsVNoVwptTdViy4GVpVLobMdVyeKC5GWGkvOGhWi2_aRlJSO4TJogs6iKUp3wKQFAKdSBZeBG79tT5neX9lL2QkqdPKNKt0IKwpQ-5Tkjtle84ke-dcb7XqER2DxWp_oky8w8MmkfBE1JpL2M_bl7N2W4QFt46q30awqGqdX97j4PbtR1hNB4mIjLYRuls_1xgf-h9gIqrCzyuKdBzWEEpWHpULHf4TDrS9CG2rnPaeoUnoUB4DJS_lqZmeGWz1dCnyKcCaPu4In-2XyW_gUSYP5rA0nhJfgcwIwooiuVaQZ8-WSSE\",\"p\":\"_1CqGnLnjY_iItbActwOvaLIc5B7E6Kcb9TjPL9V5PfQ4Et--LmjlZmb-IpGMKeKs7OZK9nKs8vd1TJT94YTPfPBx_zaEiMeSIKkb1lHVbVVX4yU-gNa-9zh4f2iIH-i4HDmp84wnMZCf3SGb-CPZ8uDdEUPv2DwKTCtLiZX7fJjsOcm_YGPYDJVP3FIuty1PJHC0HRQE5NLnvIlOWXx92w0lh8DBiqjSkQWRFvUFYpIhoz2YsrHSdwVTvJeLuDTkE8wRkGNMNZomNGlkMvFdLgndGX3e9CpkrJv06PJ2tGEFe6xxs3VmRwCqgey0dRhGM30vAOUENHcsTdWgzaxjw\",\"q\":\"wph5ENjNARC9Ws8-mf-EdeQqh83ysjeqaQxcBiXMrmo8fqObk8LozruVqUGSiK6FdS2pRO7qMztE4CCF_o55Ke0-rD7yWpYnjQZ9Y_J7tPDico6qiCviWZ-rhvW7kERcelA_bpUtO_KZw0pKYC9vSicbhn54l1t8pY-yfQS8Y_3DHukL8h9O9jPW8SpD-3TIBtRL0cNcuTFfB8I2ebylIB-UgFXT_jNBIuhPDdw6y8JO--DhtjyASuRnoSENfrkkMIRtg-qy3UAX4uC_-jjGmaVGTxTd0atjt01ggn3_rpbQ7wnmaXVEM9Cz1n99o8A8rM50C7It9enq7NwlZpeiTw\",\"qi\":\"ZPjAiO2cG-qHhaRXqf2qXkCAPIDlikhhsQI5RtEC4IIzXF6RUpfHffRdmDlmGVkIvsyKLNySlppink7uCeTIHxsf34FtYP_QH7U9cKJDb1ImFXeo_MA2Kgrp4y5wMBkv3ml9hhzOTPS7Wn9dc_-DhGGaSXb2EW3xtVTQgJN0ZFW6rrWscVF7dh4v2L1dCvlMFpfabsVAf_rZ2fgi-erGr1dDOlup7MAPBeExw5cnwg6zO7k-4FdRG6RYSRyMfAULC5pDO0DUNsXm_AeXohqXy3czZZ8d_vnKAtwjmoUySLymSB0NNfs8ljg4k7dvxf_yY_iLxuhvyuJkIio87b0A-g\",\"kid\":\"2qFx/g8VE9e2TGrekkhLQ+2W4ChWvEHUtpW7tlRvJk8=\"}"; // Replace with valid private key

            int exitCode = await Program.Main(
            [
                ReadClientSecretExpirationParameterNames.CommandName,
                $"--{ReadClientSecretExpirationParameterNames.ClientId.Long}", clientId,
                $"--{ReadClientSecretExpirationParameterNames.ExistingPrivateJwk.Long}", existingPrivateJwk,
                $"--{ReadClientSecretExpirationParameterNames.AuthorityUrl.Long}", "https://helseid-sts.test.nhn.no",
                $"--{ReadClientSecretExpirationParameterNames.BaseAddress.Long}", "https://api.selvbetjening.test.nhn.no",
            ]);

            var output = stringWriter.ToString();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.EqualTo(0), "Reading client secret expiration succeeded");
                Assert.That(output, Does.Contain("Reading client secret expiration for client"), "Output contains expected message");
            }
        }

        private static string GetTestProjectDirectory()
        {
            // Start from the current directory (bin/Debug/net9.0) and navigate back to the test project root
            var currentDirectory = new DirectoryInfo(Environment.CurrentDirectory);

            // Navigate up until we find the .csproj file or reach a reasonable limit
            while (currentDirectory != null && currentDirectory.Parent != null)
            {
                var csprojFiles = currentDirectory.GetFiles("*.csproj");
                if (csprojFiles.Length > 0)
                {
                    return currentDirectory.FullName;
                }
                currentDirectory = currentDirectory.Parent;
            }

            // Fallback: assume we're in bin/Debug/net9.0 and go up 3 levels
            var fallbackPath = Path.Combine(Environment.CurrentDirectory, "..", "..", "..");
            return Path.GetFullPath(fallbackPath);
        }
    }
}
