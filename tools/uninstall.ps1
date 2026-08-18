# Désinstalle YvanMonkapp : arrêt, démarrage automatique, raccourci, fichiers.
# Le score est conservé par défaut ; -Purge efface aussi %LOCALAPPDATA%\YvanMonkapp.

param([switch]$Purge)

$ErrorActionPreference = 'Stop'

$target = Join-Path $env:LOCALAPPDATA 'Programs\YvanMonkapp'
$data = Join-Path $env:LOCALAPPDATA 'YvanMonkapp'
$startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\YvanMonkapp.lnk'
$runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'

Get-Process YvanMonkapp -ErrorAction SilentlyContinue | ForEach-Object {
    $_ | Stop-Process -Force
    Start-Sleep -Milliseconds 800
}

if (Get-ItemProperty -Path $runKey -Name "Yvan Monk'app" -ErrorAction SilentlyContinue) {
    Remove-ItemProperty -Path $runKey -Name "Yvan Monk'app"
    Write-Host 'démarrage automatique retiré'
}

if (Test-Path $startMenu) { Remove-Item $startMenu -Force; Write-Host 'raccourci retiré' }
if (Test-Path $target) { Remove-Item $target -Recurse -Force; Write-Host 'application retirée' }

if ($Purge) {
    if (Test-Path $data) { Remove-Item $data -Recurse -Force; Write-Host 'score et réglages effacés' }
} elseif (Test-Path $data) {
    Write-Host "score conservé dans $data (relancez avec -Purge pour l'effacer)"
}

Write-Host "Yvan Monk'app est désinstallé." -ForegroundColor Green
