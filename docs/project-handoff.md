# GW GUI — état du projet pour reprendre dans une nouvelle discussion

Date de référence : 9 août 2026  
Dernier commit observé : `498300e` — `Support UCSD p-System disk images`

Ce document résume les décisions, les fonctions réalisées, les limites connues et la méthode de travail. Il permet de reprendre le projet sans relire toute la conversation. Il ne remplace pas les spécifications détaillées des autres fichiers du dossier `docs`.

## 1. Objectif du produit

GW GUI est une application Windows WPF/.NET destinée à rendre Greaseweazle utilisable sans construire manuellement les commandes `gw`. Elle doit rester complète, lisible, multilingue et adaptée aussi bien aux opérations physiques qu’à l’étude d’images de disquettes.

Les fonctions principales sont :

- Lecture d’une disquette vers SCP ou une image de format connu ;
- Écriture d’une image vers une disquette ;
- Conversion simple ou multiple ;
- Visualisation des pistes, flux, secteurs et anomalies ;
- Exploration du volume, des dossiers et des fichiers ;
- Outils, maintenance et diagnostics Greaseweazle ;
- gestion de plusieurs contrôleurs et lecteurs ;
- profils par opération ;
- console intégrée, progression, journaux, thèmes et langues ;
- version portable et installateur Windows.

## 2. Principes décidés avec l’utilisateur

- Les fonctions sont présentées par onglets, pas par une succession de fenêtres.
- Lecture, Écriture et Conversion possèdent des profils indépendants.
- Le profil système permanent s’appelle `Par défaut`/`Default` et remet les options facultatives à zéro.
- Les profils utilisateur sont enregistrés avec un nom ; enregistrer sous un autre nom crée une copie.
- Le matériel est configuré dans Options, pas dans chaque opération.
- Les caractéristiques descriptives du lecteur (taille, densité, RPM) servent au libellé et ne sont pas envoyées à `gw`.
- Avec un seul contrôleur et un seul lecteur, aucun sélecteur matériel ni argument inutile n’est affiché/émis.
- Plusieurs contrôleurs utilisent `--device` seulement lorsque le choix est nécessaire ; plusieurs lecteurs d’un même contrôleur utilisent `--drive` en interne.
- Un contrôleur absent reste mémorisé et marqué indisponible ; sa reconnexion réutilise sa configuration.
- Un nouveau contrôleur détecté n’est jamais configuré silencieusement.
- La console reste intégrée à la fenêtre et ressemble à un terminal.
- Les commandes longues sont asynchrones, annulables et ne doivent pas bloquer l’interface.
- Toute annulation de Lecture supprime le fichier partiel créé.
- Les journaux sont configurables par action et stockés dans le dossier Logs de l’application.
- Aucune chaîne visible ne doit être écrite en dur.
- Toute nouvelle clé doit être ajoutée aux ressources de langue concernées.
- Toute proposition non demandée doit être soumise avant implémentation ; l’utilisateur décide du périmètre.
- Lorsqu’une fonction est décidée, elle doit être réalisée complètement dans ce périmètre, pas comme une démo volontairement minimale.

## 3. Architecture actuelle

La solution contient principalement :

- `GWGUI.App` : application WPF, vues, contrôles, services UI et rendu ;
- `GWGUI.Domain` : requêtes, profils, catalogues, réglages et logique métier ;
- `GWGUI.Infrastructure` : exécution de `gw`, matériel Windows, persistance, Host Tools et journaux ;
- `GWGUI.MediaEngine` : conteneurs/images, flux, décodage, encodage, reconstruction sectorielle et systèmes de fichiers ;
- `GWGUI.Tests` : tests automatisés ;
- `scripts` : build, packaging et validations ;
- `installer` : projet Inno Setup ;
- `docs` : décisions, plans et guides ;
- `image_test` : corpus local privé ignoré par Git.

Les couches techniques importantes sont distinctes :

1. lecteur de conteneur ;
2. décodeur de flux ;
3. reconstruction sectorielle ;
4. lecteur de système de fichiers ;
5. encodeur de piste ;
6. écrivain de conteneur ;
7. planification/exécution de conversion ;
8. rendu et présentation UI.

