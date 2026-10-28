> Histórico da recuperação inicial. O estado atual do personagem e da validação está registrado em [Personagem Nox](personagem-nox.md) e [Qualidade](qualidade.md).

# Correções e validação da versão candidata

## Referência e escopo

A referência é a importação `7da36c5a97651df26c9fd8197ab9e5203efd7082`. O arquivo original e a primeira importação foram preservados. As mudanças mantêm os nomes dos componentes, os GUIDs existentes, os campos públicos serializados e os pontos de entrada de UI necessários durante a recuperação.

Os scripts alterados foram normalizados para UTF-8. Os arquivos de metadata originais não foram recriados. `docs/importacao.json` continua descrevendo o original, não os novos bytes dos scripts corrigidos.

## Problemas tratados

| Grupo | Implementação |
| --- | --- |
| Singleton do HUD | Atribuição da instância aceita, retorno para duplicatas e limpeza no descarte |
| Vida | Regra pura NoxRules, preenchimento proporcional, limites e atualização da cor |
| Morte | Entrada única; encerramento do comportamento do chefe; desbloqueio por atribuição idempotente |
| Estado | NoxSession distingue menu, partida, pausa, vitória e derrota; comandos de gameplay exigem partida ativa |
| Navegação | Índices validados e retorno ao menu após a última cena; atalho L restrito a editor/development |
| Física | Movimento por velocidade de Rigidbody, preservação de Y, entrada combinada com velocidade limitada, consulta de chão junto da física |
| Salto | Pedido por GetKeyDown consumido em FixedUpdate, altura configurável e bloqueio ao empurrar |
| Caixas | Candidatos por collider, saída limpa o alcance, referência segurada independente, junta física, sem parentesco com o jogador e com restauração do estado cinemático |
| Identidade e dano | A tag permanece Player; controlador principal trata BulletTurret e Fogo |
| Tutorial | Condições corrigidas; índice de cada Collider1…5 definido na preparação de cenas |
| Alavanca | HashSet de ocupantes, validação de dois extremos e material sob responsabilidade do componente |
| Áudio | Preferência de silêncio autoritativa; atualização não desfaz o silêncio dos efeitos |
| Efeitos e projéteis | Descarte agendado uma vez, consumo no impacto, forças deduplicadas por Rigidbody |
| Câmera e patrulha | Acompanhamento em LateUpdate, validação de alvo, patrulha chama sua navegação e amostra NavMesh |
| UI serializada | Preparação Editor reassocia reinício/retorno sem alvo e remove callbacks ausentes; água/terra/dificuldade ficam desativadas |
| Recuperação | ZIP conferido antes de extrair, caminhos e links validados, pasta nova e original preservado |

## Alterações intencionais de comportamento

- O deslocamento aceita diagonais sem velocidade adicional. O Rigidbody passa a coordenar o movimento, em vez de disputar posição com Transform.
- O salto passa a ter altura explícita, inicialmente 3,5 unidades. `jumpForce` é mantido como campo legado para compatibilidade de serialização, mas a nova rotina usa `jumpHeight`. A altura deve ser calibrada nas fases antes de aprovar jogabilidade; não foi afirmada equivalência com a força antiga aplicada por quadro.
- Cura não ultrapassa a vida máxima, inicialmente 3; derrota/vitória interrompem a partida por estado explícito.
- Pegar uma caixa usa FixedJoint com corpo dinâmico, sem torná-la filha do jogador. A configuração cinemática anterior é restaurada ao soltar. A revisão física e os limites estão em `fisica-e-personagem.md`.
- Controles de água, terra e dificuldade não ganham implementações fictícias: ficam indisponíveis porque essas funcionalidades não estão completas na referência.
- Ao iniciar diretamente uma fase sem o GameADM do menu, o bootstrap retorna ao menu para montar o contexto previsto.
- A patrulha experimental passa a executar a rotina que antes não era chamada. Seu uso em fases precisa ser confirmado antes da aprovação.

## Limitações e itens não concluídos

Esta versão não foi declarada pronta para distribuição. Falta executar o editor completo, conferir a integração do código de Editor, aplicar a preparação nas cenas reais, gerar a build e testar o fluxo completo no Windows.

O prefab legado Enemy contém GUIDs de scripts não resolvidos pelo catálogo dos 28 componentes. Não foi removido ou reassociado por suposição. A conferência adicional do catálogo completo de Assets não encontrou esses dois GUIDs, nem DLLs em Assets ou referências serializadas ao prefab legado. A apresentação como Missing Script e o descarte definitivo ainda exigem a conferência no Inspector. Sons de cristais, licenças, referências de animação e gráficos de Visual Scripting também precisam da conferência completa; a emissão compartilhada de áudio não foi alterada sem evidência de falha.

O PlayerTwo preserva campos e diferenças originais, mas compartilha o motor físico e a regra de proteção de dano com PlayerScript. A consolidação estrutural poderá ocorrer depois da referência jogável. Não houve migração geral para Unity 6, conversão de pipeline gráfico, calibração de NavMesh ou teste de desempenho.

O preparador Editor tem marca para evitar repetir a preparação em uma cópia já preparada. Alterações posteriores em cenas exigem nova avaliação; não se deve apagar essa marca como substituição da revisão de alterações manuais.

## Revisão posterior ao primeiro Play

A revisão de 26/10/2026 responde à deformação/aprisionamento nas plataformas identificada no teste inicial. A hierarquia de `AirPhase1.4` confirma `Mobile Platforms` com escala `(6, 1, 8)`, herdada pelas plataformas individuais. O antigo `SetParent` submetia o jogador a essa escala ao girar. O transporte passa a compor velocidade física, sem modificar pai ou escala do jogador.

Dano agora possui proteção de 0,6 s e contato persistente com perigos causa dano novamente após esse intervalo. Cura de uma unidade é limitada a 3 e o item só é consumido quando houver recuperação. Isso é decisão explícita de comportamento, não uma alegação de cura excedente comprovada na versão anterior, que já possuía clamp.
