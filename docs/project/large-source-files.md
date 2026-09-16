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
| `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` | 1 292 | **Critique** |
| `src/GWGUI.App/Services/Input/GameInput/GameInputControllerReader.cs` | 1 253 | **Critique** |
| `src/GWGUI.App/Rendering/Emulation/Processing/OpenGlVideoProcessingProgram.cs` | 983 | Moyenne, wrapper OpenGL spécialisé |
| `src/GWGUI.App/Views/Windows/Shell/MainWindow.xaml.cs` | 810 | **Élevée**, code-behind central |
| `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsSection.cs` | 767 | Moyenne, construction d’interface |
| `src/GWGUI.App/Rendering/Emulation/Processing/SoftwareEmulationVideoProcessingPipeline.cs` | 754 | Moyenne, traitements vidéo spécialisés |
| `src/GWGUI.App/Options/Controllers/UpdateOptionsController.cs` | 695 | **Élevée**, contrôleur applicatif |
| `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs` | 644 | Normale, dessin d’interface |
| `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs` | 612 | Moyenne, interface dynamique |
| `src/GWGUI.App/Views/Controls/Explorer/ExplorerSection.xaml.cs` | 598 | Moyenne, code-behind |
| `src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualInput.cs` | 591 | Normale, visualisation |
| `src/GWGUI.App/Rendering/Emulation/Processing/VeldridVideoProcessingShaders.cs` | 567 | Normale, données de shaders |
| `src/GWGUI.App/Controllers/MainWindow/ReadTabController.cs` | 559 | **Élevée**, contrôleur applicatif |
| `src/GWGUI.App/Views/Controls/Options/OptionsControllersSection.xaml.cs` | 537 | Moyenne, code-behind |
| `src/GWGUI.App/Services/Input/GameInput/Hid/HidReportDecoder.cs` | 531 | Moyenne, décodage spécialisé |

### Ressources de traduction `Emulation.resx`

`00-Base` est la ressource neutre qui définit toutes les clés de localisation. Ces clés permettent au
système de retrouver le catalogue et la traduction demandés ; leurs valeurs fournissent aussi le texte
anglais de référence et la valeur de repli lorsqu’une traduction manque. `00-Base` contient en plus les
noms et valeurs invariants dans toutes les langues, comme RAM, ROM, CPU, GPU, PAL, SECAM ou NTSC. Il
peut donc contenir davantage d’entrées et être plus long. Toutes les langues, y compris `en-US`, doivent
en revanche contenir exactement les mêmes clés traduisibles, dans le même ordre et avec le même nombre
de lignes. `en-US` doit reprendre les valeurs anglaises de `00-Base`, sans les entrées invariantes. Une
valeur d’une autre langue strictement identique à `00-Base` signale soit une traduction anglaise non
remplacée, soit une valeur invariante recopiée alors qu’elle doit rester uniquement dans `00-Base`.