Cette séparation existe partiellement. Le prochain grand chantier consiste à l’appliquer partout, notamment à la reconstruction SCP ISO FM/MFM.

## 4. Fonctions principales déjà réalisées

### 4.1 Exécution Greaseweazle

- Commandes `gw` lancées sans console externe visible.
- Arguments structurés, espaces et Unicode préservés.
- Une seule commande active dans toute l’application.
- Sorties standard/erreur affichées progressivement.
- Annulation avec confirmation et terminaison de secours.
- Construction centralisée des commandes Lecture, Écriture, Conversion, maintenance et diagnostics.
- Les 14 actions publiques de Greaseweazle possèdent un parcours UI.

### 4.2 Lecture

- SCP brut sans `--format` parasite.
- Images de formats connus via le catalogue.
- Nom sans extension, dossier persistant et extension séparée.
- Numérotation numérique ou alphabétique (`Z`, puis `AA`, `AB`, etc.).
- Incrément uniquement après réussite.
- Gestion des conflits : écraser, numéro suivant ou modifier le nom.
- Profils et options avancées persistantes.
- Résumé final et ouverture vers Visualisateur/Explorateur.
- Progression par deux lignes de blocs Face 0/Face 1.

### 4.3 Écriture

- Choix du fichier source.
- Détection du format et modification manuelle possible.
- Confirmation avant écriture et vérification sûre par défaut.
- Profils et options avancées propres à Écriture.

### 4.4 Conversion

- Conversion simple et multiconversion séquentielle.
- Sélection des formats et extensions compatibles.
- Extensions implicites ou explicites.
- Tags de noms, conflits et bilan final.
- Une erreur isolée n’arrête pas automatiquement toutes les autres sorties ; une annulation les arrête.
- Certaines conversions Apple II protégées RWTS18 peuvent conserver le format dans NIB/WOZ sans inventer d’extension.
- Les conversions utilisent encore principalement `gw`; le moteur interne n’est branché que lorsqu’un parcours complet existe.

### 4.5 Visualisateur

- Ouverture d’images disque, préparation en arrière-plan lorsqu’elle est nécessaire et compatible.
- Deux faces affichées simultanément.
- Zoom lié ou indépendant, panoramique et réinitialisation.
- Rendu progressif pendant l’analyse.
- Légende des structures et anomalies.
- Barres de pistes Face 0/Face 1.
- Inspecteur flottant avec Résumé, Révolutions, Structures et Secteurs.
- Partage de l’image chargée avec Explorateur.
- Annulation/remplacement du chargement quand une autre image est choisie.
- Silhouettes de médias physiques en cours d’amélioration.

### 4.6 Explorateur

- Ouverture d’une image existante ou lecture temporaire d’une disquette physique après confirmation.
- Sélecteurs Détection automatique, Machine, Format et Protection.
- Arborescence des dossiers, liste des fichiers et panneau de détails.
- Informations : système, protection, système de fichiers, volume, capacité, espace libre, éléments et avertissements.
- Types/icônes de fichiers adaptés à plusieurs familles.
- Une image non reconnue reste ouverte sans inventer un catalogue ni afficher nécessairement une erreur fatale.
- Une image protégée sans catalogue standard peut exposer sa structure physique réelle.
- Dernier dossier partagé et mémorisé avec Visualisateur.

### 4.7 Interface et réglages

- Onglets Lecture, Écriture, Conversion, Visualisation, Explorateur et Outils.
- Menu Options/Aide externalisé.
- Plusieurs blocs de Lecture/Écriture/Conversion externalisés en contrôles réutilisables.
- Console et barre d’état externalisées.
- Options modales avec sauvegarde immédiate des choix.
- Thèmes Système, Clair et Sombre ; le sombre reste à améliorer visuellement.
- Taille, position, maximisation, DPI et console persistants.
- Les réglages de placement doivent être appliqués avant l’affichage pour éviter un déplacement visible.

### 4.8 Matériel

