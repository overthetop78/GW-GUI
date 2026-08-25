# Inventaire exhaustif du dépôt

## Vue d’ensemble

| Type | Quantité observée | Organisation |
|---|---:|---|
| C# | 243 | quatre projets de production et un projet de tests |
| XAML | 35 | application WPF et composants visuels |
| Ressources `.resx` | 600 | 20 catalogues × 30 variantes |
| Scripts PowerShell | 17 | build, packaging, traduction et corpus |
| Projets `.csproj` | 5 | App, Domain, Infrastructure, Scp, Tests |
| Workflow GitHub | 1 | publication sur demande ou tag |
| Installateur principal | 1 | Inno Setup, complété par 6 fichiers `.isl` |

Les tableaux ci-dessous inventorient les fichiers par responsabilité réelle. Lorsqu’une ligne contient une liste de fichiers homogènes, la conclusion indiquée s’applique individuellement à chaque fichier de la liste.

## `GWGUI.App`

### Fenêtres et composition

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `App.xaml` | ressources et styles globaux WPF | cohérent ; dépend des ressources de thème/localisation |
| `App.xaml.cs` | démarrage, restauration initiale, gestion globale des exceptions | plusieurs sujets de bootstrap acceptables, à isoler par services si croissance |
| `AssemblyInfo.cs` | métadonnées WPF d’assembly | cohérent |
| `app.manifest` | manifeste Windows | cohérent |
| `GWGUI.App.csproj` | build WPF, dépendances, version applicative, langues et contenu | versioning non centralisé |
| `MainWindow.xaml` | coque, onglets, console, statut et composition visuelle | contenu réduit mais encore lié à une classe centrale |
| `MainWindow.xaml.cs` | composition et orchestration de presque tout le produit | monolithe principal à découper |
| `OptionsWindow.xaml` | onglets Général, Contrôleurs/Lecteurs et Profils | structure unique correcte, pages à externaliser |
| `OptionsWindow.xaml.cs` | réglages, matériel, Host Tools, profils, tags et journaux | monolithe secondaire ; contient aussi des modèles de lignes |
| `AboutWindow.xaml`, `AboutWindow.xaml.cs` | dialogue À propos et version affichée | cohérent, mais version tronquée |
| `ConversionConflictWindow.xaml`, `.xaml.cs` | résolution des conflits de sorties de conversion | cohérent |
| `ExplorerIssuesWindow.xaml`, `.xaml.cs` | liste détaillée des avertissements/erreurs d’exploration | cohérent |
| `GwToolWindow.xaml`, `.xaml.cs` | dialogue générique des diagnostics et outils matériels | peut rester générique tant que les définitions sont déclaratives |
| `HardwareUnavailableWindow.xaml`, `.xaml.cs` | choix lors d’un matériel indisponible | cohérent |
| `LogHistoryWindow.xaml`, `.xaml.cs` | consultation des journaux | cohérent |
| `ProfileNameWindow.xaml`, `.xaml.cs` | saisie du nom d’un profil | cohérent |
| `ReadConflictWindow.xaml`, `.xaml.cs` | conflit de fichier de Lecture | cohérent |
| `ScpInspectorWindow.xaml`, `.xaml.cs` | hôte détachable de l’inspecteur | cohérent ; état à partager avec le panneau intégré |

