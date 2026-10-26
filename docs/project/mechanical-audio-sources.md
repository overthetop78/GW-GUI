# Sources de bruitages mécaniques réels

Demande du 26 octobre 2026 : remplacer la synthèse approximative, couvrir toutes les opérations et fournir des WAV audibles avec atténuation utilisateur. Les anciens exports seront remplacés après réalisation complète ; ils sont encore présents et ne constituent pas des sons validés.

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

La banque distribuée est `src/GWGUI.MediaEngine/Assets/MechanicalAudio/<type>/<marque>/<modèle>/<bruitage>.flac` : types `Floppy`, `HardDisk`, `Optical` pour les enregistrements actuellement disponibles. Les sons Tape restent à compléter. Insertion, éjection et moteur restent dans le dossier du modèle ; les séries Seek et Buzz ont leurs sous-dossiers. Les mécanismes non identifiés restent sous `Unidentified`, sans inventer leur marque. Les fichiers ne répètent pas la marque et le modèle.

Les 1 749 FLAC occupent 48 146 242 octets contre 103 320 114 octets pour les WAV sources. Le PCM décodé a été comparé bit à bit aux originaux lors de la conversion. Les niveaux des enregistrements récupérés sont conservés. La réserve utilisateur sur la normalisation concerne les sons à créer, afin de préserver les différences entre lecteurs silencieux et bruyants.

`catalog.json` est conservé. Les fichiers `provenance.json` et `preparation-report.json` ont été supprimés et leur génération retirée. Le manifeste de recherche existant conserve les sources originales. Le préparateur génère trois catalogues dans `Constants/MechanicalAudio/` : `RecordedMechanicalSoundFiles.Floppy.cs`, `.HardDisk.cs` et `.Optical.cs`. Chacun contient les dictionnaires des matériels, regroupés par marque/modèle et ordonnés par nom de bruitage, avec identité et chemin complet du fichier sur la même ligne. Les fichiers de constantes séparés par modèle ont été supprimés. `RecordedMechanicalSoundFiles.cs` ne contient plus de liste d’empreintes : il réunit les catalogues par type et construit l’index utilisé par le chargeur. Le catalogue JSON décrit les opérations et caractéristiques des échantillons.

La lecture utilise NAudio 2.3.0 déjà référencé et `StreamMediaFoundationReader`, avec le décodeur FLAC de Windows Media Foundation ; l’encodeur FLAC externe sert uniquement à préparer les assets. App distribue une seule banque externe, sans intégration des sons dans une DLL ni duplication dans chaque module.

Après déplacement, les 1 749 fichiers ont été vérifiés par SHA-256 et confrontés aux chemins du catalogue. La validation isolée des sources C# a réussi pour 24 profils et 153 opérations, avec lecture FLAC via les constantes, continuité des fragments, volume et protection du mélange. Le script temporaire et son résultat JSON ont été supprimés. Aucun build de GW GUI ni remplacement de l’application en cours d’essai n’a été effectué. Ce contrôle ne remplace pas la validation du build complet et ne signifie pas que tous les modèles et opérations manquants sont terminés.

Contrôle isolé après ce découpage : les sources C# des quatre fichiers de catalogues ont été compilées en mémoire ; l’agrégation contient les 1 744 identités distinctes attendues pour les 1 749 fichiers, et chaque chemin référencé existe. Les quelques identités communes correspondent à des contenus source identiques. Aucun fichier temporaire ni build de l’application.


## Reconstruction complète des fichiers et identifiants — 26 octobre 2026

À la demande explicite de l’utilisateur, l’ancienne banque générée et les quatre catalogues C# ont été supprimés avant reconstruction depuis `artifacts/mechanical-audio-research/recordings`. Les paragraphes précédents relatifs aux anciens identifiants SHA-256, aux collisions « alternative » et à leur ancien index décrivent une réalisation remplacée.

Le préparateur est réécrit. `scripts/tools/mechanical-audio/source-sets.json` attribue explicitement les 26 ensembles sources à leurs matériels ; les dossiers anonymes de plusieurs sources ne sont plus fusionnés automatiquement sous une même référence. `sound-names.json` définit les noms et opérations fixes. Toute collision ou source non définie arrête la préparation : aucun suffixe numéroté n’est inventé pour la masquer.

Les noms décrivent les événements : Disk-insert, Disk-eject, Drive-open/close, Tray-open/close, Motor-start/running/stop. Les déplacements `1up` et `1down` deviennent respectivement `Seek/Toward-center/1-track.flac` et `Seek/Toward-edge/1-track.flac`. La distance et la direction restent aussi dans les métadonnées. Up/down désignent le mouvement entre pistes, pas les faces. Les fichiers source indexés seek_XX/buzz_XX restent des motifs enregistrés distincts, sans transformer leur indice en distance inventée ; step_XX désigne le pas associé à la piste précisée par la source.

Chaque identité est maintenant le chemin descriptif sans extension, par exemple `Floppy/Alps Electric/DF354H148F/Seek/Toward-center/1-track`. Les dictionnaires par matériel et par type relient directement cette identité au fichier FLAC ; le catalogue principal les agrège. Les empreintes servent uniquement à vérifier l’intégrité des originaux. Le JSON conserve pour chaque son son nom de fichier source, son chemin source, son dépôt et sa révision pour permettre une identification directe. Aucun fichier provenance ou preparation-report n’est recréé.

Résultat : 1 749 FLAC recréés, 1 749 identités lisibles distinctes, 24 profils, PCM décodé identique à chaque original sans normalisation. La validation isolée des sources réelles a vérifié 173 opérations décodées à 44,1 kHz, la continuité entre fragments et le silence au volume zéro. Chaque déplacement up/down a été confronté au nom original, à sa distance et à sa direction. Le script temporaire a été supprimé. Aucun build de GW GUI.

Cette reconstruction corrige l’organisation et l’identification des enregistrements disponibles. La couverture des lecteurs et opérations encore absents des sources reste à compléter ; elle n’est pas déclarée terminée par ce contrôle.


## Bases des lecteurs sans son spécifique

Le service commun utilise les bases suivantes, explicitement choisies plutôt que déduites de l’ordre alphabétique :

- Floppy : Alps Electric DF354H148F (`Floppy35`).
- HardDisk : Seagate ST3120A (`HardDisk35`), choisi parmi les enregistrements disponibles pour le moteur, le démarrage, l’arrêt et le déplacement des têtes.
- Optical : le seul ensemble disponible de lecteur CD-ROM à tiroir (`OpticalTray`), dont la référence commerciale reste inconnue.

Le profil de base est placé en tête des choix et utilisé lorsque le profil demandé est absent ou non renseigné. Pour un profil enregistré spécifique, chaque opération disponible reste issue de ce profil ; une opération manquante utilise celle de la base de sa catégorie. La durée de l’événement utilise le même repli que le rendu. Si la base ne possède pas non plus cette opération, aucun son supplémentaire n’est inventé. Les réglages d’activation et de volume restent appliqués par leurs consommateurs habituels.

Validation isolée sur les sources réelles : les trois bases, le rendu sans profil et avec profil absent, les durées correspondantes, le repli de l’arrêt Epson SD-800 vers Alps et la conservation de son déplacement spécifique ont réussi. Le script temporaire a été supprimé ; aucun build de GW GUI.
