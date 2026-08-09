# Feuille de route de refactoring, de qualité et de validation

Date de référence : 9 août 2026  
État du dépôt examiné : commit `498300e` (`Support UCSD p-System disk images`)

Ce document est la liste de travail de référence pour la prochaine phase de GW GUI. Il décrit des travaux à réaliser ; il n’autorise pas à modifier plusieurs responsabilités en même temps ni à changer un comportement validé sans accord.

## Règles obligatoires pour tous les travaux

- [ ] Ne pas modifier le comportement pendant un déplacement ou un découpage structurel.
- [ ] Séparer les commits de refactoring pur des commits fonctionnels.
- [ ] Avant chaque refactoring, relever les résultats de compilation et des tests ciblés concernés.
- [ ] Après chaque étape, compiler et exécuter d’abord les tests ciblés ; n’exécuter la suite complète qu’aux jalons qui le justifient.
- [ ] Ne pas créer une version minimale volontairement incomplète d’une fonction décidée.
- [ ] Ne pas extrapoler une décision produit : toute nouvelle idée est documentée et soumise à l’utilisateur avant réalisation.
- [ ] Ne jamais cibler le nom d’une image de test particulière dans le code. Une correction doit s’appliquer au format, au conteneur, au système de fichiers ou à la protection concernés.
- [ ] Ne jamais recopier une image validée : la déplacer dans `image_test/validated_images`, puis supprimer les répertoires sources devenus vides.
- [ ] Ne jamais retraiter les images déjà placées dans `validated_images`.
- [ ] Toujours créer un commit lorsqu’une tâche est terminée, y compris pour une tâche documentaire, structurelle ou de classement.
- [ ] Pousser les commits lorsqu’une ou plusieurs tâches terminées constituent ensemble un bloc de travail complet et cohérent.
- [ ] Ne pas pousser un bloc partiellement terminé en le présentant comme achevé.
- [ ] Pendant la validation du corpus, terminer chaque type de disquette validé par un commit cohérent ; plusieurs commits liés peuvent être poussés ensemble à la fin du bloc complet.
- [ ] Présenter le résultat d’une image avant de passer à la suivante ; éviter les longues commandes opaques qui traitent tout le corpus sans résultat intermédiaire.
- [ ] Toute chaîne visible doit provenir des ressources de langue. Les identifiants techniques non traduisibles doivent provenir d’un catalogue ou de constantes dédiées.
- [ ] Toute nouvelle fonction doit gérer annulation, erreur, journalisation et nettoyage de ses fichiers temporaires.

## 1. Établir une architecture de référence avant le déplacement du code

### 1.1 Cartographier les responsabilités actuelles

- [ ] Produire la carte des appels entre conteneurs, décodeurs de flux, reconstruction sectorielle, systèmes de fichiers, classification, visualisation et conversion.
- [ ] Identifier le propriétaire de chaque décision : format demandé, auto-détection, géométrie, codec, protection, conteneur de sortie et système de fichiers.
- [ ] Relever les endroits où la même image est analysée plusieurs fois inutilement.
- [ ] Relever les dépendances circulaires ou les couches UI qui connaissent des détails de décodage.
- [ ] Relever les classes nommées pour une machine mais utilisées par plusieurs familles.
- [ ] Relever les conditions répétées sur des chaînes comme `formatId.StartsWith(...)`.
- [ ] Relever les nombres magiques, suffixes, identifiants, noms de codecs, extensions, tailles et géométries écrits directement dans les algorithmes.
- [ ] Relever les textes visibles et messages techniques encore écrits en dur.
- [ ] Relever les duplications exactes ou paramétrables entre lecteurs, encodeurs, décodeurs et catalogues.

### 1.2 Définir les frontières stables

- [ ] Définir des contrats distincts pour : lecture de conteneur, décodage de piste, reconstruction sectorielle, lecture de système de fichiers, classification, encodage de piste, écriture de conteneur et conversion.
- [ ] Définir une seule représentation intermédiaire sectorielle commune, sans perdre les métadonnées propres au format.
- [ ] Préserver dans cette représentation : adresse physique, adresse logique, face, cylindre, secteur, taille, révolution, intégrité, données absentes, doublons et avertissements.
- [ ] Définir une représentation des capacités : lecture, écriture, conversion, visualisation, exploration, détection et protection.
- [ ] Définir clairement la différence entre machine, famille, format logique, géométrie, encodage, conteneur, système de fichiers et protection.
- [ ] Décider quelles données sont fermées et conviennent à un `enum`, et lesquelles doivent rester extensibles par identifiant/catalogue.
- [ ] Documenter les dépendances autorisées entre projets afin d’empêcher l’UI de devenir le routeur métier.

