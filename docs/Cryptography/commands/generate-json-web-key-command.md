# Generate new Json Web keys Commands

## Parameters

|Parameter name | Short Name| Description																					                                                                                                       | Required | Sample						|
|---------------|------------------------|----------------------------------------------------------------------------------------------------------------------------------------|----------|-------------------------------|
|--KeyFileNamePrefix|-n| Prefix of name of the public and private key file. <br> The keys will be named `<FileName>_private.json` and `<FileName>_public.json`. |<b>Yes</b>|`"newKey"`|
|--KeyDirectory|-d| Path to where private and public key will be stored.                                                                                   |No|`"C:\\temp"`|
|--KeyCustomKid|-k| Optional value to set a custom Kid inside the keys.                                                                                   |No|`"1234567890"`|

## Commands
```
 generatejsonwebkey --KeyFileNamePrefix <NAME> --KeyDirectory <PATH> --KeyCustomKid <CUSTOMKID>
```