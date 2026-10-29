[CmdletBinding()]
param(
    [string]$OriginalZip,
    [string]$Destino,
    [string]$UnityExe,
    [switch]$GerarBuild
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$repo = Split-Path $PSScriptRoot -Parent
if (-not $OriginalZip) {
    Add-Type -AssemblyName System.Windows.Forms
    $dialog = New-Object System.Windows.Forms.OpenFileDialog
    $dialog.Title = 'Selecionar Nox_Project.zip original'
    $dialog.Filter = 'Projeto original (*.zip)|*.zip'
    if ($dialog.ShowDialog() -ne 'OK') { throw 'Seleção cancelada; nenhum arquivo original foi alterado.' }
    $OriginalZip = $dialog.FileName
}
$OriginalZip = (Resolve-Path -LiteralPath $OriginalZip).Path
if (-not $Destino) {
    $Destino = Join-Path ([Environment]::GetFolderPath('MyDocuments')) ('Nox-Recuperado-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
}
if (Test-Path -LiteralPath $Destino) { throw 'O destino já existe. Selecionar uma pasta nova para preservar os arquivos.' }
$Destino = [IO.Path]::GetFullPath($Destino)
$archive = [IO.Compression.ZipFile]::OpenRead($OriginalZip)
try {
    $versions = @($archive.Entries | Where-Object { $_.FullName -match '(^|/)ProjectSettings/ProjectVersion\.txt$' })
    if ($versions.Count -ne 1) { throw 'O ZIP deve conter exatamente um projeto Unity completo.' }
    $prefix = $versions[0].FullName.Substring(0, $versions[0].FullName.Length - 'ProjectSettings/ProjectVersion.txt'.Length)
    $entries = @{}
    foreach ($entry in $archive.Entries) {
        if (-not $entry.FullName.StartsWith($prefix, [StringComparison]::Ordinal)) { continue }
        $relative = $entry.FullName.Substring($prefix.Length)
        if ($relative -notmatch '^(Assets|Packages|ProjectSettings)/') { continue }
        if ($relative.Contains('\') -or $relative -match '(^|/)\.\.(/|$)' -or $relative.Contains(':')) { throw 'Caminho inseguro no ZIP.' }
        $target = [IO.Path]::GetFullPath((Join-Path $Destino $relative))
        $base = $Destino.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
        if (-not $target.StartsWith($base, [StringComparison]::OrdinalIgnoreCase)) { throw 'O ZIP tenta gravar fora do destino.' }
        if (($entry.ExternalAttributes -shr 16 -band 0xF000) -eq 0xA000) { throw 'Links simbólicos não são aceitos no projeto de origem.' }
        if ($entry.FullName.EndsWith('/')) { continue }
        if ($entries.ContainsKey($relative)) { throw 'O ZIP contém caminhos duplicados.' }
        $entries[$relative] = $entry
    }
    $manifest = Get-Content -LiteralPath (Join-Path $repo 'docs/importacao.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($file in $manifest.arquivos) {
        if (-not $entries.ContainsKey($file.caminho)) { throw ('Arquivo original ausente: ' + $file.caminho) }
        $stream = $entries[$file.caminho].Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try { $hash = ([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-', '').ToLowerInvariant() }
        finally { $sha.Dispose(); $stream.Dispose() }
        if ($hash -ne $file.sha256) { throw ('O original difere da referência analisada: ' + $file.caminho) }
    }
    foreach ($name in @('Menu1.2', 'AirPhase1.4', 'AirPhase2.4', 'AirPhase3.5', 'FirePhase1.4')) {
        if (-not $entries.ContainsKey('Assets/Scenes/' + $name + '.unity')) { throw ('Cena ausente: ' + $name) }
    }
    New-Item -ItemType Directory -Path $Destino | Out-Null
    foreach ($relative in $entries.Keys) {
        $target = Join-Path $Destino $relative
        New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entries[$relative], $target, $false)
    }
}
finally { $archive.Dispose() }
foreach ($folder in @('Assets', 'Packages', 'ProjectSettings')) {
    $source = Join-Path $repo $folder
    Copy-Item -LiteralPath $source -Destination $Destino -Recurse -Force
}
Write-Host ('Projeto preparado em: ' + $Destino)
Write-Host 'O ZIP original foi preservado. As cenas ainda precisam da preparação pelo Unity.'
if (-not $GerarBuild) { return }
if (-not $UnityExe) {
    Add-Type -AssemblyName System.Windows.Forms
    $dialog = New-Object System.Windows.Forms.OpenFileDialog
    $dialog.Title = 'Selecionar Unity.exe do editor instalado (2021.1.28f1: referência do projeto)'
    $dialog.Filter = 'Editor Unity (Unity.exe)|Unity.exe'
    $dialog.InitialDirectory = 'C:\Program Files\Unity\Hub\Editor'
    if ($dialog.ShowDialog() -ne 'OK') { throw ('Seleção do editor cancelada. O projeto preparado está em ' + $Destino) }
    $UnityExe = $dialog.FileName
}
if (-not (Test-Path -LiteralPath $UnityExe -PathType Leaf)) { throw 'Executável do Unity não encontrado.' }
$log = Join-Path $Destino 'build-windows.log'
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $Destino + '"'), '-executeMethod', 'NoxBuild.Windows', '-logFile', ('"' + $log + '"'))
$process = Start-Process -FilePath $UnityExe -ArgumentList $arguments -Wait -PassThru
if ($process.ExitCode -ne 0) { throw ('O Unity não concluiu a build. Consultar ' + $log) }
$exe = Join-Path $Destino 'Builds/Windows/Nox.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw ('O Unity encerrou sem gerar o executável. Consultar ' + $log) }
Write-Host ('Build gerada em: ' + $exe)
Write-Host 'A geração não substitui a conferência de jogabilidade.'
