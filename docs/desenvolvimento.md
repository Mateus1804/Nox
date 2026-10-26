# Desenvolvimento e recuperação

## Requisitos

Windows com PowerShell 5.1 ou superior, o ZIP original `Nox_Project.zip` e Unity Editor instalado/licenciado pelo Hub, com suporte a build Windows. O arquivo de referência registra 2021.1.28f1. A primeira alternativa é 2021.3 LTS, em uma cópia de trabalho. Unity 6 não foi homologado nesta recuperação.

## Recuperação assistida

1. Baixar a branch de correções e extrair o repositório.
2. Executar `Preparar-Nox.cmd`.
3. Selecionar `Nox_Project.zip` e o `Unity.exe` do editor instalado.
4. Aguardar a importação e a build. Os assets originais têm cerca de 1,26 GB; o cache será regenerado pelo editor.
5. Conferir o resultado em `Documentos/Nox-Recuperado-<data>/Builds/Windows/Nox.exe`, quando houver sucesso. O arquivo `build-windows.log` fica na pasta do projeto preparado.

O processo não instala nem ativa o Unity. Falhas de instalação, licença, pacote ou compilação interrompem o preparo/build e mantêm o log. Não existe um executável novo validado incluído nesta branch.

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
```

A segunda suíte usa PowerShell 7 (`pwsh`) e fixtures criadas com os arquivos originais obtidos do histórico Git. Os dados de cena dessas fixtures não são jogáveis. O checkout precisa conter o commit de referência, como no CI com histórico completo.

Compilar C# contra as referências gerenciadas da build original verifica tipos e assinaturas desses scripts, mas não substitui o processamento de assets nem a compilação completa no editor. As bibliotecas privadas usadas nessa conferência não são versionadas.
