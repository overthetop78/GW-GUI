# Sources de bruitages mécaniques réels

## État fonctionnel livré le 27 octobre 2026

Le build Debug Acorn est prêt dans `build/Debug/GW GUI/gwgui.exe`. Les moteurs, déplacements de têtes, accès, arrêts et manipulations de supports utilisent les FLAC externes communs, avec réglage du volume des lecteurs. Les résonances des seeks rapides se chevauchent sans retard cumulatif. Le mécanisme du magnétophone utilise désormais dix clips réels ; lecture et enregistrement partagent le défilement, sans prise distincte du bouton REC.

Validation finale : build réussi sans avertissement XML après documentation du volume ; 104 tests réussis contre les DLL livrées (contrôleurs 8271/WD1770/SCSI, audio, cassette et contrôles supplémentaires des volumes et événements dans le cœur). Les 1 759 FLAC livrés ont les mêmes empreintes que les sources. Les 1 759 WAV d’écoute sont dans `artifacts/audio-preview`, rangés par type/marque/modèle. Les sorties et scripts de validation temporaires ont été supprimés. La sortie native WASAPI a également été contrôlée avec le SDK et les clips livrés, sans ouvrir de fenêtre GW GUI.

La couverture fonctionnelle BBC Model A et la réutilisation commune sont vérifiées. L’association exhaustive des références commerciales aux machines des 24 modules n’est pas démontrée par ces tests. Les mécanismes dont la référence est inconnue restent explicitement sous `Unidentified`. Les sections suivantes conservent les sources et les étapes de recherche ; les mentions de travail restant dans les étapes anciennes sont à lire avec cet état livré.


Demande du 26 octobre 2026 : remplacer la synthèse approximative, couvrir toutes les opérations et fournir des WAV audibles avec atténuation utilisateur. Les anciens exports ont été remplacés par les WAV décodés des FLAC définitifs décrits dans l’état livré.

## Références primaires consultées

- https://86box.net/2025/10/26/86box-v5-2.html : annonce de sons de disquettes enregistrés sur des lecteurs réels, déclenchés par le moteur et les déplacements des têtes.
- https://github.com/86Box/assets : dépôt officiel des ressources, avec un répertoire `sounds`. Les fichiers et références exactes sont conservés dans le manifeste de recherche et les définitions explicites des ensembles sources.
- https://github.com/86Box/docs/blob/master/usage/roms.rst : documentation officielle des ressources audio de disquette et de disque dur.
- https://github.com/mattgodbolt/jsbeeb/blob/main/CHANGELOG.md : évolution du déclenchement du bruit de lecteur suivant les déplacements des têtes et clic de relais cassette. Le clic du relais de l’ordinateur ne remplace pas l’enregistrement du mécanisme du magnétophone externe.

## Couverture à vérifier pour chaque référence choisie

- Bande : insertion, éjection, enclenchement lecture, enregistrement, rotation en lecture/enregistrement, avance rapide, retour rapide, pause et arrêt ; distinguer mécanisme et signal enregistré.
- Disquette : insertion, verrouillage éventuel, éjection, démarrage/arrêt et rotation moteur, déplacements des têtes et accès en lecture/écriture. Les différences audibles doivent provenir de la mécanique réelle, pas d’un bruit artificiel de données.
- Disque dur : démarrage, rotation, déplacements, accès lecture/écriture, arrêt et stationnement suivant le matériel ; aucune insertion/éjection artificielle pour un disque fixe.
- Optique : tiroir ou fente, insertion/éjection, démarrage, rotation et changements de régime, recherche, accès, arrêt ; écriture uniquement pour un graveur concerné.

## Périmètre confirmé

L’utilisateur demande tous les lecteurs associés aux machines des 24 modules, y compris plusieurs modèles pour une machine. Les catégories actuelles ne constituent pas des références commerciales : inventorier les déclarations matérielles du dépôt avant d’attribuer des enregistrements.

## Volume confirmé

50 % est la référence normale du profil ; 100 % doit être nettement plus fort. Prévoir une amplification au-delà de 50 %, sans saturation. La pression acoustique réelle dépend des haut-parleurs, de leur réglage et de la distance d’écoute ; elle ne se déduit pas du niveau PCM d’un enregistrement non calibré.

## Catalogue public 86Box relevé

