# Revisão geral, refatoração e funcionalidades

## Escopo e referência

Revisão de 26/10/2026 sobre `1f3b002f187be7b5e96d4ba2997e9c100da2a23c`, continuidade da recuperação e dos ajustes de plataformas. Foram lidos os 35 scripts de execução anteriores, os três scripts de Editor, as configurações, o preparador/atualizador e as suítes existentes. A conferência da cena original do menu orientou o ponto de inserção do botão de retomada.

O objetivo é corrigir lógica e reduzir duplicação preservando as cenas existentes. Classes, GUIDs, campos públicos e nomes de métodos usados pela revisão anterior são mantidos. Nomes como `acessLevelFire`, `plataforma` e `portaldentro` não foram renomeados sem migração só para adequar estilo. A implementação não altera versões, pacotes ou pipeline gráfico.

A leitura integral não equivale a provar ausência de bugs. A versão permanece candidata até a execução do percurso real, do combate e das ações de Editor no Unity.

## Funcionalidades acrescentadas

- **Progresso local:** desbloqueios de fogo, água e terra persistem entre execuções. Água e terra continuam sem fases jogáveis nesta build; salvar um desbloqueio não inventa conteúdo.
- **Continuar fase:** registra a última fase efetivamente carregada e acrescenta um botão na seleção de fases. A retomada começa no início da fase; não salva posição, vida, caixas, chefes ou inventário. Menu/índices inexistentes são rejeitados; fogo exige desbloqueio.
- **Áudio persistente:** volume e silêncio usam chaves próprias de PlayerPrefs. O silêncio é global ao jogo e não depende de enumerar cada AudioSource. O mix individual das fontes é preservado. Preferências são gravadas no fechamento ou suspensão; um encerramento abrupto pode perder alterações ainda não gravadas.
- **Pausa por perda de foco:** sair da janela ou suspender o aplicativo pausa uma partida ativa. Voltar à janela não retoma sem confirmação pelo comando Continuar.
- **Auditoria de referências:** `Nox → Auditar cenas e referências` gera `Logs/Nox-Auditoria.txt` com scripts ausentes, referências quebradas detectáveis, jogador sem componentes, ataque incompleto, máscara de chão vazia e visual Nox não vinculado. A auditoria não preenche referências por suposição.
- **Preparação com backup:** a revisão v3 copia as cenas habilitadas para `Backups/Preparacao-v3/<identificador>/` antes de alterá-las. Falha antes do commit tenta restaurar os bytes e o conjunto de cenas aberto. A vinculação visual usa o mesmo mecanismo em `Backups/Visual-Nox/`. Os backups são locais e contêm cenas do projeto original.

Progresso e configurações ficam em PlayerPrefs do aplicativo. Não há sincronização em nuvem ou salvamento de estado completo. Essas informações não são tratadas como credenciais ou armazenamento seguro.

## Achados e tratamento

| ID | Evidência no código anterior | Classificação e consequência | Tratamento |
| --- | --- | --- | --- |
| G01 | `Boss.Update` e `BossScript.Update` ativavam ataque sem parar o NavMeshAgent | Bug de estado: caminho anterior podia continuar durante o ataque | `NoxChaseController` para o agente ao atacar e fora do alcance |
| G02 | Mesmas rotinas repetiam SetDestination em todos os quadros | Organização/desempenho: recálculo desnecessário | Intervalo de 0,2 s e retomada explícita do agente |
| G03 | `Persegue` acessava agente e jogador sem validar | Risco com referência ausente ou agente fora da malha | Guarda compartilhada de agente habilitado/posicionado |
| G04 | Chefes executavam LookAt com diferença de altura | Risco de inclinação do corpo e hitbox | Orientação somente no plano horizontal |
| G05 | Cada OnTriggerEnter de Slash causava dano no alvo | Risco com golpes ou alvos de múltiplos colliders | Registro por efeito e identidade do alvo; outro golpe possui registro independente |
| G06 | Ataque aceitava Mouse0 sobre elementos UI | Risco de clique atravessar a interface | Consulta ao EventSystem antes de instanciar o golpe |
| G07 | `EnemyAir` redefinia Hit em contatos não relacionados e não ativava feedback ao receber dano | Bug de feedback | Ativa Hit por intervalo curto quando o parâmetro existe; morte desliga colliders filhos |
| G08 | `EnemiesWaypoint` verificava o ponto atual, incrementava e usava o próximo sem nova validação | Bug condicionado a lista parcialmente configurada | Revalidação após avanço e uso de distância já calculada |
| G09 | `EnemyPatrol.SortPosition` usava todas as áreas NavMesh e InvokeRepeating | Risco/configuração: destino incompatível e agendamento separado do estado | Amostragem na área do agente; temporizadores no fluxo ativo |
| G10 | `AimEnemy` usava `startTime = 1`, independente do instante real | Bug de temporização | Primeiro disparo preparado com Time.time e reinicialização no OnEnable |
| G11 | Silêncio atingia lista fixa de sons e uma fonte do chefe | Bug de cobertura: outros sons não participavam | AudioListener.volume controla o conjunto sem alterar o mix das fontes |
| G12 | Slider/sombras/fontes eram reatribuídos a cada quadro | Organização/desempenho | Reaplicação somente quando valor ou cena muda |
| G13 | Referência de luz do HUD persistia após a troca de cena | Risco de configuração apontar para objeto destruído | Busca de luz direcional na cena carregada |
| G14 | HUD e painel de resultado coordenavam música e estado separadamente | Organização/risco de efeitos duplicados | Sessão controla transição; HUD controla música; painel controla resultado |
| G15 | Métodos de painéis e sons acessavam campos sem guarda | Risco de NullReference em cenas incompletas | Rotinas pequenas para painéis e fontes, preservando pontos públicos |
| G16 | Coleta de cristais não tinha trava e ignorava estado da partida | Risco de repetição por colliders e coleta fora da partida | Coleta única, colliders desabilitados e guarda de sessão |
| G17 | Som de cristal pertencente ao próprio objeto podia ser destruído junto | Risco dependente da associação da fonte | Som destacado somente nessa configuração; fonte externa usa PlayOneShot |
| G18 | `LoadLevels.Update` repetia SetActive e DontDestroyOnLoad | Organização/desempenho | Persistência definida no ciclo de vida e atualização de botões por mudança |
| G19 | Chamadas concorrentes podiam solicitar a mesma troca de cena | Risco em triggers/cliques sobrepostos | `NoxSceneLoadGate`; falha libera a trava e restaura o estado anterior |
| G20 | Cancelar a preparação retornava ao método que ainda iniciava a build | Bug no fluxo de cancelamento | Preparação retorna sucesso/cancelamento e build respeita o resultado |
| G21 | Preparação salvava cenas anteriores antes de uma falha em cena posterior | Risco de atualização parcial | Preflight e transação de arquivos com cópia anterior |
| G22 | Vinculação visual removia o visual anterior antes de validar o novo | Risco de perda da apresentação em caso de falha | Validação antes da remoção e backup das cenas |
| G23 | Tutorial fechava e concluía ao sair um único collider; próxima etapa podia ser perdida por sobreposição | Risco com múltiplos colliders e gatilhos sobrepostos | Conjunto de visitantes, OnTriggerStay e conclusão somente do painel mostrado |
| G24 | Câmera sobrescrevia alvo definido; billboard atualizava antes da câmera | Organização/risco de alvo incorreto e atraso visual | Referência explícita preservada e billboard em LateUpdate |
| G25 | Alavanca desabilitada conservava flags e cor da ocupação anterior | Bug de ciclo de vida | Estado visual e flags reiniciados ao desabilitar |
| G26 | Explosões tinham cópias da mesma lógica | Organização | `NoxExplosion` aplica força uma vez por corpo e rejeita raio/força inválidos |
| G27 | Métodos vazios, comentários que repetiam código e log informal de combate | Organização/manutenção | Remoção e comentários focados em decisões/compatibilidade |

