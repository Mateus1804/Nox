# Qualidade e situação da recuperação

## Conferências executadas em 25/10/2026

- Leitura da listagem do Drive e do diretório central de `Nox_Project.zip` por intervalos de bytes.
- Extração de 79 arquivos do escopo importado. O leitor ZIP conferiu o CRC das entradas extraídas; não houve download ou conferência integral dos 1,77 GB do arquivo.
- Identificação do editor 2021.1.28f1 no `ProjectVersion.txt`.
- Leitura de `Packages/manifest.json` e `Packages/packages-lock.json` como JSON válido.
- Conferência de 28 scripts com 28 metadados `.meta`, GUIDs válidos e sem duplicação entre esses metadados. O `.meta` do diretório Scripts também foi preservado.
- Registro de tamanho e SHA-256 dos 79 arquivos importados, sem alteração de seus bytes.
- Conferência pontual dos campos de credenciais em ProjectSettings: os campos de senha e assinatura consultados estavam vazios. Isso não constitui auditoria completa de segredos do projeto original.

## Verificações não executadas

Não foram executados Unity, compilação C#, Edit Mode, Play Mode, a build Windows ou uma exportação WebGL. Não há resultado de testes de jogabilidade, desempenho ou compatibilidade nesta importação. A análise dos metadados e dos hashes não substitui essas verificações.

As cenas e os recursos externos não acompanham a importação. A ausência deles é conhecida e impede validar o jogo a partir apenas deste repositório.

## Roteiro para uma referência jogável

| Cenário | Critério de aceite | Situação |
| --- | --- | --- |
| Importação no editor original | Console sem erros que impeçam compilar ou executar | Não executado |
| Menu e fases | Menu abre e cada cena habilitada carrega sem referência ausente | Não executado |
| Movimentação e salto | Controles respondem e colisões impedem atravessar o cenário | Não executado |
| Combate | Ataques, projéteis, dano e morte seguem as regras observadas na referência | Não executado |
| Interface e áudio | Indicadores, pausa, reinício e sons correspondem aos eventos | Não executado |
| Progressão | Concluir uma fase libera o acesso esperado sem perder o estado necessário | Não executado |
| Build Windows | Execução fora do editor, do menu ao encerramento de uma fase | Não executado |
| Demonstração web | Compatibilidade de recursos, controles e carregamento verificada antes de hospedar | Não executado |

A primeira rodada deve usar o jogo original como referência e registrar passos, resultado, versão do editor e evidências. Testes automatizados de regras isoladas podem ser introduzidos após essa referência, sem representar como aprovados cenários ainda não executados.