Ce n’est pas le cas pour `Emulation.resx` : les 28 langues traduites possèdent chacune 739 clés, mais
leur ordre et leur nombre de lignes diffèrent. `en-US/Emulation.resx` ne contient aucune entrée et ne
fait que 15 lignes. Il faut uniformiser les fichiers traduits sans retirer les invariants de `00-Base`.

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.App/Resources/00-Base/Emulation.resx` | 940 | Normale pour la base commune |
| `src/GWGUI.App/Resources/ar-SA/Emulation.resx` | 844 | **À uniformiser** |
| `src/GWGUI.App/Resources/el-GR/Emulation.resx` | 844 | **À uniformiser** |
| `src/GWGUI.App/Resources/ja-JP/Emulation.resx` | 844 | **À uniformiser** |
| `src/GWGUI.App/Resources/ko-KR/Emulation.resx` | 844 | **À uniformiser** |
| `src/GWGUI.App/Resources/th-TH/Emulation.resx` | 844 | **À uniformiser** |
| `src/GWGUI.App/Resources/uk-UA/Emulation.resx` | 844 | **À uniformiser** |
| `src/GWGUI.App/Resources/zh-Hans/Emulation.resx` | 844 | **À uniformiser** |
| `src/GWGUI.App/Resources/zh-Hant/Emulation.resx` | 844 | **À uniformiser** |
| `src/GWGUI.App/Resources/he-IL/Emulation.resx` | 843 | **À uniformiser** |
| `src/GWGUI.App/Resources/fr-FR/Emulation.resx` | 842 | **À uniformiser** |
| `src/GWGUI.App/Resources/pl-PL/Emulation.resx` | 841 | **À uniformiser** |
| `src/GWGUI.App/Resources/pt-BR/Emulation.resx` | 841 | **À uniformiser** |
| `src/GWGUI.App/Resources/pt-PT/Emulation.resx` | 841 | **À uniformiser** |
| `src/GWGUI.App/Resources/es-ES/Emulation.resx` | 840 | **À uniformiser** |
| `src/GWGUI.App/Resources/ru-RU/Emulation.resx` | 840 | **À uniformiser** |
| `src/GWGUI.App/Resources/vi-VN/Emulation.resx` | 840 | **À uniformiser** |
| `src/GWGUI.App/Resources/id-ID/Emulation.resx` | 839 | **À uniformiser** |
| `src/GWGUI.App/Resources/tr-TR/Emulation.resx` | 839 | **À uniformiser** |
| `src/GWGUI.App/Resources/hu-HU/Emulation.resx` | 837 | **À uniformiser** |
| `src/GWGUI.App/Resources/ro-RO/Emulation.resx` | 837 | **À uniformiser** |
| `src/GWGUI.App/Resources/cs-CZ/Emulation.resx` | 836 | **À uniformiser** |
| `src/GWGUI.App/Resources/de-DE/Emulation.resx` | 836 | **À uniformiser** |
| `src/GWGUI.App/Resources/fi-FI/Emulation.resx` | 836 | **À uniformiser** |
| `src/GWGUI.App/Resources/nb-NO/Emulation.resx` | 835 | **À uniformiser** |
| `src/GWGUI.App/Resources/da-DK/Emulation.resx` | 834 | **À uniformiser** |
| `src/GWGUI.App/Resources/sv-SE/Emulation.resx` | 834 | **À uniformiser** |
| `src/GWGUI.App/Resources/it-IT/Emulation.resx` | 833 | **À uniformiser** |
| `src/GWGUI.App/Resources/nl-NL/Emulation.resx` | 831 | **À uniformiser** |

### Contrôle de tous les fichiers `Resources`

Le contrôle porte sur `00-Base`, `en-US` et les 28 langues traduites. Tous les dossiers possèdent les
21 familles de ressources localisées. Dans `00-Base`, leurs clés forment l’index neutre qui relie chaque
demande de localisation au catalogue concerné. `00-Base` possède en plus `Icons.resx`, qui contient des
valeurs communes non traduites et n’a donc pas de copie localisée.

La structure attendue représente 1 770 clés traduisibles. Chacune des 28 langues non anglaises possède
bien ces 1 770 clés, sans clé manquante. `en-US` n’en possède que 2 et il lui en manque donc 1 768.
Pour 20 familles sur 21, les 28 langues traduites ont exactement le même nombre de lignes. La seule
famille incohérente entre ces langues est `Emulation.resx`. L’ordre des clés diffère néanmoins dans
16 familles sur 21. Enfin, toutes les langues non anglaises conservent encore des valeurs strictement
identiques à `00-Base`, entre 2 et 104 selon la langue.

| Règle | Résultat actuel | Conformité |
|---|---|---|
| Même ensemble de 1 770 clés dans les 28 langues non anglaises | 1 770 clés dans chaque langue, aucune manquante | Conforme |
| Même ensemble de 1 770 clés dans `en-US` | 2 présentes, 1 768 manquantes | **Non conforme** |
| Même ordre des clés dans les 21 familles localisées | Ordre différent dans 16 familles | **Non conforme** |
| Même nombre de lignes dans les 28 langues non anglaises | 20 familles identiques, `Emulation.resx` différent | **Non conforme** |
| Aucune traduction anglaise ni valeur invariante recopiée depuis `00-Base` | De 2 à 104 valeurs identiques par langue, à distinguer entre traductions manquantes et invariants dupliqués | **Non conforme** |

| Ressource | `00-Base` | `en-US` | 28 langues traduites | Résultat |
|---|---:|---:|---:|---|
| `About.resx` | 19 | 15 | 19 | Identique dans les 28 langues |
| `Actions.resx` | 34 | 15 | 34 | Identique dans les 28 langues |
| `Advanced.resx` | 53 | 15 | 53 | Identique dans les 28 langues |
| `Common.resx` | 27 | 15 | 27 | Identique dans les 28 langues |
| `Conversion.resx` | 102 | 15 | 91 | Identique dans les 28 langues |
| `Emulation.resx` | 940 | 15 | 831 à 844 | **Incohérent : 12 nombres de lignes différents** |
| `Errors.resx` | 39 | 15 | 39 | Identique dans les 28 langues |
| `Explorer.resx` | 169 | 15 | 166 | Identique dans les 28 langues |
| `ExplorerWarnings.resx` | 63 | 7 | 71 | Identique dans les 28 langues |
| `Formats.resx` | 120 | 15 | 35 | Identique dans les 28 langues |
| `Hardware.resx` | 63 | 15 | 63 | Identique dans les 28 langues |
| `HostTools.resx` | 27 | 15 | 27 | Identique dans les 28 langues |
| `Icons.resx` | 22 | absent | absent | Base commune uniquement |
| `Logs.resx` | 17 | 15 | 17 | Identique dans les 28 langues |
| `Menus.resx` | 40 | 15 | 40 | Identique dans les 28 langues |
| `Options.resx` | 269 | 17 | 246 | Identique dans les 28 langues |
| `Profiles.resx` | 31 | 15 | 31 | Identique dans les 28 langues |
| `Read.resx` | 56 | 15 | 56 | Identique dans les 28 langues |
| `Shell.resx` | 40 | 15 | 38 | Identique dans les 28 langues |
| `Tools.resx` | 68 | 15 | 68 | Identique dans les 28 langues |
| `Visualizer.resx` | 201 | 15 | 173 | Identique dans les 28 langues |
| `Write.resx` | 37 | 15 | 37 | Identique dans les 28 langues |

Répartition actuelle des lignes de `Emulation.resx` dans les 28 langues traduites :

| Lignes | Langues |
|---:|---|
| 831 | `nl-NL` |
| 833 | `it-IT` |
| 834 | `da-DK`, `sv-SE` |
| 835 | `nb-NO` |
| 836 | `cs-CZ`, `de-DE`, `fi-FI` |
| 837 | `hu-HU`, `ro-RO` |
| 839 | `id-ID`, `tr-TR` |
| 840 | `es-ES`, `ru-RU`, `vi-VN` |
| 841 | `pl-PL`, `pt-BR`, `pt-PT` |
| 842 | `fr-FR` |
| 843 | `he-IL` |
| 844 | `ar-SA`, `el-GR`, `ja-JP`, `ko-KR`, `th-TH`, `uk-UA`, `zh-Hans`, `zh-Hant` |

Valeurs encore strictement identiques à `00-Base` dans les 28 langues non anglaises. Chacune doit être
classée comme traduction anglaise non remplacée ou comme invariant à retirer du fichier localisé :

| Langue | Valeurs identiques |
|---|---:|
| `ar-SA` | 6 |
| `cs-CZ` | 41 |
| `da-DK` | 91 |
| `de-DE` | 74 |
| `el-GR` | 5 |
| `es-ES` | 72 |
| `fi-FI` | 22 |
| `fr-FR` | 104 |
| `he-IL` | 10 |
| `hu-HU` | 32 |
| `id-ID` | 72 |
| `it-IT` | 38 |
| `ja-JP` | 4 |
| `ko-KR` | 4 |
| `nb-NO` | 78 |
| `nl-NL` | 70 |
| `pl-PL` | 40 |
| `pt-BR` | 44 |
| `pt-PT` | 43 |
| `ro-RO` | 69 |
| `ru-RU` | 3 |
| `sv-SE` | 103 |
| `th-TH` | 3 |
| `tr-TR` | 64 |
| `uk-UA` | 2 |
| `vi-VN` | 16 |
| `zh-Hans` | 4 |
| `zh-Hant` | 4 |

## GWGUI.Emulation

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Directory.cs` | 3 584 | **Élevée**, portage extrêmement volumineux |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Disk.cs` | 1 103 | Portage |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/anodes.cs` | 1 037 | Portage |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/FastFileSystem/Directory.cs` | 975 | Portage |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Update.cs` | 953 | Portage |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Allocation.cs` | 869 | Portage |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/Pfs3/Volume.cs` | 545 | Portage |
| `src/GWGUI.Emulation/HardDisks/FileSystems/Amiga/FastFileSystem/EntryStream.cs` | 512 | Portage |