### 1.3 Choisir le mécanisme de routage

- [ ] Remplacer les longues chaînes de `if` par un registre de stratégies et des descripteurs de formats lorsque le comportement est extensible.
- [ ] Utiliser un `switch` C# uniquement pour un petit ensemble fermé et stable ; ne pas remplacer un gros `if` par un gros `switch` qui garderait le même problème.
- [ ] Lorsqu’un format est explicitement choisi, router directement vers son lecteur sans essayer les autres familles.
- [ ] Lorsque la détection automatique est active, n’exécuter que les détecteurs compatibles avec le conteneur et les caractéristiques déjà connues.
- [ ] Classer les résultats de détection selon une règle commune : confiance, complétude, cohérence géométrique, intégrité et signatures.
- [ ] Conserver les résultats d’analyse réutilisables pour éviter un second décodage entre Explorateur et Visualisateur.
- [ ] Rendre l’ordre et les priorités de détection explicites et testables.

## 2. Corriger le mauvais découpage de la reconstruction SCP ISO FM/MFM

### 2.1 Problème confirmé

`AtariScpSectorImageReader.cs` n’est plus un lecteur Atari. Il traite actuellement :

- Atari 8-bit ;
- Atari ST ;
- Amstrad ;
- IBM PC ;
- Acorn/BBC Micro DFS ;
- Epson QX-10 ;
- UCSD p-System sur IBM MFM ;
- la détection FM/MFM générique ;
- la détection et les géométries particulières Epson QX-10.

Le renommer en `GenericScpSectorImageReader` ne suffirait pas : cela conserverait le monolithe et les branchements conditionnels.

### 2.2 Découpage cible

- [ ] Extraire un orchestrateur ISO FM/MFM qui ne connaît aucune machine et qui parcourt pistes/révolutions.
- [ ] Extraire la collecte commune des candidats physiques et des candidats CHRN cohérents.
- [ ] Extraire la sélection du meilleur secteur entre révolutions dans un service commun.
- [ ] Extraire le choix FM/MFM dans une stratégie testable.
- [ ] Extraire la construction finale des `SectorBlock` dans un composant commun paramétré par une définition de géométrie.
- [ ] Créer une définition de disposition de piste : premier secteur, nombre de secteurs, taille, ordre logique et exceptions par piste/face.
- [ ] Créer une stratégie séparée par famille ou format lorsque les règles diffèrent réellement :
  - [ ] Atari 8-bit ;
  - [ ] Atari ST ;
  - [ ] IBM PC ;
  - [ ] Amstrad CPC/PCW ;
  - [ ] Acorn/BBC DFS ;
  - [ ] Epson QX-10 ;
  - [ ] UCSD p-System IBM MFM.
- [ ] Déplacer les géométries et la détection Epson dans des fichiers Epson dédiés.
- [ ] Déplacer la détection IBM PC vers la famille IBM sans appel croisé caché depuis un lecteur Atari.
- [ ] Conserver les cas de secteurs variables, numérotation zéro/un, pistes mixtes et pistes physiquement incohérentes.
- [ ] Ajouter des tests de non-régression par famille avant de supprimer l’ancienne classe.
- [ ] Supprimer `AtariScpSectorImageReader` seulement lorsque tous ses appels ont été routés vers les nouvelles stratégies.

### 2.3 Arborescence indicative

```text
GWGUI.Scp/SectorImages/
├── Common/
│   ├── IsoFluxSectorCollector.cs
│   ├── SectorCandidateSelector.cs
│   ├── SectorImageAssembler.cs
│   └── TrackLayout.cs
├── Atari/
│   ├── Atari8BitScpSectorImageReader.cs
│   └── AtariStScpSectorImageReader.cs
├── IbmPc/IbmPcScpSectorImageReader.cs
├── Amstrad/AmstradScpSectorImageReader.cs
├── Acorn/BbcDfsScpSectorImageReader.cs
├── Epson/
│   ├── EpsonQx10ScpSectorImageReader.cs
│   ├── EpsonQx10FormatDetector.cs
│   └── EpsonQx10Geometries.cs
└── Ucsd/UcsdMfmScpSectorImageReader.cs
```

