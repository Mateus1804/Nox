# Correção v7 — pose, travessia e cenário

## Motivo da revisão

No teste da v6 foram relatados movimento com orientação invertida, personagem deitado em repouso, saltos difíceis ou impossíveis e pouca mudança perceptível no cenário. A revisão parte da v6 já integrada na principal (`471c444f7d0a48e3ce71dd106f79cde74beb4d09`). A aprovação anterior de regras e compilação não comprovava essas condições no Unity.

## Personagem

O FBX anterior conservava transformações diferentes no wrapper, armature e malhas. A exportação agora reconstrói uma hierarquia normalizada na cópia de trabalho, mantendo a malha acadêmica, pesos, materiais, nomes dos ossos e sete ações. As poses são amostradas e convertidas para o novo espaço de bind; o arquivo `Nox.blend` original não é alterado.

O preparador usa explicitamente o repouso, em vez de depender do take padrão. A direção é calculada pela posição dos olhos em relação ao osso da cabeça **depois da importação pelo Unity**, sem assumir que frente corresponde a um eixo específico do Blender. Um filho `NoxModelAlignment` conserva essa correção; o pivô externo `NoxVisual` acompanha somente a direção horizontal do jogador. A animação não deve escrever nesse alinhamento.

Clips próprios em `Assets/Characters/Nox/Generated-v7` removem apenas curvas de Transform com caminho vazio, que poderiam mover o objeto de importação. Todas as curvas dos ossos são preservadas. A orientação original é mantida nas configurações de importação. Prefab e controller usam caminhos v7, para não reaproveitar silenciosamente um controller gerado em revisão anterior.

A animação de morte só é habilitada quando o personagem está sem vida **e** a sessão está em derrota. Isso evita usar apenas uma leitura de vida ainda não inicializada como autorização para a pose de morte. É uma proteção de estado; a causa exata de cada ocorrência relatada depende da reprodução no projeto preparado.

## Travessia

O levantamento da cena acadêmica `AirPhase1.4` encontrou:

| Trecho | Borda de saída | Borda de chegada | Vão | Altura do destino |
| --- | --- | --- | --- | --- |
| Platform19 → Platform20 | x = 205,91 | x = 216,00 | 10,09 | 1 unidade abaixo |

Com velocidade 6, altura de salto 3,5 e gravidade de magnitude 15, o alcance balístico em terreno nivelado é aproximadamente 8,20 unidades. Mesmo com o destino 1 unidade abaixo, a estimativa continua inferior ao vão medido. Essa estimativa não inclui tamanho do personagem, aceleração ou obstáculos.

A correção adiciona um apoio de 3,5 × 7 unidades, com topo em y = 48,5, no meio desse trecho. Os dois vãos ficam próximos de 3,30 unidades. O apoio recebe collider e a camada do chão; a decoração continua sem collider. A alteração só ocorre nas coordenadas acadêmicas conhecidas. Se as plataformas tiverem sido personalizadas, a ferramenta avisa e não modifica o trecho automaticamente.

Não houve aumento global do salto ou aproximação automática de todas as plataformas: isso poderia descaracterizar puzzles e trajetos com plataformas móveis. O relatório gerado fica em `Logs/Nox-Travessia-v7.txt`. Os demais trechos continuam sujeitos ao percurso completo no Play.

## Chão e fundo

A identificação das primitivas de chão passa a considerar a máscara de apoio dos jogadores e suas dimensões, além dos nomes existentes. Isso inclui pisos com nomes genéricos, sem substituir indiscriminadamente personagens, perigos ou modelos artísticos. O limite anterior de 100 superfícies foi removido da geração no Editor.

O acabamento inclui variação mineral procedural em coordenadas de mundo, sem texturas externas. O shader mantém iluminação e sombras do pipeline original. O céu recebeu transição abaixo do horizonte para evitar uma área inferior inteiramente uniforme no enquadramento inclinado.

O fundo passa a usar duas faixas atrás do percurso, orientadas pela câmera principal. A altura das ilhas acompanha as superfícies próximas. A disposição anterior usava as extremidades da extensão total do nível, podendo deixar a cenografia distante do enquadramento. As novas peças não têm collider. Os recursos v5 e v6 ficam preservados e desativados; a revisão gera `NoxArt-v7`, `NoxSurface-v7` e `Assets/Art/Generated-v7`.

## Aplicação e proteção das cenas

O marcador atual é `ProjectSettings/NoxScenesPrepared-v7.txt`. Instalar arquivos não basta para modificar cenas recuperadas: a ferramenta precisa executá-las no Editor. Um aviso na abertura do projeto identifica essa preparação pendente e oferece acesso ao comando existente.

Antes da gravação, as cenas são copiadas para `Backups/Preparacao-v7`. A ferramenta valida no próprio Unity a verticalidade e a altura dos estados de repouso, caminhada, salto, queda e pouso em cinco pontos de cada clip. Um erro interrompe a preparação. Há também o comando **Nox → Reaplicar preparação v7 (com backup)**, com confirmação explícita. Recursos personalizados dentro dos grupos visuais gerados podem ser substituídos nessa reaplicação; o backup das cenas permanece disponível.

A opção **Nox → Verificar animações e cenário** confere poses importadas, orientação do rosto nas cenas, shaders, controller e iluminação. Essas verificações foram compiladas, mas não executadas neste ambiente.

## Evidências e limites

- 149 verificações C# aprovadas, incluindo dez casos novos de alcance e entradas inválidas.
- 21 testes Python aprovados, sem pulos: nove de preparação, quatro de contratos, quatro de integridade do FBX e quatro de animação/hierarquia.
- 56 scripts de execução e 12 de Editor compilados contra as referências reais do Unity 2021.1.28f1, com avisos como erros: zero erros e avisos. A validação local das regras utilizou Roslyn e o host .NET; a inicialização da CLI `dotnet run` falhou neste ambiente ao consultar informações de processo. O CI executa o comando normal no Windows.
- Exportação e reimportação reais no Blender 3.6.23. Comparação da malha deformada em 35 poses com a representação anterior à exportação: maior desvio inferior a 0,00006 unidade. O repouso mantém altura próxima de 1,70 m. A comparação usa a malha avaliada, não apenas existência de curvas. Dados em [Malha reimportada](evidencias/malha-reimportada-v7.json).
- O teste estrutural novo confere uma conversão de eixo e unidade consistente entre rig e malhas do FBX.

Ainda não foram executados: importação e preparação da v7 no Unity, verificação do Animator no Play, percurso completo, inspeção visual do novo shader/cenário, Profiler e build Windows. Não há captura de gameplay ou executável homologado desta revisão. A conferência no Blender não substitui esses testes.

## Aceite no projeto completo

1. Atualizar uma cópia recuperada com o Unity fechado e abrir o projeto na versão atual.
2. Aplicar a preparação v7 e executar as verificações dos menus Nox.
3. Conferir repouso, deslocamento nas quatro direções, giros, salto, queda e morte; o rosto deve acompanhar a direção e o corpo permanecer vertical em repouso.
4. Percorrer o trecho das plataformas 19/20 e as outras fases, incluindo caixas, elevadores, perigos e chefes.
5. Conferir o chão e as duas camadas de fundo pela câmera real; ajustar composição conforme as cenas e sem esconder perigos.
6. Gerar e testar uma build Windows antes de distribuir a revisão.
