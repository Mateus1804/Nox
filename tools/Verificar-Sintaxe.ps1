[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$files = @(Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.ps1' -File)
$files += @(Get-ChildItem -LiteralPath (Join-Path $root 'Tests/fixtures') -Filter '*.ps1' -File -Recurse)
foreach ($file in $files) {
    $tokens = $null
    $syntaxErrors = $null
    [System.Management.Automation.Language.Parser]::ParseFile($file.FullName, [ref]$tokens, [ref]$syntaxErrors) | Out-Null
    if ($syntaxErrors.Count -gt 0) {
        $syntaxErrors | ForEach-Object { Write-Error ($file.Name + ': ' + $_.Message) }
        throw 'A análise sintática do PowerShell falhou.'
    }
}
Write-Host ('Sintaxe PowerShell: ' + $files.Count + ' arquivos, zero erros.')
