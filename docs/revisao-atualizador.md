# Revisão complementar — recuperação do atualizador

## Referência e escopo

A revisão parte de `6ecec89d2ee54d0bf22ff0523757aca01982e945`, após a integração do PR #9. A base passou por 23 testes Python e as 170 verificações C# permanecem aprovadas. A formatação, análise estática e contratos introduzidos na revisão anterior são mantidos.

O ponto identificado foi o tratamento de falhas em `tools/Atualizar-Nox.ps1`: a validação prévia impedia conflitos de caminhos, mas um erro de I/O durante as cópias podia deixar uma combinação de scripts novos e antigos. Uma cópia que retornasse sem erro, mas com conteúdo incorreto, também não era conferida.

Não houve alteração em C#, cenas, animações, FBX, shaders, física, dados de progresso ou GUIDs. A revisão concentra-se no atualizador, seus testes e documentação.

## Refatoração

O fluxo principal ficou responsável por coordenar etapas com funções próprias:

| Função | Responsabilidade |
| --- | --- |
| `Get-NoxUpdatePlan` | Conferir caminhos e registrar existência e hashes originais e de origem. |
| `New-NoxUpdateBackup` | Copiar as pastas gerenciadas e conferir os arquivos que serão sobrescritos antes da primeira escrita. |
| `Copy-NoxVerifiedFile` | Copiar um arquivo e comparar seu SHA-256 com o conteúdo esperado. |
| `Restore-NoxAttemptedUpdates` | Reverter, em ordem inversa, os arquivos cuja cópia foi tentada e reunir falhas de recuperação. |

A lista de arquivos tentados é registrada antes de cada cópia, cobrindo também arquivos parcialmente escritos. Arquivos que já existiam são restaurados do backup e conferidos por hash. Arquivos novos daquela tentativa são removidos. Pastas vazias eventualmente criadas podem permanecer; a recuperação não remove árvores de diretórios ou dados externos ao plano.

A implementação utiliza os comandos nativos de PowerShell, sem nova dependência de execução. A lista crescente de caminhos passou a usar uma coleção própria em vez de reconstruir um array com `+=` a cada cópia.

## Resultados e diagnóstico

A conclusão normal conserva `arquivos-atualizados.json` no backup. Uma exceção tratada gera `recuperacao.json`, contendo a falha original, arquivos tentados, falhas de recuperação e um dos estados:

- `sem-alteracoes`: a atualização falhou antes de tentar escrever no projeto; o backup pode estar incompleto.
- `restaurado`: os arquivos tentados retornaram ao conteúdo anterior e os arquivos novos foram removidos.
- `recuperacao-parcial`: algum arquivo não pôde ser restaurado; a mensagem e o relatório identificam o que ficou pendente.

O erro original não é substituído por um diagnóstico genérico de sucesso. Se a gravação do relatório também falhar, essa informação é acrescentada à mensagem de erro.

## Regressões

A simulação de falhas fica exclusivamente em `Tests/fixtures/Updater-Faults.ps1`, executada em processos de teste e pastas temporárias. O atualizador de produção não possui opção para simular erros.

Os testes comparam os hashes e o conjunto de todos os arquivos do projeto temporário, incluindo cenas fictícias, recursos preservados e arquivos novos:

1. Falha depois de escrever parte de um arquivo: restaurar versões antigas e remover arquivos recém-adicionados.
2. Cópia silenciosamente corrompida: detectar a diferença de hash e restaurar os bytes anteriores.
3. Falha ao criar backup: manter o projeto sem alterações e indicar que a cópia pode estar incompleta.
4. Falha durante a própria recuperação: continuar restaurando os demais arquivos e informar os pendentes, preservando a causa original.
5. Falha ao gravar o registro final: reverter as cópias já concluídas.

Os cenários 1, 2 e 5 foram executados contra a base e falharam antes da correção. Passaram após a implementação. A seleção do executável PowerShell nos testes foi centralizada em `NOX_TEST_POWERSHELL`, com `pwsh` como padrão. O CI Windows também executa os cinco casos com `powershell` para conferir o host 5.1 utilizado pelo lançador CMD.

## Validação local

- **28 testes Python aprovados**, sem pulos: 16 de preparação/atualização, quatro de contratos, quatro de integridade FBX e quatro de animação/hierarquia.
- **170 verificações C# aprovadas**; a implementação das regras não foi alterada.
- Parser C#: **71 arquivos, três perfis, zero erros**, com cinco autoverificações.
- Lint e formatação Ruff aprovados nos sete arquivos Python; `compileall` aprovado.
- Formatação C# sem alterações pendentes.
- Parser PowerShell 7: **quatro arquivos, zero erros**, incluindo a fixture.

As ferramentas .NET foram executadas localmente por Roslyn/host por limitação da CLI. Os resultados Windows e do PowerShell 5.1 são registrados no PR após a execução do CI; não são apresentados como execução local Linux.

## Limites

A recuperação é acionada em exceções tratadas pelo processo. Não equivale a uma transação de sistema de arquivos: queda de energia, encerramento forçado, corrupção de disco ou alteração concorrente por outro processo não estão cobertos. Se a restauração não puder escrever um arquivo, será necessária recuperação manual pelo backup indicado. A verificação cobre conteúdo dos arquivos, não uma homologação de todos os atributos ou permissões do sistema de arquivos.

O Unity deve continuar fechado durante a atualização. Os testes usam projetos fictícios e não comprovam Play, importação de cenas ou funcionamento da build Windows. A compilação isolada dos scripts Unity registrada na revisão anterior permanece como evidência histórica; não foi repetida, pois nenhum desses scripts foi alterado neste complemento.
