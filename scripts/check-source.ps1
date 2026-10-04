$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Set-Location -LiteralPath $projectRoot
git diff --check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
git diff --cached --check
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$tracked = git ls-files
$forbidden = @($tracked | Where-Object { $_ -eq '.env' -or $_ -match '(^|/)(\.runtime|node_modules|bin|obj|storage|quarantine|secrets|backups)/' -or $_ -match '\.(pem|pfx)$' })
if ($forbidden.Count) { throw 'Generated data or secrets are tracked. Remove them before committing.' }
$leaks = rg --files-with-matches --hidden --glob '!.git/**' --glob '!.runtime/**' --glob '!**/node_modules/**' --glob '!**/bin/**' --glob '!**/obj/**' --glob '!.env' --glob '!scripts/check-source.ps1' 'npg_[A-Za-z0-9]{10,}|-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----' .
if ($LASTEXITCODE -eq 0) { throw 'Potential secret detected in source. Inspect locally; values are not printed.' }
if ($LASTEXITCODE -gt 1) { throw 'Secret scan failed to execute.' }
Write-Output 'Source whitespace, ignored-data and basic secret checks passed.'
exit 0
