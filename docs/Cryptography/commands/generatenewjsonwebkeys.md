# Generate new Json Web keys Command

## Run without installing (dnx)

Requires .NET SDK 10.x. `dnx` downloads and runs the tool without installing it.

```powershell
dnx Fhi.Security.Cryptography.CLI --yes generatejsonwebkey -n <fileName> --OutputTransform base64
```

## Install as global tool

Requires .NET SDK 9.x or 10.x.

```powershell
dotnet tool install --global Fhi.Security.Cryptography.CLI
```

## Examples

```powershell
$keyFileName = "KeyName"
$keyDirectory = "C:\Keys\test1"
$keyCustomKid = "CustomKid1234"

# Generate keys as JSON files (default)
fhi-cryptography generatejsonwebkey --KeyFileNamePrefix $keyFileName --KeyDirectory $keyDirectory --KeyCustomKid $keyCustomKid

# Generate keys as base64-encoded content
fhi-cryptography generatejsonwebkey --KeyFileNamePrefix $keyFileName --KeyDirectory $keyDirectory --OutputTransform base64

# Generate private key base64-encoded and public key as raw JSON
fhi-cryptography generatejsonwebkey --KeyFileNamePrefix $keyFileName --KeyDirectory $keyDirectory --OutputTransform base64 --PublicKeyOutputTransform none

# Generate EC keys (ES256) instead of the default PS512
fhi-cryptography generatejsonwebkey --KeyFileNamePrefix $keyFileName --KeyDirectory $keyDirectory --SigningAlgorithm ES256
```

## Command

```
generatejsonwebkey --KeyFileNamePrefix <NAME> --KeyDirectory <PATH> --KeyCustomKid <CUSTOMKID> --OutputTransform <TRANSFORM> --PrivateKeyOutputTransform <TRANSFORM> --PublicKeyOutputTransform <TRANSFORM> --SigningAlgorithm <ALGORITHM>
```

## Parameters

| Parameter name | Short name | Description | Required | Sample |
|---|---|---|---|---|
| --KeyFileNamePrefix | -n | Prefix of name of the public and private key file. <br> The keys will be named `<FileName>_private.json` and `<FileName>_public.json`. | **Yes** | `"newKey"` |
| --KeyDirectory | -d | Path to where private and public key will be stored. | No | `"C:\temp"` |
| --KeyCustomKid | -k | Optional value to set a custom Kid inside the keys. | No | `"1234567890"` |
| --OutputTransform | -ot | Output transform for both keys: `jsonEscape` (default), `base64` for base64-encoded content or `none` for raw JSON. When `base64`, files are saved as `.txt` instead of `.json`. | No | `"base64"` |
| --PrivateKeyOutputTransform | -pot | Output transform for the private key. Same values as `--OutputTransform`, which it overrides. | No | `"base64"` |
| --PublicKeyOutputTransform | -put | Output transform for the public key. Same values as `--OutputTransform`, which it overrides. | No | `"none"` |
| --SigningAlgorithm | -a | Signing algorithm (default: `PS512`). One of: `PS256`, `PS384`, `PS512`, `ES256`, `ES384`, `ES512`, `RS256`, `RS384`, `RS512`. RS* is rejected by HelseID from 11 January 2027. | No | `"ES256"` |
