using Fhi.Security.Cryptography.Certificates;

namespace Fhi.Security.Cryptography.CLI.Commands.GenerateCertificate
{
    internal record UpdateGenerateCertificateOptionNames(string Long, string Short);

    internal static class GenerateCertificateParameterNames
    {
        public const string CommandName = "generatecertificate";

        // Option names
        public static readonly UpdateGenerateCertificateOptionNames CertificateCommonName = new("CertificateCommonName", "cn");
        public static readonly UpdateGenerateCertificateOptionNames CertificatePassword = new("CertificatePassword", "pwd");
        public static readonly UpdateGenerateCertificateOptionNames CertificateDirectory = new("CertificateDirectory", "dir");
        public static readonly UpdateGenerateCertificateOptionNames ValidityMonths = new("ValidityMonths", "vm");
    }

    /// <summary>
    /// Parameters for generating exportable certificates with RSA algorithm with 4096-bit key length
    /// </summary>
    internal class GenerateCertificateParameters
    {
        /// <summary>
        /// Common name of certificates
        /// </summary>
        public required string CertificateCommonName { get; set; }

        /// <summary>
        /// Password for the certificate
        /// </summary>
        public required string CertificatePassword { get; set; }

        /// <summary>
        /// Directory where public and private certificates will be stored
        /// </summary>
        public string? CertificateDirectory { get; set; }

        /// <summary>
        /// Number of months the certificate is valid (default: 24)
        /// </summary>
        public int ValidityMonths { get; set; } = Certificate.DefaultValidityMonths;
    };
}
