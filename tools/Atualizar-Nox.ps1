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
$backup = $Projeto + '-Backup-Scripts-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff')
New-Item -ItemType Directory -Path $backup | Out-Null
$written = @()
try {
    foreach ($folder in @('Assets/Scripts', 'Assets/Editor', 'Assets/Characters/Nox')) {
        $old = Join-Path $Projeto $folder
        if (Test-Path -LiteralPath $old) {
            $backupParent = Split-Path (Join-Path $backup $folder) -Parent
            New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
            Copy-Item -LiteralPath $old -Destination $backupParent -Recurse
        }
    }
    # Assets e configurações locais permanecem no projeto; apenas o código é sobreposto.
    foreach ($folder in @('Assets/Scripts', 'Assets/Editor', 'Assets/Characters/Nox')) {
        $source = Join-Path $repo $folder
        $target = Join-Path $Projeto $folder
        New-Item -ItemType Directory -Path $target -Force | Out-Null
        foreach ($file in Get-ChildItem -LiteralPath $source -File) {
            Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $target $file.Name) -Force
            $written += $folder + '/' + $file.Name
        }
    }
    $written | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $backup 'arquivos-atualizados.json') -Encoding UTF8
}
catch {
    throw ('A atualização foi interrompida. Os scripts anteriores estão preservados em ' + $backup + '. Detalhe: ' + $_.Exception.Message)
}
Write-Host ('Scripts atualizados em: ' + $Projeto)
Write-Host ('Backup dos scripts anteriores: ' + $backup)
Write-Host 'Abrir o projeto no Unity e executar Nox > Preparar cenas recuperadas. Testar no Play antes de gerar a build.'
