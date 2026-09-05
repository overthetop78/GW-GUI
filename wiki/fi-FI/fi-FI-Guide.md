[🌐 Languages / Langues](../Home.md)

# GW GUI Käyttäjän opas

GW GUI on Windows-sovellus lukemiseen, kirjoittamiseen, muuntamiseen, tarkastukseen ja emulointiin levykuvat. Se voi hallita Greaseweazle laitteisto, työskennellä levykuvatiedostoja kautta sen sisäinen moottori, ja ajaa tallennettu emuloitu-kone kokoonpanot.

Tässä oppaassa kuvataan sovelluksen nykyisessä versiossa esitetty englanninkielinen käyttöliittymä. Se on kirjoitettu lähde tulostettavan käyttöohjeen: kuvakaappaukset kuvaavat ohjaimia, kun taas ympäröivä teksti selittää, mitä valita, miksi valita se, ja miten tarkistaa tulos.

> **Tärkeää:** Levyn lukeminen ei ole tuhoisaa. Kirjoittaminen, pyyhkiminen, firmware päivitys, ja jotkut laitteisto työkalut voivat muokata mediaa tai laitteistoa. Lue asianomaiseen menettelyyn liitetty varoitus ennen napsauttamista ** Suorita**.

### Miten tätä opasta käytetään

