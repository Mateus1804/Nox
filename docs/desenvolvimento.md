# Desenvolvimento e recuperação

## Referência original

O ZIP `Nox_Project.zip` contém a pasta `Nox_Project`, com Unity 2021.1.28f1. O arquivo `NOX.unitypackage.gz` é outro pacote disponível, não uma substituição comprovada do projeto completo. Há também `Build.zip`, separado do código-fonte. O arquivo original deve permanecer como cópia de segurança.

O inventário do ZIP identificou 31.051 entradas, incluindo diretórios. `Library` ocupa aproximadamente 2,43 GB descompactados e `Assets` aproximadamente 1,26 GB. Excluir caches do controle de versão reduz o volume, mas não elimina o peso dos recursos originais.

## Abertura da referência local

1. Baixar e extrair `Nox_Project.zip` em uma pasta de trabalho, preservando o ZIP.
2. Instalar pelo Unity Hub o editor 2021.1.28f1 e adicionar a pasta que contém `Assets`, `Packages` e `ProjectSettings`.
3. Abrir uma cópia de trabalho. Se o Hub sugerir atualização, conservar primeiro a referência na versão original.
4. Aguardar a importação e registrar os erros do Console antes de corrigir código.
5. Conferir as cenas em Build Settings e iniciar pelo menu. As cinco cenas habilitadas na configuração original são `Menu1.2`, `AirPhase1.4`, `AirPhase2.4`, `AirPhase3.5` e `FirePhase1.4`.

Esta sequência exige o arquivo original: as cenas não estão nesta importação parcial.

## Envio pelo computador

O repositório pode ser clonado com GitHub Desktop pela URL `https://github.com/Mateus1804/Nox.git`. A branch `chore/importacao-inicial-unity` contém a recuperação parcial para revisão. A raiz de trabalho deve conter diretamente `Assets`, `Packages` e `ProjectSettings`, sem criar uma pasta `Nox_Project` adicional dentro dela.

Antes de copiar novos recursos, a revisão das licenças deve identificar quais podem constar em repositório público e quais precisam ser obtidos separadamente pelo desenvolvedor. Recursos restritos podem ser substituídos ou documentados como dependências externas. Mudar a visibilidade de um repositório também não substitui as condições das licenças.

A transferência deve conservar cenas, prefabs e `.meta` correspondentes. `Library`, `Temp`, `Logs`, `obj` e saídas de compilação ficam fora do Git pelo `.gitignore`. O GitHub Desktop permite conferir a lista de alterações antes do commit e do envio. A opção de adicionar todos os arquivos sem revisão não faz parte deste procedimento.

O inventário não encontrou arquivos individuais acima de 100 MiB dentro de `Assets`, `Packages` ou `ProjectSettings`. Isso não garante que o tamanho total seja adequado: os assets somam cerca de 1,26 GB, com modelos, áudios e texturas binários. Git LFS não foi ativado, e nenhum consumo adicional desse serviço foi configurado. A decisão depende dos recursos que permanecerem após a revisão.

## Compatibilidade e manutenção

Os bytes dos 79 arquivos originais incluídos constam em `importacao.json`. Treze scripts contêm bytes incompatíveis com UTF-8; a importação os preservou sem conversão. A normalização de codificação deve ser uma alteração separada, com conferência dos comentários e do comportamento no editor.

Os nomes de classes, arquivos, campos públicos e métodos devem ser preservados até conferir as referências serializadas em cenas, prefabs e eventos da interface. `PlayerScript` e `PlayerTwo` têm responsabilidades semelhantes; isso é um candidato de revisão, não prova de que um deles possa ser removido.

A presença de `com.unity.test-framework` no manifesto não significa que já existam testes ou que estes tenham passado. Build, Play Mode e testes Unity dependem do editor e dos recursos completos.
