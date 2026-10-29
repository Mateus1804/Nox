> Revisão atual: v7. Após atualizar os scripts com o Unity fechado, a abertura do projeto avisa quando as cenas ainda não receberam a revisão. O comando **Nox → Preparar cenas recuperadas** aplica a atualização. Para reaplicar a v7 já marcada, há **Nox → Reaplicar preparação v7 (com backup)**. Os diretórios gerados v5 e v6 permanecem preservados e desativados.

# Desenvolvimento e recuperação

## Ambiente de referência

O projeto mantém Unity **2021.1.28f1**, C# 8, sistema de entrada legado e renderização original. A migração para outra versão do motor não foi homologada. A máquina de desenvolvimento precisa ter o editor instalado e licenciado no Unity Hub, com suporte à build Windows.

O repositório reúne código, configurações, shader e personagem exportado. O projeto completo depende do ZIP original `Nox_Project.zip`: cenas, áudio e recursos dos pacotes permanecem nesse arquivo. O ZIP do GitHub não substitui o ZIP original.

## Projeto já recuperado

1. Com o Unity Editor fechado, extrair a branch de revisão.
2. Executar **Atualizar-Nox.cmd** por duplo clique e selecionar a raiz do projeto recuperado, com `Assets` e `ProjectSettings`.
3. Reabrir essa mesma pasta no Unity Hub e aguardar a importação e a compilação.
4. Executar **Nox → Preparar cenas recuperadas**. A revisão atual é **v6**; marcadores anteriores não bloqueiam sua aplicação.
5. Conferir o caminho de backup informado e os erros do Console.
6. Executar os verificadores e testar a partir de `Menu1.2`, antes de gerar uma build.

O conteúdo do CMD não deve ser colado linha a linha no PowerShell. A chamada equivalente é:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\tools\Atualizar-Nox.ps1" -Projeto "C:\Jogos\Nox-Recuperado"
```

O atualizador copia recursivamente scripts, Editor, personagem e Art, preservando o conteúdo anterior em `<projeto>-Backup-Scripts-<data>`. Não altera cenas nessa etapa e não exclui assets gerados ou personalizados ausentes da origem. Falhas interrompem a cópia e informam o backup; não há reversão automática anunciada para uma cópia parcial.

A preparação do Unity usa `ProjectSettings/NoxScenesPrepared-v7.txt`, backup em `Backups/Preparacao-v7/` e Animator/prefab com sufixo v6. As cenas habilitadas na build são regravadas dentro da transação de backup. Os assets gerados v5 permanecem preservados, com a ambientação anterior desativada quando substituída pela v6.

## Recuperação a partir do ZIP original

**Preparar-Nox.cmd** combina o ZIP original com os arquivos da revisão em uma pasta nova. Não instala nem ativa o Unity e não modifica o ZIP. Somente Assets, Packages e ProjectSettings são extraídos; Library e caches são regenerados pelo editor.

A recuperação sem gerar build usa:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/Preparar-Nox.ps1 -OriginalZip "C:\Jogos\Nox_Project.zip" -Destino "C:\Jogos\Nox-Recuperado"
```

O destino deve ser novo. A origem é conferida pelos hashes dos 79 arquivos de referência, e as cinco cenas de build precisam existir. Hash incompatível ou caminhos inseguros interrompem a recuperação.

## Verificações no editor

- **Nox → Auditar cenas e referências:** relatório em `Logs/Nox-Auditoria.txt`; achados exigem revisão.
- **Nox → Verificar física em cena isolada:** regressões do transporte por plataformas e salto.
- **Nox → Verificar animações e cenário:** clips, repetição, parâmetros do Animator, shader e decoração sem colliders.
- **Nox → Verificar visão e ações de interface:** testes isolados de obstáculos, callbacks de botão e validade de golpes.
- **Nox → Capturar Play atual:** solicita captura real do frame seguinte em `Logs/Capturas-v6/`.

Os verificadores isolados não substituem o percurso das fases. O roteiro de aceite está em [Acabamento v6](acabamento-v6.md). A captura precisa ser conferida no disco antes de ser apresentada como evidência.

## Build Windows

**Nox → Gerar versão Windows** verifica a preparação e solicita uma build Windows 64 bits em `Builds/Windows/Nox.exe`. A geração depende do editor funcional, da licença e do módulo de build. Sucesso de compilação isolada dos scripts não comprova geração de executável.