Cette arborescence reste indicative : le nom final dépendra des contrats constatés pendant l’extraction. La séparation par machine ne doit pas dupliquer l’algorithme ISO FM/MFM commun.

## 3. Catalogues, constantes, enums et données

### 3.1 Supprimer les littéraux dispersés

- [ ] Recenser toutes les chaînes servant d’identifiants de machine, format, codec, protection, système de fichiers, commande et option `gw`.
- [ ] Recenser toutes les extensions et tous les suffixes de conteneurs.
- [ ] Recenser toutes les tailles, géométries, numéros de secteurs, valeurs de CRC, marques de synchronisation et constantes de format.
- [ ] Déplacer les constantes techniques dans des fichiers dédiés au bon domaine, pas dans un unique fichier global illisible.
- [ ] Créer notamment, si l’inventaire le confirme :
  - [ ] `MachineIds` ;
  - [ ] `DiskFormatIds` ;
  - [ ] `CodecIds` ;
  - [ ] `ProtectionIds` ;
  - [ ] `FileSystemIds` ;
  - [ ] `ImageExtensions` ;
  - [ ] `GwVerbs` et `GwOptionNames` ;
  - [ ] constantes de rendu et de disposition UI ;
  - [ ] constantes de format au plus près de chaque codec.
- [ ] Interdire les comparaisons avec des chaînes recopiées dans plusieurs classes.
- [ ] Ajouter un test détectant les identifiants du catalogue référencés mais non déclarés.

### 3.2 Modèles de données

- [ ] Créer des records/DTO pour les descripteurs de machine, format, géométrie, codec, protection, système de fichiers et conteneur.
- [ ] Créer des interfaces de stratégie seulement lorsqu’il existe plusieurs implémentations interchangeables.
- [ ] Utiliser des enums pour les états finis : intégrité, résultat de détection, type de média, densité, face, capacité d’une fonction et état d’une opération.
- [ ] Ne pas utiliser un enum pour les formats extensibles provenant des `diskdefs` de Greaseweazle.
- [ ] Séparer les modèles publics des modèles internes propres à un algorithme.
- [ ] Ajouter validation et invariants dans les constructeurs/factories plutôt que dans chaque consommateur.

### 3.3 Catalogue central et données intégrées

- [ ] Faire du catalogue central la source unique des listes Lecture, Écriture, Conversion, Explorateur et Visualisateur.
- [ ] Pour chaque entrée, déclarer machine, format, géométrie, extensions, codecs, systèmes de fichiers, protections et capacités.
- [ ] Permettre plusieurs conteneurs pour un même format sans dupliquer sa description.
- [ ] Permettre plusieurs formats pour une machine sans conditions UI spécifiques.
- [ ] Conserver les `diskdefs` officiels/personnalisés comme extension du catalogue, sans dépendre de `image_test/_diskdefs` à l’exécution.
- [ ] Si une définition spéciale est nécessaire au produit, l’intégrer dans les données embarquées du code et documenter sa source.
- [ ] Vérifier qu’une entrée non prise en charge n’est pas proposée comme exécutable.
- [ ] Vérifier que l’auto-détection retourne `Aucun`/vide lorsqu’aucun résultat fiable n’existe.

## 4. Décodeurs, encodeurs, conteneurs et protections

### 4.1 Décodeurs

- [ ] Vérifier qu’un décodeur correspond à une seule famille d’encodage clairement nommée.
- [ ] Conserver un fichier par décodeur lorsque l’algorithme est spécifique.
- [ ] Extraire uniquement les primitives vraiment identiques : lecture de bits, CRC, GCR, MFM/FM, recherche circulaire et sélection des révolutions.
- [ ] Paramétrer explicitement polynôme, initialisation, ordre des bits et finalisation des contrôles d’intégrité.
- [ ] Vérifier la parité entre registre, catalogue, Visualisateur, Explorateur et Conversion.
- [ ] Ajouter les vecteurs de tests associés à chaque format/protection réellement implémenté.

### 4.2 Encodeurs

- [ ] Vérifier la correspondance réelle entre chaque encodeur et son décodeur.
- [ ] Brancher les encodeurs au moteur de conversion interne lorsqu’une sortie valide existe.
- [ ] Ajouter les écrivains de conteneurs nécessaires ; un encodeur de piste seul ne constitue pas une conversion complète.
- [ ] Préserver index, timing, révolution, géométrie, tags et métadonnées nécessaires au conteneur cible.
- [ ] Ajouter un écrivain SCP complet avant de proposer les conversions vers SCP.
- [ ] Refuser une conversion qui perd des données sans avertissement explicite et décision de l’utilisateur.

