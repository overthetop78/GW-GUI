# Audit autonome du corpus de médias

Source : `F:\Rétro`

Point de départ : `F:\Rétro\A Trier\Atari 400-800\Atari 8bit - Applications - [ATR] (TOSEC-v2023-08-29)\8bit Mouse, The v2.01 (19xx)(Broomfield, Graham - Hunt, Colin)`

Résultats : `F:\GW GUI\artifacts\media-audit`

## Infrastructure

- [x] Remplacer les anciens tests locaux par le validateur autonome.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/GWGUI.LocalDiskImageTests.csproj` pour produire l'exécutable d'audit.
  - [x] Créer `tests/GWGUI.LocalDiskImageTests/Program.cs` avec les contrôles de reconnaissance, exploration, visualisation, écriture physique et conversion.
  - [x] Créer `tests/GWGUI.LocalDiskImageTests/MediaAuditReport.cs` avec le schéma du rapport persistant.
- [x] Créer le parcours autonome et sa reprise.
  - [x] Créer `scripts/audit-media-corpus.ps1` avec les paramètres `ImagePath`, `Root`, `StartAt`, `OutputRoot` et `Restart`.
  - [x] Modifier `scripts/audit-media-corpus.ps1` pour isoler les contrôles directs dans `artifacts/media-audit/single-tests` sans numéro ni modification du checkpoint continu.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/Program.cs` pour énumérer les chemins longs et exclure les fichiers annexes CUE, CCD et MDS.
- [ ] Réduire les artefacts sans perdre les données nécessaires à une analyse ultérieure.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaAuditReport.cs` pour référencer la source et ses fichiers associés dans le rapport principal.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/Program.cs` pour conserver les empreintes de la source et de ses fichiers associés sans copie permanente ni duplication HEX.
  - [x] Modifier `scripts/audit-media-corpus.ps1` pour copier automatiquement l'image fautive dans son dossier de diagnostic et supprimer cette copie lorsqu'elle passe après correction.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/Program.cs` pour enregistrer l'empreinte des contenus extraits sans conserver une seconde copie après leur identification.
  - [ ] Modifier `scripts/audit-media-corpus.ps1` pour numéroter le premier fichier demandé `00000000`.
  - [ ] Supprimer puis recréer `artifacts/media-audit/items`, `artifacts/media-audit/checkpoint.json` et `artifacts/media-audit/failure.json` avec le nouveau format.

## Disquettes

- [ ] Atari 8 bits
  - [x] ATR avec Atari DOS.
    - [x] Créer les rapports des cinq premières images ATR reconnues dans `artifacts/media-audit/items` et conserver leurs catalogues et fichiers extraits.
    - [x] Modifier `tests/GWGUI.LocalDiskImageTests/Program.cs` pour enregistrer les entrées Atari DOS ouvertes en écriture comme avertissements du média.
  - [ ] ATR K-file.
    - [x] Créer `src/GWGUI.MediaEngine/FileSystems/Atari/KFile/AtariKFileFileSystemReader.cs` pour reconnaître le chargeur KBoot et extraire l'exécutable en `RUN.XEX`.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemIds.cs` pour ajouter `atari-k-file`.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/Definitions/FileSystemDisplayNames.cs` pour ajouter `Atari K-file`.
    - [x] Modifier `src/GWGUI.MediaEngine/FileSystems/FileSystemReaderCatalog.cs` pour enregistrer le lecteur après Atari DOS.
    - [ ] Modifier `docs/tasks/media-corpus-audit.md` pour cocher ATR K-file après validation sur le média arrêté.
  - [ ] Autres formats Atari rencontrés.
    - [ ] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque format Atari au premier arrêt correspondant et son résultat après correction.
- [ ] Autres systèmes de disquettes.
  - [ ] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque système et format au premier arrêt correspondant et son résultat après correction.

## Cassettes

- [ ] Formats de cassette rencontrés.
  - [ ] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque format au premier arrêt correspondant et son résultat après correction.

## Supports optiques

- [ ] Formats optiques rencontrés.
  - [ ] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque format au premier arrêt correspondant et son résultat après correction.

## Disques durs et supports à blocs

- [ ] Formats de disque dur rencontrés.
  - [ ] Modifier `docs/tasks/media-corpus-audit.md` pour ajouter chaque format au premier arrêt correspondant et son résultat après correction.

## Fin du corpus

- [ ] Achever le parcours sans erreur du logiciel.
  - [ ] Modifier `artifacts/media-audit/checkpoint.json` avec l'état `complete` et le nombre total de fichiers contrôlés.
  - [ ] Modifier `docs/tasks/media-corpus-audit.md` pour cocher toutes les familles réellement rencontrées et consigner la fin du parcours.

## Contrôle récursif des contenus

- [ ] Identifier chaque fichier extrait, y compris dans les sous-dossiers du média.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/MediaAuditReport.cs` pour enregistrer catégorie, format, encodage, mode d'exécution, aperçu et empreinte du contenu.
  - [x] Modifier `tests/GWGUI.LocalDiskImageTests/Program.cs` pour parcourir récursivement les entrées et arrêter l'audit lorsqu'un fichier reste en catégorie inconnue.
  - [ ] Modifier les tables et détecteurs sous `src/GWGUI.App/Dictionaries/Explorer/FileTypes` et `src/GWGUI.App/Functions/Explorer` à chaque nouveau contenu fiable rencontré.
