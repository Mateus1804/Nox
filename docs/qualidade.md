# Qualidade e validação

## Revisão de código e limpeza

A manutenção após a v7 passou por **170 verificações C# e 23 testes Python**, sem pulos, além de lint e formatação. A compilação isolada de **57 scripts de execução e 12 de Editor** foi concluída sem erros ou avisos. O novo parser confere **71 arquivos C# em três perfis**, e o parser PowerShell local confere três scripts. Resultados, referência inicial, correções reproduzidas e limites em [Revisão de código e limpeza](revisao-clean-code.md). As convenções estão em [Boas práticas](boas-praticas.md).

Esses resultados não substituem importar o projeto, testar cenas no Play ou validar uma build Windows.

## Correção v7 — pose, percurso e cenário

A revisão parte da v6 integrada em `471c444f7d0a48e3ce71dd106f79cde74beb4d09`, após relato de orientação incorreta, pose deitada, trechos difíceis de atravessar e cenário sem melhoria perceptível.

- **149 verificações C# e 21 testes Python aprovados**, sem pulos. Dez verificações novas tratam o alcance balístico e suas entradas inválidas; um teste novo confere a conversão uniforme do rig e das malhas no FBX.
- **56 scripts de execução e 12 de Editor compilados isoladamente**, C# 8, referências oficiais Unity 2021.1.28f1, avisos como erros: zero erros e avisos.
- Exportação e reimportação reais no Blender 3.6.23: **35 poses comparadas**, com desvio máximo inferior a `0,00006` unidade em relação à malha antes da exportação; altura de repouso próxima de 1,70 m, esqueleto com 52 ossos e sete ações. Resultado em [Malha reimportada v7](evidencias/malha-reimportada-v7.json).
- A preparação passa a verificar no próprio Unity a verticalidade e a altura de cinco estados antes de regravar as cenas. Essa verificação foi **compilada, mas não executada neste ambiente**.

O levantamento dos vãos usou as transformações das cenas acadêmicas. Não substitui percorrer todas as fases: obstáculos, aceleração, plataformas móveis e colisões precisam de Play. A revisão não possui homologação visual no Unity, percurso completo ou build Windows testada. O resultado do CI é registrado no PR correspondente.

Detalhes em [Correção v7](correcao-v7.md).

## Revisão v6 — acabamento

A referência inicial é o PR #6, commit `e70c6c2ca59ef397ff1a1e07197d4f18477d5d36`, ainda não integrado na conferência inicial. A base passou por **96 verificações C# e 19 testes Python**, sem falhas.

- **139 verificações C# aprovadas**, incluindo preparação/impacto/recuperação, combo, expiração de entrada, cancelamento, antecipação inimiga, perda de visão, recuperação e cadência.
- **20 testes Python aprovados**, sem pulos: nove de preparação/atualização, quatro de contratos, quatro do FBX e três de curvas de animação. O novo teste confere preservação de marcador, Animator e material personalizado da v5, além do backup.
- **55 scripts de execução e 10 de Editor compilados isoladamente**, C# 8, referências oficiais Unity 2021.1.28f1, avisos como erros: zero erros e avisos. Foi corrigido durante a revisão o uso de um getter de UnityEvent indisponível nessa versão; o estado passou a ser lido pela representação serializada.
- Testes de arquivos conferem sete ações no FBX, assinatura/hash, skin, materiais, ausência de imagens e curvas fechadas de repouso/caminhada.
- Exportação e reimportação do personagem no Blender 3.6.23 foram executadas. O validador conferiu os dois pés em um ciclo fechado, as sete ações e a estrutura do rig; a prancha de poses foi inspecionada. Evidências em `evidencias/animacoes-v6.json` e `animacoes-v6.png`.

O CI do novo PR executa as mesmas regras e testes Python; o resultado remoto é registrado na descrição do PR após a execução. Os números desta seção são da validação local. As ferramentas isoladas do Unity, o percurso das fases, as capturas de gameplay, o Profiler e a build Windows **não foram executados nesta revisão**.

