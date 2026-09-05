[🌐 Languages / Langues](../Home.md)

# GW GUI Przewodnik dla użytkowników

GW GUI jest aplikacją Windows do czytania, pisania, konwersji, kontroli i emulowania obrazów dyskietek. Może kontrolować Greaseweazle sprzęt, pracować z plikami disk- image poprzez jego silnik wewnętrzny, i uruchomić zapisane konfiguracji emulated-machine.

Niniejszy przewodnik opisuje interfejs angielski pokazany w aktualnej wersji aplikacji. Jest on zapisany jako źródło drukowanej instrukcji użytkownika: zrzuty ekranu ilustrują regulatory, podczas gdy otaczający tekst wyjaśnia, co wybrać, dlaczego wybrać i jak zweryfikować wynik.

> **Ważne:** Czytanie dysku jest nieniszczące. Pisanie, usuwanie, aktualizacja oprogramowania firmowego oraz niektóre narzędzia sprzętowe mogą modyfikować media lub sprzęt. Przeczytaj ostrzeżenie dołączone do odpowiedniej procedury przed kliknięciem ** Wykonaj**.

### Jak stosować ten przewodnik

Jeśli to twój pierwszy raz GW GUI, complete [Rozpoczęcie](#getting-started), następnie po [Czytanie dysku](#reading-a-disk)Jeśli aplikacja jest już skonfigurowana, przejdź bezpośrednio do rozdziału dla operacji, którą chcesz wykonać. Rozdziały opcji służą jako punkt odniesienia, gdy procedura wymaga zmiany ustawienia napędu, silnika, profilu lub emulatora maszyny.

Nazwy interfejsu są pokazane w **pogrubiona**. Nazwy plików, ścieżki, polecenia i wartości dosłowne są pokazane jako `code`. Notatki wyjaśniają normalne zachowanie; ostrzeżenia identyfikują operacje, które mogą zmienić dysk, kontroler lub zapisaną konfigurację.

## Spis treści

1. [Zrozumienie przepływu pracy](#understanding-the-workflow)
2. [Zaczynając](#getting-started)
3. [Główne okno](#main-window)
4. [Czytanie dysku](#reading-a-disk)
5. [Pisanie dysku](#writing-a-disk)
6. [Konwersja obrazów dysku](#converting-disk-images)
7. [Wizualizacja obrazu dysku](#visualizing-a-disk-image)
8. [Poznaj zawartość dysku](#exploring-disk-contents)
9. [Korzystanie z narzędzi](#using-the-tools)
10. [Emulacja](#emulation)
11. [Opcje aplikacji](#application-options)
12. [Opcje emulacji](#emulation-options)
13. [Amiga konfiguracja](#amiga-configuration)
14. [Diagnostyka i konserwacja sprzętu](#hardware-diagnostics-and-maintenance)
15. [Rejestry i historia operacji](#logs-and-operation-history)
16. [Dane dotyczące zastosowania i zastosowania przenośnego](#application-data-and-portable-use)
17. [Zalecane przepływy pracy](#recommended-workflows)
18. [Lista kontrolna bezpieczeństwa](#safety-checklist)
19. [Rozwiązywanie problemów](#troubleshooting)
20. [Słowniczek](#glossary)
21. [Szybkie odniesienie](#quick-reference)

## Zrozumienie przepływu pracy

GW GUI oddziela operacje dysków fizycznych od operacji plików obrazkowych:

| Cel | Wejście | Wyjście | Zalecana strona |
|---|---|---|---|
| Zachowaj dyskietkę | Dysk fizyczny | Plik obrazka | **Czytaj** |
| Odtworzyć dyskietkę | Plik obrazka | Dysk fizyczny | **Napisz** |
| Zmień format obrazka | Plik obrazka | Jeden lub więcej plików obrazkowych | **Konwersja** |
| Ślady kontroli i anomalie | Plik obrazka | Analiza wzrokowa | **Wizualizacja** |
| Przeglądanie plików przechowywanych na obrazku | Obsługiwany system obrazowania / pliku | Pliki i katalogi | **Disk Explorer** |
| Rysuj napęd lub sterownik | Greaseweazle sprzęt | Pomiary lub status | **Narzędzia** |
| Uruchom zapisaną maszynę wirtualną | Zapisana konfiguracja maszyny | Sesja emulacji | **Emulacja** |

Dla zachowania, najpierw zrobić surowe schwytanie i zachować go bez zmian jako mistrz. Utwórz konwertowane lub naprawione kopie robocze od tego mistrza. Unika to powtarzania odczytu fizycznego i zachowuje informacje, których format oparty na sektorach może nie zachować.

## Zaczynając

### Wymagania

- Okna z Microsoft .NET Desktop Runtime wymagany przez aplikację.
- A Greaseweazle sterownik do operacji dyskietek fizycznych.
- Skonfigurowana ścieżka `gw.exe` podczas stosowania Greaseweazle Host Tools silnik.
- Legalnie uzyskane ROM pliki, gdy emulowana maszyna ich wymaga.

Aplikacja sprawdza wymagany czas pracy .NET przy starcie. Jeśli brakuje, postępuj zgodnie z instrukcją instalacji, a następnie ponownie uruchom GW GUI.

### Przed podłączeniem sprzętu

Przed rozpoczęciem operacji dysków fizycznych należy sprawdzić:

1. Połącz Greaseweazle sterownik do stajni USB Port.
2. Połączyć kabel dyskietkowy z właściwą orientacją.
3. Podłącz zasilacz napędu przed włożeniem cennych nośników.
4. Potwierdź, że rozmiar dysku i gęstość pasują do dysku.
5. Write- chronić dysk źródłowy, gdy to możliwe.

GW GUI nie może zapobiec uszkodzeniom spowodowanym przez nieprawidłowe okablowanie, nieodpowiednią moc lub mechanicznie niebezpieczny napęd. Najpierw przetestować nieznany sprzęt z dyskiem, który może być zużyty.

### Pierwsze uruchomienie

1. Otwórz `gwgui.exe`.
2. Otwórz **Opcje**.
3. W **Sterowniki i napędy**, skanować kontroler i skonfigurować dysk.
4. Weryfikacja lub wybór ścieżki `gw.exe`.
5. W **Silniki**, wybrać, który silnik powinien wykonać każdą operację.
6. Wróć do głównego okna i wybierz wymaganą kartę operacji.

### Potwierdzenie, że konfiguracja jest gotowa

Ustawienie robocze powinno pokazywać sterownik i napęd w pasku stanu, na przykład numer napędu, rozmiar, gęstość, oraz COM Port. W **Opcje > Sterowniki i napędy **, kontroler powinien być oznaczony ** Dostępne ** i napęd ** Konfiguracja **Biegnij! ** Informacje dotyczące kontrolera** przed czytaniem cennych mediów, jeśli chcesz zweryfikować komunikację bez zmiany dysku.

### Wybór silnika

GW GUI może ujawnić więcej niż jedno wdrożenie dla niektórych operacji. W **Greaseweazle Host Tools** silnik wywołuje skonfigurowane `gw.exe`; wewnętrzne GW GUI uchwyty silnika obsługiwane operacje wewnątrz aplikacji. Wybór silnika jest wyraźny i niezależny dla czytania, pisania, konwersji, oraz Disk Explorer. Jeśli operacja nie jest obsługiwana przez wybrany silnik, GW GUI zgłasza ten stan zamiast automatycznie zmieniać silniki.

## Główne okno

Główne okno grupuje główne operacje na siedem zakładek:

- **Czytaj** tworzy obraz z dysku fizycznego.
- **Napisz** zapisuje obraz na dysk fizyczny.
- **Konwersja** konwertuje jeden format disk- image na jeden lub więcej formatów wyjściowych.
- **Wizualizacja** wyświetla ścieżki i strumień lub dekodowane dane.
- **Disk Explorer** przegląda obsługiwane systemy plików i zawartość dysku.
- **Narzędzia** zapewnia obsługę sprzętu i polecenia diagnostyczne.
- **Emulacja** zarządza i działa zapisane maszyny emulowane.

Konsola na dole wyświetla komendę i jej wyjście. Pasek stanu zgłasza wybrany dysk, profil i aktualny stan.

### Czytanie interfejsu

Większość stron operacyjnych podąża za tym samym wzorem:

1. **Źródło lub miejsce przeznaczenia** steruje identyfikacją dysku, obrazu lub folderu.
2. **Sterowanie formatem** wybrać automatyczne wykrywanie lub wyraźną maszynę i format.
3. **Kontrola profilu** stosować ustawienia wielokrotnego użytku.
4. **Ustawienia zaawansowane** ujawnia parametry, które zazwyczaj są opcjonalne.
5. **Wykonaj** Rozpoczynam operację.
6. W **konsola** wyświetla wygenerowane polecenie, postęp, ostrzeżenia i błędy.

W **Wykonaj** przycisk nie oznacza, że wszystkie wartości są bezpieczne dla włożonego dysku. Zawsze przeglądaj cel i wybrany dysk przed operacją zapisu lub konserwacji.

### Pasek stanu i konsola

Lewa strona paska stanu identyfikuje aktywny napęd fizyczny. Centrum pokazuje aktywny profil po wybraniu. Wskaźnik stanu podaje, czy aplikacja jest gotowa czy zajęta. Konsola nie jest tylko diagnostyką: jest to autorytatywny zapis polecenia wysłanego do wybranego silnika. Użyj jego kontroli kopii, gdy musisz zachować lub podzielić się tym poleceniem.

## Czytanie dysku

Otwórz **Czytaj** zakładka do uchwycenia fizycznego dyskietki jako obrazu.

<p align="center"><img src="../images/main-read-en.png" alt="Przeczytaj kartę" width="78%"></p>

### Procedura podstawowa

1. Wstaw dysk źródłowy do skonfigurowanego napędu.
2. Wybierz typ obrazka:
   - **Obraz surowy (SCP)** zachowuje informacje o poziomie flux- level.
   - **Znany format dysku** tworzy obraz używając wybranej maszyny i formatu.
3. Wybierz folder docelowy.
4. Wprowadź nazwę pliku wyjściowego.
5. W razie potrzeby wybierz profil.
6. Kliknij **Wykonaj**.

Konsola pokazuje dokładne polecenie i postęp. Nie należy usuwać dysku ani rozłączać sterownika aż do zakończenia operacji.

### Wybór typu wyjścia

Stosowanie **Obraz surowy (SCP)** gdy celem jest archiwalne wychwytywanie, analiza, odzysk lub późniejsze przekształcenie. Surowy obraz rejestruje informacje o czasie i wielokrotnych rewolucjach, które są przydatne dla nietypowych formatów, słabych sektorów, systemów ochrony i uszkodzonych mediów.

Stosowanie **Znany format dysku** kiedy już znasz rodzinę dysków i potrzebujesz bezpośrednio użytecznego obrazu sektorowego. Wybór ten może być mniejszy i łatwiejszy do otwarcia w innym oprogramowaniu, ale reprezentuje dekodowany wynik, a nie każdy szczegół obserwowany przez napęd.

Gdy jest niepewny, najpierw stwórz surowy obraz. Możesz przekonwertować go później bez ponownego odczytu dysku.

### Folder, nazwa pliku i profil

W **Katalog ** jest katalogiem docelowym. W ** Nazwa pliku** powinien zidentyfikować dysk bez polegania tylko na jego etykiecie fizycznej. Przydatna nazwa archiwalna zawiera tytuł, numer dysku lub stronę oraz, w stosownych przypadkach, notatkę stanu. Nie dodawaj rozszerzenia formatu, które jest sprzeczne z wybranym formatem wyjścia.

A **Profil ** stosuje zapisany zestaw parametrów odczytu. Wybierz jeden tylko wtedy, gdy wiesz, co zawiera. W ** Domyślne** profil jest odpowiedni dla normalnej pierwszej próby; specjalistyczny profil odzysku może świadomie odczytać więcej obrotów lub inny zakres toru i tym samym trwać dłużej.

### Ustawienia zaawansowane

Rozszerz **Ustawienia zaawansowane** dostęp do parametrów specyficznych dla formatów lub parametrów eksperckich. Pozostawić te wartości bez zmian, chyba że dysk wymaga określonego zakresu toru, liczby obrotów lub opcji sterownika.

Wspólne wartości zaawansowane obejmują:

| Ustawienie | Cel | Kiedy zmienić |
|---|---|---|
| Zakres toru | Limituje cylindry i głowy do czytania | Pojedyncze media, nietypowa geometria, lub ukierunkowane przepustka odzyskiwania |
| Rewolucje | Kontroluje, ile obrotów pobiera się z próby | Zwiększenie dla utworów niestabilnych lub chronionych; zmniejszenie prędkości tylko w stosownych przypadkach |
| Argumenty ekspertów | Przesuwa dodatkowe parametry silnika | Tylko wtedy, gdy następuje udokumentowane Greaseweazle wytyczne |

### Sprawdzanie udanego odczytu

Nie można polegać tylko na braku okna błędu. Po zakończeniu komendy:

1. Potwierdź, że plik wyjściowy istnieje i nie jest pusty.
2. Przeczytaj końcowe linie konsoli dla nieudanych lub brakujących utworów.
3. Otwórz obraz w **Wizualizacja** sprawdzenie, czy obie strony i przewidywany zakres toru zawierają dane.
4. Otwórz. **Disk Explorer** kiedy system plików jest obsługiwany.
5. Zachowaj dziennik operacji z ważnymi uchwytami archiwalnymi.

Jeśli powtarzane odczyty różnią się, zachować każdy surowy przechwytywanie zamiast nadpisać pierwszy. Różnice mogą być przydatne podczas odzyskiwania.

## Zapisywanie dysku

Otwórz **Napisz** zakładka do zapisu istniejącego obrazu na dysk dyskietki fizycznej.

<p align="center"><img src="../images/main-write-en.png" alt="Zapisz kartę" width="78%"></p>

### Procedura podstawowa

1. Wstaw dysk docelowy.
2. Wybierz obraz źródłowy **Przeglądaj**.
3. Potwierdź wykryty format.
4. W razie potrzeby wybierz profil.
5. Kliknij **Wykonaj**.

Zapisywanie zastępuje dane na dysku docelowym. Weryfikacja wybranego napędu i obrazu przed rozpoczęciem.

> **Ostrzeżenie:** Pisanie jest destrukcyjne. Zastępuje dane magnetyczne na dysku docelowym. W miarę możliwości należy używać archiwum źródłowego chronionego przed zapisem i oddzielnego dysku docelowego.

### Przed napisaniem

Sprawdź cztery elementy przed kliknięciem **Wykonaj**:

1. **Obraz:** wybrana ścieżka to zamierzony obraz źródłowy.
2. **Dysk:** dysk na dysku może być bezpiecznie nadpisany.
3. **Napęd:** skonfigurowana wielkość i gęstość odpowiadają środkowi docelowemu.
4. **Format:** automatyczne wykrywanie lub ręcznie wybrany format pasuje do obrazu.

Jeśli obraz źródłowy nie został przetestowany, otwórz go w **Wizualizacja ** lub ** Disk Explorer** Najpierw. Udane napisanie nie może naprawić niekompletnego obrazu źródłowego.

### Kontrola i modyfikacja toru

Po wybraniu obrazu, **Ścieżki wizualizacji ** otwiera swoją reprezentację toru. ** Modyfikuj** Eksponuje obsługiwane modyfikacje obrazu przed napisaniem. Dostępne działania zależą od wybranego formatu i silnika.

### Weryfikacja napisanego dysku

Kiedy silnik obsługuje weryfikację, użyj jej dla ważnych mediów. W przeciwnym razie, przeczytaj zapisany dysk z powrotem do nowego obrazu i porównaj jego dekodowaną zawartość lub sprawdź go w **Wizualizacja**. Zachować zapis weryfikacji oddzielony od oryginalnego obrazu tak, aby oryginał nigdy nie został nadpisany.

Jeśli pisanie nie powiodło się na spójnych torach, sprawdź stan dysku, gęstość, czystość napędu i konfigurację napędu. Jeśli awarie występują losowo, sprawdź USB łączność ze stabilnością i kontrolerem.

## Konwersja obrazów dysku

W **Konwersja** tab konwertuje obraz źródłowy na jeden lub kilka formatów docelowych.

<p align="center"><img src="../images/main-conversion-en.png" alt="Karta konwersji" width="78%"></p>

### Procedura podstawowa

1. Wybierz obraz źródłowy.
2. Opcjonalnie podać nazwy wyjściowe.
3. Wybierz rodzinę maszyn.
4. Wybierz jeden lub więcej formatów wyjściowych i rozszerzeń.
5. Włącz **Dodaj znaczniki** jeśli nazwy plików powinny używać skonfigurowanego wzoru znacznika.
6. Kliknij **Wykonaj**.

W **Wybrane ** panel zawiera listę żądanych wyjść. ** Migracja plików** zapewnia dedykowany przepływ pracy do migracji obsługiwanych plików zamiast wykonywania standardowej konwersji obrazu.

### Wybór formatów

W **Maszyna ** lista filtruje formaty pokazane w ** Format** Panel. Nazwa formatu opisuje układ dysku logicznego; rozszerzenie opisuje pojemnik wyjściowy. Niektóre formaty mogą być reprezentowane przez więcej niż jedno rozszerzenie, a niektóre pojemniki nie mogą zachować każdej cechy surowego źródła.

Wybierz tylko wyjścia, których potrzebujesz. Wiele formatów jest przydatnych przy tworzeniu archiwalnego mistrza, kopii kompatybilnej z emulatorem oraz kopii dla innego narzędzia do analizy w jednej operacji.

### Nazwy wyjściowe i znaczniki

**Nazwy produktów ** pozwala na kontrolę nazw podstawowych generowanych dla wybranych formatów. ** Dodaj znaczniki ** stosuje ustawiony wzór nazwy pliku ** Opcje > Ogólne**. Znaczniki mogą kodować rodzinę, format, rozszerzenie, datę lub czas. Podgląd przykładu w Opcjach przed przekształceniem dużej partii tak, że pliki są nazwane konsekwentnie.

### Sprawdzanie wyników konwersji

Dla każdego żądanego wyjścia:

1. Potwierdź, że plik został utworzony.
2. Sprawdź w konsoli, czy utwory lub sektory, które nie mogły zostać odkodowane.
3. Otwórz wynik w **Disk Explorer** jeśli zawiera obsługiwany system plików.
4. Porównaj przewidywaną pojemność dysku i zawartość ze źródłem.

Konwersja może zakończyć się podczas zgłaszania utraty informacji, która jest nieodłącznie związana z formatem docelowym. Zachować oryginalny obraz surowy, nawet jeśli obraz przekształcony wydaje się poprawny.

## Wizualizacja obrazu dysku

W **Wizualizacja** zakładka wyświetla strukturę i dystrybucję danych obrazu.

<p align="center"><img src="../images/main-visualization-en.png" alt="Karta wizualizacji" width="78%"></p>

1. Kliknij **Otwórz obraz dysku**.
2. Zachowaj **Automatyczne wykrywanie** włączone, lub wybrać maszynę i format ręcznie.
3. Stosowanie **Powiększenie łącza** utrzymać obie strony na tym samym poziomie zoomu.
4. Stosowanie **Reset** aby przywrócić widok początkowy.
5. Otwórz **Inspektor** Szczegółowe informacje na temat wybranego regionu.

Legenda rozróżnia normalny strumień, krótkie i długie przejścia, nagłówki, dekodowane dane i wykryte anomalie. Surowy obraz może zawierać dane, których nie można rozszyfrować w znanym systemie plików, ale które nadal mogą być sprawdzone tutaj.

### Interpretacja widoku

Każdy duży okrągły panel reprezentuje jedną stronę dysku. Centrum identyfikuje bok i jego aktualny stan danych; pozycje koncentryczne odpowiadają torom. Kolory klasyfikują wykryte regiony zgodnie z legendą. Visualizer ma odpowiadać na pytania takie jak:

- Czy obraz zawiera dane z jednej strony czy z obu stron?
- Czy spodziewane ślady są obecne?
- Czy anomalie są izolowane czy powtarzane na dysku?
- Czy automatyczna detekcja zidentyfikowała wiarygodną maszynę i format?

Kolor anomalii jest powodem do sprawdzenia regionu, a nie dowodem na to, że dysk jest bezużyteczny. Kopiowanie ochrony, niestandardowe formatowanie, słabe nagrywanie i uszkodzony sektor mogą tworzyć różne struktury, które wymagają interpretacji kontekstowej.

### Zalecany ciąg inspekcji

Zacznij od podłączonego zoomu umożliwiającego porównanie obu stron w tej samej skali. Wybierz podejrzany region, otwórz **Inspektor** i porównać z sąsiednimi torami. Jeśli wynik wydaje się być problemem z wykrywaniem, wyłączyć automatyczne wykrywanie i wybrać znaną maszynę i format. Powrót do automatycznej detekcji po teście, tak aby wymuszone ustawienie nie było przypadkowo używane do innego obrazu.

## Przeglądanie zawartości dysku

W **Disk Explorer** zakładka przegląda obsługiwane obrazy dysku jako hierarchię plików.

<p align="center"><img src="../images/main-disk-explorer-en.png" alt="Disk Explorer zakładka" width="78%"></p>

1. Otwórz istniejący obraz lub przeczytaj dysk.
2. Zachowaj **Automatyczne wykrywanie** włączone, chyba że trzeba wymusić maszynę lub format.
3. Przegląd informacji o wolumenie: system, ochrona, system plików, pojemność, wolna przestrzeń i ilość elementów.
4. Przeglądaj katalogi w lewym panelu.
5. Wybierz element, aby wyświetlić jego szczegóły w prawym panelu.

Jeżeli format obrazu lub system plików nie są obsługiwane, użyj **Wizualizacja** by zamiast tego sprawdzić pierwotną strukturę.

### Zrozumienie paneli

Górne podsumowanie opisuje zamontowany obraz i wykrytą objętość. Panel po lewej stronie zawiera hierarchię katalogów. Centralna tabela zawiera pozycje w wybranym katalogu z nazwą, datą modyfikacji, typem i wielkością. Prawy panel pokazuje szczegóły dla wybranego elementu.

Disk Explorer nie oznacza, że każdy surowy utwór został doskonale odkodowany. Użyj podsumowania wolumenu i liczenia pozycji jako szybkiego sprawdzenia wiarygodności, a następnie otwórz reprezentatywne pliki lub porównaj je ze znaną listą katalogów, gdy liczy się dokładność zachowania.

### Kiedy nic się nie pojawia

Najpierw potwierdź, że ścieżka obrazu jest poprawna. Sprawdź wykrytą maszynę i format. Ważny obraz może zawierać nieobsługiwany lub uszkodzony system plików, w którym to przypadku odkrywca może pozostać pusty, chociaż **Wizualizacja** pokazuje zarejestrowane dane. Nie należy nadpisywać ani odrzucać obrazu źródłowego w oparciu tylko o pusty odkrywca.

## Używanie narzędzi

W **Narzędzia** grupy kart Greaseweazle czynności konserwacyjne.

<p align="center"><img src="../images/main-tools-en.png" alt="Zakładka Narzędzia" width="78%"></p>

Wybierz polecenie z listy po lewej, przejrzyj jego parametry, a następnie kliknij **Wykonaj**. Destrukcyjne lub hardware- change polecenia powinny być używane tylko po sprawdzeniu wybranego sterownika i napędu.

Większość dialogów narzędzi zawiera trzy obszary: parametry na górze, status i obszar wyjściowy raw- w centrum oraz wygenerowane polecenie na dole. Podgląd poleceń zmienia się jako opcje. Niesprawdzony parametr zazwyczaj oznacza "nie modyfikuj tej wartości", podczas gdy sprawdzony parametr zawiera tę wartość w poleceniu.

Indywidualne dialogi diagnostyczne opisane są w [Diagnostyka i konserwacja sprzętu](#hardware-diagnostics-and-maintenance).

## Emulacja

### Otwieranie zapisanej maszyny

W **Emulacja ** tab list zapisane konfiguracje. Wybierz jeden i kliknij ** Otwórz**. Każda uruchomiona maszyna pojawia się we własnej karcie.

<p align="center"><img src="../images/main-emulation-welcome-en.png" alt="Emulacyjny ekran powitalny" width="78%"></p>

Tworzenie i edycja maszyn w **Opcje > Emulacja > Konfiguracje ** oraz ** Opcje > Emulacja > Amiga**.

Jeśli nie pojawi się żadna konfiguracja, najpierw utwórz jedną w Opcjach. Zapisana konfiguracja łączy model maszyny, wersję emulatora, ROM, pamięci, wideo, audio, pamięci masowej i mappings wejściowych. Zapisywanie konfiguracji nie uruchamia go; powrót do głównego **Emulacja ** tab i kliknij ** Otwórz**.

### Sterowanie maszynami do biegania

<p align="center"><img src="../images/main-emulation-running-en.png" alt="Maszyna do emulowania biegu" width="78%"></p>

Pasek narzędzi running- machine zapewnia sterowanie mocą, pauzą, resetem, stanem bezpieczeństwa, stanem obciążenia, wychwytywaniem i wyświetlaniem. Pokazuje również:

- skonfigurowane skróty quick- save i quick- load;
- aktywny renderer, takich jak Direct3D 11;
- skróty pełnoekranowe i ustne;
- stan dźwięku, kontrolera i myszy;
- aktualna rozdzielczość, szybkość odświeżania i szybkość ramki.

Taśma dyskowa na dole wyświetlacza emulacji zarządza usuwalnymi nośnikami dla każdego emulowanego napędu. Przydziały klawiszowe można zmienić w **Opcje > Emulacja > Skróty**, podczas emulowania klawiatury, myszy i sterownik mappings są skonfigurowane w odpowiednich Amiga zakładki.

### Odniesienie do paska narzędzi

| Grupa kontrolna | Cel |
|---|---|
| Moc i pauza | Uruchomienie, zatrzymanie, przerwa lub wznowienie maszyny emulowanej |
| Regulacje resetowania | Wykonuje skonfigurowaną akcję resetowania miękkiego lub twardego |
| Kontrole państwowe | Oszczędza lub ładuje stan emulatora dla szybkiej kontynuacji |
| Uchwyt | Zaoszczędza obraz ekranu emulowanego |
| Wyświetl | Zmienia prezentację wyświetlacza lub wchodzi na pełnoekranowy ekran |
| Przypominanie stanu Quick- state | Pokazuje aktywne skróty zapisu / obciążenia |
| Renderer | Reports the active video backend |
| Przypominanie wejścia | Wyświetla skróty pełnoekranowe i mouse- release |
| Wskaźniki urządzeń | Zgłasza stan dźwięku, kontrolera i myszy |
| Wydajność | Zgłasza wielkość wyjścia, częstotliwość odświeżania i częstotliwość ramki |

### Opuszczenie pełnoekranowego ekranu lub zwolnienie myszy

Pasek narzędzi wyświetla aktualnie przypisane klucze. W ilustrowanej konfiguracji, **Alt + Powrót ** przełącza pełnoekranowy i ** F12** uwalnia mysz. Traktuj wyświetlane wartości jako autorytatywne, ponieważ skróty mogą zostać przeniesione.

### Używanie dyskietek

Pas napędowy identyfikuje każdy napęd emulowany, taki jak: `DF0:`. Użyj jego sterowniki mediów do wstawiania, wymiany lub wyrzucenia obrazu. Zastąpienie nośnika zmienia tylko wbudowany dysk uruchomionej maszyny; nie zmienia definicji urządzenia storage- urządzenia w zapisanej maszynie, chyba że działanie to jest wyraźnie zapisane.

## Opcje aplikacji

Otwórz **Opcje** z głównego okna aby skonfigurować aplikację.

### Ogólne

<p align="center"><img src="../images/options-general-en.png" alt="Opcje ogólne" width="72%"></p>

W **Ogólne** zakładka zawiera:

- domyślny folder disk- image;
- język interfejsu i temat;
- generacja znaczników filename- dla konwersji;
- predefiniowane i ostatnio niestandardowe wzory znaczników;
- przykład nazwy pliku na żywo.

Zmienne znaczników zawierają nazwę źródła, rodzinę, format, rozszerzenie, datę i czas. Użyj przycisku reset, aby przywrócić domyślny wzór.

Przed utworzeniem jakichkolwiek plików uaktualnia się nazwa pliku. Użyj go do wykrycia duplikowanych separatorów, brakujących rozszerzeń lub niejednoznacznych nazw. Najnowsze niestandardowe wzorce zapewniają szybki dostęp do wcześniejszych programów nazewnictwa bez zastępowania bieżącego ustawienia.

### Logi

<p align="center"><img src="../images/options-logs-en.png" alt="Opcje dziennika" width="72%"></p>

Logowanie można skonfigurować niezależnie dla każdej operacji. Dla każdej kategorii należy wybrać, czy zapisać dzienniki, ustawić maksymalny rozmiar pliku i zdecydować, czy poprzednie dzienniki powinny być zachowane. Rozmiar `0` oznacza nieograniczony. **Otwórz folder** otwiera bieżący katalog dziennika.

Włącz **Przechowuj poprzednie dzienniki** do prac konserwatorskich i diagnostycznych, gdzie historia kilku prób ma znaczenie. Wyłącz go, gdy przydatny jest tylko ostatni wynik. Maksymalne limity wielkości odnoszą się do pamięci dziennej, a nie do nagranych obrazów dysku.

### Sterowniki i napędy

<p align="center"><img src="../images/options-controllers-and-drives-en.png" alt="Sterowniki i napędy" width="72%"></p>

Użyj tej zakładki, aby:

- skanowanie podłączonych kontrolerów;
- dodaje i usuwa konfiguracje napędu;
- wybrać rozmiar, gęstość i prędkość napędu;
- zapisywanie ustawień sprzętowych;
- wybrać lub automatycznie znaleźć `gw.exe`;
- sprawdzić i pobrać Greaseweazle Host Tools aktualizacje;
- przywracanie wcześniej skonfigurowanej ścieżki wykonywalnej.

Zapisane ustawienia sprzętowe pozostają dostępne, gdy napęd jest tymczasowo odłączony.

#### Dodawanie napędu

1. Kliknij **Przeskanuj** i czekać, aż pojawią się podłączone sterowniki.
2. Kliknij **Dodaj dysk** jeżeli wymagany napęd nie jest jeszcze wymieniony.
3. Wybierz jej logiczny numer napędu, rozmiar fizyczny, gęstość zapisu i prędkość obrotową.
4. Ocal rząd.
5. Potwierdź, że to pokazuje **Dostępne ** oraz ** Konfiguracja**.

Użyj kontroli śmieci tylko do usunięcia zapisanej konfiguracji; nie odłącza sprzętu. Jeśli ten sam kontroler pojawia się na innym COM port później, skanować ponownie przed założeniem, że zapisany port jest nadal ważny.

#### Zarządzanie Greaseweazle Host Tools

**Znajdź gw.exe ** poszukiwania znanych miejsc. ** Wybierz ** wybiera konkretny wykonywalny. ** Sprawdź aktualizacje ** Pytania dostępne wersje bez wymiany zainstalowanego. ** Pobierz najnowszą wersję ** instaluje wybrany bieżący pakiet oraz ** Użyj poprzedniej ścieżki ** Przywraca wcześniej skonfigurowaną lokalizację. Po zmianie wykonywalnego, uruchom ** Informacje dotyczące kontrolera** potwierdzenie, że wybrana wersja może komunikować się ze sterownikiem.

### Silniki

<p align="center"><img src="../images/options-engines-en.png" alt="Wybór silnika" width="72%"></p>

Wybierz silnik niezależnie do czytania, pisania, konwersji i Disk Explorer. Wybrany silnik jest używany ściśle: jeśli nie może wykonać żądanej operacji, GW GUI zgłasza ograniczenie zamiast cichego przełączania silników.

Ta niezależność jest zamierzona. Na przykład, odczyty fizyczne mogą używać Greaseweazle Host Tools podczas gdy konwersja i eksploracja obrazu używają wewnętrznego silnika. Wybór silnika w profilu lub nocie projektowej, gdy ma znaczenie odtwarzalność.

### Profile

<p align="center"><img src="../images/options-profiles-en.png" alt="Profile" width="72%"></p>

Profile przechowują ustawienia wielokrotnego użytku dla operacji odczytu, zapisu i konwersji. Wybierz odpowiednią kategorię do zarządzania profilami. Wybrany profil jest wyświetlany w pasku stanu głównego okna oraz w ekranach operacyjnych.

Stosować profile do powtarzalnych przepływów pracy, a nie jako niewyjaśnione zbiory flag ekspertów. Podać każdemu profilu konkretną nazwę, taką jak dysk, rodzina dysków lub metoda odzyskiwania. Przegląd profilu po aktualizacji silnika bazowego, ponieważ obsługiwane opcje mogą się zmienić.

## Opcje emulacji

W **Emulacja** opcje zawierają ogólne ustawienia pamięci masowej, skróty globalne, zapisane konfiguracje oraz ustawienia specyficzne dla maszyny.

### Katalogi emulacji ogólnej

<p align="center"><img src="../images/options-emulation-general-en.png" alt="Ogólne opcje emulacji" width="72%"></p>

Ustaw folder współdzielony pamięci masowej emulacji oraz domyślne foldery dla przechwytywania i zapisywania stanów. **Otwórz folder** otwiera współdzieloną lokalizację w pliku Explorer.

Zachowaj przechwytywanie i zapisywanie stanów w oddzielnych folderach. Uchwyt jest zwykłym obrazem; zapisany stan zawiera stan maszyny specyficzny dla emulatora i może zależeć od wersji emulatora i konfiguracji, która go stworzyła. Kopia zapasowa i media obok ważnych zapisanych stanów.

### Skróty globalne

<p align="center"><img src="../images/options-emulation-shortcuts-en.png" alt="Skróty emulacyjne" width="72%"></p>

Szukaj akcji lub przypisania klucza, przypisz lub usuń skróty, przywróć domyślne i jasne konflikty. Kolumna statusu określa ważne i sprzeczne zadania.

Aby zmienić skrót, znajdź akcję, kliknij **Przypisz **, i naciśnij żądaną kombinację kluczy. Sprawdź status przed zamknięciem Opcje. ** Wyraźne konflikty ** usuwa sprzeczne zadania; nie przywraca domyślnego mapowania. Stosowanie ** Przywróć domyślne** gdy chcesz zastąpić niestandardowe zadania standardowym zestawem.

### Zapisane konfiguracje

<p align="center"><img src="../images/options-emulation-configurations-en.png" alt="Zapisane konfiguracje emulacji" width="72%"></p>

Ta strona zawiera listę zapisanych maszyn. Wybierz konfigurację do edycji **Amiga** Tab. Możesz odświeżyć listę lub usunąć wybraną konfigurację.

Usuwanie konfiguracji usuwa zapisaną definicję maszyny. Nie należy go używać jako sposobu na wyrzucenie nośników lub zamknięcie maszyny. Przed usunięciem należy zwrócić uwagę na wszelkie ROM, obraz dysku twardego i pliki stanu związane z konfiguracją.

## Amiga konfiguracja

Bieżący interfejs dostarcza szczegółowych informacji Amiga strony konfiguracyjne. Tę samą strukturę ustawień można rozszerzyć na inne systemy emulowane bez zmiany głównego przepływu pracy.

### Ogólne

<p align="center"><img src="../images/options-amiga-general-en.png" alt="Amiga Ustawienia ogólne" width="72%"></p>

Wybierz Amiga model, zapisać konfigurację, zainstalować lub zastąpić wersję emulatora i zdefiniować domyślne foldery dysków twardych i innych mediów. **Wersja wyszukiwania** Pyta oficjalne źródło emulator- wersja.

Zacznij od modelu, ponieważ ogranicza późniejsze strony. Zmiana może zmienić dostępne CPU, pamięć, ROM, chipset, i opcje przechowywania. Po wybraniu wersji emulatora, zapisz konfigurację przed uruchomieniem z głównego okna. Instalacja innej wersji emulatora zastępuje wersję używaną w tej konfiguracji; nie tworzy drugiej kopii maszyny.

### CPU

<p align="center"><img src="../images/options-amiga-cpu-en.png" alt="Amiga CPU Ustawienia" width="72%"></p>

W CPU strona pokazuje procesor wybrany przez model maszyny i zapewnia kompatybilną precyzję, FPUi wybór prędkości. Opcje, które nie mają zastosowania do wybranego modelu pozostają wyłączone.

- **CPU model** identyfikuje procesor emulowany.
- **Precyzja** kontroluje model czasowy. Tryby Cycle- dokładne sprzyjają kompatybilności sprzętu, ale wymagają więcej przetwarzania hosta.
- **FPU** umożliwia kompatybilną jednostkę floating- point podczas obsługi.
- **CPU prędkość** wybiera oryginalny czas lub tryb przyspieszony.

Dla konfiguracji bazowej należy zachować model pochodny CPU i oryginalnej prędkości. Zmień przyspieszenie tylko wtedy, gdy buty maszyny są prawidłowo ustawione.

### RAM

<p align="center"><img src="../images/options-amiga-ram-en.png" alt="Amiga RAM Ustawienia" width="72%"></p>

Konfiguruj chip RAMPowoli RAM, Szybko RAMi obsługiwana pamięć rozszerzająca. Komunikaty zgodności wyjaśniają ograniczenia dla wybranej maszyny, a całkowita skonfigurowana pamięć jest wyświetlana na dole.

**Chip RAM ** jest dostępny dla własnych żetonów i jest wymagany przez platformę. ** Powoli RAM ** reprezentuje kompatybilną pamięć rozszerzającą używaną przez wspólne konfiguracje. ** Szybko RAM ** jest zorientowaną na procesy pamięcią rozszerzającą. ** Zorro III RAM** ma zastosowanie tylko do modeli wspierających architekturę rozszerzającą. Komunikaty kompatybilności i stery wyłączone uniemożliwiają kombinacje, których wybrany model nie może reprezentować.

### ROM

<p align="center"><img src="../images/options-amiga-rom-en.png" alt="Amiga ROM Ustawienia" width="72%"></p>

Wybierz system Kickstart ROM, rozszerzony opcjonalnie ROMoraz ROM Klucz. Wykryto...ROM lista wyświetla nazwy, poprawki i kompatybilność z wybranym modelem. Wybierz wykryty ROM i kliknij **Stosowanie**, lub przeglądać plik ręcznie.

ROM pliki nie są dostarczane przez GW GUI. Korzystanie z ROM masz prawo używać.

Wykryta lista jest preferowana do zgadywania z nazwy pliku: raportuje ROM identyfikacja i weryfikacja oraz ocena zgodności z wybranym modelem. **Kompatybilny ** jest normalnym wyborem; ** Częściowo kompatybilne ** wskazuje, że ROM może uruchomić, ale nie dokładnie pasuje do maszyny. ** Odśwież ** reskanuje skonfigurowaną ROM lokalizacje. ** Stosowanie** przypisuje zaznaczony wykryty ROM do konfiguracji.

### Wideo

<p align="center"><img src="../images/options-amiga-video-en.png" alt="Amiga Ustawienia wideo" width="72%"></p>

Skonfiguruj standard wideo, proporcje, rozdzielczość, tryb linii, graniczny cropping, renderer, głębokość kolorów, ramka skipping, gamma, i utrwalanie migotania. Dodatkowe ustawienia chipset są dostępne dalej w dół strony przy wsparciu przez wybrany model.

| Ustawienie | Efekt praktyczny |
|---|---|
| Standard wideo | Wybór PAL lub NTSC czas i spodziewane zachowanie odświeżania |
| Współczynnik oceny | Kontroluje jak emulowany obraz jest skalowany |
| Rozdzielczość | Wybór automatycznego lub wyraźnego szczegółu wyjścia |
| Tryb linii | Steruje obróbką przeplatanych lub podwojonych linii wyjściowych |
| Granice upraw | Usuwa niewykorzystany nadskan tylko wtedy, gdy jest włączony |
| Renderowanie | Wybiera tło graficzne |
| Głębokość koloru | Wybór precyzji kolorów wyjściowych |
| Ramka skip | Zmniejsza wyświetlane ramki po włączeniu |
| Gamma | Dostosowuje odpowiedź jasności |
| Ficker fixer | Procesy, które w przeciwnym razie wyraźnie migoczą |

Zmień jedno ustawienie ekranu na raz. Jeśli okno emulacji stanie się puste lub niestabilne, należy powrócić do automatycznej rozdzielczości, wyłączone ramki skip, neutralne gamma, i wcześniej działający renderer.

### Audio

<p align="center"><img src="../images/options-amiga-audio-en.png" alt="Amiga Ustawienia audio" width="72%"></p>

Włącz lub wyłącz dźwięk, wybierz urządzenie wyjściowe i opóźnienie, a następnie skonfiguruj interpolację, Amiga filtrowanie, filtrowanie typu, separacja stereo, dźwięk dysków i głośność dźwięku CD-.

Niższe opóźnienie zmniejsza opóźnienie, ale może powodować wypadki na zajętym komputerze. Zwiększ, jeśli dźwięk pęknie. Interpolacja i Amiga Filtr audio raczej zmienia odtwarzanie dźwięku niż emuluje logikę programu. Objętość dźwięku steruje symulowanym dźwiękiem mechanicznym oddzielnie od normalnej Amiga audio.

### Przechowywanie

<p align="center"><img src="../images/options-amiga-storage-en.png" alt="Amiga ustawienia pamięci" width="72%"></p>

Strona przechowywania zawiera listę identyfikatorów urządzeń, typów, modeli, powiązanych mediów i dostępnych działań. Dodaj, skonfiguruj lub usuń urządzenia tutaj. Dyskietki i płyty CD mogą być wstawiane lub wymieniane bezpośrednio z maszyny do biegania.

W **identyfikator urządzenia ** tak układ emulowany adresuje urządzenie. ** Typ ** wyróżnia dyskietki, dyskietki, urządzenia optyczne i inne obsługiwane urządzenia. ** Wzór ** opisuje sprzęt emulowany, podczas gdy ** Media powiązane** identyfikuje aktualnie przypisany obraz. Konfiguracja urządzenia przed łączeniem cennych nośników zapisu i utrzymanie kopii zapasowych obrazów twardych.

### Klawiatura

<p align="center"><img src="../images/options-amiga-keyboard-en.png" alt="Amiga Ustawienia klawiatury" width="72%"></p>

Szukaj Amiga klawisze i przydziały hosta, przypisywanie nowych kluczy, usuwanie mapowania, przywracanie domyślnych lub jasnych konfliktów. Kolumna stanu podaje, czy każde przypisanie jest ważne.

Lewa kolumna określa emulowane Amiga klucz; **Stowarzyszenie** pokazuje kombinację klucza hosta. Prawidłowe mapowanie może nadal być niewygodne, jeśli system Windows lub aplikacja rezerwuje ten sam skrót, więc testować krytyczne kombinacje wewnątrz uruchomionej maszyny. Unikaj przypisywania skrótu mouse- release lub pełnoekranowego do klucza, którego emulowane oprogramowanie często potrzebuje.

### Mysz

<p align="center"><img src="../images/options-amiga-mouse-en.png" alt="Amiga Ustawienia myszy" width="72%"></p>

Ustaw fizyczną prędkość myszki, wybierz, który przycisk analogowy kontroluje mysz, dostosować analogową martwą strefę i prędkość, i skonfigurować mappings działania myszy. Przywracanie domyślnych lub czyszczenie konfliktów w razie potrzeby.

Zwiększ strefę śmierci, jeśli kontroler spowoduje dryf wskaźnika. Dostosuj prędkość lewo- i prawy-stick niezależnie, gdy oba kije są włączone. Dolna tabela mapowania łączy wejścia hosta z przyciskami lub akcjami myszki; sprawdza status konfliktu po zmianie mapowania kontrolera gdzie indziej.

### Sterowniki

<p align="center"><img src="../images/options-amiga-controllers-en.png" alt="Amiga Ustawienia kontrolera" width="72%"></p>

Wykrywanie podłączonych sterowników, przypisywanie urządzeń i typów sterowników Amiga porty i konfigurować mappings kontrolera i ustawienia ognia turbo-. Dostępne opcje zależą od wykrytego sprzętu i wybranej maszyny.

Port 1 i Port 2 są skonfigurowane niezależnie. **Automatyczne** typ sterownika jest rozsądnym punktem wyjścia, ale oprogramowanie oczekujące określonego joysticka lub myszy może wymagać wyraźnego typu. Uruchom wykrywanie przed przydzieleniem nowego kontrolera. Turbo fire wielokrotnie aktywuje mapowane wejście i powinno pozostać wyłączone, chyba że gra lub aplikacja z niego korzysta.

## Diagnostyka i konserwacja sprzętu

Te dialogi są otwarte z **Narzędzia ** Tab. Każde okno dialogowe przedstawia wygenerowane Greaseweazle Rozkaz. Przegląd przed kliknięciem ** Wykonaj**.

### Informacje dotyczące kontrolera

<p align="center"><img src="../images/tool-controller-information-en.png" alt="Informacje dotyczące kontrolera" width="62%"></p>

Wyświetla informacje przekazywane przez wybrany kontroler. Rozszerz **Produkcja surowa** gdy potrzebujesz pełnej odpowiedzi komendy.

Użyj tego jako pierwszego polecenia diagnostycznego. Udana odpowiedź potwierdza, że GW GUI może uruchomić skonfigurowane narzędzia host i komunikować się z wybranym urządzeniem. Przed wykonaniem aktualizacji rejestruje informacje o sprzęcie i sprzęcie.

### USB przepustowość

<p align="center"><img src="../images/tool-usb-bandwidth-en.png" alt="USB przepustowość" width="62%"></p>

Środki dostępne USB przepustowość komunikacyjna. Użyj go do diagnozowania niestabilnych transferów lub nieodpowiednich USB połączenie.

Zamknij inne oprogramowanie za pomocą kontrolera przed testowaniem. Powtórzyć pomiar po zmianie USB port, kabel lub węzeł. Porównaj wyniki w podobnych warunkach zamiast traktować jeden pomiar jako gwarancję absolutną.

### Prędkość napędu

<p align="center"><img src="../images/tool-drive-speed-en.png" alt="Prędkość napędu" width="62%"></p>

Mierzy prędkość obrotową napędu. Zwiększ liczbę pomiarów, gdy potrzebujesz bardziej reprezentatywnego wyniku.

Pojedynczy pomiar to szybki sprawdzian; kilka pomiarów pokazuje, czy prędkość jest stabilna. Niech napęd osiągnie normalną prędkość przed interpretacją wyniku. Nieoczekiwana wartość może wskazywać niewłaściwą prędkość konfigurowaną, problem mechaniczny lub problem z ustawieniem pomiaru.

### Szukaj głowy

<p align="center"><img src="../images/tool-seek-head-en.png" alt="Szukaj głowy" width="62%"></p>

Przesuwa głowicę napędową do wybranego cylindra. **Pozwól na ekstremalne cylindry ** dopuszcza zwykle ograniczone pozycje, oraz ** Utrzymać aktywny silnik** pozostawia silnik uruchomiony podczas operacji. Użyj skrajnych pozycji tylko wtedy, gdy procedura sprzętowa wyraźnie ich wymaga.

Normalne poszukiwanie jest przydatne do potwierdzenia ruchu głowy lub pozycjonowania przed diagnostyką. Słuchać nienormalnych powtarzających się uderzeń i zatrzymać, jeśli wymagany cylinder jest nieodpowiedni dla napędu. Narzędzie to nie odczytuje ani nie weryfikuje danych w cylindrze docelowym.

### Diagnostyka ustawienia napędu

<p align="center"><img src="../images/tool-drive-alignment-en.png" alt="Diagnostyka ustawienia napędu" width="62%"></p>

Powtórzone odczyty do analizy osiowania pojazdu. Obsługuje selekcję utworów, rewolucję i liczbę odczytów, format dekodowania, strumień surowców, indeks, prędkość, PLL, density- pin, hard- sector, TG43oraz opcje danych zwrotnych. Dostosowanie wymaga odpowiednich nośników referencyjnych i wiedzy sprzętowej.

Zacznij od znanego dysku referencyjnego i najmniejszego zestawu nadwozi. **Naprzemienne tory ** określa tory i głowy, z których pobrano próbki; ** Obroty na tor ** kontroluje czas trwania każdej próbki; ** Liczba odczytów** określa powtarzanie. Włącz własną definicję dysku lub format dekodowania tylko wtedy, gdy pasuje do nośnika referencyjnego. Opcje takie jak fałszywy indeks, twarde sektory, PLL przejazdy, zawleczki, oraz TG43 są twardy- lub format- specyficzne i może unieważnić porównanie, gdy używane nieprawidłowo.

### Szpilki sprzętowe

<p align="center"><img src="../images/tool-hardware-pins-en.png" alt="Szpilki sprzętowe" width="62%"></p>

Odczytuje lub zmienia obsługiwany pin kontrolera. Wybierz pin, włącz **Zmień pin ** tylko przy zapisie wartości i wybierz ** Wysoki poziom** gdy jest to wymagane przez zamierzoną eksploatację sprzętu.

Z **Zmień pin** Wyłączony, komenda sprawdza zawleczkę. To jest najbezpieczniejsza wartość domyślna. Zmiana poziomu bezpośrednio wpływa na kontroler I / O i powinna być wykonywana tylko z prawidłowym Greaseweazle dokumentacja sprzętowa i dołączony napęd.

### Resetuj sterownik

<p align="center"><img src="../images/tool-reset-controller-en.png" alt="Resetuj sterownik" width="62%"></p>

Resetuje Greaseweazle kontroler. Użyj tego, gdy kontroler zostanie wykryty, ale nie reaguje normalnie.

Przed ponownym ustawieniem należy poczekać na jakiekolwiek aktywne działanie dysku. Następnie ponownie przeskanuj kontroler, jeśli jego stan nie odzyska się automatycznie. Reset nie naprawia błędów `gw.exe` ścieżka lub odłączony USB urządzenie.

### Opóźnienia

<p align="center"><img src="../images/tool-delays-en.png" alt="Opóźnienia kontrolera" width="62%"></p>

Odczyty lub zmiany wartości czasowych sterownika, w tym wybór, krok, ugoda, silnik, automatyczne deselekcja, czas zapisu i maska indeksu opóźnienia. Włącz tylko wartości, które zamierzasz zmodyfikować.

Niezaznaczone pola pozostawiają odpowiednią wartość kontrolera bez zmian. Przed edycją zapisuje istniejące wartości. Zmiany w czasie mogą mieć wpływ na każdą późniejszą operację fizyczną, tak więc testować przy użyciu zbędnych nośników i przywrócić dobre wartości, jeśli zachowanie staje się niewiarygodne.

### Oprogramowanie

<p align="center"><img src="../images/tool-firmware-en.png" alt="Aktualizacja oprogramowania firmowego" width="62%"></p>

Aktualizuje oprogramowanie sterujące. **Aktualizuj bootloader** są wyraźnie oznaczone jako ryzykowne i powinny pozostać wyłączone, chyba że wymaga tego oficjalna procedura oprogramowania firmowego. Nie rozłączaj kontrolera podczas aktualizacji.

Przed aktualizacją, potwierdzić podłączony kontroler **Informacje dotyczące kontrolera**, używać stabilnego bezpośredniego USB połączenie i zamknięcie innego oprogramowania, które mogłoby do niego dotrzeć. Po zakończeniu, ponownie podłączyć lub ponownie włączyć kontroler i ponownie przeczytać jego informacje, aby zweryfikować zgłoszoną wersję oprogramowania firmowego.

## Logi i historia operacji

Otwórz historię operacji, aby sprawdzić zapisane dzienniki po operacji.

<p align="center"><img src="../images/operation-history-en.png" alt="Historia operacji" width="68%"></p>

Wybierz dziennik po lewej, aby wyświetlić jego zawartość. **Eksport** zapisuje kopię do diagnostyki lub wsparcia. Ścieżki i linie poleceń mogą zawierać osobiste nazwy folderów, więc przeglądaj eksportowane dzienniki przed ich udostępnieniem.

Konsola na żywo w głównym oknie pokazuje bieżące polecenie i ostatnie wyjście. Przycisk kopiujący kopiuje wyświetlony tekst.

### Czytanie dziennika

Przydatny dziennik diagnostyczny zawiera wygenerowane polecenie, znaczniki czasu, wyjście silnika i status końcowy. Przepracuj od dołu w górę: zidentyfikuj błąd końcowy, a następnie zlokalizuj pierwsze ostrzeżenie lub nieudaną ścieżkę, która go poprzedzała. Późniejsza ogólna awaria jest często konsekwencją wcześniejszego, bardziej konkretnego przesłania.

Porównując dwie próby, należy sprawdzić, czy kontroler, napęd, silnik, profil, ścieżka źródłowa, format wyjściowy i argumenty ekspertów były identyczne. W przeciwnym razie, inny wynik może odzwierciedlać zmienione ustawienia, a nie niestabilność dysku.

## Dane dotyczące stosowania i zastosowania przenośnego

GW GUI przechowuje dane użytkownika oddzielnie od binarnych aplikacji. W zależności od wybranego pakietu i trybu, ustawienia, logi, ściągnięte narzędzia, elementy emulatora, uchwyty, stany i konfiguracje maszyny są przechowywane w aplikacji `Data` katalog lub w skonfigurowanych lokalizacjach danych użytkownika.

Przed zastąpieniem lub przeniesieniem instalacji przenośnej należy zachować kompletny folder aplikacji razem i utworzyć kopię zapasową `Data` folder. Nie przenoś pojedynczych plików z `lib`, ponieważ aplikacja usuwa własne i trzecie biblioteki z tej struktury.

### Sugerowana zawartość kopii zapasowej

Kopia zapasowa, gdy są one ważne dla Twojego przepływu pracy:

- ustawienia i profile aplikacji;
- definicje sterownika i napędu;
- konfiguracje emulacji;
- ROM ścieżki i prawnie utrzymywane ROM kopie zapasowe;
- obrazy twardego dysku i usuwalnych nośników;
- przechwytywania i zapisywania stanów;
- dzienniki operacji wykorzystywane jako rejestry konserwacji.

Obrazy dysku mogą być znacznie większe niż ustawienia. Przechowuj archiwalnych mistrzów read- tylko wtedy, gdy to możliwe, i pracować na kopiach.

## Zalecane przepływy pracy

### Archiwizacja nieznanego dysku

1. Sprawdzić i wyczyścić napęd za pomocą odpowiedniej procedury konserwacji.
2. Write- chronić dysk, jeśli to możliwe.
3. Wybierz **Czytaj > Obraz surowy (SCP)**.
4. Użyj opisowej nazwy pliku i przeczytaj normalny zakres toru z wielokrotnymi obrotami.
5. Przejrzyj konsolę i zapisany dziennik.
6. Sprawdzić obie strony **Wizualizacja**.
7. Konwersja kopii do prawdopodobnych formatów sektorowych.
8. Badanie przeliczonych kopii w **Disk Explorer** lub odpowiedniego oprogramowania.
9. Zachowaj surowego mistrza, log i notatki razem.

### Odtwarzanie dysku z obrazu

1. Sprawdź obraz i potwierdź jego oczekiwaną rodzinę i format.
2. Wstaw zbyteczny lub umyślnie zapisany dysk o odpowiednim rozmiarze i gęstości.
3. Otwórz **Napisz** i wybrać obraz.
4. Potwierdź skonfigurowany dysk i wykryty format.
5. Napisz dysk.
6. Przeczytaj z powrotem do osobnego obrazu weryfikacji.
7. Porównaj dekodowane treści i przeglądaj podejrzane ślady wizualnie.

### Tworzenie emulowanego Amiga

1. Otwórz **Opcje > Emulacja > Konfiguracje** i tworzyć lub wybierać maszynę.
2. W **Amiga > Ogólne**, wybrać model i emulator wersji.
3. Przypisz zgodne, legalnie uzyskane ROM.
4. Keep the model defaults for CPU oraz RAM na pierwszym bucie.
5. Konfiguracja wideo i audio z konserwatywnymi automatycznymi ustawieniami.
6. Dodaj urządzenia pamięci masowej i współrzędnych kopiowane obrazy mediów.
7. Przegląd zadań klawiatury, myszy i kontrolera.
8. Zapisz konfigurację.
9. Powrót do **Emulacja **, wybierz i kliknij ** Otwórz**.
10. Dopiero po udanym starcie, zmienić przyspieszenie lub zaawansowane ustawienia jeden na raz.

## Lista kontrolna bezpieczeństwa

Przed **Czytaj**:

- dysk źródłowy znajduje się we właściwym napędzie;
- źródło jest w miarę możliwości chronione na piśmie;
- ścieżka wyjściowa nie zastąpi istniejącego mistrza;
- profil i zakres toru pasują do dysku.

Przed **Napisz ** lub ** Usuń**:

- dysk docelowy może zostać zniszczony;
- obraz i napęd są poprawne;
- rozmiar i gęstość dysku są kompatybilne;
- jako cel podróży nie używa się żadnego mistrza archiwalnego.

Przed narzędziem do zmiany składu:

- żadna inna operacja nie jest wykonywana;
- wybrano właściwy kontroler;
- zarejestrowano bieżące wartości;
- sterownik ma stałą moc i USB łączność;
- działanie jest obsługiwane przez dokumentację sprzętową.

## Rozwiązywanie problemów

### Kontroler nie jest wymieniony

1. Odtworzyć kontroler bezpośrednio do komputera.
2. Otwórz **Opcje > Sterowniki i napędy**.
3. Kliknij **Przeskanuj**.
4. Sprawdzić stan sterownika i konfigurację napędu.
5. Biegnij **Informacje dotyczące kontrolera** jeśli wykrycie się powiedzie, ale polecenia zawiodą.

Jeśli nadal się nie pojawi, spróbuj innego bezpośredniego USB Port i kablówka, potem reskan. Sprawdź Menedżer urządzeń Windows w poszukiwaniu nowo wykrytego urządzenia szeregowego. Sterownik widoczny dla systemu Windows, ale nieobecny GW GUI zazwyczaj wskazuje na ruchliwy port, nieświeżą konfigurację lub problem Host Tools; kontroler nieobecny w systemie Windows wskazuje na USB, moc, kierowca lub sprzęt.

### `gw.exe` nie można znaleźć

Otwórz **Opcje > Sterowniki i napędy **, a następnie użyć ** Znajdź gw.exe **, ** Wybierz **lub ** Pobierz najnowszą wersję**. Potwierdź, że wykryta ścieżka wskazuje na zamierzony Greaseweazle instalacja.

Po wybraniu, uruchom **Informacje dotyczące kontrolera** Jeśli to nie powiodło się przed skontaktowaniem się ze sprzętem, sprawdź dziennik dla nieprawidłowej ścieżki wykonywalnej, brakujących plików lub wersji, która nie może się rozpocząć.

### Operacja używa niewłaściwego silnika

Otwórz **Opcje > Silniki** i sprawdzić silnik przypisany do tej samej operacji. GW GUI nie wraca po cichu do drugiego silnika.

Ustawienia silnika są oddzielne: zmiana silnika konwersji nie zmienia odczytu, zapisu, lub Disk Explorer. Ponownie otworzyć operację po zapisaniu opcji i potwierdzić wygenerowane polecenie w konsoli.

### Obraz nie jest rozpoznawany

Wyłącz automatyczne wykrywanie tylko wtedy, gdy znasz właściwą maszynę i format. W przeciwnym razie, spróbuj **Wizualizacja** zakładka do sprawdzenia obrazu na niższym poziomie.

Sprawdź, czy źródłem jest surowy strumień przechwytywania, obraz sektora, skompresowany pojemnik lub niepowiązany plik z wprowadzającym w błąd rozszerzenia. Nigdy nie zmieniaj nazwy rozszerzenia tylko po to, aby wymusić wykrywanie; konwersja musi prawidłowo interpretować strukturę źródłową.

### Emulacja nie zaczyna się

Weryfikacja zapisanej konfiguracji, zainstalowanej wersji emulatora, wybrana ROM, ścieżki przechowywania i kompatybilność modelu. Przegląd dziennika aplikacji dla kompletnych szczegółów błędu.

Tymczasowy powrót CPU, RAM, wideo i pamięci masowej do prostego modelu kompatybilnego z bazą. Jeśli wartość odniesienia się zacznie, przywróć jedno ustawienie na raz. Zapisany stan stworzony z innej wersji emulatora lub definicji maszyny może również nie powiodło się, nawet gdy czysty but działa.

### Skrót lub wejście nie działają

Sprawdź oba globalne **Emulacja > Skróty** strona i strona klawiatury, myszki lub sterownika. Rozwiąż wszelkie zadania oznaczone jako sprzeczne.

Jeśli mysz jest uchwycona, użyj skrótu wydania wyświetlanego w pasku narzędzi running- machine. Jeśli kontroler został podłączony po otwarciu Opcji, uruchom wykrywanie kontrolera jeszcze raz przed przydzieleniem go.

### Polecenie nie powiodło się niespodziewanie

1. Przeczytaj wyjście konsoli na żywo.
2. Otwórz **Historia operacji** dla całego zapisanego dziennika.
3. Potwierdź wybrany kontroler, napęd, profil, silnik i ścieżki plików.
4. Eksportuj odpowiedni dziennik, jeśli musi być podzielony do diagnozy.

### Krzaki lub pauzy dźwiękowe

Zwiększ opóźnienie emulacji dźwięku, zamknij CPU-intensywne aplikacje, a powrót wideo ramki skakanie i przyspieszenie do ich poprzednich wartości. Sprawdzić, czy wybrano zamierzone urządzenie audio systemu Windows. Zmień jedno ustawienie na raz, aby można było zidentyfikować skuteczną korektę.

### Wyświetlacz emulacji jest pusty lub powolny

Przywróć tryb rozdzielczości i linii **Automatyczne**, wyłączyć skakanie ramki i utrwalanie migotania tymczasowo, i spróbuj wcześniej działa renderer. Potwierdź, że skonfigurowany ROM i wstawione nośniki startowe są ważne. W FPS wskaźnik pomaga odróżnić problem rendering- performance od maszyny, która po prostu nie uruchomił.

### Czytanie zawiera niestabilne ścieżki

Powtórzyć odczyt nowej nazwy pliku, zwiększyć obroty, w stosownych przypadkach, i porównać uszkodzone utwory. Oczyścić głowice napędowe za pomocą prawidłowej procedury i sprawdzić dysk pod kątem uszkodzeń fizycznych. Nie należy wielokrotnie czytać widocznie rozrzucających się lub uszkodzonych nośników, ponieważ dalsze przejścia mogą ją pogorszyć.

## Glosariusz

| Termin | Znaczenie GW GUI |
|---|---|
| Sterownik | W Greaseweazle interfejs sprzętowy podłączony USB |
| Dysk | Fizyczny napęd dyskietek podłączony do sterownika |
| Silnik | Realizacja wybrana do wykonania operacji |
| Flux | Informacja o czasie odczytu przejścia magnetycznego z dysku |
| Obraz surowy | Utrzymanie informacji o dyskach niskiego poziomu, takich jak SCP |
| Obraz sektora | Odszyfrowana reprezentacja zorganizowana w sektorach logicznych |
| Rewolucja | Jedna całkowita rotacja pobrana podczas czytania toru |
| Cylinder | Pozycja głowy promieniowej; jeden cylinder może zawierać tor z każdej strony |
| Głowa | Strona dysku wybrana przez dysk fizyczny |
| Profil | Zestaw ustawień wielokrotnego użytku dla operacji |
| ROM | Obraz firmware wymagany przez emulowaną maszynę |
| Stan zapisany | Zdjęcie stanu maszyny emulatora |
| Renderer | Backend graficzny używany do wyświetlania wyjścia emulacji |

## Szybkie odniesienie

| Jeśli chcesz... | Idź do... |
|---|---|
| Zachować dysk fizyczny | **Czytaj** |
| Umieść obraz z powrotem na dysku | **Napisz** |
| Wyprodukuj inny format obrazu | **Konwersja** |
| Ślady kontrolne lub anomalie strumienia | **Wizualizacja** |
| Przeglądaj pliki wewnątrz obrazu | **Disk Explorer** |
| Kontrola komunikacji kontrolera | **Narzędzia > Informacje dotyczące kontrolera** |
| Pomiar obrotów napędu | **Narzędzia > Prędkość napędu** |
| Przejrzyj poprzednie polecenie | **Historia operacji** |
| Konfiguracja sprzętu | **Opcje > Sterowniki i napędy** |
| Wybierz implementacje | **Opcje > Silniki** |
| Utwórz lub edytuj maszynę emulowaną | **Opcje > Emulacja** |
| Uruchom zapisaną maszynę | **Emulacja** |
