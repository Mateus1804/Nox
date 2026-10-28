> Atualização: o modelo original foi localizado e integrado pela preparação v4. A seção histórica de modelo pendente abaixo descreve o estado anterior; os detalhes atuais estão em [Personagem Nox](personagem-nox.md).

# Física, combate e apresentação do personagem

## Referência observada

O responsável relatou funcionamento de áudio, movimentação, salto, vida, pausa e conclusão/avanço/retorno ao menu no Play do Unity. Relatou também jogador preso nas plataformas móveis e deformação do cubo. Ataque e limite de cura não foram confirmados nesse teste. O relato é uma evidência manual externa; não equivale a execução dos testes automatizados ou da build nova.

A inspeção da cena original `AirPhase1.4` identifica o grupo `Mobile Platforms` com escala `(6, 1, 8)`. O script `plataforma` tornava o jogador filho das plataformas desse grupo. Mesmo com `worldPositionStays`, girar sob uma escala não uniforme pode produzir cisalhamento. O movimento em Update também concorria com o Rigidbody do jogador em FixedUpdate.

## Decisões de implementação

- `plataforma` mantém classe, GUID e campo legado `platforms`, mas não muda hierarquia em callbacks. A referência é preservada sem continuar o comportamento defeituoso.
- `WaypointsAirPhase` usa corpo cinemático, MovePosition e FixedUpdate antes do controlador. Expõe a velocidade do próximo passo para o suporte do jogador. A chegada usa o ponto real, com tolerância de 0,01, em vez de trocar o destino a uma unidade dele.
- `NoxPlayerMotor` concentra a física comum aos dois controladores. Consulta a cena física correta, ignora o próprio corpo e triggers, aceita somente normais de apoio, amostra centro e quatro pontos dos pés e separa velocidade do suporte da velocidade de caminhada. O salto solta o apoio; a inversão de um elevador considera a velocidade anterior do suporte para não confundir a descida com um novo salto.
- A rotação física do personagem fica travada e o material de movimento sem atrito reduz aderência lateral ao pressionar paredes. O material criado é descartado e o anterior é restaurado. A solução atual atende plataformas de translação; plataformas que giram exigem uma especificação e teste próprios.
- Caixas deixam de herdar Transform do jogador: a junta FixedJoint mantém o vínculo e permite ao solver tratar obstáculos. A preparação acrescenta Rigidbody e collider sólido onde faltarem e transforma `InRange` em trigger. O prefab original Box tinha somente malha no objeto raiz e um BoxRange não trigger; essa configuração era insuficiente para uma interação física consistente.
- O dano passa por uma janela comum de proteção de 0,6 s, usando Time.time. Contatos persistentes são reavaliados; projéteis são consumidos uma vez, independentemente da ordem dos callbacks. Esse intervalo afeta a dificuldade e permanece configurável no Inspector.
- Cura nunca excede maxHealth. Vida cheia não consome o item. Morte não é revertida por uma cura tardia. O método legado BarCount permanece para compatibilidade, enquanto as fontes atuais usam TakeDamage e Heal.
- Ataques validam prefab e mira, respeitam estado/vida e não são disparados durante a interação com caixas. As referências e métodos públicos existentes permanecem disponíveis.

## Modelo do Nox

O inventário do ZIP original não contém um prefab ou modelo de personagem identificado como Nox. Os modelos encontrados nas pastas de packs têm nomes de monstros e objetos; não foram usados como substitutos por suposição. A busca não abrangeu o conteúdo das pastas individuais dos integrantes.

A troca efetiva permanece pendente do prefab/FBX original e seus recursos. O comando **Nox → Vincular modelo selecionado do Nox** está preparado para receber um asset visual no painel Project, sem scripts, Rigidbody ou colliders adicionais. A seleção explícita evita escolher um modelo com base apenas em um nome aproximado.

A ação mantém o jogador físico e suas referências, instancia o visual no filho `NoxVisual`, ajusta altura/centro/pés ao collider existente e desativa somente o MeshRenderer do cubo. Esse ajuste conserva as dimensões físicas atuais; não redefine automaticamente a altura de gameplay. A orientação e os materiais precisam de inspeção.

`NoxCharacterVisual` desativa root motion e utiliza parâmetros existentes `Speed` (float), `Grounded` (bool) e `Attack` (trigger), com nomes configuráveis no Inspector. Parâmetros ausentes não causam chamadas inválidas; um ataque sem gatilho configurado produz um aviso único. O componente não cria animações que não existam no modelo. Permanecem pendentes identificação de rig, clips, orientação e homologação visual do personagem original.

## Critérios de aceite no Unity

1. Permanecer parado e girar sobre cada plataforma, mantendo escala e proporções.
2. Caminhar até as bordas, subir, descer, saltar e trocar de apoio sem ficar preso.
3. Empurrar/soltar caixa, colidir com paredes e soltar antes de morrer/reiniciar, sem herança de escala.
4. Receber impactos simultâneos e persistentes, respeitando a janela de dano e a pausa.
5. Curar com vida 1, 2 e 3; verificar teto 3, item preservado quando cheio e ausência de ressurreição.
6. Atacar com os três golpes, confirmar dano de inimigos/chefes e conferir mira/efeitos.
7. Repetir navegação e áudio já relatados após aplicar esta revisão.
8. Após fornecer o modelo, conferir material, escala, pés, orientação, animação e câmera em todas as fases.

O menu **Nox → Verificar física em cena isolada** executa sete verificações no PhysX do editor sem salvar ou editar fases: transporte horizontal, hierarquia, escala após giro, saída do apoio ao saltar, subida do salto, elevador e descida. Esse verificador foi acrescentado, mas sua execução requer editor funcional. Não substitui o percurso real nem testa animações ou caixas.

## Referências técnicas

- [Unity 2021: Rigidbody.MovePosition](https://docs.unity3d.com/2021.1/Documentation/ScriptReference/Rigidbody.MovePosition.html).
- [Unity 2021: escala e Transform](https://docs.unity3d.com/2021.1/Documentation/Manual/class-Transform.html).
- [Unity 2021: PhysicsScene](https://docs.unity3d.com/2021.1/Documentation/ScriptReference/PhysicsScene.html).
- [Unity 2021: FixedJoint](https://docs.unity3d.com/2021.1/Documentation/Manual/class-FixedJoint.html).

Os recursos visuais originais permanecem locais, com seus avisos e licenças. Nenhum pacote ou asset de terceiros foi publicado nesta revisão.

## Continuidade da revisão

A ampliação posterior, em `revisao-geral.md`, mantém o motor de plataformas e adiciona deduplicação de golpes, áudio/progresso persistentes, proteção das ferramentas de cena e auditoria. A preparação v3 possui marca própria e backup de cenas; não requer remover manualmente a marca v2.
