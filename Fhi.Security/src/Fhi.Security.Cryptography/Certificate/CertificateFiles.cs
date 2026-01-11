namespace Fhi.Security.Cryptography.Certificate;

public class CertificateFiles
{
    public byte[] CertificatePrivateKey { get; set; } = [];
    public string CertificatePublicKey { get; set; } = string.Empty;
    public string CertificateThumbprint { get; set; } = string.Empty;
}