param([ValidateSet('api','worker','migrate','test')][string]$Action = 'api')
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Set-Location -LiteralPath $projectRoot
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'
if (Test-Path -LiteralPath '.env') {
  foreach ($line in [IO.File]::ReadAllLines((Join-Path $projectRoot '.env'))) {
    if ($line.StartsWith('#') -or [string]::IsNullOrWhiteSpace($line)) { continue }
    $index = $line.IndexOf('=')
    if ($index -gt 0) { [Environment]::SetEnvironmentVariable($line.Substring(0,$index),$line.Substring($index+1),'Process') }
  }
}
$env:DOCUNEST_PROJECT_ROOT = $projectRoot
switch ($Action) {
  'api' { dotnet run --project backend/DocuNest.Api --no-restore -- --urls http://127.0.0.1:5080 }
  'worker' { dotnet run --project backend/DocuNest.Worker --no-restore -- --urls http://127.0.0.1:5081 }
  'migrate' { dotnet run --project backend/DocuNest.Api --no-restore -- --migrate }
  'test' { dotnet test DocuNest.slnx --no-restore }
}
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