### 4.3 Protections et déplombage

- [ ] Modéliser une protection séparément du conteneur et du système de fichiers.
- [ ] Pour chaque protection reconnue, fournir : détecteur, décodeur, encodeur si possible, rendu, informations Explorateur et compatibilités de Conversion.
- [ ] Afficher le nom de la protection dans Explorateur et Visualisateur ; afficher un tiret lorsqu’aucune protection n’est reconnue.
- [ ] Ne jamais inventer une extension à partir du nom d’une protection ou d’un codec.
- [ ] Proposer le déplombage uniquement lorsqu’une transformation complète et vérifiée existe.
- [ ] Ne jamais modifier la source pendant un déplombage.
- [ ] Distinguer conservation de la protection, normalisation et suppression de la protection.
- [ ] Pour RWTS18/Apple II, conserver la prise en charge déjà réalisée et reporter les évolutions fonctionnelles non demandées pour maintenant.

## 5. Lecteurs de systèmes de fichiers et Explorateur

- [ ] Conserver un lecteur par système de fichiers, avec primitives communes séparées.
- [ ] Distinguer absence de catalogue standard, système non reconnu et corruption réelle.
- [ ] Ne jamais inventer de dossiers ou noms de fichiers pour une image protégée non cataloguée.
- [ ] Lorsque le catalogue n’est pas connu, exposer la structure physique décodée et les données brutes extractibles.
- [ ] Vérifier noms de volume, dossiers, fichiers, types, tailles, dates, attributs, espace libre et avertissements.
- [ ] Utiliser une classification de fichiers propre à la machine/système : une extension `.bat`, `.prg` ou autre ne doit pas avoir le même sens partout.
- [ ] Prévoir des profils de types de fichiers par système et des signatures de contenu lorsque l’extension est insuffisante.
- [ ] Conserver la possibilité de choisir manuellement machine et format lorsque l’auto-détection échoue.
- [ ] Mutualiser le résultat chargé avec le Visualisateur afin d’éviter une nouvelle conversion ou analyse.
- [ ] Permettre l’annulation immédiate lorsqu’une autre image est choisie.
- [ ] Nettoyer tous les fichiers temporaires, y compris après erreur ou annulation.

## 6. Conversion

- [ ] Séparer le planificateur, la validation des compatibilités, l’exécution via `gw` et la conversion interne.
- [ ] Utiliser le catalogue central pour filtrer les sorties par machine, format, conteneur et protection.
- [ ] Conserver la multiconversion et réinitialiser la progression avant chaque sortie.
- [ ] Dans la liste des sélections, afficher une ligne par couple format/extension choisi.
- [ ] Présenter séparément machines et formats compatibles, sans liste globale illisible.
- [ ] Conserver les formats sélectionnés en tête selon la règle validée.
- [ ] Brancher les conversions internes seulement lorsque lecteur, reconstruction, encodeur et écrivain de conteneur sont complets.
- [ ] Préserver l’utilisation de `gw` comme solution actuelle et comme repli pour les formats annoncés compatibles.
- [ ] Ajouter les conversions de déprotection validées au même planificateur, sans fausse extension.
- [ ] Vérifier les conflits de noms, tags, sorties multiples, annulation, fichiers partiels et bilan final.

## 7. Refactoring de l’application WPF

### 7.1 Fenêtre principale

- [ ] Réduire `MainWindow.xaml.cs`, actuellement proche de 2 000 lignes, en déplaçant les responsabilités dans les contrôles/ViewModels/services propriétaires.
- [ ] Conserver exactement la navigation par onglets.
- [ ] Faire de `MainWindow` le point de composition et le conteneur des fonctions globales seulement.
- [ ] Extraire les comportements encore partagés de manière implicite entre Lecture, Écriture, Conversion, Visualisation, Explorateur, Outils, console et barre d’état.
- [ ] Ne pas créer de fichiers `partial` uniquement pour masquer la taille du monolithe.
- [ ] Réutiliser les blocs réellement identiques sous forme de composants configurables, sans partager leur état métier entre onglets.
- [ ] Vérifier que chaque instance du bloc Profil reste indépendante par opération.

### 7.2 Options