### Contrôles réutilisables

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `CardSection.cs` | conteneur visuel de carte | cohérent |
| `MainMenu.xaml`, `.xaml.cs` | menus Options/Aide et événements de navigation | composant réel, cohérent |
| `MainTabHeader.xaml`, `.xaml.cs` | icône et texte d’un onglet principal | cohérent |
| `ProfileSection.xaml`, `.xaml.cs` | sélection/sauvegarde/réinitialisation d’un profil | réutilisable ; chaque instance conserve son store propre |
| `PathSection.xaml`, `.xaml.cs` | chemin et bouton Parcourir | réutilisable |
| `ReadImageSection.xaml`, `.xaml.cs` | choix brut/format connu en Lecture | spécifique Lecture, cohérent |
| `ReadFileNameSection.xaml`, `.xaml.cs` | nom de sortie de Lecture | spécifique Lecture, cohérent |
| `WriteFormatSection.xaml`, `.xaml.cs` | classification/format d’Écriture | cohérent |
| `WriteAdvancedSection.xaml`, `.xaml.cs` | options avancées d’Écriture | cohérent |
| `ConversionAdvancedSection.xaml`, `.xaml.cs` | options avancées de Conversion | cohérent |
| `ConversionOutputSection.xaml`, `.xaml.cs` | source, nom de sorties et tags | cohérent |
| `ConversionFormatsSection.xaml`, `.xaml.cs` | sélection multiformat de Conversion | logique de liste encore liée aux modèles App |
| `ConversionFormatControl.xaml`, `.xaml.cs` | une ligne/entrée de format de conversion | cohérent |
| `DiskClassificationSelector.xaml`, `.xaml.cs` | auto-détection, machine, format et protection | composant partagé à relier à un catalogue unique |
| `ExplorerSection.xaml` | mise en page arborescence/liste/détails | cohérent visuellement |
| `ExplorerSection.xaml.cs` | contrôle + modèles UI + formatage | responsabilités à séparer |
| `ExplorerDetailsPanel.xaml` | panneau d’informations disque/dossier/fichier | cohérent |
| `ExplorerDetailsPanel.xaml.cs` | contrôle + records + présentateur | responsabilités à séparer |
| `ExplorerFileIconClassifier.cs` | type/icône selon machine, FS et extension | connaissance technique placée dans App ; catalogue par système nécessaire |
| `FileEntryIcon.xaml`, `.xaml.cs` | dessin d’icône de fichier/dossier | cohérent |
| `TerminalSection.xaml`, `.xaml.cs` | terminal intégré et copie de sortie | cohérent |
| `TrackProgressStrip.xaml`, `.xaml.cs` | deux lignes de blocs de pistes/faces | cohérent ; données fournies par le tracker |
| `ScpDiskView.xaml`, `.xaml.cs` | surface d’une face, zoom/pan/sélection/rendu | cohérent, dépend de `IScpRenderer` |
| `ScpInspectorPanel.xaml`, `.xaml.cs` | panneau déplaçable Résumé/Révolutions/Structures/Secteurs | cohérent ; modèles/presenter séparables |
| `VisualizerHeaderSection.xaml`, `.xaml.cs` | fichier courant, classification et commandes du visualiseur | cohérent |
| `VisualizerLegend.xaml`, `.xaml.cs` | légende des couleurs | cohérent |
| `VisualizerTrackOverview.xaml`, `.xaml.cs` | résumé par piste et face | cohérent |

### Présentation, rendu et services

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `ScpDecoderChoice.cs` | choix visible d’un décodeur | modèle de présentation à rapprocher du catalogue de codecs |
| `StoragePaths.cs` | chemins Data, journaux, Host Tools et mode portable | constantes de stockage cohérentes |
| `ThemeManager.cs` | application immédiate du thème | cohérent |
| `Localization/LocExtension.cs` | agrégation des catalogues et binding WPF | composant central, sensible à toute réorganisation des ressources |
| `Localization/UiLanguageResolver.cs` | catalogue et résolution des cultures | sépare données et résolution imparfaitement |
| `Localization/ExplorerWarningLocalizer.cs` | traduction structurée des avertissements | cohérent |
| `Rendering/IScpRenderer.cs` | contrat du rendu disque | cohérent |
| `Rendering/SkiaScpRenderer.cs` | rendu Skia des flux/structures | volumineux mais spécialisé |
| `Services/BusinessDialogService.cs` | dialogues métier de conflits/confirmation | cohérent |
| `Services/CancelledOutputCleaner.cs` | suppression d’une sortie partielle annulée | cohérent |
| `Services/ConversionBatchExecutor.cs` | exécution séquentielle d’un plan de conversions | cohérent |
| `Services/ErrorLog.cs` | journal global des exceptions | cohérent |
| `Services/FileDialogService.cs` | ouverture/sauvegarde de fichiers et mémorisation des dossiers | cohérent |
| `Services/MessageDialogService.cs` | abstraction des messages WPF | cohérent |
| `Services/MonitorWorkArea.cs` | calcul écran/DPI/zone de travail | cohérent |
| `Services/StartupHardwareMonitor.cs` | vérification silencieuse du matériel configuré | cohérent |
| `Services/WindowNavigationService.cs` | ouverture modale des fenêtres | cohérent |

