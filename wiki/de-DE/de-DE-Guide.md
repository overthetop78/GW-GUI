[🌐 Languages / Langues](../Home.md)

# GW GUI Benutzerhandbuch

GW GUI ist eine Windows-Anwendung zum Lesen, Schreiben, Konvertieren, Prüfen und Emulieren von Floppy-Disk-Images. Es kann kontrollieren Greaseweazle Hardware, arbeiten mit Festplattenbilddateien über die interne Engine und führen gespeicherte Emuled-Machine-Konfigurationen aus.

Dieses Handbuch beschreibt die englische Benutzeroberfläche, die in der aktuellen Version der Anwendung gezeigt wird. Es ist als Quelle des druckbaren Benutzerhandbuchs geschrieben: Screenshots veranschaulichen die Steuerelemente, während der umgebende Text erklärt, was zu wählen ist, warum es ausgewählt wird und wie das Ergebnis überprüft werden kann.

> **Wichtig:** Das Lesen einer Festplatte ist zerstörungsfrei. Schreiben, Löschen, Firmware-Aktualisierung und einige Hardware-Tools können Medien oder Hardware ändern. Lesen Sie den Warnhinweis, der dem entsprechenden Verfahren beigefügt ist, bevor Sie klicken ** Ausführung**.

### Wie man dieses Handbuch benutzt