- [ ] Découper `OptionsWindow.xaml.cs` par page fonctionnelle lorsque cela réduit réellement les dépendances.
- [ ] Isoler Général, Contrôleurs et lecteurs, Profils, Journaux et Host Tools.
- [ ] Conserver une seule fenêtre modale avec un bouton Fermer et la croix.
- [ ] Préserver la sauvegarde immédiate des choix et la sauvegarde finale de sécurité à la fermeture.

### 7.3 Composants globaux

- [ ] Conserver le menu principal dans son propre composant.
- [ ] Conserver la console/terminal dans son propre composant.
- [ ] Conserver la barre d’état et les deux lignes de blocs par face dans leurs propres composants.
- [ ] Centraliser les styles, icônes, espacements, rayons et dimensions réutilisés.
- [ ] Éviter toute référence directe depuis les composants visuels vers les détails de Greaseweazle ou des codecs.

## 8. Revue de l’affichage et de l’expérience utilisateur

Ces travaux précèdent la validation finale du corpus, car une fonction techniquement correcte doit aussi être visible et utilisable.

### 8.1 Cohérence générale

- [ ] Vérifier la fenêtre à sa taille normale, réduite, maximisée et à plusieurs facteurs DPI.
- [ ] Conserver les barres de défilement automatiques uniquement lorsque le contenu dépasse réellement.
- [ ] Vérifier qu’aucun contrôle ne sort de son cadre et qu’aucun texte n’est tronqué.
- [ ] Vérifier le centrage vertical des textes, champs et boutons.
- [ ] Uniformiser onglets, cadres, boutons, icônes, survols, focus et états désactivés.
- [ ] Reprendre ultérieurement le thème sombre, déjà identifié comme visuellement incomplet.
- [ ] Préserver la barre de titre native Windows ; différencier légèrement le fond intérieur sans imiter la barre de titre.
- [ ] Vérifier la restauration de taille/position avant affichage, sans déplacement visible au démarrage.

### 8.2 Lecture et Écriture

- [ ] Vérifier l’ordre validé : paramètres avancés en haut, image et profil côte à côte, dossier et nom côte à côte.
- [ ] Conserver les boutons de profil sous forme d’icônes et dans leur cadre.
- [ ] Vérifier le bouton Exécuter/Arrêter, la confirmation et la suppression des fichiers partiels après annulation.
- [ ] Vérifier le résumé final, le renommage depuis l’emplacement décidé et les accès Visualisateur/Explorateur.
- [ ] Vérifier que le sélecteur de lecteur est invisible lorsqu’aucun choix n’est nécessaire.

### 8.3 Conversion

- [ ] Vérifier les trois colonnes : sélections, formats courants, formats rares.
- [ ] Vérifier le défilement interne à la molette sans barre permanente dans les listes.
- [ ] Vérifier le défilement général seulement lorsque la fenêtre est réellement trop petite.
- [ ] Vérifier la sélection machine/format/protection, les tags et le nom des sorties.
- [ ] Vérifier la visualisation immédiate d’une source compatible.

### 8.4 Visualisateur

- [ ] Vérifier les sélecteurs Détection automatique, Machine, Format et Protection.
- [ ] Vérifier la silhouette de média correcte : 3 pouces, 3,5 pouces DD/HD, 5,25 pouces et 8 pouces, recto/verso.
- [ ] Améliorer les silhouettes sans utiliser les photos de référence comme assets non autorisés.
- [ ] Conserver la taille du disque presque égale à celle du support et synchroniser son zoom.
- [ ] Vérifier légende, couleurs d’anomalies, barres Face 0/Face 1 et inspecteur flottant.
- [ ] Retirer les survols qui déclenchent un calcul ou une sélection non demandée.
- [ ] Vérifier déplacement, ancrage et détachement de l’inspecteur sans dépassement.

### 8.5 Explorateur

- [ ] Vérifier la disposition dossiers, fichiers et panneau de détails.
- [ ] Afficher les informations du disque lorsqu’aucun fichier de la liste principale n’est sélectionné.
- [ ] Afficher les informations du dossier/fichier sélectionné uniquement depuis la liste principale.
- [ ] Vérifier icônes et types de fichiers selon la machine.
- [ ] Vérifier le bouton d’avertissements et son dialogue détaillé.
- [ ] Vérifier le comportement « format non reconnu » sans boîte d’erreur injustifiée.

### 8.6 Console et barre d’état