### ViewModels

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `MainWindowViewModel.cs` | état observable global limité | insuffisant pour retirer l’orchestration de la vue |
| `ReadOperationViewModel.cs` | état/commande de Lecture | à devenir propriétaire complet de l’onglet Lecture |
| `WriteOperationViewModel.cs` | état/commande d’Écriture | même cible pour Écriture |
| `ConversionOperationViewModel.cs` | état/commande de Conversion | même cible pour Conversion |
| `OperationCoordinator.cs` | exclusivité, annulation et cycle commun | cohérent |
| `OperationOptionViewModels.cs` | options observables communes | plusieurs petits modèles proches, cohérent pour l’instant |
| `OperationResultPresenter.cs` | transforme résultat en état UI/console | cohérent |
| `ConversionConflictResolver.cs` | applique les décisions de conflit | cohérent |
| `ConversionFormatPresenter.cs` | transforme le catalogue en choix visibles | dépendance centrale à préserver |
| `ScpDocumentLoader.cs` | charge un SCP sans bloquer et publie la progression | nom devenu trop étroit si toutes les images sont couvertes |
| `ScpInspectorPresenter.cs` | construit la vue de l’inspecteur | cohérent |

### Assets

| Fichier | Responsabilité |
|---|---|
| `Assets/app-icon.png`, `app-icon-chroma.png`, `app-icon.ico` | icônes distribuées |
| `Assets/DiskDefinitions/built-in.cfg` | définitions `diskdefs` intégrées et copiées au build |

## `GWGUI.Domain`

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `GWGUI.Domain.csproj` | projet métier indépendant | cohérent, version commune absente |
| `Commands/GwCommand.cs` | représentation d’une commande et rendu lisible | cohérent |
| `Commands/GwExecution.cs` | contrat/résultat/exécution abstraite | vérifier séparation interface/modèles si croissance |
| `Commands/IGwCommandBuilder.cs` | contrat de construction | cohérent |
| `Commands/GwBatchExecutor.cs` | exécution d’un lot | cohérent |
| `Commands/GwProgressTracker.cs` | interprétation des lignes gw en progression par pistes | cohérent, grammaire sensible aux versions gw |
| `Commands/GwOptionValidator.cs` | validation de plusieurs syntaxes d’options | plusieurs grammaires dans un fichier |
| `Conversion/ConversionPlanner.cs` | modèles, builder et planification | mélangé |
| `Conversion/ConversionSourceCompatibility.cs` | compatibilité source/sorties | cohérent |
| `Formats/BuiltInDiskDefinitions.cs` | accès aux `diskdefs` intégrées | cohérent |
| `Formats/DiskClassificationCatalog.cs` | machines/formats/protections communs | doit devenir la source unique de classification |
| `Formats/DiskDefsFormatReader.cs` | lecture du fichier cfg | cohérent |
| `Formats/GwFormatArgument.cs` | construction de l’argument `--format` | cohérent |
| `Formats/GwFormatCapabilities.cs` | capacités découvertes de gw | cohérent |
| `Formats/GwVisualizationPolicy.cs` | décision d’utiliser ou non gw pour visualiser une extension | cohérent, à alimenter par catalogue plutôt que listes dupliquées |
| `Formats/ImageFormatCatalog.cs` | modèles + interface + catalogues | mélangé et destiné à grossir |
| `Hardware/GwDeviceInfo.cs` | modèles contrôleur/lecteur | cohérent |
| `Hardware/HardwareRoutingPolicy.cs` | `--device`/`--drive` selon configuration | cohérent |
| `Hardware/IHardwareRegistry.cs` | contrat registre et résultats de scan | cohérent |
| `Hardware/SerialDevice.cs` | périphérique série découvert | cohérent |
| `HostTools/IGwInstallationManager.cs` | contrat et modèles Host Tools | cohérent pour un contrat unique |
| `Maintenance/MaintenanceCommands.cs` | modèles des actions de maintenance | cohérent |
| `Maintenance/ToolCommandBuilder.cs` | commandes Diagnostics/Matériel | cohérent |
| `Naming/OutputConflictResolver.cs` | politique d’écrasement/numéro suivant | cohérent |
| `Naming/SequenceFormatter.cs` | séquences numériques/alphabetiques | cohérent |
| `Profiles/IProfileStore.cs` | contrat + implémentation mémoire | implémentation dans Domain à revoir |
| `Profiles/OperationProfile.cs` | profil et portée par onglet | cohérent |
| `Read/ReadRequest.cs` | modèles + builder + tokenizer | mélangé |
| `Write/WriteRequest.cs` | détection + modèles + builder | mélangé |
| `Settings/AppSettings.cs` | tous les modèles de réglages + contrat store | trop de sujets dans un fichier court |
| `Settings/SettingsMigrator.cs` | migration de schéma | cohérent |
| `Settings/WindowPlacementPolicy.cs` | validation/confinement du placement | cohérent |