- Détection Windows via SetupAPI et identification Greaseweazle.
- Identité stable par numéro de série/USB et mise à jour du COM si nécessaire.
- Contrôleurs configurés, nouveaux, absents et reconnectés distingués.
- Plusieurs contrôleurs/lecteurs simulés dans les tests.
- Configuration directe par ligne dans Options.
- Oublier le dernier lecteur oublie aussi le contrôleur après confirmation.
- L’essai réel avec plusieurs contrôleurs reste à faire lorsque le matériel sera disponible.

### 4.9 Host Tools

- Détection d’un `gw.exe` existant.
- Recherche de version et téléchargement volontaire de la dernière version.
- Installation gérée et versionnée sous les données locales.
- Retour au chemin précédent.
- Contrôle de capacités par la version installée.

### 4.10 Journaux et erreurs

- Journal global des exceptions avec pile complète.
- Journal distinct par action (`read.log`, `write.log`, `convert.log`, etc.).
- Activation, taille maximale et conservation des anciens journaux configurables.
- Dialogue utilisateur localisé ; détail technique écrit dans le journal.
- Bouton d’ouverture du dossier Logs.

### 4.11 Localisation

- 600 fichiers `.resx` observés, couvrant ressources neutres et cultures distribuées.
- Ressources séparées par domaines logiques, mais actuellement toutes placées à plat dans `Resources`.
- Changement de langue immédiat sans rouvrir les fenêtres.
- Détection initiale de la langue Windows avec repli anglais.
- Langues de l’application et de l’installateur indépendantes.
- Contrôles automatiques de parité et de placeholders déjà présents.
- Prochain travail structurel : ranger les domaines dans `Resources/Languages/<Domaine>/` sans casser le chargeur.

### 4.12 Build et distribution

- `scripts/build.ps1` produit le build rapide sous `artifacts/build/GW GUI`.
- `scripts/package.ps1` produit la publication, la version portable, le ZIP, l’installateur et les SHA-256.
- L’installateur Inno Setup et le portable ont déjà été construits et testés.
- `.github/workflows/release.yml` construit les paquets sur tag ou déclenchement manuel.
- Un workflow continu séparé pour pushes/pull requests reste à créer.

## 5. Formats et familles déjà travaillés

Le code et le corpus local couvrent déjà, à des niveaux différents :

- Amiga : ADF DD/HD, OFS/FFS, SCP MFM, AmigaDOS ;
- Atari ST : ST, MSA, SCP ISO MFM, TOS FAT12 ;
- Atari 8-bit : ATR, SCP FM/MFM, Atari DOS ;
- IBM PC : IMG/IMA, SCP FM/MFM, FAT12 et plusieurs géométries ;
- Commodore : D64, D71, D81, SCP 1541/1571/1581, CBM DOS et CP/M 3 ;
- Amstrad CPC/PCW : DSK/EDSK et systèmes CP/M concernés ;
- Apple II/III : D13, DSK/DO/PO, 2MG, NIB, WOZ, SCP, DOS/ProDOS/SOS et RWTS18 ;
- Apple Macintosh/Lisa : DiskCopy, images sectorielles, SCP, MFS/HFS/Lisa Office ;
- Acorn/BBC : DFS/ADFS/FileCore selon les formats déjà ajoutés ;
- Epson QX-10 : géométries mixtes TPM ;
- MSX ;
- UCSD p-System sur IBM MFM ;
- DEC RX02 ;
- plusieurs familles rares synthétiques : Membrain, AED 6200P, QD MO5, Centurion, E-mu, Arburg, Victor 9000, TYCOM, Heathkit, Micral N et NorthStar.

Cette liste signifie qu’un travail existe ; elle ne signifie pas que toutes les images de chaque famille sont définitivement validées. `validated_images` est la seule référence pour les images considérées terminées.

## 6. Validation locale déjà effectuée

Des validations de corpus ont déjà été réalisées pour notamment :

- Amiga DD/HD OFS/FFS ;
- Atari ST et Atari 8-bit ;
- IBM PC FAT12 ;
- Apple II/III, Macintosh et Lisa ;
- Commodore ;
- Acorn ;
- Epson QX-10 ;
- MSX ;
- UCSD p-System.