- [ ] Conserver le terminal dans un cadre intérieur, sans titre « Commande ».
- [ ] Conserver une icône de copie intégrée et visible au survol.
- [ ] Afficher fin, code, durée et bilan de chaque opération dans le terminal et le journal concerné.
- [ ] Garder des zones fixes dans la barre d’état pour matériel, profil et état.
- [ ] Placer progression et chronomètre sans déplacer les autres zones.
- [ ] Réinitialiser les blocs Face 0/Face 1 à gris avant chaque nouvelle commande.
- [ ] Utiliser les couleurs de lecture/décodage réellement disponibles, sans inventer une qualité que `gw` ne fournit pas.

## 9. Localisation et absence de texte brut

### 9.1 Réorganisation physique

Les 600 fichiers `.resx` sont actuellement à plat dans `src/GWGUI.App/Resources`. Ils sont déjà séparés par domaine logique, mais pas par dossier.

- [ ] Créer un dossier `Languages` sous les ressources.
- [ ] Créer un sous-dossier par domaine spécialisé, par exemple :

```text
Resources/Languages/
├── Common/
├── Errors/
├── Shell/
├── Menus/
├── Read/
├── Write/
├── Conversion/
├── Visualizer/
├── Explorer/
├── Formats/
├── Hardware/
├── HostTools/
├── Options/
├── Profiles/
├── Logs/
├── Tools/
└── About/
```

- [ ] Conserver dans `Common` uniquement les textes véritablement partagés et basiques.
- [ ] Conserver les erreurs spécialisées dans le domaine qui les produit ou dans un sous-catalogue d’erreurs clairement propriétaire ; éviter les doublons exacts.
- [ ] Conserver les noms techniques identiques pour toutes les langues dans la ressource neutre commune lorsqu’ils ne doivent réellement pas être traduits.
- [ ] Mettre à jour les noms de ressources embarquées et le chargeur composite sans casser les clés existantes.
- [ ] Conserver le repli : culture exacte, culture parente si applicable, puis anglais/neutre.

### 9.2 Audit des chaînes

- [ ] Rechercher les textes visibles écrits dans XAML et C#.
- [ ] Distinguer texte utilisateur, message de journal technique, identifiant stable et donnée de format.
- [ ] Déplacer les textes utilisateur vers le catalogue de langue approprié.
- [ ] Déplacer les identifiants stables vers les constantes/catalogues techniques.
- [ ] Éliminer les traductions corrompues par un mauvais encodage.
- [ ] Vérifier placeholders, retours à la ligne, apostrophes, pluriels et sens droite-gauche.
- [ ] Vérifier la parité de toutes les clés dans toutes les cultures distribuées.
- [ ] Vérifier que Read, Write, Convert, Explorateur et Visualisateur utilisent les mêmes noms de formats issus du catalogue central.

### 9.3 Structure et outils de contrôle

- [ ] Adapter les scripts de contrôle à la nouvelle arborescence.
- [ ] Détecter automatiquement clés manquantes, clés en double, valeurs vides et placeholders incompatibles.
- [ ] Détecter les séquences typiques de mojibake (`Ã`, `Â`, `â€`, etc.) dans les fichiers de ressources.
- [ ] Maintenir un glossaire technique commun.
- [ ] Vérifier les textes de l’installateur séparément de ceux de l’application.

## 10. Robustesse, performance et maintenance

### 10.1 Performance

- [ ] Mesurer séparément lecture du conteneur, décodage, reconstruction, système de fichiers et rendu.
- [ ] Mettre en cache les résultats immuables partagés entre Visualisateur et Explorateur.
- [ ] Éviter de rescanner tous les codecs lorsqu’un format explicite est connu.
- [ ] Prévoir une détection rapide puis une analyse approfondie seulement si nécessaire.
- [ ] Rendre tout traitement long annulable et remplacer immédiatement une demande devenue obsolète.
- [ ] Afficher progressivement le visualiseur pendant le chargement sans bloquer l’interface.
- [ ] Vérifier la fermeture pendant une analyse lourde.

### 10.2 Erreurs et journaux

- [ ] Vérifier que toutes les exceptions utilisateur sont localisées et que le détail technique va dans les journaux.
- [ ] Éviter les boîtes d’erreur pour un simple format non reconnu.
- [ ] Conserver un journal par action selon les préférences existantes.
- [ ] Vérifier rotation, taille zéro illimitée, archivage daté et bouton d’ouverture du dossier Logs.
- [ ] Ajouter le contexte machine/format/codec/protection aux erreurs de décodage.
- [ ] Vérifier que les erreurs sur une image n’empêchent pas d’ouvrir rapidement une autre image.

