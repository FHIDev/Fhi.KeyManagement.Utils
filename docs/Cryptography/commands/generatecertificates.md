# Generate keys in certificate format Command

## Run without installing (dnx)

Requires .NET SDK 10.x. `dnx` downloads and runs the tool without installing it.

```powershell
dnx Fhi.Security.Cryptography.CLI --yes generatecertificate -cn CertName -pwd <PASSWORD> -dir C:\Certificates
```

## Install as global tool

Requires .NET SDK 9.x or 10.x.

```powershell
dotnet tool install --global Fhi.Security.Cryptography.CLI
```

## Example

```powershell
$certificateCommonName = "CertName"
$certificatePassword = "CertPw123!"
$certificateDirectory = "C:\Certificates\test1"

fhi-cryptography generatecertificate --CertificateCommonName $certificateCommonName --CertificatePassword $certificatePassword --CertificateDirectory $certificateDirectory
```

## Command

```
fhi-cryptography generatecertificate --CertificateCommonName <NAME> --CertificatePassword <PASSWORD> --CertificateDirectory <PATH> [--ValidityMonths <MONTHS>]
```

By default, the certificate is valid for **24 months**. Use `ValidityMonths` to customize the validity period.

## Parameters

| Parameter name | Short name | Description | Required | Sample |
|---|---|---|---|---|
| --CertificateCommonName | -cn | Common name of the certificate, key file name prefix. <br><br>Files generated: <br>`<CommonName>_private.pfx`<br>`<CommonName>_public.pem`<br>`<CommonName>_thumbprint.txt` | **Yes** | `"newCertificate"` |
| --CertificatePassword | -pwd | Password for the private certificate. | **Yes** | `"CertPassword123!"` |
| --CertificateDirectory | -dir | Path to where private and public key will be stored. | No | `"C:\temp"` |
| --ValidityMonths | -vm | Number of months the certificate is valid. Default: 24 | No | `18` |
