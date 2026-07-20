param()

$ErrorActionPreference = 'Stop'

$installDirectory = Join-Path $env:LOCALAPPDATA 'ProjectBrain\RdpProtocol'
$handlerSource = Join-Path $PSScriptRoot 'ProjectBrain.RdpHandler.ps1'
$handlerDestination = Join-Path $installDirectory 'ProjectBrain.RdpHandler.ps1'
$protocolKey = 'HKCU:\Software\Classes\projectbrain-rdp'
$powershellPath = Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
$mstscPath = Join-Path $env:SystemRoot 'System32\mstsc.exe'

if (-not (Test-Path -LiteralPath $handlerSource)) {
    throw "找不到协议处理脚本：$handlerSource"
}

New-Item -ItemType Directory -Path $installDirectory -Force | Out-Null
Copy-Item -LiteralPath $handlerSource -Destination $handlerDestination -Force

New-Item -Path $protocolKey -Force | Out-Null
Set-Item -Path $protocolKey -Value 'URL:Project Brain Remote Desktop Protocol'
New-ItemProperty -Path $protocolKey -Name 'URL Protocol' -Value '' -PropertyType String -Force | Out-Null

$iconKey = Join-Path $protocolKey 'DefaultIcon'
New-Item -Path $iconKey -Force | Out-Null
Set-Item -Path $iconKey -Value "`"$mstscPath`",0"

$commandKey = Join-Path $protocolKey 'shell\open\command'
New-Item -Path $commandKey -Force | Out-Null
$command = "`"$powershellPath`" -NoLogo -NoProfile -NonInteractive -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$handlerDestination`" `"%1`""
Set-Item -Path $commandKey -Value $command

Write-Host 'Project Brain 一键远程协议安装成功。'
Write-Host '返回网页点击“一键远程”；浏览器首次询问时请选择允许打开。'
