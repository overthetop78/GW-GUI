# GW GUI Guida utente

GW GUI è un'applicazione Windows per la lettura, la scrittura, la conversione, l'ispezione e l'emulazione di immagini floppy-disk. Può controllare Greaseweazle hardware, lavorare con i file di immagine disco attraverso il suo motore interno, ed eseguire le configurazioni emulate-macchina salvate.

Questa guida descrive l'interfaccia inglese mostrata nella versione attuale dell'applicazione. È scritto come fonte del manuale dell'utente stampabile: gli screenshot illustrano i controlli, mentre il testo circostante spiega cosa scegliere, perché scegliere e come verificare il risultato.

> **Importante:** Leggere un disco non è distruttivo. Scrivere, cancellare, aggiornare il firmware e alcuni strumenti hardware possono modificare supporti o hardware. Leggere l'avvertimento allegato alla relativa procedura prima di fare clic ** Esecuzione**.

### Come utilizzare questa guida

Se questa è la tua prima volta GW GUI, completo [Getting iniziato](#getting-started), poi seguire [Leggi un disco](#reading-a-disk). Se l'applicazione è già configurata, vai direttamente al capitolo per l'operazione che si desidera eseguire. I capitoli delle opzioni servono come riferimento quando una procedura ti chiede di cambiare un'unità, un motore, un profilo o un'impostazione emulata-macchina.

I nomi di interfaccia sono visualizzati in **coraggiosa**. I nomi dei file, i percorsi, i comandi e i valori letterali sono visualizzati come `code`. Le note spiegano il comportamento normale; gli avvisi identificano le operazioni che possono alterare un disco, un controller o una configurazione memorizzata.

## Indice

1. [A parte il flusso di lavoro](#understanding-the-workflow)
2. [Già cominciarono](#getting-started)
3. [La finestra principale](#main-window)
4. [Leggi un disco](#reading-a-disk)
5. [Scrivere un disco](#writing-a-disk)
6. [Convertire le immagini del disco](#converting-disk-images)
7. [Visualizzare un'immagine del disco](#visualizing-a-disk-image)
8. [Esplora il contenuto del disco](#exploring-disk-contents)
9. [Usando gli strumenti](#using-the-tools)
10. [Emulazione](#emulation)
11. [Opzioni di applicazione](#application-options)
12. [Opzioni di emulazione](#emulation-options)
13. [Amiga configurazione](#amiga-configuration)
14. [Sistema diagnostico e manutenzione dell'Hardware](#hardware-diagnostics-and-maintenance)
15. [Log e cronologia delle operazioni](#logs-and-operation-history)
16. [Dati di applicazione e uso portatile](#application-data-and-portable-use)
17. [Consigliati flussi di lavoro](#recommended-workflows)
18. [Richiesta sicurezza](#safety-checklist)
19. [Risoluzione dei problemi](#troubleshooting)
20. [Glossary](#glossary)
21. [Riferimento rapido](#quick-reference)

## Capire il flusso di lavoro

GW GUI separa le operazioni fisiche-disk dalle operazioni di file immagine:

| Obiettivo | Input | Produzione | Pagina consigliata |
|---|---|---|---|
| Conservare un disco floppy | Disco fisico | File immagine | **Leggi** |
| Ricreare un disco floppy | File immagine | Disco fisico | **Scrivere** |
| Cambia formato immagine | File immagine | Uno o più file immagine | **Conversione** |
| Ispezione di tracce e anomalie | File immagine | Analisi visiva | **Visualizzazione** |
| Sfoglia i file memorizzati in un'immagine | Sistema di immagine/file supportato | File e directory | **Disk Explorer** |
| Diagnosi di un'unità o di un controller | Greaseweazle hardware | Misure o stato | **Strumenti** |
| Eseguire una macchina virtuale salvata | Configurazione delle macchine salvate | Sessione di emulazione | **Emulazione** |

Per la conservazione, fare prima una cattura cruda e tenerlo invariato come un maestro. Crea copie di lavoro convertite o riparate da quel master. Questo evita di ripetere una lettura fisica e conserva informazioni che un formato basato sul settore potrebbe non conservare.

## Iniziare

### Requisiti

- Finestre con Microsoft .NET Desktop Runtime richiesto dall'applicazione.
- A Greaseweazle controllore per operazioni di floppy-disk fisiche.
- Un percorso configurato per `gw.exe` quando si utilizza Greaseweazle Host Tools motore.
- Legalmente ottenuto ROM file quando una macchina emulata richiede loro.

L'applicazione controlla il tempo di esecuzione necessario .NET all'avvio. Se manca, seguire il prompt di installazione, quindi riavviare GW GUI.

### Prima di collegare l'hardware

Controllare quanto segue prima di eseguire un'operazione fisica-disco:

1. Collegare Greaseweazle controllore a una stabile USB Porto.
2. Collegare il cavo floppy con l'orientamento corretto.
3. Collegare l'alimentatore dell'unità prima di inserire supporti preziosi.
4. Confermare che la dimensione e la densità dell'unità corrispondono al disco.
5. Scrivi-proteggi il disco sorgente quando possibile.

GW GUI non può impedire danni causati da cablaggi errati, alimentazione inadatta, o un'unità meccanicamente non sicura. Test hardware non familiare con un disco espandibile prima.

### Primo lancio

1. Aperto `gwgui.exe`.
2. Aperto **Opzioni**.
3. In **Regolatori e unità**, eseguire la scansione del controller e configurare l'unità.
4. Verificare o selezionare il percorso `gw.exe`.
5. In **Motori**, scegliere quale motore dovrebbe eseguire ogni operazione.
6. Torna alla finestra principale e seleziona la scheda di funzionamento richiesta.

### Confermare che la configurazione è pronta

Una configurazione di lavoro dovrebbe mostrare il controller e guidare nella barra di stato, ad esempio un numero di unità, dimensioni, densità e COM Porto. In **Opzioni > Regolatori e unità **, il controller deve essere contrassegnato ** Disponibile ** e l'unità ** Configurato **. Correre ** Informazioni sul controller** prima di leggere supporti preziosi se si desidera verificare la comunicazione senza alterare un disco.

### Scegliere un motore

GW GUI può esporre più di una implementazione per alcune operazioni. The **Greaseweazle Host Tools** motore invoca il configurato `gw.exe`; GW GUI maniglie motore supportate all'interno dell'applicazione. La selezione del motore è esplicita e indipendente per la lettura, la scrittura, la conversione e Disk Explorer. Se un'operazione non è supportata dal motore selezionato, GW GUI segnala che la condizione invece di cambiare i motori automaticamente.

## Finestra principale

La finestra principale raggruppa le operazioni principali in sette schede:

- **Leggi** crea un'immagine da un disco fisico.
- **Scrivere** scrive un'immagine a un disco fisico.
- **Conversione** converte un formato di immagine disco in uno o più formati di output.
- **Visualizzazione** visualizza tracce e flussi o dati decodificati.
- **Disk Explorer** naviga i file system supportati e il contenuto del disco.
- **Strumenti** fornisce la manutenzione hardware e comandi diagnostici.
- **Emulazione** gestisce e gestisce le macchine emulate salvate.

La console in basso visualizza il comando eseguito e la sua uscita. La barra di stato riporta l'unità selezionata, il profilo e lo stato attuale.

### Leggere l'interfaccia

La maggior parte delle pagine di funzionamento segue lo stesso modello:

1. **Fonte o destinazione** i controlli identificano il disco, l'immagine o la cartella.
2. **Formato controlli** selezionare il rilevamento automatico o una macchina esplicita e il formato.
3. **Controlli dei profili** applicare impostazioni riutilizzabili.
4. **Impostazioni avanzate** esporre i parametri che sono normalmente facoltativi.
5. **Esecuzione** inizia l'operazione.
6. The **console** mostra il comando generato, il progresso, gli avvisi e gli errori.

The **Esecuzione** pulsante non implica che tutti i valori siano sicuri per il disco inserito. Controllare sempre la destinazione e l'unità selezionata prima di un'operazione di scrittura o manutenzione.

### Barra di stato e console

Il lato sinistro della barra di stato identifica l'unità fisica attiva. Il centro mostra il profilo attivo quando viene selezionato. L'indicatore di stato segnala se l'applicazione è pronta o impegnata. La console non è semplicemente diagnostica: è il record autorevole del comando inviato al motore selezionato. Utilizzare il suo controllo di copia quando è necessario conservare o condividere quel comando.

## Leggere un disco

Aprire **Leggi** scheda per catturare un disco floppy fisico come immagine.

<p align="center"><img src="images/main-read-en.png" alt="Leggi la scheda" width="78%"></p>

### Procedura di base

1. Inserire il disco sorgente nell'unità configurata.
2. Scegli il tipo di immagine:
   - **Immagine grezzaSCP)** conserva informazioni a livello di flusso.
   - **Formato del disco conosciuto** crea un'immagine utilizzando una macchina e un formato selezionato.
3. Scegli la cartella di destinazione.
4. Inserisci il nome del file di output.
5. Seleziona un profilo se necessario.
6. Fare clic **Esecuzione**.

La console mostra il comando esatto e il progresso. Non rimuovere il disco o scollegare il controller fino a quando l'operazione non è finita.

### Scegliere il tipo di uscita

Uso **Immagine grezzaSCP)** quando l'obiettivo è la cattura, l'analisi, il recupero o la conversione successiva. Un'immagine grezza registra informazioni di tempistica e rivoluzioni multiple, che è utile per formati insoliti, settori deboli, schemi di protezione e media danneggiati.

Uso **Formato del disco conosciuto** quando si conosce già la famiglia del disco e ha bisogno di un'immagine del settore direttamente utilizzabile. Questa scelta può essere più piccola e più facile da aprire in altri software, ma rappresenta il risultato decoded piuttosto che ogni dettaglio osservato dall'unità.

Quando incerto, creare l'immagine grezza prima. È possibile convertirlo in seguito senza leggere di nuovo il disco.

### Cartella, nome del file e profilo

The **Cartella ** è la directory di destinazione. The ** Nome del file** dovrebbe identificare il disco senza fare affidamento solo sulla sua etichetta fisica. Un nome d'archivio utile contiene il titolo, il numero di disco o il lato, e una nota di condizione quando applicabile. Non aggiungere un'estensione di formato in conflitto con il formato di output selezionato.

A **Profilo ** applica un insieme salvato di parametri di lettura. Seleziona uno solo quando sai cosa contiene. The ** Predefinito** il profilo è appropriato per un normale primo tentativo; un profilo di recupero specializzato può deliberatamente leggere più rivoluzioni o una diversa gamma di traccia e quindi richiedere più tempo.

### Impostazioni avanzate

Espansione **Impostazioni avanzate** per accedere ai parametri specifici del formato o esperti. Lasciare questi valori invariati a meno che il disco non richieda una particolare gamma di tracce, conteggio di rivoluzione o opzione controller.

I valori comuni avanzati includono:

| Impostazione | Oggetto | Quando cambiarlo |
|---|---|---|
| Gamma di traccia | Limita i cilindri e le teste da leggere | Supporti unilaterali, geometria insolita, o un passaggio di recupero mirato |
| Rivoluzioni | Controlla quante rotazioni vengono campionate | Aumento delle tracce instabili o protette; ridurre solo la velocità quando necessario |
| Argomenti esperti | Passi ulteriori parametri del motore | Solo quando segue documentato Greaseweazle orientamento |

### Verificare una lettura di successo

Non fare affidamento solo sull'assenza di una finestra di dialogo di errore. Dopo il comando completa:

1. Confermare che il file di output esiste e non è vuoto.
2. Leggi le linee di console finali per tracce fallite o mancanti.
3. Aprire l'immagine in **Visualizzazione** per verificare che entrambi i lati e l'intervallo di traccia prevista contengono dati.
4. Aprire **Disk Explorer** quando il file system è supportato.
5. Tenere il registro delle operazioni con importanti catture archivistiche.

Se le letture ripetute differiscono, preservare ogni cattura cruda piuttosto che sovrascrivere il primo. Le differenze possono essere utili durante il recupero.

## Scrivere un disco

Aprire **Scrivere** scheda per scrivere un'immagine esistente a un disco floppy fisico.

<p align="center"><img src="images/main-write-en.png" alt="Scrivi scheda" width="78%"></p>

### Procedura di base

1. Inserire il disco di destinazione.
2. Selezionare l'immagine sorgente con **Sfoglia**.
3. Confermare il formato rilevato.
4. Seleziona un profilo se necessario.
5. Fare clic **Esecuzione**.

La scrittura sostituisce i dati sul disco di destinazione. Verificare l'unità selezionata e l'immagine prima di iniziare.

> **Attenzione:** La scrittura è distruttiva. Sostituisce i dati magnetici sul disco di destinazione. Utilizzare un archivio sorgente protetto da scrittura e un disco di destinazione separato quando possibile.

### Prima di scrivere

Controllare quattro elementi prima di cliccare **Esecuzione**:

1. **Immagine:** il percorso selezionato è l'immagine di origine prevista.
2. **Disco:** il disco nell'unità può essere tranquillamente sovrascritto.
3. **Guida:** la dimensione configurata e la densità si adattano al mezzo di destinazione.
4. **Formato:** il rilevamento automatico o il formato selezionato manualmente corrisponde all'immagine.

Se l'immagine sorgente non è stata testata, aprila **Visualizzazione ** o ** Disk Explorer** Prima. Una scrittura di successo non può riparare un'immagine sorgente incompleta.

### ispezione e modifica della pista

Dopo che un'immagine è selezionata, **Visualizza le tracce ** apre la sua rappresentazione traccia. ** Modifica** espone le modifiche dell'immagine supportate prima di scrivere. Le azioni disponibili dipendono dal formato e dal motore selezionato.

### Verificare un disco scritto

Quando il motore supporta la verifica, utilizzarlo per i media importanti. Altrimenti, leggere il disco scritto di nuovo a una nuova immagine e confrontare il suo contenuto decoded o ispezionare in **Visualizzazione**. Mantenere la cattura di verifica separata dall'immagine originale in modo che l'originale non sia mai sovrascritto.

Se la scrittura non riesce a tracce coerenti, controlla la condizione del disco, la densità, la pulizia dell'unità e la configurazione dell'unità. Se si verificano guasti casualmente, controllare USB stabilità e comunicazione del controller.

## Convertire immagini disco

The **Conversione** scheda converte un'immagine sorgente in uno o più formati di destinazione.

<p align="center"><img src="images/main-conversion-en.png" alt="Scheda di conversione" width="78%"></p>

### Procedura di base

1. Selezionare l'immagine sorgente.
2. Opzionalmente fornire nomi di output.
3. Scegli una famiglia di macchine.
4. Selezionare uno o più formati di output e estensioni.
5. Abilitare **Aggiungi i tag** se i nomi di file devono utilizzare il modello di tag configurato.
6. Fare clic **Esecuzione**.

The **Selezionato ** pannello elenca le uscite richieste. ** migrazione file** fornisce il flusso di lavoro dedicato per la migrazione di file supportati piuttosto che eseguire una conversione di immagine standard.

### Selezione dei formati

The **Macchina ** elenco filtri i formati visualizzati nel ** Formato** pannello. Un nome di formato descrive il layout del disco logico; l'estensione descrive il contenitore di uscita. Alcuni formati possono essere rappresentati da più di un'estensione, e alcuni contenitori non possono preservare ogni caratteristica di una fonte grezza.

Seleziona solo gli output di cui hai bisogno. I formati multipli sono utili quando si crea un master archivistico, una copia compatibile con l'emulatore e una copia per un altro strumento di analisi in un'unica operazione.

### Denominazione e tag di uscita

**Nomi di uscita ** consente di controllare i nomi di base generati per i formati selezionati. ** Aggiungi i tag ** applica il modello del nome del file configurato in ** Opzioni > Generale**. Tags possono codificare famiglia, formato, estensione, data o ora. Anteprima l'esempio in Opzioni prima di convertire un grande lotto in modo che i file vengano nominati in modo coerente.

### Controllare i risultati della conversione

Per ogni uscita richiesta:

1. Conferma che è stato creato un file.
2. Controllare la console per tracce o settori che non potrebbero essere decodificati.
3. Aprire il risultato in **Disk Explorer** se contiene un file system supportato.
4. Confrontare la capacità del disco prevista e i contenuti con la fonte.

Una conversione può completare durante la segnalazione perdita di informazioni che è inerente al formato di destinazione. Mantenere l'immagine grezza originale anche quando l'immagine convertita appare corretta.

## Visualizzazione di un'immagine del disco

The **Visualizzazione** scheda visualizza la struttura e la distribuzione dei dati di un'immagine.

<p align="center"><img src="images/main-visualization-en.png" alt="Scheda di visualizzazione" width="78%"></p>

1. Fare clic **Aprire un'immagine del disco**.
2. Continua **Rilevamento automatico** abilitato, o selezionare la macchina e il formato manualmente.
3. Uso **Link zoom** mantenere entrambi i lati allo stesso livello di zoom.
4. Uso **Ripristino** per ripristinare la vista iniziale.
5. Aperto **Ispettore** per informazioni dettagliate sulla regione selezionata.

La leggenda distingue il flusso normale, transizioni brevi e lunghe, intestazioni, dati decodificati e anomalie rilevate. Un'immagine raw può contenere dati che non possono essere decodificati in un file system noto, ma possono ancora essere ispezionati qui.

### Interpretare la vista

Ogni grande pannello circolare rappresenta un lato disco. Il centro identifica il lato e lo stato dei dati corrente; le posizioni concentriche corrispondono alle tracce. I colori classificano le regioni rilevate secondo la leggenda. Il visualizzatore è destinato a rispondere a domande come:

- L'immagine contiene dati da un lato o entrambi?
- Sono presenti le tracce attesi?
- Le anomalie sono isolate o ripetute sul disco?
- Il rilevamento automatico ha identificato una macchina e un formato plausibile?

Un colore anomalia è un motivo per ispezionare la regione, non provare che il disco è inutilizzabile. Protezione copia, formattazione non standard, una registrazione debole e un settore danneggiato possono produrre diverse strutture che richiedono un'interpretazione contestuale.

### Sequenza di ispezione consigliata

Iniziare con zoom collegato abilitato per confrontare entrambi i lati con la stessa scala. Seleziona una regione sospetta, apri **Ispettore**, e confrontarlo con le tracce vicine. Se il risultato sembra essere un problema di rilevamento, disabilitare il rilevamento automatico e scegliere una macchina e un formato noti. Ritorno al rilevamento automatico dopo il test in modo che un'impostazione forzata non sia accidentalmente utilizzata per un'altra immagine.

## Esplorare il contenuto del disco

The **Disk Explorer** scheda naviga le immagini del disco supportate come gerarchia dei file.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Disk Explorer scheda" width="78%"></p>

1. Aprire un'immagine esistente o leggere un disco.
2. Continua **Rilevamento automatico** abilitato a meno che non sia necessario forzare una macchina o un formato.
3. Rivedere le informazioni sul volume: sistema, protezione, file system, capacità, spazio libero e conteggio articoli.
4. Sfoglia le directory nel pannello di sinistra.
5. Selezionare un elemento per visualizzare i suoi dettagli nel pannello giusto.

Se il formato dell'immagine o il file system non è supportato, utilizzare **Visualizzazione** ispezionare invece la struttura grezza.

### Comprendere i pannelli

Il riassunto superiore descrive l'immagine montata e il volume rilevato. Il pannello inferiore sinistro contiene la gerarchia delle directory. La tabella centrale elenca gli elementi nella directory selezionata con nome, data di modifica, tipo e dimensione. Il pannello destro mostra i dettagli per l'elemento selezionato.

Disk Explorer non implica che ogni traccia cruda sia stata decodificata perfettamente. Utilizzare il riepilogo del volume e il conteggio dell'elemento come un rapido controllo di plausibilità, quindi aprire i file rappresentativi o confrontarli con un elenco di directory noto quando la precisione di conservazione è importante.

### Quando nulla appare

Prima conferma che il percorso dell'immagine è corretto. Quindi controllare la macchina rilevata e il formato. Un'immagine valida può contenere un file system non supportato o danneggiato, in tal caso l'esploratore può rimanere vuoto anche se **Visualizzazione** mostra i dati registrati. Non sovrascrivere o scartare l'immagine sorgente basata solo su un esploratore vuoto.

## Utilizzo degli strumenti

The **Strumenti** scheda gruppi Greaseweazle operazioni di manutenzione.

<p align="center"><img src="images/main-tools-en.png" alt="Scheda strumenti" width="78%"></p>

Selezionare un comando dall'elenco a sinistra, rivedere i suoi parametri, quindi fare clic **Esecuzione**. I comandi distruttivi o di sostituzione hardware devono essere utilizzati solo dopo la verifica del controller e dell'unità selezionato.

La maggior parte delle finestre di dialogo degli strumenti contengono tre aree: i parametri in alto, un'area di stato e di output grezzo nel centro, e il comando generato in basso. L'anteprima del comando cambia come opzioni sono abilitate. Un parametro non controllato significa normalmente “non modificare questo valore”, mentre un parametro controllato include quel valore nel comando.

I singoli dialoghi diagnostici sono descritti in [Hardware diagnostica e manutenzione](#hardware-diagnostics-and-maintenance).

## Emulazione

### Apertura di una macchina salvata

The **Emulazione ** tab liste configurazioni salvate. Selezionare uno e fare clic ** Aperto**. Ogni macchina in esecuzione appare nella sua scheda.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Schermo di benvenuto dell'emulazione" width="78%"></p>

Creare e modificare macchine in **Opzioni > Emulazione > Configurazioni ** e ** Opzioni > Emulazione Amiga**.

Se non viene visualizzata alcuna configurazione, crearne una in Opzioni prima. Una configurazione salvata combina il modello della macchina, la versione emulatrice, ROM, memoria, video, audio, storage e mappature di input. Salvataggio di una configurazione non lo avvia; torna alla pagina principale **Emulazione ** scheda e fare clic ** Aperto**.

### Comandi per macchine da corsa

<p align="center"><img src="images/main-emulation-running-en.png" alt="Macchina emulatrice in esecuzione" width="78%"></p>

La barra degli strumenti di esecuzione-macchina fornisce controlli di potenza, pausa, reset, stato di salvataggio, stato di carico, cattura e visualizzazione. Mostra anche:

- i collegamenti veloci configurati e veloci;
- il rendering attivo, come Direct3D 11;
- le scorciatoie a schermo intero e mouse;
- audio, controller e stato del mouse;
- la risoluzione attuale, la frequenza di aggiornamento e la velocità del telaio.

La striscia del disco nella parte inferiore del display di emulazione gestisce i supporti rimovibili per ogni unità emulata. Le assegnazioni della tastiera possono essere cambiate in **Opzioni > Emulazione > Scorciatoie**, mentre la tastiera emulata, il mouse e le mappe del controller sono configurate nella corrispondente Amiga Tavole.

### Riferimento della barra degli strumenti

| Gruppo di controllo | Oggetto |
|---|---|
| Potenza e pausa | Avvia, ferma, ferma o riprende la macchina emulata |
| Reset controlli | Esegue l'azione di reset morbida o dura configurata |
| Controlli statali | Salva o carica uno stato emulatore per una rapida continuazione |
| Capacità | Salva un'immagine del display emulato |
| Visualizza | Modifica la presentazione del display o entra a schermo intero |
| Promemoria a stato rapido | Mostra i collegamenti di salvataggio/carico attivi |
| Resoconto | Segnala il backend video attivo |
| Promemoria di ingresso | Mostra scorciatoie a schermo intero e mouse-release |
| Indicatori del dispositivo | Rapporti audio, controller e stato del mouse |
| Prestazioni | Segnala dimensione di uscita, frequenza di aggiornamento e frame rate |

### Lasciando lo schermo intero o rilasciando il mouse

La barra degli strumenti visualizza le chiavi attualmente assegnate. Nella configurazione illustrata, **Alt+ Ritorno ** toggles a schermo intero e ** F12** rilascia il mouse. Trattare i valori visualizzati come autorevole perché le scorciatoie possono essere riassegnate.

### Utilizzo dei supporti floppy

La striscia di unità identifica ogni unità emulata, come `DF0:`. Utilizzare i suoi controlli multimediali per inserire, sostituire o espellere un'immagine. Sostituire i media cambia solo il disco inserito della macchina in esecuzione; non cambia la definizione di dispositivo di archiviazione nella macchina salvata a meno che l'azione non venga salvata esplicitamente.

## Opzioni di applicazione

Aperto **Opzioni** dalla finestra principale per configurare l'applicazione.

### Generale

<p align="center"><img src="images/options-general-en.png" alt="Opzioni generali" width="72%"></p>

The **Generale** scheda contiene:

- la cartella predefinita di immagine disco;
- linguaggio di interfaccia e tema;
- generazione filename-tag per le conversioni;
- modelli di tag personalizzati predefiniti e recenti;
- un esempio live di nome file.

Le variabili tag includono il nome sorgente, la famiglia, il formato, l'estensione, la data e l'ora. Utilizzare il pulsante di reset per ripristinare il modello predefinito.

Il nome del file visualizza gli aggiornamenti prima che vengano creati i file. Usalo per rilevare separatori duplicati, estensioni mancanti o nomi ambigui. I modelli personalizzati recenti forniscono un rapido accesso a schemi di denominazione precedenti senza sostituire la preimpostazione corrente.

### Logs

<p align="center"><img src="images/options-logs-en.png" alt="Opzioni di log" width="72%"></p>

La registrazione può essere configurata in modo indipendente per ogni operazione. Per ogni categoria, scegliere se salvare i log, impostare una dimensione massima del file e decidere se i registri precedenti devono essere conservati. Una dimensione `0` significa illimitato. **Apri cartella** apre la directory di registro corrente.

Abilitare **Tenere i registri precedenti** per la conservazione e il lavoro diagnostico in cui la storia di diversi tentativi conta. Disattivarlo quando solo il risultato più recente è utile. I limiti di dimensione massima si applicano all'archiviazione di registro, non alle immagini di disco catturate.

### Regolatori e unità

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="Regolatori e unità" width="72%"></p>

Utilizzare questa scheda per:

- scansione per controller collegati;
- aggiungere e rimuovere le configurazioni dell'unità;
- selezionare dimensione dell'unità, densità e velocità;
- salvare le impostazioni hardware;
- scegliere o trovare automaticamente `gw.exe`;
- controllare e scaricare Greaseweazle Host Tools aggiornamenti;
- ripristinare un percorso eseguibile precedentemente configurato.

Le impostazioni hardware salvate rimangono disponibili quando un'unità è temporaneamente disconnessa.

#### Aggiungere un'unità

1. Fare clic **Scansione** e attendere che vengano visualizzati i controller collegati.
2. Fare clic **Aggiungi un'unità** se l'unità richiesta non è già elencata.
3. Selezionare il suo numero di unità logico, dimensione fisica, densità di registrazione e velocità di rotazione.
4. Salva la fila.
5. Confermare che mostra **Disponibile ** e ** Configurato**.

Utilizzare il controllo spazzatura solo per rimuovere la configurazione salvata; non disconnette l'hardware. Se lo stesso controller appare su un diverso COM porta più tardi, la scansione di nuovo prima di presumere che la porta memorizzata è ancora valida.

#### Gestione Greaseweazle Host Tools

**Trova gw.exe ** ricerche località conosciute. ** Scegli ** seleziona un eseguibile specifico. ** Controllare gli aggiornamenti ** domande disponibili versioni senza sostituire quella installata. ** Scarica la versione più recente ** installa il pacchetto corrente selezionato, e ** Utilizzare il percorso precedente ** ripristina la posizione precedentemente configurata. Dopo aver cambiato l'eseguibile, eseguire ** Informazioni sul controller** per confermare che la versione selezionata può comunicare con il controller.

### Motori

<p align="center"><img src="images/options-engines-en.png" alt="Selezione del motore" width="72%"></p>

Scegliere il motore indipendentemente per la lettura, la scrittura, la conversione e Disk Explorer. Il motore selezionato viene utilizzato rigorosamente: se non può eseguire l'operazione richiesta, GW GUI segnala la limitazione invece di commutare silenziosamente i motori.

Questa indipendenza è intenzionale. Ad esempio, le letture fisiche possono usare Greaseweazle Host Tools mentre la conversione dell'immagine e l'esplorazione utilizzano il motore interno. Scelte del motore di registrazione in un profilo o una nota di progetto quando la riproducibilità è importante.

### Profili

<p align="center"><img src="images/options-profiles-en.png" alt="Profili" width="72%"></p>

I profili memorizzano impostazioni riutilizzabili per le operazioni di lettura, scrittura e conversione. Selezionare la relativa categoria per gestire i suoi profili. Un profilo selezionato è mostrato nella barra di stato della finestra principale e nelle schermi di funzionamento.

Utilizzare profili per flussi di lavoro ripetibili piuttosto che come collezioni inspiegabili di bandiere di esperti. Dare ad ogni profilo un nome specifico per lo scopo, come un particolare disco, famiglia disco, o metodo di recupero. Verificare un profilo dopo l'aggiornamento del motore sottostante perché le opzioni supportate possono cambiare.

## Opzioni di emulazione

The **Emulazione** opzioni contengono impostazioni generali di archiviazione, scorciatoie globali, configurazioni salvate e impostazioni specifiche della macchina.

### Cartelle generali di emulazione

<p align="center"><img src="images/options-emulation-general-en.png" alt="Opzioni generali di emulazione" width="72%"></p>

Impostare la cartella di archiviazione di emulazione condivisa e le cartelle predefinite per le catture e gli stati salvati. **Apri cartella** apre la posizione condivisa in File Explorer.

Tenere le catture e gli stati salvati in cartelle separate. Una cattura è un'immagine ordinaria; uno stato salvato contiene lo stato macchina specifico dell'emulatore e può dipendere dalla versione e dalla configurazione dell'emulatore che lo ha creato. Backup della configurazione e dei media insieme a importanti stati salvati.

### Scorciatoie globali

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Scorciatoie di emulazione" width="72%"></p>

Cercare un'azione o un'assegnazione chiave, assegnare o rimuovere scorciatoie, ripristinare i default e conflitti chiari. La colonna di stato identifica incarichi validi e contrastanti.

Per cambiare una scorciatoia, trovare l'azione, fare clic **Assegnazione **, e premere la combinazione di tasti desiderata. Controlla lo stato prima di chiudere Opzioni. ** Cancella conflitti ** rimuove gli incarichi in conflitto; non ripristina la mappatura predefinita. Uso ** Ripristinare le impostazioni predefinite** quando si desidera sostituire le assegnazioni personalizzate con il set standard.

### Configurazioni salvate

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Configurazioni di emulazione salvate" width="72%"></p>

Questa pagina elenca le macchine salvate. Selezionare una configurazione per modificarla **Amiga** scheda. È possibile aggiornare l'elenco o eliminare la configurazione selezionata.

Cancellare una configurazione rimuove la definizione della macchina salvata. Non dovrebbe essere usato come un modo per espellere i media o chiudere una macchina in esecuzione. Prima della cancellazione, nota qualsiasi ROM, immagine disco rigido e file di stato associati alla configurazione.

## Amiga configurazione

L'interfaccia corrente fornisce dettagli Amiga pagine di configurazione. La stessa struttura delle impostazioni può essere estesa per altri sistemi emulati senza cambiare il flusso di lavoro principale.

### Generale

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga Impostazioni generali" width="72%"></p>

Scegli il Amiga modello, salvare la configurazione, installare o sostituire la versione emulatore e definire cartelle predefinite per dischi rigidi e altri supporti. **Versioni di ricerca** interroga la fonte ufficiale di emulatore-versione.

Iniziare con il modello perché si limita pagine successive. Cambiarlo può cambiare il disponibile CPU, memoria, ROM, chipset e opzioni di archiviazione. Dopo aver selezionato una versione emulatore, salvare la configurazione prima di lanciarla dalla finestra principale. L'installazione di un'altra versione emulatore sostituisce la versione utilizzata da tale configurazione; non crea una seconda copia della macchina.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Amiga CPU impostazioni" width="72%"></p>

The CPU pagina mostra il processore selezionato dal modello della macchina e fornisce precisione compatibile, FPU, e scelte di velocità. Le opzioni che non si applicano al modello selezionato rimangono disabilitate.

- **CPU modello modello** identifica il processore emulato.
- **Precisione** controlla il modello di tempistica. Le modalità Cycle-exact favoriscono la compatibilità hardware ma richiedono un'elaborazione più host.
- **FPU** consente un'unità a punto variabile compatibile quando supportata.
- **CPU velocità** seleziona la tempistica originale o una modalità accelerata.

Per una configurazione di base, mantenere il modello derivato CPU e velocità originale. Cambia l'accelerazione solo dopo che la macchina si avvia correttamente alle impostazioni standard.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Amiga RAM impostazioni" width="72%"></p>

Configura il chip RAM♪ RAM♪ RAM, e la memoria di espansione supportata. I messaggi di compatibilità spiegano le restrizioni per la macchina selezionata e la memoria configurata totale viene visualizzata in basso.

**Chip RAM ** è accessibile ai chip personalizzati ed è richiesto dalla piattaforma. ** Piano. RAM ** rappresenta la memoria di espansione compatibile utilizzata da configurazioni comuni. ** Veloce RAM ** è la memoria di espansione orientata al processore. ** Zorro III RAM** si applica solo ai modelli che supportano l'architettura di espansione. I messaggi di compatibilità e i controlli disabilitati impediscono combinazioni che il modello selezionato non può rappresentare.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Amiga ROM impostazioni" width="72%"></p>

Selezionare il sistema Kickstart ROM, facoltativo esteso ROMe ROM chiave. Il rilevato...ROM elenco visualizza nomi, revisioni e compatibilità con il modello selezionato. Selezionare un rilevato ROM e cliccare **Uso**, o sfogliare manualmente un file.

ROM i file non sono forniti da GW GUI. Utilizzare ROM che sono legalmente autorizzati a utilizzare.

L'elenco rilevato è preferibile indovinare da un nome di file: segnala il ROM identità e revisione e valuta la compatibilità con il modello selezionato. **Compatibile ** è la scelta normale; ** Parzialmente compatibile ** indica che ROM può avviarsi ma non corrisponde esattamente alla macchina. ** Rifiuti ** rescans il configurato ROM luoghi. ** Uso** assegna il rilevato selezionato ROM alla configurazione.

### Video

<p align="center"><img src="images/options-amiga-video-en.png" alt="Amiga Impostazioni video" width="72%"></p>

Configurare lo standard video, il rapporto di aspetto, la risoluzione, la modalità linea, il ritaglio di bordi, il rendering, la profondità di colore, il frame skipping, la gamma e il fissaggio flicker. Ulteriori impostazioni del chipset sono disponibili ulteriormente nella pagina quando supportata dal modello selezionato.

| Impostazione | Effetto pratico |
|---|---|
| Standard video | Seleziona PAL o NTSC tempistica e comportamento di aggiornamento previsto |
| Rapporto di controllo | Controlla come l'immagine emulata è scalata |
| Risoluzione | Seleziona dettagli di uscita automatici o espliciti |
| Modalità linea | Controlli il trattamento di uscita interlacciata o doppia linea |
| Confezioni di crosta | Rimuove il sovraccarico non utilizzato solo quando abilitato |
| Rendering | Scegli la grafica backend |
| Profondità del colore | Seleziona la precisione del colore di uscita |
| Scivolo telaio | Riduce i frame resi quando abilitati |
| Gamma | Regola la risposta della luminosità |
| Flicker fixer | Processi modi che altrimenti visibilmente flicker |

Cambiare un'impostazione di visualizzazione alla volta. Se la finestra di emulazione diventa vuota o instabile, tornare alla risoluzione automatica, saltare frame disabilitato, gamma neutro, e il renderer precedente.

### Audio

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Amiga impostazioni audio" width="72%"></p>

Abilitare o disabilitare l'audio, scegliere il dispositivo di uscita e la latenza, quindi configurare l'interpolazione, Amiga filtraggio, tipo di filtro, separazione stereo, suono floppy-drive e volume CD-audio.

La latenza inferiore riduce il ritardo, ma può causare drop-out su un computer occupato. Aumentare se le crepe audio. Interpolazione e Amiga il filtro audio cambia la riproduzione del suono piuttosto che la logica del programma emulata. Il volume di trasmissione controlla il suono meccanico simulato separatamente dal normale Amiga audio.

### Stoccaggio

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Amiga impostazioni di archiviazione" width="72%"></p>

La pagina di archiviazione elenca identificatori, tipi, modelli, supporti associati e azioni disponibili. Aggiungere, configurare o rimuovere i dispositivi qui. I dischi e i CD Floppy possono essere inseriti o sostituiti direttamente da una macchina in esecuzione.

The **identificativo del dispositivo ** è come il sistema emulato indirizza il dispositivo. ** Tipo ** distingue floppy, hard-disk, ottica e altri dispositivi supportati. ** Modello ** descrive l'hardware emulato, mentre ** Media associati** identifica l'immagine attualmente assegnata. Configurare il dispositivo prima di associare preziosi supporti scrivibili e mantenere i backup delle immagini hard-disk.

### Tastiera

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Amiga impostazioni della tastiera" width="72%"></p>

Ricerca Amiga chiavi e incarichi host, assegnare nuove chiavi, rimuovere mappature, ripristinare i default, o conflitti chiari. La colonna di stato riporta se ogni assegnazione è valida.

La colonna sinistra nomina l'emulazione Amiga chiave; **Associazione** mostra la combinazione chiave host. Una mappatura valida può ancora essere scomoda se Windows o l'applicazione si riserva la stessa scorciatoia, quindi testa le combinazioni critiche all'interno della macchina in esecuzione. Evitare di assegnare il mouse-release o il collegamento a schermo intero a una chiave che il software emulato ha bisogno frequentemente.

### Mouse

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Amiga impostazioni del mouse" width="72%"></p>

Impostare la velocità fisica del mouse, scegliere quale bastone analogico controlla il mouse, regolare la zona morta analogica e la velocità, e configurare mappature di azione del mouse. Ripristinare le impostazioni predefinite o eliminare i conflitti di mappatura quando necessario.

Aumentare la zona morta se un controller causa la deriva del puntatore. Regolare la velocità sinistra e destra indipendentemente quando entrambi i bastoni sono abilitati. La tabella di mappatura inferiore associa input host con pulsanti del mouse o azioni; ispezionare lo stato del conflitto dopo aver cambiato mappature del controller altrove.

### Regolatori

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Amiga impostazioni del controller" width="72%"></p>

Rileva i controller collegati, assegna i dispositivi e i tipi di controller Amiga porte e configurare mappature del controller e impostazioni turbo-fuoco. Le scelte disponibili dipendono dall'hardware rilevato e dalla macchina selezionata.

Port 1 e Port 2 sono configurati in modo indipendente. **Automatico** il tipo di controller è un punto di partenza ragionevole, ma il software che aspetta un particolare joystick o il mouse può richiedere un tipo esplicito. Eseguire il rilevamento prima di assegnare un controller appena connesso. Turbo fuoco attiva ripetutamente un ingresso mappato e dovrebbe rimanere disabilitato a meno che il gioco o l'applicazione non benefici da esso.

## Diagnostica e manutenzione hardware

Queste finestre di dialogo sono aperte dal **Strumenti ** scheda. Ogni finestra di dialogo prevede il generato Greaseweazle comando. Scrivilo prima di cliccare ** Esecuzione**.

### Informazioni sul controller

<p align="center"><img src="images/tool-controller-information-en.png" alt="Informazioni sul controller" width="62%"></p>

Visualizza le informazioni riportate dal controller selezionato. Espansione **Uscita cruda** quando hai bisogno della risposta di comando completa.

Usa questo come primo comando diagnostico. Una risposta di successo conferma che GW GUI può avviare l'eseguibile Strumenti Host configurati e comunicare con il dispositivo selezionato. Registra le informazioni firmware e hardware prima di eseguire un aggiornamento.

### USB larghezza di banda

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="USB larghezza di banda" width="62%"></p>

Misure disponibili USB comunicazione larghezza di banda. Utilizzare per diagnosticare trasferimenti instabili o un inadatto USB connessione.

Chiudere altri software utilizzando il controller prima di testare. Ripetere la misurazione dopo aver cambiato il USB porta, cavo o hub. Confronta i risultati in condizioni simili piuttosto che trattare una singola misura come garanzia assoluta.

### Velocità di trasmissione

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Velocità di trasmissione" width="62%"></p>

Misura la velocità di rotazione dell'azionamento. Aumentare il numero di misurazioni quando è necessario un risultato più rappresentativo.

Una singola misura è un controllo rapido; diverse misure rivelano se la velocità è stabile. Lasciare che l'unità raggiunga la velocità normale prima di interpretare il risultato. Un valore inaspettato può indicare una velocità configurata sbagliata, un problema meccanico o un problema di configurazione di misura.

### Testa di tenuta

<p align="center"><img src="images/tool-seek-head-en.png" alt="Testa di tenuta" width="62%"></p>

Sposta la testa di azionamento in un cilindro selezionato. **Consentire cilindri estremi ** consente posizioni normalmente limitate, e ** Tenere attivo il motore** lascia il motore acceso durante l'operazione. Utilizzare posizioni estreme solo quando la procedura hardware richiede esplicitamente loro.

La ricerca normale è utile per confermare il movimento della testa o il posizionamento prima di una diagnostica. Ascoltare gli effetti ripetuti anormali e fermarsi se il cilindro richiesto è inappropriato per l'unità. Questo strumento non legge o convalida i dati al cilindro di destinazione.

### Diagnostica dell'allineamento dell'unità

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Diagnostica dell'allineamento dell'unità" width="62%"></p>

Esegue le letture ripetute per l'analisi di allineamento del disco. Supporta la selezione delle tracce, la rivoluzione e i conteggi di lettura, il formato di decodifica, il flusso grezzo, l'indice, la velocità, PLL, densità-pin, hard-sector, TG43, e opzioni di dati inversa. Il lavoro di allineamento richiede una corretta conoscenza dei supporti di riferimento e dell'hardware.

Iniziare con un disco di riferimento noto e il più piccolo insieme di overrides. **Tracce alternative ** definisce le tracce e le teste campionate; ** Rivoluzioni per traccia ** controlla ogni durata del campione; ** Numero di letture** determina la ripetizione. Abilitare una definizione del disco personalizzato o il formato di decodifica solo quando corrisponde al supporto di riferimento. Opzioni come indice falso, settori duri, PLL overrides, perni di densità, e TG43 sono hardware- o formato-specifico e può invalidare un confronto quando utilizzato in modo errato.

### Perni hardware

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="Perni hardware" width="62%"></p>

Legge o modifica un pin del controller supportato. Selezionare il perno, abilitare **Perno di cambiamento ** solo quando si scrive un valore e selezionare ** Alto livello** quando richiesto dal funzionamento hardware previsto.

Con **Perno di cambiamento** disabilitato, il comando interroga il perno. Questo è il default più sicuro. Cambiare un livello influisce direttamente sul controller I/O e dovrebbe essere fatto solo con il corretto Greaseweazle documentazione hardware e cablaggio annesso.

### Reset controller

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Reset controller" width="62%"></p>

Reimposta il Greaseweazle Controllore. Utilizzare questo quando il controller viene rilevato ma non risponde più normalmente.

Attendere che qualsiasi operazione del disco attivo finisca prima di resettare. Successivamente, eseguire la scansione del controller di nuovo se lo stato di connessione non viene recuperato automaticamente. Un reset non ripara un errore `gw.exe` percorso o disconnesso USB dispositivo.

### Ritardi

<p align="center"><img src="images/tool-delays-en.png" alt="Ritardo del controller" width="62%"></p>

Legge o cambia i valori di temporizzazione del controller, tra cui selezione, passo della testa, sistema, motore, deselezionamenti automatici, tempi di scrittura e indice di ritardi della maschera. Abilitare solo i valori che si intende modificare.

I campi non controllati lasciano invariato il valore del controller corrispondente. Prima di modificare, registrare i valori esistenti. I cambiamenti di temporizzazione possono influenzare ogni successiva operazione fisica, quindi testare con mezzi espandibili e ripristinare i valori noti-buoni se il comportamento diventa inaffidabile.

### Firmware

<p align="center"><img src="images/tool-firmware-en.png" alt="Aggiornamento firmware" width="62%"></p>

Aggiorna il firmware del controller. **Aggiorna il bootloader** è esplicitamente contrassegnato come rischioso e dovrebbe rimanere disabilitato a meno che la procedura ufficiale del firmware non lo richieda. Non scollegare il controller durante un aggiornamento.

Prima di aggiornare, confermare il controller connesso con **Informazioni sul controller**, utilizzare una stabile diretta USB connessione, e chiudere altri software che potrebbero accedervi. Dopo il completamento, riconnettere o ripristinare il controller e leggere nuovamente le sue informazioni per verificare la versione del firmware riportata.

## Log e cronologia delle operazioni

Aprire la cronologia delle operazioni per ispezionare i registri salvati per operazione.

<p align="center"><img src="images/operation-history-en.png" alt="Storia dell'operazione" width="68%"></p>

Selezionare un log a sinistra per visualizzare il suo contenuto. **Esportazione** salva una copia per la diagnostica o il supporto. Percorsi e linee di comando possono contenere nomi di cartelle personali, quindi rivedere i registri esportati prima di condividerli.

La console live nella finestra principale mostra il comando corrente e l'output recente. Il suo pulsante copia copia il testo visualizzato.

### Leggere un registro

Un utile registro diagnostico contiene il comando generato, timestamp, uscita del motore e lo stato finale. Lavorare dal basso verso l'alto: identificare l'errore finale, quindi individuare il primo avvertimento o traccia fallita che l'ha preceduto. Un successivo fallimento generico è spesso solo la conseguenza di un messaggio precedente e più specifico.

Quando si confrontano due tentativi, verificare che il controller, l'unità, il motore, il profilo, il percorso sorgente, il formato di output e gli argomenti esperti erano identici. In caso contrario, un risultato diverso può riflettere le impostazioni modificate piuttosto che l'instabilità del disco.

## Dati di applicazione e uso portatile

GW GUI mantiene i dati degli utenti separati dai binari delle applicazioni. A seconda del pacchetto e della modalità selezionata, le impostazioni, i registri, gli strumenti scaricati, i componenti dell'emulatore, le catture, gli stati e le configurazioni della macchina vengono memorizzate nell'applicazione `Data` directory o nelle posizioni dei dati utente configurati.

Prima di sostituire o spostare un'installazione portatile, mantenere la cartella completa dell'applicazione insieme e eseguire il backup `Data` cartella. Non spostare i file individuali da `lib`, perché l'applicazione risolve le proprie librerie e terze parti da quella struttura.

### Contenuto di backup consigliato

Torna indietro quando sono importanti per il flusso di lavoro:

- impostazioni di applicazione e profili;
- definizione del controller e dell'unità;
- configurazioni di emulazione;
- ROM percorsi e legalmente tenuti ROM backup;
- immagini hard-disk e sfoderabili;
- cattura e salva gli stati;
- registri di funzionamento utilizzati come record di conservazione.

Le immagini del disco possono essere molto più grandi delle impostazioni. Conservare i master archivistici solo quando possibile e lavorare su copie.

## Flussi di lavoro raccomandati

### Archiviazione di un disco sconosciuto

1. Ispezionare e pulire l'unità utilizzando una procedura di manutenzione appropriata.
2. Se possibile, proteggere il disco.
3. Seleziona **Leggere > Immagine grezzaSCP)**.
4. Utilizzare un nome di file descrittivo e leggere il range di traccia normale con rivoluzioni multiple.
5. Rivedere la console e il registro salvato.
6. Ispezione di entrambi i lati **Visualizzazione**.
7. Convertire una copia in formati settoriali probabili.
8. Prova le copie convertite in **Disk Explorer** o software adatto.
9. Conservare il master, il log e le note crude insieme.

### Ricreare un disco da un'immagine

1. Ispezionare l'immagine e confermare la sua famiglia e formato previsto.
2. Inserire un disco passibile o volutamente scrivibile della dimensione e della densità corrette.
3. Aperto **Scrivere** e selezionare l'immagine.
4. Confermare l'unità configurata e il formato rilevato.
5. Scrivi il disco.
6. Leggilo in un'immagine di verifica separata.
7. Confronta contenuti decod e controlla tracce sospette visivamente.

### Creare un emulato Amiga

1. Aperto **Opzioni > Emulazione > Configurazioni** e creare o selezionare una macchina.
2. In **Amiga > Generale**, scegliere il modello e la versione emulatore.
3. Assegnare un compatibile, legalmente ottenuto ROM.
4. Tenere il modello predefinito per CPU e RAM sul primo stivale.
5. Configurare video e audio con impostazioni automatiche conservatrici.
6. Aggiungi dispositivi di archiviazione e associa immagini multimediali copiate.
7. Rivedere le assegnazioni di tastiera, mouse e controller.
8. Salvare la configurazione.
9. Torna a **Emulazione **, selezionare e fare clic ** Aperto**.
10. Solo dopo un avvio di base di successo, cambiare l'accelerazione o le impostazioni avanzate una alla volta.

## Controllo di sicurezza

Prima **Leggi**:

- il disco sorgente è nell'unità corretta;
- la fonte è protetta da scrittura ove possibile;
- il percorso di uscita non sovrascriverà un master esistente;
- il profilo e l'intervallo di traccia corrispondono al disco.

Prima **Scrivere ** o ** Cancellazione**:

- il disco di destinazione può essere distrutto;
- l'immagine e l'unità sono corrette;
- dimensione del disco e densità sono compatibili;
- nessun master archivistico viene utilizzato come destinazione.

Prima di uno strumento di cambio hardware:

- nessun'altra operazione è in esecuzione;
- il corretto controller viene selezionato;
- sono stati registrati valori attuali;
- il controllore ha potenza stabile e USB connettività;
- l'azione è supportata dalla documentazione hardware.

## Risoluzione dei problemi

### Il controller non è elencato

1. Ricollegare il controller direttamente al computer.
2. Aperto **Opzioni > Regolatori e unità**.
3. Fare clic **Scansione**.
4. Verifica lo stato del controller e la configurazione dell'unità.
5. Corri! **Informazioni sul controller** se il rilevamento riesce, ma i comandi falliscono.

Se ancora non appare, prova un altro diretto USB porta e cavo, poi rescan. Controlla Windows Device Manager per un dispositivo seriale appena rilevato. Un controller visibile a Windows ma assente da GW GUI di solito punti a una porta impegnata, configurazione stale, o problemi Strumenti host; un controller assente da punti di Windows a USB, potenza, driver o hardware.

### `gw.exe` non può essere trovato

Aperto **Opzioni > Regolatori e unità **, poi usare ** Trova gw.exe **, ** Scegli **o ** Scarica la versione più recente**. Confermare che il percorso rilevato indica il percorso previsto Greaseweazle installazione.

Dopo aver selezionato, eseguire **Informazioni sul controller**. Se ciò non funziona prima di contattare l'hardware, ispezionare il registro per un percorso eseguibile non valido, file mancanti, o una versione che non può iniziare.

### Un'operazione utilizza il motore sbagliato

Aperto **Opzioni > Motori** e controllare il motore assegnato a quella operazione esatta. GW GUI non rientra silenziosamente all'altro motore.

Le impostazioni del motore sono separate: cambiare il motore di conversione non cambia lettura, scrittura o Disk Explorer. Riaprire l'operazione di fallimento dopo aver salvato l'opzione e confermare il comando generato nella console.

### Un'immagine non è riconosciuta

Disattivare il rilevamento automatico solo se si conosce la macchina corretta e il formato. Altrimenti, prova il **Visualizzazione** scheda per ispezionare l'immagine ad un livello inferiore.

Controllare se la fonte è una cattura del flusso grezzo, un'immagine del settore, un contenitore compresso, o un file non correlato con un'estensione fuorviante. Non rinominare mai un'estensione solo per forzare il rilevamento; la conversione deve interpretare correttamente la struttura sorgente.

### L'emulazione non inizia

Verificare la configurazione salvata, la versione installata dell'emulatore, selezionata ROM, percorsi di archiviazione e compatibilità del modello. Verificare il log dell'applicazione per i dettagli di errore completi.

Ritorno temporaneo CPU, RAM, video e storage a un semplice modello compatibile linea di base. Se la linea di base inizia, ripristinare una impostazione personalizzata alla volta. Uno stato salvato creato con un'altra versione emulatore o la definizione della macchina può anche fallire anche quando un boot pulito funziona.

### Una scorciatoia o un input non funziona

Controllare sia il globale **Emulazione > Scorciatoie** pagina e la pagina della tastiera, del mouse o del controller specifico della macchina. Risolvere qualsiasi incarico segnato come conflitto.

Se il mouse viene catturato, utilizzare la scorciatoia di rilascio visualizzata nella barra degli strumenti di esecuzione. Se un controller è stato collegato dopo l'apertura delle Opzioni, eseguire nuovamente il rilevamento del controller prima di assegnarlo.

### Un comando fallisce inaspettatamente

1. Leggi l'output della console live.
2. Aperto **Storia dell'operazione** per il registro completo salvato.
3. Confermare i percorsi di controllo, unità, profilo, motore e file selezionati.
4. Esportare il registro relativo se deve essere condiviso per la diagnosi.

### Cracker audio o pause

Aumentare la latenza audio di emulazione, chiudere CPU- applicazioni intensive, e ritorno video frame skipping e accelerazione ai loro valori precedenti. Verificare che il dispositivo audio Windows previsto sia selezionato. Cambiare una impostazione alla volta in modo che la correzione efficace è identificabile.

### Il display di emulazione è vuoto o lento

Risoluzione di ritorno e modalità di riga per **Automatico**, disabilitare il montaggio del telaio e del flicker temporaneamente, e provare il renderer precedentemente funzionante. Confermare che la configurazione ROM e i supporti di avvio inseriti sono validi. The FPS indicatore aiuta a distinguere un problema di rendering-performance da una macchina che semplicemente non ha avviato.

### Una lettura contiene tracce instabili

Ripetere la lettura di un nuovo nome di file, aumentare le rivoluzioni se del caso, e confrontare le tracce interessate. Pulire le teste dell'unità utilizzando una procedura corretta e ispezionare il disco per danni fisici. Non leggere più ripetutamente spargimento visibilmente o media danneggiati, perché ulteriori passaggi possono peggiorare.

## Glossario

| Termine | Significato in GW GUI |
|---|---|
| Controller | The Greaseweazle interfaccia hardware collegata USB |
| Drive | L'unità floppy fisica collegata al controller |
| Motore | L'implementazione selezionata per eseguire un'operazione |
| Flusso | Informazioni di sincronizzazione che rappresentano transizioni magnetiche leggere da un disco |
| Immagine cruda | Una cattura che conserva informazioni su disco di basso livello, come SCP |
| Immagine del settore | Una rappresentanza decodifica organizzata in settori logici |
| Rivoluzione | Una rotazione completa campione durante la lettura di una traccia |
| Cilindro | Una posizione della testa radiale; un cilindro può contenere una traccia su ogni lato |
| Capo | Il lato del disco selezionato dall'unità fisica |
| Profilo | Un set riutilizzabile di impostazioni per un'operazione |
| ROM | Immagine firmware richiesta da una macchina emulata |
| Stato salvato | Un’istantanea dello stato macchina dell’emulatore in esecuzione |
| Resoconto | Il backend grafico utilizzato per visualizzare l'output di emulazione |

## Riferimento rapido

| Se vuoi... | Vai a... |
|---|---|
| Conservare un disco fisico | **Leggi** |
| Rimetti un'immagine su un disco | **Scrivere** |
| Produci un altro formato immagine | **Conversione** |
| Ispezionare tracce o anomalie del flusso | **Visualizzazione** |
| Sfoglia i file all'interno di un'immagine | **Disk Explorer** |
| Controllare la comunicazione del controller | **Strumenti > Informazioni sul controller** |
| Misurare la rotazione dell'unità | **Strumenti > Velocità di trasmissione** |
| Recensione di un comando passato | **Storia dell'operazione** |
| Configurare l'hardware | **Opzioni > Regolatori e unità** |
| Selezionare le implementazioni | **Opzioni > Motori** |
| Creare o modificare una macchina emulata | **Opzioni > Emulazione** |
| Avviare una macchina salvata | **Emulazione** |