Os binários temporários de validação precisaram ser recuperados após a retomada do ambiente. A execução das regras usou Roslyn/host .NET; não é apresentada como execução da CLI local ou do Unity. A validação do Editor continua sendo compilação de tipos e assinaturas, sem homologação dos fluxos visuais.

Hashes dos arquivos conferidos e resumo de execução em `evidencias/acabamento-v6.json`.

Detalhes, alterações de balanceamento, riscos restantes e roteiro de aceite em [Acabamento v6](acabamento-v6.md).

## Revisão v5 — animação e ambientação

- Base antes das alterações: 87 verificações C#, oito testes de preparação/atualização, quatro de contratos e quatro de FBX aprovados. A primeira tentativa Python não encontrou `pwsh`; após incluir o executável portátil no PATH, a base passou.
- Revisão: **96 verificações C#** e **19 testes Python** aprovados localmente, sem testes pulados. Os testes Python incluem oito de preparação/atualização, quatro de contratos, quatro de FBX e três das curvas de animação.
- **44 scripts de execução e oito de Editor** compilados com C# 8, referências oficiais Unity 2021.1.28f1 e avisos como erros: sem erros ou avisos. Essa é compilação isolada; não é compilação pelo Unity Editor.
- Exportação pelo Blender 3.6.23, reimportação do FBX e amostragem avaliada dos pés: concluídas. Caminhada de 0,8 s com fechamento das curvas e alternância dos dois pés. Hash e estrutura em `personagem-nox.json`.
- GIF do asset reimportado inspecionado: `caminhada-blender.gif`. É evidência do Blender, sem captura de gameplay.
- O CI do PR executa as regras C#, as oito verificações de preparação, os quatro contratos, os quatro testes do FBX e os três testes dos ciclos exportados. Os resultados remotos ficam nos checks e na descrição do PR; os números acima são da execução local.

A vinculação do personagem da revisão anterior foi confirmada no Play. A **v5 ainda não foi executada no Unity neste ambiente**: shader, preparação das cenas, iluminação, materiais, física, menus de diagnóstico, build Windows e percurso real permanecem sem homologação. O impedimento de execução do Unity registrado no histórico continua sendo uma limitação deste ambiente.

A rotina de teste local compilou os scripts com Roslyn e executou as regras pelo host .NET; não foi informado sucesso da CLI `dotnet run` local. Os testes de preparação usam ZIP fictício, preservação de cenas e backup de scripts, FBX e shader. Não são testes de cenas jogáveis. Detalhes e critérios do Play em [Animação e cenários](animacao-e-cenarios.md).

## Histórico anterior

A integração atual está descrita em `personagem-nox.md`. O FBX foi reimportado no Blender com 52 ossos, quatro clips, cinco malhas e sete materiais, sem imagens externas. Localmente passaram 87 verificações C#, oito testes do preparador/atualizador, quatro de contratos e quatro do FBX. Os 43 scripts de execução e seis de Editor compilaram isoladamente contra as bibliotecas originais, com avisos como erros. O CI Windows 40611671152 aprovou as quatro suítes (87 verificações e 8/4/4 testes), sem pulos. Nenhuma importação/Play/build no Unity foi executada nesta rodada.

Os registros anteriores abaixo são históricos; a antiga pendência de localizar o modelo foi resolvida nesta integração. A validação no Unity permanece pendente.

# Qualidade da recuperação e revisões

## Revisão geral e funcionalidades — 26/10/2026

