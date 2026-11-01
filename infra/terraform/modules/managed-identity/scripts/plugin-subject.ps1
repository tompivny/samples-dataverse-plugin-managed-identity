# Builds the version 2 federated credential subject for a Dataverse plug-in.
# Used by the Terraform "external" data source: JSON query on stdin, JSON result on stdout.
# Needs PowerShell 7 (pwsh) on Windows, Linux or macOS.
$ErrorActionPreference = 'Stop'

$query = [Console]::In.ReadToEnd() | ConvertFrom-Json

# Encoded tenant ID: GUID bytes (first three groups little-endian) as base64url without padding.
$tenantBytes = ([guid]$query.tenant_id).ToByteArray()
$encodedTenant = [Convert]::ToBase64String($tenantBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')

# Certificate hash: SHA-256 of the DER bytes as lowercase hex.
$certBytes = [Convert]::FromBase64String($query.certificate_b64)
$hash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($certBytes)).ToLowerInvariant()

$subject = "/eid1/c/pub/t/$encodedTenant/a/qzXoWDkuqUa3l6zM5mM0Rw/n/plugin/e/$($query.environment_id)/h/$hash"
@{ subject = $subject } | ConvertTo-Json -Compress
