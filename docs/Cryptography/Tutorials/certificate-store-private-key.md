# Loading a private key from the Windows certificate store

This guide covers how to install a certificate in the Windows certificate store and load the private key into `IConfiguration` using `Fhi.Security.Cryptography`.

Typical use cases:

- **Client assertion** — signing JWTs for authentication against HelseID or Maskinporten
- **Encryption** — encrypting or decrypting data at rest or in transit

---

## Prerequisites

```xml
<PackageReference Include="Fhi.Security.Cryptography" Version="x.y.z" />
```

---

## Step 1 — Generate a certificate

Use `Fhi.Security.Cryptography.CLI` to generate a self-signed certificate. See [](../commands/generatecertificates.ipynb)

This produces two files:

| File | Contents |
|---|---|
| `MyApp.pfx` | Private + public key (PKCS#12), password-protected |
| `MyApp.pem` | Public certificate (upload to HelseID/Maskinporten) |


---

## Step 2 — Install the certificate in the Windows certificate store

### Using powershell
Open PowerShell **as administrator** (LocalMachine) or as yourself (CurrentUser):

```powershell
# CurrentUser\My — recommended for applications running as your own user
$cert = Import-PfxCertificate `
    -FilePath ".\MyApp.pfx" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -Password (ConvertTo-SecureString "MyPassword" -AsPlainText -Force) `
    -Exportable   # required so that .NET can export the private key

$cert.Thumbprint   # copy this value
```

> **Important:** The `-Exportable` flag is required. Without it .NET cannot export the private key and the configuration value will never be populated.

For services running as `NETWORK SERVICE` or a dedicated service account:

```powershell
# LocalMachine\My — for Windows services and IIS
Import-PfxCertificate `
    -FilePath ".\MyApp.pfx" `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -Password (ConvertTo-SecureString "MyPassword" -AsPlainText -Force) `
    -Exportable
```

### Using Windows mmc certificate snap-in

See [Import the certificate into the local computer store](https://learn.microsoft.com/en-us/troubleshoot/windows-server/certificates-and-public-key-infrastructure-pki/install-imported-certificates?source=recommendations#import-the-certificate-into-the-local-computer-store)

---

## Step 3 — Grant the service account read access (LocalMachine only)

If the certificate is installed in `LocalMachine\My` and the service runs as a non-administrator account:

```powershell
$thumbprint    = "<your-thumbprint>"
$serviceAccount = "DOMAIN\MyServiceAccount"   # or "IIS AppPool\MyAppPool"

$cert     = Get-Item "Cert:\LocalMachine\My\$thumbprint"
$keyPath  = $cert.PrivateKey.CspKeyContainerInfo.UniqueKeyContainerName
$fullPath = "$env:ProgramData\Microsoft\Crypto\RSA\MachineKeys\$keyPath"

$acl = Get-Acl $fullPath
$acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule(
    $serviceAccount, "Read", "Allow")))
Set-Acl $fullPath $acl
```

---

## Step 4 — Load the private key into IConfiguration

### Single key (most common)

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddPrivateKeyFromCertificateStore(
        configKey:     "HelseId:PrivateKey",
        thumbprint:    "AABBCCDDEEFF...",           // from step 2
        keyUse:        CertificateKeyUse.Signing,   // default
        storeLocation: StoreLocation.CurrentUser,
        storeName:     StoreName.My)
    .Build();

// The key is now available as a PEM string
string? privateKeyPem = config["HelseId:PrivateKey"];
```

### Multiple clients

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddPrivateKeyFromCertificateStore(source =>
    {
        source.Add("HelseId:PrivateKey",       "AABB1122...", CertificateKeyUse.Signing);
        source.Add("Maskinporten:PrivateKey",  "CCDD3344...", CertificateKeyUse.Signing);
        source.Add("Encryption:PrivateKey",    "EEFF5566...", CertificateKeyUse.Encryption);
    })
    .Build();
```

### With dependency injection

```csharp
// Program.cs
builder.Configuration.AddPrivateKeyFromCertificateStore(
    configKey:  "HelseId:PrivateKey",
    thumbprint: builder.Configuration["HelseId:CertThumbprint"]!);

// Consume in a service
public class TokenService(IConfiguration config)
{
    public void Configure()
    {
        var pem = config["HelseId:PrivateKey"];
        // load with RSA.ImportFromPem(pem) or IdentityModel
    }
}
```

### Logging skipped certificates

When a certificate cannot be loaded, the entry is silently skipped and the config key is left absent.
Connect the `onValidationError` callback to surface the reason through your application logger:

```csharp
// Program.cs — ILogger not yet available, use a deferred approach
var warnings = new List<CertificateLoadDiagnostic>();

builder.Configuration.AddPrivateKeyFromCertificateStore(
    configKey:         "HelseId:PrivateKey",
    thumbprint:        builder.Configuration["HelseId:CertThumbprint"]!,
    onValidationError: warnings.Add);

var app = builder.Build();

// Now ILogger is available — flush any warnings collected during startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
foreach (var d in warnings)
    logger.LogWarning("Certificate not loaded [{ConfigKey}] '{Identifier}': {Reason}",
        d.ConfigKey, d.Identifier, d.Reason);
```

---

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Config value is `null`, warning says "not found" | Thumbprint not found in store | Check store location (`CurrentUser` vs `LocalMachine`) and that the thumbprint is uppercase with no spaces |
| `CryptographicException: The requested operation is not supported` | Certificate installed without `-Exportable` | Reinstall with the `-Exportable` flag |
| Config value is `null`, warning says "expired" | Certificate has passed its `NotAfter` date | Renew the certificate and reinstall |
| Config value is `null`, warning says "not valid until" | Certificate `NotBefore` is in the future | Check the system clock or the certificate validity period |
| Config value is `null`, warning says "missing required KeyUsage" | Certificate does not have `DigitalSignature` (signing) or `KeyEncipherment` (encryption) | Generate a new certificate with the correct key usage, or omit the KeyUsage extension for self-signed certs |
| Service cannot find certificate (LocalMachine) | Service account lacks read permission on the private key | Follow step 3 |
