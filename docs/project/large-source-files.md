# Fichiers de plus de 500 lignes dans les projets GWGUI

Ce relevé couvre les fichiers texte suivis par Git dans les dossiers `GWGUI.*`. Les sorties générées
`bin`, `obj`, `build` et `artifacts` sont exclues. Un fichier de 500 lignes exactement est exclu.

## Sévérité

- **Critique** : classe centrale beaucoup trop volumineuse ou réunissant plusieurs responsabilités.
- **Élevée** : logique importante qui devrait probablement être découpée.
- **Moyenne** : fichier volumineux, mais consacré à un domaine spécialisé.
- **Normale** : taille cohérente avec des ressources, traductions, dessins, shaders, catalogues ou tests.
- **Portage** : adaptation d’une autre implémentation ; le découpage doit préserver la correspondance avec la source.

## GWGUI.App

### Logique et interface

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` | 1 292 | **Critique** — Découpage effectué |
| `src/GWGUI.App/Services/Input/GameInput/GameInputControllerReader.cs` | 1 253 | **Critique** — Découpage effectué |
| `src/GWGUI.App/Rendering/Emulation/Processing/OpenGlVideoProcessingProgram.cs` | 983 | Moyenne, wrapper OpenGL spécialisé — Découpage effectué |
| `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` | 810 | **Élevée**, code-behind central — Découpage effectué |
| `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsSection.cs` | 767 | Moyenne, construction d’interface — Découpage effectué |
| `src/GWGUI.App/Rendering/Emulation/Processing/SoftwareEmulationVideoProcessingPipeline.cs` | 754 | Moyenne, traitements vidéo spécialisés — Découpage effectué |
| `src/GWGUI.App/Options/Controllers/UpdateOptionsController.cs` | 695 | **Élevée**, contrôleur applicatif — Découpage effectué |
| `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs` | 644 | Normale, dessin d’interface — Découpage effectué |
| `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` | 612 | Moyenne, interface dynamique — Découpage effectué |
| `src/GWGUI.App/Views/Controls/Explorer/ExplorerSection.xaml.cs` | 598 | Moyenne, code-behind — Découpage effectué |
| `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualInput.cs` | 591 | Normale, visualisation — Découpage effectué |
| `src/GWGUI.App/Rendering/Emulation/Processing/VeldridVideoProcessingShaders.cs` | 567 | Normale, données de shaders — Découpage effectué |
| `src/GWGUI.App/Controllers/MainWindow/ReadTabController.cs` | 559 | **Élevée**, contrôleur applicatif — Découpage effectué |
| `src/GWGUI.App/Views/Controls/Options/OptionsControllersSection.xaml.cs` | 537 | Moyenne, code-behind — Découpage effectué |
| `src/GWGUI.App/Services/Input/GameInput/Hid/HidReportDecoder.cs` | 531 | Moyenne, décodage spécialisé — Découpage effectué |

## GWGUI.Emulation

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Pfs3Directory.cs` | 3 584 | **Élevée**, portage extrêmement volumineux — Renommage et découpage effectués |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Pfs3Disk.cs` | 1 103 | Portage — Renommage et découpage effectués |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Pfs3Anodes.cs` | 1 037 | Portage — Renommage et découpage effectués |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/FastFileSystem/FastFileSystemDirectory.cs` | 975 | Portage — Renommage et découpage effectués |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Pfs3Update.cs` | 953 | Portage — Renommage et découpage effectués |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Pfs3Allocation.cs` | 869 | Portage — Renommage et découpage effectués |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Pfs3VolumeOperations.cs` | 545 | Portage — Renommage et découpage effectués |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/FastFileSystem/FastFileSystemEntryStream.cs` | 512 | Portage — Renommage et découpage effectués |

## GWGUI.Emulation.Amiga

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.Emulation.Amiga/Services/AmigaExternalHostCallbacks.cs` | 638 | Moyenne, interop et callbacks — Découpage effectué |

## GWGUI.Emulation.Atari

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs` | 513 | Normale à moyenne, catalogue de réglages — Découpage effectué |

## GWGUI.Tests

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `tests/GWGUI.Tests/Interface/VisualizerViews/VisualizerDocumentScenarios.cs` | 693 | Normale, scénarios de tests |
| `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` | 553 | Normale, tests de disposition |

## GWGUI.VideoPresentation

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.VideoPresentation/Dictionaries/EmulationVideoProcessingCatalog.cs` | 527 | Normale, catalogue déclaratif — Découpage effectué |

## Priorités

Tous les fichiers de production recensés ont été découpés. Les deux fichiers restant sans mention de
découpage appartiennent à `GWGUI.Tests` et sont conservés tels quels.