- [CI Windows 40267779204](https://github.com/Mateus1804/Nox/actions/runs/40267779204), código `122937b24487a27a761467425dc298d0f8c7776c`: **87 verificações C#, oito testes de preparação/atualização e quatro testes de contratos aprovados**, sem testes pulados. Foram conferidos status, etapas e logs do job `121393822366`.
- A rodada de push `40267774096` também concluiu com sucesso; não houve ampliação da matriz.
- Execução local: mesmas 87 verificações e suítes de oito/quatro testes aprovadas. Os testes independentes usam .NET 8 e PowerShell 7.6.9; não executam Unity.
- Compilação isolada dos **43 scripts próprios de execução e cinco de Editor** com bibliotecas da versão original 2021.1.28f1, C# 8 e avisos tratados como erros: sem erros e sem avisos.
- A revisão anterior já aprovava 49 verificações e oito testes. Os novos casos cobrem volume, identidade de golpes, retomada elegível, trava de carregamento e escolha de ação dos chefes. A suíte de contratos confere nomes públicos/GUIDs e separação runtime/Editor; não substitui um compilador semântico ou a conferência dos UnityEvents.
- O código de preparação v3, a transação/backup de cenas e a auditoria foram compilados, mas **não executados no Unity na execução registrada**. A suíte do preparador não é uma prova de restauração real das cenas por essas ferramentas de Editor.

Ainda faltam Play e inspeção visual do novo botão, teste de áudio global, perda de foco, leitura/gravação de PlayerPrefs no Unity, NavMesh, animações, golpes e geração/execução da nova build. A retomada salva o índice da fase, não a posição ou o estado completo. Naquela etapa, o modelo original Nox ainda não havia sido localizado. Nenhuma build nova é apresentada como validada.

O relatório `revisao-geral.md` diferencia bugs de lógica, riscos de configuração, mudanças de organização e funcionalidades acrescentadas. `evidencias/revisao-geral-verificada.json` registra o código e os hashes conferidos. Commits posteriores de documentação não alteram os scripts e ferramentas validados.

## Revisão de física e interação — 26/10/2026

- Baseline executável das regras: 25 verificações aprovadas antes de ampliar a suíte.
- Nova suíte C#: 49 verificações aprovadas, incluindo limites de cura, proteção de dano, impactos simultâneos, morte, entradas inválidas e classificação de apoio.
- Compilação isolada dos 35 scripts de execução: zero erros e avisos, com C# 8 e as bibliotecas da build original.
- Compilação isolada dos três scripts de Editor, contra as bibliotecas oficiais 2021.1.28f1: zero erros e avisos. Uma duplicidade de referências no comando temporário de compilação foi removida; não foi alterado o projeto Unity para contorná-la.
- Oito testes de integração em PowerShell 7.6.9/Linux: aprovados, incluindo os seis de recuperação, atualização com cópia dos scripts anteriores/preservação das cenas e rejeição de pasta inválida. Nenhum teste foi pulado.
- GUIDs dos componentes existentes preservados; a ordem de execução de WaypointsAirPhase passou para -200 no metadado, consistente com o atributo C#.

A validação local foi feita pelo compilador Roslyn e pelo host .NET diretamente. Não se afirma que uma compilação integral do Unity foi executada. O [CI Windows 40265063605](https://github.com/Mateus1804/Nox/actions/runs/40265063605), com o código do commit `b1d42ff94800ec979e02f4483ae4b8fc0df459ac` no merge temporário da PR, aprovou as 49 verificações C# e os oito testes do preparador/atualizador, sem testes pulados. Foram consultados o resultado do job e suas mensagens de execução. Isso não inclui o motor Unity. A rodada de push 40265061435 também foi disparada; não é necessária outra matriz de plataformas para esta etapa.

### Teste manual inicial

No primeiro teste no Play, confirmei áudio e silêncio, movimentação, salto, vida, pausa e conclusão, avanço e retorno ao menu. Encontrei aprisionamento e deformação do cubo nas plataformas. Ataque e limite de cura não foram conferidos nessa sessão. Esse registro corresponde à revisão anterior, sem validar automaticamente as alterações posteriores.

### Limites desta revisão

O verificador de sete cenários físicos foi compilado, mas não executado no motor Unity na execução registrada. Os trajetos reais, caixas, combate, dano contínuo, visual do Nox e a nova build Windows permanecem pendentes de Play e execução do jogo. Naquela etapa, o modelo original do personagem não havia sido localizado no inventário do ZIP; o comando de vinculação está implementado, mas a substituição não foi apresentada como concluída. Pastas individuais dos colaboradores não foram usadas como fonte de assets.

Os testes do atualizador conferem dados e backups em fixtures. A janela de seleção de pasta e a execução com PowerShell 5.1 não fazem parte dessa validação.

## Histórico: primeira recuperação


## Executado localmente

- Compilação dos 32 scripts de execução em C# 8, usando Roslyn e as bibliotecas gerenciadas extraídas de `Build.zip` original: sem erros e sem avisos na rodada final.
- Compilação dos 71 scripts de execução da cópia completa preparada (incluindo exemplos de pacotes): sem erros, com um aviso CS0169 de campo não utilizado no exemplo Benchmark01 do TextMesh Pro.
- Recuperação usando o ZIP original de 1.773.937.352 bytes: concluída em pasta nova, com 3.688 arquivos e sem extrair Library.
- Compilação isolada de Assets/Editor/NoxBuild.cs contra as bibliotecas do editor original 2021.1.28f1: sem erros ou avisos após corrigir uma sobrecarga de API indisponível nessa versão. Isso confere tipos e assinaturas, não executa as ações de cena.
- 25 verificações executáveis das regras de vida, porcentagens, limites, entradas inválidas, índices e próxima cena: aprovadas.
- Seis testes do preparador executados em PowerShell 7.6.9 no Linux: aprovados, incluindo preservação do original, cópia dos assets, bloqueio de sobrescrita, hash alterado, travessia de diretórios, cena ausente e link simbólico.
- Conferência de metadados e preservação dos GUIDs originais durante a preparação das alterações.

O instalador portátil do SDK reportou erro ao aplicar proprietário de arquivos na execução registrada; o host .NET e o compilador Roslyn puderam ser executados diretamente. A CLI `dotnet` falhou ao consultar informações do processo. Os testes foram compilados e executados pelo host, sem alegar aprovação de comandos da CLI que não funcionaram.

## CI

O workflow `Qualidade das correções` executa as 25 verificações C# e seis testes de integração do preparador no Windows. A integração utiliza um ZIP fictício montado a partir dos 79 arquivos originais e cinco cenas de teste sem jogabilidade.

A [execução 40245039208](https://github.com/Mateus1804/Nox/actions/runs/40245039208), sobre o commit `a6e072ddc617f1bee0cb248fe7cb3d2ee22e2ac4`, aprovou as 25 verificações C# e os seis testes de integração no Windows, sem testes pulados. O trabalho de CI concluiu com sucesso. A execução não inclui Unity Editor ou testes de jogabilidade.

## Tentativa de execução do Unity

Foi instalado temporariamente o editor oficial 2021.1.28f1 (f3f9dc10f3dd) e solicitada a execução em batchmode de `NoxBuild.Windows` sobre a cópia completa preparada. O processo encerrou com código **134**, após falhas de comunicação IPC com LicensingClient e `free(): double free detected in tcache 2`. A importação e o método de build não chegaram a ser confirmados.

A falha de comunicação com o cliente de licenciamento não permite concluir que a licença do editor seja inválida. O resumo técnico está em [evidências](evidencias/unity-editor-resumo.log).

## Ainda não executado

- Importação e compilação integral pelo próprio Unity, incluindo a integração das assemblies de Editor.
- Preparação dos vínculos nas cenas reais pelo editor.
- Build Windows corrigida e sua execução.
- Testes Play Mode, colisões, salto, caixas/plataformas, NavMesh, animação, áudio e sequência completa das fases.
- Migração para Unity 6, comparação visual e desempenho.

A compilação de scripts não aprova automaticamente a jogabilidade. O original pode ser aberto separadamente para comparação. Nenhuma build original foi apresentada como se contivesse as correções.