### 10.3 Persistance et migrations

- [ ] Auditer les réglages et séparer les modèles lorsqu’une prochaine migration de schéma le justifie.
- [ ] Préserver les anciens réglages pendant le refactoring.
- [ ] Conserver le dernier dossier ouvert commun à Explorateur et Visualisateur.
- [ ] Conserver les sélections automatiques/manuelles sans imposer un format précédent à une nouvelle image non reconnue.
- [ ] Tester fichier JSON absent, ancien, partiel ou corrompu.

### 10.4 Crédits et références

- [ ] Maintenir dans À propos les projets réellement utilisés ou étudiés, leur rôle, leur licence et leur lien.
- [ ] Distinguer code intégré, dépendance binaire et simple référence technique.
- [ ] Conserver Greaseweazle, HxC, SkiaSharp, .NET/WPF et Inno Setup dans la liste.
- [ ] Ajouter toute nouvelle référence de format/protection utilisée pendant les validations.

## 11. Tests et garde-fous d’architecture

- [ ] Ajouter des tests interdisant une dépendance de l’UI vers une implémentation concrète de codec.
- [ ] Ajouter des tests garantissant l’unicité des identifiants de catalogue.
- [ ] Ajouter des tests de parité décodeur/encodeur/capacités.
- [ ] Ajouter des tests garantissant que chaque format affiché possède le parcours annoncé.
- [ ] Ajouter des tests garantissant que la sélection explicite ne lance pas d’autres détecteurs.
- [ ] Ajouter des tests de cache et d’annulation lors d’un changement rapide d’image.
- [ ] Ajouter des tests de non-régression par famille avant chaque extraction structurelle.
- [ ] Créer les nouveaux tests dans des fichiers ciblés ; le découpage de l’ancien gros fichier de tests reste secondaire.
- [ ] Ajouter un contrôle des chaînes visibles codées en dur, avec liste blanche technique documentée.
- [ ] Ajouter un contrôle de taille/responsabilités pour empêcher la recréation d’un nouveau monolithe multi-machines.

## 12. Build local, versionnement et GitHub Actions

### 12.1 Scripts locaux

- [ ] Conserver `scripts/build.ps1` pour le build rapide dans `artifacts/build/GW GUI`.
- [ ] Conserver `scripts/package.ps1` pour la publication complète, le portable, le ZIP, l’installateur et les sommes SHA-256.
- [ ] Vérifier que les scripts utilisent la même version et les mêmes propriétés de compilation.
- [ ] Distinguer clairement version produit, numéro de build et révision Git pour EXE et DLL.

### 12.2 Workflow de build continu à créer

Le dépôt possède déjà `.github/workflows/release.yml`, consacré aux tags et paquets de publication. Il manque un workflow continu distinct.

- [ ] Créer `.github/workflows/build.yml` pour les pushes et pull requests.
- [ ] Utiliser un runner Windows et la version .NET déclarée par le projet.
- [ ] Restaurer les dépendances une seule fois.
- [ ] Compiler la solution en Release.
- [ ] Exécuter les tests automatisés adaptés au CI, sans matériel et sans corpus privé `image_test`.
- [ ] Exécuter les contrôles de ressources, traductions, encodage et architecture.
- [ ] Produire un artifact de build testable, distinct du paquet final de release.
- [ ] Donner à l’artifact un nom contenant version/build/révision.
- [ ] Définir une durée de conservation raisonnable.
- [ ] Annuler les anciens runs d’une même branche lorsqu’un nouveau commit arrive.
- [ ] Vérifier que les scripts PowerShell échouent réellement avec un code non nul en cas d’erreur.

### 12.3 Workflow de release existant à auditer

- [ ] Conserver le déclenchement manuel et sur tag.
- [ ] Vérifier que la version fournie et le tag sont cohérents.
- [ ] Réutiliser autant que possible les contrôles du workflow de build sans duplication fragile.
- [ ] Étendre les essais de l’installateur au système multilingue selon un échantillon représentatif et les contrôles de ressources complets.
- [ ] Vérifier ZIP, installateur, SHA-256, lancement, fermeture et sauvegarde des réglages.
- [ ] Publier la release uniquement après succès de toutes les étapes.
- [ ] Ne jamais inclure `image_test` ou `validated_images` dans les artifacts.

## 13. Documentation à maintenir pendant les travaux

