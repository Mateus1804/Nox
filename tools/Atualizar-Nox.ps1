[CmdletBinding()]
param([string]$Projeto)
$ErrorActionPreference = 'Stop'
if (-not $Projeto) {
    Add-Type -AssemblyName System.Windows.Forms
    $dialog = New-Object System.Windows.Forms.FolderBrowserDialog
    $dialog.Description = 'Selecionar a pasta Nox-Recuperado que contém Assets e ProjectSettings'
    if ($dialog.ShowDialog() -ne 'OK') { throw 'Seleção cancelada; nenhum arquivo foi alterado.' }
    $Projeto = $dialog.SelectedPath
}
$Projeto = (Resolve-Path -LiteralPath $Projeto).Path
if (-not (Test-Path -LiteralPath (Join-Path $Projeto 'ProjectSettings/ProjectVersion.txt') -PathType Leaf) -or
    -not (Test-Path -LiteralPath (Join-Path $Projeto 'Assets') -PathType Container)) {
    throw 'Selecionar a pasta raiz do projeto Unity recuperado, contendo Assets e ProjectSettings.'
}
if (Get-Process -Name Unity -ErrorAction SilentlyContinue) { throw 'Fechar o Unity Editor antes de atualizar os scripts.' }
$repo = Split-Path $PSScriptRoot -Parent
if ([IO.Path]::GetFullPath($repo).TrimEnd([char[]]"\/") -eq [IO.Path]::GetFullPath($Projeto).TrimEnd([char[]]"\/")) {
    throw 'O destino deve ser o projeto completo recuperado, separado desta pasta de correções.'
}
$managedFolders = @('Assets/Scripts', 'Assets/Editor', 'Assets/Characters/Nox', 'Assets/Art')
# Validar o plano inteiro antes de sobrescrever qualquer arquivo do projeto.
$updates = foreach ($folder in $managedFolders) {
    $source = Join-Path $repo $folder
    if (-not (Test-Path -LiteralPath $source -PathType Container)) {
        throw ('Pasta de origem ausente: ' + $folder)
    }
    foreach ($file in Get-ChildItem -LiteralPath $source -File -Recurse) {
        $relative = $file.FullName.Substring($source.Length).TrimStart([char[]]"\/")
        $destination = Join-Path (Join-Path $Projeto $folder) $relative
        if (Test-Path -LiteralPath $destination -PathType Container) {
            throw ('Existe uma pasta onde deveria haver um arquivo: ' + $destination)
        }
        $parent = Split-Path $destination -Parent
        while ($parent.Length -gt $Projeto.Length) {
            if (Test-Path -LiteralPath $parent -PathType Leaf) {
                throw ('Existe um arquivo onde deveria haver uma pasta: ' + $parent)
            }
            $parent = Split-Path $parent -Parent
        }
        [PSCustomObject]@{
            Source = $file.FullName
            Destination = $destination
            Relative = $folder + '/' + $relative
        }
    }
}
$backup = $Projeto + '-Backup-Scripts-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff')
New-Item -ItemType Directory -Path $backup | Out-Null
$written = @()
try {
    foreach ($folder in $managedFolders) {
        $old = Join-Path $Projeto $folder
        if (Test-Path -LiteralPath $old) {
            $backupParent = Split-Path (Join-Path $backup $folder) -Parent
            New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
            Copy-Item -LiteralPath $old -Destination $backupParent -Recurse
        }
    }
    # O plano já validado evita copiar um arquivo para dentro de uma pasta de mesmo nome.
    foreach ($update in $updates) {
        New-Item -ItemType Directory -Path (Split-Path $update.Destination -Parent) -Force | Out-Null
        Copy-Item -LiteralPath $update.Source -Destination $update.Destination -Force
        $written += $update.Relative
    }
    $written | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $backup 'arquivos-atualizados.json') -Encoding UTF8
}
catch {
    throw ('A atualização foi interrompida. Os scripts anteriores estão preservados em ' + $backup + '. Detalhe: ' + $_.Exception.Message)
}
Write-Host ('Scripts atualizados em: ' + $Projeto)
Write-Host ('Backup dos scripts anteriores: ' + $backup)
Write-Host 'Abrir o projeto no Unity e executar Nox > Preparar cenas recuperadas. Testar no Play antes de gerar a build.'
