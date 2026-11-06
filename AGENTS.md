# AGENTS.md

CLIs and libraries for HelseID / Maskinporten key and client management (FHI). User docs are mkdocs in `docs/`, published to GitHub Pages.

## Layout
- `Fhi.Security/` — `Fhi.Security.Cryptography` (lib: JWK, certificates, serialization) + `Fhi.Security.Cryptography.CLI` (key/cert generation)
- `Fhi.HelseId/` — `Fhi.HelseIdSelvbetjening` (lib: HelseID selvbetjening API) + `Fhi.HelseIdSelvbetjening.CLI` (read secret expiration, update client key)
- `*/tests/` — `*.UnitTests` and `*.CLI.Tests` per area
- `Samples/` (PowerShell scripts), `docker/`, `docs/`

## Build and test
- `dotnet build Fhi.KeyManagement.Utils.sln`
- `dotnet test` — single test: `dotnet test <proj> --filter "FullyQualifiedName~<Name>"`
- SDK pinned in `global.json`; projects target `net9.0;net10.0`

## Conventions
- Central package management: versions only in `Directory.Packages.props`, never `Version=` in a csproj
- `TreatWarningsAsErrors` and XML docs on public members (`Directory.Build.Props`)
- Nullable enabled; tests use NUnit + NSubstitute
- CLI commands live in `Commands/<Feature>/`: `*CommandBuilder` (implements `ICommandBuilder`, registered in `HostBuilder.cs`), `*Parameters`, `*CommandHandler`. Uses System.CommandLine 2.x
- When CLI options change, update `docs/` and the project README

## Rules
- One issue per PR; don't refactor unrelated code
- Never read or commit secrets, private JWKs/PEMs or real client configuration