- [ ] Mettre à jour ce fichier après chaque phase terminée.
- [ ] Mettre à jour `project-handoff.md` lorsque l’état réel change de manière importante.
- [ ] Mettre à jour `Liste-imagesdisk.md` pour les familles et conteneurs effectivement pris en charge.
- [ ] Conserver les résultats détaillés des images validées dans un journal dédié plutôt que dans les commits.
- [ ] Documenter les décisions de structure qui empêchent de comprendre le code sans relire l’historique.
- [ ] Corriger progressivement les anciens documents devenus faux, sans effacer leur valeur historique.

## 14. Validation finale du corpus `image_test` — toujours en dernier

Cette phase vient après le refactoring, les catalogues, l’UI, les traductions, la robustesse et les workflows. Elle reprend exactement le protocole demandé.

### 14.1 Ordre et classement

- [ ] Parcourir `image_test` dans l’ordre alphabétique des dossiers.
- [ ] Ignorer entièrement `image_test/validated_images`.
- [ ] Traiter aussi les fichiers produits sous `_generated`.
- [ ] Tester une image à la fois et communiquer son résultat avant la suivante.
- [ ] Une fois une image totalement validée, la **déplacer** vers : `validated_images/<marque>/<modèle>/<type de disquette>/`.
- [ ] Ne laisser aucune copie dans le dossier d’origine.
- [ ] Supprimer les fichiers texte/parasites devenus inutiles dans un dossier terminé.
- [ ] Supprimer chaque dossier source devenu vide.
- [ ] Classer aussi les images générées dans la bonne marque, le bon modèle et le bon type, pas dans un dossier global `generated` final.

### 14.2 Contrôles obligatoires pour chaque image ou SCP

- [ ] Le conteneur est lu sans erreur indue.
- [ ] La machine est détectée correctement ou l’état Aucun est affiché.
- [ ] Le format et la géométrie sont corrects.
- [ ] Le codec/décodeur utilisé est correct.
- [ ] Le décodage retrouve les pistes, faces et secteurs attendus.
- [ ] L’encodeur correspondant produit un résultat relisible lorsqu’un aller-retour est possible.
- [ ] La conversion via `gw` ou interne propose uniquement les sorties réellement compatibles.
- [ ] Le Visualisateur affiche le média, les faces, pistes, anomalies, légende et inspecteur correctement.
- [ ] L’Explorateur affiche le système, la protection, le volume, dossiers, fichiers, types, tailles, dates, espace libre et avertissements.
- [ ] Une image protégée est décodée selon sa protection connue ; à défaut de catalogue, sa structure physique réelle reste accessible sans faux fichiers.
- [ ] Les listes Lecture, Écriture, Conversion, Explorateur et Visualisateur contiennent les entrées nécessaires et cohérentes.
- [ ] Les textes nouveaux sont localisés dans toutes les langues nécessaires.
- [ ] Le chargement est rapide, annulable et remplaçable par une nouvelle image.
- [ ] Les erreurs et avertissements détaillés sont consultables sans bloquer le chargement suivant.
- [ ] La tâche de validation terminée est enregistrée dans un commit, même si elle ne concernait que le classement ou la documentation.
- [ ] Le commit est poussé immédiatement ou avec les tâches liées dès que leur ensemble constitue un bloc complet.

### 14.3 Contrôles physiques en fin de parcours

- [ ] Lecture réelle avec Greaseweazle sur les types de disquettes disponibles.
- [ ] Écriture sur une disquette de test sacrifiable, puis relecture et comparaison.
- [ ] Conversion des captures obtenues vers les sorties compatibles.
- [ ] Visualisation des captures réelles.
- [ ] Exploration des images et des captures reconstruites.
- [ ] Effacement et vérification uniquement sur le support de test prévu.
- [ ] Ne pas tester le nettoyage des têtes ni la mise à jour du firmware.
- [ ] Tester ultérieurement plusieurs contrôleurs/lecteurs physiques lorsque le matériel existe.

## Critère d’achèvement global

Le chantier est terminé lorsque le code est orienté par des contrats et catalogues cohérents, qu’aucun lecteur multi-machines mal nommé ni chaîne de conditions équivalente ne subsiste, que les cinq interfaces utilisent les mêmes données, que les ressources sont organisées et contrôlées, que le build continu fonctionne, et que chaque image restante a été validée puis déplacée dans `validated_images` selon le protocole ci-dessus.
