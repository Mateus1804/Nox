# Qualidade da versão candidata

## Executado localmente

- Compilação dos 32 scripts de execução em C# 8, usando Roslyn e as bibliotecas gerenciadas extraídas de `Build.zip` original: sem erros e sem avisos na rodada final.
- 25 verificações executáveis das regras de vida, porcentagens, limites, entradas inválidas, índices e próxima cena: aprovadas.
- Seis testes do preparador executados em PowerShell 7.6.9 no Linux: aprovados, incluindo preservação do original, cópia dos assets, bloqueio de sobrescrita, hash alterado, travessia de diretórios, cena ausente e link simbólico.
- Conferência de metadados e preservação dos GUIDs originais durante a preparação das alterações.

O instalador portátil do SDK reportou erro ao aplicar proprietário de arquivos neste ambiente; o host .NET e o compilador Roslyn puderam ser executados diretamente. A CLI `dotnet` falhou ao consultar informações do processo. Os testes foram compilados e executados pelo host, sem alegar aprovação de comandos da CLI que não funcionaram.

## CI

O workflow `Qualidade das correções` executa as 25 verificações C# e seis testes de integração do preparador no Windows. A integração utiliza um ZIP fictício montado a partir dos 79 arquivos originais e cinco cenas de teste sem jogabilidade.

Resultado do CI: pendente nesta edição inicial; atualizar com a execução efetiva antes da entrega.

## Ainda não executado

- Importação e compilação integral dentro do Unity, incluindo Assets/Editor/NoxBuild.cs.
- Preparação dos vínculos nas cenas reais pelo editor.
- Build Windows corrigida e sua execução.
- Testes Play Mode, colisões, salto, caixas/plataformas, NavMesh, animação, áudio e sequência completa das fases.
- Migração para Unity 6, comparação visual e desempenho.

A compilação de scripts não aprova automaticamente a jogabilidade. O original pode ser aberto separadamente para comparação. Nenhuma build original foi apresentada como se contivesse as correções.
