param([string]$Updater, [string]$ProjectRoot, [string]$Fault)
$ErrorActionPreference = 'Stop'
$global:Injected = $false
$global:FaultMode = $Fault
$global:ProjectPrefix = $ProjectRoot.Replace('\', '/').TrimEnd('/') + '/'
function global:Copy-Item {
    [CmdletBinding()]
    param([string]$LiteralPath, [string]$Destination, [switch]$Recurse, [switch]$Force)
    $path = $Destination.Replace('\', '/')
    $source = $LiteralPath.Replace('\', '/')
    $projectWrite = $path.StartsWith($global:ProjectPrefix, [StringComparison]::OrdinalIgnoreCase)
    if ($global:FaultMode -eq 'backup' -and $path.Contains('-Backup-Scripts-')) {
        throw 'simulated backup failure'
    }
    if ($global:FaultMode -eq 'restore' -and $global:Injected -and
        $source.Contains('-Backup-Scripts-') -and $path.EndsWith('/Assets/Art/NoxSky.shader')) {
        throw 'simulated restore failure'
    }
    if (-not $global:Injected -and $projectWrite -and $path.EndsWith('/Assets/Art/NoxSky.shader') -and
        $global:FaultMode -ne 'manifest') {
        $global:Injected = $true
        [IO.File]::WriteAllText($Destination, 'partial write')
        if ($global:FaultMode -ne 'corrupt') { throw 'simulated copy failure' }
        return
    }
    Microsoft.PowerShell.Management\Copy-Item @PSBoundParameters
}
function global:Set-Content {
    [CmdletBinding()]
    param([Parameter(ValueFromPipeline = $true)]$Value, [string]$LiteralPath, [string]$Encoding)
    process {
        if ($global:FaultMode -eq 'manifest' -and $LiteralPath.EndsWith('arquivos-atualizados.json')) {
            throw 'simulated manifest failure'
        }
        Microsoft.PowerShell.Management\Set-Content @PSBoundParameters
    }
}
& $Updater -Projeto $ProjectRoot
