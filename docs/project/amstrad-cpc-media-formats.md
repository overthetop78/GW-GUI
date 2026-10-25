# Formats du corpus Amstrad CPC

Corpus observé en lecture seule : `F:\Retro\A Trier\Amstrad CPC`.

## Inventaire du 27 septembre 2026

| Extension | Fichiers | Nature démontrée par le contenu |
|---|---:|---|
| `.bin` | 102 | ROM CPC brute : principalement 16 Kio, plus 8 Kio, 32 Kio et cartouches brutes de 128 Kio. |
| `.cpr` | 43 | Cartouche CPC Plus RIFF dont le type est `AMS!` et les banques sont des chunks `cbNN`. |
| `.dsk` | 5 886 | Images CPCEMU DSK standard ou étendues. |
| `.hxcstream` | 616 | Pistes de flux HxC/Pauline portant la signature `CHKH`, regroupées par répertoire et nommées `trackNN.S.hxcstream`. |
| `.raw` | 84 | Pistes de flux KryoFlux d’une même disquette, nommées `...NN.S.raw`, avec enregistrements OOB et métadonnées de capture. |
| `.cdt` | 151 | Bandes CPC Digital Tape utilisant le conteneur TZX. |
| `.tzx` | 1 | Bande TZX compatible avec la structure CDT. |
| `.wav` | 23 | Captures audio PCM mono de bandes CPC. |
| `.mp3` | 1 | Capture audio compressée de bande CPC. |
| `.rom` | 6 | Cartouches Dandanator de 128 ou 512 Kio. |
| `.sna` | 50 | Instantanés CPCEMU/Caprice32 : contenus d’émulation, hors bibliothèque de médias. |

Les répertoires sans extension entre crochets contiennent principalement des `.dsk`; leur format est déterminé par chaque fichier et non par le nom du répertoire.

## Comportement attendu

- Une famille de fichiers HxC Stream ou KryoFlux constitue une seule image de disquette multipiste. Le lecteur sélectionne automatiquement les fichiers frères sans les modifier.
- Les flux HxC Stream, KryoFlux et SCP utilisent la même représentation de flux du moteur. Ils alimentent le même visualisateur, les mêmes décodeurs de secteurs et les mêmes conversions compatibles.
- Les fichiers audio sont décodés en PCM puis exposés comme bande séquentielle. Le visualisateur affiche leur signal ; le décodeur CPC retrouve les blocs et vrais noms enregistrés sur la bande pour l’explorateur et les conversions CDT/TZX/WAV compatibles.
- L’extension `.tap` déjà prise en charge par le moteur désigne le conteneur de blocs Spectrum. Aucun fichier `.tap` CPC ni aucune structure CPC distincte n’est démontré par ce corpus : elle ne doit donc pas être attribuée au CPC par simple égalité d’extension. Les sorties CPC documentées restent CDT/TZX, WAV et VOC.
- Une ROM brute n’est pas transformée arbitrairement en disquette ou cassette. Elle est reconnue comme mémoire/cartouche selon sa structure et sa capacité.
- Une CPR conserve ses chunks RIFF et ses banques réelles. Une conversion CPR vers ROM brute peut concaténer les banques dans leur ordre d’adresse ; l’opération inverse exige des banques de taille valide.
- Les banques CPR/ROM sont des plages distinctes de la représentation bloc commune. Le visualisateur bloc existant conserve donc chaque frontière de banque sans traitement graphique propre à l’Amstrad.
- Un instantané `.sna` CPCEMU/Caprice32 est un contenu de démarrage propre à l’émulateur. Il n’est ni lu, ni exploré, ni visualisé par MediaEngine.
- Toute entrée exposée par l’explorateur, notamment les banques CPR/ROM, passe par `FileSystemEntryAnalyzer`; la classification reste ainsi la responsabilité de `GWGUI.MediaAnalysis` sans logique de classification ajoutée dans MediaEngine ou dans l’application.

Références de format utilisées : implémentation officielle HxCFloppyEmulator pour HxC Stream, implémentation Greaseweazle et documentation KryoFlux Stream Protocol pour KryoFlux, spécification CDT/TZX et structure RIFF AMS! des cartouches CPC Plus.
