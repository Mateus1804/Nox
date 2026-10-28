# Desenvolvimento e recuperação

## Requisitos

Windows com PowerShell 5.1 ou superior, o ZIP original `Nox_Project.zip` e Unity Editor instalado/licenciado pelo Hub, com suporte a build Windows. O arquivo de referência registra 2021.1.28f1. A primeira alternativa é 2021.3 LTS, em uma cópia de trabalho. Unity 6 não foi homologado nesta recuperação.

## Recuperação do projeto

1. Baixar a branch de correções e extrair o repositório.
2. Executar `Preparar-Nox.cmd`.
3. Selecionar `Nox_Project.zip` e o `Unity.exe` do editor instalado.
4. Aguardar a importação e a build. Os assets originais têm cerca de 1,26 GB; o cache será regenerado pelo editor.
5. Conferir o resultado em `Documentos/Nox-Recuperado-<data>/Builds/Windows/Nox.exe`, quando houver sucesso. O arquivo `build-windows.log` fica na pasta do projeto preparado.

O processo não instala nem ativa o Unity. Falhas de instalação, licença, pacote ou compilação interrompem o preparo/build e mantêm o log. Não existe um executável novo validado incluído nesta branch.

## Atualização do projeto já recuperado

Com o Unity fechado, o ponto de entrada é `Atualizar-Nox.cmd`, executado por duplo clique a partir do ZIP de correções extraído. O arquivo CMD não deve ser colado linha a linha no PowerShell. A seleção aponta para a pasta raiz que contém `Assets` e `ProjectSettings`.

Os diretórios anteriores `Assets/Scripts`, `Assets/Editor` e `Assets/Characters/Nox`, quando existentes, são copiados para `<projeto>-Backup-Scripts-<data>`, ao lado do projeto. A atualização sobrepõe os arquivos desses três diretórios. Cenas, áudio, configurações e demais assets permanecem intactos nessa etapa. Em caso de erro de cópia, o processo interrompe e informa onde estão os scripts anteriores; não há reversão automática anunciada.

A chamada equivalente no PowerShell, dentro da pasta de correções, é:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\tools\Atualizar-Nox.ps1" -Projeto "C:\Jogos\Nox-Recuperado"
```

Após reabrir o Unity e concluir a compilação, **Nox → Preparar cenas recuperadas** aplica a preparação v4. As marcas v1/v2/v3 não bloqueiam essa revisão. As cenas são copiadas para `Backups/Preparacao-v4/` antes de alterações; falhas na operação acionam a tentativa de reversão. A ação altera e salva as cenas habilitadas na build, acrescentando a configuração física de plataformas e caixas. O código atualizado usa a pasta existente; não exige baixar ou extrair novamente o ZIP original.

A restauração de scripts consiste em fechar o Unity e recuperar os diretórios correspondentes do backup. Alterações de cena feitas depois pelo editor são separadas desse backup; a preparação v4 cria um backup dos arquivos anteriores das cenas. Uma cópia completa da pasta ainda é útil para cobrir configurações, recursos e alterações feitas fora dessas ferramentas.

## Preparação sem gerar build

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/Preparar-Nox.ps1 -OriginalZip "C:\Jogos\Nox_Project.zip" -Destino "C:\Jogos\Nox-Recuperado"
```

O destino deve ser novo. O ZIP é identificado pelos hashes dos 79 arquivos de referência, e as cinco cenas de build precisam estar presentes. Somente Assets, Packages e ProjectSettings são extraídos. Library, caches, builds anteriores e materiais de outras pastas do Drive ficam fora do preparo.

Após abrir a cópia no Unity, o menu **Nox → Preparar cenas recuperadas** aplica os reparos de UI e índices dos tutoriais. O menu **Nox → Gerar versão Windows** faz a preparação e solicita a compilação. A versão final deve ser testada fora do editor antes de ser distribuída.

## Alterações e referências

Os scripts e os `.meta` corrigidos são sobrepostos apenas na nova pasta. O original permanece no ZIP. Hash incompatível indica outra referência: o processo interrompe em vez de aplicar correções sobre um projeto diferente silenciosamente.

A preparação corrige os vínculos antigos conhecidos sem recriar cenas ou inventar fases de água e terra. Mudanças em métodos/campos serializados devem preservar nomes ou possuir migração explícita. O código de Editor depende das bibliotecas de Editor e não integra o executável do jogo. A importação e a execução dessas ações exigem o Unity.

## Testes independentes

```powershell
dotnet run --project Tests/Core/CoreTests.csproj --configuration Release
python Tests/test_preparation.py
python Tests/test_source_contracts.py
python Tests/test_character_asset.py
```

A segunda suíte usa PowerShell 7 (`pwsh`) e fixtures criadas com os arquivos originais obtidos do histórico Git. Os dados de cena dessas fixtures não são jogáveis. O checkout precisa conter o commit de referência, como no CI com histórico completo.

Compilar C# contra as referências gerenciadas da build original verifica tipos e assinaturas desses scripts, mas não substitui o processamento de assets nem a compilação completa no editor. As bibliotecas privadas usadas nessa conferência não são versionadas.

## Preferências e retomada

PlayerPrefs utiliza o prefixo `Nox.v1.` para volume, silêncio, desbloqueios e última fase. A mudança não remove preferências de outros projetos. O botão **Continuar fase** é inserido pela preparação v4 na seleção de fases. A retomada usa o início da fase salva, com os valores de vida e objetos definidos na cena. Não há salvamento de posição.

A auditoria está em **Nox → Auditar cenas e referências**, com saída em `Logs/Nox-Auditoria.txt`. O relatório apresenta achados para revisão; não corrige todos os campos vazios automaticamente. Os backups de cenas ficam fora de Assets, em `Backups/`, e não devem ser publicados como código do projeto.

## Personagem original

A atualização inclui `Assets/Characters/Nox`. A preparação v4 gera prefab/Animator a partir do FBX e aplica a substituição do cubo sem seleção manual. Os controles físicos são preservados; o modelo acompanha repouso, caminhada, salto e ataque. Procedência, reprodução da exportação e limites estão em `personagem-nox.md`.


## Atualização de animação e ambientação v5

A revisão v5 inclui o FBX atualizado e `Assets/Art/NoxSky.shader`. O atualizador copia essas pastas recursivamente, com backup do conteúdo anterior. A atualização é aplicada ao projeto completo recuperado, com o Unity fechado.

Após a importação e a compilação, **Nox → Preparar cenas recuperadas** aplica a nova revisão mesmo quando a v4 já foi preparada. O marcador atual é `ProjectSettings/NoxScenesPrepared-v5.txt`; o marcador v4 permanece como histórico. As cenas anteriores ficam em `Backups/Preparacao-v5/`.

**Nox → Verificar animações e cenário** confere clips, repetição, presença do novo Animator, shader e ausência de colisores nos detalhes visuais. É uma verificação estrutural do editor; o percurso das fases no Play continua necessário. O menu de build permanece **Nox → Gerar versão Windows**.

A descrição e os limites estão em [Animação e cenários](animacao-e-cenarios.md). O exportador e o validador offline aceitam argumentos:

```text
blender -b --python tools/exportar_nox_blender.py -- --source Nox.blend --output exportacao
blender -b --python tools/validar_animacao_blender.py -- --fbx Assets/Characters/Nox/Nox.fbx --output evidencias
python Tests/test_animation_cycles.py
```
