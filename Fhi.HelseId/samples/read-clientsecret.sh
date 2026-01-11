#!/bin/bash
# Capture exit code and output
output=$(helseid-cli readclientsecretexpiration --ClientId "$CLIENT_ID" --ExistingPrivateJwkPath "$KEY_PATH" 2>&1 --AuthorityUrl "$AUTHORITY_URL" -BaseAddress "$BASE_ADDRESS")
exit_code=$?

if [ $exit_code -eq 0 ]; then
    echo "Output: $output"
    # Extract date from output (assuming format: "Client secret expiration date: 2025-06-27 14:30:00")
    expiration_date=$(echo "$output" | grep -o "[0-9]\{4\}-[0-9]\{2\}-[0-9]\{2\} [0-9]\{2\}:[0-9]\{2\}:[0-9]\{2\}")
    
    if [ ! -z "$expiration_date" ]; then
        echo "Secret expires at: $expiration_date"
        # Calculate days until expiration (requires date command)
        expiry_seconds=$(date -d "$expiration_date" +%s)
        current_seconds=$(date +%s)
        days_until_expiry=$(( ($expiry_seconds - $current_seconds) / 86400 ))
        echo "Days until expiry: $days_until_expiry"
        
        if [ $days_until_expiry -lt 30 ]; then
            echo "WARNING: Secret expires soon - schedule renewal!"
        fi
    fi
else
    echo "Failed to read expiration: $output"
    exit 1
fi
