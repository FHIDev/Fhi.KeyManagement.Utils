# Fhi.Security.Cryptography.CLI

A Command Line Tool for secure key management.  
Currently, it provides functionality for generating **JSON Web Keys (JWKs)** by command line with cryptographic standards supported by HelseId and Maskinporten.

See further documentation in the official [wiki](https://fhidev.github.io/Fhi.KeyManagement.Utils/)

---

## Features

- Generate RSA-based JWK key pairs (public and private).
- Automatic `kid` (Key ID) generation via JWK thumbprint or optional custom kid value.
- Public/Private JWK separation with custom serialization.

---

## Installation

Add the package via NuGet:

```bash
dotnet tool install --global Fhi.Security.Cryptography.CLI --version x.y.x
```

---

## Usage

See further documentation about usage in the official [wiki](https://fhidev.github.io/Fhi.KeyManagement.Utils/)

---

## Contributing

Contributions are welcome!  
Please fsee the repository guide for contributions.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
