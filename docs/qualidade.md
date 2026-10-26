# Qualidade da versão candidata

## Executado localmente

- Compilação dos 32 scripts de execução em C# 8, usando Roslyn e as bibliotecas gerenciadas extraídas de `Build.zip` original: sem erros e sem avisos na rodada final.
- Compilação dos 71 scripts de execução da cópia completa preparada (incluindo exemplos de pacotes): sem erros, com um aviso CS0169 de campo não utilizado no exemplo Benchmark01 do TextMesh Pro.
- Recuperação usando o ZIP original de 1.773.937.352 bytes: concluída em pasta nova, com 3.688 arquivos e sem extrair Library.
- Compilação isolada de Assets/Editor/NoxBuild.cs contra as bibliotecas do editor original 2021.1.28f1: sem erros ou avisos após corrigir uma sobrecarga de API indisponível nessa versão. Isso confere tipos e assinaturas, não executa as ações de cena.
- 25 verificações executáveis das regras de vida, porcentagens, limites, entradas inválidas, índices e próxima cena: aprovadas.
- Seis testes do preparador executados em PowerShell 7.6.9 no Linux: aprovados, incluindo preservação do original, cópia dos assets, bloqueio de sobrescrita, hash alterado, travessia de diretórios, cena ausente e link simbólico.
- Conferência de metadados e preservação dos GUIDs originais durante a preparação das alterações.

O instalador portátil do SDK reportou erro ao aplicar proprietário de arquivos neste ambiente; o host .NET e o compilador Roslyn puderam ser executados diretamente. A CLI `dotnet` falhou ao consultar informações do processo. Os testes foram compilados e executados pelo host, sem alegar aprovação de comandos da CLI que não funcionaram.

## CI

O workflow `Qualidade das correções` executa as 25 verificações C# e seis testes de integração do preparador no Windows. A integração utiliza um ZIP fictício montado a partir dos 79 arquivos originais e cinco cenas de teste sem jogabilidade.

A [execução 40245039208](https://github.com/Mateus1804/Nox/actions/runs/40245039208), sobre o commit `a6e072ddc617f1bee0cb248fe7cb3d2ee22e2ac4`, aprovou as 25 verificações C# e os seis testes de integração no Windows, sem testes pulados. O trabalho de CI concluiu com sucesso. A execução não inclui Unity Editor ou testes de jogabilidade.

## Tentativa de execução do Unity

Foi instalado temporariamente o editor oficial 2021.1.28f1 (f3f9dc10f3dd) e solicitada a execução em batchmode de `NoxBuild.Windows` sobre a cópia completa preparada. O processo encerrou com código **134**, após falhas de comunicação IPC com LicensingClient e `free(): double free detected in tcache 2`. A importação e o método de build não chegaram a ser confirmados.

Não se concluiu que a licença pessoal do responsável está inválida: o erro pertence à execução do cliente de licenciamento neste ambiente. O resumo técnico está em [evidências](evidencias/unity-editor-resumo.log). Nenhuma credencial ou licença foi solicitada ou inserida.

## Ainda não executado

- Importação e compilação integral pelo próprio Unity, incluindo a integração das assemblies de Editor.
- Preparação dos vínculos nas cenas reais pelo editor.
- Build Windows corrigida e sua execução.
- Testes Play Mode, colisões, salto, caixas/plataformas, NavMesh, animação, áudio e sequência completa das fases.
- Migração para Unity 6, comparação visual e desempenho.

A compilação de scripts não aprova automaticamente a jogabilidade. O original pode ser aberto separadamente para comparação. Nenhuma build original foi apresentada como se contivesse as correções.
