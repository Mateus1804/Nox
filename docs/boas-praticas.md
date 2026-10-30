# Boas práticas de manutenção

## Escopo e ferramentas

As convenções se aplicam ao código mantido neste repositório: scripts de execução, ferramentas do Editor, preparação em PowerShell, ferramentas Blender em Python e testes. Recursos de terceiros, cenas recuperadas, arquivos gerados e binários possuem tratamento próprio.

O jogo permanece em Unity 2021.1.28f1 e C# 8. O `global.json` seleciona a família de SDK 8.0 e aceita suas revisões de manutenção. O SDK .NET 8 serve somente aos testes e à análise do repositório; não substitui o Unity nem altera a versão do projeto. Ruff é uma dependência de desenvolvimento fixada em `requirements-dev.txt`, sem inclusão na build do jogo.

## Nomes, responsabilidades e compatibilidade

- Usar nomes que indiquem intenção. Preferir `protectionSeconds` a abreviações sem contexto.
- Manter funções com uma responsabilidade clara e guardas curtas para condições inválidas. Separar regras testáveis dos componentes que consultam input, física e interface.
- Extrair uma rotina quando houver duplicação real e contratos compatíveis. Evitar uma classe utilitária geral ou uma hierarquia criada apenas para reduzir linhas.
- Conservar nome e GUID dos componentes, nomes dos campos serializados, assinaturas de callbacks, caminhos dos recursos gerados e chaves de preferências. Métodos sem chamadas em C# podem estar em UnityEvents, animações ou cenas externas.
- Remover código somente após conferir referências no repositório, acesso, serialização e uso pelo motor. Registrar quando essa conclusão depender das cenas completas.
- Manter os adaptadores antigos restritos à leitura de cenas recuperadas. A lógica do jogador pertence a `NoxPlayerController` e a dos chefes a `NoxBossController`; a preparação v8 converte os componentes com cópia dos dados e redirecionamento das referências.
- Preservar parâmetros antigos de callbacks enquanto a migração de todos os consumidores não estiver comprovada. Documentar os parâmetros mantidos por compatibilidade.

## Formatação e comentários

C# segue `.editorconfig`: quatro espaços, chaves em linha própria, espaçamento consistente e linhas LF. A verificação é feita por `dotnet format whitespace --folder --verify-no-changes`. Os tipos de código verificados possuem checkout LF definido em `.gitattributes`, inclusive no Windows. Python usa `ruff check` e `ruff format --check`, com a configuração em `pyproject.toml`. PowerShell mantém UTF-8 com BOM para leitura correta também pelo Windows PowerShell 5.1.

Comentários devem explicar restrições, decisões e compatibilidade. Evitar regiões que apenas repetem o nome de uma função, comentários contraditórios e grandes blocos de código antigo comentado. O histórico pertence ao Git e à documentação de revisão.

Nomes antigos preservados por compatibilidade são exceções explícitas à nomenclatura atual, não motivo para renomear indiscriminadamente cenas e campos.

## Ciclo de vida e desempenho no Unity

- Usar `Update` para entrada e decisões, `FixedUpdate` para o motor físico e `LateUpdate` para acompanhamento visual.
- Associar referências estáveis no Inspector, `Awake` ou `Start`; evitar buscas repetidas por objetos ou componentes a cada quadro.
- Encerrar inscrições de eventos e restaurar recursos temporários no ciclo correspondente. Distinguir desativação de destruição.
- Conservar os colliders e o Rigidbody fora da hierarquia visual do personagem. A animação não deve transportar o corpo físico.
- Respeitar `NoxSession.CanPlay` e distinguir tempo da partida de tempo não escalado. Pausa não deve consumir janelas de combate.
- Validar números finitos nas fronteiras de regras. Não permitir que `NaN` ou infinito se transformem em uma ação válida.
- Centralizar constantes com significado de negócio e unidade reconhecível. Não converter valores arbitrários de cena em uma configuração global sem verificar o percurso.
- Atualizar interface e preferências quando houver mudança. Medir com o Profiler antes de declarar ganhos de desempenho; reduzir buscas e atribuições repetidas não comprova aumento de FPS.

## Preparação, arquivos e falhas

O atualizador monta e valida o plano antes de copiar arquivos. Uma pasta com nome de arquivo, ou um arquivo no caminho de uma pasta, deve interromper a atualização antes das sobrescritas. O backup permanece obrigatório. Em exceções tratadas durante as cópias, restaurar os arquivos anteriores, remover os novos da tentativa e conferir o conteúdo por hash. Se a recuperação ficar incompleta, informar arquivos pendentes e preservar o backup. Queda de energia e encerramento forçado não equivalem a uma exceção tratada; não há garantia de transação atômica entre todos os arquivos. O fluxo e as regressões estão em [Revisão do atualizador](revisao-atualizador.md).

A preparação das cenas usa backup e marcador v8; a migração é descrita em [Estrutura de scripts](estrutura-scripts.md). Alterações em scripts não exigem uma nova versão do marcador por si só. Mudanças na geração de geometria, prefab ou controller exigem planejar como projetos já preparados receberão a atualização.

As classes em `Assets/Editor` são excluídas da build pelo tratamento dessa pasta no Unity. Wrappers `#if UNITY_EDITOR` envolvendo o arquivo inteiro são redundantes nesse diretório. Diretivas continuam necessárias quando um arquivo de execução contém um trecho exclusivo de Editor ou desenvolvimento.

## Critérios de revisão

1. Explicar o problema e o comportamento esperado, com evidência reproduzível quando houver bug.
2. Separar correções de comportamento, organização e documentação em commits compreensíveis.
3. Conferir contratos de serialização e callbacks antes de excluir ou renomear elementos.
4. Executar formatação, análise sintática, lint, regressões e compilação aplicáveis.
5. Acrescentar regressões que falhem antes da correção; não criar testes que apenas procuram uma linha específica da implementação.
6. Conferir as interações afetadas no Unity. Para mudanças de física ou cenas, percorrer o trecho real e testar uma build quando disponível.
7. Registrar o que passou, falhou ou não foi executado. Compilação isolada, parser e testes de regras não comprovam a execução do jogo.

Os comandos estão em [Desenvolvimento](desenvolvimento.md), e os resultados desta revisão em [Revisão de código e limpeza](revisao-clean-code.md).