## `GWGUI.Infrastructure`

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `GWGUI.Infrastructure.csproj` | infrastructure dépendante de Domain | cohérent, version commune absente |
| `Hardware/WindowsSerialDeviceDiscovery.cs` | inventaire Windows des ports/PnP | cohérent |
| `Hardware/GreaseweazleDeviceMatcher.cs` | identification stable numéro de série/PnP | cohérent |
| `Hardware/GreaseweazleHardwareRegistry.cs` | scan, vérification et persistance logique des contrôleurs | cohérent mais sensible aux règles configuré/détecté |
| `HostTools/GwFormatCapabilityReader.cs` | lecture des formats supportés par gw | cohérent |
| `HostTools/GwInstallationManager.cs` | détection, version, téléchargement, extraction et historique | plusieurs étapes d’un même cas d’usage ; extraction si réutilisation |
| `Processes/GreaseweazleRunner.cs` | lancement asynchrone, sortie, annulation et verrou global | cohérent |
| `Processes/ConsoleLogSession.cs` | session de journal d’une opération | cohérent |
| `Processes/RotatingOperationLogWriter.cs` | limite, rotation et conservation des journaux | cohérent |
| `Settings/JsonSettingsStore.cs` | sérialisation atomique JSON | cohérent |

## `GWGUI.MediaEngine`

### Conteneur, flux et contrats

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `GWGUI.MediaEngine.csproj` | moteur disque indépendant | cohérent, version commune absente |
| `ScpCaptureInfo.cs` | résumé d’une capture | cohérent |
| `ScpImage.cs` | modèles SCP + parser/header/track data | modèles et lecture à séparer |
| `Flux/FluxBitstream.cs` | conversion timing/cellules | cohérent |
| `Decoding/FluxDecodeModels.cs` | résultats et structures décodées | cohérent |
| `Decoding/IFluxDecoder.cs` | contrat d’un décodeur | cohérent |
| `Decoding/FluxDecoderRegistry.cs` | liste, choix manuel et automatique | registre concret codé en dur |
| `Decoding/Base/SignatureMfmDecoder.cs` | base de codecs à signatures | cohérent si paramètres réellement communs |
| `Encoding/FluxEncoding.cs`, `Encoding/TrackEncoding.cs` | primitives d’encodage de flux et de piste | responsabilités proches mais distinctes ; clarifier les noms sans les fusionner |
| `Encoding/TrackEncodeModels.cs` | contrats et résultats d’encodage | cohérent |
| `Encoding/TrackEncoderBase.cs` | base d’encodeur | cohérent |
| `Encoding/TrackEncoding.cs` | primitives de piste | cohérent |
| `Encoding/FluxEncoderRegistry.cs` | liste des encodeurs | registre concret codé en dur |
| `FileSystems/FileSystemModels.cs` | document, entrées, avertissements et contrat lecteur | ensemble cohérent de contrats |
| `FileSystems/FileSystemRegistry.cs` | liste et détection de systèmes de fichiers | ordre des lecteurs significatif, construction codée en dur |
| `SectorImages/SectorImage.cs` | modèle sectoriel intermédiaire | cohérent |
| `Images/DiskImageMetadata.cs` | métadonnées machine/format/protection | cohérent |

