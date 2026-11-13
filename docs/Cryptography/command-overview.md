# Cryptography Tool (Jwk)

Fhi Cryptography Tool is a console application that allows for generation of secure keys and certificates. These Keys and certificates are supported by multiple different OpenId Connect Providers. The tool provides commands for:

- **Generation of new keys** 
    - Command for generating new private and public Json Web Keys (jwk). See [key generation commands](./commands/generatenewjsonwebkeys.md)
    - Command for generating new private and public keys in certificate format. See [certificate generation commands](./commands/generatecertificates.md)

### Prerequisites

.NET SDK version 9.x or 10.x is required to install and use the Fhi.Cryptography tool.

To run the tool without installing it, use `dnx` (requires .NET SDK 10.x):

```powershell
dnx Fhi.Security.Cryptography.CLI --yes generatejsonwebkey -n helsedata --OutputTransform base64
```
