# Nox — Between Worlds

Desenvolvi o Nox em equipe durante o curso Técnico em Programação de Jogos Digitais do SENAI. É um jogo de plataforma, ação e fantasia feito em Unity e C#, com exploração, combate e desafios entre mundos.

Mantenho este repositório para recuperar o projeto original, revisar os scripts e continuar o desenvolvimento sem perder a identidade do trabalho acadêmico.

## O jogo

Nox é um espírito de panda-vermelho em busca de redenção. A jornada envolve criaturas corrompidas e o equilíbrio entre os elementos. O GDD descreve a proposta completa; a versão implementada contém as fases de ar e fogo. Água, terra, escuridão e o sistema completo de habilidades ainda não estão concluídos.

Os controles atuais são WASD para movimentação, Espaço para salto, E para interação, botão esquerdo do mouse para ataque e Esc para pausa.

## Recursos implementados

- Movimento com aceleração e frenagem, salto com tolerância de borda e buffer de entrada, plataformas móveis, caixas e alavancas.
- Combate com registro de acertos por golpe e alvo, limites de vida e cura e proteção entre impactos.
- Estados de menu, partida, pausa, vitória e derrota.
- Volume e silêncio persistentes, desbloqueios salvos e retomada pelo início da última fase.
- Pausa automática ao perder o foco da janela.
- Ferramentas de preparação das cenas, backup e auditoria de referências.
- Ambientação das fases de ar e fogo com céu próprio, névoa, iluminação, ilhas distantes e detalhes de plataformas.

A retomada não salva posição, vida ou estado dos objetos. A existência desses recursos no código não substitui a validação da revisão atual no Play e na build.

## Personagem

O personagem original foi recuperado de `Nox.blend`, com malha de 1.994 vértices e esqueleto de 52 ossos. Os ciclos de repouso e caminhada foram reconstruídos na revisão v5, com repetição explícita e cadência ligada à velocidade do jogador. O modelo inclui materiais, duas machadinhas visuais e poses de salto e ataque.

A preparação v5 gera o prefab e o controlador de animação e vincula o Nox no lugar do cubo, preservando os componentes físicos e as referências das fases. A imagem e a procedência do asset estão em [Personagem Nox](docs/personagem-nox.md).

## Estado do projeto

No primeiro teste no Play do Unity, confirmei áudio, movimentação, salto, vida, pausa e navegação entre fases. Também encontrei aprisionamento e deformação do cubo nas plataformas móveis, que motivaram a revisão da física.

As revisões posteriores passaram pelas verificações registradas em [Qualidade](docs/qualidade.md). A substituição do cubo pelo personagem foi confirmada no Play após preparar as cenas. A revisão v5 de animação e ambientação ainda precisa de validação no Play e fora do editor; não há uma nova build Windows homologada disponível neste repositório.

## Estrutura

- `Assets/Scripts/`: controles, combate, física, interface, progresso e regras compartilhadas.
- `Assets/Art/`: shader do céu e recursos gerados pelo preparador.
- `Assets/Characters/Nox/`: modelo FBX com rig, materiais e animações.
- `Assets/Editor/`: preparação das cenas, integração do personagem, auditoria e geração da build.
- `Packages/` e `ProjectSettings/`: configuração do projeto original.
- `Tests/`: verificações de regras, preparação, contratos e integridade do FBX.
- `tools/`: recuperação, atualização e reprodução da exportação do personagem.
- `docs/`: decisões técnicas, histórico de revisão e evidências.

## Desenvolvimento e qualidade

A referência original utiliza Unity **2021.1.28f1**. O projeto mantém a configuração original, sem migração homologada para Unity 6. Os procedimentos de recuperação e atualização estão em [Desenvolvimento](docs/desenvolvimento.md).

Cenas, músicas e demais recursos do projeto completo permanecem no ZIP original. O repositório reúne o código, as configurações e o personagem exportado. O preparador combina esses arquivos em uma cópia local e preserva o original.

O CI verifica regras C#, recuperação e atualização, contratos dos componentes e integridade do FBX. Compilação isolada e testes automatizados não comprovam física, renderização, áudio ou jogabilidade no Unity.

## Documentação

- [Desenvolvimento e recuperação](docs/desenvolvimento.md)
- [Revisão geral](docs/revisao-geral.md)
- [Física e personagem](docs/fisica-e-personagem.md)
- [Animação e ambientação v5](docs/animacao-e-cenarios.md)
- [Personagem original e integração](docs/personagem-nox.md)
- [Resultados e limites da validação](docs/qualidade.md)

## Equipe e créditos

O GDD identifica João Vitor Vieira Cardoso, Lucas Alfredo Brandão e Mateus William Alves Torres Bento Magalhães como autores do projeto acadêmico. Os avisos e as licenças dos recursos de terceiros devem ser preservados; nenhuma licença geral foi aplicada a esses materiais.