L’arbre et les configurations officiels sont conservés dans `artifacts/mechanical-audio-research`. Les enregistrements originaux sont également conservés dans le sous-dossier `recordings`. Le catalogue relevé comporte quatre lecteurs de disquettes et quatre disques durs ; aucun jeu cassette ni optique n’y figure.

| Référence du dossier source | WAV disponibles | Volume total (Mio) |
| --- | ---: | ---: |
| 3.5_Alps_Electric_Co_Ltd_DF354H148F_1.44M_80tracks | 164 | 4.00 |
| 3.5_Epson_SD-800_1.44M_80tracks | 159 | 2.51 |
| 5.25_Epson_SD-700_1.2M_80tracks | 159 | 2.71 |
| Panasonic_JU-475-5_5.25_360k_40tracks | 80 | 1.52 |
| 1987 Miniscribe 8425 | 4 | 0.91 |
| 1992 Seagate ST3120A | 5 | 3.80 |
| 1993 IBM H3171 | 4 | 1.50 |
| 1997 Quantum Bigfoot 2110AT | 4 | 3.47 |

Les profils de disquettes distinguent démarrage, boucle de rotation, arrêt et déplacements dans les deux directions pour plusieurs distances. Ces fichiers ne couvrent pas à eux seuls insertion/éjection ni toutes les machines du dépôt. Les profils HDD décrivent démarrage, rotation, arrêt et déplacement d’une piste. Une lecture ou écriture de données ne justifie pas un signal artificiel ajouté au moteur : restituer l’activité mécanique réellement observée.

Autres sources primaires trouvées :

- https://github.com/tonioni/WinUAE/blob/master/README_unix.md : utilisation officielle de `drive_sounds.zip` et des ensembles de sons externes, avec le jeu A500 intégré.
- https://github.com/FrodeSolheim/fs-uae/blob/main/docs/options/floppy_drive_0_sounds : jeux A500, A500Real, ST et LOUD. A500Real et ST sont des pistes à examiner ; LOUD n’est pas un modèle commercial et ne doit pas être présenté comme tel.

Les correspondances exactes des références sources avec les lecteurs des machines restent à vérifier. Aucun profil générique existant n’a été remplacé par le son d’un matériel arbitraire, et aucun nouvel export n’est présenté comme complet.

### Précision utilisateur sur les niveaux

Les seuils −50 dB minimum et 0 à −10 dB maximum décrivent les sons sources à rendre audibles. Ils ne définissent pas une plage du curseur. Conserver une atténuation classique pour l’écoute, par exemple à 20 %. Le choix de −3 dBFS proposé pour les crêtes des exports reste un choix de réalisation dans la plage demandée, pas une mesure acoustique du lecteur réel.

## Import effectif des originaux

1 749 WAV ont été importés sans erreur depuis 86Box/assets, RetroDriveSounds, jsbeeb et WinUAE (environ 98,5 Mio). Le manifeste `artifacts/mechanical-audio-research/recordings-manifest.json` conserve le commit source, le chemin original, les empreintes Git/SHA-256, la fréquence, les canaux, la précision et la crête de chaque fichier. Tous ont pu être lus comme PCM ; aucun n’a une crête inférieure à −50 dBFS. Les originaux ne sont pas normalisés.

Outre les huit références 86Box, les sources comprennent sept variantes de lecteurs 3,5 pouces Flopster (NEC, TEAC, Mitsumi, Samsung), des enregistrements d’insertion/éjection Panasonic et Alps, des jeux BBC/Amiga et des jeux de mécanismes IBMulator/MAME. Les jeux génériques dont le modèle exact n’est pas identifié doivent rester explicitement identifiés par leur provenance, sans leur attribuer un modèle commercial arbitraire. Aucun enregistrement de mécanisme cassette n’est présent dans ces quatre dépôts importés.

La règle des niveaux porte exclusivement sur les crêtes : ne pas relever les silences ni les passages faibles, ne pas compresser la dynamique pour atteindre le seuil. Le rendu final du mélange doit être protégé de l’écrêtage.

### Correction de niveau : aucune normalisation

L’utilisateur a précisé que les lecteurs silencieux doivent rester plus silencieux que les autres. Le calcul de gain par profil a donc été retiré du préparateur et du rendu ; les WAV restent identiques aux sources et le catalogue reprend leur crête d’origine. Aucun gain correctif par échantillon/profil n’est appliqué. Les anciens paragraphes proposant une cible uniforme de −3 dBFS décrivent une proposition abandonnée, pas le fonctionnement retenu.


