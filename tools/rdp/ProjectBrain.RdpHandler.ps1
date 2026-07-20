param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$ProtocolUri
)

$ErrorActionPreference = 'Stop'

function Show-ProtocolError {
    param([string]$Message)

    Add-Type -AssemblyName PresentationFramework
    [System.Windows.MessageBox]::Show(
        $Message,
        'Project Brain 远程桌面',
        [System.Windows.MessageBoxButton]::OK,
        [System.Windows.MessageBoxImage]::Warning
    ) | Out-Null
}

function Get-QueryParameters {
    param([System.Uri]$Uri)

    $result = @{}
    foreach ($part in $Uri.Query.TrimStart('?').Split('&', [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $pair = $part.Split('=', 2)
        $name = [System.Uri]::UnescapeDataString($pair[0])
        $value = if ($pair.Length -eq 2) {
            [System.Uri]::UnescapeDataString($pair[1].Replace('+', ' '))
        } else {
            ''
        }
        $result[$name] = $value
    }
    return $result
}

try {
    $uri = [System.Uri]$ProtocolUri
    if ($uri.Scheme -ne 'projectbrain-rdp' -or $uri.Host -ne 'connect') {
        throw '无效的远程桌面链接。'
    }

    $parameters = Get-QueryParameters -Uri $uri
    $address = ([string]$parameters['address']).Trim()
    if ([string]::IsNullOrWhiteSpace($address) -or $address.Length -gt 255) {
        throw '远程桌面地址为空或过长。'
    }

    $ipAddress = $null
    $isIpAddress = [System.Net.IPAddress]::TryParse($address, [ref]$ipAddress)
    $isHostName = $address -match '^[A-Za-z0-9_](?:[A-Za-z0-9._-]{0,253}[A-Za-z0-9_])?$'
    if (-not $isIpAddress -and -not $isHostName) {
        throw '远程桌面地址格式不正确。'
    }

    $port = 3389
    $portText = ([string]$parameters['port']).Trim()
    if (-not [string]::IsNullOrWhiteSpace($portText)) {
        if (-not [int]::TryParse($portText, [ref]$port) -or $port -lt 1 -or $port -gt 65535) {
            throw '远程桌面端口必须是 1 到 65535 之间的数字。'
        }
    }

    $displayAddress = if ($isIpAddress -and $ipAddress.AddressFamily -eq [System.Net.Sockets.AddressFamily]::InterNetworkV6) {
        "[$address]"
    } else {
        $address
    }
    $target = "${displayAddress}:$port"
    $mstsc = Join-Path $env:SystemRoot 'System32\mstsc.exe'
    Start-Process -FilePath $mstsc -ArgumentList @("/v:$target")
} catch {
    Show-ProtocolError -Message $_.Exception.Message
    exit 1
}
