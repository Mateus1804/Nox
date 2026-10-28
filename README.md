# Nox

Jogo acadêmico desenvolvido com Unity e C# durante o curso Técnico em Programação de Jogos Digitais do SENAI. A recuperação atual preserva as referências e os recursos originais, corrigindo problemas de estado, vida, interação e interface.

## Estado atual

A recuperação já foi aberta no Play do Unity pelo responsável pelo projeto: áudio, movimentação, salto, vida, pausa e navegação entre fases funcionaram na conferência relatada. Foram observados aprisionamento e deformação nas plataformas móveis; ataque e cura ainda precisam de conferência dirigida.

A revisão atual remove o parentesco entre jogador e plataforma, compartilha o motor físico entre os controladores e organiza dano, cura, caixas e ataque. As verificações de regras e a compilação isolada estão registradas em [Qualidade](docs/qualidade.md). A jogabilidade desta nova revisão ainda depende de validação no Unity.

Cenas e recursos visuais/sonoros continuam no `Nox_Project.zip` original. O repositório público não redistribui esses materiais. O preparador combina o arquivo local original com as correções em uma pasta nova, sem alterar o ZIP ou sobrescrever uma pasta existente.

## Preparação no Windows

O ponto de entrada é **`Preparar-Nox.cmd`**. Ele solicita o ZIP original e o executável do editor Unity instalado. A recuperação confere os 79 arquivos de referência, extrai somente Assets, Packages e ProjectSettings, aplica os scripts e solicita a preparação das cenas e a build Windows. Em caso de sucesso, o executável fica em `Builds/Windows/Nox.exe` dentro da nova pasta.

O processo depende de um editor Unity instalado e licenciado, com suporte a build Windows. O original registra **2021.1.28f1**; **2021.3 LTS** é a primeira alternativa de recuperação. A compatibilidade com Unity 6 ainda não foi validada. Uma atualização ocorre somente na cópia preparada.

Os procedimentos e os limites estão em [Desenvolvimento](docs/desenvolvimento.md). A preparação não significa que o jogo tenha sido testado ou que uma build já esteja disponível para download.

## Atualização de uma cópia já recuperada

`Atualizar-Nox.cmd` atualiza somente os scripts e o código de Editor de uma pasta recuperada, com o Unity fechado. Uma cópia dos scripts anteriores é criada ao lado do projeto. Cenas, modelos, áudio e configurações locais são preservados pela atualização. Após reabrir, **Nox → Preparar cenas recuperadas** aplica a revisão física às cenas.

O comando **Nox → Verificar física em cena isolada** permite conferir transporte e salto em um cenário temporário. O uso do modelo original do personagem depende de sua localização; nenhum asset de terceiros foi apresentado como Nox. O fluxo de vinculação e os limites constam em [Física e personagem](docs/fisica-e-personagem.md).

## Recursos da revisão geral

- Volume e silêncio persistentes, aplicados a todos os sons do jogo.
- Desbloqueios de fases salvos localmente e retomada da última fase pelo botão **Continuar fase**.
- Pausa automática ao perder o foco da janela, sem retomada forçada.
- Combate com registro por golpe/alvo e navegação dos chefes separada da decisão de ataque.
- Preparação de cenas com backup anterior e ferramenta de auditoria de referências no Unity.

A retomada começa na entrada da fase: posição, vida, caixas e estado dos chefes não são salvos. As fases de água e terra continuam fora da build. A [revisão geral](docs/revisao-geral.md) registra decisões, achados, testes e critérios de aceite.

## Correções

- Vida e barra derivadas da mesma regra, com limites de cura e dano.
- Morte dos chefes e conclusão executadas uma única vez.
- Estados separados para menu, partida, pausa, vitória e derrota.
- Navegação com limite de cenas e retorno definido após a última fase.
- Movimentação pelo Rigidbody e salto solicitado por comando, com altura configurável.
- Interação com a caixa mais próxima em alcance, separando objeto candidato e segurado, com junta física em vez de parentesco de Transform.
- Identidade do jogador preservada ao empurrar; dano de torre tratado pelo controlador principal.
- Tutoriais com índices fixos e validação das referências.
- Alavanca com conjunto de ocupantes, sem fechar ao sair apenas uma caixa.
- Silêncio de áudio respeitado nos quadros seguintes.
- Preparação de vínculos de botões antigos e desativação dos controles de fases/dificuldade não implementados.

As mudanças intencionais e os pontos ainda pendentes constam em [Correções e validação](docs/correcoes.md).

## Estrutura

- `Assets/Scripts/`: componentes originais e regras de sessão, vida e navegação.
- `Assets/Editor/NoxBuild.cs`: preparação das cenas e geração Windows no editor.
- `Packages/` e `ProjectSettings/`: referência de dependências e configuração original.
- `Tests/`: regressões C# e integração do preparador com arquivos fictícios.
- `tools/Preparar-Nox.ps1`: recuperação em pasta isolada e chamada do Unity.
- `docs/importacao.json`: hashes dos 79 arquivos originais anteriores às correções, usados para identificar o ZIP de referência.

## Qualidade e limites

Os resultados efetivamente executados estão em [Qualidade](docs/qualidade.md). O CI verifica regras e o preparo do projeto com um ZIP sintético, sem redistribuir assets ou executar Unity. Não comprova renderização, física, áudio ou funcionamento de uma build.

A autoria e as contribuições individuais permanecem em conferência com a documentação acadêmica. Licenças dos recursos externos devem ser preservadas. Nenhuma licença geral foi aplicada sobre materiais de terceiros.
