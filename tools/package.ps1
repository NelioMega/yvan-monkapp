# Fabrique la version à donner à quelqu'un : .NET embarqué, rien à installer en face.
#
#   powershell -ExecutionPolicy Bypass -File tools\package.ps1
#   powershell -ExecutionPolicy Bypass -File tools\package.ps1 -Dossier
#
# Par défaut : un seul YvanMonkapp.exe (le plus simple à envoyer).
# Avec -Dossier : l'exe accompagné de ses DLL, sans auto-extraction au lancement.
# C'est la forme à préférer si un antivirus tatillon rouspète sur le fichier unique.

param(
    [string]$Version = '1.2.0',
    [switch]$Dossier
)

$ErrorActionPreference = 'Stop'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $root 'src\YvanMonkapp\YvanMonkapp.csproj'

$flavour = if ($Dossier) { 'dossier' } else { 'autonome' }
$output = Join-Path $root "publish\$flavour"
$zip = Join-Path $root "publish\YvanMonkapp-$Version-win-x64-$flavour.zip"

$options = @(
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained', 'true',
    "-p:Version=$Version",
    '-o', $output,
    '--nologo'
)

if (-not $Dossier) {
    $options += '-p:PublishSingleFile=true'
    $options += '-p:IncludeNativeLibrariesForSelfExtract=true'
}

if (Test-Path $output) { Remove-Item $output -Recurse -Force }

Write-Host "Publication $flavour (version $Version)..." -ForegroundColor Cyan
dotnet publish $project @options | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'la publication a échoué' }

if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $output '*') -DestinationPath $zip

$exe = Join-Path $output 'YvanMonkapp.exe'
$sizeZip = [Math]::Round((Get-Item $zip).Length / 1MB, 1)
$count = (Get-ChildItem $output -Recurse -File).Count

Write-Host ''
Write-Host 'Prêt.' -ForegroundColor Green
Write-Host "  dossier      : $output ($count fichier(s))"
Write-Host "  zip à donner : $zip ($sizeZip Mo)"

# Contrôle antivirus local, tant qu'à faire.
$defender = 'C:\Program Files\Windows Defender\MpCmdRun.exe'
if (Test-Path $defender) {
    Write-Host ''
    Write-Host 'Analyse Windows Defender...' -ForegroundColor Cyan
    & $defender -Scan -ScanType 3 -File $exe | Select-Object -Last 2
}

Write-Host ''
Write-Host "En face : dézipper, double-cliquer YvanMonkapp.exe. Rien d'autre à installer."
Write-Host "L'exe n'étant pas signé, Windows peut afficher un écran bleu SmartScreen au"
Write-Host "premier lancement : « Informations complémentaires » puis « Exécuter quand même »."
