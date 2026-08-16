# GW GUI Guia do Usuário

GW GUI é um aplicativo Windows para ler, escrever, converter, inspecionar e emular imagens de disquetes. Pode controlar Greaseweazle hardware, trabalhar com arquivos de imagem de disco através de seu motor interno, e executar configurações salvas emulado-máquina.

Este guia descreve a interface em inglês mostrada na versão atual da aplicação. Ele é escrito como a fonte do manual do usuário imprimível: screenshots ilustram os controles, enquanto o texto ao redor explica o que escolher, por que escolher, e como verificar o resultado.

> **Importante:** Ler um disco não é destrutivo. Escrever, apagar, atualizar firmware e algumas ferramentas de hardware podem modificar mídia ou hardware. Leia o aviso anexado ao procedimento relevante antes de clicar ** Executar**.

### Como utilizar este guia

Se esta for a sua primeira vez a utilizar GW GUI, completo [Começar](#getting-started), então siga [Lendo um disco](#reading-a-disk). Se o aplicativo já estiver configurado, vá diretamente para o capítulo para a operação que você deseja executar. Os capítulos de opções servem como referência quando um procedimento pede que você altere uma configuração de acionamento, motor, perfil ou máquina emulado.

Os nomes da interface são mostrados em **negrito**. Nomes de arquivos, caminhos, comandos e valores literais são mostrados como `code`. Notas explicam o comportamento normal; avisos identificam operações que podem alterar um disco, controlador ou configuração armazenada.

## Índice

1. [Compreendendo o fluxo de trabalho](#understanding-the-workflow)
2. [Começar](#getting-started)
3. [Janela principal](#main-window)
4. [Lendo um disco](#reading-a-disk)
5. [Escrever um disco](#writing-a-disk)
6. [Convertendo imagens de disco](#converting-disk-images)
7. [Visualizar uma imagem de disco](#visualizing-a-disk-image)
8. [Explorando o conteúdo do disco](#exploring-disk-contents)
9. [Usar as ferramentas](#using-the-tools)
10. [Emulação](#emulation)
11. [Opções de aplicação](#application-options)
12. [Opções de emulação](#emulation-options)
13. [Amiga configuração](#amiga-configuration)
14. [Diagnóstico e manutenção de hardware](#hardware-diagnostics-and-maintenance)
15. [Logs e histórico de operações](#logs-and-operation-history)
16. [Dados de aplicação e utilização portátil](#application-data-and-portable-use)
17. [Flowflows recomendados](#recommended-workflows)
18. [checklist de segurança](#safety-checklist)
19. [Resolução de problemas](#troubleshooting)
20. [Glossário](#glossary)
21. [Referência rápida](#quick-reference)

## Compreender o fluxo de trabalho

GW GUI separa as operações do disco físico das operações do ficheiro de imagem:

| Objetivo | Entrada | Saída | Página recomendada |
|---|---|---|---|
| Preservar uma disquete | Disco físico | Ficheiro de imagem | **Ler** |
| Recriar uma disquete | Ficheiro de imagem | Disco físico | **Escrever** |
| Mudar o formato da imagem | Ficheiro de imagem | Um ou mais arquivos de imagem | **Conversão** |
| Inspecionar faixas e anomalias | Ficheiro de imagem | Análise visual | **Visualização** |
| Procurar os ficheiros armazenados numa imagem | Sistema de imagem/ficheiro suportado | Ficheiros e pastas | **Disk Explorer** |
| Diagnose uma unidade ou controlador | Greaseweazle hardware | Medições ou estatuto | **Ferramentas** |
| Executar uma máquina virtual salva | Configuração da máquina salva | Sessão de emulação | **Emulação** |

Para preservação, primeiro fazer uma captura crua e mantê-lo inalterado como um mestre. Crie cópias de trabalho convertidas ou reparadas desse mestre. Isso evita repetir uma leitura física e preserva informações que um formato baseado no setor não pode reter.

## Começar

### Requisitos

- Janelas com Microsoft .NET Tempo de execução da área de trabalho exigido pela aplicação.
- A Greaseweazle controlador para operações físicas de disco flexível.
- Um caminho configurado para `gw.exe` quando utilizar a Greaseweazle Host Tools Motor.
- Obtidos legalmente ROM arquivos quando uma máquina emulado precisa deles.

O aplicativo verifica o tempo de execução necessário .NET na inicialização. Se estiver faltando, siga o prompt de instalação e reinicie GW GUI.

### Antes de conectar hardware

Verificar o seguinte antes de executar uma operação de disco físico:

1. Ligar a Greaseweazle controlador para um estável USB Porto.
2. Conecte o cabo flexível com a orientação correta.
3. Conecte a fonte de alimentação da unidade antes de inserir mídia valiosa.
4. Confirme que o tamanho da unidade e densidade correspondem ao disco.
5. Gravar-proteger o disco de origem quando possível.

GW GUI não pode evitar danos causados por cabeamento incorreto, energia inadequada ou uma movimentação mecanicamente insegura. Teste hardware desconhecido com um disco descartável primeiro.

### Primeiro lançamento

1. Abrir `gwgui.exe`.
2. Abrir **Opções**.
3. In **Controladores e unidades**, digitalizar para o controlador e configurar a unidade.
4. Verificar ou selecionar o caminho para `gw.exe`.
5. In **Motores**, escolha qual motor deve executar cada operação.
6. Volte para a janela principal e selecione a guia de operação necessária.

### Confirmar que a configuração está pronta

Uma configuração de trabalho deve mostrar o controlador e unidade na barra de status, por exemplo, um número de unidade, tamanho, densidade, e COM Porto. In **Opções > Controladores e unidades **, o controlador deve ser marcado ** Disponível ** e a unidade ** Configurado **Corre. ** Informação do controlador** antes de ler mídia valiosa se você quiser verificar a comunicação sem alterar um disco.

### Escolher um motor

GW GUI pode expor mais de uma implementação para algumas operações. A **Greaseweazle Host Tools** motor invoca o configurado `gw.exe`; o GW GUI O motor manipula operações suportadas dentro da aplicação. A seleção do motor é explícita e independente para leitura, escrita, conversão e Disk Explorer. Se uma operação não for suportada pelo motor seleccionado, GW GUI relata essa condição em vez de mudar os motores automaticamente.

## Janela principal

A janela principal agrupa as operações principais em sete páginas:

- **Ler** cria uma imagem a partir de um disco físico.
- **Escrever** escreve uma imagem para um disco físico.
- **Conversão** converte um formato de imagem de disco em um ou mais formatos de saída.
- **Visualização** Apresenta faixas e fluxos ou dados descodificados.
- **Disk Explorer** navega sistemas de arquivos suportados e conteúdo de disco.
- **Ferramentas** fornece comandos de manutenção e diagnóstico de hardware.
- **Emulação** gerencia e executa máquinas emuladas salvas.

O console na parte inferior exibe o comando sendo executado e sua saída. A barra de status relata a unidade, perfil e estado atual selecionados.

### Lendo a interface

A maioria das páginas de operação segue o mesmo padrão:

1. **Origem ou destino** controles identificar o disco, imagem ou pasta.
2. **Controlos de formato** selecionar detecção automática ou uma máquina e formato explícitos.
3. **Controlos de perfil** aplicar configurações reutilizáveis.
4. **Configuração avançada** expor parâmetros que são normalmente opcionais.
5. **Executar** Começa a operação.
6. A **consola** mostra o comando, progresso, avisos e erros gerados.

A **Executar** botão não implica que todos os valores são seguros para o disco inserido. Reveja sempre o destino e a unidade selecionada antes de uma operação de gravação ou manutenção.

### Barra de status e console

O lado esquerdo da barra de status identifica a unidade física ativa. O centro mostra o perfil ativo quando um é selecionado. O indicador de estado informa se a aplicação está pronta ou ocupada. O console não é meramente diagnóstico: é o registro autoritário do comando enviado para o motor selecionado. Use seu controle de cópia quando você precisar preservar ou compartilhar esse comando.

## Lendo um disco

Abrir o **Ler** aba para capturar um disquete físico como uma imagem.

<p align="center"><img src="images/main-read-en.png" alt="Ler a página" width="78%"></p>

### Procedimento de base

1. Insira o disco fonte na unidade configurada.
2. Escolha o tipo de imagem:
   - **Imagem em bruto (SCP)** preserva informações de nível de fluxo.
   - **Formato de disco conhecido** cria uma imagem usando uma máquina e formato selecionados.
3. Escolha a pasta de destino.
4. Indique o nome do ficheiro de saída.
5. Selecione um perfil, se necessário.
6. Clique **Executar**.

O console mostra o comando e o progresso exatos. Não remover o disco ou desconectar o controlador até que a operação tenha terminado.

### Escolher o tipo de saída

Utilização **Imagem em bruto (SCP)** quando o objetivo é a captura arquivística, análise, recuperação ou posterior conversão. Uma imagem bruta registra informações de timing e múltiplas revoluções, que é útil para formatos incomuns, setores fracos, esquemas de proteção e mídia danificada.

Utilização **Formato de disco conhecido** quando você já conhece a família do disco e precisa de uma imagem do setor diretamente utilizável. Esta escolha pode ser menor e mais fácil de abrir em outro software, mas representa o resultado decodificado em vez de cada detalhe observado pela unidade.

Quando incerto, crie a imagem crua primeiro. Você pode convertê-lo mais tarde sem ler o disco novamente.

### Pasta, nome do ficheiro e perfil

A **Pasta ** é o directório de destino. A ** Nome do arquivo** deve identificar o disco sem depender apenas do seu rótulo físico. Um nome de arquivo útil contém o título, número de disco ou lado, e uma nota de condição quando aplicável. Não adicionar uma extensão de formato que entre em conflito com o formato de saída selecionado.

A **Perfil ** aplica um conjunto salvo de parâmetros de leitura. Selecione um apenas quando você sabe o que ele contém. A ** Predefinição** O perfil é adequado para uma primeira tentativa normal; um perfil de recuperação especializado pode deliberadamente ler mais revoluções ou uma faixa de via diferente e, portanto, demorar mais tempo.

### Configuração avançada

Expandir **Configuração avançada** Aceder a parâmetros específicos do formato ou peritos. Deixe estes valores inalterados, a menos que o disco exija uma faixa específica, contagem de rotações ou opção de controlador.

Os valores avançados comuns incluem:

| Configuração | Objecto | Quando mudar |
|---|---|---|
| Intervalo de faixas | Limita os cilindros e as cabeças a ler | Meios unilaterais, geometria incomum ou um passe de recuperação direcionado |
| Revolutions | Controla quantas rotações são amostradas | Aumento para faixas instáveis ou protegidas; reduzir apenas para velocidade quando apropriado |
| Argumentos de peritos | Passa parâmetros adicionais do motor | Apenas quando se segue documentado Greaseweazle orientação |

### Verificando uma leitura bem sucedida

Não confie apenas na ausência de uma janela de erro. Depois que o comando terminar:

1. Confirme se o arquivo de saída existe e não está vazio.
2. Leia as linhas finais do console para faixas falhadas ou ausentes.
3. Abrir a imagem em **Visualização** verificar se ambos os lados e a faixa de faixa esperada contêm dados.
4. Abre-o. **Disk Explorer** quando o sistema de arquivos é suportado.
5. Mantenha o registro de operação com importantes capturas de arquivos.

Se as leituras repetidas diferem, preservar cada captura bruta em vez de substituir a primeira. As diferenças podem ser úteis durante a recuperação.

## Gravando um disco

Abrir o **Escrever** tab para gravar uma imagem existente num disquete físico.

<p align="center"><img src="images/main-write-en.png" alt="Página de gravação" width="78%"></p>

### Procedimento de base

1. Inserir o disco de destino.
2. Selecione a imagem de origem com **Navegar**.
3. Confirme o formato detectado.
4. Selecione um perfil, se necessário.
5. Clique **Executar**.

A gravação substitui os dados no disco de destino. Verifique a unidade e imagem selecionadas antes de iniciar.

> **Aviso:** Escrever é destrutivo. Substitui dados magnéticos no disco de destino. Use um arquivo fonte protegido por gravação e um disco de destino separado sempre que possível.

### Antes de escrever

Verificar quatro itens antes de clicar **Executar**:

1. **Imagem:** o caminho selecionado é a imagem de origem pretendida.
2. **Disco:** O disco na unidade pode ser substituído com segurança.
3. **Unidade:** o tamanho e densidade configurados se adequam ao meio de destino.
4. **Formato:** detecção automática ou o formato manualmente selecionado corresponde à imagem.

Se a imagem de origem não foi testada, abra- a em **Visualização ** ou ** Disk Explorer** Primeiro. Uma escrita bem sucedida não pode reparar uma imagem de origem incompleta.

### Inspecção e modificação da via

Depois de uma imagem ser selecionada, **Visualizar faixas ** abre a sua representação. ** Modificar** expõe as modificações de imagem suportadas antes de escrever. As ações disponíveis dependem do formato e do motor selecionados.

### Verificando um disco escrito

Quando o motor suporta verificação, use-o para mídia importante. Caso contrário, leia o disco escrito de volta para uma nova imagem e compare seu conteúdo decodificado ou inspecione-o em **Visualização**. Mantenha a captura de verificação separada da imagem original para que o original nunca seja substituído.

Se a escrita falhar em faixas consistentes, verifique a condição do disco, a densidade, a limpeza da unidade e a configuração da unidade. Se as falhas ocorrerem aleatoriamente, verifique USB estabilidade e comunicação do controlador.

## Convertendo imagens de disco

A **Conversão** tab converte uma imagem de origem em um ou vários formatos de destino.

<p align="center"><img src="images/main-conversion-en.png" alt="Página de conversão" width="78%"></p>

### Procedimento de base

1. Selecione a imagem de origem.
2. Opcionalmente fornecer nomes de saída.
3. Escolha uma família de máquinas.
4. Selecione um ou mais formatos de saída e extensões.
5. Activar **Adicionar etiquetas** se os nomes de arquivos devem usar o padrão de tag configurado.
6. Clique **Executar**.

A **Seleccionado ** painel lista as saídas solicitadas. ** Migração de arquivos** fornece o fluxo de trabalho dedicado para migrar arquivos suportados em vez de executar uma conversão de imagem padrão.

### Seleccionar os formatos

A **Máquina ** lista filtra os formatos mostrados na ** Formato** Painel. Um nome de formato descreve o layout lógico do disco; a extensão descreve o recipiente de saída. Alguns formatos podem ser representados por mais de uma extensão, e alguns contêineres não podem preservar cada característica de uma fonte bruta.

Selecione apenas saídas que você realmente precisa. Vários formatos são úteis ao criar um mestre de arquivo, uma cópia compatível com emulador e uma cópia para outra ferramenta de análise em uma operação.

### Nome de saída e etiquetas

**Nomes de saída ** permite- lhe controlar os nomes de base gerados para os formatos seleccionados. ** Adicionar etiquetas ** aplica o padrão de ficheiros configurado em ** Opções > Geral**. Tags podem codificar família, formato, extensão, data ou hora. Visualize o exemplo em Opções antes de converter um lote grande para que os arquivos sejam nomeados consistentemente.

### Verificando resultados de conversão

Para cada saída solicitada:

1. Confirme que um arquivo foi criado.
2. Verifique o console para faixas ou setores que não puderam ser decodificados.
3. Abrir o resultado em **Disk Explorer** se contém um sistema de ficheiros suportado.
4. Compare a capacidade e o conteúdo esperados do disco com a fonte.

Uma conversão pode completar ao relatar perda de informação inerente ao formato de destino. Mantenha a imagem bruta original mesmo quando a imagem convertida aparecer correta.

## Visualizando uma imagem de disco

A **Visualização** tab mostra a estrutura e distribuição de dados de uma imagem.

<p align="center"><img src="images/main-visualization-en.png" alt="Página de visualização" width="78%"></p>

1. Clique **Abrir uma imagem de disco**.
2. Manter **Detecção automática** habilitado, ou selecione a máquina e formato manualmente.
3. Utilização **Ampliação da ligação** para manter ambos os lados no mesmo nível de zoom.
4. Utilização **Reiniciar** para restaurar a visão inicial.
5. Abrir **Inspector** para informações detalhadas sobre a região selecionada.

A legenda distingue fluxos normais, transições curtas e longas, cabeçalhos, dados decodificados e anomalias detectadas. Uma imagem bruta pode conter dados que não podem ser decodificados em um sistema de arquivos conhecido, mas ainda podem ser inspecionados aqui.

### Interpretando a visão

Cada grande painel circular representa um lado do disco. O centro identifica o lado e seu estado atual de dados; posições concêntricas correspondem a faixas. As cores classificam as regiões detectadas de acordo com a legenda. O visualizador pretende responder a perguntas como:

- A imagem contém dados de um lado ou de ambos?
- As faixas esperadas estão presentes?
- As anomalias são isoladas ou repetidas através do disco?
- A detecção automática identificou uma máquina plausível e formato?

Uma cor de anomalia é uma razão para inspecionar a região, não para provar que o disco é inutilizável. Proteção de cópia, formatação não padrão, uma gravação fraca e um setor danificado podem produzir diferentes estruturas que exigem interpretação contextual.

### Sequência de inspeção recomendada

Comece com o zoom ligado habilitado para comparar ambos os lados na mesma escala. Selecione uma região suspeita, aberta **Inspector**, e compará-lo com faixas vizinhas. Se o resultado parecer ser um problema de detecção, desabilite a detecção automática e escolha uma máquina e formato conhecidos. Retornar à detecção automática após o teste para que uma configuração forçada não seja usada acidentalmente para outra imagem.

## Explorando o conteúdo do disco

A **Disk Explorer** tab navega imagens de disco suportadas como uma hierarquia de arquivos.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Disk Explorer aba" width="78%"></p>

1. Abra uma imagem existente ou leia um disco.
2. Manter **Detecção automática** habilitado a menos que você precise forçar uma máquina ou formato.
3. Reveja as informações de volume: sistema, proteção, sistema de arquivos, capacidade, espaço livre e contagem de itens.
4. Navegue nas pastas no painel esquerdo.
5. Selecione um item para ver seus detalhes no painel direito.

Se o formato da imagem ou sistema de arquivos não for suportado, use **Visualização** para inspecionar a estrutura bruta.

### Compreender os painéis

O resumo superior descreve a imagem montada e o volume detectado. O painel inferior esquerdo contém a hierarquia do diretório. A tabela central lista itens no diretório selecionado com nome, data de modificação, tipo e tamanho. O painel direito mostra detalhes para o item selecionado.

Disk Explorer não implica que cada faixa bruta foi decodificada perfeitamente. Use o resumo de volume e a contagem de itens como uma rápida verificação de plausibilidade, então abra arquivos representativos ou compare-os com uma listagem de diretório conhecida quando a precisão de preservação importa.

### Quando nada aparece

Primeiro confirme que o caminho da imagem está correto. Em seguida, verifique a máquina e formato detectados. Uma imagem válida pode conter um sistema de arquivos não suportado ou danificado, caso em que o explorador pode permanecer vazio, mesmo que **Visualização** mostra dados gravados. Não sobrescrever ou descartar a imagem de origem com base apenas num explorador vazio.

## Usando as ferramentas

A **Ferramentas** grupos de páginas Greaseweazle Operações de manutenção.

<p align="center"><img src="images/main-tools-en.png" alt="Página Ferramentas" width="78%"></p>

Selecione um comando na lista à esquerda, reveja seus parâmetros e clique em **Executar**. Comandos destrutivos ou de mudança de hardware só devem ser usados após verificar o controlador e unidade selecionados.

A maioria das janelas de ferramentas contém três áreas: parâmetros no topo, um estado e uma área de saída bruta no centro e o comando gerado na parte inferior. As alterações de antevisão do comando como opções estão habilitadas. Um parâmetro não verificado normalmente significa “não modificar este valor”, enquanto que um parâmetro verificado inclui esse valor no comando.

Os diálogos diagnósticos individuais são descritos em [Diagnósticos de Hardware e manutenção](#hardware-diagnostics-and-maintenance).

## Emulação

### Abrindo uma máquina salva

A **Emulação ** listas de tabulação salvas configurações. Selecione um e clique ** Abrir**. Cada máquina em execução aparece em sua própria aba.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Tela de boas-vindas da emulação" width="78%"></p>

Criar e editar máquinas em **Opções > Emulação > Configurações ** e ** Opções > Emulação > Amiga**.

Se nenhuma configuração aparecer, crie uma em Opções primeiro. Uma configuração salva combina o modelo da máquina, versão do emulador, ROM, memória, vídeo, áudio, armazenamento e mapeamentos de entrada. A gravação de uma configuração não a inicia; retorna à principal **Emulação ** aba e clique ** Abrir**.

### Controlos das máquinas de correr

<p align="center"><img src="images/main-emulation-running-en.png" alt="Máquina emuladora em execução" width="78%"></p>

A barra de ferramentas de máquina em execução fornece controles de potência, pausa, reset, estado de salvamento, estado de carga, captura e exibição. Também mostra:

- os atalhos de gravação rápida e de carga rápida configurados;
- o renderizador ativo, como Direct3D 11;
- Os atalhos de ecrã completo e de libertação do rato;
- estado de áudio, controlador e mouse;
- a resolução atual, taxa de atualização e taxa de quadros.

A tira de disco na parte inferior do display de emulação gerencia mídia removível para cada unidade emulado. As atribuições do teclado podem ser alteradas em **Opções > Emulação > Atalhos**, enquanto os mapeamentos emulados de teclado, mouse e controlador são configurados no correspondente Amiga tabulações.

### Referência da barra de ferramentas

| Grupo de controlo | Objecto |
|---|---|
| Poder e pausa | Inicia, pára, pausa ou retoma a máquina emulado |
| Repor os controlos | Executa a ação configurada de redefinição suave ou dura |
| Controlos estatais | Salva ou carrega um estado emulador para uma rápida continuação |
| Capturar | Salva uma imagem da exibição emulado |
| Visualização | Muda a apresentação da tela ou entra em tela cheia |
| Lembrete de estado rápido | Mostra os atalhos de gravação/carregamento ativos |
| Renderizador | Reporta a infraestrutura de vídeo ativa |
| Chamada de entrada | Mostra os atalhos de ecrã completo e de libertação do rato |
| Indicadores de dispositivos | Reporta o estado de áudio, controle e mouse |
| Desempenho | Reporta tamanho de saída, frequência de atualização e taxa de quadros |

### Deixar o ecrã completo ou libertar o rato

A barra de ferramentas mostra as chaves atualmente atribuídas. Na configuração ilustrada, **Alt+ Voltar ** comuta a tela cheia e ** F12** liberta o rato. Trate os valores exibidos como autoritários porque os atalhos podem ser reatribuídos.

### Usando mídia de disquetes

A tira da unidade identifica cada unidade emulado, como `DF0:`. Use seus controles de mídia para inserir, substituir ou ejetar uma imagem. Substituir a mídia muda apenas o disco inserido da máquina em execução; ela não altera a definição de dispositivo de armazenamento na máquina salva, a menos que essa ação seja explicitamente salva.

## Opções do aplicativo

Abrir **Opções** da janela principal para configurar a aplicação.

### Geral

<p align="center"><img src="images/options-general-en.png" alt="Opções gerais" width="72%"></p>

A **Geral** tab contém:

- a pasta de imagem de disco padrão;
- linguagem de interface e tema;
- geração de nome de arquivo-tag para conversões;
- padrões de tag personalizados predefinidos e recentes;
- um exemplo de nome de arquivo ao vivo.

As variáveis de tag incluem o nome da fonte, família, formato, extensão, data e hora. Use o botão reset para restaurar o padrão padrão.

As actualizações de antevisão de ficheiros antes de quaisquer ficheiros serem criados. Use-o para detectar separadores duplicados, extensões em falta ou nomes ambíguos. Os padrões personalizados recentes fornecem acesso rápido a esquemas de nomenclatura anteriores sem substituir a predefinição atual.

### Registos

<p align="center"><img src="images/options-logs-en.png" alt="Opções de registo" width="72%"></p>

O registro pode ser configurado independentemente para cada operação. Para cada categoria, escolha se deseja salvar logs, definir o tamanho máximo do arquivo e decidir se os logs anteriores devem ser retidos. Um tamanho de `0` significa ilimitado. **Abrir pasta** abre a pasta de registos actual.

Activar **Manter os registos anteriores** para trabalhos de preservação e diagnóstico onde a história de várias tentativas importa. Desactiva- o quando apenas o resultado mais recente for útil. Os limites máximos de tamanho aplicam-se ao armazenamento de logs, não às imagens capturadas em disco.

### Controladores e unidades

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="Controladores e unidades" width="72%"></p>

Use esta aba para:

- Procurar por controladores ligados;
- adicionar e remover configurações da unidade;
- Selecione o tamanho, a densidade e a velocidade da unidade;
- salvar configurações de hardware;
- escolher ou localizar automaticamente `gw.exe`;
- verificar e baixar Greaseweazle Host Tools Actualizações;
- restaurar um caminho executável previamente configurado.

As configurações de hardware salvas permanecem disponíveis quando uma unidade é temporariamente desconectada.

#### Adicionando uma unidade

1. Clique **Digitalizar** e esperar que os controladores conectados apareçam.
2. Clique **Adicionar uma unidade** se a unidade requerida não estiver já listada.
3. Selecione seu número de unidade lógica, tamanho físico, densidade de gravação e velocidade de rotação.
4. Salva a linha.
5. Confirmar que mostra **Disponível ** e ** Configurado**.

Use o controle de lixo apenas para remover a configuração salva; ele não desconecta o hardware. Se o mesmo controlador aparecer em um diferente COM porto posterior, digitalize novamente antes de assumir que a porta armazenada ainda é válida.

#### Gestão Greaseweazle Host Tools

**Procurar gw.exe ** procura locais conhecidos. ** Escolher ** seleciona um executável específico. ** Verificar as actualizações ** consulta versões disponíveis sem substituir a instalada. ** Baixar a versão mais recente ** instala o pacote atual selecionado, e ** Usar o caminho anterior ** restaura a localização configurada anteriormente. Depois de alterar o executável, execute ** Informação do controlador** para confirmar que a versão selecionada pode se comunicar com o controlador.

### Motores

<p align="center"><img src="images/options-engines-en.png" alt="Selecção do motor" width="72%"></p>

Escolha o motor independentemente para ler, escrever, converter e Disk Explorer. O motor selecionado é usado estritamente: se não puder executar a operação solicitada, GW GUI relata a limitação em vez de mudar silenciosamente os motores.

Esta independência é intencional. Por exemplo, leituras físicas podem usar Greaseweazle Host Tools enquanto conversão de imagem e exploração usam o motor interno. Gravar as escolhas do motor em um perfil ou nota do projeto quando a reprodutibilidade importa.

### Perfis

<p align="center"><img src="images/options-profiles-en.png" alt="Perfis" width="72%"></p>

Os perfis armazenam configurações reutilizáveis para operações de leitura, escrita e conversão. Selecione a categoria relevante para gerenciar seus perfis. Um perfil selecionado é mostrado na barra de status da janela principal e nas telas de operação.

Use perfis para fluxos de trabalho repetitivos ao invés de como coleções inexplicáveis de flags de especialistas. Dê a cada perfil um nome específico, como uma unidade específica, família de discos ou método de recuperação. Reveja um perfil após atualizar o motor subjacente porque as opções suportadas podem mudar.

## Opções de emulação

A **Emulação** opções contêm configurações gerais de armazenamento, atalhos globais, configurações salvas e configurações específicas da máquina.

### Pastas gerais de emulação

<p align="center"><img src="images/options-emulation-general-en.png" alt="Opções gerais de emulação" width="72%"></p>

Defina a pasta de armazenamento de emulação compartilhada e as pastas padrão para captura e estados salvos. **Abrir pasta** abre a localização partilhada no Explorador de Ficheiros.

Mantenha as capturas e os estados salvos em pastas separadas. Uma captura é uma imagem comum; um estado salvo contém o estado específico da máquina do emulador e pode depender da versão e configuração do emulador que o criou. Faça backup da configuração e mídia ao lado de estados salvos importantes.

### Atalhos globais

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Atalhos de emulação" width="72%"></p>

Procure por uma ação ou atribuição de chaves, atribua ou remova atalhos, restaure padrões e limpe conflitos. A coluna de status identifica atribuições válidas e conflitantes.

Para alterar um atalho, encontre a ação, clique **Atribuir **, e pressione a combinação de teclas desejada. Verifique o estado antes de fechar Opções. ** Limpar conflitos ** remove atribuições conflitantes; ele não restaura o mapeamento padrão. Utilização ** Restaurar os padrões** quando você deseja substituir atribuições personalizadas com o conjunto padrão.

### Configurações salvas

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Configurações de emulação salvas" width="72%"></p>

Esta página lista máquinas salvas. Seleccione uma configuração para a editar na **Amiga** tab. Você pode atualizar a lista ou excluir a configuração selecionada.

Excluir uma configuração remove a definição da máquina salva. Não deve ser usado como forma de ejetar mídia ou fechar uma máquina em execução. Antes da eliminação, note qualquer ROM, imagem de disco rígido e arquivos de estado associados à configuração.

## Amiga configuração

A interface atual fornece detalhes Amiga páginas de configuração. A mesma estrutura de configurações pode ser estendida para outros sistemas emulados sem alterar o fluxo de trabalho principal.

### Geral

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga configurações gerais" width="72%"></p>

Escolha o Amiga modelar, salvar a configuração, instalar ou substituir a versão emulador, e definir pastas padrão para discos rígidos e outros meios. **Procurar versões** consulta a fonte oficial do emulador-versão.

Comece com o modelo porque ele restringe páginas posteriores. Alterá-lo pode alterar o disponível CPU, memória, ROM, chipset, e opções de armazenamento. Após selecionar uma versão emuladora, salve a configuração antes de lançá-la da janela principal. Instalar outra versão emuladora substitui a versão usada por essa configuração; ela não cria uma segunda cópia da máquina.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Amiga CPU configurações" width="72%"></p>

A CPU página mostra o processador selecionado pelo modelo da máquina e fornece precisão compatível, FPU, e escolhas de velocidade. As opções que não se aplicam ao modelo seleccionado permanecem desactivadas.

- **CPU modelo** identifica o processador emulado.
- **Precisão** controla o modelo de tempo. Os modos exatos do ciclo favorecem a compatibilidade do hardware, mas requerem mais processamento do host.
- **FPU** permite uma unidade de ponto flutuante compatível quando suportada.
- **CPU velocidade** selecciona o tempo original ou um modo acelerado.

Para uma configuração de base, mantenha o modelo derivado CPU e velocidade original. Altere a aceleração apenas após o arranque da máquina corretamente em suas configurações padrão.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Amiga RAM configurações" width="72%"></p>

Configurar o Chip RAM, Devagar RAM, Rápido RAM, e memória de expansão suportada. Mensagens de compatibilidade explicam restrições para a máquina selecionada, e a memória total configurada é exibida na parte inferior.

**Chip RAM ** é acessível aos chips personalizados e é exigido pela plataforma. ** Devagar RAM ** representa memória de expansão compatível usada por configurações comuns. ** Rápido RAM ** é memória de expansão orientada para o processador. ** Zorro III RAM** aplica-se apenas a modelos que suportam essa arquitetura de expansão. As mensagens de compatibilidade e controles desabilitados impedem combinações que o modelo selecionado não pode representar.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Amiga ROM configurações" width="72%"></p>

Selecione o sistema Kickstart ROM, extensão opcional ROM, e ROM Chave. O detectado...ROM lista exibe nomes, revisões e compatibilidade com o modelo selecionado. Selecionar um detectado ROM e clique **Utilização**, ou navegar para um arquivo manualmente.

ROM os ficheiros não são fornecidos por GW GUI. Use ROMs que você está legalmente autorizado a usar.

A lista detectada é preferível a adivinhar a partir de um nome de arquivo: ele reporta o ROM identidade e revisão e avalia compatibilidade com o modelo selecionado. **Compatível ** é a escolha normal; ** Parcialmente compatível ** indica que a ROM pode arrancar, mas não corresponde precisamente à máquina. ** Actualizar ** rescans o configurado ROM Locais. ** Utilização** atribui o selecionado detectado ROM à configuração.

### Vídeo

<p align="center"><img src="images/options-amiga-video-en.png" alt="Amiga configurações de vídeo" width="72%"></p>

Configure o padrão de vídeo, proporção de aspecto, resolução, modo de linha, corte de borda, renderizador, profundidade de cor, salto de quadro, gama, e fixação de brilho. Configurações adicionais de chipset estão disponíveis mais abaixo na página quando suportado pelo modelo selecionado.

| Configuração | Efeito prático |
|---|---|
| Padrão de vídeo | Selecciona PAL ou NTSC tempo e comportamento de atualização esperado |
| Razão de proporções | Controla como a imagem emulado é escalada |
| Resolução | Selecciona o detalhe de saída automático ou explícito |
| Modo de linha | Controles de tratamento de saída entrelaçada ou dupla linha |
| Fronteiras da cultura | Remove o overscan não usado somente quando habilitado |
| Renderização | Escolhe a infra- estrutura gráfica |
| Profundidade de cor | Selecciona a precisão da cor da saída |
| Saltar quadro | Reduz as imagens renderizadas quando activadas |
| Gama | Ajusta a resposta de brilho |
| Flicker fixador | Processa modos que de outra forma visivelmente piscam |

Altere uma configuração de exibição de cada vez. Se a janela de emulação ficar em branco ou instável, retorne à resolução automática, skip de frame desativado, gama neutro e o renderizador de trabalho anterior.

### Áudio

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Amiga configurações de áudio" width="72%"></p>

Activar ou desactivar o áudio, escolher o dispositivo de saída e a latência, depois configurar a interpolação, Amiga filtragem, tipo de filtro, separação estéreo, som disquete-drive e volume CD-audio.

A menor latência reduz o atraso, mas pode causar desistências em um computador ocupado. Aumenta se o áudio estalar. A Interpol e a Amiga O filtro de áudio muda a reprodução do som em vez de a lógica do programa emulado. O volume do som do motor controla o som mecânico simulado separadamente do normal Amiga Áudio.

### Armazenamento

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Amiga configurações de armazenamento" width="72%"></p>

A página de armazenamento lista identificadores do dispositivo, tipos, modelos, mídia associada e ações disponíveis. Adicionar, configurar ou remover dispositivos aqui. Disquetes e CDs podem ser inseridos ou substituídos diretamente de uma máquina em execução.

A **identificador do dispositivo ** é como o sistema emulado aborda o dispositivo. ** Tipo ** distingue disquete, disco rígido, óptica e outros dispositivos suportados. ** Modelo ** descreve o hardware emulado, enquanto ** Mídia associada** identifica a imagem atualmente atribuída. Configure o dispositivo antes de associar valiosos meios de escrita, e mantenha backups de imagens de disco rígido.

### Teclado

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Amiga configurações do teclado" width="72%"></p>

Procurar Amiga chaves e atribuições de host, atribuir novas chaves, remover mapeamentos, restaurar padrões ou limpar conflitos. A coluna de estado informa se cada atribuição é válida.

A coluna esquerda nomeia o emulado Amiga Chave; **Associação** mostra a combinação de teclas da máquina. Um mapeamento válido ainda pode ser inconveniente se o Windows ou o aplicativo se reservam o mesmo atalho, então teste combinações críticas dentro da máquina em execução. Evite atribuir o atalho de liberação do mouse ou tela cheia a uma chave que o software emulado precisa frequentemente.

### Mouse

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Amiga configurações do mouse" width="72%"></p>

Defina a velocidade física do mouse, escolha qual stick analógico controla o mouse, ajuste a zona morta analógica e velocidade, e configure mapeamentos de ação do mouse. Restaurar padrões ou limpar conflitos de mapeamento quando necessário.

Aumentar a zona morta se um controlador causar deriva do ponteiro. Ajuste a velocidade da vara esquerda e direita de forma independente quando ambas as varas estiverem habilitadas. A tabela de mapeamento mais baixa associa entradas de host com botões ou ações do mouse; inspecione seu estado de conflito após alterar os mapeamentos de controladores em outro lugar.

### Controladores

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Amiga configurações do controlador" width="72%"></p>

Detectar controladores conectados, atribuir dispositivos e tipos de controladores Amiga portas, e configurar mapeamentos de controladores e configurações de turbo-fogo. As escolhas disponíveis dependem do hardware detectado e da máquina selecionada.

A porta 1 e a porta 2 são configuradas de forma independente. **Automático** o tipo de controlador é um ponto de partida sensível, mas o software que espera um determinado joystick ou mouse pode exigir um tipo explícito. Executar a detecção antes de atribuir um controlador recém-conectado. O Turbo Fire ativa repetidamente uma entrada mapeada e deve permanecer desativado, a menos que o jogo ou aplicação beneficie dele.

## Diagnóstico e manutenção de hardware

Estas janelas são abertas a partir do **Ferramentas ** tab. Cada diálogo visualiza o gerado Greaseweazle Comando. Reveja- o antes de clicar ** Executar**.

### Informação do controlador

<p align="center"><img src="images/tool-controller-information-en.png" alt="Informação do controlador" width="62%"></p>

Exibe informações relatadas pelo controlador selecionado. Expandir **Resultado bruto** quando você precisa da resposta completa do comando.

Use isto como o primeiro comando de diagnóstico. Uma resposta bem sucedida confirma que GW GUI pode iniciar o executável do Host Tools configurado e comunicar com o dispositivo selecionado. Grave as informações de firmware e hardware antes de realizar uma atualização.

### USB largura de banda

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="USB largura de banda" width="62%"></p>

Medidas USB Largura de banda de comunicação. Use-o para diagnosticar transferências instáveis ou um inadequado USB ligação.

Feche outro software usando o controlador antes de testar. Repetir a medição após alterar a USB porto, cabo ou hub. Comparar resultados em condições semelhantes em vez de tratar uma única medição como uma garantia absoluta.

### Velocidade de condução

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Velocidade de condução" width="62%"></p>

Mede a velocidade de rotação. Aumente o número de medições quando você precisar de um resultado mais representativo.

Uma única medição é uma verificação rápida; várias medições revelam se a velocidade é estável. Deixe a unidade atingir a velocidade normal antes de interpretar o resultado. Um valor inesperado pode indicar uma velocidade configurada errada, um problema mecânico ou um problema de configuração de medição.

### Procurar cabeça

<p align="center"><img src="images/tool-seek-head-en.png" alt="Procurar cabeça" width="62%"></p>

Move a cabeça da unidade para um cilindro selecionado. **Permitir cilindros extremos ** permite posições normalmente restritas, e ** Manter o motor activo** deixa o motor ligado durante a operação. Use posições extremas apenas quando o procedimento de hardware as requer explicitamente.

A busca normal é útil para confirmar o movimento ou posicionamento da cabeça antes do diagnóstico. Ouça os impactos repetidos anormais e pare se o cilindro solicitado for inadequado para a unidade. Esta ferramenta não lê nem valida dados no cilindro de destino.

### Diagnóstico do alinhamento do motor

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Diagnóstico do alinhamento do motor" width="62%"></p>

Executa leituras repetidas para análise drive-alignment. Ele suporta seleção de faixas, contagem de rotações e leituras, formato de decodificação, fluxo bruto, índice, velocidade, PLL, pino de densidade, sector duro, TG43, e opções de dados inversos. O trabalho de alinhamento requer meios de referência adequados e conhecimento de hardware.

Comece com um disco de referência conhecido e o menor conjunto de sobreposições. **Alternando faixas ** define as vias e cabeças amostradas; ** Revoluções por via ** Controla a duração de cada amostra; ** Número de leituras** determina a repetição. Active uma definição de disco personalizada ou um formato de decodificação apenas quando corresponder à mídia de referência. Opções como índice falso, setores duros, PLL sobreposições, pinos de densidade, e TG43 são específicos de hardware ou formato e podem invalidar uma comparação quando usado incorretamente.

### Pins de hardware

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="Pins de hardware" width="62%"></p>

Lê ou altera um pino de controle suportado. Selecione o pino, habilitar **Mudar o pino ** somente ao escrever um valor, e selecionar ** Nível elevado** Quando exigido pela operação de hardware prevista.

Com **Mudar o pino** desabilitado, o comando consulta o pino. Este é o padrão mais seguro. Alterar um nível afeta diretamente o controlador I/O e deve ser feito apenas com o correto Greaseweazle documentação de hardware e fiação anexada.

### Reiniciar o controlador

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Reiniciar o controlador" width="62%"></p>

Reinicia o Greaseweazle controlador. Use isto quando o controlador for detectado, mas não responder normalmente.

Aguarde que qualquer operação ativa do disco termine antes de reiniciar. Depois, escaneie o controlador novamente se seu status de conexão não recuperar automaticamente. Um reset não repara um erro `gw.exe` caminho ou um desconectado USB dispositivo.

### Atrasos

<p align="center"><img src="images/tool-delays-en.png" alt="Atrasos do controlador" width="62%"></p>

Lê ou altera os valores de temporização do controlador, incluindo seleção, passo da cabeça, liquidação, motor, deseleção automática, tempo de gravação e atrasos na máscara de índice. Activar apenas os valores que pretende modificar.

Campos não verificados deixam o valor correspondente do controlador inalterado. Antes de editar, grave os valores existentes. Mudanças de tempo podem afetar cada operação física subsequente, então teste com mídia dispensável e restaure valores conhecidos se o comportamento não for confiável.

### Firmware

<p align="center"><img src="images/tool-firmware-en.png" alt="Actualização do Firmware" width="62%"></p>

Actualiza o firmware do controlador. **Actualizar o carregador de arranque** é explicitamente marcado como arriscado e deve permanecer desativado a menos que o procedimento oficial de firmware o exija. Não desconectar o controlador durante uma atualização.

Antes de atualizar, confirme o controlador conectado com **Informação do controlador**, usar um USB conexão, e fechar outro software que poderia acessá-lo. Após a conclusão, reconecte ou rescan o controlador e leia suas informações novamente para verificar a versão de firmware relatada.

## Registos e histórico de operações

Abra o histórico da operação para inspecionar logs salvos pela operação.

<p align="center"><img src="images/operation-history-en.png" alt="Histórico da operação" width="68%"></p>

Selecione um registro à esquerda para exibir seu conteúdo. **Exportação** salva uma cópia para diagnósticos ou suporte. Caminhos e linhas de comando podem conter nomes de pastas pessoais, então reveja os logs exportados antes de compartilhá-los.

O console ao vivo na janela principal mostra o comando atual e a saída recente. Seu botão de cópia copia o texto exibido.

### Lendo um registro

Um registro diagnóstico útil contém o comando gerado, timestamps, saída do motor e o status final. Trabalhe de baixo para cima: identifique o erro final e, em seguida, localize o primeiro aviso ou pista falha que o precedeu. Uma falha genérica posterior é muitas vezes apenas a consequência de uma mensagem anterior, mais específica.

Ao comparar duas tentativas, verifique se o controlador, drive, motor, perfil, caminho de origem, formato de saída e argumentos de especialistas foram idênticos. Caso contrário, um resultado diferente pode refletir configurações alteradas em vez de instabilidade de disco.

## Dados de aplicação e uso portátil

GW GUI mantém os dados do usuário separados dos binários de aplicativos. Dependendo do pacote e modo selecionados, as configurações, os registros, as ferramentas baixadas, os componentes do emulador, as capturas, os estados e as configurações da máquina são armazenados na aplicação `Data` diretório ou nas localizações configuradas de dados do usuário.

Antes de substituir ou mover uma instalação portátil, mantenha a pasta completa da aplicação em conjunto e faça backup da `Data` pasta. Não mover arquivos individuais de `lib`, porque o aplicativo resolve suas próprias e de terceiros bibliotecas a partir dessa estrutura.

### Conteúdo de backup sugerido

Faça backup do seguinte quando forem importantes para o seu fluxo de trabalho:

- Definições e perfis da aplicação;
- Definições do controlador e da unidade;
- configurações de emulação;
- ROM caminhos e legalmente detidos ROM cópias de segurança;
- Imagens de disco rígido e de suporte removível;
- Capturas e estados salvos;
- Registos de operações utilizados como registos de conservação.

As imagens de disco podem ser muito maiores do que as configurações. Armazenar mestres de arquivo somente leitura quando possível, e trabalhar em cópias.

## Fluxos de trabalho recomendados

### Arquivando um disco desconhecido

1. Inspecionar e limpar a unidade utilizando um procedimento de manutenção adequado.
2. Gravar-proteger o disco, se possível.
3. Selecionar **Ler > Imagem em bruto (SCP)**.
4. Use um nome de arquivo descritivo e leia a faixa normal com múltiplas revoluções.
5. Revise o console e salve o log.
6. Inspecionar ambos os lados **Visualização**.
7. Converta uma cópia para formatos de setor prováveis.
8. Testar as cópias convertidas em **Disk Explorer** ou software adequado.
9. Preservar o mestre bruto, log, e notas juntos.

### Recrear um disco de uma imagem

1. Inspecione a imagem e confirme sua família e formato esperados.
2. Inserir um disco descartável ou intencionalmente gravável do tamanho e densidade corretos.
3. Abrir **Escrever** e selecione a imagem.
4. Confirme a unidade configurada e o formato detectado.
5. Escreve o disco.
6. Leia-o de volta para uma imagem de verificação separada.
7. Compare conteúdos decodificados e reveja faixas suspeitas visualmente.

### Criando um emulado Amiga

1. Abrir **Opções > Emulação > Configurações** e criar ou selecionar uma máquina.
2. In **Amiga > Geral**, escolha o modelo e a versão emuladora.
3. Atribuir um compatível, obtido legalmente ROM.
4. Manter os padrões do modelo para CPU e RAM na primeira bota.
5. Configure vídeo e áudio com configurações automáticas conservadoras.
6. Adicione dispositivos de armazenamento e associe imagens de mídia copiadas.
7. Reveja as atribuições de teclado, mouse e controle.
8. Gravar a configuração.
9. Voltar para **Emulação **, selecione-o, e clique ** Abrir**.
10. Apenas depois de uma inicialização de base bem-sucedida, alterar aceleração ou configurações avançadas uma de cada vez.

## Verificação de segurança

Antes **Ler**:

- O disco de origem está na unidade correcta;
- Se possível, a fonte é protegida por escrita;
- o caminho de saída não substituirá um mestre existente;
- O perfil e o intervalo de faixas correspondem ao disco.

Antes **Escrever ** ou ** Apagar**:

- O disco de destino pode ser destruído;
- a imagem e a unidade estão corretas;
- o tamanho e a densidade do disco são compatíveis;
- nenhum mestre de arquivo está sendo usado como destino.

Antes de uma ferramenta de mudança de hardware:

- Nenhuma outra operação está em execução;
- O controlador correcto é seleccionado;
- Os valores actuais foram registados;
- o controlador tem potência estável e USB Conectividade;
- a ação é suportada pela documentação do hardware.

## Resolução de Problemas

### O controlador não está listado

1. Reconectar o controlador diretamente ao computador.
2. Abrir **Opções > Controladores e unidades**.
3. Clique **Digitalizar**.
4. Verifique o estado do controlador e a configuração da unidade.
5. Executar **Informação do controlador** se a detecção tiver sucesso, mas os comandos falharem.

Se ainda não aparecer, tente outro direto USB bombordo e cabo, depois reescane. Verifique o Windows Device Manager para um dispositivo serial recentemente detectado. Um controlador visível para Windows mas ausente de GW GUI geralmente aponta para uma porta ocupada, configuração defasada ou problema do Host Tools; um controlador ausente do Windows aponta para USB, potência, driver ou hardware.

### `gw.exe` não foi encontrado

Abrir **Opções > Controladores e unidades **, em seguida, utilizar ** Procurar gw.exe **, ** Escolher **, ou ** Baixar a versão mais recente**. Confirmar que o caminho detectado aponta para o Greaseweazle instalação.

Depois de selecioná- lo, execute **Informação do controlador**. Se isso falhar antes de contactar o hardware, inspeccione o log para um caminho executável inválido, arquivos em falta ou uma versão que não possa ser iniciada.

### Uma operação utiliza o motor errado

Abrir **Opções > Motores** e verificar o motor atribuído a essa operação exata. GW GUI não deve voltar silenciosamente para o outro motor.

As configurações do motor são separadas: mudar o motor de conversão não muda leitura, escrita, ou Disk Explorer. Reabrir a operação falhando após salvar a opção e confirmar o comando gerado no console.

### Uma imagem não é reconhecida

Desactivar a detecção automática apenas se souber a máquina e o formato correctos. Caso contrário, tente o **Visualização** aba para inspecionar a imagem em um nível inferior.

Verifique se a fonte é uma captura de fluxo bruto, uma imagem do setor, um recipiente comprimido, ou um arquivo não relacionado com uma extensão enganosa. Nunca renomeie uma extensão apenas para forçar a detecção; a conversão deve interpretar a estrutura da fonte corretamente.

### A emulação não começa

Verificar a configuração salva, a versão instalada do emulador, selecionada ROM, caminhos de armazenamento e compatibilidade de modelos. Reveja o log do aplicativo para os detalhes completos do erro.

Retorno temporário CPU, RAM, vídeo e armazenamento para uma linha de base simples compatível com modelos. Se a linha de base começar, restaure uma configuração personalizada de cada vez. Um estado salvo criado com outra versão emulador ou definição de máquina também pode falhar mesmo quando uma inicialização limpa funciona.

### Um atalho ou entrada não funciona

Verificar tanto o global **Emulação > Atalhos** página e a máquina-específica teclado, mouse, ou página de controle. Resolver qualquer tarefa marcada como conflitante.

Se o mouse for capturado, use o atalho de liberação exibido na barra de ferramentas da máquina em execução. Se um controlador foi conectado após a abertura das Opções, execute novamente a detecção do controlador antes de atribuí-lo.

### Um comando falhou inesperadamente

1. Leia a saída do console ao vivo.
2. Abrir **Histórico da operação** para o log gravado completo.
3. Confirme os caminhos de controle, unidade, perfil, motor e arquivo selecionados.
4. Exportar o diário de bordo relevante se tiver de ser partilhado para diagnóstico.

### O áudio estala ou pausa

Aumentar a latência do áudio da emulação, fechar CPU- aplicações intensivas, e retorno de vídeo skipping quadro e aceleração aos seus valores anteriores. Verifique se o dispositivo de áudio do Windows pretendido está selecionado. Alterar uma configuração de cada vez para que a correção eficaz seja identificável.

### O ecrã de emulação está em branco ou lento

Devolver a resolução e o modo de linha para **Automático**, desactivar o skipping de frame e a fixação de flicker temporariamente, e tentar o renderizador de trabalho anterior. Confirmar que o configurado ROM e os meios de inicialização inseridos são válidos. A FPS indicador ajuda a distinguir um problema de renderização-desempenho de uma máquina que simplesmente não inicializou.

### Uma leitura contém faixas instáveis

Repita a leitura para um novo nome de arquivo, aumente as revoluções quando apropriado e compare as faixas afetadas. Limpe as cabeças da unidade usando um procedimento correto e inspecione o disco para danos físicos. Não leia repetidamente derrapar visivelmente ou danificar os meios de comunicação, porque outros passes podem piorar.

## Glossário

| Termo | Significado em GW GUI |
|---|---|
| Controlador | A Greaseweazle interface de hardware conectada sobre USB |
| Dirija | A unidade de disquete física ligada ao controlador |
| Motor | A implementação selecionada para executar uma operação |
| Fluxo | Informação cronometrando representando transições magnéticas lidas de um disco |
| Imagem em bruto | Uma captura mantendo informações de disco de baixo nível, como SCP |
| Imagem do setor | Uma representação descodificada organizada em sectores lógicos |
| Revolução | Uma rotação completa amostrada ao ler uma faixa |
| Cilindro | Posição da cabeça radial; um cilindro pode conter uma faixa de cada lado |
| Cabeçalho | O lado do disco selecionado pela unidade física |
| Perfil | Um conjunto reutilizável de configurações para uma operação |
| ROM | Imagem de Firmware exigida por uma máquina emuladora |
| Estado salvo | Um instantâneo do estado da máquina de um emulador em execução |
| Renderizador | A infra- estrutura gráfica usada para mostrar o resultado da emulação |

## Referência rápida

| Se quiseres... | Vai para... |
|---|---|
| Preservar um disco físico | **Ler** |
| Colocar uma imagem de volta num disco | **Escrever** |
| Produzir outro formato de imagem | **Conversão** |
| Inspecionar vias ou anomalias de fluxo | **Visualização** |
| Procurar arquivos dentro de uma imagem | **Disk Explorer** |
| Verificar a comunicação do controlador | **Ferramentas > Informação do controlador** |
| Medir a rotação da unidade | **Ferramentas > Velocidade de condução** |
| Rever um comando passado | **Histórico da operação** |
| Configurar hardware | **Opções > Controladores e unidades** |
| Selecionar implementações | **Opções > Motores** |
| Criar ou editar uma máquina emuladora | **Opções > Emulação** |
| Iniciar uma máquina gravada | **Emulação** |