Jos käytät tätä ensimmäistä kertaa GW GUI, täydellinen [Aloitus](#getting-started), sitten seuraa [Luetaan levy](#reading-a-disk). Jos sovellus on jo konfiguroitu, mene suoraan leikkaukseen, jonka haluat suorittaa. Vaihtoehdot luvut toimivat viitteenä, kun menettely pyytää sinua muuttamaan aseman, moottorin, profiilin tai emuloitu koneen asetuksia.

Liitännän nimet näytetään **rohkea**. Tiedostonimet, polut, komennot ja kirjaimelliset arvot näytetään `code`. Huomautukset selittävät normaalin käyttäytymisen; varoitukset tunnistavat toiminnot, jotka voivat muuttaa levyä, ohjainta tai tallennettua kokoonpanoa.

## Sisältö

1. [Työvirran ymmärtäminen](#understanding-the-workflow)
2. [Aloittaminen](#getting-started)
3. [Pääikkuna](#main-window)
4. [Luetaan levyä](#reading-a-disk)
5. [Kirjoitan levyä](#writing-a-disk)
6. [Muokataan levykuvia](#converting-disk-images)
7. [Visualisoidaan levykuvaa](#visualizing-a-disk-image)
8. [Tutkitaan levyn sisältöä](#exploring-disk-contents)
9. [Työkalujen käyttö](#using-the-tools)
10. [Emulointi](#emulation)
11. [Hakemuksen valinnat](#application-options)
12. [Emulointivaihtoehdot](#emulation-options)
13. [Amiga kokoonpano](#amiga-configuration)
14. [Hardware-diagnostiikka ja huolto](#hardware-diagnostics-and-maintenance)
15. [Kirjat ja toimintahistoria](#logs-and-operation-history)
16. [Hakemustiedot ja kannettava käyttö](#application-data-and-portable-use)
17. [Suositellaan työnkulkua](#recommended-workflows)
18. [Turvallisuustarkistuslista](#safety-checklist)
19. [Vahingonmääritys](#troubleshooting)
20. [Sanasto](#glossary)
21. [Pikaviite](#quick-reference)

## Työnkulun ymmärtäminen

GW GUI erottaa fyysisen levyn toiminnot kuvatiedoston toiminnoista:

| Tavoite | Syöte | Tulos | Suositeltava sivu |
|---|---|---|---|
| Säilytä levyke | Fyysinen levy | Kuvatiedosto | **Lue** |
| Luo levyke uudelleen | Kuvatiedosto | Fyysinen levy | **Kirjoita** |
| Vaihda kuvan muotoa | Kuvatiedosto | Yksi tai useampi kuvatiedosto | **Muuntaminen** |
| Raiteet ja poikkeamat | Kuvatiedosto | Näköanalyysi | **Visualisointi** |
| Selaa levykuvaan tallennettuja tiedostoja | Tuettu kuva- tai tiedostojärjestelmä | Tiedostot ja kansiot | **Disk Explorer** |
| Diagnoosi asema tai ohjain | Greaseweazle laitteisto | Mittaukset tai tila | **Työkalut** |
| Suorita tallennettu virtuaalikone | Tallennetun koneen kokoonpano | Emulsioistunto | **Emulsio** |

Suojellakseen, ensin tehdä raaka kiinniotto ja pitää se ennallaan isäntä. Luo mestarilta muunnetut tai korjatut työkopiot. Näin vältetään fyysisen lukemisen toistaminen ja säilytetään tiedot, joita alakohtainen muoto ei välttämättä säilytä.

## Aloittaminen

### Vaatimukset

- Ikkunat Microsoft .NET Sovelluksen vaatima työpöytäajoaika.
- A Greaseweazle fyysisen levytyksen ohjain.
- Asetettu polku `gw.exe` käytettäessä Greaseweazle Host Tools Moottori.
- Laillisesti saadut ROM tiedostoja, kun emuloitu kone tarvitsee niitä.

Sovellus tarkistaa tarvittavan .NET-käyttöajan käynnistettäessä. Jos se puuttuu, seuraa asennuskehotusta ja käynnistä se uudelleen GW GUI.

### Ennen laitteiston liittämistä

Tarkista seuraavat tiedot ennen kuin suoritat fyysisen levyn toimintaa:

1. Yhdistä Greaseweazle ohjain talliin USB satama.
2. Liitä levykekaapeli oikeaan asentoon.
3. Kytke voimanlähde ennen arvokkaiden medioiden asentamista.
4. Varmista, että aseman koko ja tiheys vastaavat levyä.
5. Write-protect lähdelevyn mahdollisuuksien mukaan.

GW GUI ei voi estää virheellistä kaapelointia, sopimatonta tehoa tai mekaanisesti vaarallista ajoa. Testaa tuntematon laitteisto, jossa on käytetty levy ensin.

### Ensimmäinen laukaisu

1. Avaa `gwgui.exe`.
2. Avaa **Valinnat**.
3. Sisään **Ohjaimet ja asemat**, skannaa säätimen ja määrittää aseman.
4. Tarkista tai valitse valittu polku `gw.exe`.
5. Sisään **Moottorit**, valitse mikä moottori suorittaa kunkin toiminnan.
6. Palaa pääikkunaan ja valitse vaadittu käyttövälilehti.

### Vahvistan, että asennus on valmis

Työasetukset näyttävät ohjaimen ja aseman tilapalkissa, esimerkiksi aseman numeron, koon, tiheyden ja COM satama. Sisään **Vaihtoehdot > Ohjaimet ja asemat **, rekisterinpitäjä olisi merkittävä ** Saatavilla ** ja asema ** Muokkaa **Juokse! ** Rekisterinpitäjän tiedot** ennen arvokkaan median lukemista, jos haluat varmistaa viestinnän muuttamatta levyä.

### Moottorin valinta

GW GUI voi paljastaa useamman kuin yhden toteutuksen joissakin toimissa. • **Greaseweazle Host Tools** moottori vetoaa määritetty `gw.exe`; sisäinen GW GUI moottorin kahvat tuettu toiminta sovelluksen sisällä. Moottorin valinta on selkeä ja riippumaton lukemiseen, kirjoittamiseen, muuntamiseen ja Disk ExplorerJos valittu moottori ei tue toimenpidettä, GW GUI ilmoittaa, että ehto sijasta moottoreiden automaattisesti.

## Pääikkuna

Pääikkuna ryhmittelee pääasialliset toiminnot seitsemään välilehteen:

- **Lue** luo kuvan fyysisestä levystä.
- **Kirjoita** kirjoittaa kuvan fyysiselle levylle.
- **Muuntaminen** muuntaa yhden levykuvaformaatin yhdeksi tai useammaksi lähtöformaatiksi.
- **Visualisointi** näyttää kappaleita ja virtauksia tai koodattuja tietoja.
- **Disk Explorer** selaa tuettuja tiedostojärjestelmiä ja levyn sisältöä.
- **Työkalut** tarjoaa laitteiston huoltoa ja diagnostisia komentoja.
- **Emulsio** hallinnoi ja käyttää tallennettuja emuloituja koneita.

Alhaalla oleva konsoli näyttää suoritettavan komennon ja sen ulostulon. Tilarivi raportoi valitun aseman, profiilin ja nykyisen tilan.

### Käyttöliittymän lukeminen

Useimmat käyttösivut noudattavat samaa kaavaa:

1. **Lähde tai määräpaikka** ohjaimet tunnistaa levyn, kuvan tai kansion.
2. **Muototarkastukset** Valitse automaattinen havaitseminen tai kone ja muoto.
3. **Profiilin ohjaimet** soveltaa uudelleenkäytettäviä asetuksia.
4. **Lisäasetukset** altistaa parametrit, jotka ovat tavallisesti valinnaisia.
5. **Suorita** Aloita operaatio.
6. • **konsoli** näyttää luodun komennon, edistymisen, varoitukset ja virheet.

• **Suorita** painike ei tarkoita, että kaikki arvot ovat turvallisia asetettu levy. Tarkista aina kohde ja valittu asema ennen kirjoittamista tai huoltoa.

### Tilapalkki ja konsoli

Tilarivin vasen puoli tunnistaa aktiivisen fyysisen aseman. Keskus näyttää aktiivisen profiilin, kun se valitaan. Valtion indikaattori ilmoittaa, onko sovellus valmis vai kiireinen. Konsoli ei ole pelkästään diagnostinen: se on valittuun moottoriin lähetetyn komennon arvovaltainen merkintä. Käytä sen kopionhallintaa, kun sinun täytyy säilyttää tai jakaa komento.

## Levyn lukeminen

Avaa **Lue** välilehti tallentaa fyysinen levyke kuvana.

<p align="center"><img src="../images/main-read-en.png" alt="Lue välilehti" width="78%"></p>

### Perusmenettely

1. Aseta lähdelevy asemaan.
2. Valitse kuvan tyyppi:
   - **Raakakuva (SCP)** Säilyttää virtatason tiedot.
   - **Tunnettu levymuoto** luo kuvan valitsemalla koneen ja muodon.
3. Valitse kohdekansio.
4. Anna tulostiedoston nimi.
5. Valitse profiili tarvittaessa.
6. Klikkaa **Suorita**.

Konsoli näyttää tarkan komennon ja edistymisen. Älä poista levyä tai irrota ohjainta ennen kuin toiminta on päättynyt.

### Tulostustyypin valinta

Käyttö **Raakakuva (SCP)** kun tavoitteena on arkistointi, analysointi, talteenotto tai myöhempi muuntaminen. Raaka kuva tallentaa ajoitustiedot ja useita vallankumouksia, joka on hyödyllinen epätavallisia muotoja, heikko sektoreita, suojajärjestelmät, ja vaurioitunut media.

Käyttö **Tunnettu levymuoto** kun jo tunnet levyperheen ja tarvitset suoraan käyttökelpoisen sektorikuvan. Tämä valinta voi olla pienempi ja helpompi avata muissa ohjelmistoissa, mutta se edustaa koodattua tulosta eikä jokaista aseman havaitsemaa yksityiskohtaa.

Kun epävarma, luo raaka kuva ensin. Voit muuntaa sen myöhemmin lukematta levyä uudelleen.

### Kansio, tiedostonimi ja profiili

• **Kansio ** on kohdehakemisto. • ** Tiedostonimi** tunnistaa levyn luottamatta ainoastaan sen fyysiseen etikettiin. Hyödyllinen arkiston nimi sisältää nimen, levyn numeron tai sivun sekä tarvittaessa ehtoilmoituksen. Älä lisää tiedostomuodon laajennusta, joka on ristiriidassa valitun tulostusmuodon kanssa.

A **Profiili ** käyttää tallennettuja lukuparametreja. Valitse yksi vain, kun tiedät mitä se sisältää. • ** Oletus** profiili sopii normaaliin ensimmäiseen yritykseen; erikoistunut toipumisprofiili voi tarkoituksellisesti lukea enemmän vallankumouksia tai erilaista rataväliä ja siten kestää kauemmin.

### Lisäasetukset

Laajenna **Lisäasetukset** käyttää muotokohtaisia tai asiantuntijaparametreja. Jätä nämä arvot ennalleen, ellei levy vaadi tiettyä raideväliä, vallankumouslukua tai säädintä.

Yhteisiä edistyneitä arvoja ovat:

| Asetukset | Aihe | Milloin sitä muutetaan |
|---|---|---|
| Rataväli | Rajaa sylinterit ja päät luettavaksi | Yksipuolinen media, epätavallinen geometria tai kohdennettu palautumispassi |
| Vallankumoukset | Tarkastaa kuinka monta kierrosta näytetään | Epävakaat tai suojatut radat lisääntyvät; nopeutta vähennetään tarvittaessa vain nopeudella |
| Asiantuntijalausunnot | Ohittaa moottorin lisäparametrit | Vain silloin, kun se on dokumentoitu Greaseweazle ohjeet |

### Onnistuneen lukemisen varmistaminen

Älä luota vain virheikkunan puuttumiseen. Kun komento on suoritettu:

1. Varmista, että tulostetiedosto on olemassa eikä ole tyhjä.
2. Lue lopulliset konsolirivit epäonnistuneille tai puuttuville kappaleille.
3. Avaa kuva **Visualisointi** tarkistaa, että molemmat puolet ja odotettu raideväli sisältävät tietoa.
4. Avaa se **Disk Explorer** kun tiedostojärjestelmää tuetaan.
5. Pidä operaatioloki tärkeillä arkistokaappauksilla.

Jos toistuvat lukemat eroavat toisistaan, säilytä jokainen raaka pyydystys eikä ylikirjoita ensimmäistä. Eroista voi olla hyötyä toipumisen aikana.

## Kirjoitetaan levyä

Avaa **Kirjoita** välilehti kirjoittaa olemassa olevan kuvan fyysiselle levykkeelle.

<p align="center"><img src="../images/main-write-en.png" alt="Kirjoita välilehti" width="78%"></p>

### Perusmenettely

1. Aseta kohdelevy.
2. Valitse lähdekuva **Selaa**.
3. Vahvista havaittu muoto.
4. Valitse profiili tarvittaessa.
5. Klikkaa **Suorita**.

Kirjoittaminen korvaa kohdelevyn tiedot. Tarkista valittu asema ja kuva ennen kuin aloitat.

> **Varoitus:** Kirjoittaminen on tuhoisaa. Se korvaa magneettiset tiedot kohdelevyllä. Käytä kirjoitussuojattua lähdearkistoa ja erillistä kohdelevyä aina kun mahdollista.

### Ennen kirjoittamista

Tarkista neljä kohtaa ennen klikkaamista **Suorita**:

1. **Kuva:** valittu polku on tarkoitettu lähdekuvaksi.
2. **Levy:** asemalla oleva levy voidaan turvallisesti korvata.
3. **Aja:** määritetty koko ja tiheys sopivat määränpäähän.
4. **Muoto:** automaattinen havaitseminen tai manuaalisesti valittu muoto vastaa kuvaa.

Jos lähdekuvaa ei ole testattu, avaa se **Visualisointi ** tai ** Disk Explorer** Ensin. Onnistunut kirjoitus ei voi korjata epätäydellistä lähdekuvaa.

### Radan tarkastus ja muuttaminen

Kun kuva on valittu, **Visualisoi kappaleet ** avaa rataesittelyn. ** Muokkaa** paljastaa tuetut kuvamuutokset ennen kirjoittamista. Käytettävissä olevat toimet riippuvat valitusta muodosta ja moottorista.

### Kirjallisen levyn tarkistaminen

Kun moottori tukee varmistusta, käytä sitä tärkeissä tiedotusvälineissä. Muuten lue kirjoitettu levy takaisin uuteen kuvaan ja vertaa sen purettua sisältöä tai tarkista se **Visualisointi**. Pidä varmistuskaappaus erillään alkuperäisestä kuvasta niin, että alkuperäistä ei koskaan ylikirjoiteta.

Jos kirjoitus epäonnistuu yhdenmukaisissa kappaleissa, tarkista levyn kunto, tiheys, aseman puhtaus ja aseman konfiguraatio. Jos virheitä esiintyy satunnaisesti, tarkista USB vakaus ja valvoja viestintä.

## Muunnetaan levykuvia

• **Muuntaminen** välilehti muuntaa lähdekuvan yhdeksi tai useaksi kohdeformaatiksi.

<p align="center"><img src="../images/main-conversion-en.png" alt="Muunnosvälilehti" width="78%"></p>

### Perusmenettely

1. Valitse lähdekuva.
2. Valinnaisesti ilmoitetaan tulostusnimet.
3. Valitse koneperhe.
4. Valitse yksi tai useampi lähtömuoto ja laajennukset.
5. Käytä **Lisää tagit** jos tiedostonimien pitäisi käyttää määritettyä tag-kuviota.
6. Klikkaa **Suorita**.

• **Valittu ** paneeli luettelee vaaditut tuotokset. ** Tiedoston siirtäminen** tarjoaa oman työnkulun tuettujen tiedostojen siirtämiseksi sen sijaan, että suoritettaisiin vakiokuvan muuntaminen.

### Valitaan muotoja

• **Kone ** luettelo suodattaa formaatteja näytetään ** Muoto** paneeli. Muotonimi kuvaa loogista levyasettelua; laajennus kuvaa lähtöastiaa. Joitakin muotoja voi edustaa useampi kuin yksi laajennus, ja jotkut säiliöt eivät voi säilyttää kaikkia ominaisuuksia raaka lähde.

Valitse vain tarvittavat lähdöt. Useita formaatteja on hyödyllistä luoda arkiston master, emulaattori-yhteensopiva kopio, ja kopio toinen analyysityökalu yhdessä toiminnossa.

### Tulosten nimet ja tunnisteet

**Tulostusnimet ** Voit hallita valittuihin muotoihin luotuja perusnimiä. ** Lisää tagit ** soveltaa tiedostonimikuviota ** Vaihtoehdot > Yleistä**. Tunnisteet voivat koodata perheen, muoto, laajennus, päivämäärä tai aika. Esikatsele esimerkkiä Valinnat ennen muuntamista suuri erä niin, että tiedostot nimetään johdonmukaisesti.

### Muuntamisen tulosten tarkistaminen

Kunkin pyydetyn tuotoksen osalta:

1. Vahvista, että tiedosto luotiin.
2. Tarkista konsolista kappaleet tai alat, joita ei voitu purkaa.
3. Avaa tulos **Disk Explorer** jos se sisältää tuetun tiedostojärjestelmän.
4. Vertaa odotettua levyn kapasiteettia ja sisältöä lähteeseen.

Muuntaminen voi olla täydellinen samalla kun ilmoitetaan määrämuotoon kuuluvasta tietojen menetyksestä. Säilytä alkuperäinen raaka kuva silloinkin, kun muunnettu kuva näyttää oikealta.

## Levykuvan visualisointi

• **Visualisointi** välilehti näyttää kuvan rakenteen ja tiedonjaon.

<p align="center"><img src="../images/main-visualization-en.png" alt="Visualisointivälilehti" width="78%"></p>

1. Klikkaa **Avaa levykuva**.
2. Säilytä **Automaattinen tunnistus** käytössä tai valitse kone ja muoto käsin.
3. Käyttö **Link zoomaa** pitää molemmat puolet samalla zoomaustasolla.
4. Käyttö **Nollaa** palauttaaksesi alkuperäisen näkymän.
5. Avaa **Tarkastaja** yksityiskohtaiset tiedot valitusta alueesta.

Legenda erottaa normaalin vuon, lyhyet ja pitkät siirtymät, otsikot, koodatun datan ja havaitut poikkeamat. Raakakuva voi sisältää tietoja, joita ei voida purkaa tunnettuun tiedostojärjestelmään mutta jotka voidaan silti tarkastaa täällä.

### Lausunnon tulkitseminen

Jokainen suuri pyöreä paneeli edustaa yhtä levyn puolta. Keskus tunnistaa sivun ja sen nykyisen tietotilan; samankeskiset asemat vastaavat kappaleita. Värit luokittelevat havaitut alueet legendan mukaan. Visualisoijan tarkoituksena on vastata seuraaviin kysymyksiin:

- Onko kuvassa tietoja toiselta puolelta vai molemmilta?
- Ovatko odotetut jäljet paikalla?
- Ovatko poikkeavuudet eristetty vai toistetaan koko levyllä?
- Tunnistiko automaattinen havaitseminen uskottavan koneen ja muodon?

Poikkeavuuden väri on syy tarkastaa alue, ei todiste siitä, että levy on käyttökelvoton. Kopiosuojaus, epätavallinen muotoilu, heikko tallennus ja vahingoittunut ala voivat tuottaa erilaisia rakenteita, jotka edellyttävät kontekstitulkintaa.

### Suositeltu tarkastusjakso

Aloita linkitetyllä zoomilla, jonka avulla molempia puolia voidaan verrata samassa mittakaavassa. Valitse epäilyttävä alue, avaa **Tarkastaja**, ja verrata sitä naapurin jälkiä. Jos tulos näyttää olevan havaitsemisongelma, poista automaattinen havaitseminen ja valitse tunnettu kone ja muoto. Palataan automaattiseen havaitsemiseen testin jälkeen, jotta pakotettua asetusta ei käytetä vahingossa toiseen kuvaan.

## Levyn sisällön tutkiminen

• **Disk Explorer** välilehti selaa tuettuja levykuvia tiedostohierarkiana.

<p align="center"><img src="../images/main-disk-explorer-en.png" alt="Disk Explorer välilehti" width="78%"></p>

1. Avaa olemassa oleva kuva tai lue levy.
2. Säilytä **Automaattinen tunnistus** käytössä, ellei sinun tarvitse pakottaa konetta tai muotoa.
3. Tarkista volyymitiedot: järjestelmä, suojaus, tiedostojärjestelmä, kapasiteetti, vapaa tila, ja kohteen määrä.
4. Selaa hakemistoja vasemmassa paneelissa.
5. Valitse kohde nähdäksesi sen yksityiskohdat oikeassa paneelissa.

Jos kuvamuotoa tai tiedostojärjestelmää ei tueta, käytä **Visualisointi** tarkastaa raaka rakenne sen sijaan.

### Paneelien ymmärtäminen

Top yhteenveto kuvaa asennettu kuva ja havaittu tilavuus. Alavasen paneeli sisältää hakemistohierarkian. Keskustaulukossa on valitun hakemiston kohdat, joiden nimi, muutospäivä, tyyppi ja koko. Oikea paneeli näyttää valitun kohteen yksityiskohdat.

Disk Explorer Ei tarkoita, että jokainen raita purettiin täydellisesti. Käytä äänenvoimakkuuden yhteenvetoa ja kohteen laskentaa nopeana uskottavuuden tarkistuksena, avaa sitten edustavat tiedostot tai vertaa niitä tunnettuun hakemistoon, kun säilytystarkkuus on tärkeää.

### Kun mitään ei näy

Vahvista ensin, että kuvapolku on oikea. Tarkista sitten havaittu kone ja muoto. Voimassa oleva kuva voi sisältää tukemattoman tai vahingoittumattoman tiedostojärjestelmän, jolloin tutkimusmatkailija voi pysyä tyhjänä, vaikka **Visualisointi** osoittaa tallennetut tiedot. Älä korvaa tai hylkää lähdekuvaa vain tyhjän tutkimusmatkailijan perusteella.

## Työkalujen käyttö

• **Työkalut** välilehtiryhmät Greaseweazle huoltotoimet.

<p align="center"><img src="../images/main-tools-en.png" alt="Työkalut- välilehti" width="78%"></p>

Valitse komento vasemmalla olevasta luettelosta, tarkista sen parametrit ja napsauta sitten **Suorita**. Tuhoavia tai laitteistoa vaihtavia komentoja tulisi käyttää vain valitun ohjaimen ja aseman tarkistamisen jälkeen.

Useimmat työkaluikkunat sisältävät kolme aluetta: parametreja huipulla, tila ja raaka-tuotosalue keskellä, ja luotu komento alareunassa. Komennon esikatselu muuttuu, kun valinnat ovat käytössä. Tarkastamaton parametri tarkoittaa tavallisesti, että älä muuta tätä arvoa.

Yksittäiset diagnostiset ikkunat on kuvattu [Hardware diagnostiikka ja huolto](#hardware-diagnostics-and-maintenance).

## Emulsio

### Tallennetun koneen avaaminen

• **Emulsio ** välilehtiluettelot tallennettuja asetuksia. Valitse yksi ja napsauta ** Avaa**. Jokainen käynnissä kone näkyy omassa välilehdessä.

<p align="center"><img src="../images/main-emulation-welcome-en.png" alt="Emulsio tervetuliaisnäyttö" width="78%"></p>

Luo ja muokkaa koneita **Vaihtoehdot > Emulsio > Asetukset ** sekä ** Vaihtoehdot > Emulsio > Amiga**.

Jos asetuksia ei näy, luo yksi asetuksiin ensin. Tallennetussa konfiguraatiossa yhdistyvät konemalli, emulaattoriversio, ROM, muisti, video, audio, tallennus, ja tulokarttoja. Konfiguroinnin tallentaminen ei käynnisty; palaa pääverkkoon **Emulsio ** välilehteä ja napsauta ** Avaa**.

### Moottorin ohjauslaitteet

<p align="center"><img src="../images/main-emulation-running-en.png" alt="Ajoemuloitu kone" width="78%"></p>

Käyttökoneen työkalupalkki tarjoaa tehoa, taukoa, nollausta, tallennustilaa, kuormitustilaa, kaappausta ja näyttöohjaimia. Se osoittaa myös:

- konfiguroidut pikatallennus- ja pikalataus pikanäppäimet;
- aktiivinen renderointilaite, kuten Direct3D 11;
- koko näytön ja hiirenvapautuksen pikanäppäimet;
- audio, ohjain ja hiiren tila;
- nykyinen resoluutio, päivitysnopeus ja kuvanopeus.

Emulointinäytön alareunassa oleva levynauha hallinnoi irrotettavaa mediaa jokaiselle emuloidulle asemalle. Näppäimistön tehtäviä voidaan muuttaa **Vaihtoehdot > Emulsio > Pikanäppäimet**, kun taas emuloitu näppäimistö, hiiri, ja ohjain kuvaukset on määritetty vastaavat Amiga piikit.

### Työkalupalkin viite

| Vertailuryhmä | Aihe |
|---|---|
| Virta ja tauko | Käynnistää, pysäyttää, keskeyttää tai jatkaa emuloitua konetta |
| Nollaa ohjaimet | Suorittaa konfiguroidun pehmeän tai kovan nollaustoiminnon |
| Valtion valvonta | Säästää tai kuormittaa emulaattoritilaa nopeaan jatkoon |
| Ota | Tallentaa kuvan emuloidusta näytöstä |
| Näyttö | Muuttaa näytön esitystapaa tai siirtyy kokoruutuun |
| Pikatilamuistutus | Näyttää aktiiviset tallenna/lataa pikanäppäimet |
| Renderöijä | Ilmoita aktiivinen videotaustaosa |
| Syöttömuistutus | Näyttää koko näytön ja hiirenvapautuksen pikanäppäimet |
| Laiteindikaattorit | Raportit audio, ohjain ja hiiren tila |
| Suorituskyky | Raportoi lähtökoko, päivitystaajuus ja kuvanopeus |

### Poistutaan koko näytön tai vapauttaa hiiren

Työkalurivi näyttää tällä hetkellä annetut avaimet. Kuvatussa konfiguraatiossa **Alt+ Palautus ** vaihtaa koko näytön ja ** F12** Vapauta hiiri. Kohtele esitettyjä arvoja arvovaltaisena, koska pikanäppäimiä voidaan siirtää.

### Leveän median käyttäminen

Asemanauha tunnistaa jokaisen emulgoidun aseman, kuten `DF0:`. Käytä sen mediaohjaimia lisätä, korvata tai poistaa kuvan. Median korvaaminen muuttaa vain juoksukonetta. Se ei muuta tallennetun koneen tallennuslaitteen määritelmää, ellei tämä toiminto ole nimenomaisesti tallennettu.

## Sovellusvalinnat

Avaa **Valinnat** pääikkunasta määrittääksesi sovelluksen.

### Yleistä

<p align="center"><img src="../images/options-general-en.png" alt="Yleiset vaihtoehdot" width="72%"></p>

• **Yleistä** välilehti sisältää:

- oletuslevykuvakansioon;
- käyttöliittymän kieli ja teema;
- tiedostonimimerkkien luominen muunnoksia varten;
- ennalta määritellyt ja viimeaikaiset yksilölliset tunnisteet;
- Live-tiedostonimiesimerkki.

Tag muuttujat sisältävät lähdenimi, perhe, muoto, laajennus, päivämäärä ja aika. Käytä nollauspainiketta palauttaaksesi oletusmallin.

Tiedostonimen esikatselupäivitykset ennen tiedostojen luomista. Sen avulla voidaan havaita kaksoiserotin, puuttuvat laajennukset tai moniselitteiset nimet. Viimeaikaiset mukautetut mallit tarjoavat nopean pääsyn aikaisempiin nimeämisjärjestelmiin korvaamatta nykyistä esiasetettua.

### lokit

<p align="center"><img src="../images/options-logs-en.png" alt="Lokivalinnat" width="72%"></p>

Kirjautuminen voidaan määrittää itsenäisesti kullekin operaatiolle. Valitse jokaisen luokan osalta, tallennetaanko lokit, asetetaan maksimitiedostokoko ja päätetään, onko aiemmat lokit säilytettävä. Koko `0` tarkoittaa rajattomasti. **Avaa kansio** avaa nykyisen lokihakemiston.

Käytä **Säilytä aiemmat lokit** Säilyttäminen ja diagnostinen työ, jos historia useita yrityksiä. Poista se käytöstä, kun vain viimeisin tulos on hyödyllinen. Enimmäiskokorajoja sovelletaan lokitallennukseen, ei tallennettuihin levykuviin.

### Ohjaimet ja asemat

<p align="center"><img src="../images/options-controllers-and-drives-en.png" alt="Ohjaimet ja asemat" width="72%"></p>

Käytä tätä välilehteä:

- Skannaa kytketyt ohjaimet;
- lisätään ja poistetaan asemakokoonpanot;
- Valitse aseman koko, tiheys ja nopeus;
- tallentaa laitteistoasetukset;
- valitse tai etsi automaattisesti `gw.exe`;
- tarkista ja lataa Greaseweazle Host Tools päivitykset
- palauttaa aiemmin määritetty suoritettava polku.

Tallennetut laitteistoasetukset ovat käytettävissä, kun asema on tilapäisesti kytketty irti.

#### Aseman lisääminen

1. Klikkaa **Etsi** ja odottaa yhteydessä ohjaimet ilmestyvät.
2. Klikkaa **Lisää levy** jos vaadittua asemaa ei ole jo lueteltu.
3. Valitse sen looginen asemanumero, fyysinen koko, tallennustiheys ja pyörimisnopeus.
4. Säästäkää rivi.
5. Varmista, että se näkyy. **Saatavilla ** sekä ** Muokkaa**.

Käytä roskakorin ohjausta vain tallennetun konfiguraation poistamiseen; se ei irrota laitteistoa. Jos sama ohjain näkyy eri COM skannaa myöhemmin uudelleen ennen kuin oletat, että varastoitu portti on edelleen voimassa.

#### Hallinta Greaseweazle Host Tools

**Etsi gw.exe ** Etsi tunnettuja paikkoja. ** Valitse ** valitsee tietyn suoritustiedoston. ** Tarkista päivitykset ** kyselyt saatavilla versioita korvaamatta asennettu. ** Lataa uusin versio ** asentaa valitun nykyisen paketin ja ** Käytä edellistä polkua ** palauttaa aikaisemman asetuspaikan. Suorita suorituksen jälkeen ** Rekisterinpitäjän tiedot** vahvistaa, että valittu versio voi kommunikoida ohjaimen kanssa.

### Moottorit

<p align="center"><img src="../images/options-engines-en.png" alt="Moottorin valinta" width="72%"></p>

Valitse moottori itsenäisesti lukemiseen, kirjoittamiseen, muuntamiseen, ja Disk Explorer. Valittua moottoria käytetään tiukasti: jos se ei pysty suorittamaan pyydettyä toimintaa, GW GUI ilmoittaa rajoituksesta moottorien hiljaisen kytkemisen sijaan.

Tämä riippumattomuus on tahallista. Esimerkiksi fyysiset lukemat voivat käyttää Greaseweazle Host Tools kun kuvan muuntaminen ja etsintä käyttää sisäistä moottoria. Tallenna moottorivalinnat profiiliin tai projektin muistiin, kun uusittavuus on tärkeää.

### Profiilit

<p align="center"><img src="../images/options-profiles-en.png" alt="Profiilit" width="72%"></p>

Profiilit tallentavat uudelleenkäytettäviä asetuksia lukemiseen, kirjoittamiseen ja muuntamiseen. Valitse profiilien hallintaan sopiva luokka. Valittu profiili näkyy pääikkunan tilapalkissa ja käyttönäytöissä.

Käytä profiileja toistuviin työnkulkuihin eikä selittämättöminä kokoelmina asiantuntijalippuja. Anna jokaiselle profiilille käyttötarkoituskohtainen nimi, kuten tietty asema, levyperhe tai palautusmenetelmä. Tarkista profiilin päivityksen jälkeen taustalla moottorin, koska tuetut vaihtoehdot voivat muuttua.

## Emulsiovaihtoehdot

• **Emulsio** Valinnat sisältävät yleiset tallennusasetukset, yleiset pikanäppäimet, tallennetut konfiguraatiot ja konekohtaiset asetukset.

### Yleiset emulointikansiot

<p align="center"><img src="../images/options-emulation-general-en.png" alt="Yleiset emulointivaihtoehdot" width="72%"></p>

Aseta jaettu emulointi tallennuskansio ja oletuskansiot kaappauksille ja tallennetuille olosuhteille. **Avaa kansio** avaa jaetun sijainnin File Explorerissa.

Pidä kaappaukset ja tallennetut tilat erillisissä kansioissa. Tallennus on tavallinen kuva; tallennettu tila sisältää emulaattorin oma konetila ja voi riippua emulaattorin versiosta ja sen muodosta. Varmista kokoonpano ja media tärkeiden tallennettujen valtioiden rinnalla.

### Pikanäppäimet

<p align="center"><img src="../images/options-emulation-shortcuts-en.png" alt="Emulsio pikanäppäimet" width="72%"></p>

Etsi toimintoa tai avainta, anna tai poista pikanäppäimet, palauta oletukset ja selvät ristiriidat. Tilasarakkeessa yksilöidään pätevät ja ristiriitaiset tehtävät.

Jos haluat muuttaa pikanäppäintä, etsi toiminto, napsauta **Määrittele **, ja paina haluttu avainyhdistelmä. Tarkista tila ennen sulkemista. ** Selkeät ristiriidat ** poistaa ristiriitaisia tehtäviä; se ei palauta oletuskartoitusta. Käyttö ** Palauta oletukset** kun haluat korvata mukautetut tehtävät standardilla.

### Tallennetut kokoonpanot

<p align="center"><img src="../images/options-emulation-configurations-en.png" alt="Tallennetut emulointikokoonpanot" width="72%"></p>

Tämä sivu listaa tallennetut koneet. Valitse muokattava asetus **Amiga** Lasku. Voit päivittää luetteloa tai poistaa valitun asetukset.

Konfiguroinnin poistaminen poistaa tallennetun koneen määritelmän. Sitä ei pitäisi käyttää tapana poistaa mediaa tai sulkea käynnissä oleva kone. Ennen poistoa huomioi mahdolliset ROM, kova levy kuva, ja valtion tiedostoja liittyvät asetukset.

## Amiga kokoonpano

Nykyinen käyttöliittymä tarjoaa yksityiskohtaiset tiedot Amiga asetussivut. Samaa asetusrakennetta voidaan laajentaa muihin emuloituihin järjestelmiin muuttamatta työnkulkua.

### Yleistä

<p align="center"><img src="../images/options-amiga-general-en.png" alt="Amiga yleiset asetukset" width="72%"></p>

Valitse Amiga malli, tallenna konfiguraatio, asenna tai korvaa emulaattoriversio, ja määritellä oletuskansioita kiintolevyille ja muille medialle. **Hakuversiot** Kysy virallisen emulaattoriversion lähteestä.

Aloita mallista, koska se rajoittaa myöhempiä sivuja. Muuttaminen voi muuttaa käytettävissä CPU, muisti, ROM, piirisarja, ja varastointi vaihtoehtoja. Valittuasi emulaattoriversion, tallenna asetukset ennen sen käynnistämistä pääikkunasta. Toisen emulaattoriversion asentaminen korvaa kyseisen kokoonpanon käyttämän version; se ei luo toista koneen kopiota.

### CPU

<p align="center"><img src="../images/options-amiga-cpu-en.png" alt="Amiga CPU asetukset" width="72%"></p>

• CPU sivu näyttää konemallin valitseman prosessorin ja tarjoaa yhteensopivan tarkkuuden, FPU, ja nopeusvalinnat. Valittuun malliin kuulumattomat vaihtoehdot eivät ole käytössä.

- **CPU malli** tunnistaa emulgoidun prosessorin.
- **Tarkkuus** valvoo ajoitusmallia. Cycle-exact-tilat suosivat laitteiston yhteensopivuutta, mutta vaativat enemmän isäntäkäsittelyä.
- **FPU** mahdollistaa yhteensopivan liukulukuyksikön tuettaessa.
- **CPU nopeus** valitsee alkuperäisen ajoituksen tai nopeutetun tilan.

Perustason konfiguraatiota varten pidetään mallista johdettu CPU ja alkuperäinen nopeus. Vaihda kiihtyvyys vasta kun kone saappaat oikein sen vakioasetuksissa.

### RAM

<p align="center"><img src="../images/options-amiga-ram-en.png" alt="Amiga RAM asetukset" width="72%"></p>

Aseta siru RAM, Hitaasti RAM, Nopea RAM, ja tuettu laajennusmuisti. Yhteensopivuusviestit selittävät valitun koneen rajoitukset, ja konfiguroitu kokonaismuisti näkyy alareunassa.

**Chip RAM ** on saatavilla mukautettuja pelimerkkejä ja tarvitaan alustalla. ** Hitaasti. RAM ** edustaa yhteensopivaa laajennusmuistia, jota käytetään yhteisissä kokoonpanoissa. ** Nopea RAM ** on prosessorisuuntautunut laajennusmuisti. ** Zorro III RAM** Sovelletaan vain malleja, jotka tukevat tätä laajennusarkkitehtuuria. Yhteensopivuusviestit ja käytöstä poistetut ohjaimet estävät yhdistelmiä, joita valittu malli ei voi edustaa.

### ROM

<p align="center"><img src="../images/options-amiga-rom-en.png" alt="Amiga ROM asetukset" width="72%"></p>

Valitse järjestelmä Kickstart ROM, valinnainen laajennettu ROMja ROM Avain. Havaittu...ROM luettelo näyttää nimet, korjaukset ja yhteensopivuus valitun mallin kanssa. Valitse havaittu ROM ja napsauta **Käyttö**, tai selaa tiedostoa manuaalisesti.

ROM tiedostoja ei toimiteta GW GUI. Käytä ROMs sinulla on laillinen oikeus käyttää.

Havaittu luettelo on parempi kuin arvata tiedostonimestä: se raportoi ROM henkilöllisyys ja tarkistus sekä arviointi yhteensopivuudesta valitun mallin kanssa. **Yhteensopiva ** on tavanomainen valinta; ** Osittain yhteensopiva ** osoittaa, että ROM voi käynnistyä, mutta ei täsmää koneeseen. ** Päivitä ** skannaa uudelleen määritetty ROM sijainti. ** Käyttö** määrittää valitun havaitun ROM kokoonpanoon.

### Video

<p align="center"><img src="../images/options-amiga-video-en.png" alt="Amiga videoasetukset" width="72%"></p>

Muokkaa videostandardia, kuvasuhdetta, resoluutiota, linjatilaa, rajaviivan rajaamista, renderointia, värisyvyyttä, rungon hyppyä, gamma- ja välkkymistä. Lisäpiirisarjaasetukset ovat saatavilla sivua alaspäin, kun valittu malli tukee niitä.

| Asetukset | Käytännön vaikutus |
|---|---|
| Videostandardi | Valitsimet PAL tai NTSC ajoitus ja odotettu virkistävä käyttäytyminen |
| Kuvasuhde | Hallitsee, miten emuloitu kuva skaalataan |
| Päätöslauselma | Valitsee automaattisen tai nimenomaisen tulosteen |
| Viivatila | Kontrolloi interlaced tai line-kaksoistehon hoitoa |
| Kasvirajat | Poistaa käyttämättömän yliskannauksen vain kun käytössä |
| Renderointi | Valitsee grafiikan taustaosan |
| Värisyvyys | Valitsee tulostusvärin tarkkuuden |
| Ruudun ohitus | Vähentää renderöityjä kehyksiä, kun käytössä |
| Gamma | Säätää kirkkausvastetta |
| Välkynnän korjaaja | Prosessitilat, jotka muuten välkkyisivät näkyvästi |

Vaihda yksi näyttöasetus kerrallaan. Jos emulointiikkuna muuttuu tyhjäksi tai epävakaaksi, palaa automaattiseen resoluutioon, pois käytöstä - runko hyppää, neutraali gamma ja aiemmin toimiva renderaattori.

### Ääni

<p align="center"><img src="../images/options-amiga-audio-en.png" alt="Amiga ääniasetukset" width="72%"></p>

Ota audio käyttöön tai poista käytöstä, valitse ulostulolaite ja latenssi, määritä sitten interpolointi, Amiga Suodatus, suodatintyyppi, stereoiden erottelu, levyke-ajoääni ja CD-audio-tilavuus.

Pienempi latenssi vähentää viivettä, mutta voi aiheuttaa keskeytyksiä kiireisellä tietokoneella. Lisää sitä, jos ääni särkee. Interpolointi ja Amiga audio suodatin muuttaa äänen toiston sijaan emuloitu ohjelma logiikka. Vetoäänen äänenvoimakkuus säätää simuloitua mekaanista ääntä erillään normaalista Amiga Ääni.

### Varastointi

<p align="center"><img src="../images/options-amiga-storage-en.png" alt="Amiga säilytysasetukset" width="72%"></p>

Tallennussivulla luetellaan laitetunnisteet, -tyypit, -mallit, siihen liittyvät välineet ja käytettävissä olevat toiminnot. Lisää, määritä tai poista laitteita tästä. Levykkeet ja CD-levyt voidaan lisätä tai vaihtaa suoraan käynnissä olevasta koneesta.

• **laitteen tunniste ** Näin emuloitu järjestelmä käsittelee laitetta. ** Tyyppi ** erottaa levykkeet, kovalevy, optinen, ja muut tuetut laitteet. ** Malli ** kuvataan emuloitu laitteisto, kun taas ** Liittyvät viestimet** tunnistaa tällä hetkellä annetun kuvan. Aseta laite ennen kuin kytket arvokasta kirjoitettavaa mediaa ja pidä varmuuskopiot kovalevykuvista.

### Näppäimistö

<p align="center"><img src="../images/options-amiga-keyboard-en.png" alt="Amiga näppäimistön asetukset" width="72%"></p>

Etsi Amiga avaimet ja isäntätehtävät, uusien avaimien antaminen, kartoitusten poistaminen, oletusarvojen palauttaminen tai selvät ristiriidat. Tilasarake ilmoittaa, onko jokainen toimeksianto pätevä.

Vasen sarake nimeää emulgoidun Amiga avain; **Assosiaatio** näyttää isäntäavaimen yhdistelmän. Voimassa oleva kartoitus voi silti olla hankalaa, jos Windows tai sovellus varaa saman pikanäppäimen, joten testaa kriittiset yhdistelmät käynnissä olevan koneen sisällä. Vältä hiirenvapautus- tai koko näytön pikanäppäintä avaimelle, jota emuloitu ohjelmisto tarvitsee usein.

### Hiiri

<p align="center"><img src="../images/options-amiga-mouse-en.png" alt="Amiga hiiren asetukset" width="72%"></p>

Aseta fyysinen hiiren nopeus, valitse mikä analoginen tikku ohjaa hiirtä, säädä analoginen kuollut vyöhyke ja nopeus ja määritä hiiren toimintakartoitukset. Palauta oletusarvot tai selkeät kartoitusristiriidat tarvittaessa.

Lisää kuollutta vyöhykettä, jos ohjain aiheuttaa vinkkerin ajelehtimisen. Säädä vasemman- ja oikeanpuoleinen nopeus itsenäisesti, kun molemmat tikut ovat käytössä. Alempi kartoitustaulukko yhdistää syötteet hiiren painikkeilla tai toimilla; tarkista sen konfliktitila muuttaessaan ohjaimen kartoituksia muualla.

### Valvojat

<p align="center"><img src="../images/options-amiga-controllers-en.png" alt="Amiga ohjaimen asetukset" width="72%"></p>

Havaitse kytketyt ohjaimet, aseta laitteet ja ohjaintyypit Amiga portit, ja määrittää ohjaimen kartoitusten ja turbo-palo asetukset. Käytettävissä olevat valinnat riippuvat havaitusta laitteistosta ja valitusta koneesta.

Portit 1 ja 2 on konfiguroitu itsenäisesti. **Automaattinen** ohjaimen tyyppi on järkevä lähtökohta, mutta tietyn joystickin tai hiiren odottaminen voi edellyttää selkeää tyyppiä. Suorita havainnointi ennen kuin määrität uuden ohjaimen. Turbopalo aktivoi toistuvasti kartoitetun syötteen ja sen pitäisi pysyä poissa käytöstä, ellei peli tai sovellus hyödy siitä.

## Laitteiston vianmääritys ja huolto

Nämä ikkunat avataan **Työkalut ** Lasku. Jokainen ikkuna esikatselee luotuja Greaseweazle Komento. Tarkista se ennen klikkaamista ** Suorita**.

### Rekisterinpitäjän tiedot

<p align="center"><img src="../images/tool-controller-information-en.png" alt="Rekisterinpitäjän tiedot" width="62%"></p>

Näyttää valitun ohjaimen ilmoittamat tiedot. Laajenna **Raakatuotanto** kun tarvitset täydellisen komennon.

Käytä tätä ensimmäisenä diagnostisena komentona. Onnistunut vastaus vahvistaa, että GW GUI voi käynnistää asetellut isäntätyökalut suoritettavaksi ja kommunikoida valitun laitteen kanssa. Tallenna laitteisto- ja laitteistotiedot ennen päivitystä.

### USB kaistanleveys

<p align="center"><img src="../images/tool-usb-bandwidth-en.png" alt="USB kaistanleveys" width="62%"></p>

Toimenpiteet käytettävissä USB viestintäkaistanleveys. Käyttää sitä diagnosoida epävakaita siirtoja tai sopimaton USB yhteys.

Sulje muut ohjelmistot ohjaimella ennen testausta. Toista mittaus vaihtamisen jälkeen USB portti, kaapeli tai solmukohta. Verrataan tuloksia samanlaisissa olosuhteissa sen sijaan, että käsiteltäisiin yhtä mittausta absoluuttisena takuuna.

### Käyttönopeus

<p align="center"><img src="../images/tool-drive-speed-en.png" alt="Käyttönopeus" width="62%"></p>

Mittaa ajon pyörimisnopeuden. Lisää mittausten määrää, kun tarvitset edustavamman tuloksen.

Yksi mittaus on nopea tarkistus; useat mittaukset osoittavat, onko nopeus vakaa. Anna ajon saavuttaa normaali nopeus ennen tuloksen tulkintaa. Odottamaton arvo voi osoittaa väärän nopeuden, mekaanisen ongelman tai mittausjärjestelmän ongelman.

### Etsi pää

<p align="center"><img src="../images/tool-seek-head-en.png" alt="Etsi pää" width="62%"></p>

Siirtää aseman pään valittuun sylinteriin. **Salli äärimmäiset sylinterit ** luvat, jotka ovat tavallisesti rajoitettuja, ja ** Pidä moottori käynnissä** jättää moottorin käynnissä operaation aikana. Käytä äärimmäisiä asentoja vain silloin, kun laitteistomenettely nimenomaisesti vaatii niitä.

Normaali etsiminen on hyödyllistä vahvistaa pään liikkeen tai sijainnin ennen diagnoosia. Kuuntele poikkeavia toistuvia iskuja ja pysähdy, jos pyydetty sylinteri ei sovi ajolle. Tämä työkalu ei lue eikä validoi kohdesylinterin tietoja.

### Aja linjausdiagnostiikkaa

<p align="center"><img src="../images/tool-drive-alignment-en.png" alt="Aja linjausdiagnostiikkaa" width="62%"></p>

Se lukee usein ajolinja-analyysiä varten. Se tukee kappaleiden valintaa, vallankumousta ja lukumääriä, dekoodausmuotoa, raakavuota, indeksiä, nopeutta, PLL, tiheys-pin, kova-ala, TG43, ja käänteisdata vaihtoehtoja. Tasaustyö edellyttää asianmukaista viitemediaa ja laitteistotietoa.

Aloita tunnetulla referenssilevyllä ja pienimmillä ohitussarjoilla. **Vaihdetaan raitoja ** määritellään kappaleet ja päät, joista näytteet on otettu; ** Kierrokset raitaa kohti ** tarkastaa kunkin näytteen keston; ** Lukumäärä** määrittää toiston. Käytä muokattua levyn määritelmää tai dekoodausmuotoa vain silloin, kun se vastaa viitemediaa. Valeindeksi, kovat sektorit, PLL ohituslaitteet, tiheystapit ja TG43 ovat laitteisto- tai muotokohtaisia ja voivat mitätöidä vertailun väärin käytettynä.

### Laitteiston nastat

<p align="center"><img src="../images/tool-hardware-pins-en.png" alt="Laitteiston nastat" width="62%"></p>

Luee tai muuttaa tuettua ohjaimen piniä. Valitse tappi, ota käyttöön **Vaihda piniä ** vain kirjoittaessaan arvoa ja valitse ** Korkea taso** jos suunniteltu laitteiston käyttö sitä edellyttää.

Kun **Vaihda piniä** Ei käytössä, komento kuulustelee sokkaa. Tämä on turvallisempi oletus. Tason muuttaminen vaikuttaa suoraan ohjaimeen I/O ja se tulisi tehdä vain oikealla Greaseweazle laitteiston dokumentointi ja siihen liittyvät johdot.

### Nollaa ohjain

<p align="center"><img src="../images/tool-reset-controller-en.png" alt="Nollaa ohjain" width="62%"></p>

Palauttaa Greaseweazle ohjain. Käytä tätä, kun ohjain havaitaan, mutta ei enää vastaa normaalisti.

Odota minkä tahansa aktiivisen levyn toiminnan päättymistä ennen uudelleenasettamista. Skannaa ohjain uudelleen, jos sen yhteystila ei palautu automaattisesti. Nollaus ei korjaa väärää `gw.exe` polku tai irrotettu USB laite.

### Viivästykset

<p align="center"><img src="../images/tool-delays-en.png" alt="Valvojan viivästykset" width="62%"></p>

Lukee tai muuttaa ohjaimen ajoitusarvoja, kuten valinta, pään askel, asettua, moottori, automaattinen valinta, kirjoittaa ajoitus, ja indeksi maski viiveitä. Ota käyttöön vain arvot, joita aiot muuttaa.

Tarkastamattomat kentät jättävät vastaavan säätimen arvon ennalleen. Ennen muokkausta tallenna olemassa olevat arvot. Ajoitusmuutokset voivat vaikuttaa jokaiseen myöhempään fyysiseen toimintaan, joten testaa käytetyllä medialla ja palauta tunnetut hyvät arvot, jos käyttäytymisestä tulee epäluotettavaa.

### Firmware

<p align="center"><img src="../images/tool-firmware-en.png" alt="Firmware-päivitys" width="62%"></p>

Päivittää ohjainohjelmistoa. **Päivitä bootloader** on nimenomaisesti merkitty riskialttiiksi, ja sen olisi pysyttävä toimintakyvyttömänä, ellei virallinen ohjelmistomenettely sitä edellytä. Älä irrota ohjainta päivityksen aikana.

Ennen päivitystä vahvista, että **Rekisterinpitäjän tiedot**, käytä vakaa suora USB yhteys, ja sulje muita ohjelmistoja, jotka voivat käyttää sitä. Valmistuttuaan, kytke tai skannaa uudelleen rekisterinpitäjä ja lue sen tiedot uudelleen tarkistaa raportoitu firmware versio.

## Lokit ja toimintahistoria

Avaa toimintahistoria tarkistaa tallennetut lokit operoinnin.

<p align="center"><img src="../images/operation-history-en.png" alt="Toimintahistoria" width="68%"></p>

Valitse vasemmalta loki näyttääksesi sen sisällön. **Vie** tallentaa kopion diagnostiikkaa tai tukea varten. Polut ja komentorivit voivat sisältää henkilökohtaisia kansioiden nimiä, joten tarkista viedyt lokit ennen niiden jakamista.

Pääikkunan live-konsoli näyttää nykyisen komennon ja viimeisimmän ulostulon. Sen kopiointipainike kopioi näytetyn tekstin.

### Lokin lukeminen

Hyödyllinen vianmääritysloki sisältää luodun komennon, aikaleimat, moottorin ulostulon ja lopullisen tilan. Virkkaa alareunasta ylöspäin: tunnista lopullinen virhe ja paikanna sitten sitä edeltänyt ensimmäinen varoitus tai epäonnistunut kappale. Myöhempi yleinen vika on usein vain seurausta aikaisemmasta, täsmällisemmästä viestistä.

Kun vertaat kahta yritystä, tarkista, että ohjain, ajaa, moottori, profiili, lähdepolku, lähtömuoto, ja asiantuntija argumentit olivat identtisiä. Muussa tapauksessa erilainen tulos voi heijastella muuttuneita asetuksia eikä levyn epävakautta.

## Sovellustiedot ja kannettava käyttö

GW GUI pitää käyttäjän tiedot erillään sovellus binäärit. Valitusta paketista ja tilasta riippuen asetukset, lokit, ladatut työkalut, emulaattorin komponentit, sieppaukset, tilat ja konekokoonpanot tallennetaan joko sovellukseen `Data` kansiossa tai konfiguroiduissa käyttäjätietopaikoissa.

Ennen kuin korvaat kannettavan asennuksen tai liikutat sitä, pidä koko hakemuskansio koossa ja varmuuskopioi `Data` kansio. Älä siirrä yksittäisiä tiedostoja `lib`, koska sovellus ratkaisee omat ja kolmannen osapuolen kirjastot tästä rakenteesta.

### Ehdotettu varmuuskopion sisältö

Varmista seuraavat, kun ne ovat tärkeitä työnkulun kannalta:

- sovellusasetukset ja -profiilit
- lennonjohtajan ja kuljettajan määritelmät;
- emulointikokoonpanot;
- ROM polut ja laillisesti hallussa ROM varmuuskopioita;
- kovalevyiset ja irrotettavat mediakuvat;
- kaappaukset ja pelastetut valtiot
- säilytysrekistereinä käytetyt toimintalokit.

Levykuvat voivat olla paljon suurempia kuin asetukset. Tallenna arkiston masters luku vain mahdollisuuksien mukaan, ja työstä kopioita.

## Suositeltu työnkulku

### Tuntemattoman levyn arkistointi

1. Tarkista ja puhdista asema asianmukaisella huoltotoimenpiteellä.
2. Kirjoita suojalevylle, jos mahdollista.
3. Valitse **Lue > Raakakuva (SCP)**.
4. Käytä kuvailevaa tiedostonimeä ja lue normaali rata-alue monilla vallankumouksilla.
5. Tarkista konsoli ja tallennettu loki.
6. Tarkasta molemmat puolet **Visualisointi**.
7. Muuta kopio todennäköiseen sektorimuotoon.
8. Testaa muunnetut kopiot **Disk Explorer** tai sopiva ohjelmisto.
9. Säilytä raaka-mestari, loki ja muistiinpanot yhdessä.

### Levyn uudelleenluonti kuvasta

1. Tarkista kuva ja vahvista sen odotettu perhe ja muoto.
2. Lisää käyttökelpoinen tai tarkoituksella kirjoitettava levy oikea koko ja tiheys.
3. Avaa **Kirjoita** ja valitse kuva.
4. Vahvista määritetty asema ja havaittu muoto.
5. Kirjoita levyke.
6. Lue se erilliseen varmistuskuvaan.
7. Vertaa purettua sisältöä ja tarkista epäilyttäviä kappaleita visuaalisesti.

### Emuloidun emulgoinnin luominen Amiga

1. Avaa **Vaihtoehdot > Emulsio > Asetukset** ja luoda tai valita koneen.
2. Sisään **Amiga > Yleistä**, valitse malli ja emulaattori versio.
3. Määrittele yhteensopiva, laillisesti hankittu ROM.
4. Pidä mallin oletusarvot CPU sekä RAM Ensimmäisessä kengässä.
5. Muokkaa videota ja ääntä konservatiivisilla automaattisilla asetuksilla.
6. Lisää tallennuslaitteet ja liitä kopioidut mediakuvat.
7. Tarkista näppäimistön, hiiren ja ohjaimen tehtävät.
8. Tallenna asetukset.
9. Palaa **Emulsio **, valitse se, ja napsauta ** Avaa**.
10. Vasta onnistuneen lähtötason käynnistyksen, kiihdytyksen tai edistyneiden asetusten jälkeen yksi kerrallaan.

## Turvalista

Ennen **Lue**:

- lähdelevy on oikeassa asemassa;
- lähde on mahdollisuuksien mukaan kirjoitussuojattu;
- lähtötie ei korvaa olemassa olevaa päällikköä;
- profiili ja kappalealue vastaavat levyä.

Ennen **Kirjoita ** tai ** Poista**:

- kohdelevy voidaan tuhota;
- kuva ja asema ovat oikein;
- levyn koko ja tiheys ovat yhteensopivia;
- Kohdena ei käytetä arkistonhoitajaa.

Ennen laitteistonvaihtotyökalua:

- mikään muu toimenpide ei ole käynnissä;
- oikea ohjain on valittu;
- nykyiset arvot on kirjattu;
- ohjaimella on vakaa teho ja USB yhteydet;
- toimea tukevat laitteistoasiakirjat.

## Vianmääritys

### Rekisterinpitäjää ei ole listattu

1. Yhdistä ohjain suoraan tietokoneeseen.
2. Avaa **Vaihtoehdot > Ohjaimet ja asemat**.
3. Klikkaa **Etsi**.
4. Tarkista ohjaimen tila ja aseman asetukset.
5. Juokse **Rekisterinpitäjän tiedot** jos havaitseminen onnistuu, mutta käskyt epäonnistuvat.

Jos se ei vieläkään näy, kokeile toista suoraa USB Portti ja kaapeli, sitten uudelleenskannaus. Tarkista Windows-laitteen hallinta uudesta sarjalaitteesta. Ohjain näkyy Windows mutta puuttuu GW GUI yleensä osoittaa kiireinen portti, vaikea kokoonpano, tai Host Tools ongelma; ohjain puuttuu Windows osoittaa USB, teho, kuljettaja, tai laitteisto.

### `gw.exe` ei löytynyt

Avaa **Vaihtoehdot > Ohjaimet ja asemat **, sitten käyttää ** Etsi gw.exe **, ** Valitse **, tai ** Lataa uusin versio**. Vahvista, että havaittu polku osoittaa aiottuun Greaseweazle asennus.

Valittuasi sen, suorita **Rekisterinpitäjän tiedot**. Jos tämä ei onnistu ennen kuin otat yhteyttä laitteistoon, tarkista, onko loki virheellinen suoritettava polku, puuttuvat tiedostot tai versio, joka ei voi käynnistyä.

### Toiminta käyttää väärää moottoria

Avaa **Vaihtoehdot > Moottorit** ja tarkistaa koneen tähän tarkoitukseen. GW GUI ei hiljaa putoa takaisin toiseen moottoriin.

Moottorin asetukset ovat erillisiä: muuntomoottorin vaihtaminen ei muuta lukemista, kirjoittamista tai Disk Explorer. Avaa epäonnistuva toiminto uudelleen tallennuksen jälkeen ja vahvista luotu komento konsolissa.

### Kuvaa ei tunnisteta

Poista automaattinen tunnistus käytöstä vain, jos tiedät oikean koneen ja muodon. Muuten kokeile **Visualisointi** välilehti tarkastaa kuvan alemmalla tasolla.

Tarkista, onko lähde raakavuon talteenotto, alakohtainen kuva, pakattu kontti, tai etuyhteydetön tiedosto harhaanjohtava laajennus. Älä koskaan uudelleen nimeä laajennusta vain pakottaaksesi havaitsemaan; muuntaminen tulee tulkita lähderakennetta oikein.

### Emulsio ei käynnisty

Tarkista tallennettu asetus, asennettu emulaattoriversio, valittu ROM, varastointireitit, ja mallin yhteensopivuus. Tarkista hakemusloki täydellinen virhetiedot.

Väliaikaisesti palautettava CPU, RAM, video, ja varastointi yksinkertainen malli-yhteensopiva perusviiva. Jos perustaso alkaa, palauta yksi mukautettu asetus kerrallaan. Tallennettu tila, joka on luotu toisella emulaattoriversiolla tai konemäärityksellä, voi myös epäonnistua, vaikka puhdas saapas toimisi.

### Pikanäppäin tai syöte ei toimi

Tarkista molemmat globaalit **Emulsio > Pikanäppäimet** sivu ja konekohtainen näppäimistö, hiiri tai ohjain sivu. Selvitä kaikki tehtävät, jotka on merkitty ristiriitaisiksi.

Jos hiiri napataan, käytä juoksukoneen työkalupalkissa näkyvää julkaisun pikanäppäintä. Jos ohjain liitettiin Optionsin avaamisen jälkeen, suorita ohjaintunnistus uudelleen ennen sen antamista.

### Komento epäonnistuu odottamatta

1. Lue live-konsolin tuloste.
2. Avaa **Toimintahistoria** täydellinen tallennettu loki.
3. Vahvista valitut ohjaimet, asema, profiili, moottori ja tiedostopolut.
4. Vie kyseinen loki, jos se on jaettava diagnoosia varten.

### Äänenvaimentimet tai tauot

Lisää emulointi äänen latenssi, sulje CPU- intensiiviset sovellukset, ja palauttaa videokehys ohittaa ja nopeuttaa niiden aiemmat arvot. Varmista, että suunniteltu Windows-äänilaite on valittu. Muutetaan asetus kerrallaan, jotta tehokas korjaus voidaan tunnistaa.

### Emulointinäyttö on tyhjä tai hidas

Palautusresoluutio ja linjatila **Automaattinen**, poistaa rungon ohitus ja välkkyminen kiinni tilapäisesti, ja kokeile aiemmin toimiva renderaattori. Vahvista, että määritetty ROM ja lisätty saappaat ovat voimassa. • FPS indikaattori auttaa erottamaan renderointi-suoritus ongelma koneesta, joka ei yksinkertaisesti ole käynnistynyt.

### Luku sisältää epävakaita kappaleita

Toista lukeminen uudelle tiedostonimelle, lisää tarvittaessa vallankumouksia ja vertaa näitä kappaleita. Puhdista asemapäät oikealla menettelyllä ja tarkista levyltä fyysiset vauriot. Älä lue toistuvasti näkyvästi irtoaminen tai vaurioitunut media, koska edelleen kulkee voi pahentaa sitä.

## Sanasto

| Termi | Merkitys GW GUI |
|---|---|
| Kontrolleri | • Greaseweazle laitteistoliitäntä kytketty yli USB |
| Asema | Ohjaimeen kiinnitetty fyysinen levykeasema |
| Moottori | Toimen toteuttamiseen valittu toteutus |
| Flux | Levyltä luettavaa magneettista siirtymää kuvaavien tietojen ajoitus |
| Raakakuva | Tallennus säilyttää matalan tason levytiedot, kuten SCP |
| Alan kuva | Loogisille aloille järjestetty koodattu edustus |
| vallankumous | Yksi täydellinen kiertonäyte raidan lukemisen aikana |
| Sylinteri | Radiaalinen pään asento; yksi sylinteri voi sisältää raiteen kummallakin puolella |
| Pää | Fyysisen aseman valitsema levyn puoli |
| Profiili | Toiminnon uudelleenkäytettävät asetukset |
| ROM | Emuloidun koneen vaatima firmware-kuva |
| Tallennettu tila | Kuva käynnissä olevasta emulaattorista |
| Renderöijä | Emulointilähdön näyttämiseen käytetty grafiikkatausta |

## Nopea viite

| Jos haluat... | Mene... |
|---|---|
| Säilytä fyysinen levy | **Lue** |
| Laita kuva takaisin levylle | **Kirjoita** |
| Tuota toinen kuvamuoto | **Muuntaminen** |
| Tarkasta raiteet tai vuopoikkeamat | **Visualisointi** |
| Selaa tiedostoja kuvan sisällä | **Disk Explorer** |
| Tarkastusohjaimen viestintä | **Työkalut > Rekisterinpitäjän tiedot** |
| Mittaa ajon kierto | **Työkalut > Käyttönopeus** |
| Tarkista edellinen komento | **Toimintahistoria** |
| Aseta laitteisto | **Vaihtoehdot > Ohjaimet ja asemat** |
| Valitse toteutukset | **Vaihtoehdot > Moottorit** |
| Luo tai muokkaa emuloitua konetta | **Vaihtoehdot > Emulsio** |
| Käynnistä tallennettu kone | **Emulsio** |
