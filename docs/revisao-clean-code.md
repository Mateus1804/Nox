# Revisão de código, limpeza e análise estática

> Complemento após a integração do PR #9: o atualizador passou a separar planejamento, backup, cópia e recuperação verificada. A limitação de cópias parcialmente concluídas foi tratada para exceções capturadas, com 28 testes Python aprovados no total. Resultados e limites atuais em [Revisão do atualizador](revisao-atualizador.md). Os números abaixo documentam a revisão anterior.

## Referência

Base: `06d3335f311628436e4a61fbe29c62b8a61b44d7`, com a correção v7 integrada. A árvore local começou sem alterações. A revisão preserva motor, renderização, controles, dados de progresso, GUIDs, campos serializados e callbacks públicos da v7.

Antes das alterações, passaram 149 verificações C# e 21 testes Python. A compilação isolada dos 56 scripts de execução e 12 de Editor não apresentou erros ou avisos. A análise Python inicial encontrou 135 ocorrências de estilo, principalmente várias instruções na mesma linha e imports fora do início do arquivo. Não havia uma configuração de formatação compartilhada nem etapas de lint/sintaxe no CI.

## Achados e alterações

| Classificação | Arquivo ou função | Impacto e correção |
| --- | --- | --- |
| Bug reproduzido em regra | `NoxJumpAssist.Request/TryConsume` | Um pedido com `NaN` podia produzir salto. A execução isolada da base retornou `True` para esse caso. Entradas de relógio inválidas agora são recusadas. |
| Bug reproduzido na atualização | `tools/Atualizar-Nox.ps1` | Uma pasta ocupando o nome de um arquivo podia receber a cópia dentro dela; outros conflitos podiam falhar depois de já sobrescrever scripts. O plano inteiro é validado antes das cópias. Dois testes reproduzem os conflitos e exigem preservação dos arquivos. |
| Robustez | `NoxAttackCycle` e `NoxEnemyCycle` | Relógios não finitos podiam deixar fases e progresso incoerentes. As ações passam a cancelar ou reiniciar de forma fechada. Não se trata de uma falha observada no relógio normal do Unity. |
| Diagnóstico | `NoxDamageWindow.TryApply` | Exceções para relógio ou proteção inválidos indicavam sempre `amount`. O parâmetro efetivamente inválido passa a ser identificado. |
| Duplicação | `PlayerScript` e `PlayerTwo` | Entrada, dano por contato, velocidades e apresentação da vida podiam divergir. As operações comuns passaram para `NoxPlayerControllerSupport`, mantendo dados e callbacks nos componentes existentes. |
| Trabalho repetido | Barras de vida e referência visual | Atribuições idênticas de cor e preenchimento passam a ser evitadas. A referência ao visual é associada no início do controlador, e o collider do inimigo em `Awake`. Não houve medição de FPS. |
| Código redundante comprovado | `BossScript.currentDistance`, validação de inteiro e exportador Blender | Removidos campo privado apenas escrito por `out`, teste de finitude em um parâmetro inteiro e uma leitura de poses sobrescrita antes de ser usada. |
| Duplicação de ferramenta | `Tests/test_character_asset.py` | Removido o segundo leitor FBX. Os testes usam `Tests/fbx_reader.py`, também utilizado pelas regressões de animação. |
| Consistência de preparação | `NoxBuild`, aviso e verificador visual | Marcador de preparação e nome do grupo de arte possuem definições compartilhadas. Os valores continuam v7; não há regeneração de recursos nesta revisão. |
| Organização | C# de execução e Editor | Padronização de espaços e quebras de linha; retirada de regiões que repetiam métodos. Removidos wrappers completos de compilação condicional somente da pasta `Assets/Editor`. |
| Organização | Python e PowerShell | Python formatado e verificado com Ruff; imports reorganizados e leituras de arquivos sem handles abertos desnecessariamente. A seleção do Unity deixou de recomendar outra versão no texto do diálogo. |

