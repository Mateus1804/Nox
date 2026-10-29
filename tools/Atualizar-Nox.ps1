[CmdletBinding()]
param([string]$Projeto)
$ErrorActionPreference = 'Stop'

function Copy-NoxVerifiedFile {
    param([string]$Source, [string]$Destination, [string]$ExpectedHash)
    New-Item -ItemType Directory -Path (Split-Path $Destination -Parent) -Force | Out-Null
    Copy-Item -LiteralPath $Source -Destination $Destination -Force
    if ((Get-FileHash -LiteralPath $Destination -Algorithm SHA256).Hash -ne $ExpectedHash) {
        throw ('A cópia não corresponde ao arquivo esperado: ' + $Destination)
    }
}

function Get-NoxUpdatePlan {
    param([string]$RepoRoot, [string]$ProjectRoot, [string[]]$Folders)
    foreach ($folder in $Folders) {
        $source = Join-Path $RepoRoot $folder
        if (-not (Test-Path -LiteralPath $source -PathType Container)) {
            throw ('Pasta de origem ausente: ' + $folder)
        }
        foreach ($file in Get-ChildItem -LiteralPath $source -File -Recurse) {
            $relative = $folder + '/' + $file.FullName.Substring($source.Length).TrimStart([char[]]"\/")
            $destination = Join-Path $ProjectRoot $relative
            if (Test-Path -LiteralPath $destination -PathType Container) {
                throw ('Existe uma pasta onde deveria haver um arquivo: ' + $destination)
            }
            $parent = Split-Path $destination -Parent
            while ($parent.Length -gt $ProjectRoot.Length) {
                if (Test-Path -LiteralPath $parent -PathType Leaf) {
                    throw ('Existe um arquivo onde deveria haver uma pasta: ' + $parent)
                }
                $parent = Split-Path $parent -Parent
            }
            $existed = Test-Path -LiteralPath $destination -PathType Leaf
            [PSCustomObject]@{
                Source = $file.FullName
                Destination = $destination
                Relative = $relative
                Existed = $existed
                OriginalHash = if ($existed) { (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash } else { $null }
                SourceHash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
            }
        }
    }
}

function New-NoxUpdateBackup {
    param([string]$ProjectRoot, [string]$BackupRoot, [string[]]$Folders, [object[]]$Updates)
    foreach ($folder in $Folders) {
        $old = Join-Path $ProjectRoot $folder
        if (Test-Path -LiteralPath $old) {
            $backupParent = Split-Path (Join-Path $BackupRoot $folder) -Parent
            New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
            Copy-Item -LiteralPath $old -Destination $backupParent -Recurse -Force
        }
    }
    foreach ($update in $Updates) {
        if (-not $update.Existed) { continue }
        $copy = Join-Path $BackupRoot $update.Relative
        if (-not (Test-Path -LiteralPath $copy -PathType Leaf) -or
            (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash -ne $update.OriginalHash) {
            throw ('Backup não confirmado para: ' + $update.Relative)
        }
    }
}

function Restore-NoxAttemptedUpdates {
    param([object[]]$Attempted, [string]$BackupRoot)
    $failures = New-Object 'System.Collections.Generic.List[string]'
    for ($index = $Attempted.Count - 1; $index -ge 0; $index--) {
        $update = $Attempted[$index]
        try {
            if ($update.Existed) {
                Copy-NoxVerifiedFile -Source (Join-Path $BackupRoot $update.Relative) `
                    -Destination $update.Destination -ExpectedHash $update.OriginalHash
            }
            elseif (Test-Path -LiteralPath $update.Destination) {
                # Somente arquivos novos deste plano; pastas e dados externos não são removidos.
                if (-not (Test-Path -LiteralPath $update.Destination -PathType Leaf)) {
                    throw ('O destino deixou de ser um arquivo: ' + $update.Destination)
                }
                Remove-Item -LiteralPath $update.Destination -Force
                if (Test-Path -LiteralPath $update.Destination) {
                    throw ('O arquivo novo não pôde ser removido: ' + $update.Destination)
                }
            }
        }
        catch { $failures.Add($update.Relative + ': ' + $_.Exception.Message) }
    }
    return $failures.ToArray()
}

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
$updates = @(Get-NoxUpdatePlan -RepoRoot $repo -ProjectRoot $Projeto -Folders $managedFolders)
$backup = $Projeto + '-Backup-Scripts-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff')
New-Item -ItemType Directory -Path $backup | Out-Null
$attempted = New-Object 'System.Collections.Generic.List[object]'
try {
    New-NoxUpdateBackup -ProjectRoot $Projeto -BackupRoot $backup -Folders $managedFolders -Updates $updates
    foreach ($update in $updates) {
        # Registrar antes da cópia também cobre uma falha que deixe o arquivo parcialmente escrito.
        $attempted.Add($update)
        Copy-NoxVerifiedFile -Source $update.Source -Destination $update.Destination -ExpectedHash $update.SourceHash
    }
    @($updates | ForEach-Object { $_.Relative }) | ConvertTo-Json |
        Set-Content -LiteralPath (Join-Path $backup 'arquivos-atualizados.json') -Encoding UTF8
}
catch {
    $originalFailure = $_.Exception.Message
    $restoreErrors = @(Restore-NoxAttemptedUpdates -Attempted $attempted.ToArray() -BackupRoot $backup)
    $status = if ($attempted.Count -eq 0) { 'sem-alteracoes' }
        elseif ($restoreErrors.Count -eq 0) { 'restaurado' }
        else { 'recuperacao-parcial' }
    $report = [PSCustomObject]@{
        status = $status
        erroOriginal = $originalFailure
        arquivosTentados = @($attempted | ForEach-Object { $_.Relative })
        falhasDeRecuperacao = $restoreErrors
    }
    $reportFailure = ''
    try {
        $report | ConvertTo-Json -Depth 4 |
            Set-Content -LiteralPath (Join-Path $backup 'recuperacao.json') -Encoding UTF8
    }
    catch { $reportFailure = ' O relatório de recuperação não pôde ser gravado: ' + $_.Exception.Message }
    if ($restoreErrors.Count -gt 0) {
        throw ('A atualização falhou e a recuperação ficou incompleta. Backup: ' + $backup +
            '. Falha original: ' + $originalFailure + '. Arquivos pendentes: ' + ($restoreErrors -join '; ') + $reportFailure)
    }
    if ($attempted.Count -eq 0) {
        throw ('A atualização foi interrompida antes de alterar o projeto. O backup pode estar incompleto: ' + $backup +
            '. Falha original: ' + $originalFailure + $reportFailure)
    }
    throw ('A atualização foi interrompida e os arquivos tentados foram restaurados. Backup: ' + $backup +
        '. Falha original: ' + $originalFailure + $reportFailure)
}
Write-Host ('Scripts atualizados e conferidos em: ' + $Projeto)
Write-Host ('Backup dos scripts anteriores: ' + $backup)
Write-Host 'Abrir o projeto no Unity e executar Nox > Preparar cenas recuperadas. Testar no Play antes de gerar a build.'