Les commits récents montrent les corrections successives de formats et de détection. Les validations réalisées avant chaque commit restent valables ; les nombres historiques de tests différents correspondent à des étapes successives, pas à des résultats contradictoires.

Le dernier état Git observé avant la rédaction était propre. Les répertoires `_generated` suivants étaient vides : Apple Macintosh, Atari 8-bit, Atari ST, BBC Micro et Acorn/Archimedes. Leur nettoyage appartient au protocole de validation, pas à cette rédaction documentaire.

## 7. Exemple confirmé du problème structurel général

`src/GWGUI.MediaEngine/SectorImages/AtariScpSectorImageReader.cs` est mal nommé et trop chargé. Il contient des branches pour Atari, Amstrad, IBM, BBC, Epson et UCSD, ainsi que la collecte de secteurs, la sélection FM/MFM, la détection IBM, les géométries Epson et l’assemblage final.

Ce fichier est seulement un exemple visible. L’ensemble du code doit être audité puis découpé selon les responsabilités réelles. Il ne faut pas limiter le chantier au moteur SCP, à cette classe, à `FluxDecoding.cs` ou aux deux grandes fenêtres WPF.

La correction attendue n’est pas un simple renommage ni le remplacement mécanique de `if` par `switch`. Il faut :

- un moteur commun de collecte ISO FM/MFM ;
- une stratégie par famille lorsque la géométrie ou l’ordre logique diffère ;
- un routage fondé sur les informations réellement connues, sans supprimer la reconnaissance des disquettes multiformats ;
- une détection automatique capable de conserver plusieurs résultats compatibles ;
- des descripteurs de données au lieu de répétitions de chaînes ;
- des tests de non-régression par famille.

L’audit complet et le découpage sont décrits dans `tasks/01-full-code-audit.md` et `tasks/02-full-refactoring.md`.

## 8. Autres dettes structurelles importantes

- `MainWindow.xaml.cs` approche 2 000 lignes et garde encore trop de coordination.
- `OptionsWindow.xaml.cs` approche 700 lignes.
- `DiskImageExplorer.cs` dépasse 500 lignes et route de nombreux conteneurs/systèmes.
- Les catalogues de formats et de classification vont continuer à grossir.
- Les ressources sont nombreuses et rangées à plat.
- Plusieurs identifiants, géométries et nombres techniques restent dispersés.
- Certains anciens documents décrivent encore un état antérieur du nombre de codecs ou de langues.
- Le thème sombre et plusieurs détails visuels ne sont pas définitivement validés.

## 9. Décisions spécifiques à ne pas perdre

### 9.1 Formats et auto-détection

- À chaque nouvelle image, si Détection automatique est cochée, Machine, Format et Protection doivent être recalculés.
- Si rien n’est reconnu, les sélecteurs doivent être vides ou sur `Aucun`, jamais conserver le choix de l’image précédente.
- Si la détection automatique est décochée, les choix manuels restent inchangés.
- Les listes de formats doivent provenir du même catalogue dans Lecture, Écriture, Conversion, Explorateur et Visualisateur.
- Un SCP est une capture brute ; son décodage nécessite un format ou une détection, mais son extension reste SCP.

### 9.2 Protections

- Une protection n’est ni un système de fichiers ni une extension.
- L’Explorateur doit afficher `Protection : —` ou le nom reconnu.
- Chaque protection ajoutée doit être couverte par les couches nécessaires : décodeur, encodeur si possible, Explorateur, Visualisateur et Conversion.
- Le déplombage doit créer une nouvelle sortie, jamais modifier la source.
- RWTS18 est un codec/procédé Apple II, pas une extension `.rwts18`.

### 9.3 Images protégées/non cataloguées

- Il faut chercher à décoder toute image, protégée ou non.
- Un émulateur peut charger un programme qui lit directement les secteurs sans catalogue standard.
- Dans ce cas, GW GUI doit montrer pistes/secteurs/état et permettre l’extraction brute, sans inventer des noms de fichiers.
- Un lecteur spécifique de catalogue propriétaire n’est ajouté que lorsque sa structure est connue et correctement implémentable.

