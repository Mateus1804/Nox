# Estrutura de scripts e migração v8

## Organização

A revisão substitui as implementações paralelas por controladores comuns e dá nomes descritivos aos componentes. O total de arquivos C# de execução passou de **57 para 52**. As ferramentas de Editor passaram de 12 para 13 por causa da migração de componentes. O total sob `Assets` passou de 69 para 65 arquivos C#.

| Pasta de execução | Conteúdo |
| --- | --- |
| `Player` | `NoxPlayerController`, motor físico, visual e interação com caixas. |
| `Combat` | Ataques, áreas de dano, efeitos, projéteis e explosões. |
| `Enemies` | Controlador de chefe, vida, percepção, patrulha e ataques inimigos. |
| `World` | Plataformas móveis, interruptores de pressão, cristais, portais e elementos de apresentação. |
| `UI` | Interface da partida, seleção e conclusão de fase, tutoriais e opções. |
| `Core` | Sessão, inicialização, progresso e preferências. |
| `Domain` | Regras de combate, movimento, progresso e validação sem dependência do Unity. |
| `Compatibility` | Seis adaptadores mínimos para carregar os componentes do projeto recuperado. |

As ferramentas de Editor estão divididas em `Build`, `Character`, `Scenes` e `Validation`.

## Consolidações e exclusões

- `PlayerScript` e `PlayerTwo` passaram a utilizar uma única implementação: `NoxPlayerController`. Entrada, salto, vida, dano, cura e contatos não possuem duas cópias.
- `Boss` e `BossScript` utilizam `NoxBossController`. O perfil preserva o uso de vida inteira e a recompensa de fogo da variante original.
- O módulo `NoxPlayerControllerSupport` foi eliminado; suas operações foram incorporadas ao controlador que as utiliza.
- As pequenas regras de combate foram reunidas em `NoxCombatRules.cs`; movimento em `NoxMovementRules.cs`; progresso e bloqueio de carregamento em `NoxProgressRules.cs`. Os tipos públicos das regras continuam disponíveis, mas não exigem um arquivo e meta separados por classe sem componente.
- A rotação do portal possui uma única implementação configurável; a direção interna é migrada como velocidade negativa.
- O marcador `plataforma`, sem comportamento, é removido das cenas preparadas.

Os nomes antigos de jogador e chefe não identificam uma segunda implementação. São adaptadores para a leitura dos GUIDs do ZIP original. Retirá-los antes da conversão das cenas e prefabs originais causaria componentes ausentes. A preparação v8 substitui esses componentes nas cenas habilitadas; prefabs e cenas fora da build não são regravados automaticamente.

## Nomes de componentes

| Nome anterior | Nome atual |
| --- | --- |
| `AimEnemy` | `NoxRangedAttack` |
| `BulletEnemy` | `NoxEnemyProjectile` |
| `Bullet` | `NoxPlayerProjectile` |
| `EnemyAir` | `NoxEnemyHealth` |
| `EnemyPatrol` | `NoxEnemyNavigation` |
| `EnemiesWaypoint` | `NoxWaypointPatrol` |
| `CameraFollow` | `NoxFollowCamera` |
| `HUDAndSounds` | `NoxGameInterface` |
| `LoadLevels` | `NoxLevelSelection` |
| `PlaceHolderPanel` | `NoxLevelResultScreen` |
| `MovableObjectsScript` | `NoxCrateInteraction` |
| `Lever` | `NoxPressureSwitch` |
| `Cristais` | `NoxCrystalPickup` |
| `WaypointsAirPhase` | `NoxMovingPlatform` |
| `portalR` | `NoxPortalRotation` |
| `TutorialList` / `TutorialMethod` | `NoxTutorialSequence` / `NoxTutorialTrigger` |
| `ChangeTextColor` | `NoxButtonHighlight` |
| `Billboard` / `RotateSky` | `NoxBillboard` / `NoxSkyRotation` |
| `SlashScript` / `ThirdSlash` | `NoxMeleeEffect` / `NoxTravelingEffect` |

O mapa completo dos **69 caminhos anteriores**, GUIDs e destinos está em [script-migration.json](../Assets/Editor/Build/script-migration.json). Os GUIDs dos componentes renomeados foram mantidos e os tipos usam `MovedFrom`. Os campos renomeados usam `FormerlySerializedAs`. Callbacks antigos dos UnityEvents continuam como delegações para operações descritivas, como `QueueAttack`, `RefreshHealthDisplay`, `ShowLevelSelection` e `RestartLevel`.

## Atualização de uma cópia recuperada

Com o Unity fechado, o atualizador copia a estrutura nova, confere os arquivos e retira os caminhos antigos conhecidos. A retirada é registrada no mesmo plano de recuperação das cópias. Uma falha durante a exclusão restaura os arquivos anteriores e remove arquivos novos da tentativa.

Um caminho antigo com GUID diferente interrompe o plano antes das escritas. Arquivo antigo sem meta só é reconhecido quando seu hash corresponde a uma referência conhecida. Um meta órfão com GUID conhecido pode ser retirado; um GUID diferente não é apagado. Pastas existentes conservam seus próprios GUIDs de pasta.

A recuperação inicial utiliza esse mesmo fluxo, em vez de sobrepor os arquivos novos e deixar os antigos juntos. O backup conserva os scripts originais e os seus metas antes da migração. O ZIP original e os dados das cenas são preservados nessa etapa de arquivos.

## Preparação no Unity

A preparação usa o marcador `ProjectSettings/NoxScenesPrepared-v8.txt` e backup em `Backups/Preparacao-v8`. Ela substitui somente os tipos legados conhecidos de jogador, chefe e portal, copia os campos gerenciados e redireciona as referências dos componentes e dos UnityEvents na cena. Classes personalizadas derivadas não são convertidas indiscriminadamente. Referências que ainda exigem um tipo antigo interrompem a transação.

Os recursos visuais e o FBX v7 continuam sendo utilizados. Uma cena já ambientada conserva sua arte na preparação normal. O comando explícito de reaplicação pode regenerar a arte, com backup, conforme o comportamento de manutenção existente.

O menu **Nox → Verificar migração de scripts** confere o catálogo de GUIDs e possui cenários com prefabs temporários usando os nomes serializados antigos, referências de jogador, perfis de chefe e direção do portal. Foi compilado, mas ainda não executado neste ambiente.

## Validação e limites

A validação local inclui 170 verificações C#, 35 testes Python, sintaxe e formatação, além de compilação isolada dos **52 scripts de execução e 13 de Editor** contra Unity 2021.1.28f1, com avisos como erros. O parser abrange 67 arquivos C# nos três perfis. A integridade do FBX e suas curvas continuam verificadas; o modelo não foi reexportado.

Os testes de arquivos cobrem recuperação inicial sem caminhos duplicados, migração de layout antigo, preservação de GUID, rejeição de caminho ocupado por outro GUID, retirada de meta órfão conhecido e recuperação após falha ao excluir um caminho antigo. Os testes de contratos passaram a resolver herança e propriedades de compatibilidade e a conferir os nomes de migração dos campos.

Ainda faltam a importação e a execução dos verificadores no próprio Unity, inspeção do Inspector, callbacks em cenas reais, Play completo e build Windows. Compilação, atributos e testes de arquivos não comprovam o sucesso da desserialização de todas as cenas ou de recursos personalizados. O PR permanece revisável, com backup e sem publicação automática.
