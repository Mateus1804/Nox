# Validação da integração do personagem

A integração atual está descrita em `personagem-nox.md`. O FBX foi reimportado no Blender com 52 ossos, quatro clips, cinco malhas e sete materiais, sem imagens externas. Localmente passaram 87 verificações C#, oito testes do preparador/atualizador, quatro de contratos e quatro do FBX. Os 43 scripts de execução e seis de Editor compilaram isoladamente contra as bibliotecas originais, com avisos como erros. Nenhuma importação/Play/build no Unity foi executada nesta rodada.

Os registros anteriores abaixo são históricos; a antiga pendência de localizar o modelo foi resolvida nesta integração. A validação no Unity permanece pendente.

# Qualidade da recuperação e revisões

## Revisão geral e funcionalidades — 26/10/2026

- [CI Windows 40267779204](https://github.com/Mateus1804/Nox/actions/runs/40267779204), código `122937b24487a27a761467425dc298d0f8c7776c`: **87 verificações C#, oito testes de preparação/atualização e quatro testes de contratos aprovados**, sem testes pulados. Foram conferidos status, etapas e logs do job `121393822366`.
- A rodada de push `40267774096` também concluiu com sucesso; não houve ampliação da matriz.
- Execução local: mesmas 87 verificações e suítes de oito/quatro testes aprovadas. Os testes independentes usam .NET 8 e PowerShell 7.6.9; não executam Unity.
- Compilação isolada dos **43 scripts próprios de execução e cinco de Editor** com bibliotecas da versão original 2021.1.28f1, C# 8 e avisos tratados como erros: sem erros e sem avisos.
- A revisão anterior já aprovava 49 verificações e oito testes. Os novos casos cobrem volume, identidade de golpes, retomada elegível, trava de carregamento e escolha de ação dos chefes. A suíte de contratos confere nomes públicos/GUIDs e separação runtime/Editor; não substitui um compilador semântico ou a conferência dos UnityEvents.
- O código de preparação v3, a transação/backup de cenas e a auditoria foram compilados, mas **não executados no Unity neste ambiente**. A suíte do preparador não é uma prova de restauração real das cenas por essas ferramentas de Editor.

Ainda faltam Play e inspeção visual do novo botão, teste de áudio global, perda de foco, leitura/gravação de PlayerPrefs no Unity, NavMesh, animações, golpes e geração/execução da nova build. A retomada salva o índice da fase, não a posição ou o estado completo. O modelo original Nox permanece pendente. Nenhuma build nova é apresentada como validada.

O relatório `revisao-geral.md` diferencia bugs de lógica, riscos de configuração, mudanças de organização e funcionalidades acrescentadas. `evidencias/revisao-geral-verificada.json` registra o código e os hashes conferidos. Commits posteriores de documentação não alteram os scripts e ferramentas validados.

## Revisão de física e interação — 26/10/2026

- Baseline executável das regras: 25 verificações aprovadas antes de ampliar a suíte.
- Nova suíte C#: 49 verificações aprovadas, incluindo limites de cura, proteção de dano, impactos simultâneos, morte, entradas inválidas e classificação de apoio.
- Compilação isolada dos 35 scripts de execução: zero erros e avisos, com C# 8 e as bibliotecas da build original.
- Compilação isolada dos três scripts de Editor, contra as bibliotecas oficiais 2021.1.28f1: zero erros e avisos. Uma duplicidade de referências no comando temporário de compilação foi removida; não foi alterado o projeto Unity para contorná-la.
- Oito testes de integração em PowerShell 7.6.9/Linux: aprovados, incluindo os seis de recuperação, atualização com cópia dos scripts anteriores/preservação das cenas e rejeição de pasta inválida. Nenhum teste foi pulado.
- GUIDs dos componentes existentes preservados; a ordem de execução de WaypointsAirPhase passou para -200 no metadado, consistente com o atributo C#.

A validação local foi feita pelo compilador Roslyn e pelo host .NET diretamente. Não se afirma que uma compilação integral do Unity foi executada. O [CI Windows 40265063605](https://github.com/Mateus1804/Nox/actions/runs/40265063605), com o código do commit `b1d42ff94800ec979e02f4483ae4b8fc0df459ac` no merge temporário da PR, aprovou as 49 verificações C# e os oito testes do preparador/atualizador, sem testes pulados. Foram consultados o resultado do job e suas mensagens de execução. Isso não inclui o motor Unity. A rodada de push 40265061435 também foi disparada; não é necessária outra matriz de plataformas para esta etapa.

### Relato manual recebido

O responsável informou funcionamento no Play de áudio e silêncio, movimentação, salto, vida, pausa e fluxo de conclusão/avanço/retorno. Informou também aprisionamento e deformação do cubo em plataformas. Ataque e limite de cura permaneceram sem confirmação. Essa observação corresponde à revisão anterior e não aprova automaticamente o novo código.

### Limites desta revisão

O verificador de sete cenários físicos foi compilado, mas não executado no motor Unity neste ambiente. Os trajetos reais, caixas, combate, dano contínuo, visual do Nox e a nova build Windows permanecem pendentes de Play e execução do jogo. O modelo original do personagem não foi localizado no inventário do ZIP; o comando de vinculação está implementado, mas a substituição não foi apresentada como concluída. Pastas individuais dos colaboradores não foram usadas como fonte de assets.

Os testes do atualizador conferem dados e backups em fixtures. A janela de seleção de pasta e o PowerShell 5.1 do usuário não foram executados neste ambiente.

## Histórico: primeira recuperação


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
