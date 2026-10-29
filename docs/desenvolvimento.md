# Desenvolvimento e recuperação

## Ambiente

A referência é Unity **2021.1.28f1**, C# 8, sistema de entrada legado e renderização original. O editor precisa estar instalado e licenciado no Unity Hub, com suporte à build Windows. A migração de motor não está homologada.

O repositório contém scripts, configurações, shaders e o personagem exportado. Cenas, músicas e outros recursos dependem do ZIP original `Nox_Project.zip`. O ZIP do GitHub contém a revisão do código e não substitui o projeto completo.

## Atualização de um projeto recuperado

1. Fechar o Unity Editor e extrair a revisão do repositório.
2. Executar **Atualizar-Nox.cmd** por duplo clique e selecionar a pasta recuperada que contém `Assets` e `ProjectSettings`.
3. Conferir o resultado da cópia e o caminho do backup.
4. Abrir essa mesma pasta no Unity Hub e aguardar a compilação.
5. Executar **Nox → Preparar cenas recuperadas**. O marcador atual é **v8**.
6. Executar os verificadores do menu Nox e conferir as cenas antes do Play e da build.

O conteúdo do CMD não deve ser colado linha a linha no PowerShell. A chamada equivalente, a partir da pasta do repositório, é:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\tools\Atualizar-Nox.ps1" -Projeto "C:\Jogos\Nox-Recuperado"
```

O atualizador usa o mapa `Assets/Editor/Build/script-migration.json` para retirar caminhos antigos conhecidos, preservando os GUIDs nos novos destinos. Copiar a pasta nova manualmente e deixar os scripts antigos juntos pode causar classes duplicadas. O atualizador valida caminhos, faz backup e confere as cópias por hash. Pastas e recursos personalizados fora do plano não são apagados.

Em uma exceção tratada, a recuperação tenta restaurar as versões anteriores, inclusive arquivos retirados pela migração. O backup contém `recuperacao.json` com `sem-alteracoes`, `restaurado` ou `recuperacao-parcial`, causa original e pendências. Uma recuperação parcial exige resolver os arquivos indicados antes de abrir o Unity. Queda de energia ou encerramento forçado não são cobertos como uma transação atômica.

## Recuperação inicial

Executar **Preparar-Nox.cmd** e selecionar o ZIP original. O destino precisa ser uma pasta nova. A ferramenta confere hashes dos 79 arquivos de referência e a presença das cinco cenas de build, extrai somente `Assets`, `Packages` e `ProjectSettings` e aplica a mesma atualização verificada da estrutura de scripts.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/Preparar-Nox.ps1 -OriginalZip "C:\Jogos\Nox_Project.zip" -Destino "C:\Jogos\Nox-Recuperado"
```

O ZIP original é preservado; caches como `Library` são regenerados pelo Unity. Os scripts extraídos ficam disponíveis no backup da atualização inicial. A ferramenta não instala nem ativa o motor.

## Preparação e verificadores

A preparação v8 grava backup em `Backups/Preparacao-v8` e usa `ProjectSettings/NoxScenesPrepared-v8.txt`. Converte adaptadores antigos de jogador, chefe e portal nas cenas habilitadas, redireciona referências e retira o marcador de plataforma sem comportamento. Um erro interrompe a transação de cenas. A estrutura completa está em [Estrutura e migração](estrutura-scripts.md).

- **Nox → Verificar migração de scripts:** catálogo de GUIDs, prefabs temporários com campos antigos e conversão de referências e perfis.
- **Nox → Auditar cenas e referências:** relatório em `Logs/Nox-Auditoria.txt`.
- **Nox → Verificar física em cena isolada:** transporte por plataformas e salto.
- **Nox → Verificar animações e cenário:** clips, parâmetros, shaders e decoração.
- **Nox → Verificar visão e ações de interface:** obstáculos, callbacks e validade de golpes.
- **Nox → Capturar Play atual:** captura real no frame seguinte em `Logs/Capturas-v6`.

A preparação normal conserva uma ambientação v7 já existente. **Nox → Reaplicar preparação v8 (com backup)** refaz a preparação e pode regenerar a arte. Os recursos e o FBX v7 continuam sendo utilizados; a revisão v8 é de código e migração.

Testar no Play movimento, vida, cura, combate, plataformas, caixas, interface, pausa, áudio, progressão e perfis dos chefes. Os cenários de arquivo fictício não comprovam esses fluxos no jogo.

## Build Windows

**Nox → Gerar versão Windows** verifica a preparação e solicita uma build Windows 64 bits em `Builds/Windows/Nox.exe`. Isso depende de editor, licença e módulo de build funcionais. Executar o jogo fora do editor e percorrer as fases antes de distribuir. Não há uma build Windows homologada desta revisão.

## Análise e testes do repositório

Executar na raiz do repositório de correções, separado da cópia Unity completa e dos pacotes de terceiros. O `global.json` seleciona SDK .NET 8.0. Usar Python 3.10 ou superior e PowerShell 7 nos testes locais de arquivos.

```powershell
python -m pip install -r requirements-dev.txt
dotnet format whitespace --folder --verify-no-changes
dotnet run --project tools/CodeQuality/CodeQuality.csproj --configuration Release
python -m ruff check .
python -m ruff format --check .
python -m compileall -q tools Tests
pwsh -NoProfile -File tools/Verificar-Sintaxe.ps1
dotnet run --project Tests/Core/CoreTests.csproj --configuration Release
python -m unittest discover -s Tests -p "test_*.py" -v
```

Para aplicar formatação, usar `dotnet format whitespace --folder` e `python -m ruff format .` e revisar o diff. Para selecionar outro host nos testes, definir `$env:NOX_TEST_POWERSHELL = "powershell"`; o CI Windows também testa recuperação e migração no PowerShell 5.1.

O parser C# verifica sintaxe em três perfis. Os testes de contratos resolvem herança, callbacks e nomes de serialização; não substituem a importação no Unity. A compilação isolada usa referências reais do motor, sem stubs, mas também não executa física ou renderização.

Os testes precisam do histórico Git completo para montar a referência original e verificar contratos. O CI usa `fetch-depth: 0`. As regras de manutenção estão em [Boas práticas](boas-praticas.md).

## Personagem e procedência

O modelo original, o GDD e os recursos de terceiros mantêm seus créditos e restrições. As ferramentas Python de Blender permanecem em `tools`; sua reprodução depende do `Nox.blend` original e da versão Blender documentada. A reorganização não reexportou o FBX nem alterou sua malha ou animações. Evidências anteriores estão em [Personagem Nox](personagem-nox.md), [Correção v7](correcao-v7.md) e [Qualidade](qualidade.md).