### 9.4 Corpus privé

- Les images de `image_test` sont locales, ignorées par Git et ne doivent pas être publiées.
- Elles peuvent rester disponibles pour éviter de les télécharger à nouveau.
- Une image validée est déplacée, pas copiée.
- Le classement final est marque/modèle/type de disquette.
- Les noms de logiciels ou volumes ne déterminent jamais une règle codée en dur.
- Chaque tâche terminée doit toujours faire l’objet d’un commit, y compris une tâche documentaire, structurelle, de classement ou de validation.
- Un push est effectué lorsqu’une ou plusieurs tâches terminées constituent un bloc de travail complet et cohérent.
- Un bloc incomplet ne doit pas être poussé en étant présenté comme terminé.

## 10. État du corpus au moment de la reprise

`image_test` contient encore de nombreux dossiers à traiter, en commençant alphabétiquement par les collections Acorn/Archimedes, puis Acorn BBC, ACT Apricot, systèmes CP/M divers, Amiga, Amstrad, Apple, Atari, BBC, COHERENT, Epson et IBM PC.

`validated_images` contient actuellement des familles classées sous :

- Acorn ;
- Amstrad ;
- Apple ;
- Atari ;
- Commodore ;
- DEC ;
- Epson ;
- IBM ;
- MSX ;
- UCSD.

Le prochain passage ne doit pas reprendre ces fichiers validés. Il doit continuer avec la première image non validée dans l’ordre réel de `image_test`.

## 11. Méthode de validation à reprendre

Pour chaque image :

1. identifier conteneur, machine, format, géométrie, système de fichiers et protection attendus ;
2. tester la lecture du conteneur ;
3. tester décodage et reconstruction ;
4. tester l’encodeur/aller-retour lorsque cela s’applique ;
5. tester les sorties de conversion compatibles ;
6. tester le Visualisateur ;
7. tester l’Explorateur, y compris noms, tailles, dates, types, volume et avertissements ;
8. vérifier les cinq catalogues UI et les traductions nécessaires ;
9. si quelque chose échoue, corriger le format de manière générale et retester les familles déjà concernées ;
10. si tout est correct, déplacer l’image vers `validated_images/<marque>/<modèle>/<type>/` ;
11. supprimer les dossiers sources devenus vides ;
12. créer un commit lorsque la tâche de validation est terminée, même si elle ne concernait que le classement ou la documentation ;
13. pousser ce commit avec les autres tâches liées lorsque l’ensemble constitue un bloc complet ;
14. communiquer le résultat avant de passer à l’image suivante.

## 12. Ordre de reprise

1. Auditer tout le code, fichier par fichier.
2. Refactoriser et découper tout le code concerné, en séparant modèles et formats.
3. Centraliser les constantes et retirer les textes bruts.
4. Structurer enums, DTO, records, modèles de données et interfaces nécessaires.
5. Séparer les fonctions et services lorsque leur responsabilité le demande.
6. Réorganiser les traductions sous `Languages/<Domaine>`.
7. Contrôler l’interface, la robustesse, les performances et la maintenance.
8. Créer le workflow GitHub de build et auditer le workflow de release.
9. Reprendre seulement ensuite la validation exhaustive de `image_test`.
10. Terminer par les contrôles physiques Lecture, Écriture, Conversion, Visualisateur et Explorateur.

## 13. Documents à consulter en complément

- `rules.md` : règles permanentes du projet ;
- `tasks/README.md` : ordre obligatoire et tâches détaillées ;
- `decisions.md` : décisions produit ;
- `questions-and-answers.md` : réponses détaillées ;
- `Liste-imagesdisk.md` : inventaire des familles et formats ;
- `ui/` : spécifications visuelles par écran ;
- `versioning.md` : version produit, build et révision Git.

Les anciens plans et audits datés sont conservés dans `old/`. En cas de contradiction, la décision la plus récente de l’utilisateur prime. Une ambiguïté doit être présentée avant modification ; elle ne doit pas être résolue silencieusement par extrapolation.
