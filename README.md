# Nox — Between Worlds

Desenvolvi o Nox em equipe durante o curso Técnico em Programação de Jogos Digitais do SENAI. É um jogo de plataforma, ação e fantasia feito em Unity e C#, com exploração, combate e desafios entre mundos.

Mantenho este repositório para recuperar o projeto original, revisar os scripts e continuar o desenvolvimento sem perder a identidade do trabalho acadêmico.

## O jogo

Nox é um espírito de panda-vermelho em busca de redenção. A jornada envolve criaturas corrompidas e o equilíbrio entre os elementos. O GDD descreve a proposta completa; a versão implementada contém as fases de ar e fogo. Água, terra, escuridão e o sistema completo de habilidades ainda não estão concluídos.

Os controles atuais são WASD para movimentação, Espaço para salto, E para interação, botão esquerdo do mouse para ataque e Esc para pausa.

## Recursos implementados

- Movimento com aceleração e frenagem, salto com tolerância de borda e buffer de entrada, plataformas móveis, caixas e alavancas.
- Combate com preparação, impacto, recuperação e combo por cliques; inimigos com aviso de ataque e percepção de obstáculos.
- Registro de acertos por golpe e alvo, limites de vida e cura e proteção entre impactos.
- Estados de menu, partida, pausa, vitória e derrota, com conclusão da jornada e retomada de fase.
- Menus em português, navegação por teclado, vida numérica, objetivos e dicas contextuais.
- Volume, silêncio, sombras e preferências de apresentação persistentes; desbloqueios salvos e retomada pelo início da última fase.
- Pausa automática ao perder o foco da janela.
- Ferramentas de preparação das cenas, backup, auditoria de referências, conferência das cópias e recuperação após falhas detectadas de atualização.
- Ambientação das fases de ar e fogo com céu próprio, névoa, iluminação, ilhas distantes e detalhes de plataformas.

A retomada não salva posição, vida ou estado dos objetos. A existência desses recursos no código não substitui a validação da revisão atual no Play e na build.

## Personagem

O personagem original foi recuperado de `Nox.blend`, com malha de 1.994 vértices e esqueleto de 52 ossos. Os ciclos de repouso e caminhada possuem repetição explícita e cadência ligada à velocidade do jogador. A revisão v6 acrescenta estados próprios de queda, pouso e morte. O modelo inclui materiais, duas machadinhas visuais e poses de salto e ataque.

O personagem e o Animator v7 continuam em uso. A preparação v8 aplica a nova estrutura de scripts e converte os componentes antigos nas cenas, preservando os componentes físicos. A imagem e a procedência do asset estão em [Personagem Nox](docs/personagem-nox.md).

## Estado do projeto

No primeiro teste no Play do Unity, confirmei áudio, movimentação, salto, vida, pausa e navegação entre fases. Também encontrei aprisionamento e deformação do cubo nas plataformas móveis, que motivaram a revisão da física.

As revisões posteriores passaram pelas verificações registradas em [Qualidade](docs/qualidade.md). A substituição do cubo pelo personagem foi confirmada no Play após preparar as cenas. Após o teste da v6, foram relatados problemas de orientação e pose do personagem, travessia e apresentação das fases. A revisão v7 normaliza o FBX, calibra a direção pelo rosto, corrige um vão medido e amplia a aplicação do acabamento ao chão e ao fundo. Essas correções ainda precisam de validação no Play e fora do editor; não há uma nova build Windows homologada disponível neste repositório.

## Estrutura

- `Assets/Scripts/`: `Player`, `Combat`, `Enemies`, `World`, `UI`, `Core`, `Domain` e adaptadores em `Compatibility`.
- `Assets/Art/`: shader do céu e recursos gerados pelo preparador.
- `Assets/Characters/Nox/`: modelo FBX com rig, materiais e animações.
- `Assets/Editor/`: ferramentas separadas em `Build`, `Character`, `Scenes` e `Validation`.
- `Packages/` e `ProjectSettings/`: configuração do projeto original.
- `Tests/`: verificações de regras, preparação, contratos e integridade do FBX.
- `tools/`: recuperação, atualização e reprodução da exportação do personagem.
- `docs/`: decisões técnicas, histórico de revisão e evidências.

## Organização do código

O jogador utiliza `NoxPlayerController` e os chefes utilizam `NoxBossController`, com perfis próprios para as diferenças de cada encontro. As implementações repetidas foram consolidadas, e os arquivos de execução passaram de 57 para 52. Componentes como `NoxCrateInteraction`, `NoxMovingPlatform`, `NoxGameInterface` e `NoxLevelResultScreen` possuem nomes que indicam sua função.

Os tipos antigos ficam em adaptadores pequenos para carregar o projeto original; a preparação converte as cenas habilitadas. Renomeações mantêm GUIDs e nomes de serialização. A migração possui backup e ainda exige conferência no Unity antes de homologar a revisão. O mapa e os detalhes estão em [Estrutura de scripts](docs/estrutura-scripts.md).

## Desenvolvimento e qualidade

A referência original utiliza Unity **2021.1.28f1**. O projeto mantém a configuração original, sem migração homologada para Unity 6. Os procedimentos de recuperação e atualização estão em [Desenvolvimento](docs/desenvolvimento.md).

Cenas, músicas e demais recursos do projeto completo permanecem no ZIP original. O repositório reúne o código, as configurações e o personagem exportado. O preparador combina esses arquivos em uma cópia local e preserva o original.

O CI verifica formatação e sintaxe C#, lint e formatação Python, sintaxe PowerShell, regras de jogo, recuperação e atualização, contratos dos componentes e integridade do FBX. As rotinas repetidas dos controladores possuem suporte compartilhado, mantendo os campos e callbacks das cenas. Compilação isolada e testes automatizados não comprovam física, renderização, áudio ou jogabilidade no Unity.

## Documentação

- [Estrutura de scripts e migração v8](docs/estrutura-scripts.md)
- [Boas práticas de manutenção](docs/boas-praticas.md)
- [Revisão de código e limpeza](docs/revisao-clean-code.md)
- [Recuperação verificada do atualizador](docs/revisao-atualizador.md)

- [Desenvolvimento e recuperação](docs/desenvolvimento.md)
- [Revisão geral](docs/revisao-geral.md)
- [Física e personagem](docs/fisica-e-personagem.md)
- [Correção de pose, travessia e cenário v7](docs/correcao-v7.md)
- [Acabamento de jogabilidade e apresentação v6](docs/acabamento-v6.md)
- [Animação e ambientação v5 — histórico](docs/animacao-e-cenarios.md)
- [Personagem original e integração](docs/personagem-nox.md)
- [Resultados e limites da validação](docs/qualidade.md)

## Equipe e créditos

O GDD identifica João Vitor Vieira Cardoso, Lucas Alfredo Brandão e Mateus William Alves Torres Bento Magalhães como autores do projeto acadêmico. Os avisos e as licenças dos recursos de terceiros devem ser preservados; nenhuma licença geral foi aplicada a esses materiais.