## Banque externe : classement et lecture FLAC

La banque distribuée est `src/GWGUI.MediaEngine/Assets/MechanicalAudio/<type>/<marque>/<modèle>/<bruitage>.flac` : types `Floppy`, `HardDisk`, `Optical` et `Tape` pour les enregistrements désormais disponibles. Insertion, éjection et moteur restent dans le dossier du modèle ; les séries Seek et Buzz ont leurs sous-dossiers. Les mécanismes non identifiés restent sous `Unidentified`, sans inventer leur marque. Les fichiers ne répètent pas la marque et le modèle.

Les 1 749 FLAC occupent 48 146 242 octets contre 103 320 114 octets pour les WAV sources. Le PCM décodé a été comparé bit à bit aux originaux lors de la conversion. Les niveaux des enregistrements récupérés sont conservés. La réserve utilisateur sur la normalisation concerne les sons à créer, afin de préserver les différences entre lecteurs silencieux et bruyants.

`catalog.json` a ensuite été supprimé : le chargement utilise directement les catalogues C# typés. Les fichiers `provenance.json` et `preparation-report.json` ont été supprimés et leur génération retirée. Le manifeste de recherche existant conserve les sources originales. Le préparateur génère trois catalogues dans `Constants/MechanicalAudio/` : `RecordedMechanicalSoundFiles.Floppy.cs`, `.HardDisk.cs` et `.Optical.cs`. Chacun contient les dictionnaires des matériels, regroupés par marque/modèle et ordonnés par nom de bruitage, avec identité et chemin complet du fichier sur la même ligne. Les définitions sont maintenant séparées par modèle dans les dossiers de types et de marques. `RecordedMechanicalSoundFiles.cs` ne contient plus de liste d’empreintes : il réunit les catalogues par type et construit l’index utilisé par le chargeur. Les catalogues C# associent les fichiers aux opérations ; les caractéristiques audio sont lues dans les FLAC.

La lecture utilise NAudio 2.3.0 déjà référencé et `StreamMediaFoundationReader`, avec le décodeur FLAC de Windows Media Foundation ; l’encodeur FLAC externe sert uniquement à préparer les assets. App distribue une seule banque externe, sans intégration des sons dans une DLL ni duplication dans chaque module.

Après déplacement, les 1 749 fichiers ont été vérifiés par SHA-256 et confrontés aux chemins du catalogue. La validation isolée des sources C# a réussi pour 24 profils et 153 opérations, avec lecture FLAC via les constantes, continuité des fragments, volume et protection du mélange. Le script temporaire et son résultat JSON ont été supprimés. Aucun build de GW GUI ni remplacement de l’application en cours d’essai n’a été effectué. Ce contrôle ne remplace pas la validation du build complet et ne signifie pas que tous les modèles et opérations manquants sont terminés.

Contrôle isolé après ce découpage : les sources C# des quatre fichiers de catalogues ont été compilées en mémoire ; l’agrégation contient les 1 744 identités distinctes attendues pour les 1 749 fichiers, et chaque chemin référencé existe. Les quelques identités communes correspondent à des contenus source identiques. Aucun fichier temporaire ni build de l’application.


## Reconstruction complète des fichiers et identifiants — 26 octobre 2026

À la demande explicite de l’utilisateur, l’ancienne banque générée et les quatre catalogues C# ont été supprimés avant reconstruction depuis `artifacts/mechanical-audio-research/recordings`. Les paragraphes précédents relatifs aux anciens identifiants SHA-256, aux collisions « alternative » et à leur ancien index décrivent une réalisation remplacée.

Le préparateur est réécrit. `scripts/tools/mechanical-audio/source-sets.json` attribue explicitement les 26 ensembles sources à leurs matériels ; les dossiers anonymes de plusieurs sources ne sont plus fusionnés automatiquement sous une même référence. `sound-names.json` définit les noms et opérations fixes. Toute collision ou source non définie arrête la préparation : aucun suffixe numéroté n’est inventé pour la masquer.

