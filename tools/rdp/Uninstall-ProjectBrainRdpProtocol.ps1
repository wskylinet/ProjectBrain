param()

$ErrorActionPreference = 'Stop'

$protocolKey = 'HKCU:\Software\Classes\projectbrain-rdp'
$installDirectory = Join-Path $env:LOCALAPPDATA 'ProjectBrain\RdpProtocol'

if (Test-Path -LiteralPath $protocolKey) {
    Remove-Item -LiteralPath $protocolKey -Recurse -Force
}
if (Test-Path -LiteralPath $installDirectory) {
    Remove-Item -LiteralPath $installDirectory -Recurse -Force
}

Write-Host 'Project Brain 一键远程协议已卸载。'
