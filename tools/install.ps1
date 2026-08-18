# Installe YvanMonkapp dans %LOCALAPPDATA%\Programs\YvanMonkapp, pose un raccourci dans le
# menu Démarrer, puis lance l'application.
#
# Le démarrage automatique n'est PAS activé ici : l'application le propose elle-même
# au premier lancement, case à cocher à l'appui.

$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot '..\src\YvanMonkapp\YvanMonkapp.csproj'
$target = Join-Path $env:LOCALAPPDATA 'Programs\YvanMonkapp'
$staging = Join-Path $env:TEMP ('YvanMonkapp-publish-' + [Guid]::NewGuid().ToString('N'))

# Autonome : la copie installée embarque .NET et survit à une désinstallation du SDK.
Write-Host 'Compilation...' -ForegroundColor Cyan
dotnet publish $project -c Release -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $staging --nologo | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'la compilation a échoué' }

Get-Process YvanMonkapp -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host 'Arrêt de la version en cours...' -ForegroundColor Cyan
    $_ | Stop-Process -Force
    Start-Sleep -Milliseconds 800
}

Write-Host "Installation dans $target" -ForegroundColor Cyan
New-Item -ItemType Directory -Force $target | Out-Null
Copy-Item (Join-Path $staging '*') $target -Recurse -Force
Remove-Item $staging -Recurse -Force

$exe = Join-Path $target 'YvanMonkapp.exe'
$startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\Yvan Monk'app.lnk'
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($startMenu)
$link.TargetPath = $exe
$link.WorkingDirectory = $target
$link.IconLocation = $exe
$link.Description = "Yvan Monka vous pose un calcul de temps en temps"
$link.Save()

Write-Host 'Lancement...' -ForegroundColor Cyan
Start-Process $exe

Write-Host ''
Write-Host "Yvan Monk'app est installé." -ForegroundColor Green
Write-Host "  application : $exe"
Write-Host "  raccourci   : $startMenu"
Write-Host "  données     : $env:LOCALAPPDATA\YvanMonkapp"
Write-Host ''
Write-Host "Le démarrage avec Windows est déjà en place ; décochable dans le tableau de bord."