Les noms décrivent les événements : Disk-insert, Disk-eject, Drive-open/close, Tray-open/close, Motor-start/running/stop. Les déplacements `1up` et `1down` deviennent respectivement `Seek/Toward-center/1-track.flac` et `Seek/Toward-edge/1-track.flac`. La distance et la direction restent aussi dans les métadonnées. Up/down désignent le mouvement entre pistes, pas les faces. Les fichiers source indexés seek_XX/buzz_XX restent des motifs enregistrés distincts, sans transformer leur indice en distance inventée ; step_XX désigne le pas associé à la piste précisée par la source.

Chaque identité est maintenant le chemin descriptif sans extension, par exemple `Floppy/Alps Electric/DF354H148F/Seek/Toward-center/1-track`. Les dictionnaires par matériel et par type relient directement cette identité au fichier FLAC ; le catalogue principal les agrège. Les empreintes servent uniquement à vérifier l’intégrité des originaux. Les définitions exécutées ne conservent aucune provenance ni empreinte source. Aucun fichier provenance ou preparation-report n’est recréé.

Résultat : 1 749 FLAC recréés, 1 749 identités lisibles distinctes, 24 profils, PCM décodé identique à chaque original sans normalisation. La validation isolée des sources réelles a vérifié 173 opérations décodées à 44,1 kHz, la continuité entre fragments et le silence au volume zéro. Chaque déplacement up/down a été confronté au nom original, à sa distance et à sa direction. Le script temporaire a été supprimé. Aucun build de GW GUI.

Cette reconstruction corrige l’organisation et l’identification des enregistrements disponibles. La couverture des lecteurs et opérations encore absents des sources reste à compléter ; elle n’est pas déclarée terminée par ce contrôle.


## Bases des lecteurs sans son spécifique

Le service commun utilise les bases suivantes, explicitement choisies plutôt que déduites de l’ordre alphabétique :

- Floppy : Alps Electric DF354H148F (`Floppy35`).
- HardDisk : Seagate ST3120A (`HardDisk35`), choisi parmi les enregistrements disponibles pour le moteur, le démarrage, l’arrêt et le déplacement des têtes.
- Optical : le seul ensemble disponible de lecteur CD-ROM à tiroir (`OpticalTray`), dont la référence commerciale reste inconnue.

Le profil de base est placé en tête des choix et utilisé lorsque le profil demandé est absent ou non renseigné. Pour un profil enregistré spécifique, chaque opération disponible reste issue de ce profil ; une opération manquante utilise celle de la base de sa catégorie. La durée de l’événement utilise le même repli que le rendu. Si la base ne possède pas non plus cette opération, aucun son supplémentaire n’est inventé. Les réglages d’activation et de volume restent appliqués par leurs consommateurs habituels.

Validation isolée sur les sources réelles : les trois bases, le rendu sans profil et avec profil absent, les durées correspondantes, le repli de l’arrêt Epson SD-800 vers Alps et la conservation de son déplacement spécifique ont réussi. Le script temporaire a été supprimé ; aucun build de GW GUI.

## Lecture directe des FLAC et raccordement mécanique en cours

Cette section décrit le fonctionnement actuel ; les descriptions précédentes du JSON et des scripts de préparation décrivent les étapes remplacées. Les scripts temporaires sous `scripts/tools` ont été supprimés à la demande de l’utilisateur.

`CreateSample` a été supprimé. Chaque définition C# contient uniquement le dossier du matériel, les fichiers externes et leurs opérations. La fréquence, les canaux et la durée ne sont plus saisis dans les catalogues. Le décodeur existant NAudio/Media Foundation ouvre le FLAC ; sa durée est comptée sur les échantillons décodés, car `Length` retourne zéro pour certains flux FLAC. Les durées et échantillons rééchantillonnés sont conservés en mémoire après leur première lecture. Aucun exécutable de conversion n’est utilisé pendant la lecture et aucun JSON ne pilote les sons.

## Définitions réduites par dossier et matériel

Les champs de provenance SourceFileName, SourcePath, SourceRepository, SourceRevision et SourceSha256 ont été supprimés du code de production. Les données inutilisées FileName, MissingOperations et le conteneur RecordedMechanicalSoundCatalog ont également été retirés.

Les définitions sont rangées par type de lecteur et marque, puis dans un fichier par modèle, par exemple `Constants/MechanicalAudio/Floppy/Epson/SD-700.cs`. Le dossier externe est défini une seule fois par matériel ; chaque entrée associe un nom relatif de FLAC à ses opérations. Les agrégateurs par type restent courts.