### Décodeurs individuels

Chaque fichier suivant contient un décodeur de piste autonome et doit rester séparé :

`Aed6200pMfmDecoder.cs`, `AmigaMfmDecoder.cs`, `AppleIIGcrDecoder.cs`, `AppleLisaFileWareGcrDecoder.cs`, `AppleMacGcrDecoder.cs`, `AppleRwts18Decoder.cs`, `ArburgDecoder.cs`, `CenturionMfmDecoder.cs`, `Commodore900GcrDecoder.cs`, `CommodoreGcrDecoder.cs`, `DataGeneralFmDecoder.cs`, `DecRx02Decoder.cs`, `EmuFmDecoder.cs`, `HeathkitFmDecoder.cs`, `HpMmfmDecoder.cs`, `IsoFmDecoder.cs`, `IsoMfmDecoder.cs`, `MembrainMfmDecoder.cs`, `MicralNFmDecoder.cs`, `MicropolisMfmDecoder.cs`, `NorthstarMfmDecoder.cs`, `QdMo5MfmDecoder.cs`, `RawFluxDecoder.cs`, `TycomFmDecoder.cs`, `Victor9kGcrDecoder.cs`.

Conclusion commune : bonne granularité. Les identifiants, noms visibles et paramètres partagés doivent venir de définitions, sans fusionner les algorithmes.

### Encodeurs individuels

Chaque fichier suivant contient l’encodeur correspondant et doit rester séparé :

`Aed6200pMfmTrackEncoder.cs`, `AmigaMfmTrackEncoder.cs`, `AppleIIGcrTrackEncoder.cs`, `AppleLisaFileWareGcrTrackEncoder.cs`, `AppleMacGcrTrackEncoder.cs`, `AppleRwts18TrackEncoder.cs`, `ArburgTrackEncoder.cs`, `CenturionMfmTrackEncoder.cs`, `Commodore900GcrTrackEncoder.cs`, `CommodoreGcrTrackEncoder.cs`, `DataGeneralFmTrackEncoder.cs`, `DecRx02TrackEncoder.cs`, `EmuFmTrackEncoder.cs`, `HeathkitFmTrackEncoder.cs`, `HpMmfmTrackEncoder.cs`, `IsoFmTrackEncoder.cs`, `IsoMfmTrackEncoder.cs`, `MembrainMfmTrackEncoder.cs`, `MicralNFmTrackEncoder.cs`, `MicropolisMfmTrackEncoder.cs`, `NorthstarMfmTrackEncoder.cs`, `QdMo5MfmTrackEncoder.cs`, `TycomFmTrackEncoder.cs`, `Victor9kGcrTrackEncoder.cs`.

Conclusion commune : bonne granularité. La parité registre décodeur/encodeur et les tests par codec doivent être préservés.

### Lecteurs de systèmes de fichiers

Un fichier par système/famille :

`AcornAdfsFileSystemReader.cs`, `AcornFileCoreNewMap.cs`, `AmigaDosFileSystemReader.cs`, `AmstradCpmFileSystemReader.cs`, `AppleDosFileSystemReader.cs`, `AppleInformXzipFileSystemReader.cs`, `AtariDosFileSystemReader.cs`, `BbcDfsFileSystemReader.cs`, `CoherentFileSystemReader.cs`, `CommodoreDosFileSystemReader.cs`, `CpmFileSystemReader.cs`, `Fat12FileSystemReader.cs`, `LisaFileSystemReader.cs`, `MacHfsFileSystemReader.cs`, `MacMfsFileSystemReader.cs`, `ProDosFileSystemReader.cs`, `Rt11FileSystemReader.cs`, `UcsdFileSystemReader.cs`.