## GWGUI.Emulation.Amiga

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.Emulation.Amiga/Services/AmigaExternalHostCallbacks.cs` | 638 | Moyenne, interop et callbacks |

## GWGUI.Emulation.Atari

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs` | 513 | Normale à moyenne, catalogue de réglages |

## GWGUI.Tests

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `tests/GWGUI.Tests/Interface/VisualizerViews/VisualizerDocumentScenarios.cs` | 693 | Normale, scénarios de tests |
| `tests/GWGUI.Tests/Interface/VisualizerViews/MediaVisualizationLayoutTests.cs` | 553 | Normale, tests de disposition |

## GWGUI.VideoPresentation

| Fichier | Lignes | Sévérité |
|---|---:|---|
| `src/GWGUI.VideoPresentation/Dictionaries/EmulationVideoProcessingCatalog.cs` | 527 | Normale, catalogue déclaratif |

## Priorités

Les deux fichiers applicatifs les plus urgents à examiner sont :

1. `src/GWGUI.App/Services/DiskImages/DiskImageWorkspaceController.cs` ;
2. `src/GWGUI.App/Services/Input/GameInput/GameInputControllerReader.cs`.

`Pfs3/Directory.cs` est beaucoup plus long, mais appartient à un portage technique. Sa provenance et
sa correspondance avec l’implémentation d’origine doivent être conservées lors d’un éventuel découpage.