Validation isolée : 24 fichiers de modèles, au plus 189 lignes par fichier ; les 1 749 chemins et associations sont identiques avant/après. Les définitions et le lecteur compilent ensemble ; les 1 749 fichiers référencés existent et 173 opérations sont décodées depuis les FLAC. Les fichiers de contrôle temporaires ont été supprimés après validation.

La mécanique commune de MediaEngine suit les accès des représentations sectorielles, blocs et pistes optiques : moteur, insertion/éjection, position, sens et distance du déplacement. La sélection de face reste distincte du déplacement des têtes. Les transferts continus actualisent leur position finale sans inventer un nouveau seek. Les opérations de lecture, exploration, conversion et écriture disposent de scopes de fin d’activité. Le raccordement au mixeur App et aux consommateurs SDK/Acorn reste en cours et n’est pas déclaré terminé.

Validation isolée des sources actuelles : compilation de MediaEngine en mémoire avec ses générateurs, lecture directe des FLAC, catalogue de 24 profils et 1 749 fichiers, conservation des octets sectoriels et blocs, positions atteintes et fin d’activité après erreur/annulation. Le démarrage unique, la continuité de rotation entre deux fragments audio et la fin du son d’arrêt ont également été vérifiés. Les fichiers de validation temporaires ont été supprimés. Aucun build de GW GUI, arrêt ou remplacement de l’application.

## Raccordement disque Acorn et volume à chaud vérifiés

Les lecteurs Acorn possèdent maintenant chacun une mécanique commune relayée par le SDK. Les contrôleurs Intel 8271 et WD1770 y transmettent leurs mouvements physiques ; le Winchester transmet aussi les commandes de positionnement sans transfert. Les anciennes files et compteurs de bruitages propres au cœur Acorn ont été retirés. Les compteurs matériels de diagnostic restent dans les contrôleurs.

Le runtime signale les lectures et écritures de secteurs, y compris les données déjà modifiées en mémoire. La publication du conteneur ne déplace pas artificiellement la tête et ne coupe pas le moteur du lecteur emprunté. Une image libère seulement la mécanique qu’elle a créée ; le lecteur fourni par le cœur conserve sa durée de vie indépendante du support monté.

Les options sonores du lecteur — volume, activation et profil — sont appliquées sans redémarrage. Les paramètres matériels restent soumis à leur politique habituelle. Le volume nul rend du silence ; 20 % conserve la forme du signal et applique son atténuation sans remettre la rotation au début.

Contrôles réalisés sur les sources actuelles : compilation isolée de MediaEngine, du SDK et d’Acorn ; 77 cas de commandes/configuration/audio existants ; contrôle du volume 100/20/0 %, changements à chaud dans le cœur, adresse physique atteinte, écriture et relecture persistée d’une image DFS, conservation du lecteur emprunté après fermeture de l’image. Les validations et leurs DLL, ressources et images temporaires ont été supprimées après utilisation. Le build de livraison reste à faire après les compléments suivants.

Restent à terminer : sortie audio commune des usages App hors émulation, couverture réelle du magnétophone, exploitation correcte des séquences de mouvements rapides produisant les bourdonnements, nouveaux WAV d’écoute, vérification du build Debug Acorn.

## Sources cassette complémentaires repérées

