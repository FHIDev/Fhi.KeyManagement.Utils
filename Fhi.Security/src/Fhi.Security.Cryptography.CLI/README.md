# Fhi.Security.Cryptography.CLI

A Command Line Tool for secure key management.  
Currently, it provides functionality for generating **JSON Web Keys (JWKs)** by command line with cryptographic standards supported by HelseId and Maskinporten.

See further documentation in the official [wiki](https://fhidev.github.io/Fhi.KeyManagement.Utils/)

---

## Prerequisites

.NET SDK version 9.x or 10.x is required to install and use the Fhi.Cryptography tool.

---

## Features

- Generate RSA and EC JWK key pairs (public and private).
- Supports configurable signing algorithms (default: `PS512`).
- Automatic `kid` (Key ID) generation via JWK thumbprint or optional custom kid value.
- Public/Private JWK separation with custom serialization.

---

## Installation

Add the package via NuGet:

```bash
dotnet tool install --global Fhi.Security.Cryptography.CLI --version x.y.x
```

Or run it without installing, using `dnx` (requires .NET SDK 10.x):

```bash
dnx Fhi.Security.Cryptography.CLI --yes generatejsonwebkey -n helsedata --OutputTransform base64
```

---

## Usage

See further documentation about usage in the official [wiki](https://fhidev.github.io/Fhi.KeyManagement.Utils/)

---

## Contributing

Contributions are welcome!  
Please see the repository guide for contributions.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