## Código preservado por compatibilidade

Campos como `jumpForce`, `slashCount`, `startTime`, `currentTime`, `shootTime` e assinaturas antigas de barras/ataques não foram eliminados por falta de chamadas aparentes. As cenas completas, animações e eventos são consumidores que não estão integralmente neste repositório. Isso exige uma migração específica antes de retirar contratos.

O teste de contratos agora usa a v7 integrada como referência, em vez de proteger apenas os primeiros scripts importados. Assim, também cobre nomes e GUIDs acrescentados nas revisões recentes.

Não houve modificação no FBX, shaders, cenas, prefabs, materiais ou configurações de física. As ferramentas Blender foram formatadas e tiveram redundâncias removidas, mas não houve nova exportação do personagem.

## Análise automatizada

- `.editorconfig`, `.gitattributes` e `dotnet format whitespace`: formatação C# e checkout consistente em LF. A primeira rodada Windows expôs conversão automática para CRLF e foi interrompida na formatação; os atributos corrigem essa divergência sem desativar a regra.
- `tools/CodeQuality`: parser Roslyn do SDK .NET 8, sem NuGet adicional. `global.json` mantém a seleção na família 8.0 mesmo quando há SDKs mais novos instalados. O CI identificou seleção automática do SDK 10 e incompatibilidade de assemblies; a seleção explícita corrige a causa. Confere 71 arquivos C# em três perfis: execução, `UNITY_EDITOR` e `DEVELOPMENT_BUILD`; cinco autoverificações conferem código válido, texto e erro dentro de diretivas.
- `pyproject.toml` e `requirements-dev.txt`: Ruff fixado, com regras de imports, sintaxe, nomes indefinidos e variáveis locais sem uso, além de formatação Python.
- `tools/Verificar-Sintaxe.ps1`: parser do PowerShell, sem executar os scripts analisados. O CI executa a verificação com PowerShell 7 e Windows PowerShell 5.1.
- Regressões anteriores de regras, preparação, contratos, FBX e curvas mantidas no CI. Execuções anteriores da mesma revisão são canceladas por concorrência para evitar acumulação.

O parser C# não resolve APIs Unity nem substitui compilação. A análise sintática de PowerShell não equivale a um conjunto completo de regras do PSScriptAnalyzer. A identificação de código morto em callbacks continua exigindo revisão das cenas e dos consumidores; não há exclusão automática baseada em ausência de chamadas.

## Resultados locais

| Verificação | Resultado |
| --- | --- |
| Regras C# | 170 verificações aprovadas, 21 além da base |
| Testes Python | 23 aprovados, sem pulos; dois novos cenários de atualização |
| Parser C# | 71 arquivos, três perfis, zero erros; cinco autoverificações aprovadas |
| Compilação isolada | 57 scripts de execução e 12 de Editor, C# 8, referências Unity 2021.1.28f1; zero erros/avisos |
| Ruff | Lint aprovado; sete arquivos Python com formatação aprovada |
| Sintaxe Python | `compileall` aprovado |
| Sintaxe PowerShell 7 | Três arquivos, zero erros |
| Formatação C# | Verificação sem alterações pendentes |

A execução local das ferramentas .NET usou o compilador Roslyn e o host diretamente, por limitação da CLI neste ambiente. O CI Windows executa os comandos documentados de `dotnet run` e `dotnet format`; o resultado remoto é registrado no PR.

## Limites

Ainda faltam importação e Play no Unity, inspeção do Inspector nos dois controladores, callbacks de cena, colisões de contato, barras de vida e build Windows. A exportação Blender não foi executada novamente. PowerShell 5.1 é validado pelo CI Windows, não pelo ambiente local Linux. O backup do atualizador é preservado, mas erros de I/O durante uma cópia continuam podendo exigir recuperação manual.

O acabamento visual e o percurso das fases da v7 continuam com o roteiro de aceite em [Correção v7](correcao-v7.md). Esta limpeza de código não homologa esses itens.
