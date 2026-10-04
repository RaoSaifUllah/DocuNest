$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Set-Location -LiteralPath $projectRoot
$secretRoot = Join-Path $projectRoot '.runtime/secrets'
New-Item -ItemType Directory -Force -Path $secretRoot | Out-Null
$jwtPath = Join-Path $secretRoot 'jwt.pem'
$keyPath = Join-Path $secretRoot 'encryption.key'
$tokenPath = Join-Path $secretRoot 'bootstrap.token'
$certPath = Join-Path $secretRoot 'localhost-cert.pem'
$certKeyPath = Join-Path $secretRoot 'localhost-key.pem'
if (!(Test-Path -LiteralPath $certPath) -or !(Test-Path -LiteralPath $certKeyPath)) {
  $certRsa = [Security.Cryptography.RSA]::Create(3072)
  try {
    $request = [Security.Cryptography.X509Certificates.CertificateRequest]::new('CN=localhost',$certRsa,[Security.Cryptography.HashAlgorithmName]::SHA256,[Security.Cryptography.RSASignaturePadding]::Pkcs1)
    $san = [Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder]::new()
    $san.AddDnsName('localhost'); $san.AddIpAddress([Net.IPAddress]::Parse('127.0.0.1'))
    $request.CertificateExtensions.Add($san.Build())
    $certificate = $request.CreateSelfSigned([DateTimeOffset]::UtcNow.AddMinutes(-5),[DateTimeOffset]::UtcNow.AddMonths(6))
    try { [IO.File]::WriteAllText($certPath,$certificate.ExportCertificatePem()); [IO.File]::WriteAllText($certKeyPath,$certRsa.ExportPkcs8PrivateKeyPem()) } finally { $certificate.Dispose() }
  } finally { $certRsa.Dispose() }
}
if (!(Test-Path -LiteralPath $jwtPath)) {
  $rsa = [Security.Cryptography.RSA]::Create(3072)
  try { [IO.File]::WriteAllText($jwtPath,$rsa.ExportPkcs8PrivateKeyPem()) } finally { $rsa.Dispose() }
}
if (!(Test-Path -LiteralPath $keyPath)) { [IO.File]::WriteAllBytes($keyPath,[Security.Cryptography.RandomNumberGenerator]::GetBytes(32)) }
if (!(Test-Path -LiteralPath $tokenPath)) {
  $token = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32)).TrimEnd('=').Replace('+','-').Replace('/','_')
  [IO.File]::WriteAllText($tokenPath,$token)
}
$envPath = Join-Path $projectRoot '.env'
if (!(Test-Path -LiteralPath $envPath)) { Copy-Item -LiteralPath (Join-Path $projectRoot '.env.example') -Destination $envPath }
$content = [IO.File]::ReadAllText($envPath)
if ($content -notmatch '(?m)^DocuNest__BootstrapTokenHash=') {
  $token = [IO.File]::ReadAllText($tokenPath)
  $hash = [Convert]::ToHexStringLower([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($token)))
  [IO.File]::AppendAllText($envPath,"`nDocuNest__BootstrapTokenHash=$hash`n")
}
Write-Output 'Local signing/encryption keys and one-time bootstrap token are ready in ignored .runtime/secrets. No account or notification was created.'
