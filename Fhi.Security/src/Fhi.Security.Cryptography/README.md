# Fhi.Security.Cryptography

A .NET library for secure key management.  
Currently, it provides functionality for generating **JSON Web Keys (JWKs)** with cryptographic standards supported by HelseId and Maskinporten.

---

## Features

- Generate RSA-based JWK key pairs (public and private).
- TODO: Supports configurable signing algorithms (default: `RSA SHA-512`).
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
using Fhi.Security.Cryptography;

// Create a new JWK key pair
var jwkPair = JWK.Create();

// Access public and private JWK JSON
Console.WriteLine("Public JWK: " + jwkPair.PublicKey);
Console.WriteLine("Private JWK: " + jwkPair.PrivateKey);
```

### Customize Parameters

```csharp
var jwkPair = JWK.Create(
    signingAlgorithm: Microsoft.IdentityModel.Tokens.SecurityAlgorithms.RsaSha512,
    keyUse: "sig",
    kid: "custom-key-id"
);
```

---

## Code Overview

| Class / Method | Description |
|----------------|-------------|
| `JwkKeyPair`   | Record holding `PublicKey` and `PrivateKey` JSON strings. |
| `JWK.Create()` | Generates a new JWK key pair with optional parameters. |
| `JsonWebKeyGenerator` | Internal helper for RSA JWK generation. |
| `PublicJsonWebKeyConverter` | Custom converter ensuring only public JWK fields are serialized. |

---

## Contributing

Contributions are welcome!  
Please fsee the repository guide for contributions.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