Alguns achados são falhas demonstráveis de ramificação; outros dependem da configuração da cena. A classificação acima não afirma que todos tenham ocorrido no teste manual já realizado.

## Organização adotada

- `NoxRules`, `NoxDamageWindow`, `NoxHitRegistry`, `NoxSceneLoadGate` e `NoxProgressRules` contêm decisões testáveis sem Unity.
- `NoxPlayerMotor`, `NoxChaseController`, `NoxExplosion` e `NoxAttackHitbox` compartilham mecanismos usados por mais de um controlador, sem um framework de arquitetura novo.
- `NoxProgress` concentra acesso aos dados locais de progresso; HUD mantém UI e preferências de áudio. A separação não exige novos componentes nas fases para começar a funcionar.
- `NoxSceneTransaction` é usado pelas duas ferramentas que regravam cenas. `NoxProjectAudit` é somente uma ferramenta de inspeção e relatório.
- `PlayerTwo` permanece compatível e experimental; não foi removido por ausência de referência nas cenas consultadas.

Os métodos de explosão e ataque originais continuam disponíveis. O movimento de projéteis não foi substituído por um sistema novo de projéteis ou pooling sem evidência de necessidade. A física de plataformas da revisão anterior permanece intacta nesta ampliação.

## Validação e limites

A execução local aprovou **87 verificações C#**, **oito testes de preparação/atualização** e **quatro testes de contratos**. Os 43 scripts de execução e cinco de Editor compilaram contra as bibliotecas originais, com avisos tratados como erros. Esses números não são uma contagem de testes de gameplay.

Os testes de contratos usam o histórico Git da revisão anterior e conferem GUIDs, nomes de campos/métodos públicos, unicidade de metadados e a separação entre runtime e Editor. A leitura de declarações é intencionalmente simples: não equivale a uma análise semântica completa de compatibilidade binária ou dos UnityEvents serializados.

O resultado do CI Windows está registrado em `qualidade.md` após a execução efetiva. O novo botão, foco da janela, PlayerPrefs no Unity, navegação NavMesh, áudio real, animações, auditoria e reversão das cenas ainda precisam ser executados no editor. A compilação isolada confirma tipos/assinaturas, não esses comportamentos.

O modelo original Nox continua pendente do asset correto. Nenhum personagem de um pacote externo foi apresentado como o personagem original.

## Aceite no computador de desenvolvimento

1. Com o Unity fechado, aplicar `Atualizar-Nox.cmd` na cópia recuperada.
2. Reabrir e executar **Nox → Preparar cenas recuperadas**; conferir o caminho do backup informado.
3. Executar a auditoria e examinar `Logs/Nox-Auditoria.txt`. Achados são pendências, não mensagens de aprovação automática.
4. Testar a verificação física e percorrer plataformas/caixas como na revisão anterior.
5. Atacar inimigos/chefes com os três golpes, verificar parada para ataque e ausência de dano repetido do mesmo efeito.
6. Alterar volume/silêncio, encerrar e abrir de novo. Conferir sons de cristais, chefe e menus.
7. Concluir fases, encerrar e reabrir, conferir desbloqueios e **Continuar fase**. A retomada deve começar na entrada da fase salva.
8. Alternar para outra janela durante a partida; confirmar pausa e retorno somente pelo comando Continuar.
9. Conferir menu em diferentes resoluções, inclusive o novo botão. A posição foi escolhida pela estrutura original, sem inspeção visual nesta execução.
10. Gerar a build e repetir os fluxos fora do editor antes de distribuir.
