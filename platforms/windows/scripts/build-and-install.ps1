<#
.SYNOPSIS
    Build the standalone "Tor-proxy" app as a SINGLE self-contained .exe, sign it
    "by bylka", and install it to %LOCALAPPDATA%\Programs\TorProxy with a Desktop
    shortcut.

.DESCRIPTION
    The Tor binaries (tor.exe, lyrebird.exe) are embedded in the exe and extracted
    to %LOCALAPPDATA%\TorProxy on first run, so the whole tool is one portable file
    that needs neither the .NET runtime nor administrator rights.

    The exe is Authenticode-signed with a self-signed "CN=bylka" code-signing
    certificate (created once in the current user's store). The signature shows
    "bylka" in the file's Digital Signatures tab.

    Pass -FrameworkDependent for a much smaller exe that needs the .NET 8 Desktop
    Runtime installed on the target.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\scripts\install-torapp.ps1
#>
[CmdletBinding()]
param([switch] $FrameworkDependent, [switch] $NoShortcut)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$project  = Join-Path $repoRoot 'src\TorApp\DpiBypass.TorApp.csproj'
$outDir   = Join-Path $repoRoot 'publish-torapp'
$dotnet   = 'C:\Program Files\dotnet\dotnet.exe'

if (Test-Path $outDir) { Remove-Item -Recurse -Force $outDir }
$selfContained = if ($FrameworkDependent) { 'false' } else { 'true' }

& $dotnet publish $project `
    -c Release -r win-x64 --self-contained $selfContained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=None -p:DebugSymbols=false `
    -o $outDir
if ($LASTEXITCODE -ne 0) { throw "publish failed (exit $LASTEXITCODE)." }

$exe = Join-Path $outDir 'Tor-proxy.exe'
if (-not (Test-Path $exe)) { throw "Published exe not found at $exe" }

# --- Digital signature "by bylka" ---
try {
    $cert = Get-ChildItem Cert:\CurrentUser\My -CodeSigningCert -ErrorAction SilentlyContinue |
            Where-Object { $_.Subject -eq 'CN=bylka' } | Select-Object -First 1
    if (-not $cert) {
        $cert = New-SelfSignedCertificate -Subject 'CN=bylka' -Type CodeSigningCert `
                    -CertStoreLocation Cert:\CurrentUser\My -KeyExportPolicy Exportable `
                    -KeyUsage DigitalSignature -FriendlyName 'bylka code signing' `
                    -NotAfter (Get-Date).AddYears(10)
        # Trust it on this machine so the signature validates locally (current user only).
        foreach ($store in @('Root', 'TrustedPublisher')) {
            try {
                $s = New-Object System.Security.Cryptography.X509Certificates.X509Store($store, 'CurrentUser')
                $s.Open('ReadWrite'); $s.Add($cert); $s.Close()
            } catch {}
        }
    }
    $sig = Set-AuthenticodeSignature -FilePath $exe -Certificate $cert -HashAlgorithm SHA256
    Write-Host "Подпись: $($sig.Status) (signer CN=bylka)" -ForegroundColor Green
} catch {
    Write-Host "ВНИМАНИЕ: не удалось подписать exe: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Ship a plain-text README next to the exe for people you hand the file to.
$readmeSrc = Join-Path $repoRoot 'src\TorApp\README.md'
if (Test-Path $readmeSrc) {
    Copy-Item $readmeSrc (Join-Path $outDir 'README.txt') -Force
    Write-Host "README для раздачи: $(Join-Path $outDir 'README.txt')" -ForegroundColor Green
}

$sizeMb = [math]::Round((Get-Item $exe).Length / 1MB, 1)

# Install to a stable per-user location (no admin needed).
$install = Join-Path $env:LOCALAPPDATA 'Programs\TorProxy'
New-Item -ItemType Directory -Force -Path $install | Out-Null
$installedExe = Join-Path $install 'Tor-proxy.exe'
Copy-Item $exe $installedExe -Force

# Clean up artifacts from the old name.
$oldExe = Join-Path $install 'TorProxy.exe'
if (Test-Path $oldExe) { Remove-Item $oldExe -Force -ErrorAction SilentlyContinue }

if (-not $NoShortcut) {
    $desktop = [Environment]::GetFolderPath('Desktop')
    $oldLnk  = Join-Path $desktop 'Тор-Прокси.lnk'
    if (Test-Path $oldLnk) { Remove-Item $oldLnk -Force -ErrorAction SilentlyContinue }

    $lnkPath = Join-Path $desktop 'Tor-proxy.lnk'
    $sh  = New-Object -ComObject WScript.Shell
    $lnk = $sh.CreateShortcut($lnkPath)
    $lnk.TargetPath       = $installedExe
    $lnk.WorkingDirectory = $install
    $lnk.IconLocation     = "$installedExe,0"
    $lnk.Description       = 'Tor-proxy - доступ к заблокированным сайтам через Tor (Snowflake). by bylka'
    $lnk.Save()
    Write-Host "Ярлык на рабочем столе: $lnkPath" -ForegroundColor Green
}

Write-Host "Готово. Один файл: $installedExe ($sizeMb МБ)" -ForegroundColor Green
Write-Host "Портативный exe для раздачи: $exe" -ForegroundColor Green
Write-Host "Прав администратора и .NET-рантайма не требует (self-contained)."