- [BigSoundBank — Cassette Player: Rewind and End](https://bigsoundbank.com/cassette-player-rewind-and-end-s0567.html) : capture de Joseph Sardin, mono 48 kHz, 16 bits, deux retours rapides jusqu’à l’arrêt. Téléchargement annoncé sans compte ; référence commerciale non précisée, à conserver comme inconnue.
- [Freesound — TEAC W-860R](https://freesound.org/people/charonfaustinus/sounds/751057/) : capture stéréo 48 kHz, 24 bits, avec repères décrivant lecture, retour, avance rapide et manipulation du compartiment. Le téléchargement de l’original demande une connexion au site ; aucun original n’a encore été importé.
- [Freesound — Casio AS-51R](https://freesound.org/people/OneKellyOrdered/sounds/624636/) : capture mono 44,1 kHz, 16 bits, lecture puis arrêt ; original non importé.

Ces pistes sont des sources à préparer, pas des sons déjà intégrés. Une préécoute MP3 ne constitue pas un original PCM sans perte et ne doit pas être présentée comme telle.

## Sortie commune des outils App raccordée

Le contrôleur MediaDriveAudioController écoute les activités du service de mécanique utilisé par les lecteurs et écrivains d’images. Il mélange les fragments mono et utilise WasapiAudioOutput, déjà employé par les émulateurs avec NAudio 2.3.0 ; aucune nouvelle dépendance audio n’a été ajoutée. Une sortie est créée pendant une période d’activité puis libérée après vidage du tampon. La tâche possédée est annulée et attendue à la fermeture ; les mécaniques empruntées ne sont pas détruites par le contrôleur.

Les scopes d’exploration et de visualisation encadrent les accès aux représentations. Un arrêt demandé pendant un transfert attend la fin du dernier accès. Le raccordement sectoriel conserve les données immuables existantes au lieu de reconstruire une image dont la copie défensive consommait puis retirait le relais de mécanique.

Validation isolée des sources MediaEngine, SDK et du contrôleur App avec la classe WASAPI existante compilée et une sortie de contrôle injectée : lecteurs simultanés dans un même mélange, reprise après inactivité, format mono 44,1 kHz, conservation du lecteur emprunté, libération après erreur du périphérique, erreur d’un lecteur, annulation et exception d’un rappel d’annulation. Un véritable fichier DFS a été créé, lu et exploré ; les accès de secteurs actualisent la position du lecteur et produisent des fragments non silencieux. La copie immuable et l’absence d’accès anticipé pendant le raccordement ont été contrôlées.

Les fichiers de validation, DLL et image temporaire ont été supprimés. Ce contrôle n’ouvre pas de périphérique audio Windows et ne constitue pas encore une validation du build WPF complet ou de l’écoute native ; ces contrôles restent prévus avec le build de livraison après les sons cassette et séquences rapides.

### Magnétophone : originaux disponibles

Les sept WAV de la série BigSoundBank 0565–0571 sont présents sous `artifacts/mechanical-audio-research/recordings/Tape/Unidentified/Cassette-player/`. Les en-têtes confirment du PCM signé 16 bits, mono, 48 000 Hz. Cette série provient du même magnétophone sans référence commerciale identifiée : aucune marque ni modèle précis ne lui est attribué.

Sources originales : [lecture et arrêt](https://bigsoundbank.com/cassette-player-play-s0565.html), [retour et arrêt](https://bigsoundbank.com/cassette-player-rewind-and-stop-s0566.html), [retour et fin](https://bigsoundbank.com/cassette-player-rewind-and-end-s0567.html), [avance et arrêt](https://bigsoundbank.com/cassette-player-forward-and-stop-s0568.html), [avance et fin](https://bigsoundbank.com/cassette-player-forward-and-end-s0569.html), [lecture et pause](https://bigsoundbank.com/cassette-player-play-and-pause-s0570.html), [ouverture, insertion et fermeture](https://bigsoundbank.com/cassette-player-cassette-s0571.html).

Les originaux sont téléchargés via les formulaires WAV du site. Le découpage et le raccordement ne sont pas encore validés ; les fichiers complets ne doivent pas être bouclés comme un bruit moteur.

### Clips réels du magnétophone et contrôles isolés

Dix FLAC externes sont définis sous `Tape/Unidentified/Cassette-player/`. Les démarrages sont propres à chaque commande (lecture, avance rapide et retour) ; les boucles proviennent des parties continues de ces mêmes prises. La jonction de chaque boucle comporte un fondu croisé de 5 ms, sans normalisation ni égalisation. Lecture et enregistrement utilisent le même défilement mécanique ; aucune prise distincte du bouton REC ne figure dans cette source.

Découpes en secondes dans les originaux : lecture, démarrage 0–0,5 et rotation 1–10 ; arrêt 11,4–12,05. Avance : démarrage 0–0,45 et rotation 0,6–2,4. Retour : démarrage 0–0,5 et rotation 0,7–2,3. Pause : 2,85–3,45. Ouverture/éjection : 0–0,55. Insertion : manipulation 3,25–4,9 puis fermeture 6–6,85, en retirant uniquement l’attente entre ces gestes.

Les dix clips ont été encodés puis redécodés avec FLAC : PCM identique. Leurs crêtes vont de −46,12 à −0,09 dBFS ; les différences de niveau du magnétophone sont conservées. Un contrôle isolé compile les définitions et le lecteur audio effectifs, puis décode les huit opérations publiques à 44 100 Hz. Il vérifie les volumes 100/20/0 %, l’identité entre rendu continu et vingt fragments, les boucles persistantes et les fins silencieuses des actions ponctuelles. Les 1 759 fichiers référencés existent. Ce contrôle ne prouve pas encore le fonctionnement du module compilé dans App.

Acorn utilise désormais les durées réelles des commandes, et produit aussi les événements insertion et pause. Les modes accéléré et direct restent silencieux pour les bruitages. La compilation complète et l’écoute via la sortie Windows restent à contrôler avant livraison.

### Build et sortie Windows contrôlés

`scripts/local-building.cmd --building=debug --modules=acorn` a terminé avec succès. La livraison contient `build/Debug/GW GUI/gwgui.exe` et `Modules/acorn/gwgui.emulation.acorn.dll`. Le script inclut également son module Exemple systématique. Les dix FLAC Tape ont les mêmes empreintes dans la livraison et dans les sources externes.

Un exécutable temporaire a référencé les DLL livrées, décodé les FLAC via `EmulationSdk.MediaProcessing`, puis alimenté la vraie classe `WasapiAudioOutput` en mono à 44 100 Hz. La sortie était active, son tampon a été vidé et `Dispose` exécuté dans un `finally` a supprimé son tampon. Aucune fenêtre GW GUI n’a été lancée par ce contrôle. Les fichiers et artefacts des deux validations temporaires ont été supprimés. La fidélité subjective au magnétophone précis de l’utilisateur n’est pas démontrée : la référence commerciale du magnétophone enregistré reste inconnue.

Un avertissement XML CS1573 signale la documentation absente du paramètre `MechanicalVolumePercent` du contrat cassette ; aucune erreur de compilation. Les déplacements rapides et les WAV d’écoute définitifs restent à terminer avant de déclarer l’objectif complet.

### Déplacements rapides et résonances des têtes

Les sons des commandes Seek ne passent plus par la file des manipulations insertion/arrêt/éjection. Chaque déplacement réel démarre sa voix avec sa distance, son sens et sa piste ; les résonances encore audibles des commandes précédentes peuvent finir en parallèle. Le nombre de voix est borné à 32 ; la voix la plus ancienne est retirée si cette borne est atteinte, au lieu de retarder les commandes suivantes. La rotation reste une voix distincte. La protection commune atténue seulement un mélange qui dépasserait le PCM.

Cette séparation correspond au principe des [voix de seek simultanées de 86Box](https://github.com/86Box/86Box/blob/master/src/floppy/fdd_audio.c). Il ne s’agit pas de jouer une séquence musicale arbitraire du dossier Buzz lors de n’importe quel accès : le cliquetis répété provient des déplacements réellement déclarés. Les enregistrements de séquences complètes restent disponibles comme fichiers, sans leur attribuer une commande matérielle inexistante.

Le contrôle isolé, exécuté sur les sources actuelles et les FLAC externes, vérifie pour disquette et HDD : premier déplacement à la distance exacte ; nouvelle commande en sens inverse mélangée immédiatement à la résonance précédente ; mille déplacements bornés à 32 voix et totalement terminés après trois secondes de rendu, sans attente cumulée ; volume modifié à chaud ; sourdine ; désactivation et reset. Tous ces contrôles ont réussi. Les fichiers temporaires ont été supprimés.

### WAV d’écoute régénérés

Le contenu précédent de `artifacts/audio-preview` a été supprimé, puis les 1 759 FLAC définitifs ont été décodés en 1 759 WAV valides. L’arborescence conserve type de lecteur, marque, modèle, action, sens et distance. Les WAV ne sont pas normalisés ; ils permettent d’écouter les prises utilisées par le moteur. Le script temporaire de génération a été supprimé après réussite.


## Affectation automatique des lecteurs Acorn

Les dialogues AcornEmu ne proposent plus de choisir arbitrairement une banque sonore. Les disquettes utilisent l'enregistrement BBC Micro 5,25 pouces existant ; les opérations absentes suivent les replis communs, notamment Alps Electric. Winchester utilise le mécanisme Miniscribe 8425 enregistré comme repli. Aucun fichier de cette banque n'a été identifié comme un enregistrement des Rodime RO203E des Winchester Acorn 130 : ce manque reste explicite et les fichiers ne sont pas renommés pour leur attribuer ce matériel. Les niveaux et les enregistrements ne sont pas normalisés pendant cette correction.
