$clientId = ""
$newKey = ""
$oldKey =""
$authority = "https://helseid-sts.test.nhn.no"
$baseAddress = "https://api.selvbetjening.test.nhn.no"


$result = & helseid-cli updateclientkey --ClientId $clientId --NewClientJwk $newKey --OldClientJwk $oldKey --AuthorityUrl $authority --BaseAddress $baseAddress

Write-Host $result

   