A build deve ser executada fora do editor e percorrida antes de distribuição. Não há uma nova build Windows homologada incluída nesta revisão.

## Testes independentes

```powershell
dotnet run --project Tests/Core/CoreTests.csproj --configuration Release
python Tests/test_preparation.py
python Tests/test_source_contracts.py
python Tests/test_character_asset.py
python Tests/test_animation_cycles.py
```

Os testes de preparação usam PowerShell 7 (`pwsh`) e ZIP fictício com cenas não jogáveis, montado a partir do histórico Git. O checkout precisa conter o commit de referência; o CI utiliza histórico completo. Os testes puros não executam Unity, áudio, física ou renderização.

## Personagem e exportação

O FBX versionado já contém o personagem e não exige Blender na máquina que abre o jogo. A reprodução artística utiliza o `Nox.blend` original, conferido por hash:

```text
blender -b --python tools/exportar_nox_blender.py -- --source Nox.blend --output exportacao
blender -b --python tools/validar_animacao_blender.py -- --fbx Assets/Characters/Nox/Nox.fbx --output evidencias
```

Procedência, créditos e histórico estão em [Personagem Nox](personagem-nox.md). Os recursos originais e suas licenças permanecem preservados; uma referência visual externa não autoriza redistribuir seu conteúdo.

## Preferências, progresso e restauração

Áudio e progresso usam `Nox.v1.*`; apresentação usa `Nox.v1.Presentation.*`, e sombras usam `Nox.v1.Graphics.*`. A retomada é pelo início da última fase válida, com desbloqueios preservados. Não salva posição, caixas, portas ou vida.

A restauração de código exige fechar o editor e recuperar os diretórios correspondentes do backup do atualizador. As cenas anteriores ficam no backup de preparação, separado dos scripts. Uma cópia completa do projeto cobre também alterações locais fora dessas ferramentas.

Backups, logs, capturas e builds não devem ser confundidos com código do repositório. Os resultados e as limitações efetivamente verificados estão em [Qualidade](qualidade.md).


## Análise estática e manutenção

Executar os comandos abaixo na raiz **deste repositório de correções**, separado da cópia Unity recuperada e dos seus pacotes de terceiros. O `global.json` mantém a seleção do SDK na família 8.0. Usar SDK .NET 8, Python 3.10 ou superior e PowerShell 7 para os testes de preparação.

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

Para aplicar somente formatação, executar `dotnet format whitespace --folder` e `python -m ruff format .`. Revisar o diff antes do commit. Correções automáticas de lint exigem conferir se referências e comportamento foram preservados.

No Windows, `powershell -NoProfile -File tools/Verificar-Sintaxe.ps1` confere também a gramática do Windows PowerShell 5.1. O CI usa os dois parsers. O teste de C# analisa sintaxe nos três perfis, mas não substitui compilar os scripts no Unity. O repositório não distribui DLLs do Editor nem stubs simulando suas APIs.

As convenções estão em [Boas práticas](boas-praticas.md) e os achados em [Revisão de código e limpeza](revisao-clean-code.md). O SDK .NET 8 e o Ruff pertencem às ferramentas de manutenção; a versão do jogo continua Unity 2021.1.28f1/C# 8.


## Recuperação de uma atualização com falha

O atualizador confere o conteúdo copiado e tenta desfazer as escritas quando uma exceção é capturada. A mensagem informa o backup e o resultado. Consultar `recuperacao.json` na pasta de backup: `restaurado` indica verificação concluída; `recuperacao-parcial` lista arquivos que ainda exigem recuperação manual; `sem-alteracoes` indica falha anterior às escritas. Manter o Unity fechado até resolver uma recuperação parcial.

Esse mecanismo não cobre interrupção de energia ou encerramento forçado. O backup continua necessário. O funcionamento e os testes estão em [Revisão do atualizador](revisao-atualizador.md).

Os testes usam `pwsh` por padrão. Para executar os cinco cenários de recuperação no Windows PowerShell 5.1, definir `$env:NOX_TEST_POWERSHELL = "powershell"` no terminal dos testes. Essa variável é exclusiva da suíte; o atualizador continua sendo usado da mesma forma.
