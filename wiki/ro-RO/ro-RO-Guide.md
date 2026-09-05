[🌐 Languages / Langues](../Home.md)

# GW GUI Ghid utilizator

GW GUI este o aplicație Windows pentru citirea, scrierea, convertirea, inspecția și emularea imaginilor cu dischetă. Poate controla Greaseweazle hardware, lucrați cu fișiere de imagini de disc prin motorul său intern, și executați configurații de mașini emulate salvate.

Acest ghid descrie interfața engleză afișată în versiunea curentă a aplicației. Acesta este scris ca sursă a manualului de utilizator imprimat: capturi de ecran ilustrează controalele, în timp ce textul din jur explică ce să aleagă, de ce să-l aleagă, și cum să verifice rezultatul.

> **Important:** Citirea unui disc nu e distructivă. Scriere, ştergere, actualizare firmware, şi unele instrumente hardware pot modifica media sau hardware. Citiți avertismentul atașat la procedura relevantă înainte de a face clic ** Execută**.

### Cum să utilizaţi acest ghid

Dacă aceasta este prima dată când utilizaţi GW GUI, complet [Începem](#getting-started), apoi urmați [Citirea unui disc](#reading-a-disk)Dacă aplicaţia este deja configurată, mergeţi direct la capitolul operaţiunii pe care doriţi să o efectuaţi. Capitolele opțiunilor servesc ca referință atunci când o procedură vă cere să modificați o unitate, motor, profil sau setare emulată-mașină.

Numele interfeței sunt afișate în **aldine**. Numele fișierelor, căile, comenzile și valorile literale sunt afișate ca `code`. Note explica comportamentul normal; avertismentele identifica operatiunile care pot modifica un disc, controler sau configuratie stocata.

## Cuprins

1. [Înțelegerea fluxului de lucru](#understanding-the-workflow)
2. [Începem](#getting-started)
3. [Fereastra principală](#main-window)
4. [Citirea unui disc](#reading-a-disk)
5. [Scrierea unui disc](#writing-a-disk)
6. [Conversia imaginilor discului](#converting-disk-images)
7. [Vizualizarea unei imagini de disc](#visualizing-a-disk-image)
8. [Explorarea conținutului discului](#exploring-disk-contents)
9. [Utilizarea instrumentelor](#using-the-tools)
10. [Emulare](#emulation)
11. [Opțiuni de aplicare](#application-options)
12. [Opțiuni de emulare](#emulation-options)
13. [Amiga configurare](#amiga-configuration)
14. [Diagnosticare și întreținere hardware](#hardware-diagnostics-and-maintenance)
15. [Jurnale și istoricul operațiunilor](#logs-and-operation-history)
16. [Date de aplicare și utilizare portabilă](#application-data-and-portable-use)
17. [Fluxuri de lucru recomandate](#recommended-workflows)
18. [Lista de verificare privind siguranța](#safety-checklist)
19. [Depanare](#troubleshooting)
20. [Glosar](#glossary)
21. [Referinţă rapidă](#quick-reference)

## Înțelegerea fluxului de lucru

GW GUI separă operațiunile cu disc fizic de operațiunile cu fișiere de imagine:

| Gol | Intrare | Rezultat | Pagina recomandată |
|---|---|---|---|
| Păstrați un disc floppy | Disc fizic | Fișier imagine | **Citit** |
| Recreați un disc floppy | Fișier imagine | Disc fizic | **Scrie** |
| Schimbă formatul imaginii | Fișier imagine | Unul sau mai multe fișiere de imagine | **Conversie** |
| Inspectează urmele și anomaliile | Fișier imagine | Analiza vizuală | **Vizualizare** |
| Navighează fișierele stocate într-o imagine | Sistem de imagini/fişiere susţinut | Fișiere și dosare | **Disk Explorer** |
| Diagnoză o unitate sau un controler | Greaseweazle hardware | Măsurători sau stare | **Unelte** |
| Rulează o mașină virtuală salvată | Configurare mașină salvată | Sesiunea de emulare | **Emulare** |

Pentru conservare, face mai întâi o captură brută și păstrați-l neschimbat ca un maestru. Creați copii de lucru convertite sau reparate de la acel maestru. Aceasta evită repetarea unei citiri fizice și păstrează informații pe care un format sectorial nu le poate păstra.

## Începem

### Cerințe

- Ferestre cu Microsoft .NET Desktop Runtime cerut de aplicație.
- A Greaseweazle controler pentru operațiuni fizice floppy-disc.
- O cale configurată către `gw.exe` atunci când se utilizează Greaseweazle Host Tools Motor.
- Obţinute legal ROM fișiere atunci când o mașină emulată le necesită.

Cererea verifică durata de funcționare necesară .NET la pornire. Dacă lipsește, urmați prompt instalarea, apoi reporniți GW GUI.

### Înainte de conectarea hardware-ului

Verificați următoarele înainte de a efectua o operație fizică-disc:

1. Conectează Greaseweazle controler la un grajd USB Port.
2. Conectați cablul floppy cu orientarea corectă.
3. Conectați sursa de alimentare a motorului înainte de introducerea unor medii valoroase.
4. Confirmă că dimensiunea și densitatea motorului se potrivesc discului.
5. Dacă este posibil, protejați discul sursă.

GW GUI nu pot preveni daunele cauzate de cablare incorectă, de o putere necorespunzătoare sau de o unitate nesigură mecanic. Testul hardware necunoscut cu un disc de consum mai întâi.

### Prima lansare

1. Deschide `gwgui.exe`.
2. Deschide **Opțiuni**.
3. În **Controloare și unități** Scanează după controler şi configurează unitatea.
4. Verificați sau selectați calea spre `gw.exe`.
5. În **Motoare**, alege care motor ar trebui să efectueze fiecare operațiune.
6. Întoarceți-vă la fereastra principală și selectați fila de operare necesară.

### Confirmând că configurarea este gata

Un setup de lucru ar trebui să arate controlorul și conduce în bara de stare, de exemplu, un număr de unitate, dimensiunea, densitatea, și COM Port. În **Opțiuni > Controloare și unități **, operatorul trebuie marcat ** Disponibil ** și unitatea ** Configurat **Fugi. ** Informații privind controlorul** înainte de a citi media valoroasă dacă doriți să verificați comunicarea fără a modifica un disc.

### Alegerea unui motor

GW GUI pot expune mai mult de o implementare pentru unele operațiuni. ă **Greaseweazle Host Tools** motorul invocă configurația `gw.exe`; intern GW GUI mânerele motorului care funcționează în cadrul aplicației. Selectarea motorului este explicită și independentă pentru citire, scriere, conversie și Disk Explorer. Dacă o operațiune nu este susținută de motorul selectat, GW GUI raportează această stare în loc să schimbe automat motoarele.

## Fereastra principală

Principala fereastră grupează principalele operațiuni în șapte file:

- **Citit** creează o imagine dintr-un disc fizic.
- **Scrie** scrie o imagine pe un disc fizic.
- **Conversie** convertește un format de imagine de disc într-unul sau mai multe formate de ieșire.
- **Vizualizare** afișează traseele și fluxul sau datele decodate.
- **Disk Explorer** browses suported file systems and disk content.
- **Unelte** furnizează comenzi de întreținere și diagnosticare hardware.
- **Emulare** gestionează și rulează mașini emulate salvate.

Consola din partea de jos afișează comanda fiind executat și ieșirea sa. Bara de stare raportează unitatea selectată, profilul și starea curentă.

### Citirea interfeței

Cele mai multe pagini de operare urmează același model:

1. **Sursa sau destinația** comenzile identifică discul, imaginea sau dosarul.
2. **Controale format** selectați detectarea automată sau o mașină și format explicit.
3. **Controale ale profilului** aplică setările reutilizabile.
4. **Configurări avansate** să expună parametrii care sunt în mod normal opționali.
5. **Execută** Începe operaţiunea.
6. ă **consolă** arată comanda generată, progres, avertismente și erori.

ă **Execută** butonul nu implică faptul că toate valorile sunt sigure pentru discul introdus. Examinați întotdeauna destinația și unitatea selectată înainte de o operațiune de scriere sau întreținere.

### Bară de stare și consolă

Partea stângă a barei de stare identifică unitatea fizică activă. Centrul arată profilul activ atunci când este selectat unul. Indicatorul de stat raportează dacă cererea este pregătită sau ocupată. Consola nu este doar un diagnostic: este înregistrarea autoritară a comenzii trimise la motorul selectat. Utilizați controlul copiei atunci când aveți nevoie pentru a păstra sau partaja acea comandă.

## Citirea unui disc

Deschide **Citit** fila pentru a captura un discheta fizica ca o imagine.

<p align="center"><img src="../images/main-read-en.png" alt="Citiţi fila" width="78%"></p>

### Procedura de bază

1. Introduceți discul sursă în unitatea configurată.
2. Alege tipul de imagine:
   - **Imagine brută (SCP)** păstrează informații la nivel de flux.
   - **Format disc cunoscut** creează o imagine folosind o mașină selectată și format.
3. Alegeți dosarul de destinație.
4. Introduceți numele fișierului de ieșire.
5. Alegeți un profil dacă este necesar.
6. Click **Execută**.

Consola arată comanda exactă şi progresul. Nu scoateți discul sau nu deconectați controlerul până când operațiunea nu este finalizată.

### Alegerea tipului de ieșire

Utilizare **Imagine brută (SCP)** atunci când obiectivul este capturarea, analiza, recuperarea sau conversia ulterioară a arhivalului. O imagine brută înregistrează informații de sincronizare și mai multe revoluții, care este util pentru formate neobișnuite, sectoare slabe, scheme de protecție, și medii deteriorate.

Utilizare **Format disc cunoscut** atunci când știți deja familia de discuri și au nevoie de o imagine sector direct utilizabil. Această alegere poate fi mai mică și mai ușor de deschis în alte programe, dar reprezintă rezultatul decodat mai degrabă decât fiecare detaliu observat de unitate.

Când sunt nesigure, creați imaginea brută mai întâi. Puteți converti mai târziu fără a citi discul din nou.

### Dosar, nume de fișier și profil

ă **Dosar ** este directorul de destinație. ă ** Nume fișier** trebuie să identifice discul fără a se baza doar pe eticheta sa fizică. Un nume de arhivare util conține titlul, numărul discului sau partea, și o notă de condiție, atunci când este cazul. Nu adăugați o extensie a formatului care intră în conflict cu formatul de ieșire selectat.

A **Profil ** aplică un set salvat de parametri de citire. Alegeți unul numai atunci când știți ce conține. ă ** Implicit** profilul este adecvat pentru o primă încercare normală; un profil de recuperare specializat poate citi în mod deliberat mai multe revoluții sau o gamă de cale diferite și, prin urmare, să dureze mai mult.

### Configurări avansate

Extinde **Configurări avansate** accesul la parametri specifici formatului sau experți. Lăsați aceste valori neschimbate cu excepția cazului în care discul necesită un anumit interval de cale, număr de revoluție, sau opțiunea controler.

Valorile avansate comune includ:

| Setare | Scop | Când să-l schimbe |
|---|---|---|
| Gama de linii | Limitează cilindrii și capetele de citit | Medii unilaterale, geometrie neobișnuită sau un permis de recuperare vizat |
| Revoluții | Controlează câte rotaţii sunt eșantionate | Creștere pentru șenile instabile sau protejate; reducerea vitezei numai atunci când este cazul |
| Argumentele experților | Trece parametrii suplimentari ai motorului | Numai atunci când urmează documentat Greaseweazle orientări |

### Verificarea unei citiri de succes

Nu te baza doar pe lipsa unui dialog de eroare. După ce comanda se încheie:

1. Confirmați că fișierul de ieșire există și nu este gol.
2. Citiți liniile finale ale consolei pentru piese eșuate sau lipsă.
3. Deschide imaginea în **Vizualizare** pentru a verifica dacă ambele părți și intervalul de cale preconizat conțin date.
4. Deschide-l. **Disk Explorer** atunci când sistemul de fișiere este susținut.
5. Păstrați jurnalul de operare cu capturi importante de arhivare.

Dacă citirile repetate diferă, se păstrează fiecare captură brută în loc să se suprascrie prima. Diferenţele pot fi utile în timpul recuperării.

## Scrierea unui disc

Deschide **Scrie** fila pentru a scrie o imagine existentă pe un disk fizic floppy.

<p align="center"><img src="../images/main-write-en.png" alt="Scrie fila" width="78%"></p>

### Procedura de bază

1. Introduceți discul de destinație.
2. Alegeți imaginea sursă cu **Răsfoiește**.
3. Confirmaţi formatul detectat.
4. Alegeți un profil dacă este necesar.
5. Click **Execută**.

Scrierea înlocuiește datele de pe discul de destinație. Verificați unitatea selectată și imaginea înainte de a începe.

> **Avertisment:** Scrisul este distructiv. Înlocuieşte datele magnetice de pe discul destinaţiei. Utilizați o arhivă sursă protejată prin scriere și un disc de destinație separat ori de câte ori este posibil.

### Înainte de a scrie

Verificați patru elemente înainte de a face clic **Execută**:

1. **Imagine:** calea selectată este imaginea sursă preconizată.
2. **Disc:** discul din unitate poate fi suprascris în siguranță.
3. **Condu:** dimensiunea și densitatea configurate se potrivesc mediului de destinație.
4. **Format:** detectarea automată sau formatul selectat manual corespunde imaginii.

Dacă imaginea sursei nu a fost testată, deschideți-o **Vizualizare ** sau ** Disk Explorer** Mai întâi. Un scris de succes nu poate repara o imagine de sursă incompletă.

### Inspecția și modificarea liniei

După selectarea unei imagini, **Vizualizează piesele ** îşi deschide reprezentarea. ** Modificare** expune modificările de imagine acceptate înainte de scriere. Acţiunile disponibile depind de formatul şi motorul selectate.

### Verificarea unui disc scris

Atunci când motorul susține verificarea, utilizați-l pentru medii importante. În caz contrar, citiți discul scris înapoi la o imagine nouă și comparați conținutul decodat sau inspectați-l în **Vizualizare**. Păstrați captura de verificare separat de imaginea originală, astfel încât originalul nu este niciodată suprascris.

În cazul în care scrierea nu reușește la piese consistente, verifica starea discului, densitatea, conduce curățenia, și configurarea unitate. Dacă eșecurile apar aleatoriu, verificați USB stabilitate și comunicare controler.

## Conversia imaginilor discului

ă **Conversie** fila convertește o imagine sursă în unul sau mai multe formate de destinație.

<p align="center"><img src="../images/main-conversion-en.png" alt="Conversie filă" width="78%"></p>

### Procedura de bază

1. Selectaţi imaginea sursă.
2. Oferă opțional nume de ieșire.
3. Alege o familie de maşini.
4. Alegeți unul sau mai multe formate de ieșire și extensii.
5. Activează **Adaugă etichete** dacă numele fișierului trebuie să utilizeze modelul de etichetă configurat.
6. Click **Execută**.

ă **Selectat ** Panoul enumeră rezultatele solicitate. ** Migrarea fișierelor** oferă fluxul de lucru dedicat pentru migrarea fișierelor acceptate, în loc să efectueze o conversie standard a imaginii.

### Selectarea formatelor

ă **Mașină ** filtrează formatele afișate în ** Format** Panou. Un nume de format descrie aspectul logic al discului; extensia descrie containerul de ieșire. Unele formate pot fi reprezentate de mai mult de o extensie, iar unele containere nu pot păstra fiecare caracteristică a unei surse brute.

Selectaţi doar ieşirile de care aveţi nevoie. Mai multe formate sunt utile atunci când se creează un maestru de arhivă, o copie emulator-compatibil, și o copie pentru un alt instrument de analiză într-o singură operațiune.

### Nume și etichete de ieșire

**Nume de ieșire ** vă permite să controlați numele de bază generate pentru formate selectate. ** Adaugă etichete ** aplică modelul de nume de fișier configurat în ** Opțiuni > Generale**Etichetele pot coda familia, formatul, extensia, data sau ora. Previzualizează exemplul din Opțiuni înainte de a converti un lot mare, astfel încât fișierele să fie numite în mod consecvent.

### Verificarea rezultatelor conversiei

Pentru fiecare ieșire solicitată:

1. Confirmați că a fost creat un fișier.
2. Verificați consola pentru piese sau sectoare care nu au putut fi decodate.
3. Deschide rezultatul **Disk Explorer** dacă conține un sistem de fișiere susținut.
4. Comparați capacitatea și conținutul de disc preconizate cu sursa.

O conversie poate completa în timp ce raportează pierderi de informații inerente formatului de destinație. Mentineti imaginea originala bruta chiar si atunci cand imaginea convertita apare corect.

## Vizualizarea unei imagini de disc

ă **Vizualizare** fila afișează structura și distribuția datelor unei imagini.

<p align="center"><img src="../images/main-visualization-en.png" alt="Fila de vizualizare" width="78%"></p>

1. Click **Deschide o imagine de disc**.
2. Păstrează **Detectare automată** activate sau selectați mașina și formatul manual.
3. Utilizare **Mărește legătura** pentru a păstra ambele părți la același nivel zoom.
4. Utilizare **Reinițializează** pentru a restabili vederea inițială.
5. Deschide **Inspectore.** pentru informații detaliate despre regiunea selectată.

Legenda distinge fluxul normal, tranziţiile scurte şi lungi, antetele, datele decodate şi anomaliile detectate. O imagine brută poate conține date care nu pot fi decodate într-un sistem de fișiere cunoscut, dar pot fi încă inspectate aici.

### Interpretare vizualizare

Fiecare panou circular mare reprezintă o parte a discului. Centrul identifică partea laterală și starea actuală a datelor; pozițiile concentrice corespund liniilor. Culorile clasifică regiunile detectate conform legendei. Vizualizatorul este destinat să răspundă la întrebări precum:

- Are imaginea conține date pe o parte sau ambele?
- Sunt prezente urmele aşteptate?
- Sunt anomalii izolate sau repetate pe disc?
- Detectarea automată a identificat o mașină și un format plauzibil?

O culoare anomalie este un motiv pentru a inspecta regiunea, nu dovada că discul este inutilizabil. Protecţia copierii, formatarea nestandardizată, o înregistrare slabă şi un sector afectat pot produce diferite structuri care necesită o interpretare contextuală.

### Secvența de inspecție recomandată

Începeți cu zoomul conectat activat pentru a compara ambele părți la aceeași scară. Selectaţi o regiune suspectă, deschideţi **Inspectore.** Şi compară-l cu urmele vecine. Dacă rezultatul pare a fi o problemă de detectare, dezactivați detectarea automată și alegeți o mașină și un format cunoscute. Se revine la detectarea automată după încercare, astfel încât o setare forțată să nu fie utilizată accidental pentru o altă imagine.

## Explorarea conținutului discului

ă **Disk Explorer** browsează imagini de disc acceptate ca ierarhie de fișiere.

<p align="center"><img src="../images/main-disk-explorer-en.png" alt="Disk Explorer tab" width="78%"></p>

1. Deschide o imagine existentă sau citeşte un disc.
2. Păstrează **Detectare automată** activat cu excepția cazului în care aveți nevoie pentru a forța o mașină sau un format.
3. Revizuiți informațiile privind volumul: sistem, protecție, sistem de fișiere, capacitate, spațiu liber și număr de elemente.
4. Browse directoare în panoul din stânga.
5. Alegeți un element pentru a vizualiza detaliile sale în panoul din dreapta.

Dacă formatul imaginii sau sistemul de fișiere nu este susținut, utilizați **Vizualizare** să inspecteze structura brută.

### Înțelegerea panourilor

Rezumatul de sus descrie imaginea montata si volumul detectat. Panoul din stânga jos conține ierarhia directorului. Tabelul central enumeră elementele din directorul selectat cu numele, data modificării, tipul și dimensiunea. Panoul din dreapta arată detalii pentru elementul selectat.

Disk Explorer nu înseamnă că fiecare pistă brută a fost decodată perfect. Utilizați rezumatul volumului și numărul de elemente ca o verificare rapidă a plauzibilității, apoi deschide fișierele reprezentative sau comparați-le cu o listă de directoare cunoscute atunci când precizia de conservare contează.

### Când nu apare nimic

Mai întâi confirmați că calea imaginii este corectă. Apoi verificați mașina detectată și formatul. O imagine validă poate conține un sistem de fișiere nesuportat sau deteriorat, caz în care exploratorul poate rămâne gol chiar dacă **Vizualizare** arată date înregistrate. Nu suprascrieți sau aruncați imaginea sursă bazată doar pe un explorator gol.

## Utilizarea instrumentelor

ă **Unelte** grupuri tab Greaseweazle operațiuni de întreținere.

<p align="center"><img src="../images/main-tools-en.png" alt="Tools tab" width="78%"></p>

Selectaţi o comandă din lista din stânga, revizuiţi parametrii săi, apoi faceţi clic pe **Execută**. Comenzile distructive sau de schimbare a hardware-ului ar trebui utilizate numai după verificarea controler-ului selectat și conduce.

Majoritatea dialogurilor de instrumente contin trei domenii: parametrii de sus, o zona de stare si de iesire bruta in centru, si comanda generata in partea de jos. @ info: whatsthis În mod normal, un parametru necontrolat înseamnă să nu modificați această valoare, iar un parametru verificat include această valoare în comandă.

Dialogurile individuale de diagnostic sunt descrise în [Diagnosticare și întreținere hardware](#hardware-diagnostics-and-maintenance).

## Emulare

### Deschiderea unei mașini salvate

ă **Emulare ** liste de file salvate configuraţii. Alegeți unul și faceți clic ** Deschide**Fiecare mașină de rulare apare în propria filă.

<p align="center"><img src="../images/main-emulation-welcome-en.png" alt="Ecran de bun venit emulație" width="78%"></p>

Creează și editează mașini în **Opțiuni > Emulare > Configurații ** şi ** Opțiuni > Emulare > Amiga**.

Dacă nu apare nicio configurație, creați mai întâi una în Opțiuni. O configurare salvată combină modelul de mașină, versiunea emulator, ROM, memorie, video, audio, stocare, și cartografii de intrare. Salvarea unei configuraţii nu o porneşte; reveniţi la principal **Emulare ** tab și clic ** Deschide**.

### Comenzile mașinilor-unelte

<p align="center"><img src="../images/main-emulation-running-en.png" alt="Mașină emulată" width="78%"></p>

Bara de unelte a mașinii de rulare oferă comenzi de putere, pauză, resetare, stare de salvare, stare de încărcare, captură și afișare. De asemenea, arată:

- comenzile rapide și rapide configurate;
- Fabricant activ, cum ar fi Direct3D 11;
- scurtăturile cu ecran complet și cu eliberare de șoarece;
- starea audio, controler și mouse-ului;
- rezoluția actuală, rata de actualizare și rata de cadru.

Banda de disc din partea de jos a ecranului de emulare gestionează medii detașabile pentru fiecare unitate emulată. Sarcinile de tastatură pot fi schimbate în **Opțiuni > Emulare > Scurtături**, în timp ce tastatura emulată, mouse-ul, și cartografiere controler sunt configurate în corespunzătoare Amiga File.

### Referință bară de unelte

| Grup de control | Scop |
|---|---|
| Putere și pauză | Începe, se oprește, pauze, sau reia mașina emulată |
| Resetează controalele | Execută acțiunea de resetare ușoară sau dură configurată |
| Controalele de stat | Salvează sau încarcă o stare de emulator pentru continuarea rapidă |
| Captură | Salvează o imagine a ecranului emulat |
| Afișează | Modificați prezentarea ecranului sau intrați pe ecran complet |
| Reamintire rapidă | Arată comenzile rapide de salvare/încărcare active |
| Renderer | Raportează suportul video activ |
| Reamintire intrare | Arată comenzi rapide cu ecran complet și eliberare mouse-uri |
| Indicatoare de dispozitiv | Rapoarte audio, controler, și starea mouse-ului |
| Performanță | Raportează dimensiunea de ieșire, frecvența de reîmprospătare și rata de cadru |

### Lăsând ecran complet sau eliberând mouse-ul

Bara de instrumente afișează cheile alocate în prezent. În configurația ilustrată, **Alt. Înapoi ** comută ecranul complet și ** F12** Eliberează şoarecele. Tratează valorile afișate ca autoritate deoarece comenzile rapide pot fi redistribuite.

### Folosind media floppy

Banda de drive identifică fiecare unitate emulată, cum ar fi `DF0:`. Utilizați controalele sale media pentru a introduce, înlocui, sau ejecta o imagine. Înlocuirea mediilor schimbă doar maşina de funcţionare introdus disc; nu schimbă definiţia dispozitivului de stocare în maşina salvată decât dacă acţiunea este salvată în mod explicit.

## Opțiuni de aplicare

Deschide **Opțiuni** din fereastra principală pentru a configura aplicația.

### Generale

<p align="center"><img src="../images/options-general-en.png" alt="Opțiuni generale" width="72%"></p>

ă **Generale** tab conţine:

- dosarul implicit al imaginii de disc;
- limba și tema interfeței;
- generarea numelui de fișier-etichetă pentru conversii;
- modele de etichete personalizate predefinite și recente;
- un exemplu de nume de fișier live.

Variabilele etichetării includ numele sursei, familia, formatul, extensia, data și ora. Utilizați butonul de resetare pentru a restabili modelul implicit.

Actualizările previzualizarea fișierului înainte de crearea oricărui fișier. Utilizați-l pentru a detecta separatoare duplicate, extensii lipsă, sau nume ambigue. Modelele personalizate recente oferă acces rapid la sistemele de denumire anterioare fără a înlocui setul curent.

### Jurnale

<p align="center"><img src="../images/options-logs-en.png" alt="Opțiuni jurnal" width="72%"></p>

Logging-ul poate fi configurat independent pentru fiecare operațiune. Pentru fiecare categorie, alegeți dacă să salvați jurnalele, setați o dimensiune maximă a fișierului și decideți dacă jurnalele anterioare ar trebui păstrate. O dimensiune de `0` înseamnă nelimitat. **Deschide dosarul** deschide dosarul jurnal curent.

Activează **Păstrați jurnalele anterioare** pentru lucrări de conservare și diagnosticare în cazul în care istoria mai multor încercări contează. Dezactivează-l atunci când doar cel mai recent rezultat este util. Limitele maxime de dimensiune se aplică în cazul stocării jurnalelor, nu în cazul imaginilor pe disc capturate.

### Controloare și unități

<p align="center"><img src="../images/options-controllers-and-drives-en.png" alt="Controloare și unități" width="72%"></p>

Utilizați această filă pentru:

- scanarea controlorilor conectați;
- adăugați și eliminați configurațiile motorului;
- selectați dimensiunea, densitatea și viteza motorului;
- salvează setările hardware;
- alege sau găsi automat `gw.exe`;
- verifică și descarcă Greaseweazle Host Tools actualizări;
- restabilește o cale executabilă configurată anterior.

Setări hardware salvate rămân disponibile atunci când o unitate este temporar deconectată.

#### Adăugare unitate

1. Click **Scanează** şi aşteaptă să apară controlorii conectaţi.
2. Click **Adaugă o unitate** dacă unitatea necesară nu este deja listată.
3. Selectați numărul său logic de unitate, dimensiunea fizică, densitatea de înregistrare, și viteza de rotație.
4. Salvați rândul.
5. Confirmă că arată **Disponibil ** şi ** Configurat**.

Utilizați controlul gunoiului doar pentru a elimina configurația salvată; nu deconectează hardware-ul. Dacă acelaşi controler apare pe un alt COM babord mai târziu, scanați din nou înainte de a presupune că portul stocat este încă valabil.

#### Gestionarea Greaseweazle Host Tools

**Caută gw.exe ** Caută locaţii cunoscute. ** Alege ** alege un executabil specific. ** Verificați actualizările ** întrebări versiuni disponibile fără a înlocui una instalată. ** Descărcați ultima versiune ** instalează pachetul curent selectat și ** Folosește calea anterioară ** restabilește locația configurată anterior. După schimbarea executabilului, executați ** Informații privind controlorul** pentru a confirma că versiunea selectată poate comunica cu operatorul.

### Motoare

<p align="center"><img src="../images/options-engines-en.png" alt="Selectarea motorului" width="72%"></p>

Alegeți motorul independent pentru citire, scriere, conversie și Disk Explorer. Motorul selectat este utilizat strict: dacă nu poate efectua operațiunea solicitată; GW GUI raportează limitarea în loc să schimbe în tăcere motoarele.

Această independenţă este intenţionată. De exemplu, citirile fizice pot folosi Greaseweazle Host Tools în timp ce conversia imaginii și explorarea utilizează motorul intern. Înregistrați opțiunile motorului într-un profil sau notă de proiect atunci când reproductibilitatea contează.

### Profile

<p align="center"><img src="../images/options-profiles-en.png" alt="Profile" width="72%"></p>

Profilele păstrează setările reutilizabile pentru operațiunile de citire, scriere și conversie. Alegeți categoria relevantă pentru a gestiona profilurile sale. Un profil selectat este afişat în bara de stare a ferestrei principale şi în ecranele de operare.

Utilizați profiluri pentru fluxuri de lucru repetabile mai degrabă decât ca colecții inexplicabile de steaguri expert. Da fiecare profil un nume specific scopului, cum ar fi o anumită unitate, familie de discuri, sau metoda de recuperare. Revizuiți un profil după actualizarea motorului suport, deoarece opțiunile susținute se pot modifica.

## Opțiuni de emulare

ă **Emulare** opţiunile conţin setările generale de stocare, comenzile rapide globale, configuraţiile salvate şi setările specifice maşinilor.

### Dosare generale de emulare

<p align="center"><img src="../images/options-emulation-general-en.png" alt="Opțiuni generale de emulare" width="72%"></p>

Setează dosarul de stocare emulație comună și dosarele implicite pentru capturi și stări salvate. **Deschide dosarul** deschide locația comună în File Explorer.

Păstrați capturi și stări salvate în dosare separate. O captură este o imagine obișnuită; o stare salvată conține starea de mașină specifică emulatorului și poate depinde de versiunea emulator și de configurația care a creat-o. Back up configurare și media alături de state importante salvate.

### Scurtături globale

<p align="center"><img src="../images/options-emulation-shortcuts-en.png" alt="Scurtături de emulație" width="72%"></p>

Căutaţi o acţiune sau o misiune cheie, atribuiţi sau eliminaţi comenzi rapide, restabiliţi implicit, şi conflicte clare. Coloana privind statutul identifică sarcinile valabile și contradictorii.

Pentru a schimba o scurtătură, găsiţi acţiunea, faceţi clic **Atribuiți **, și apăsați combinația cheie dorită. Verificați starea înainte de închidere Opțiuni. ** Conflicte clare ** elimină sarcinile contradictorii; nu restabilește cartografierea implicită. Utilizare ** Restaurare implicite** atunci când doriți să înlocuiți sarcinile personalizate cu setul standard.

### Configurații salvate

<p align="center"><img src="../images/options-emulation-configurations-en.png" alt="Configurații de emulație salvate" width="72%"></p>

Această pagină enumeră mașinile salvate. Alegeți o configurație pentru a o edita în **Amiga** tab. Puteți reîmprospăta lista sau șterge configurația selectată.

Eliminarea unei configuraţii elimină definiţia salvată a maşinii. Acesta nu trebuie utilizat ca modalitate de a ejecta media sau de a închide o mașină de rulare. Înainte de ștergere, notați orice ROM, imagine hard-disc, și fișiere de stat asociate cu configurația.

## Amiga configurare

Interfața curentă oferă detalii Amiga Pagini de configurare. Aceeași structură de setări poate fi extinsă pentru alte sisteme emulate fără modificarea fluxului de lucru principal.

### Generale

<p align="center"><img src="../images/options-amiga-general-en.png" alt="Amiga configurări generale" width="72%"></p>

Alege Amiga model, salvaţi configurarea, instalaţi sau înlocuiţi versiunea emulator, şi defini foldere implicite pentru hard disk-uri şi alte medii. **Versiune căutare** Interoghează sursa oficială de emulator-versiune.

Începe cu modelul pentru că limitează paginile ulterioare. Schimbarea ei poate modifica disponibilul CPU, memorie, ROM, chipset, și opțiuni de stocare. După selectarea unei versiuni de emulator, salvați configurația înainte de a o lansa din fereastra principală. Instalarea unei alte versiuni de emulator înlocuiește versiunea utilizată de această configurație; nu creează o a doua copie a mașinii.

### CPU

<p align="center"><img src="../images/options-amiga-cpu-en.png" alt="Amiga CPU configurări" width="72%"></p>

ă CPU pagina prezintă procesorul selectat de modelul de mașină și oferă o precizie compatibilă; FPU, și alegeri de viteză. Opțiunile care nu se aplică modelului selectat rămân dezactivate.

- **CPU model** identifică procesorul emulat.
- **Precizie** controlează modelul de sincronizare. Modurile de calcul al ciclului favorizează compatibilitatea hardware-ului, însă necesită mai multă procesare a gazdelor.
- **FPU** permite o unitate de punct plutitor compatibilă atunci când este susținută.
- **CPU viteza** selectează calendarul original sau un mod accelerat.

Pentru o configurație de bază, păstrați modelul derivat CPU şi viteza originală. Se modifică accelerația numai după ce cizmele mașinii corect la setările standard.

### RAM

<p align="center"><img src="../images/options-amiga-ram-en.png" alt="Amiga RAM configurări" width="72%"></p>

Configurează Chip RAM, Încet RAM, Repede RAM, și a sprijinit memoria de expansiune. Mesajele de compatibilitate explică restricțiile pentru mașina selectată, iar memoria configurată totală este afișată în partea de jos.

**Chip RAM ** este accesibil cipurilor personalizate și este solicitat de platformă. ** Încet. RAM ** reprezintă memoria de expansiune compatibilă utilizată de configuraţiile comune. ** Rapid RAM ** este memorie de expansiune orientată spre procesor. ** Zorro III RAM** se aplică numai modelelor care sprijină această arhitectură de extindere. Mesajele de compatibilitate și controalele dezactivate împiedică combinațiile pe care modelul selectat nu le poate reprezenta.

### ROM

<p align="center"><img src="../images/options-amiga-rom-en.png" alt="Amiga ROM configurări" width="72%"></p>

Alegeți sistemul Kickstart ROM, opțional extins ROM, și ROM Cheia. Detectat-ROM lista afișează numele, revizuirile și compatibilitatea cu modelul selectat. Alegeți un detector ROM și faceți clic **Utilizare**, sau naviga la un fișier manual.

ROM fișierele nu sunt furnizate de GW GUIUtilizaţi ROM-uri care vă sunt permise legal să utilizaţi.

Lista detectată este preferabilă presupunerii dintr-un nume de fișier: raportează ROM identitatea și revizuirea și evaluează compatibilitatea cu modelul selectat. **Compatibil ** este alegerea normală; ** Parţial compatibil ** indică faptul că ROM poate boot, dar nu se potrivește exact cu mașina. ** Reîmprospătează ** rescanează configuratul ROM locaţii. ** Utilizare** atribuie detectate selectate ROM la configuraţie.

### Video

<p align="center"><img src="../images/options-amiga-video-en.png" alt="Amiga configurări video" width="72%"></p>

Configurați standardul video, raportul de aspect, rezoluție, modul linie, bordură, rander, adâncimea culorii, cadru sărind peste, gama, și pâlpâire de fixare. Setări suplimentare chipset sunt disponibile mai jos pagina atunci când este susținută de modelul selectat.

| Setare | Efect practic |
|---|---|
| Standard video | Selectează PAL sau NTSC calendarul și comportamentul de reîmprospătare preconizat; |
| Raportul Aspect | Controlează modul în care imaginea emulată este scalată |
| Rezoluția | Selectează detalii de ieșire automate sau explicite |
| Modul linie | Controlează tratamentul producției duble sau interconectate |
| Limitele culturilor | Elimină suprascanul neutilizat numai atunci când este activat |
| Închiriere | Alege platforma grafică |
| Adâncime culoare | Selectează precizia culorii de ieșire |
| Sărire cadru | Reduce cadrele redate atunci când sunt activate |
| Gamma | Reglează răspunsul la luminozitate |
| Fixator pentru flicker | Procesează moduri care altfel ar pâlpâi vizibil |

Schimbă setările de ecran pe rând. În cazul în care fereastra de emulare devine goală sau instabilă, reveniți la rezoluția automată, saritura de cadru dezactivată, gama neutră și randorul care a lucrat anterior.

### Audio

<p align="center"><img src="../images/options-amiga-audio-en.png" alt="Amiga configurări audio" width="72%"></p>

Activează sau dezactivează audio, alege dispozitivul de ieșire și latență, apoi configurează interpolarea; Amiga filtrare, tip filtru, separare stereo, sunet floppy-drive, și CD-audio volum.

Latența inferioară reduce întârzierea, dar poate provoca abandonuri pe un calculator ocupat. Măreşte-l dacă se sparge audio. Interpolarea și Amiga Filtrul audio schimbă reproducerea sunetului în loc să emuleze logica programului. Volumul motorului controlează sunetul mecanic simulat separat de cel normal Amiga audio.

### Depozitare

<p align="center"><img src="../images/options-amiga-storage-en.png" alt="Amiga setările de stocare" width="72%"></p>

Pagina de stocare enumeră identificatorii dispozitivelor, tipurile, modelele, media asociată și acțiunile disponibile. Adaugă, configurează sau elimină dispozitivele aici. Discurile și CD-urile floppy pot fi introduse sau înlocuite direct de la o mașină de rulare.

ă **Identificatorul dispozitivului ** este modul în care sistemul emulat se adresează dispozitivului. ** Tip ** distinge dispozitivele floppy, hard-disk, optice și alte dispozitive suport. ** Model ** descrie hardware-ul emulat; ** Media asociată** identifică imaginea atribuită în prezent. Configurați dispozitivul înainte de a asocia medii scriebile valoroase, și să păstreze copii de rezervă de imagini hard-disc.

### Tastatură

<p align="center"><img src="../images/options-amiga-keyboard-en.png" alt="Amiga configurări tastatură" width="72%"></p>

Caută Amiga chei și misiuni gazdă, atribui noi chei, elimina cartografii, restabili implicit, sau conflicte clare. Coloana de stare raportează dacă fiecare misiune este valabilă.

Coloana din stânga numește emulat Amiga cheie; **Asociaţie** arată combinaţia cheie a gazdei. O cartografiere validă poate fi încă incomodă dacă Windows sau aplicația rezervă aceeași scurtătură, astfel încât să se testeze combinații critice în interiorul mașinii de rulare. Evitați atribuirea scurtătură mouse-ul de eliberare sau fullscreen la o cheie de care software-ul emulat are nevoie frecvent.

### Șoarece

<p align="center"><img src="../images/options-amiga-mouse-en.png" alt="Amiga configurări mouse" width="72%"></p>

Setează viteza fizică a mouse-ului, alege care stick analogic controlează mouse-ul, reglează zona analogică moartă și viteza, și configurează cartografiile mouse-ului. Restaurarea cazurilor implicite sau a conflictelor clare de cartografiere atunci când este necesar.

Cresteti zona moarta daca un controler cauzeaza deriva pointer. Reglaţi viteza pe stânga şi pe dreapta în mod independent atunci când ambele beţe sunt activate. Tabelul de cartografiere inferioară asociază intrările cu butoanele sau acțiunile mouse-ului; inspectează starea sa de conflict după schimbarea cartografiilor controlerului în altă parte.

### Controlori

<p align="center"><img src="../images/options-amiga-controllers-en.png" alt="Amiga setări controler" width="72%"></p>

Detectează controlorii conectați, atribuie dispozitive și tipuri de controler Amiga porturi, și configurați cartografiere controler și setările turbo-fire. Opțiunile disponibile depind de hardware-ul detectat și de mașina selectată.

Portul 1 şi Portul 2 sunt configurate independent. **Automat** controler tip este un punct de pornire sensibil, dar software-ul așteaptă un anumit joystick sau mouse-ul poate necesita un tip explicit. Rulați detectarea înainte de a atribui un controler nou conectat. Focul turbo activează în mod repetat o intrare cartografiată și ar trebui să rămână dezactivat, cu excepția cazului în care jocul sau aplicația beneficiază de ea.

## Diagnosticare și întreținere hardware

Aceste dialoguri sunt deschise din **Unelte ** tab. Fiecare dialog previzualizare generată Greaseweazle Comandă. Revizuiți-l înainte de click ** Execută**.

### Informații privind controlorul

<p align="center"><img src="../images/tool-controller-information-en.png" alt="Informații privind controlorul" width="62%"></p>

Afișează informațiile raportate de operatorul selectat. Extinde **Producția brută** atunci când aveți nevoie de răspunsul de comandă completă.

Foloseşte asta ca prima comandă de diagnosticare. Un răspuns de succes confirmă că GW GUI poate porni instrumentul gazdă configurat executabil și comunica cu dispozitivul selectat. Înregistrați informațiile despre firmware și hardware înainte de a efectua o actualizare.

### USB lățime de bandă

<p align="center"><img src="../images/tool-usb-bandwidth-en.png" alt="USB lățime de bandă" width="62%"></p>

Măsuri disponibile USB banda de bandă de comunicare. Utilizați-l pentru a diagnostica transferuri instabile sau nepotrivite USB Conexiune.

Închide alt software folosind controlorul înainte de testare. Se repetă măsurarea după schimbarea USB port, cablu sau hub. Comparați rezultatele în condiții similare, în loc să tratați o singură măsurătoare ca garanție absolută.

### Viteza de rulare

<p align="center"><img src="../images/tool-drive-speed-en.png" alt="Viteza de rulare" width="62%"></p>

Măsoară viteza de rotaţie. Creșterea numărului de măsurători atunci când aveți nevoie de un rezultat mai reprezentativ.

O singură măsurătoare este o verificare rapidă; mai multe măsurători arată dacă viteza este stabilă. Lasă motorul să atingă viteza normală înainte de a interpreta rezultatul. O valoare neașteptată poate indica o viteză configurată greșită, o problemă mecanică sau o problemă de configurare a măsurătorilor.

### Caută cap

<p align="center"><img src="../images/tool-seek-head-en.png" alt="Caută cap" width="62%"></p>

Mută capul de unitate într-un cilindru selectat. **Permite cilindrilor extremi ** permisele de ședere în mod normal restricționate; și ** Menţineţi motorul activ** lasă motorul pornit în timpul operațiunii. Utilizați poziții extreme numai atunci când procedura hardware necesită în mod explicit acestea.

Căutarea normală este utilă pentru a confirma mișcarea capului sau poziționarea înainte de un diagnostic. Ascultați pentru efecte anormale repetate și opriți în cazul în care cilindrul solicitat este nepotrivit pentru unitatea. Acest instrument nu citește sau validează datele din cilindrul de destinație.

### Diagnosticul alinierii motorului

<p align="center"><img src="../images/tool-drive-alignment-en.png" alt="Diagnosticul alinierii motorului" width="62%"></p>

Rulează citiri repetate pentru analiza de aliniere. Acesta susține selectarea liniei, revoluție și numărătoare de citire, format de decodare, flux brut, index, viteză, PLL, density-pin, hard-sector, TG43, și opțiuni inverse de date. Activitatea de aliniere necesită mijloace de referinţă adecvate şi cunoştinţe hardware.

Începeți cu un disc de referință cunoscut și cel mai mic set de suprascrieri. **Urme alternante ** definește urmele și capetele eșantionate; ** Revoluții pe pistă ** controlează fiecare durată a eșantionului; ** Numărul de citiri** determină repetarea. Activează o definiție personalizată a discului sau un format de decodare numai atunci când se potrivește cu media de referință. Opțiuni cum ar fi indicele fals, sectoare dure, PLL suprascrieri, pini de densitate și TG43 sunt specifice hardware-ului sau formatului și pot invalida o comparație atunci când este utilizată incorect.

### Piulițe pentru scule

<p align="center"><img src="../images/tool-hardware-pins-en.png" alt="Piulițe pentru scule" width="62%"></p>

Citeşte sau schimbă un ac de control. Alegeți acul, activați **Schimbare pin ** numai atunci când scrieți o valoare și selectați ** Nivel ridicat** atunci când este necesar pentru funcționarea hardware-ului prevăzut.

Cu **Schimbare pin** dezactivat, comanda interoghează pinul. Aceasta este starea de nerambursare mai sigură. Schimbarea unui nivel afectează direct controlorul I/O și ar trebui să se facă numai cu corect Greaseweazle documentaţie hardware şi cabluri ataşate.

### Controlor de resetare

<p align="center"><img src="../images/tool-reset-controller-en.png" alt="Controlor de resetare" width="62%"></p>

Resetează Greaseweazle Controler. Utilizați acest lucru atunci când controlorul este detectat, dar nu mai răspunde în mod normal.

Aşteptaţi ca orice operaţiune activă de disc să se termine înainte de resetare. După aceea, scanați din nou controlorul dacă starea conexiunii sale nu se recuperează automat. O resetare nu repară o eroare `gw.exe` calea sau o cale deconectată USB dispozitiv.

### Întârzieri

<p align="center"><img src="../images/tool-delays-en.png" alt="Întârzieri ale controlorului" width="62%"></p>

Citeşte sau modifică valorile de timp ale controlerului, inclusiv selecţia, pasul şef, se stabilească, motor, deselecţie automată, sincronizarea scrierii şi întârzierile cu masca index. Activaţi numai valorile pe care intenţionaţi să le modificaţi.

Câmpurile necontrolate lasă neschimbată valoarea corespunzătoare a controlorului. Înainte de editare, înregistraţi valorile existente. Modificările de sincronizare pot afecta fiecare operațiune fizică ulterioară, astfel încât testul cu medii consumabile și de a restabili valori cunoscute-bună dacă comportamentul devine nesigur.

### Firmware

<p align="center"><img src="../images/tool-firmware-en.png" alt="Actualizare firmware" width="62%"></p>

Actualizează firma de control. **Actualizează bootloader** este marcat în mod explicit ca fiind riscant și ar trebui să rămână dezactivat, cu excepția cazului în care procedura oficială de firmware o cere. Nu deconectați controlerul în timpul unei actualizări.

Înainte de actualizare, confirmați controlerul conectat cu **Informații privind controlorul**, utilizaţi un sistem stabil direct USB Conexiune, și închide alte software-ul care ar putea accesa. După finalizarea, reconectați sau rescanați operatorul și citiți din nou informațiile sale pentru a verifica versiunea firmware raportată.

## Jurnale și istoricul operațiunilor

Deschide istoricul operaţiunii pentru a inspecta jurnalele salvate prin operaţiune.

<p align="center"><img src="../images/operation-history-en.png" alt="Istoricul operațiunii" width="68%"></p>

Alegeți un jurnal din stânga pentru a-i afișa conținutul. **Exportă** păstrează o copie pentru diagnostic sau suport. Căile și liniile de comandă pot conține nume de dosare personale, astfel încât să revizuiască jurnalele exportate înainte de a le partaja.

Consola live din fereastra principală arată comanda curentă și ieșire recentă. Butonul copiat copiază textul afișat.

### Citirea unui jurnal

Un jurnal de diagnosticare util conține comanda generată, marcaje de timp, ieșire motor, și starea finală. Lucrează de jos în sus: identifică eroarea finală, apoi localizează primul avertisment sau piesa eșuată care a precedat-o. Un eşec generic ulterior este adesea doar consecinţa unui mesaj anterior, mai specific.

Atunci când se compară două încercări, verificați dacă operatorul, unitatea, motorul, profilul, calea de sursă, formatul de ieșire, și argumente expert au fost identice. În caz contrar, un rezultat diferit poate reflecta setări modificate mai degrabă decât instabilitatea discului.

## Date de aplicare și utilizare portabilă

GW GUI păstrează datele utilizatorului separat de binarele aplicației. În funcție de pachetul și modul selectat, setările, jurnalele, instrumentele descărcate, componentele emulatoare, capturile, stările și configurațiile mașinilor sunt stocate fie în aplicație `Data` directorul sau în locaţiile configurate de utilizator-date.

Înainte de a înlocui sau muta o instalație portabilă, țineți dosarul complet de aplicare împreună și înapoi în sus `Data` Dosar. Nu muta fișierele individuale din `lib`, deoarece cererea își rezolvă propriile biblioteci și terțe părți din această structură.

### Conţinutul de rezervă sugerat

Înapoi la următoarele atunci când acestea sunt importante pentru fluxul de lucru:

- setările și profilurile de aplicație;
- definițiile operatorului și ale conducerii;
- configuraţii de emulare;
- ROM căi și deținute în mod legal ROM copii de rezervă;
- imagini hard-disk și detașabile-media;
- Statele capturate și salvate;
- jurnalele de operare utilizate ca înregistrări de conservare.

Imaginile de disc pot fi mult mai mari decât setările. Magazin de masterat de arhivare citit-doar atunci când este posibil, și de lucru pe copii.

## Fluxuri de lucru recomandate

### Arhivarea unui disc necunoscut

1. Inspectaţi şi curăţaţi unitatea folosind o procedură de întreţinere adecvată.
2. Protejați discul dacă este posibil.
3. Alegeți **Citiţi > Imagine brută (SCP)**.
4. Utilizați un nume de fișier descriptiv și citiți gama normală de cale cu mai multe revoluții.
5. Revizuiţi consola şi salvaţi jurnalul.
6. Inspectaţi ambele părţi în **Vizualizare**.
7. Conversia unei copii în formate sectoriale probabile.
8. Testați copiile convertite în **Disk Explorer** sau software-ul adecvat.
9. Păstrați maestrul brut, jurnal, și note împreună.

### Recrearea unui disc dintr-o imagine

1. Inspectaţi imaginea şi confirmaţi-i familia şi formatul.
2. Se introduce un disc de unică folosință sau care poate fi scris intenționat de mărimea și densitatea corectă.
3. Deschide **Scrie** și selectați imaginea.
4. Confirmați unitatea configurată și formatul detectat.
5. Scrie discul.
6. Citiți-l înapoi la o imagine de verificare separată.
7. Compară conţinutul decodat şi verifică vizual urmele suspecte.

### Crearea unui emular Amiga

1. Deschide **Opțiuni > Emulare > Configurații** și de a crea sau selecta o mașină.
2. În **Amiga > Generale**, alege modelul și versiunea emulator.
3. Atribuiți un acord compatibil, obținut în mod legal ROM.
4. Păstrează modelul implicit pentru CPU şi RAM pe primul boot.
5. Configurează video și audio cu setări automate conservatoare.
6. Adăugaţi dispozitive de stocare şi asociaţi imagini media copiate.
7. Review tastatură, mouse-ul, și misiuni controler.
8. Salvează configurația.
9. Înapoi la **Emulare **, selectați-l și faceți clic pe ** Deschide**.
10. Numai după o pornire de bază de succes, modifica accelerația sau setările avansate unul la un moment dat.

## Lista de verificare privind siguranța

Înainte **Citit**:

- discul sursă este în unitatea corectă;
- sursa este protejată în scris, dacă este posibil;
- calea de ieșire nu va suprascrie un maestru existent;
- profilul și gama de cale se potrivesc discului.

Înainte **Scrie ** sau ** Șterge**:

- discul de destinație poate fi distrus;
- imaginea și unitatea sunt corecte;
- dimensiunea și densitatea discului sunt compatibile;
- Nici un maestru al arhivalului nu este folosit ca destinaţie.

Înaintea unui instrument de schimb hardware:

- nicio altă operațiune nu se desfășoară;
- operatorul corect este selectat;
- valorile curente au fost înregistrate;
- operatorul are putere stabilă și USB conectivitatea;
- acțiunea este susținută de documentația hardware.

## Depanare

### Controlorul nu este listat

1. Reconectează controlerul direct la computer.
2. Deschide **Opțiuni > Controloare și unități**.
3. Click **Scanează**.
4. Verifica starea controlerului și configurarea motorului.
5. Fugi! **Informații privind controlorul** dacă detectarea reușește, dar comenzile nu reușesc.

Dacă încă nu apare, încercaţi un alt direct USB Port și cablu, apoi rescan. Verificați Windows Device Manager pentru un dispozitiv serial nou detectat. Un controler vizibil pentru Windows, dar absent de la GW GUI de obicei, indică un port ocupat, configurare vechi, sau problema Host Tools; un controler absent din Windows puncte la USB, putere, conducător auto, sau hardware.

### `gw.exe` nu poate fi găsit

Deschide **Opțiuni > Controloare și unități **, apoi utilizaţi ** Caută gw.exe **, ** Alege **, sau ** Descărcați ultima versiune**. Confirmați că calea detectată indică spre planificat Greaseweazle instalare.

După selectarea acestuia, executați **Informații privind controlorul**. Dacă acest lucru nu reuşeşte înainte de a contacta hardware-ul, inspectaţi jurnalul pentru o cale executabilă invalidă, fişiere lipsă, sau o versiune care nu poate începe.

### O operațiune folosește motorul greșit

Deschide **Opțiuni > Motoare** și verificați motorul atribuit acestei operațiuni exacte. GW GUI nu cade în tăcere înapoi la celălalt motor.

Setările motorului sunt separate: schimbarea motorului de conversie nu modifică citirea, scrierea sau Disk Explorer. Redeschide operaţiunea eşuată după salvarea opţiunii şi confirmă comanda generată în consolă.

### O imagine nu este recunoscută

Dezactivează detectarea automată numai dacă știi mașina și formatul corecte. În caz contrar, încercaţi **Vizualizare** fila pentru a inspecta imaginea la un nivel inferior.

Verificați dacă sursa este o captare a fluxului brut, o imagine sectorială, un recipient comprimat sau un fișier fără legătură cu o extensie înșelătoare. Nu redenumiți niciodată o extensie doar pentru detectarea forței; conversia trebuie să interpreteze corect structura sursei.

### Emularea nu începe

Verificați configurația salvată, versiunea emulator instalată, selectată ROM, căi de stocare, și compatibilitate model. Revizuiți jurnalul de aplicații pentru detaliile complete ale erorilor.

Întoarcerea temporară CPU, RAM, video, și de stocare la un model simplu compatibil de bază. În cazul în care linia de bază începe, restabiliți un set personalizat la un moment dat. O stare salvată creată cu o altă versiune de emulator sau definiția mașinii poate, de asemenea, să nu reușească chiar și atunci când funcționează o boot curat.

### O scurtătură sau intrare nu funcționează

Verificaţi ambele global **Emulare > Scurtături** pagina și tastatura, mouse-ul sau pagina de control. Rezolva orice misiune marcată ca fiind conflictuală.

Dacă mouse-ul este capturat, utilizați scurtătură de eliberare afișată în bara de unelte de funcționare. Dacă un controler a fost conectat după deschiderea opțiunilor, executați din nou detectarea controlerului înainte de a-l atribui.

### O comandă eşuează pe neaşteptate.

1. Citiți ieșirea consolei live.
2. Deschide **Istoricul operațiunii** pentru jurnalul complet salvat.
3. Confirmați controler selectat, unitate, profil, motor, și căi de fișiere.
4. Exportă jurnalul relevant dacă trebuie să fie partajat pentru diagnostic.

### Crackles sau pauze audio

Crește latența audio emulație, închide CPU- aplicatii intensive, si returneaza cadru video sarind si accelerand la valorile anterioare. Verificați dacă dispozitivul audio Windows este selectat. Se modifică un set la un moment dat, astfel încât corecția eficientă este identificabilă.

### Afişajul de emulare este gol sau lent

Returnare rezoluție și modul linie la **Automat**, dezactivați cadru sărind și pâlpâire de fixare temporar , și încercați redler anterior de lucru . Confirmă că configurat ROM și media de boot introdusă sunt valabile. ă FPS Indicatorul ajută la distingerea unei probleme de redare-performanță de o mașină care pur și simplu nu a booted.

### O citire conține piese instabile

Repetați citire la un nou nume de fișier, creșteți revoluțiile, după caz, și comparați piesele afectate. Curățați capetele de unitate folosind o procedură corectă și inspectați discul pentru daune fizice. A nu se citi în mod repetat scurgerile vizibile sau mediile deteriorate, deoarece alte pasaje pot agrava.

## Glosar

| Termen | Semnificație în GW GUI |
|---|---|
| Controlor | ă Greaseweazle interfață hardware conectată peste USB |
| Condu | Drive-ul floppy fizic atașat la controler |
| Motor | Implementarea selectată pentru efectuarea unei operații |
| Flux | Sincronizarea informațiilor reprezentând tranzițiile magnetice citite de pe un disc |
| Imagine brută | O captură care păstrează informații de nivel scăzut pe disc, cum ar fi SCP |
| Imaginea sectorială | O reprezentare decodată organizată în sectoare logice |
| Revoluție | O rotație completă eșantionată în timp ce citiți o pistă |
| Cilindru | O poziție radială a capului; un cilindru poate conține o pistă pe fiecare parte |
| Cap | Partea discului selectată de unitatea fizică |
| Profil | Un set reutilizabil de setări pentru o operațiune |
| ROM | Imagine firmware necesară de o mașină emulată |
| Stare salvată | Un instantaneu al unui emulator rulează starea mașinii |
| Renderer | Platforma grafică folosită pentru a afișa ieșirea de emulare |

## Referinţă rapidă

| Dacă vrei să... | Du-te la... |
|---|---|
| Păstrați un disc fizic | **Citit** |
| Pune o imagine înapoi pe un disc | **Scrie** |
| Produce un alt format de imagine | **Conversie** |
| Inspectează urmele sau anomaliile fluxului | **Vizualizare** |
| Navighează fișierele din interiorul unei imagini | **Disk Explorer** |
| Verificare comunicare controler | **Unelte > Informații privind controlorul** |
| Măsurarea rotației motorului | **Unelte > Viteza de rulare** |
| Revizuiţi o comandă anterioară | **Istoricul operațiunii** |
| Configurează hardware-ul | **Opțiuni > Controloare și unități** |
| Alegeți implementarea | **Opțiuni > Motoare** |
| Creează sau editează o mașină emulată | **Opțiuni > Emulare** |
| Porniți o mașină salvată | **Emulare** |