Wenn dies Ihre erste Verwendung ist GW GUI, abgeschlossen [Anfang]](#getting-started), dann folgen [Lesen einer Festplatte]](#reading-a-disk)Wenn die Anwendung bereits konfiguriert ist, gehen Sie direkt zum Kapitel für die Operation, die Sie ausführen möchten. Die Optionskapitel dienen als Referenz, wenn ein Verfahren Sie auffordert, eine Einstellung für Antrieb, Motor, Profil oder emulierte Maschine zu ändern.

Interface-Namen werden in **fett** Dateinamen, Pfade, Befehle und Literalwerte werden als `code`Anmerkungen erläutern das normale Verhalten; Warnungen identifizieren Vorgänge, die eine Festplatte, einen Controller oder eine gespeicherte Konfiguration verändern können.

## Inhalt

1. [den Workflow verstehen]](#understanding-the-workflow)
2. [Anfang]](#getting-started)
3. [Hauptfenster]](#main-window)
4. [Lesen einer Festplatte]](#reading-a-disk)
5. [Schreiben einer Festplatte]](#writing-a-disk)
6. [Disk Images konvertieren]](#converting-disk-images)
7. [Visualisierung eines Disk Images]](#visualizing-a-disk-image)
8. [Datenträgerinhalte erkunden]](#exploring-disk-contents)
9. [Verwendung der Werkzeuge]](#using-the-tools)
10. [Emulation]](#emulation)
11. [Anwendungsoptionen]](#application-options)
12. [Emulationsoptionen]](#emulation-options)
13. [Amiga Konfiguration](#amiga-configuration)
14. [Hardware-Diagnose und Wartung]](#hardware-diagnostics-and-maintenance)
15. [Logs und Betriebshistorie]](#logs-and-operation-history)
16. [Anwendungsdaten und tragbare Nutzung]](#application-data-and-portable-use)
17. [Empfohlene Workflows]](#recommended-workflows)
18. [Sicherheitscheckliste]](#safety-checklist)
19. [Troubleshooting]](#troubleshooting)
20. [Glossar]](#glossary)
21. [Schnelle Referenz]](#quick-reference)

## Den Workflow verstehen

GW GUI trennt physikalische Festplattenoperationen von Bilddateioperationen:

| Ziel | Input | Output | Empfohlene Seite |
|---|---|---|---|
| Bewahren Sie eine Diskette | Physische Scheibe | Bilddatei | **Lesen** |
| Erstellen einer Diskette | Bilddatei | Physische Scheibe | **Schreiben** |
| Bildformat ändern | Bilddatei | Eine oder mehrere Bilddateien | **Umwandlung** |
| Spuren und Anomalien untersuchen | Bilddatei | Visuelle Analyse | **Visualisierung** |
| Durchsuchen von Dateien, die in einem Bild gespeichert sind | Unterstütztes Bild/Dateisystem | Dateien und Verzeichnisse | **Disk Explorer** |
| Diagnose eines Laufwerks oder Controllers | Greaseweazle Hardware | Messungen oder Status | **Werkzeuge** |
| Führen Sie eine gespeicherte virtuelle Maschine aus | Gespeicherte Maschinenkonfiguration | Emulationssitzung | **Emulation** |

Zur Erhaltung, machen Sie zuerst eine rohe Erfassung und halten Sie es unverändert als Meister. Erstellen Sie konvertierte oder reparierte Arbeitskopien von diesem Master. Dies vermeidet die Wiederholung eines physischen Lesens und bewahrt Informationen, die ein sektorbasiertes Format möglicherweise nicht behält.

## Beginnen Sie

### Anforderungen

- Windows mit dem Microsoft .NET Desktop-Laufzeit, die von der Anwendung benötigt wird.
- A Greaseweazle Steuerung für physikalische Diskettenoperationen.
- Ein konfigurierter Pfad zum `gw.exe` Bei der Verwendung der Greaseweazle Host Tools Motor.
- Legal erworben ROM Dateien, wenn eine emulierte Maschine sie benötigt.

Die Anwendung überprüft die erforderliche .NET-Laufzeit beim Start. Wenn es fehlt, folgen Sie der Installationsaufforderung und starten Sie dann neu GW GUI.

### Vor dem Verbinden von Hardware

Überprüfen Sie Folgendes, bevor Sie eine physikalische Festplattenoperation ausführen:

1. Verbinden Sie Greaseweazle Controller zu einem stabilen USB Hafen.
2. Verbinden Sie das Floppy-Kabel mit der richtigen Ausrichtung.
3. Schließen Sie das Antriebsnetzteil an, bevor Sie wertvolle Medien einfügen.
4. Bestätigen Sie, dass die Laufwerksgröße und -dichte mit der Festplatte übereinstimmen.
5. Schreibschutz der Quelldiskette, wenn möglich.

GW GUI Schäden durch falsche Verkabelung, ungeeignete Leistung oder einen mechanisch unsicheren Antrieb nicht verhindern können. Testen Sie zunächst unbekannte Hardware mit einer entbehrlichen Festplatte.

### Erster Start

1. Offen `gwgui.exe`.
2. Offen **Optionen**.
3. In **Steuerungen und Antriebe** Scannen Sie nach dem Controller und konfigurieren Sie den Antrieb.
4. Überprüfen oder wählen Sie den Pfad zum `gw.exe`.
5. In **Motoren** Wählen Sie, welcher Motor jede Operation durchführen soll.
6. Kehren Sie zum Hauptfenster zurück und wählen Sie die erforderliche Registerkarte aus.

### Bestätigung, dass das Setup fertig ist

Ein Arbeitsaufbau sollte den Controller und das Laufwerk in der Statusleiste anzeigen, beispielsweise eine Laufwerksnummer, Größe, Dichte und COM Hafen. In **Optionen > Steuerungen und Antriebe ** Der Controller sollte markiert werden **verfügbar ** und der Antrieb ** Konfiguriert **. Lauf ** Angaben zum Verantwortlichen** bevor Sie wertvolle Medien lesen, wenn Sie die Kommunikation überprüfen möchten, ohne eine Festplatte zu ändern.

### Auswahl eines Motors

GW GUI kann mehr als eine Implementierung für einige Operationen aussetzen. Die **Greaseweazle Host Tools** Engine ruft die konfigurierte `gw.exe`; der interne GW GUI Motor handhabt unterstützte Operationen innerhalb der Anwendung. Die Motorauswahl ist explizit und unabhängig für Lesen, Schreiben, Konvertieren und Disk Explorer. Wenn ein Vorgang von dem ausgewählten Motor nicht unterstützt wird, GW GUI meldet diesen Zustand, anstatt die Motoren automatisch zu ändern.

## Hauptfenster

Das Hauptfenster gruppiert die Hauptoperationen in sieben Registerkarten:

- **Lesen** Erzeugt ein Bild von einer physischen Festplatte.
- **Schreiben** Schreibt ein Bild auf eine physische Festplatte.
- **Umwandlung** wandelt ein Plattenbildformat in ein oder mehrere Ausgabeformate um.
- **Visualisierung** Zeigt Spuren und Fluss- oder dekodierte Daten an.
- **Disk Explorer** Browses unterstützte Dateisysteme und Festplatteninhalte.
- **Werkzeuge** Bereitstellung von Hardware-Wartungs- und Diagnosebefehlen.
- **Emulation** verwaltet und führt gespeicherte emulierte Maschinen aus.

Die Konsole unten zeigt den ausgeführten Befehl und seine Ausgabe an. Die Statusleiste meldet das ausgewählte Laufwerk, Profil und den aktuellen Zustand.

### Lesen der Schnittstelle

Die meisten Operationsseiten folgen dem gleichen Muster:

1. **Quelle oder Bestimmungsort** Steuerelemente identifizieren die Festplatte, das Bild oder den Ordner.
2. **Formatkontrollen** Wählen Sie eine automatische Erkennung oder eine explizite Maschine und ein explizites Format.
3. **Profilkontrollen** Wiederverwendbare Einstellungen anwenden.
4. **Erweiterte Einstellungen** Parameter freilegen, die normalerweise optional sind.
5. **Ausführung** Beginn der Operation.
6. Die **Konsole** zeigt den generierten Befehl, Fortschritt, Warnungen und Fehler.

Die **Ausführung** Eine Schaltfläche bedeutet nicht, dass alle Werte für die eingefügte Festplatte sicher sind. Überprüfen Sie immer das Ziel und das ausgewählte Laufwerk vor einem Schreib- oder Wartungsvorgang.

### Statusleiste und Konsole

Die linke Seite der Statusleiste identifiziert das aktive physische Laufwerk. Das Zentrum zeigt das aktive Profil an, wenn eines ausgewählt wird. Der Zustandsindikator gibt an, ob die Anwendung fertig oder beschäftigt ist. Die Konsole ist nicht nur Diagnose: Es ist die autoritative Aufzeichnung des Befehls, der an die ausgewählte Engine gesendet wird. Verwenden Sie das Kopiersteuerelement, wenn Sie diesen Befehl beibehalten oder teilen müssen.

## Lesen einer Festplatte

Offen **Lesen** Tab, um eine physische Diskette als Bild aufzunehmen.

<p align="center"><img src="../images/main-read-en.png" alt="Lesetipp" width="78%"></p>

### Basisverfahren

1. Legen Sie die Quellenscheibe in das konfigurierte Laufwerk ein.
2. Wählen Sie den Bildtyp:
   - **Rohbild (SCP)** erhält Informationen auf Fluss-Ebene.
   - **Bekanntes Plattenformat** erstellt ein Bild mit einer ausgewählten Maschine und einem ausgewählten Format.
3. Wählen Sie den Zielordner.
4. Geben Sie den Output-Dateinamen ein.
5. Wählen Sie bei Bedarf ein Profil aus.
6. Klicken **Ausführung**.

Die Konsole zeigt den genauen Befehl und Fortschritt an. Entfernen Sie die Festplatte nicht oder trennen Sie den Controller nicht, bis der Vorgang abgeschlossen ist.

### Auswahl des Ausgabetyps

Verwenden Sie **Rohbild (SCP)**, wenn das Ziel die Archivierung, Analyse, Wiederherstellung oder spätere Konvertierung ist. Ein Rohbild zeichnet Timing-Informationen und mehrere Umdrehungen auf, was für ungewöhnliche Formate, schwache Sektoren, Schutzsysteme und beschädigte Medien nützlich ist.

Verwendung **Bekanntes Plattenformat** wenn Sie die Festplattenfamilie bereits kennen und ein direkt verwendbares Sektorbild benötigen. Diese Auswahl kann in anderer Software kleiner und einfacher zu öffnen sein, aber sie stellt das dekodierte Ergebnis dar und nicht jedes Detail, das vom Laufwerk beobachtet wird.

Wenn unsicher, erstellen Sie zuerst das Rohbild. Sie können es später konvertieren, ohne die Festplatte erneut zu lesen.

### Ordner, Dateiname und Profil

Die **Ordner ** ist das Zielverzeichnis. Die ** Dateiname** sollte die Festplatte identifizieren, ohne sich nur auf ihr physisches Label zu verlassen. Ein nützlicher Archivname enthält den Titel, die Festplattennummer oder die Seite und gegebenenfalls eine Bedingungsnotiz. Fügen Sie keine Formaterweiterung hinzu, die mit dem ausgewählten Ausgabeformat kollidiert.

A **Profil ** wendet einen gespeicherten Satz von Leseparametern an. Wählen Sie eine nur, wenn Sie wissen, was es enthält. Die ** Ausfall** Ein Profil eignet sich für einen normalen ersten Versuch; ein spezialisiertes Bergungsprofil kann bewusst mehr Umdrehungen oder eine andere Spurweite lesen und daher länger dauern.

### Erweiterte Einstellungen

Expansion **Erweiterte Einstellungen** Zugriff auf formatspezifische oder Expertenparameter. Lassen Sie diese Werte unverändert, es sei denn, die Festplatte benötigt eine bestimmte Spurweite, Umdrehungszahl oder Controller-Option.

Gemeinsame fortgeschrittene Werte umfassen:

| Einstellung | Zweck | Wann man es ändern sollte |
|---|---|---|
| Spurweite | Begrenzt die Zylinder und Köpfe zu lesen | Einseitige Medien, ungewöhnliche Geometrie oder ein gezielter Recovery Pass |
| Revolutionen | Steuert, wie viele Umdrehungen beprobt werden | Erhöhung bei instabilen oder geschützten Gleisen; Verringerung nur für Geschwindigkeit, wenn angemessen |
| Sachverständigengutachten | Übergibt zusätzliche Motorparameter | Nur im Folgenden dokumentiert Greaseweazle Führung |

### Verifizieren eines erfolgreichen Lesens

Verlassen Sie sich nicht nur auf das Fehlen eines Fehlerdialogs. Nachdem der Befehl abgeschlossen ist:

1. Bestätigen Sie, dass die Ausgabedatei existiert und nicht leer ist.
2. Lesen Sie die letzten Konsolenzeilen für fehlgeschlagene oder fehlende Tracks.
3. Öffnen Sie das Bild in **Visualisierung** zu prüfen, ob beide Seiten und der erwartete Gleisbereich Daten enthalten.
4. Öffnen Sie es in **Disk Explorer** wenn das Dateisystem unterstützt wird.
5. Führen Sie das Operationsprotokoll mit wichtigen Archivaufnahmen.

Wenn sich wiederholte Lesevorgänge unterscheiden, bewahren Sie jede rohe Erfassung auf, anstatt die erste zu überschreiben. Unterschiede können während der Erholung nützlich sein.

## Schreiben einer Diskette

Offen **Schreiben** Tab, um ein vorhandenes Bild auf eine physische Diskette zu schreiben.

<p align="center"><img src="../images/main-write-en.png" alt="Schreibe Tab" width="78%"></p>

### Basisverfahren

1. Legen Sie die Zielscheibe ein.
2. Wählen Sie das Quellbild mit **Browsen**.
3. Bestätigen Sie das erkannte Format.
4. Wählen Sie bei Bedarf ein Profil aus.
5. Klicken **Ausführung**.

Schreiben ersetzt Daten auf der Zieldiskette. Überprüfen Sie das ausgewählte Laufwerk und Bild vor dem Start.

> **Warnung:** Schreiben ist destruktiv. Es ersetzt magnetische Daten auf der Zielscheibe. Verwenden Sie nach Möglichkeit ein schreibgeschütztes Quellarchiv und eine separate Zieldiskette.

### Vor dem Schreiben

Überprüfen Sie vier Elemente, bevor Sie klicken **Ausführung**:

1. **Bild:** der gewählte Pfad ist das beabsichtigte Quellbild.
2. **Scheibe:** Die Festplatte im Laufwerk kann sicher überschrieben werden.
3. **Antrieb:** die konfigurierte Größe und Dichte passen zum Zielmedium.
4. **Format:** automatische Erkennung oder das manuell ausgewählte Format stimmt mit dem Bild überein.

Wenn das Quellbild nicht getestet wurde, öffnen Sie es in **Visualisierung ** oder ** Disk Explorer** zuerst. Ein erfolgreiches Schreiben kann ein unvollständiges Quellbild nicht reparieren.

### Gleisinspektion und -änderung

Nachdem ein Bild ausgewählt wurde, **Visualisieren von Tracks ** öffnet seine Gleisdarstellung. ** Änderung** zeigt die unterstützten Bildänderungen vor dem Schreiben. Verfügbare Aktionen hängen vom gewählten Format und der Engine ab.

### Überprüfen einer geschriebenen Festplatte

Wenn die Engine die Verifizierung unterstützt, verwenden Sie sie für wichtige Medien. Andernfalls lesen Sie die geschriebene Festplatte zurück zu einem neuen Bild und vergleichen Sie ihre dekodierten Inhalte oder inspizieren Sie sie in **Visualisierung** Halten Sie die Überprüfungsaufnahme vom Originalbild getrennt, damit das Original nie überschrieben wird.

Wenn das Schreiben bei konsistenten Spuren fehlschlägt, überprüfen Sie den Zustand der Festplatte, die Dichte, die Sauberkeit des Laufwerks und die Laufwerkskonfiguration. Wenn Fehler zufällig auftreten, überprüfen USB Stabilität und Controller-Kommunikation.

## Konvertieren von Disk Images

Die **Umwandlung** Tab konvertiert ein Quellbild in ein oder mehrere Zielformate.

<p align="center"><img src="../images/main-conversion-en.png" alt="Umrechnungstabelle" width="78%"></p>

### Basisverfahren

1. Wählen Sie das Quellbild aus.
2. Optional Ausgabenamen angeben.
3. Wählen Sie eine Maschinenfamilie.
4. Wählen Sie ein oder mehrere Ausgabeformate und Erweiterungen aus.
5. Ermöglicht **Tags hinzufügen** Wenn Dateinamen das konfigurierte Tagmuster verwenden sollen.
6. Klicken **Ausführung**.

Die **Ausgewählt ** Panel listet die angeforderten Outputs auf. ** Dateimigration** stellt den dedizierten Workflow für die Migration unterstützter Dateien bereit, anstatt eine Standardbildkonvertierung durchzuführen.

### Formatauswahl

Die **Maschine ** Liste filtert die im ** Format** Panel. Ein Formatname beschreibt das logische Festplattenlayout; die Erweiterung beschreibt den Ausgabecontainer. Einige Formate können durch mehr als eine Erweiterung dargestellt werden, und einige Container können nicht jedes Merkmal einer Rohquelle beibehalten.

Wählen Sie nur Outputs aus, die Sie tatsächlich benötigen. Mehrere Formate sind nützlich, wenn Sie einen Archivierungsmaster, eine emulatorkompatible Kopie und eine Kopie für ein anderes Analysewerkzeug in einem Vorgang erstellen.

### Output Benennung und Tags

**Ausgabebezeichnungen ** Sie können die Basisnamen steuern, die für ausgewählte Formate generiert wurden. ** Tags hinzufügen ** wendet das Dateinamenmuster an, das in ** Optionen > Generalmajor**Tags können Familie, Format, Erweiterung, Datum oder Uhrzeit codieren. Zeigen Sie das Beispiel in Optionen an, bevor Sie einen großen Batch konvertieren, damit Dateien konsistent benannt werden.

### Überprüfung der Konvertierungsergebnisse

Für jede angeforderte Ausgabe:

1. Bestätige, dass eine Datei erstellt wurde.
2. Überprüfen Sie die Konsole auf Tracks oder Sektoren, die nicht dekodiert werden konnten.
3. Öffnen Sie das Ergebnis in **Disk Explorer** wenn es ein unterstütztes Dateisystem enthält.
4. Vergleichen Sie die erwartete Festplattenkapazität und den Inhalt mit der Quelle.

Eine Konvertierung kann abgeschlossen werden, während der Informationsverlust gemeldet wird, der dem Zielformat inhärent ist. Behalten Sie das ursprüngliche Rohbild bei, auch wenn das konvertierte Bild korrekt erscheint.

## Visualisierung eines Disk Images

Die **Visualisierung** Tab zeigt die Struktur und Datenverteilung eines Bildes an.

<p align="center"><img src="../images/main-visualization-en.png" alt="Tab Visualisierung" width="78%"></p>

1. Klicken **Öffnen Sie ein Disk Image**.
2. Bleiben **Automatische Erkennung** aktiviert oder die Maschine und das Format manuell auswählen.
3. Verwendung **Linkzoom** um beide Seiten auf dem gleichen Zoomniveau zu halten.
4. Verwendung **Zurücksetzen** um die ursprüngliche Ansicht wiederherzustellen.
5. Offen **Inspektor** für detaillierte Informationen über die ausgewählte Region.

Die Legende unterscheidet normalen Fluss, kurze und lange Übergänge, Header, dekodierte Daten und erkannte Anomalien. Ein Rohbild kann Daten enthalten, die nicht in ein bekanntes Dateisystem decodiert werden können, aber hier noch inspiziert werden können.

### Interpretation der Ansicht

Jede große Kreisplatte stellt eine Scheibenseite dar. Das Zentrum identifiziert die Seite und ihren aktuellen Datenzustand; konzentrische Positionen entsprechen Spuren. Farben klassifizieren die erkannten Regionen nach der Legende. Der Visualizer soll Fragen beantworten wie:

- Enthält das Bild Daten auf einer Seite oder auf beiden?
- Sind die erwarteten Strecken vorhanden?
- Werden Anomalien isoliert oder auf der gesamten Festplatte wiederholt?
- Hat die automatische Erkennung eine plausible Maschine und ein plausibles Format identifiziert?

Eine Anomaliefarbe ist ein Grund, die Region zu inspizieren, nicht der Beweis, dass die Scheibe unbrauchbar ist. Kopierschutz, nicht standardisierte Formatierung, eine schwache Aufzeichnung und ein beschädigter Sektor können verschiedene Strukturen erzeugen, die eine kontextbezogene Interpretation erfordern.

### Empfohlene Inspektionssequenz

Beginnen Sie mit verknüpftem Zoom, der aktiviert ist, um beide Seiten auf derselben Skala zu vergleichen. Wählen Sie eine verdächtige Region, öffnen **Inspektor**, und vergleichen Sie es mit benachbarten Spuren. Wenn das Ergebnis ein Erkennungsproblem zu sein scheint, deaktivieren Sie die automatische Erkennung und wählen Sie eine bekannte Maschine und ein bekanntes Format. Kehren Sie nach dem Test zur automatischen Erkennung zurück, damit eine erzwungene Einstellung nicht versehentlich für ein anderes Bild verwendet wird.

## Erkundung des Platteninhalts

Die **Disk Explorer** Tab-Browses unterstützt Disk-Images als Dateihierarchie.

<p align="center"><img src="../images/main-disk-explorer-en.png" alt="Disk Explorer Tab." width="78%"></p>

1. Öffnen Sie ein vorhandenes Bild oder lesen Sie eine Festplatte.
2. Bleiben **Automatische Erkennung** aktiviert, es sei denn, Sie müssen eine Maschine oder ein Format erzwingen.
3. Überprüfen Sie die Volumeninformationen: System, Schutz, Dateisystem, Kapazität, freier Speicherplatz und Artikelanzahl.
4. Durchsuchen Sie Verzeichnisse im linken Bereich.
5. Wählen Sie ein Element aus, um die Details im rechten Bereich anzuzeigen.

Wenn das Bildformat oder Dateisystem nicht unterstützt wird, verwenden Sie **Visualisierung** stattdessen die Rohstruktur zu inspizieren.

### Die Panels verstehen

Die obere Zusammenfassung beschreibt das montierte Bild und das erfasste Volumen. Das untere linke Panel enthält die Verzeichnishierarchie. Die zentrale Tabelle listet Elemente im ausgewählten Verzeichnis mit Name, Änderungsdatum, Typ und Größe auf. Das rechte Feld zeigt Details für das ausgewählte Element an.

Disk Explorer Das bedeutet nicht, dass jeder Raw Track perfekt dekodiert wurde. Verwenden Sie die Volumenzusammenfassung und die Artikelanzahl als schnelle Plausibilitätsprüfung, öffnen Sie dann repräsentative Dateien oder vergleichen Sie sie mit einer bekannten Verzeichnisliste, wenn es auf die Erhaltungsgenauigkeit ankommt.

### Wenn nichts erscheint

Bestätigen Sie zunächst, dass der Bildweg korrekt ist. Überprüfen Sie dann die erkannte Maschine und das Format. Ein gültiges Bild kann ein nicht unterstütztes oder beschädigtes Dateisystem enthalten, wobei der Explorer leer bleiben kann, obwohl **Visualisierung** aufgezeichnete Daten zeigt. Überschreiben oder verwerfen Sie das Quellbild nicht, das nur auf einem leeren Explorer basiert.

## Mit den Werkzeugen

Die **Werkzeuge** Tab Gruppen Greaseweazle Instandhaltungsarbeiten.

<p align="center"><img src="../images/main-tools-en.png" alt="Tab. Werkzeuge" width="78%"></p>

Wählen Sie einen Befehl aus der Liste auf der linken Seite, überprüfen Sie die Parameter, dann klicken **Ausführung** Destruktive oder Hardware-Änderungsbefehle sollten nur nach Überprüfung der ausgewählten Steuerung und des Antriebs verwendet werden.

Die meisten Werkzeugdialoge enthalten drei Bereiche: Parameter oben, einen Status- und Rohausgabebereich in der Mitte und den generierten Befehl unten. Die Befehlsvorschau ändert sich, wenn Optionen aktiviert sind. Ein nicht überprüfter Parameter bedeutet normalerweise "ändern Sie diesen Wert nicht", während ein überprüfter Parameter diesen Wert im Befehl enthält.

Die einzelnen Diagnosedialoge sind beschrieben in [Hardware-Diagnose und Wartung]](#hardware-diagnostics-and-maintenance).

## Emulation

### Öffnen einer gespeicherten Maschine

Die **Emulation ** Tab-Listen gespeicherte Konfigurationen. Eine auswählen und klicken ** Offen**Jede laufende Maschine erscheint in ihrem eigenen Tab.

<p align="center"><img src="../images/main-emulation-welcome-en.png" alt="Emulation Welcome Screen" width="78%"></p>

Erstellen und Bearbeiten von Maschinen in **Optionen > Emulation > Konfigurationen ** und ** Optionen > Emulation > Amiga**.

Wenn keine Konfiguration angezeigt wird, erstellen Sie zuerst eine in Optionen. Eine gespeicherte Konfiguration kombiniert das Maschinenmodell, die Emulatorversion, ROM, Speicher, Video, Audio, Speicher und Eingabe-Mappings. Speichern einer Konfiguration startet nicht; Zurück zum Haupt **Emulation ** Tab und Klick ** Offen**.

### Steuerung der Laufmaschine

<p align="center"><img src="../images/main-emulation-running-en.png" alt="Laufende emulierte Maschine" width="78%"></p>

Die Running Machine Toolbar bietet Power, Pause, Reset, Save-State, Load-State, Capture und Display-Steuerelemente. Es zeigt auch:

- die konfigurierten Quick-Save- und Quick-Load-Shortcuts;
- Der aktive Renderer, wie Direct3D 11;
- die Abkürzungen für Vollbild- und Mausfreigabe;
- Audio, Controller und Mauszustand;
- aktuelle Auflösung, Bildwiederholrate und Bildrate.

Die Scheibenleiste am unteren Rand des Emulationsdisplays verwaltet abnehmbare Medien für jedes emulierte Laufwerk. Tastaturzuweisungen können geändert werden **Optionen > Emulation > Abkürzungen** während emulierte Tastatur-, Maus- und Controller-Mappings in den entsprechenden Amiga Tabs.

### Bezug auf die Symbolleiste

| Kontrollgruppe | Zweck |
|---|---|
| Kraft und Pause | Startet, stoppt, pausiert oder nimmt die emulierte Maschine wieder auf |
| Reset-Kontrollen | Führt die konfigurierte Soft- oder Hard-Reset-Aktion aus |
| Staatliche Kontrollen | Speichert oder lädt einen Emulatorzustand für eine schnelle Fortsetzung |
| Einfangen | Speichert ein Bild des emulierten Displays |
| Anzeige | Ändert die Display-Präsentation oder geht in Vollbild |
| Quick State Erinnerung | Zeigt die aktiven Save/Load Shortcuts an |
| Renderer | Meldet das aktive Video Backend |
| Eingabeerinnerung | Vollbild- und Maus-Release-Verknüpfungen |
| Geräteanzeiger | Berichtet Audio, Controller und Mauszustand |
| Leistung | Reports Ausgabegröße, Aktualisierungsfrequenz und Bildrate |

### Vollbild verlassen oder Maus freigeben

Die Symbolleiste zeigt die aktuell zugewiesenen Tasten an. In der dargestellten Konfiguration, **Alt+ Rückkehr ** schaltet Vollbild und ** F12** lässt die Maus frei. Behandeln Sie die angezeigten Werte als autoritativ, da Verknüpfungen neu zugewiesen werden können.

### Verwendung von Floppy-Medien

Die Antriebsleiste kennzeichnet jeden emulierten Antrieb, wie `DF0:`Verwenden Sie die Mediensteuerung, um ein Bild einzufügen, zu ersetzen oder auszuwerfen. Das Ersetzen des Mediums ändert nur die eingesetzte Festplatte der laufenden Maschine; es ändert nicht die Definition des Speichergeräts in der gespeicherten Maschine, es sei denn, diese Aktion wird explizit gespeichert.

## Anwendungsoptionen

Offen **Optionen** aus dem Hauptfenster, um die Anwendung zu konfigurieren.

### Generalmajor

<p align="center"><img src="../images/options-general-en.png" alt="Allgemeine Optionen" width="72%"></p>

Die **Generalmajor** tab enthält:

- den Standard-Festplattenbildordner;
- Schnittstellensprache und -thema;
- Filename-Tag-Generierung für Conversions;
- vordefinierte und aktuelle benutzerdefinierte Tag-Muster;
- Ein Beispiel für einen Live-Dateinamen.

Tag-Variablen umfassen den Quellnamen, die Familie, das Format, die Erweiterung, das Datum und die Uhrzeit. Verwenden Sie die Reset-Taste, um das Standardmuster wiederherzustellen.

Die Dateiname-Vorschau wird aktualisiert, bevor Dateien erstellt werden. Verwenden Sie es, um doppelte Separatoren, fehlende Erweiterungen oder mehrdeutige Namen zu erkennen. Aktuelle benutzerdefinierte Muster bieten schnellen Zugriff auf frühere Namensschemata, ohne die aktuelle Voreinstellung zu ersetzen.

### Protokolle

<p align="center"><img src="../images/options-logs-en.png" alt="Log-Optionen" width="72%"></p>

Die Protokollierung kann für jede Operation unabhängig konfiguriert werden. Wählen Sie für jede Kategorie aus, ob Protokolle gespeichert werden sollen, legen Sie eine maximale Dateigröße fest und entscheiden Sie, ob frühere Protokolle beibehalten werden sollen. Eine Größe von `0` bedeutet unbegrenzt. **Ordner öffnen** öffnet das aktuelle Logverzeichnis.

Ermöglicht **Vorherige Protokolle aufbewahren** für Konservierungs- und Diagnosearbeiten, bei denen die Geschichte mehrerer Versuche von Bedeutung ist. Deaktivieren Sie es, wenn nur das neueste Ergebnis nützlich ist. Maximale Größenbeschränkungen gelten für die Protokollspeicherung, nicht für erfasste Disk-Images.

### Steuerungen und Antriebe

<p align="center"><img src="../images/options-controllers-and-drives-en.png" alt="Steuerungen und Antriebe" width="72%"></p>

Verwenden Sie diesen Tab für:

- Scan nach angeschlossenen Controllern;
- Hinzufügen und Entfernen von Antriebskonfigurationen;
- Wählen Sie Antriebsgröße, -dichte und -geschwindigkeit aus;
- Hardwareeinstellungen speichern;
- Wählen oder automatisch finden `gw.exe`;
- Check for und Download Greaseweazle Host Tools Aktualisierungen;
- Wiederherstellen eines zuvor konfigurierten ausführbaren Pfads.

Gespeicherte Hardwareeinstellungen bleiben verfügbar, wenn ein Laufwerk vorübergehend getrennt wird.

#### Hinzufügen eines Laufwerks

1. Klicken **Scan** und warten, bis angeschlossene Controller erscheinen.
2. Klicken **Hinzufügen eines Laufwerks** wenn der erforderliche Antrieb nicht bereits aufgeführt ist.
3. Wählen Sie die logische Laufwerksnummer, die physische Größe, die Aufzeichnungsdichte und die Rotationsgeschwindigkeit.
4. Rette die Reihe.
5. Bestätigen Sie, dass es zeigt **verfügbar ** und ** Konfiguriert**.

Verwenden Sie die Müllkontrolle nur, um die gespeicherte Konfiguration zu entfernen; es trennt die Hardware nicht. Wenn der gleiche Controller auf einer anderen COM Scannen Sie später erneut, bevor Sie annehmen, dass der gespeicherte Port noch gültig ist.

#### Verwaltung Greaseweazle Host Tools

**Finde gw.exe ** Sucht bekannte Standorte. ** Wählen ** wählt eine bestimmte ausführbare Datei aus. ** Überprüfen Sie auf Updates ** Abfragen verfügbarer Versionen, ohne die installierte zu ersetzen. ** Download der neuesten Version ** das ausgewählte aktuelle Paket installiert und ** Vorheriger Pfad verwenden ** stellt den zuvor konfigurierten Standort wieder her. Nach dem Ändern der ausführbaren Datei, laufen ** Angaben zum Verantwortlichen** um zu bestätigen, dass die ausgewählte Version mit dem Controller kommunizieren kann.

### Motoren

<p align="center"><img src="../images/options-engines-en.png" alt="Motorauswahl" width="72%"></p>

Wählen Sie den Motor unabhängig zum Lesen, Schreiben, Konvertieren und Disk Explorer. Der ausgewählte Motor wird ausschließlich verwendet, wenn er den gewünschten Vorgang nicht ausführen kann, GW GUI meldet die Einschränkung, anstatt stillschweigend die Motoren zu wechseln.

Diese Unabhängigkeit ist beabsichtigt. Zum Beispiel können physische Lesevorgänge verwendet werden Greaseweazle Host Tools während Bildkonvertierung und -exploration die interne Engine verwenden. Motorauswahl in einer Profil- oder Projektnotiz aufzeichnen, wenn Reproduzierbarkeit wichtig ist.

### Profile

<p align="center"><img src="../images/options-profiles-en.png" alt="Profile" width="72%"></p>

Profile speichern wiederverwendbare Einstellungen für Lese-, Schreib- und Konvertierungsvorgänge. Wählen Sie die entsprechende Kategorie aus, um ihre Profile zu verwalten. Ein ausgewähltes Profil wird in der Statusleiste des Hauptfensters und in Betriebsbildschirmen angezeigt.

Verwenden Sie Profile für wiederholbare Workflows und nicht als unerklärte Sammlungen von Expertenflags. Geben Sie jedem Profil einen zweckspezifischen Namen, z. B. ein bestimmtes Laufwerk, eine bestimmte Festplattenfamilie oder eine bestimmte Wiederherstellungsmethode. Überprüfen Sie ein Profil nach der Aktualisierung der zugrunde liegenden Engine, da sich die unterstützten Optionen ändern können.

## Emulationsoptionen

Die **Emulation** Optionen enthalten allgemeine Speichereinstellungen, globale Verknüpfungen, gespeicherte Konfigurationen und maschinenspezifische Einstellungen.

### Allgemeine Emulationsordner

<p align="center"><img src="../images/options-emulation-general-en.png" alt="Allgemeine Emulationsoptionen" width="72%"></p>

Legen Sie den freigegebenen Emulationsspeicherordner und die Standardordner für Erfassungen und gespeicherte Zustände fest. **Ordner öffnen** öffnet den freigegebenen Speicherort im File Explorer.

Bewahren Sie Captures und gespeicherte Zustände in separaten Ordnern auf. Eine Aufnahme ist ein gewöhnliches Bild; ein gespeicherter Zustand enthält einen emulatorspezifischen Maschinenzustand und kann von der Emulatorversion und -konfiguration abhängen, die ihn erstellt hat. Sichern Sie Konfiguration und Medien neben wichtigen gespeicherten Zuständen.

### Globale Abkürzungen

<p align="center"><img src="../images/options-emulation-shortcuts-en.png" alt="Emulationsabkürzungen" width="72%"></p>

Suchen Sie nach einer Aktion oder Schlüsselzuweisung, weisen Sie Verknüpfungen zu oder entfernen Sie diese, stellen Sie Standardwerte wieder her und löschen Sie Konflikte. Die Statusspalte identifiziert gültige und widersprüchliche Zuweisungen.

Um eine Verknüpfung zu ändern, finden Sie die Aktion, klicken **Zuweisung **, und drücken Sie die gewünschte Tastenkombination. Überprüfen Sie den Status, bevor Sie Optionen schließen. ** Klare Konflikte ** entfernt widersprüchliche Zuweisungen; es stellt die Standardzuordnung nicht wieder her. Verwendung ** Standardabweichungen wiederherstellen** wenn Sie benutzerdefinierte Zuweisungen durch den Standardsatz ersetzen möchten.

### Speicherte Konfigurationen

<p align="center"><img src="../images/options-emulation-configurations-en.png" alt="Gespeicherte Emulationskonfigurationen" width="72%"></p>

Diese Seite listet gespeicherte Maschinen auf. Wählen Sie eine Konfiguration zum Bearbeiten im **Amiga** Tab. Sie können die Liste aktualisieren oder die ausgewählte Konfiguration löschen.

Durch das Löschen einer Konfiguration wird die gespeicherte Maschinendefinition entfernt. Es sollte nicht als eine Möglichkeit verwendet werden, Medien auszuwerfen oder eine laufende Maschine zu schließen. Vor der Löschung notieren Sie alle ROM, Festplattenbild und Statusdateien, die der Konfiguration zugeordnet sind.

## Amiga Konfiguration

Die aktuelle Schnittstelle bietet detaillierte Amiga Konfigurationsseiten. Die gleiche Einstellungsstruktur kann für andere emulierte Systeme erweitert werden, ohne den Hauptworkflow zu ändern.

### Generalmajor

<p align="center"><img src="../images/options-amiga-general-en.png" alt="Amiga allgemeine Einstellungen" width="72%"></p>

Wählen Sie Amiga Modellieren, Speichern der Konfiguration, Installieren oder Ersetzen der Emulatorversion und Definieren von Standardordnern für Festplatten und andere Medien. **Suchversionen** fragt die offizielle Emulator-Versionsquelle ab.

Beginnen Sie mit dem Modell, weil es spätere Seiten einschränkt. Ändern es kann die verfügbare ändern CPU, Speicher, ROMChipsatz und Speicheroptionen. Nachdem Sie eine Emulatorversion ausgewählt haben, speichern Sie die Konfiguration, bevor Sie sie aus dem Hauptfenster starten. Die Installation einer anderen Emulatorversion ersetzt die von dieser Konfiguration verwendete Version; es wird keine zweite Kopie der Maschine erstellt.

### CPU

<p align="center"><img src="../images/options-amiga-cpu-en.png" alt="Amiga CPU Einstellungen" width="72%"></p>

Die CPU Seite zeigt den vom Maschinenmodell ausgewählten Prozessor und liefert kompatible Präzision, FPUund Geschwindigkeitsauswahl. Optionen, die nicht für das ausgewählte Modell gelten, bleiben deaktiviert.

- **CPU Modell** den emulierten Prozessor identifiziert.
- **Präzision** steuert das Timing-Modell. Zyklus-genaue Modi bevorzugen Hardware-Kompatibilität, erfordern aber mehr Host-Verarbeitung.
- **FPU** ermöglicht bei Unterstützung eine kompatible Gleitkommaeinheit.
- **CPU Drehzahl** wählt das ursprüngliche Timing oder einen beschleunigten Modus aus.

Für eine Baseline-Konfiguration halten Sie das Modell abgeleitet CPU Originalgeschwindigkeit. Ändern Sie die Beschleunigung erst, nachdem die Maschine korrekt in ihren Standardeinstellungen bootet.

### RAM

<p align="center"><img src="../images/options-amiga-ram-en.png" alt="Amiga RAM Einstellungen" width="72%"></p>

Chip konfigurieren RAM, Langsam RAM, Schnell RAMund unterstützten Erweiterungsspeicher. Kompatibilitätsmeldungen erklären Einschränkungen für die ausgewählte Maschine, und der insgesamt konfigurierte Speicher wird unten angezeigt.

**Chip RAM ** ist für die benutzerdefinierten Chips zugänglich und wird von der Plattform benötigt. ** Langsam RAM ** stellt einen kompatiblen Erweiterungsspeicher dar, der von gängigen Konfigurationen verwendet wird. ** Schnell RAM ** ist ein prozessororientierter Erweiterungsspeicher. ** Zorro III RAM** gilt nur für Modelle, die diese Erweiterungsarchitektur unterstützen. Die Kompatibilitätsmeldungen und deaktivierten Bedienelemente verhindern Kombinationen, die das ausgewählte Modell nicht darstellen kann.

### ROM

<p align="center"><img src="../images/options-amiga-rom-en.png" alt="Amiga ROM Einstellungen" width="72%"></p>

Wählen Sie das System Kickstart ROM, optional erweitert ROM, und ROM Schlüssel. Die entdeckten-ROM Liste zeigt Namen, Überarbeitungen und Kompatibilität mit dem ausgewählten Modell an. Auswählen eines erkannten ROM und klicken **Verwendung** oder browsen Sie manuell zu einer Datei.

ROM Dateien werden nicht geliefert von GW GUIVerwenden Sie ROMs, die Sie gesetzlich verwenden dürfen.

Die erkannte Liste ist dem Raten aus einem Dateinamen vorzuziehen: Sie meldet die ROM Identität und Überarbeitung und bewertet die Kompatibilität mit dem ausgewählten Modell. **Kompatibel ** ist die normale Wahl; ** Teilweise kompatibel ** zeigt, dass die ROM kann booten, passt aber nicht genau zur Maschine. ** Erfrischend ** Rescannen der konfigurierten ROM Standorte. ** Verwendung** ordnet den ausgewählten detektierten ROM zur Konfiguration.

### Video

<p align="center"><img src="../images/options-amiga-video-en.png" alt="Amiga Videoeinstellungen" width="72%"></p>

Konfigurieren Sie Videostandard, Seitenverhältnis, Auflösung, Linienmodus, Randbeschneidung, Renderer, Farbtiefe, Frame Skipping, Gamma und Flimmerfixierung. Zusätzliche Chipsatzeinstellungen sind weiter unten auf der Seite verfügbar, wenn sie vom ausgewählten Modell unterstützt werden.

| Einstellung | Praktische Wirkung |
|---|---|
| Videostandard | Ausgewählt PAL oder NTSC Timing und erwartetes Refreshverhalten |
| Aspektverhältnis | Steuert, wie das emulierte Bild skaliert wird |
| Resolution | Wählen Sie automatische oder explizite Ausgabedetails aus |
| Streckenmodus | Steuerungen Behandlung von interlaced oder line-doubled Output |
| Anbaugrenzen | Entfernt unbenutzten Overscan nur, wenn aktiviert |
| Tierkörperbeseitigung | Wählen Sie das Grafik-Backend |
| Farbtiefe | Wählen Sie die Output-Farbpräzision aus |
| Rahmensprung | Reduziert gerenderte Frames, wenn aktiviert |
| Gamma | Anpassung des Helligkeitsverhaltens |
| Flickerfixer | Verarbeitet Modi, die sonst sichtbar flackern würden |

Ändern Sie jeweils eine Anzeigeeinstellung. Wenn das Emulationsfenster leer oder instabil wird, kehren Sie zur automatischen Auflösung, zum deaktivierten Frame-Skipp, zum neutralen Gamma und zum zuvor funktionierenden Renderer zurück.

### Audio

<p align="center"><img src="../images/options-amiga-audio-en.png" alt="Amiga Audioeinstellungen" width="72%"></p>

Audio aktivieren oder deaktivieren, Ausgabegerät und Latenz auswählen und dann Interpolation konfigurieren, Amiga Filterung, Filtertyp, Stereotrennung, Floppy-Drive-Sound und CD-Audio-Volume.

Eine geringere Latenz reduziert die Verzögerung, kann jedoch zu Ausfällen auf einem beschäftigten Computer führen. Erhöhen Sie es, wenn Audio knistert. Interpolation und die Amiga Audiofilter ändern die Tonwiedergabe anstelle der emulierten Programmlogik. Drive-Sound-Volume steuert den simulierten mechanischen Klang getrennt von normal Amiga Audio.

### Lagerung

<p align="center"><img src="../images/options-amiga-storage-en.png" alt="Amiga Speichereinstellungen" width="72%"></p>

Die Speicherseite listet Gerätekennungen, Typen, Modelle, zugehörige Medien und verfügbare Aktionen auf. Fügen, konfigurieren oder entfernen Sie hier Geräte. Disketten und CDs können direkt von einer laufenden Maschine eingelegt oder ersetzt werden.

Die **Gerätekennung ** ist, wie das emulierte System das Gerät anspricht. ** Typ ** unterscheidet Diskette, Festplatte, optische und andere unterstützte Geräte. ** Modell ** beschreibt die emulierte Hardware, während ** Verbundene Medien** identifiziert das aktuell zugewiesene Bild. Konfigurieren Sie das Gerät, bevor Sie wertvolle beschreibbare Medien zuordnen, und speichern Sie Backups von Festplattenbildern.

### Tastatur

<p align="center"><img src="../images/options-amiga-keyboard-en.png" alt="Amiga Tastatureinstellungen" width="72%"></p>

Suche Amiga Schlüssel und Hostzuweisungen, neue Schlüssel zuweisen, Zuordnungen entfernen, Standardwerte wiederherstellen oder Konflikte löschen. Die Statusspalte gibt an, ob jede Zuweisung gültig ist.

Die linke Spalte benennt die emulierten Amiga Schlüssel; **Verband** zeigt die Host-Schlüsselkombination. Eine gültige Zuordnung kann immer noch unbequem sein, wenn Windows oder die Anwendung die gleiche Verknüpfung reserviert, also kritische Kombinationen innerhalb der laufenden Maschine testen. Vermeiden Sie es, die Mausfreigabe oder Vollbildverknüpfung einem Schlüssel zuzuweisen, den die emulierte Software häufig benötigt.

### Maus

<p align="center"><img src="../images/options-amiga-mouse-en.png" alt="Amiga Mauseinstellungen" width="72%"></p>

Legen Sie die physische Mausgeschwindigkeit fest, wählen Sie, welcher Analogstick die Maus steuert, passen Sie die analoge Totzone und Geschwindigkeit an und konfigurieren Sie Maus-Aktions-Mappings. Stellen Sie Standardwerte wieder her oder löschen Sie bei Bedarf Zuordnungskonflikte.

Erhöhen Sie die tote Zone, wenn ein Controller eine Zeigerdrift verursacht. Passen Sie die Geschwindigkeit des linken und rechten Sticks unabhängig an, wenn beide Sticks aktiviert sind. Die untere Zuordnungstabelle verknüpft Hosteingaben mit Maustasten oder Aktionen; überprüfen Sie den Konfliktstatus, nachdem Sie die Steuerungszuordnungen an anderer Stelle geändert haben.

### Controller

<p align="center"><img src="../images/options-amiga-controllers-en.png" alt="Amiga Reglereinstellungen" width="72%"></p>

Angeschlossene Steuerungen erkennen, Geräte und Steuerungstypen zuweisen Amiga Ports und konfigurieren Controller-Mappings und Turbo-Fire-Einstellungen. Die verfügbaren Auswahlmöglichkeiten hängen von der erkannten Hardware und der ausgewählten Maschine ab.

Port 1 und Port 2 sind unabhängig voneinander konfiguriert. **Automatik** Der Controller-Typ ist ein vernünftiger Ausgangspunkt, aber Software, die einen bestimmten Joystick oder eine bestimmte Maus erwartet, erfordert möglicherweise einen expliziten Typ. Führen Sie die Erkennung aus, bevor Sie einen neu angeschlossenen Controller zuweisen. Turbofeuer aktiviert wiederholt eine zugeordnete Eingabe und sollte deaktiviert bleiben, es sei denn, das Spiel oder die Anwendung profitiert davon.

## Hardware-Diagnose und Wartung

Diese Dialoge werden vom **Werkzeuge ** Tab. Jeder Dialog zeigt die generierte Greaseweazle Befehl. Überprüfen Sie es, bevor Sie klicken ** Ausführung**.

### Angaben zum Verantwortlichen

<p align="center"><img src="../images/tool-controller-information-en.png" alt="Angaben zum Verantwortlichen" width="62%"></p>

Zeigt Informationen an, die vom ausgewählten Controller gemeldet wurden. Expansion **Rohoutput** Wenn Sie die vollständige Befehlsantwort benötigen.

Verwenden Sie dies als ersten Diagnosebefehl. Eine erfolgreiche Antwort bestätigt: GW GUI kann die konfigurierte ausführbare Host-Tools starten und mit dem ausgewählten Gerät kommunizieren. Notieren Sie die Firmware- und Hardwareinformationen, bevor Sie ein Update durchführen.

### USB Bandbreite

<p align="center"><img src="../images/tool-usb-bandwidth-en.png" alt="USB Bandbreite" width="62%"></p>

Maßnahmen zur Verfügung USB Kommunikationsbandbreite. Verwenden Sie es, um instabile Transfers oder eine ungeeignete USB Verbindung.

Schließen Sie andere Software mit dem Controller vor dem Testen. Wiederholen Sie die Messung nach der Änderung der USB Port, Kabel oder Hub. Vergleichen Sie die Ergebnisse unter ähnlichen Bedingungen, anstatt eine einzelne Messung als absolute Garantie zu behandeln.

### Antriebsdrehzahl

<p align="center"><img src="../images/tool-drive-speed-en.png" alt="Antriebsdrehzahl" width="62%"></p>

misst die Antriebsdrehzahl. Erhöhen Sie die Anzahl der Messungen, wenn Sie ein repräsentativeres Ergebnis benötigen.

Eine einzelne Messung ist eine schnelle Überprüfung; mehrere Messungen zeigen, ob die Geschwindigkeit stabil ist. Lassen Sie das Laufwerk die normale Geschwindigkeit erreichen, bevor Sie das Ergebnis interpretieren. Ein unerwarteter Wert kann auf eine falsch konfigurierte Geschwindigkeit, ein mechanisches Problem oder ein Messaufbauproblem hinweisen.

### Kopf suchen

<p align="center"><img src="../images/tool-seek-head-en.png" alt="Kopf suchen" width="62%"></p>

Bewegt den Antriebskopf zu einem ausgewählten Zylinder. **Extreme Zylinder zulassen ** normalerweise eingeschränkte Positionen erlaubt und ** Motoraktiv halten** lässt den Motor während des Betriebs laufen. Verwenden Sie extreme Positionen nur, wenn das Hardware-Verfahren sie ausdrücklich erfordert.

Normales Suchen ist nützlich, um die Kopfbewegung oder Positionierung vor einer Diagnose zu bestätigen. Hören Sie auf abnormale wiederholte Aufpralle und stoppen Sie, wenn der angeforderte Zylinder für das Laufwerk ungeeignet ist. Dieses Tool liest oder validiert keine Daten am Zielzylinder.

### Diagnose der Antriebsausrichtung

<p align="center"><img src="../images/tool-drive-alignment-en.png" alt="Diagnose der Antriebsausrichtung" width="62%"></p>

Läuft wiederholte Lesevorgänge für die Antriebsausrichtungsanalyse aus. Es unterstützt Track-Auswahl, Revolution und Lesen zählt, Decodierung Format, Rohfluss, Index, Geschwindigkeit, PLL, Dichtestift, Hartsektor, TG43und Reverse-Data-Optionen. Ausrichtungsarbeit erfordert entsprechendes Referenzmedien- und Hardwarewissen.

Beginnen Sie mit einer bekannten Referenzscheibe und dem kleinsten Satz von Overrides. **Gleiswechsel ** definiert die beprobten Gleise und Köpfe; ** Umdrehungen pro Strecke ** steuert jede Probendauer; ** Anzahl der Ablesungen** bestimmt die Wiederholung. Aktivieren Sie ein benutzerdefiniertes Festplattendefinitions- oder Decodierungsformat nur, wenn es mit dem Referenzmedium übereinstimmt. Optionen wie gefälschter Index, harte Sektoren, PLL Overrides, Dichtestifte und TG43 Hardware- oder formatspezifisch sind und einen Vergleich bei falscher Verwendung ungültig machen können.

### Stecknadeln

<p align="center"><img src="../images/tool-hardware-pins-en.png" alt="Stecknadeln" width="62%"></p>

Lesen oder Ändern eines unterstützten Controller-Pins. Wählen Sie den Pin, aktivieren **Wechselstift ** nur beim Schreiben eines Wertes, und wählen ** Hohes Niveau** wenn dies für den vorgesehenen Hardwarebetrieb erforderlich ist.

mit **Wechselstift** deaktiviert, fragt der Befehl den Pin ab. Dies ist der sicherere Standard. Das Ändern eines Pegels wirkt sich direkt auf die Controller-I/O aus und sollte nur mit dem richtigen Greaseweazle Hardware-Dokumentation und Steckverdrahtung.

### Rücksetzregler

<p align="center"><img src="../images/tool-reset-controller-en.png" alt="Rücksetzregler" width="62%"></p>

Resets Greaseweazle Controller. Verwenden Sie dies, wenn der Controller erkannt wird, aber nicht mehr normal reagiert.

Warten Sie, bis eine aktive Festplattenoperation abgeschlossen ist, bevor Sie zurücksetzen. Danach scannen Sie den Controller erneut, wenn sich sein Verbindungsstatus nicht automatisch erholt. Ein Reset repariert keinen Fehler `gw.exe` Pfad oder getrennt USB Vorrichtung.

### Verzögerungen

<p align="center"><img src="../images/tool-delays-en.png" alt="Verzögerungen des Reglers" width="62%"></p>

Lesen oder Ändern von Controller-Timing-Werten, einschließlich Auswahl, Kopfschritt, Abgleich, Motor, automatische Desauswahl, Schreib-Timing und Indexmaskenverzögerungen. Aktivieren Sie nur die Werte, die Sie ändern möchten.

Ungeprüfte Felder lassen den entsprechenden Controllerwert unverändert. Vor dem Bearbeiten notieren Sie die vorhandenen Werte. Timing-Änderungen können sich auf jede nachfolgende physische Operation auswirken, also testen Sie mit entbehrlichen Medien und stellen Sie bekannte gute Werte wieder her, wenn das Verhalten unzuverlässig wird.

### Firmware

<p align="center"><img src="../images/tool-firmware-en.png" alt="Firmware Update" width="62%"></p>

Updates Controller Firmware. **Update Bootloader** ist ausdrücklich als riskant gekennzeichnet und sollte deaktiviert bleiben, es sei denn, das offizielle Firmware-Verfahren verlangt dies. Trennen Sie den Controller während eines Updates nicht.

Vor der Aktualisierung bestätigen Sie den angeschlossenen Controller mit **Angaben zum Verantwortlichen** Verwenden Sie eine stabile direkte USB Verbindung herstellen und andere Software schließen, die darauf zugreifen könnte. Nach Abschluss verbinden oder scannen Sie den Controller erneut und lesen Sie seine Informationen erneut, um die gemeldete Firmware-Version zu überprüfen.

## Protokolle und Betriebshistorie

Öffnen Sie die Operationshistorie, um gespeicherte Protokolle nach Operation zu inspizieren.

<p align="center"><img src="../images/operation-history-en.png" alt="Betriebsgeschichte" width="68%"></p>

Wählen Sie links ein Protokoll, um den Inhalt anzuzeigen. **Ausfuhren** Speichert eine Kopie für Diagnose oder Support. Pfade und Befehlszeilen können persönliche Ordnernamen enthalten, also überprüfen Sie exportierte Protokolle, bevor Sie sie teilen.

Die Live-Konsole im Hauptfenster zeigt den aktuellen Befehl und die letzte Ausgabe an. Seine Kopierschaltfläche kopiert den angezeigten Text.

### Ein Logbuch lesen

Ein nützliches Diagnoseprotokoll enthält den generierten Befehl, die Zeitstempel, die Motorausgabe und den endgültigen Status. Arbeiten Sie von unten nach oben: Identifizieren Sie den endgültigen Fehler und suchen Sie dann die erste Warnung oder den fehlgeschlagenen Track, der ihm vorausging. Ein späteres generisches Versagen ist oft nur die Folge einer früheren, spezifischeren Botschaft.

Überprüfen Sie beim Vergleich zweier Versuche, ob Controller, Antrieb, Motor, Profil, Quellpfad, Ausgabeformat und Expertenargumente identisch waren. Andernfalls kann ein anderes Ergebnis geänderte Einstellungen anstelle von Festplatteninstabilität widerspiegeln.

## Anwendungsdaten und tragbare Nutzung

GW GUI hält Benutzerdaten von Anwendungsbinärdateien getrennt. Je nach ausgewähltem Paket und Modus werden Einstellungen, Protokolle, heruntergeladene Tools, Emulatorkomponenten, Erfassungen, Zustände und Maschinenkonfigurationen entweder in der Anwendung gespeichert. `Data` Verzeichnis oder in den konfigurierten Benutzerdatenstandorten.

Bevor Sie eine tragbare Installation ersetzen oder verschieben, halten Sie den gesamten Anwendungsordner zusammen und sichern Sie die `Data` Ordner. Verschieben Sie keine einzelnen Dateien aus `lib`, weil die Anwendung ihre eigenen Bibliotheken und Bibliotheken von Drittanbietern aus dieser Struktur auflöst.

### Vorgeschlagene Backup-Inhalte

Sichern Sie Folgendes, wenn es für Ihren Workflow wichtig ist:

- Anwendungseinstellungen und Profile;
- Regler- und Antriebsdefinitionen;
- Emulationskonfigurationen;
- ROM Trassen und rechtmäßig gehalten ROM Backups;
- Festplatten- und Wechselmedienbilder;
- Staaten einnimmt und rettet;
- Betriebsprotokolle, die als Bestandserhaltungsprotokolle verwendet werden.

Festplattenbilder können viel größer sein als Einstellungen. Speichern Sie Archivierungsmaster nach Möglichkeit schreibgeschützt und arbeiten Sie an Kopien.

## Empfohlene Workflows

### Archivierung einer unbekannten Festplatte

1. Überprüfen und reinigen Sie das Laufwerk mit einem geeigneten Wartungsverfahren.
2. Schreibschutz der Festplatte, wenn möglich.
3. Wählen **Lesen Sie > RohbildSCP)**.
4. Verwenden Sie einen beschreibenden Dateinamen und lesen Sie den normalen Track-Bereich mit mehreren Umdrehungen.
5. Überprüfen Sie die Konsole und gespeichertes Protokoll.
6. Beide Seiten inspizieren **Visualisierung**.
7. Konvertieren Sie eine Kopie in wahrscheinliche Sektorformate.
8. Testen Sie die konvertierten Kopien in **Disk Explorer** oder geeignete Software.
9. Bewahren Sie den Rohmaster, das Protokoll und die Notizen zusammen.

### Erstellen einer Festplatte aus einem Bild

1. Überprüfen Sie das Bild und bestätigen Sie die erwartete Familie und das Format.
2. Legen Sie eine entbehrliche oder absichtlich beschreibbare Scheibe der richtigen Größe und Dichte ein.
3. Offen **Schreiben** und wählen Sie das Bild aus.
4. Bestätigen Sie das konfigurierte Laufwerk und das erkannte Format.
5. Schreibe die Diskette.
6. Lesen Sie es zurück zu einem separaten Verifizierungsbild.
7. Vergleichen Sie dekodierte Inhalte und überprüfen Sie verdächtige Tracks visuell.

### Erstellen einer emulierten Amiga

1. Offen **Optionen > Emulation > Konfigurationen** und eine Maschine erstellen oder auswählen.
2. In **Amiga > Allgemein** Wählen Sie die Modell- und Emulatorversion.
3. Zuweisen eines kompatiblen, legal erworbenen ROM.
4. Halten Sie das Modell standardmäßig für CPU und RAM Beim ersten Boot.
5. Konfigurieren Sie Video und Audio mit konservativen automatischen Einstellungen.
6. Fügen Sie Speichergeräte hinzu und verknüpfen Sie kopierte Medienbilder.
7. Überprüfen Sie Tastatur-, Maus- und Controllerzuweisungen.
8. Speichern Sie die Konfiguration.
9. Zurück zum **Emulation **, wählen Sie es aus und klicken ** Offen**.
10. Erst nach einem erfolgreichen Start der Baseline ändern Sie die Beschleunigung oder die erweiterten Einstellungen einzeln.

## Sicherheitscheckliste

Vorher **Lesen**:

- die Quellenscheibe befindet sich im richtigen Laufwerk;
- die Quelle nach Möglichkeit schreibgeschützt ist;
- der Ausgabepfad wird einen vorhandenen Master nicht überschreiben;
- Profil und Spurweite stimmen mit der Scheibe überein.

Vorher **Schreiben ** oder ** Erase**:

- die Zielscheibe kann zerstört werden;
- Bild und Laufwerk korrekt sind;
- Scheibengröße und -dichte sind kompatibel;
- kein Archivmaster als Zielort verwendet wird.

Vor einem Hardware-Wechsel-Tool:

- kein anderer Vorgang läuft;
- die richtige Steuerung ausgewählt wird;
- aktuelle Werte aufgezeichnet wurden;
- der Regler eine stabile Leistung hat und USB Vernetzung;
- die Maßnahme wird durch die Hardware-Dokumentation unterstützt.

## Fehlerbehebung

### Der Controller ist nicht aufgeführt

1. Verbinden Sie den Controller direkt mit dem Computer.
2. Offen **Optionen > Steuerungen und Antriebe**.
3. Klicken **Scan**.
4. Überprüfen Sie den Controllerstatus und die Laufwerkskonfiguration.
5. Lauf **Angaben zum Verantwortlichen** Wenn die Erkennung erfolgreich ist, aber Befehle fehlschlagen.

Wenn es immer noch nicht erscheint, versuchen Sie eine andere direkte USB Port und Kabel, dann rescannen. Überprüfen Sie Windows Device Manager nach einem neu erkannten seriellen Gerät. Ein Controller, der für Windows sichtbar ist, aber nicht von GW GUI Zeigt normalerweise auf einen beschäftigten Port, eine veraltete Konfiguration oder ein Problem mit Host-Tools; ein in Windows abwesender Controller zeigt auf USBLeistung, Treiber oder Hardware.

### `gw.exe` kann nicht gefunden werden

Offen **Optionen > Steuerungen und Antriebe **, dann verwenden ** Finde gw.exe **, ** Wählen **, oder ** Download der neuesten Version**Bestätigen Sie, dass der ermittelte Weg auf den beabsichtigten Weg weist Greaseweazle Installation.

Nachdem Sie es ausgewählt haben, laufen **Angaben zum Verantwortlichen** Wenn dies fehlschlägt, bevor Sie die Hardware kontaktieren, überprüfen Sie das Protokoll auf einen ungültigen ausführbaren Pfad, fehlende Dateien oder eine Version, die nicht gestartet werden kann.

### Eine Operation verwendet den falschen Motor

Offen **Optionen > Motoren** und überprüfen Sie den Motor, der genau diesem Vorgang zugeordnet ist. GW GUI nicht stillschweigend auf den anderen Motor zurückfällt.

Motoreinstellungen sind getrennt: Das Ändern der Konvertierungsmaschine ändert nicht Lesen, Schreiben oder Disk ExplorerÖffnen Sie den fehlgeschlagenen Vorgang nach dem Speichern der Option erneut und bestätigen Sie den generierten Befehl in der Konsole.

### Ein Bild wird nicht erkannt

Deaktivieren Sie die automatische Erkennung nur, wenn Sie die richtige Maschine und das richtige Format kennen. Ansonsten versuchen Sie die **Visualisierung** Tab, um das Bild auf einer niedrigeren Ebene zu inspizieren.

Überprüfen Sie, ob es sich bei der Quelle um eine Rohflusserfassung, ein Sektorbild, einen komprimierten Container oder eine nicht verwandte Datei mit einer irreführenden Erweiterung handelt. Benennen Sie eine Erweiterung niemals nur um, um die Erkennung zu erzwingen; die Konvertierung muss die Quellstruktur korrekt interpretieren.

### Emulation startet nicht

Überprüfen Sie die gespeicherte Konfiguration, installierte Emulatorversion, ausgewählt ROMSpeicherpfade und Modellkompatibilität. Überprüfen Sie das Anwendungsprotokoll auf die vollständigen Fehlerdetails.

Vorübergehende Rückkehr CPU, RAM, Video und Speicher für eine einfache modellkompatible Baseline. Wenn die Baseline startet, stellen Sie jeweils eine benutzerdefinierte Einstellung wieder her. Ein gespeicherter Zustand, der mit einer anderen Emulatorversion oder Maschinendefinition erstellt wurde, kann auch dann fehlschlagen, wenn ein sauberer Boot funktioniert.

### Eine Verknüpfung oder Eingabe funktioniert nicht

Überprüfen Sie sowohl die globale **Emulation > Abkürzungen** Seite und die maschinenspezifische Tastatur-, Maus- oder Controllerseite. Lösen Sie alle Zuweisungen, die als widersprüchlich gekennzeichnet sind.

Wenn die Maus erfasst wird, verwenden Sie die Release-Verknüpfung, die in der Toolbar der laufenden Maschine angezeigt wird. Wenn ein Controller nach dem Öffnen von Optionen verbunden war, führen Sie die Controllererkennung erneut aus, bevor Sie sie zuweisen.

### Ein Befehl schlägt unerwartet fehl

1. Lesen Sie die Live-Konsolenausgabe.
2. Offen **Betriebsgeschichte** für das vollständige gespeicherte Protokoll.
3. Bestätigen Sie den ausgewählten Controller, Antrieb, Profil, Motor und Dateipfade.
4. Exportieren Sie das entsprechende Protokoll, wenn es für die Diagnose freigegeben werden muss.

### Audio-Knistern oder Pausen

Emulations-Audiolatenz erhöhen, schließen CPU- intensive Anwendungen und geben das Überspringen und Beschleunigen von Videorahmen auf ihre vorherigen Werte zurück. Stellen Sie sicher, dass das vorgesehene Windows-Audiogerät ausgewählt ist. Ändern Sie eine Einstellung zu einer Zeit, so dass die effektive Korrektur identifizierbar ist.

### Die Emulationsanzeige ist leer oder langsam

Return Resolution und Line Mode auf **Automatik** Deaktivieren Sie Frame Skipping und Flicker Fixierung vorübergehend, und versuchen Sie den zuvor funktionierenden Renderer. Bestätigen Sie, dass die konfigurierte ROM und eingefügte Boot-Medien sind gültig. Die FPS Ein Indikator hilft, ein Rendering-Performance-Problem von einer Maschine zu unterscheiden, die einfach nicht gebootet wurde.

### Ein Read enthält instabile Tracks

Wiederholen Sie das Lesen mit einem neuen Dateinamen, erhöhen Sie gegebenenfalls die Umdrehungen und vergleichen Sie die betroffenen Tracks. Reinigen Sie die Antriebsköpfe mit einem korrekten Verfahren und inspizieren Sie die Scheibe auf physische Schäden. Lesen Sie nicht wiederholt sichtbar abwerfende oder beschädigte Medien, da weitere Pässe es verschlechtern können.

## Glossar

| Laufzeit | Bedeutung in GW GUI |
|---|---|
| Regler | Die Greaseweazle Hardware-Schnittstelle verbunden über USB |
| Antrieb | Das physische Floppy-Laufwerk, das an den Controller angeschlossen ist |
| Motor | Die Implementierung ausgewählt, um eine Operation durchzuführen |
| Fluss | Timing-Informationen, die magnetische Übergänge darstellen, die von einer Festplatte gelesen werden |
| Rohbild | Eine Erfassung, die Datenträgerinformationen auf niedriger Ebene speichert, wie z.B. SCP |
| Sektorbild | Eine dekodierte Darstellung in logischen Sektoren organisiert |
| Revolution | Eine vollständige Umdrehung beim Lesen einer Spur |
| Zylinder | Eine radiale Kopfposition; ein Zylinder kann eine Spur auf jeder Seite enthalten |
| Kopf | Die vom physischen Antrieb ausgewählte Scheibenseite |
| Profil | Ein wiederverwendbarer Satz von Einstellungen für eine Operation |
| ROM | Firmware-Image, das von einer emulierten Maschine benötigt wird |
| Geretteter Staat | Eine Momentaufnahme des Maschinenzustands eines laufenden Emulators |
| Renderer | Das Grafik-Backend zur Anzeige der Emulationsausgabe |

## Schnellreferenz

| Wenn du willst... | Gehen Sie zu... |
|---|---|
| Bewahren Sie eine physische Festplatte | **Lesen** |
| Setzen Sie ein Bild zurück auf eine Festplatte | **Schreiben** |
| Produzieren Sie ein anderes Bildformat | **Umwandlung** |
| Prüfen von Gleisen oder Flussanomalien | **Visualisierung** |
| Durchsuchen von Dateien innerhalb eines Bildes | **Disk Explorer** |
| Kontroll-Controller-Kommunikation | **Werkzeuge > Angaben zum Verantwortlichen** |
| Messung der Antriebsdrehung | **Werkzeuge > Antriebsdrehzahl** |
| Überprüfen Sie einen vergangenen Befehl | **Betriebsgeschichte** |
| Hardware konfigurieren | **Optionen > Steuerungen und Antriebe** |
| Auswählen von Implementierungen | **Optionen > Motoren** |
| Erstellen oder Bearbeiten einer emulierten Maschine | **Optionen > Emulation** |
| Starten Sie eine gespeicherte Maschine | **Emulation** |
