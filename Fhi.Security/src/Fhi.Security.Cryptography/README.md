# Fhi.Security.Cryptography

A .NET library for secure key management.  
Currently, it provides functionality for generating **JSON Web Keys (JWKs)** with cryptographic standards supported by HelseId and Maskinporten.

---

## Features

- Generate RSA and EC JWK key pairs (public and private).
- Supports configurable signing algorithms (default: `PS512`).
- Supports key usages (`sig`, `enc`).
- Automatic `kid` (Key ID) generation via JWK thumbprint or optional custom kid value.
- Public/Private JWK separation with custom serialization.

---

## Installation

Add the package via NuGet:

```bash
dotnet add package Fhi.Security.Cryptography
```

Or reference it in your `.csproj`:

```xml
<PackageReference Include="Fhi.Security.Cryptography" Version="x.y.z" />
```

---

## Usage

### Generate a JWK Key Pair

```csharp
using Fhi.Security.Cryptography.Jwks;

// Create a new JWK key pair
var jwkPair = JWK.Create();

// Access public and private JWK JSON
Console.WriteLine("Public JWK: " + jwkPair.PublicKey);
Console.WriteLine("Private JWK: " + jwkPair.PrivateKey);
```

### Customize Parameters

```csharp
var jwkPair = JWK.Create(
    signingAlgorithm: Microsoft.IdentityModel.Tokens.SecurityAlgorithms.EcdsaSha256,
    keyUse: "sig",
    kid: "custom-key-id"
);
```

Supported signing algorithms (`JWK.SupportedSigningAlgorithms`), default `PS512`:

| Algorithm | Key |
|-----------|-----|
| `PS256`, `PS384`, `PS512` | RSA 4096 |
| `ES256`, `ES384`, `ES512` | EC P-256, P-384, P-521 |
| `RS256`, `RS384`, `RS512` | RSA 4096 — rejected by HelseID from 11 January 2027 |

---

## Code Overview

| Class / Method | Description |
|----------------|-------------|
| `JwkKeyPair`   | Record holding `PublicKey` and `PrivateKey` JSON strings. |
| `JWK.Create()` | Generates a new JWK key pair with optional parameters. |
| `PublicJsonWebKeyConverter` | Custom converter ensuring only public JWK fields are serialized. |

---

## Contributing

Contributions are welcome!  
Please see the repository guide for contributions.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
