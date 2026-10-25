# Nox

Projeto de jogo desenvolvido durante o curso Técnico em Programação de Jogos Digitais do SENAI, com Unity e C#.

## Estado do repositório

A recuperação do projeto original está em andamento. Esta importação reúne **28 scripts**, seus arquivos `.meta`, a configuração de pacotes e as configurações do Unity. Os arquivos importados foram preservados sem alterações de lógica, nomes ou GUIDs.

**Este repositório ainda não contém uma cópia jogável completa.** Cenas, prefabs, imagens, modelos, materiais, áudio e demais recursos continuam no arquivo original e não acompanham esta importação. Abrir apenas esta cópia no editor não reproduz o jogo original.

O projeto original registra **Unity 2021.1.28f1**, revisão `f3f9dc10f3dd`, com TextMesh Pro 3.0.6 e Unity Test Framework 1.1.33. Não houve atualização do editor ou dos pacotes.

## Organização

- `Assets/Scripts/`: movimentação, combate, inimigos, câmera, interface, tutoriais, portais e progressão entre fases.
- `Packages/`: dependências e respectivas versões originais.
- `ProjectSettings/`: entradas, física, tags, cenas de build e demais configurações originais.
- `docs/importacao.json`: relação dos 79 arquivos recuperados, tamanhos e hashes SHA-256.
- `docs/desenvolvimento.md`: recuperação local e preparação do envio completo.
- `docs/qualidade.md`: conferências realizadas, limitações e roteiro de testes.

## Recuperação e próximos passos

O primeiro objetivo é reproduzir a versão original no editor correspondente e validar sua build. A recuperação dos recursos deve preservar os arquivos `.meta`, pois suas referências ligam scripts, cenas, materiais e prefabs.

O arquivo original também contém packs de terceiros, exemplos de bibliotecas e músicas. Sua presença em um projeto acadêmico não foi tratada como comprovação de autorização para redistribuir os arquivos-fonte. As licenças desses recursos e os créditos da equipe precisam ser conferidos antes de ampliar a importação pública.

Depois de estabelecer uma versão reproduzível, o planejamento inclui documentar controles e cenas, conferir o fluxo completo do jogo, introduzir testes para regras isoláveis e avaliar uma demonstração para Windows ou navegador. Nenhuma dessas validações foi declarada concluída nesta etapa.

## Autoria e recursos externos

O desenvolvimento ocorreu em contexto acadêmico. Os créditos individuais e as contribuições da equipe permanecem em conferência com a documentação original. Não foi aplicada uma licença geral que conceda direitos sobre código ou recursos de terceiros.
