# Real-world example: Get JWK from HelseID API and check expiration
$clientId = "my-client-id"

# API response comes with escaped quotes - use as-is
$jwkFromApi = '{\"kty\":\"RSA\",\"kid\":\"my-key-2024\",\"d\":\"MIIEowIBAAKCAQEA...\"}'

$authority = "https://helseid-sts.test.nhn.no",
$baseAddress = "https://api.selvbetjening.test.nhn.no"

# Pass API response directly without modification
$result = & helseid-cli readclientsecretexpiration --ClientId $clientId --ExistingPrivateJwk $jwkFromApi --AuthorityUrl $authority -BaseAddress $baseAddress

if ($LASTEXITCODE -eq 0) {
    Write-Host "Secret expiration retrieved: $result"
    
    # Extract date and calculate days until expiry
    if ($result -match "(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})") {
        $expirationDate = [DateTime]::Parse($matches[1])
        $daysUntilExpiry = ($expirationDate - (Get-Date)).Days
        
        Write-Host "Expires: $expirationDate ($daysUntilExpiry days)"
        
        if ($daysUntilExpiry -lt 30) {
            Write-Warning "Secret expires soon - schedule renewal!"
            # Add renewal logic or notification here
        }
    }
} else {
    Write-Error "Failed: $result"
}