Conclusion commune : la granularité par système est correcte. `AcornFileCoreNewMap.cs` est un helper/variante du lecteur Acorn et doit être nommé/documenté comme tel. Les règles de classification de fichiers doivent venir du système identifié, pas d’une liste globale d’extensions.

### Lecteurs de conteneurs/images et orchestration

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `Images/AdfImageReader.cs` | ADF et contrat générique de lecteur sectoriel | déplacer le contrat hors du fichier ADF |
| `AmstradDskImageReader.cs` | DSK/EDSK Amstrad | cohérent |
| `AppleDiskImageReader.cs` | plusieurs conteneurs Apple II/III/Mac/Lisa et ordres | famille large ; séparer conteneurs/ordres |
| `AppleNibbleImageDecoder.cs` | décodage NIB/WOZ vers secteurs | cohérent |
| `AppleNibbleImageWriter.cs` | écriture nibble Apple | cohérent |
| `AppleRwts18ConversionService.cs` | conversion/déprotection RWTS18 interne | service spécialisé, à brancher par capacité et non fausse extension |
| `AtariStImageReader.cs` | ST/IMG Atari TOS | cohérent |
| `Containers/Atari/Atr/AtrReader.cs` | validation et lecture sectorielle ATR Atari 8 bits | cohérent ; définitions, disposition et erreurs ATR isolées dans le même module |
| `Conversion/Atari/AtrPayloadWriter.cs` | extraction d'une charge utile ATR validée | séparé du parser du conteneur |
| `BbcDfsImageReader.cs` | SSD/DSD BBC | cohérent |
| `CoherentImageReader.cs` | image Coherent | cohérent |
| `CommodoreD64ImageReader.cs`, `D71`, `D81` | conteneurs Commodore | séparés correctement ; géométrie partagée |
| `CommodoreGeometry.cs` | définitions communes Commodore | cohérent |
| `Cp2ImageReader.cs` | CP2 | cohérent |
| `DecRx02ImageReader.cs` | DEC RX02 | cohérent |
| `I86fImageReader.cs` | conteneur 86F | cohérent |
| `IbmPcImageReader.cs` | images sectorielles PC | cohérent, variations par géométrie à déclarer |
| `ImdImageReader.cs` | IMD | cohérent |
| `MsaImageReader.cs` | MSA Atari ST | cohérent |
| `MsxImageReader.cs` | MSX | cohérent |
| `Td0ImageReader.cs` | Teledisk TD0 | cohérent |
| `DiskImageExplorer.cs` | orchestrateur global de tous les lecteurs et FS | monolithe à remplacer par catalogues/stratégies |
| `SectorImageFluxVisualizer.cs` | représentation visuelle d’une image sectorielle | dépend de chaînes de formats ; classification à injecter |

### Reconstruction SCP vers image sectorielle

| Fichier | Responsabilité réelle | Constat |
|---|---|---|
| `AmigaScpSectorImageReader.cs` | reconstruction Amiga MFM | cohérent |
| `AppleScpSectorImageReader.cs` | reconstruction Apple GCR/RWTS18/Mac | plusieurs variantes Apple ; séparation par stratégie possible |
| `AtariScpSectorImageReader.cs` | reconstruction ISO FM/MFM multi-machine | nom faux et mélange confirmé |
| `CommodoreScpSectorImageReader.cs` | reconstruction Commodore GCR | cohérent |
| `DecRx02ScpSectorImageReader.cs` | reconstruction DEC RX02 | cohérent |

## Tests

| Fichier | Portée |
|---|---|
| `CoreTests.cs` | nombreuses fonctions Domain/App/Infrastructure ; monolithe à découper plus tard |
| `AdditionalFluxDecoderTests.cs`, `RecentFormatCodecTests.cs`, `TrackEncoderTests.cs` | codecs et parité |
| `AmstradDiskImageTests.cs`, `AppleDiskImageTests.cs`, `AtariDiskImageTests.cs`, `BbcDiskImageTests.cs`, `CoherentDiskImageTests.cs`, `CommodoreDiskImageTests.cs`, `Cp2ImageTests.cs`, `DecDiskImageTests.cs`, `EpsonQx10DiskImageTests.cs`, `I86fImageTests.cs`, `IbmPcDiskImageTests.cs`, `ImdImageTests.cs`, `MsxDiskImageTests.cs`, `UcsdDiskImageTests.cs` | formats et systèmes ciblés |
| `DiskImageExplorerTests.cs` | orchestration/détection/Explorateur |
| `SectorImageFluxVisualizerTests.cs` | visualisation sectorielle |
| `LocalizationTests.cs` | parité, encodage et qualité des ressources |
| `HostToolsTests.cs`, `RunnerTests.cs` | installation et processus |
| `ExternalDiskCorpusTests.cs`, `RealScpCorpusTests.cs` | corpus local ignoré par Git |
| `GWGUI.Tests.csproj` | projet xUnit et références |

## Ressources de traduction

Les 20 catalogues sont : `About`, `Actions`, `Advanced`, `Common`, `Conversion`, `Errors`, `Explorer`, `ExplorerWarnings`, `Formats`, `Hardware`, `HostTools`, `Logs`, `Menus`, `Options`, `Profiles`, `Read`, `Shell`, `Tools`, `Visualizer`, `Write`.

Chacun existe en ressource neutre et pour : `ar-SA`, `cs-CZ`, `da-DK`, `de-DE`, `el-GR`, `en-US`, `es-ES`, `fi-FI`, `fr-FR`, `he-IL`, `hu-HU`, `id-ID`, `it-IT`, `ja-JP`, `ko-KR`, `nb-NO`, `nl-NL`, `pl-PL`, `pt-BR`, `pt-PT`, `ro-RO`, `ru-RU`, `sv-SE`, `th-TH`, `tr-TR`, `uk-UA`, `vi-VN`, `zh-Hans`, `zh-Hant`.

Cela représente exactement 600 fichiers. L’organisation fonctionnelle est correcte ; la phase 06 doit seulement les déplacer sous un dossier `Languages` et un sous-dossier par catalogue/langue selon la décision documentée, sans recréer un fichier géant.

## Scripts, installateur et workflow

| Fichier | Responsabilité |
|---|---|
| `scripts/build.ps1` | build Release rapide vers `dist/build/GW GUI` |
| `scripts/package.ps1` | publish, portable, ZIP, installateur et SHA-256 |
| `scripts/create-icon.ps1` | génération des icônes |
| `scripts/google-translate-resx.ps1` | traduction assistée des ressources |
| `scripts/test-*-corpus.ps1` | corpus Amiga, Amstrad, Apple, Atari, Commodore, IBM et SCP |
| `scripts/test-app-accessibility.ps1` | audit automatisé d’accessibilité |
| `scripts/test-guide-images.ps1` | validation des captures de guide |
| `scripts/test-host-tools-releases.ps1` | vérification des releases Host Tools |
| `scripts/test-installer.ps1`, `test-installer-interactive.ps1`, `test-installer-upgrade.ps1` | installation, langues et mise à niveau |
| `installer/GWGUI.iss` | définition Inno Setup |
| six fichiers `.isl` de l’installateur | langues non fournies directement par Inno Setup |
| `.github/workflows/release.yml` | tests, package et release sur tag/déclenchement manuel |

## Fichiers racine

`GWGUI.sln` compose les projets. `README.md`, `LICENSE`, `.gitignore` et les documents sous `docs/` décrivent/distribuent le projet. Ils ne contiennent pas de logique applicative mais font partie du paquet et du processus de contribution.
