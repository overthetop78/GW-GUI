# Améliorations souhaitées pour l’interface d’émulation

## But du document

Ce document reprend uniquement les demandes et les idées formulées à partir des six images de l’interface.

Il distingue les demandes validées des pistes encore à étudier. La fin du document contient l’ordre général retenu et les checklists techniques détaillées des points 1 à 8.

## 1. Écran d’émulation

### Focus de l’écran

Dans l’onglet d’émulation actif, le focus doit revenir à la fenêtre d’émulation après une action ponctuelle effectuée dans l’interface lorsque la machine est allumée ou vient d’être allumée. L’extinction ne rend pas le focus à la machine éteinte.

Cela concerne notamment :

- le chargement ou le changement d’une image de disquette ;
- l’allumage de la machine ;
- le reset logiciel ou matériel ;
- la sauvegarde d’un état ;
- le chargement d’un état ;
- le basculement entre la manette et la souris ;
- les autres commandes comparables de l’instance.

Pendant l’ouverture d’une boîte de dialogue, celle-ci conserve le focus. Une fois l’opération terminée et la boîte de dialogue fermée, le focus revient à l’écran de l’instance affichée dans l’onglet actif.

Un clic dans la zone grise autour de l’image doit également redonner le focus à l’émulation sans capturer la souris.

Les clics dans l’interface et les raccourcis de GW GUI doivent continuer à fonctionner.

### Filtres vidéo

Il faut établir une liste large des filtres existants, sans la limiter aux machines Amiga et Atari actuellement prises en charge. Cette recherche devra également servir aux futures machines ajoutées à GW GUI.

La recherche doit déterminer :

- ce qui est déjà proposé par Libretro ou par les émulateurs utilisés ;
- ce qui peut être réutilisé ou reproduit dans GW GUI ;
- ce qui devra être développé directement dans l’application.

Les collections de shaders Libretro peuvent servir de références pour reproduire des effets similaires dans GW GUI. Avant de reprendre directement le code d’un shader, il faudra vérifier sa licence. La manière de l’intégrer ou de reproduire son effet reste à étudier.

Les effets évoqués comprennent notamment les filtres CRT, les scanlines, les rendus LCD et le moiré horizontal et vertical. La liste exacte doit être établie pendant la recherche.

Les choix de signal vidéo déjà fournis par les émulateurs, tels que RGB, composite, S-Video, RF, PAL ou NTSC selon le moteur, restent des options de l’émulateur. Ils ne doivent pas être recréés ni dupliqués comme filtres propres à GW GUI. La possibilité de proposer plus tard un effet visuel inspiré d’un type de signal ne sera étudiée que comme un effet distinct et explicitement sélectionné.

Références de recherche :

- [Documentation des shaders Libretro](https://docs.libretro.com/guides/shaders/)
- [Collection officielle Slang](https://github.com/libretro/slang-shaders)
- [Collection Common Shaders](https://github.com/libretro/common-shaders)

### Organisation des filtres

Les filtres sont développés une seule fois et sont communs aux machines compatibles. Le filtre sélectionné et ses réglages sont cependant enregistrés séparément dans la configuration de chaque machine et utilisés par chacune de ses instances.

L’onglet **Vidéo** de la configuration Amiga ou Atari doit proposer :

- des présélections avec des réglages de base déjà choisis ;
- la possibilité de modifier les réglages ;
- l’enregistrement immédiat de chaque changement dans la configuration de la machine ;
- l’application immédiate du changement à l’instance correspondante lorsqu’elle fonctionne ;
- l’utilisation des réglages enregistrés au prochain démarrage si aucune instance ne fonctionne.

Les filtres et leurs réglages ne doivent pas être ajoutés ailleurs dans l’interface.

L’interface doit permettre de choisir une fonctionnalité dans une liste, puis d’afficher son panneau de configuration. Chaque fonctionnalité peut y être activée ou désactivée et, lorsqu’elle est active, ses réglages deviennent accessibles.

Il n’est pas nécessaire de transformer toute la page en groupes rigides. Les compatibilités restent gérées par familles logiques :

- les effets compatibles peuvent être combinés, par exemple un rendu CRT avec des scanlines ;
- lorsqu’une fonctionnalité activée est incompatible avec une ou plusieurs fonctionnalités déjà actives, l’application propose d’abord de les désactiver ;
- après confirmation, l’activation de la nouvelle fonctionnalité désactive automatiquement les fonctionnalités incompatibles ;
- sans confirmation, la combinaison actuelle reste inchangée.

Les réglages généraux de l’image — luminosité, contraste, gamma, saturation et netteté — restent toujours visibles indépendamment de la fonctionnalité sélectionnée. Leur valeur neutre est 0. Les réglages autres que le gamma utilisent une plage de -10 à +10. La représentation et la plage du gamma restent à définir en fonction de l’implémentation retenue.

La recherche devra établir la liste des groupes ainsi que les effets compatibles ou incompatibles entre eux.

### Séparation des réglages internes et externes à l’émulateur

Dans les onglets **Vidéo** et **Audio**, les réglages doivent être séparés visuellement entre :

- les options propres à l’émulateur ;
- les traitements réalisés par GW GUI sans demander de changement à l’émulateur.

Les traitements réalisés par GW GUI doivent pouvoir être modifiés en direct. La modification en direct des options internes de l’émulateur sera étudiée plus tard et ne fait pas partie de la demande actuelle.

Les intitulés des deux cadres ne sont pas encore choisis. Les formulations **Réglages de l’émulateur**, **Traitement vidéo par GW GUI** et **Traitement audio par GW GUI** ne sont pas retenues comme libellés définitifs.

### Idée future : habillages d’écran

Cette partie est une idée à conserver pour plus tard. Elle ne doit pas être réalisée maintenant.

Il faudra étudier la possibilité d’afficher un habillage en plein écran :

- une télévision ou un écran d’ordinateur pour les ordinateurs et consoles de salon ;
- le corps de la console pour une console portable.

Sans habillage, le plein écran classique actuel reste disponible.

Le choix de l’habillage est enregistré dans la configuration de la machine. Plusieurs habillages peuvent être proposés pour une même machine, notamment lorsque le matériel a connu plusieurs modèles ou plusieurs couleurs, par exemple Lynx et Lynx II, ou différentes couleurs de Game Boy Color.

Pour une console portable, les habillages représentent uniquement cette console et ses variantes. Il n’est pas obligatoire que tout le boîtier reste visible : l’habillage peut ne montrer que le contour de l’écran d’une Game Boy, d’une Lynx, d’une Game Gear ou d’une autre console portable. Une partie extérieure du boîtier peut donc être coupée, mais l’écran de la console doit rester entièrement visible, correctement placé et sans déformation.

Au départ, les habillages sont décoratifs. Une évolution ultérieure pourra éventuellement afficher les boutons pressés, les voyants allumés ou d’autres éléments animés.

L’utilisation des habillages en mode fenêtré reste à étudier. Les images nécessaires pourront être recherchées et rendues transparentes au besoin lorsque cette fonction sera effectivement abordée.

## 2. Identification de la machine modifiée

Il faut rendre plus évident ce que l’utilisateur est en train de modifier dans les options d’émulation.

### Machines possédant déjà une configuration

Dans la liste des machines, celles qui possèdent déjà une configuration doivent être distinguées par un fond d’une nuance de gris clair et un texte vert forêt en gras. Une icône autre qu’une coche peut également accompagner cette présentation ; son apparence reste à choisir.

Cette indication doit être visible :

- dans la liste déroulante lorsqu’elle est ouverte ;
- sur la machine sélectionnée lorsque la liste est fermée.

### Barre de titre

Lorsque l’utilisateur modifie une machine dans la partie **Émulation**, la barre de titre de la fenêtre doit indiquer la marque et la machine concernées. Cette indication reste visible dans tous les sous-onglets, contrairement au champ **Modèle**, qui n’est visible que dans l’onglet **Général** de la marque.

Cette information ne doit pas être ajoutée dans le libellé de l’onglet.

Le format retenu est **Options — Amiga : Amiga 500**, avec le format correspondant pour Atari, par exemple **Options — Atari : Atari ST**.

Dans l’onglet général **Configuration**, où aucune machine particulière n’est en cours de modification, la barre de titre reste simplement **Options**. Aucun titre **Options — Émulation — Configurations** ne doit être ajouté.

### Chargement d’une configuration existante

Lorsqu’une machine possédant déjà une configuration est sélectionnée, ses réglages enregistrés doivent être chargés dans l’onglet correspondant à cette machine, côté Amiga ou Atari.

### Création et enregistrement des changements

Le comportement dépend de l’existence de la configuration :

- si la configuration de la machine n’existe pas encore, chaque changement effectué par l’utilisateur est conservé dans un brouillon en mémoire pendant toute l’exécution de l’application, sans écrire de fichier tant que l’utilisateur ne clique pas sur **Créer** ;
- chaque machine sans configuration possède son propre brouillon en mémoire et le conserve lorsque l’utilisateur affiche une autre machine ou ferme puis rouvre la fenêtre **Paramètres** ;
- après la création, le bouton **Créer** disparaît immédiatement ;
- si la configuration de cette machine est ensuite supprimée depuis la liste des configurations, le bouton **Créer** réapparaît lorsque cette machine est sélectionnée ;
- aucune configuration existante n’affiche de bouton **Modifier**, puisque chaque changement est enregistré automatiquement ;
- aucun enregistrement automatique ne demande de confirmation.

Tous les champs modifiables d’une configuration existante doivent être couverts par l’enregistrement automatique. Il faudra vérifier chaque champ de l’ensemble des onglets Amiga et Atari.

Le moment de l’enregistrement dépend du contrôle :

- liste, case, bouton de choix, curseur ou autre sélecteur : dès que la valeur change ;
- champ de saisie : lorsque le champ perd le focus ;
- association de clavier, souris ou manette : dès que l’association est créée, remplacée, supprimée ou restaurée ;
- action modifiant plusieurs valeurs : toutes les valeurs concernées sont enregistrées à la fin de l’action.

Il n’est pas prévu de créer plusieurs configurations nommées pour une même machine ni d’ajouter un nom de profil.

## 3. Tableau des configurations

La présentation textuelle actuelle doit être remplacée par un tableau plus joli et plus facile à lire.

### Filtre par marque

Une liste déroulante permet de choisir la marque dont les configurations doivent être affichées. Elle contient uniquement les marques pour lesquelles au moins une configuration existe.

Si aucune marque ne possède de configuration, la liste déroulante est vide, le tableau est vide et rien n’est sélectionné.

Si la dernière configuration de la marque affichée est supprimée, aucune autre marque ne doit être sélectionnée automatiquement. La liste déroulante revient à un état sans sélection et le tableau devient vide jusqu’à ce que l’utilisateur choisisse lui-même une autre marque.

### Absence de sélection dans le tableau

Le tableau ne possède aucun mécanisme de sélection de ligne. Un simple clic sur une ligne ne la sélectionne pas et ne charge rien.

Les seules interactions disponibles sur une configuration sont :

- un double-clic sur sa ligne pour la modifier ;
- le bouton avec une icône de crayon pour la modifier ;
- le bouton avec une icône de poubelle pour demander sa suppression.

Cliquer ailleurs ne conserve ou ne crée aucune sélection.

### Informations affichées

Les configurations sont classées par ordre alphabétique du nom de la machine. Un système de tri supplémentaire n’est pas nécessaire, car le nombre de configurations par marque restera limité.

Une machine ne possédant qu’une seule configuration, son nom suffit à identifier la ligne. Aucun identifiant technique ne doit être affiché.

Les colonnes retenues sont :

- **Machine**, placée en premier ;
- **CPU** ;
- **RAM totale** ;
- **Lecteurs** ;
- **Périphériques** ;
- **Actions**.

Le moteur d’affichage vidéo, par exemple Direct3D 11, ainsi que les informations audio ne doivent pas être ajoutés au tableau. Il ne faut pas ajouter d’informations inutiles simplement pour remplir le tableau.

### Lecteurs

La colonne **Lecteurs** affiche une icône pour chaque lecteur configuré. Deux lecteurs sont représentés par deux icônes et non par une seule icône accompagnée du nombre deux.

Les icônes doivent distinguer les différents types de lecteurs, par exemple disquette, disque dur, CD, cassette ou cartouche selon les machines prises en charge.

### Périphériques

La colonne **Périphériques** utilise des icônes pour indiquer les périphériques configurés, notamment le clavier, la souris, les joysticks et les manettes.

Le nombre de joysticks ou de manettes configurés doit être visible en affichant le nombre correspondant d’icônes.

### Modification

Le bouton avec une icône de crayon et le double-clic sur la ligne déclenchent exactement la même action :

- ouvrir l’onglet **Général** de la marque correspondante ;
- afficher la machine concernée ;
- charger sa configuration complète afin de remplir les champs de tous les onglets de cette marque.

Cette page ne permet pas de lancer la machine.

### Suppression

Le bouton avec une icône de poubelle ouvre une boîte de dialogue Oui/Non avant toute suppression.

La boîte de dialogue affiche seulement le minimum permettant d’identifier sans ambiguïté la configuration supprimée : la marque et la machine. Elle ne doit pas devenir une fiche détaillée et ne doit afficher ni CPU, ni RAM, ni ROM, ni lecteurs, ni périphériques, ni réglages vidéo ou audio, ni identifiant technique.

Si la configuration supprimée était chargée dans l’éditeur de sa marque :

- les données correspondantes sont retirées de la mémoire ;
- les champs reviennent à leurs valeurs de base ;
- le bouton **Créer** réapparaît puisque cette machine ne possède plus de configuration.

Lorsque l’utilisateur ouvre l’onglet d’une marque, l’application relit les configurations existantes. La présence du bouton **Créer** dépend donc de l’existence réelle de la configuration de la machine affichée.

### Points restant à décider

- la présentation graphique définitive du tableau et de ses icônes.

## 4. Aides sur les champs

Une petite icône **(i)** doit être placée immédiatement après le nom de chaque champ dont le nom seul ne permet pas réellement de comprendre son rôle, ses choix ou leurs conséquences.

Cette aide concerne les champs, pas les boutons ni les titres de groupes.

L’icône doit toujours être visible. Sa taille normale constitue sa zone cliquable ; aucune zone invisible plus grande n’est demandée. L’infobulle au survol confirme que le pointeur se trouve bien sur l’icône.

### Aide rapide au survol

Lorsque la souris survole l’icône, une explication rapide apparaît.

Cette explication :

- tient sur une seule ligne ;
- indique simplement à quoi sert le champ ;
- disparaît lorsque le survol se termine ;
- ne contient pas de texte long ni de défilement.

### Aide détaillée au clic

Un clic sur l’icône ouvre une aide plus détaillée avec une présentation de type post-it.

Cette aide explique simplement ce que fait le champ, les choix disponibles et leurs différences utiles. Le texte reste court, clair et concis, sans longs paragraphes ni mise en forme de documentation. Un défilement n’est utilisé que si le contenu concis ne tient réellement pas dans le post-it.

Une fois le post-it ouvert, n’importe quelle touche du clavier ou un nouveau clic le ferme.

### Présentation validée du post-it

- largeur maximale : 380 px ;
- hauteur maximale : 240 px ;
- espacement entre l’icône et le post-it : 8 px ;
- marge intérieure du post-it : 12 px ;
- placement normal : à droite de l’icône, centré verticalement sur celle-ci ;
- repli : à gauche de l’icône lorsque l’espace disponible à droite est insuffisant ;
- arrière-plan : ressource de thème CardBrush ;
- bordure : ressource de thème BorderBrush ;
- texte : ressource de thème TextBrush.

### Périmètre et traductions

Le système doit être utilisé pour les champs concernés dans les différents onglets Amiga et Atari.

Pour chaque champ portant une icône **(i)**, deux textes distincts — l’aide courte au survol et l’aide concise au clic — doivent être ajoutés aux ressources et traduits dans toutes les langues prises en charge par GW GUI. Aucun de ces textes ne doit être écrit directement dans le code.

### Inventaire des champs visibles

Les boutons, les titres de groupes, les tableaux d’associations et les valeurs de résumé ne sont pas des champs de réglage et sont exclus. Les champs communs aux deux modules sont regroupés lorsqu’ils utilisent le même libellé et la même ressource. Le sélecteur de périphérique physique reste recensé mais ne recevra pas d’aide, car sa suppression est demandée au point 6.

| Origine | Onglet | Champ visible | Identifiant(s) | Ressource du libellé | Aide contextuelle | Clé d’aide courte | Texte court | Clé d’aide concise | Texte concis |
|---|---|---|---|---|---|---|---|---|---|
| Amiga, Atari | Audio | Activer le son | `AmigaSettingsConstants.AudioEnabled, AtariSettingsConstants.AudioEnabled` | `Emulation.Audio.Enabled` | Non | — | — | — | — |
| Amiga, Atari | Audio | Bruit des lecteurs | `AmigaSettingsDescriptionFunctionsConstants.OptionFloppySound, AtariVideoAudioSettingsConstants.FloppySoundVolumeOption` | `Emulation.Audio.Floppy.Sound` | Non | — | — | — | — |
| Atari | Audio | Bruit des lecteurs de disquettes | `AtariVideoAudioSettingsConstants.FloppySoundOption` | `Emulation.Audio.Floppy.Enabled` | Non | — | — | — | — |
| Amiga | Audio | Couper le son des lecteurs vides | `AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundEmptyMute` | `Emulation.Audio.Floppy.MuteEmpty` | Oui | `Emulation.Help.Audio.Floppy.MuteEmpty.Short` | Silence empty floppy drives | `Emulation.Help.Audio.Floppy.MuteEmpty.Detailed` | Silences the emulated drive sound while no disk is inserted. Disable this option to keep the idle mechanical noise. |
| Amiga | Audio | Filtre Amiga | `AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilter` | `Emulation.Audio.Filter` | Oui | `Emulation.Help.Audio.Filter.Short` | Select analog audio filtering | `Emulation.Help.Audio.Filter.Detailed` | Selects how the original analog audio filter is reproduced. Emulated follows the hardware behavior. The other choices force the filter on or off. |
| Atari | Audio | Filtre audio polarisé | `AtariVideoAudioSettingsConstants.PolarizedFilterOption` | `Emulation.Audio.PolarizedFilter` | Oui | `Emulation.Help.Audio.PolarizedFilter.Short` | Enable the polarized audio filter | `Emulation.Help.Audio.PolarizedFilter.Detailed` | Applies the polarized filter to the generated audio signal. Enable it to reproduce the filtered sound. Disable it to keep the unfiltered signal. |
| Amiga | Audio | Interpolation | `AmigaSettingsDescriptionFunctionsConstants.OptionSoundInterpol` | `Emulation.Audio.Interpolation` | Oui | `Emulation.Help.Audio.Interpolation.Short` | Select audio interpolation | `Emulation.Help.Audio.Interpolation.Detailed` | Selects how audio samples are calculated between generated sample points. Higher-quality methods produce smoother sound but require more processing. |
| Amiga | Audio | Latence | `AmigaSettingsConstants.AudioLatency` | `Emulation.Audio.LatencyLabel` | Oui | `Emulation.Help.Audio.Latency.Short` | Set audio latency | `Emulation.Help.Audio.Latency.Detailed` | Sets the duration of the audio buffer. A shorter buffer reduces delay but can cause crackling. A longer buffer improves stability but increases delay. |
| Atari | Audio | Latence | `AtariVideoAudioSettingsConstants.AudioLatencyOption` | `Emulation.Audio.Latency` | Oui | `Emulation.Help.Audio.Latency.Short` | Set the audio buffer delay | `Emulation.Help.Audio.Latency.Detailed` | Chooses the audio buffer duration. Lower values reduce delay but may crackle; higher values improve stability with more latency. |
| Amiga | Audio | Périphérique | `AmigaSettingsConstants.AudioOutput` | `Emulation.Audio.Device` | Non | — | — | — | — |
| Atari | Audio | POKEY stéréo | `AtariEightBitSettingsConstants.PokeyStereoOptionKey` | `Emulation.Atari.Audio.PokeyStereo` | Oui | `Emulation.Help.Audio.PokeyStereo.Short` | Enable dual POKEY stereo | `Emulation.Help.Audio.PokeyStereo.Detailed` | Adds a second emulated POKEY chip to produce stereo sound. Enable this option only for software that supports dual POKEY audio. |
| Amiga | Audio | Séparation stéréo | `AmigaSettingsConstants.AudioStereoSeparation` | `Emulation.Audio.StereoSeparation` | Oui | `Emulation.Help.Audio.StereoSeparation.Short` | Set stereo separation | `Emulation.Help.Audio.StereoSeparation.Detailed` | Sets the separation between the left and right channels. A lower value mixes the channels together. A higher value keeps them farther apart. |
| Atari | Audio | Sortie audio | `AtariVideoAudioSettingsConstants.AudioOutputOption` | `Emulation.Audio.Output` | Non | — | — | — | — |
| Amiga | Audio | Type de bruit des lecteurs | `AmigaSettingsDescriptionFunctionsConstants.OptionFloppySoundType` | `Emulation.Audio.Floppy.SoundType` | Oui | `Emulation.Help.Audio.Floppy.SoundType.Short` | Select the floppy drive sound set | `Emulation.Help.Audio.Floppy.SoundType.Detailed` | Selects the sample set used for the mechanical sounds of emulated floppy drives. It does not change drive behavior or disk data. |
| Amiga | Audio | Type de filtre | `AmigaSettingsDescriptionFunctionsConstants.OptionSoundFilterType` | `Emulation.Audio.FilterType` | Oui | `Emulation.Help.Audio.FilterType.Short` | Select the audio filter model | `Emulation.Help.Audio.FilterType.Detailed` | Selects the hardware filter response to emulate. Auto follows the selected machine model. A specific choice always uses that filter response. |
| Atari | Audio | Volume | `AtariVideoAudioSettingsConstants.AudioVolumeOption` | `Explorer.Volume` | Non | — | — | — | — |
| Amiga | Audio | Volume audio du CD | `AmigaSettingsDescriptionFunctionsConstants.OptionSoundVolumeCd` | `Emulation.Audio.Cd.Volume` | Non | — | — | — | — |
| Amiga | Manettes | Adaptateur parallèle pour quatre joysticks | `AmigaSettingsConstants.ParallelJoystickAdapter` | `Emulation.Amiga.Controller.ParallelAdapter` | Oui | `Emulation.Help.Controller.ParallelAdapter.Short` | Enable two additional joystick ports | `Emulation.Help.Controller.ParallelAdapter.Detailed` | Emulates a parallel-port adapter that adds two joystick ports. Enable it for software that supports four simultaneous players. |
| Amiga | Manettes | Cadence du turbo | `AmigaSettingsDescriptionFunctionsConstants.OptionTurboPulse` | `Emulation.Controller.Turbo.Pulse` | Oui | `Emulation.Help.Controller.TurboPulse.Short` | Set the turbo pulse interval | `Emulation.Help.Controller.TurboPulse.Detailed` | Sets the time between automatic button presses produced by turbo. A shorter interval produces faster repeated firing. |
| Atari | Manettes | Compatibilité des manettes | `AtariEightBitSettingsConstants.ControllerCompatibilityOptionKey` | `Emulation.Atari.Controller.Compatibility` | Oui | `Emulation.Help.Controller.Compatibility.Short` | Select a controller compatibility mode | `Emulation.Help.Controller.Compatibility.Detailed` | Selects a special controller mapping, such as dual-stick control, swapped ports, or Joy2B+. Choose None to keep the standard port mapping. |
| Application | Manettes | Périphérique physique | `PhysicalDevice` | `Emulation.Controller.Device` | Non | — | — | — | — |
| Atari | Manettes | Sensibilité analogique | `AtariEightBitSettingsConstants.AnalogSensitivityOptionKey` | `Emulation.Atari.Controller.AnalogSensitivity` | Oui | `Emulation.Help.Controller.AnalogSensitivity.Short` | Set analog input sensitivity | `Emulation.Help.Controller.AnalogSensitivity.Detailed` | Sets how strongly an analog input responds to movement. A higher value produces a larger response from the same physical movement. |
| Atari | Manettes | Sensibilité numérique | `AtariEightBitSettingsConstants.DigitalSensitivityOptionKey` | `Emulation.Atari.Controller.DigitalSensitivity` | Oui | `Emulation.Help.Controller.DigitalSensitivity.Short` | Set digital input sensitivity | `Emulation.Help.Controller.DigitalSensitivity.Detailed` | Sets how quickly a digital direction reaches full movement. A higher value makes direction changes reach their maximum more quickly. |
| Atari | Manettes | Tir automatique | `AtariEightBitSettingsConstants.AutofireOptionKey` | `Emulation.Atari.Controller.Autofire` | Oui | `Emulation.Help.Controller.Autofire.Short` | Select the autofire mode | `Emulation.Help.Controller.Autofire.Detailed` | Disabled turns autofire off. On button repeats fire while the assigned button is held. Always repeats fire continuously. |
| Application | Manettes | Type de manette | `ControllerType` | `Emulation.Controller.Type` | Non | — | — | — | — |
| Atari | Manettes | Vitesse des paddles | `AtariEightBitSettingsConstants.PaddleMovementSpeedOptionKey` | `Emulation.Atari.Controller.PaddleSpeed` | Oui | `Emulation.Help.Controller.PaddleSpeed.Short` | Set paddle movement speed | `Emulation.Help.Controller.PaddleSpeed.Detailed` | Sets how quickly digital input moves an emulated paddle. A lower value moves it more slowly. A higher value moves it more quickly. |
| Amiga, Atari | CPU | FPU | `AmigaSettingsDescriptionFunctionsConstants.OptionFpuModel, AtariSettingsConstants.Fpu` | `Emulation.Fpu.Model` | Oui | `Emulation.Help.Cpu.FpuModel.Short` | Select the FPU | `Emulation.Help.Cpu.FpuModel.Detailed` | Selects the floating-point unit used for floating-point instructions. Choose None to disable the FPU. Available models depend on the selected CPU. |
| Amiga, Atari | CPU | Modèle de CPU | `AmigaSettingsDescriptionFunctionsConstants.OptionCpuModel, AtariSettingsConstants.Cpu` | `Emulation.Cpu.Model` | Non | — | — | — | — |
| Amiga, Atari | CPU | Précision | `AmigaSettingsDescriptionFunctionsConstants.OptionCpuCompatibility, AtariSettingsConstants.CpuPrecision` | `Emulation.Cpu.Precision` | Oui | `Emulation.Help.Cpu.Precision.Short` | Select CPU emulation accuracy | `Emulation.Help.Cpu.Precision.Detailed` | Selects how closely CPU timing follows the original hardware. More accurate modes can improve compatibility but may require more processing. |
| Amiga, Atari | CPU | Vitesse d’origine | `AmigaSettingsConstants.CpuOriginalSpeed, AtariSettingsConstants.CpuOriginalFrequency` | `Emulation.Cpu.SpeedOriginal` | Non | — | — | — | — |
| Amiga, Atari | CPU | Vitesse du CPU | `AmigaSettingsConstants.CpuSpeed, AtariSettingsConstants.CpuFrequency` | `Emulation.Cpu.Speed` | Oui | `Emulation.Help.Cpu.Speed.Short` | Set the emulated CPU speed | `Emulation.Help.Cpu.Speed.Detailed` | Sets the processor frequency used by the emulated machine. The original speed preserves hardware timing. Higher values accelerate CPU-limited software. |
| Atari | Général | Disques durs | `AtariSettingsConstants.HardDiskFolder` | `Emulation.Storage.HardDisk.List` | Non | — | — | — | — |
| Application | Général | Modèle | `ModelSelector` | `Emulation.Model` | Non | — | — | — | — |
| Amiga | Souris | Joysticks analogiques contrôlant la souris | `AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouse` | `Emulation.Mouse.Analog` | Oui | `Emulation.Help.Mouse.Analog.Short` | Use analog sticks to control the mouse | `Emulation.Help.Mouse.Analog.Detailed` | Allows analog joystick axes to move the emulated mouse pointer. Enable it when a physical analog stick should control the pointer. |
| Amiga, Atari | Souris | Vitesse de la souris | `AmigaSettingsDescriptionFunctionsConstants.OptionMouseSpeed, AtariMouseSettingsConstants.SpeedOptionKey` | `Emulation.Mouse.Speed` | Non | — | — | — | — |
| Amiga | Souris | Vitesse de la souris analogique | `AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeed, AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseSpeedRight` | `Emulation.Mouse.AnalogSpeed` | Oui | `Emulation.Help.Mouse.AnalogSpeed.Short` | Set analog mouse speed | `Emulation.Help.Mouse.AnalogSpeed.Detailed` | Sets pointer speed when an analog stick controls the emulated mouse. A higher value moves the pointer farther for the same stick movement. |
| Amiga | Souris | Zone morte des joysticks analogiques | `AmigaSettingsDescriptionFunctionsConstants.OptionAnalogmouseDeadzone` | `Emulation.Mouse.AnalogDeadzone` | Oui | `Emulation.Help.Mouse.AnalogDeadzone.Short` | Set the analog mouse dead zone | `Emulation.Help.Mouse.AnalogDeadzone.Detailed` | Ignores small analog-stick movements around the center while controlling the mouse. Increase the value if the pointer moves while the stick is released. |
| Atari | RAM | Banque fantôme Axlon $0F | `AtariEightBitSettingsConstants.AxlonShadowOptionKey` | `Emulation.Atari.Memory.AxlonShadow` | Oui | `Emulation.Help.Memory.AxlonShadow.Short` | Enable the Axlon $0F shadow bank | `Emulation.Help.Memory.AxlonShadow.Detailed` | Mirrors the Axlon control bank at address $0F. Enable it only for software or memory expansions that use this additional address. |
| Atari | RAM | Extension mémoire Axlon | `AtariEightBitSettingsConstants.AxlonMemoryOptionKey` | `Emulation.Atari.Memory.Axlon` | Oui | `Emulation.Help.Memory.Axlon.Short` | Set Axlon expansion memory | `Emulation.Help.Memory.Axlon.Detailed` | Sets the amount of bank-switched Axlon memory. Disabled removes the expansion. The other choices provide the selected memory capacity. |
| Atari | RAM | Extension mémoire Mosaic | `AtariEightBitSettingsConstants.MosaicMemoryOptionKey` | `Emulation.Atari.Memory.Mosaic` | Oui | `Emulation.Help.Memory.Mosaic.Short` | Set Mosaic expansion memory | `Emulation.Help.Memory.Mosaic.Detailed` | Sets the amount of bank-switched Mosaic memory. Disabled removes the expansion. The other choices provide the selected memory capacity. |
| Atari | RAM | Extensions mémoire | `AtariSettingsConstants.AlternateMemory` | `Emulation.Memory.Extensions` | Oui | `Emulation.Help.Memory.Extensions.Short` | Set additional expansion memory | `Emulation.Help.Memory.Extensions.Detailed` | Sets the amount of additional expansion memory supported by the selected machine. Choose None to use only the configured main memory. |
| Amiga | RAM | Fast RAM | `AmigaSettingsDescriptionFunctionsConstants.OptionFastmemSize` | `Emulation.Memory.Fast` | Oui | `Emulation.Help.Memory.Fast.Short` | Set Fast RAM size | `Emulation.Help.Memory.Fast.Detailed` | Sets the amount of Fast RAM available directly to the CPU. Additional Fast RAM can help compatible software but changes the emulated hardware configuration. |
| Atari | RAM | MapRAM | `AtariEightBitSettingsConstants.MapRamOptionKey` | `Emulation.Atari.Memory.MapRam` | Oui | `Emulation.Help.Memory.MapRam.Short` | Enable MapRAM | `Emulation.Help.Memory.MapRam.Detailed` | Allows compatible software to map writable RAM into the system ROM address area. This option is available only on machine models that support MapRAM. |
| Amiga, Atari | RAM | Mémoire principale | `AmigaSettingsDescriptionFunctionsConstants.OptionChipmemSize, AtariConfigurationOptionConstants.MainMemory` | `Emulation.Memory.Main` | Non | — | — | — | — |
| Amiga | RAM | RAM Zorro III | `AmigaSettingsDescriptionFunctionsConstants.OptionZ3memSize` | `Emulation.Memory.Z3` | Oui | `Emulation.Help.Memory.Z3.Short` | Set Zorro III RAM size | `Emulation.Help.Memory.Z3.Detailed` | Sets the amount of 32-bit Fast RAM connected through the Zorro III bus. Use it only with compatible 32-bit machines and software. |
| Amiga | RAM | Slow RAM | `AmigaSettingsDescriptionFunctionsConstants.OptionBogomemSize` | `Emulation.Memory.Slow` | Oui | `Emulation.Help.Memory.Slow.Short` | Set Slow RAM size | `Emulation.Help.Memory.Slow.Detailed` | Sets the amount of Slow RAM in the trapdoor expansion area. This memory is slower than Fast RAM and uses a different hardware address range. |
| Amiga | ROM | Clé ROM | `AmigaSettingsConstants.RomKeyPath` | `Emulation.Firmware.Rom.Key` | Oui | `Emulation.Help.Firmware.RomKey.Short` | Select the ROM decryption key | `Emulation.Help.Firmware.RomKey.Detailed` | Selects the key file used to decrypt a licensed encrypted ROM image. Leave this field empty when the selected ROM is not encrypted. |
| Atari | ROM | Démarrage rapide | `AtariSettingsDescriptionFunctionsConstants.HatariFastboot` | `Emulation.Atari.FastBoot` | Oui | `Emulation.Help.Firmware.FastBoot.Short` | Enable fast startup | `Emulation.Help.Firmware.FastBoot.Detailed` | Skips selected hardware initialization delays to shorten startup. Disable this option when software requires the original startup sequence. |
| Amiga | ROM | Kickstart | `AmigaSettingsConstants.KickstartPath` | `Emulation.Firmware.Rom.Kickstart` | Non | — | — | — | — |
| Amiga | ROM | ROM étendue | `AmigaSettingsConstants.ExtendedRomPath` | `Emulation.Firmware.Rom.Extended` | Oui | `Emulation.Help.Firmware.ExtendedRom.Short` | Select an extended ROM | `Emulation.Help.Firmware.ExtendedRom.Detailed` | Selects the secondary firmware ROM required by some machine models. Leave this field empty when the selected model does not use an extended ROM. |
| Atari | ROM | ROM système | `AtariSettingsConstants.SystemFirmware` | `Emulation.Firmware.Rom.System` | Non | — | — | — | — |
| Atari | Stockage | Accélération SIO | `AtariEightBitSettingsConstants.SioAccelerationOptionKey` | `Emulation.Atari.Storage.SioAcceleration` | Oui | `Emulation.Help.Storage.SioAcceleration.Short` | Enable accelerated SIO transfers | `Emulation.Help.Storage.SioAcceleration.Detailed` | Speeds up compatible transfers through the serial input/output bus. Disable this option when software depends on the original transfer timing. |
| Atari | Stockage | Afficher l’activité des lecteurs sur l’écran de l’émulateur | `AtariMachineOptionConstants.DriveActivity, AtariEightBitSettingsConstants.ShowActivityOptionKey` | `Emulation.Storage.ActivityOsd` | Non | — | — | — | — |
| Atari | Stockage | Afficher la vitesse d’émulation à l’écran | `AtariEightBitSettingsConstants.ShowSpeedOptionKey` | `Emulation.Atari.Storage.SpeedOsd` | Non | — | — | — | — |
| Atari | Stockage | Afficher le compteur secteur/bloc | `AtariEightBitSettingsConstants.ShowSectorOptionKey` | `Emulation.Atari.Storage.SectorOsd` | Non | — | — | — | — |
| Atari | Stockage | Démarrer depuis la cassette | `AtariEightBitSettingsConstants.CassetteBootOptionKey` | `Emulation.Atari.Storage.CassetteBoot` | Oui | `Emulation.Help.Storage.CassetteBoot.Short` | Enable cassette startup | `Emulation.Help.Storage.CassetteBoot.Detailed` | Makes the emulated machine try to start from the attached cassette image. Disable this option when starting from another device. |
| Atari | Stockage | Horloge temps réel R-Time 8 | `AtariEightBitSettingsConstants.RealTimeClockOptionKey` | `Emulation.Atari.Storage.RealTimeClock` | Oui | `Emulation.Help.Storage.RealTimeClock.Short` | Enable the R-Time 8 clock | `Emulation.Help.Storage.RealTimeClock.Detailed` | Emulates an R-Time 8 real-time clock so compatible software can read the current date and time. |
| Atari | Stockage | Périphérique d’impression P: | `AtariEightBitSettingsConstants.PrinterDeviceOptionKey` | `Emulation.Atari.Storage.PrinterDevice` | Oui | `Emulation.Help.Storage.PrinterDevice.Short` | Enable the P: printer device | `Emulation.Help.Storage.PrinterDevice.Detailed` | Makes the emulated P: printer device available to software. Disable this option when printer-device emulation is not needed. |
| Atari | Stockage | Périphérique série R: | `AtariEightBitSettingsConstants.SerialDeviceOptionKey` | `Emulation.Atari.Storage.SerialDevice` | Oui | `Emulation.Help.Storage.SerialDevice.Short` | Enable the R: serial device | `Emulation.Help.Storage.SerialDevice.Detailed` | Makes the emulated R: serial device available to software. Disable this option when serial-device emulation is not needed. |
| Atari | Vidéo | Artéfacts haute résolution | `AtariEightBitSettingsConstants.ArtifactingModeOptionKey` | `Emulation.Atari.Video.Artifacting` | Oui | `Emulation.Help.Video.Artifacting.Short` | Select high-resolution color artifacting | `Emulation.Help.Video.Artifacting.Detailed` | Selects how high-resolution patterns produce composite-video colors. None disables artifact colors. The other modes reproduce different palettes or chip behavior. |
| Amiga | Vidéo | Blitter | `AmigaSettingsDescriptionFunctionsConstants.OptionImmediateBlits` | `Emulation.State.ImmediateBlits` | Oui | `Emulation.Help.Video.ImmediateBlits.Short` | Select blitter timing | `Emulation.Help.Video.ImmediateBlits.Detailed` | Selects whether blitter operations finish immediately or follow emulated hardware timing. Immediate mode is faster but less timing-accurate. |
| Amiga | Vidéo | Changement de fréquence | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoAllowHzChange` | `Emulation.Video.HzChange` | Oui | `Emulation.Help.Video.HzChange.Short` | Allow output refresh-rate changes | `Emulation.Help.Video.HzChange.Detailed` | Allows the output refresh rate to follow changes in the emulated video mode. Locked keeps the current output refresh rate. |
| Amiga | Vidéo | Collisions | `AmigaSettingsDescriptionFunctionsConstants.OptionCollisionLevel` | `Emulation.Video.Collision.Level` | Oui | `Emulation.Help.Video.CollisionLevel.Short` | Select collision detection detail | `Emulation.Help.Video.CollisionLevel.Detailed` | Selects which sprite and playfield collisions are calculated. More complete detection improves compatibility but requires more processing. |
| Atari | Vidéo | Contraste | `AtariEightBitSettingsConstants.ColorContrastOptionKey` | `Emulation.Atari.Video.Contrast` | Non | — | — | — | — |
| Amiga | Vidéo | Corriger le scintillement | `AmigaSettingsDescriptionFunctionsConstants.OptionGfxFlickerfixer` | `Emulation.Video.FlickerFixer` | Oui | `Emulation.Help.Video.FlickerFixer.Short` | Reduce interlaced display flicker | `Emulation.Help.Video.FlickerFixer.Detailed` | Reduces flicker in interlaced video output. Enable it for a steadier image. Disable it to preserve the original interlaced display behavior. |
| Amiga, Atari | Vidéo | Format d’image | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoAspect, AtariVideoAudioSettingsConstants.AspectRatioOption` | `Emulation.Video.AspectRatio` | Oui | `Emulation.Help.Video.AspectRatio.Short` | Select the displayed aspect ratio | `Emulation.Help.Video.AspectRatio.Detailed` | Selects how the emulated image is scaled horizontally and vertically. Auto follows the emulated output. A fixed choice forces that display shape. |
| Amiga, Atari | Vidéo | Gamma | `AmigaSettingsDescriptionFunctionsConstants.OptionGfxGamma, AtariEightBitSettingsConstants.ColorGammaOptionKey` | `Emulation.Video.Gamma` | Oui | `Emulation.Help.Video.Gamma.Short` | Adjust image gamma | `Emulation.Help.Video.Gamma.Detailed` | Changes the brightness of midtones without directly changing the black and white levels. Lower values darken midtones. Higher values brighten them. |
| Atari | Vidéo | Luminosité | `AtariEightBitSettingsConstants.ColorBrightnessOptionKey` | `Emulation.Atari.Video.Brightness` | Non | — | — | — | — |
| Amiga | Vidéo | Mode de lignes | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoVresolution` | `Emulation.Video.LineMode` | Oui | `Emulation.Help.Video.LineMode.Short` | Select the video line mode | `Emulation.Help.Video.LineMode.Detailed` | Selects how vertical video lines are displayed. Auto follows the emulated mode. Other choices force single lines, doubled lines, or scanlines when available. |
| Amiga, Atari | Vidéo | Norme | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoStandard, AtariVideoAudioSettingsConstants.StandardOption, AtariConfigurationOptionConstants.VideoStandard` | `Emulation.Video.Standard` | Oui | `Emulation.Help.Video.Standard.Short` | Select the video standard | `Emulation.Help.Video.Standard.Detailed` | Selects the emulated television timing, such as PAL or NTSC. This affects refresh rate, hardware timing, and software compatibility. |
| Atari | Vidéo | Palette externe | `AtariEightBitSettingsConstants.ExternalPaletteOptionKey` | `Emulation.Atari.Video.ExternalPalette` | Oui | `Emulation.Help.Video.ExternalPalette.Short` | Select an external color palette | `Emulation.Help.Video.ExternalPalette.Detailed` | Selects a predefined color palette instead of colors generated from the current video settings. Choose None to use the generated colors. |
| Amiga | Vidéo | Profondeur des couleurs | `AmigaSettingsDescriptionFunctionsConstants.OptionGfxColors` | `Emulation.Video.Colors` | Oui | `Emulation.Help.Video.Colors.Short` | Select output color depth | `Emulation.Help.Video.Colors.Detailed` | Selects the color depth used to render the image. 24-bit keeps more color detail. 16-bit uses a smaller range of colors. |
| Atari | Vidéo | Région | `AtariSettingsConstants.Region` | `Emulation.Atari.Video.Region` | Oui | `Emulation.Help.Video.Region.Short` | Select the hardware region | `Emulation.Help.Video.Region.Detailed` | Selects the regional timing used by the emulated machine. The choice can affect video frequency, CPU timing, and compatible firmware. |
| Amiga, Atari | Vidéo | Rendu | `AmigaSettingsConstants.VideoRenderer, AtariSettingsConstants.VideoRenderer` | `Emulation.Video.Settings.Rendering` | Oui | `Emulation.Help.Video.Rendering.Short` | Select the video renderer | `Emulation.Help.Video.Rendering.Detailed` | Selects the graphics backend used to draw the emulated display. Available renderers can differ in performance and compatibility with the host system. |
| Amiga, Atari | Vidéo | Résolution | `AmigaSettingsDescriptionFunctionsConstants.OptionVideoResolution, AtariVideoAudioSettingsConstants.ResolutionOption` | `Emulation.Video.Resolution` | Non | — | — | — | — |
| Atari | Vidéo | Retard de couleur GTIA | `AtariEightBitSettingsConstants.ColorDelayOptionKey` | `Emulation.Atari.Video.ColorDelay` | Oui | `Emulation.Help.Video.ColorDelay.Short` | Adjust GTIA color delay | `Emulation.Help.Video.ColorDelay.Detailed` | Sets the GTIA color phase delay used to reproduce colors. Default uses the standard value. Numeric choices shift the resulting hues. |
| Amiga, Atari | Vidéo | Rogner les bordures | `AmigaSettingsDescriptionFunctionsConstants.OptionCrop, AtariVideoAudioSettingsConstants.CropOption` | `Emulation.Video.Crop` | Non | — | — | — | — |
| Atari | Vidéo | Saturation | `AtariEightBitSettingsConstants.ColorSaturationOptionKey` | `Emulation.Atari.Video.Saturation` | Non | — | — | — | — |
| Amiga, Atari | Vidéo | Saut d’images | `AmigaSettingsDescriptionFunctionsConstants.OptionGfxFramerate, AtariVideoAudioSettingsConstants.FrameSkipOption` | `Emulation.Video.FrameSkip` | Oui | `Emulation.Help.Video.FrameSkip.Short` | Set frame skipping | `Emulation.Help.Video.FrameSkip.Detailed` | Sets how many display frames are omitted. Disabled draws every frame. Higher values reduce rendering work but make motion less smooth. |
| Atari | Vidéo | Teinte | `AtariEightBitSettingsConstants.ColorHueOptionKey` | `Emulation.Atari.Video.Hue` | Non | — | — | — | — |

## 5. Destination des ROM

Ajouter à la liste existante des ROM détectées une colonne indiquant dans quel champ la ROM sera placée pour la machine actuellement affichée. Cette destination doit réutiliser l’information de routage déjà employée par le bouton **Utiliser**, qui place déjà la ROM dans le bon champ ; elle ne doit pas être recalculée par une seconde logique.

Une ROM correspond à un seul champ pour cette machine. Les autres machines et leurs éventuelles utilisations ne doivent pas être prises en compte dans cette colonne.

### Affichage

La destination est affichée sous la forme d’un nom simple, dans le même style que la colonne indiquant la compatibilité.

Le texte doit reprendre directement le libellé déjà traduit du champ cible, par exemple **Kickstart** ou **ROM étendue**. Il ne faut pas créer une nouvelle traduction uniquement pour cette colonne.

La longueur affichée est limitée à 20 caractères, ellipse comprise. Si le libellé est trop long, il est tronqué avec une ellipse. La colonne **Destination** est placée après la colonne indiquant la compatibilité.

Si aucune destination ne peut être déterminée pour la machine affichée, la cellule reste vide.

### Comportement

Cette nouvelle colonne est uniquement informative. Elle ne modifie pas le fonctionnement actuel du bouton **Utiliser** ni les autres informations déjà affichées pour les ROM.

## 6. Associations des manettes et joysticks

### Disposition générale

Une représentation réaliste du périphérique émulé sélectionné doit être affichée à droite du tableau des associations.

Le tableau et le bloc réservé à cette représentation conservent leur disposition. Si la largeur disponible diminue, le tableau ne doit pas être réduit : seule l’image du périphérique est redimensionnée à l’intérieur de son bloc.

Lorsque le tableau défile verticalement, le bloc du périphérique reste fixe et visible afin de faciliter la définition des associations.

La colonne **État** du tableau doit être réduite à la largeur nécessaire pour conserver uniquement son icône. Le texte **Valide** est retiré afin de gagner de la place sans masquer l’information importante.

Les boutons **Assigner** du tableau restent disponibles.

### Ports émulés

Les ports sont déjà présentés dans des onglets distincts et un seul tableau de port est visible à la fois.

Le visuel affiché à droite correspond donc simplement au périphérique du port actuellement ouvert. Lorsque l’utilisateur change d’onglet de port, le tableau et le visuel correspondant à ce port sont affichés ensemble. Plusieurs représentations ne doivent pas être affichées simultanément.

### Choix et enregistrement du visuel

Le type de périphérique émulé et son visuel sont deux choix distincts. Le type est la valeur fournie par la DLL d’émulation, par exemple `Joystick`, `Cd32Pad` ou `None`. Pour le port actuellement ouvert, l’utilisateur peut choisir un visuel parmi les modèles matériels déclarés compatibles avec le module, la machine et ce type.

Le changement de visuel ne modifie ni le type de périphérique émulé, ni ses associations. Le visuel choisi est enregistré avec la configuration du port de la machine par le même enregistrement automatique que les autres réglages. Tant qu’aucune configuration n’a encore été enregistrée, le choix reste porté par l’état d’édition courant.

Un même modèle matériel n’existe qu’une fois dans le catalogue et peut être proposé à plusieurs ordinateurs lorsque ce modèle a réellement existé pour eux. Une compatibilité technique seule ne suffit pas pour proposer le visuel d’une manette propre à une autre console ou famille de machines.

Les DLL d’émulation déclarent les VisualId compatibles avec chacun de leurs types et le VisualId utilisé par défaut. Elles peuvent déjà déclarer des VisualId dont l’image n’existe pas encore. L’application croise cette déclaration avec les profils réellement disponibles dans son catalogue et n’affiche dans le sélecteur que ceux dont l’image et les zones existent effectivement.

Lorsqu’une console ou un ordinateur possède un contrôleur de base propre à sa machine, ce modèle est le visuel par défaut. Les modules Amiga et Atari ne possédant pas un unique joystick de base commun à leurs machines, leur type `Joystick` utilise le QuickShot comme visuel par défaut. Les visuels Mega Drive peuvent être renseignés pour un futur module Mega Drive, mais ne sont pas proposés actuellement comme visuels d’une console absente.

Les noms de produits et de modèles, tels que `Competition Pro 5000`, ne sont pas traduits. Ils sont conservés dans les ressources générales `00-Base` et ne sont pas recopiés dans les fichiers propres aux langues.

### Périphériques à représenter

Pour commencer, il faut réaliser les images des périphériques basiques déjà reconnus par les émulateurs. La liste de ces périphériques existe déjà dans l’application et doit être utilisée directement.

Des représentations supplémentaires pourront être ajoutées plus tard.

#### Inventaire réel des périphériques émulés

Cet inventaire reprend uniquement les valeurs de `EmulationControllerChoice` effectivement produites par `AmigaInputSettingsFunctions` et `AtariInputSettingsFunctions`. Dans la colonne des commandes, chaque définition est écrite sous la forme `identifiant / clé de ressource / association par défaut / valeur invariante`. `—` signifie une chaîne vide ou une valeur nulle. Les DLL ne choisissent aucune touche ni aucun bouton physique par défaut : toutes les associations sont vides jusqu’à une affectation explicite de l’utilisateur.

| Module | `EmulationControllerChoice.Id` | Réalisation | Machines et ports concernés | `InputBindingDefinition` produites |
| --- | --- | --- | --- | --- |
| Amiga | `Joystick` | Maintenant — image présente | Tous les modèles Amiga, ports standards ; tous les modèles avec adaptateur parallèle activé, ports parallèles | `Up / Emulation.Controller.Action.Up / — / —` ; `Down / Emulation.Controller.Action.Down / — / —` ; `Left / Emulation.Controller.Action.Left / — / —` ; `Right / Emulation.Controller.Action.Right / — / —` ; `B / Emulation.Controller.Action.Fire1 / — / —` ; `A / Emulation.Controller.Action.Fire2 / — / —` ; `L2 / Emulation.Controller.Action.TurboFire / — / —` |
| Amiga | `AnalogJoystick` | Maintenant — image présente | Tous les modèles Amiga, ports standards | Mêmes définitions que `Joystick` |
| Amiga | `Cd32Pad` | Maintenant — image présente | Amiga CD32, ports standards | `Up / Emulation.Controller.Action.Up / — / —` ; `Down / Emulation.Controller.Action.Down / — / —` ; `Left / Emulation.Controller.Action.Left / — / —` ; `Right / Emulation.Controller.Action.Right / — / —` ; `B / Emulation.Amiga.Controller.Cd32.Red / — / —` ; `A / Emulation.Amiga.Controller.Cd32.Blue / — / —` ; `Y / Emulation.Amiga.Controller.Cd32.Green / — / —` ; `X / Emulation.Amiga.Controller.Cd32.Yellow / — / —` ; `L / Emulation.Amiga.Controller.Cd32.Rewind / — / —` ; `R / Emulation.Amiga.Controller.Cd32.FastForward / — / —` ; `Start / Emulation.Amiga.Controller.Cd32.PlayPause / — / —` ; `L2 / Emulation.Controller.Action.TurboFire / — / —` |
| Amiga | `None` | Sans représentation | Tous les modèles Amiga, ports standards et ports parallèles | Aucune définition |
| Atari | `Joystick` | Maintenant — image présente | Atari ST, STF, STFM, Mega ST, STE, Mega STE, TT et Falcon | `Up / Emulation.Controller.Action.Up / — / —` ; `Down / Emulation.Controller.Action.Down / — / —` ; `Left / Emulation.Controller.Action.Left / — / —` ; `Right / Emulation.Controller.Action.Right / — / —` ; `Fire1 / Emulation.Controller.Action.Fire1 / — / —` ; `Turbo / Emulation.Controller.Action.TurboFire / — / —` |
| Atari | `Joystick` | Maintenant — image présente | Atari 400, 800, 800XL, 130XE, XEGS, XL/XE et 2600 | Définitions `Up`, `Down`, `Left`, `Right` et `Fire1` de la ligne précédente, avec les mêmes associations par défaut |
| Atari | `AnalogJoystick` | Maintenant — image présente | Atari 5200 | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés `Emulation.Controller.Action.{identifiant}` et les associations par défaut `DPadUp`, `DPadDown`, `DPadLeft`, `DPadRight`, `ButtonA` et `ButtonB` ; `Start / Emulation.Controller.Action.Start / — / Start` ; `Pause / Emulation.Controller.Action.Pause / — / Pause` ; `Reset / Emulation.Controller.Action.Reset / — / Reset` ; `Key0` à `Key9`, `Star` et `Hash / Emulation.Controller.Action.{identifiant} / — / {identifiant}` |
| Atari | `Paddle` | Ajout ultérieur — image manquante | Atari 400, 800, 800XL, 130XE, XEGS, XL/XE et 2600 | `Fire1 / Emulation.Controller.Action.Fire1 / — / —` |
| Atari | `DrivingController` | Ajout ultérieur — image manquante | Atari 2600 | `Fire1 / Emulation.Controller.Action.Fire1 / — / —` |
| Atari | `BoosterGrip` | Ajout ultérieur — image manquante | Atari 2600 | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés et associations par défaut déjà détaillées ; `Turbo / Emulation.Controller.Action.TurboFire / — / —` |
| Atari | `GenesisController` | Sans représentation actuelle — le visuel Mega Drive reste réservé à un futur module Mega Drive | Atari 2600 | `Up`, `Down`, `Left`, `Right` et `Fire1` avec les clés et associations par défaut déjà détaillées |
| Atari | `Joy2BPlus` | Ajout ultérieur — image manquante | Atari 2600 | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés et associations par défaut déjà détaillées |
| Atari | `ProLineController` | Maintenant — image présente | Atari 7800 | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés et associations par défaut déjà détaillées |
| Atari | `LightGun` | Ajout ultérieur — image manquante | Atari 7800 | `Fire1 / Emulation.Controller.Action.Fire1 / — / —` |
| Atari | `EnhancedController` | Ajout ultérieur — image manquante | Atari Lynx | `Up`, `Down`, `Left`, `Right`, `Fire1` et `Fire2` avec les clés et associations par défaut déjà détaillées ; `Option1 / Emulation.Controller.Action.Option1 / — / Option 1` ; `Option2 / Emulation.Controller.Action.Option2 / — / Option 2` ; `Pause / Emulation.Controller.Action.Pause / — / Pause` |
| Atari | `EnhancedController` | Maintenant — image présente | Atari Jaguar et Jaguar CD | `Up`, `Down`, `Left` et `Right` avec les clés et associations par défaut déjà détaillées ; `A / Emulation.Controller.Action.A / — / A` ; `B / Emulation.Controller.Action.B / — / B` ; `C / Emulation.Controller.Action.C / — / C` ; `Option / Emulation.Controller.Action.Option / — / Option` ; `Pause / Emulation.Controller.Action.Pause / — / Pause` ; `Key0` à `Key9`, `Star` et `Hash / Emulation.Controller.Action.{identifiant} / — / {identifiant}` |
| Atari | `None` | Sans représentation | Tous les modèles Atari | Aucune définition |

#### Visuels déclarés par les DLL

Les listes suivantes sont déclarées par les DLL d’émulation. L’application n’affiche dans le sélecteur que les VisualId possédant effectivement un profil dans son catalogue.

| Module et choix | VisualId compatibles déclarés | VisualId par défaut |
| --- | --- | --- |
| Amiga `Joystick` | `quickshot`, `quickshot-deluxe`, `quickshot-ii-turbo`, `competition-pro-5000`, `zipstik-super-pro`, `konix-speedking-left-hand`, `konix-speedking-right-hand`, `suncom-tac-2`, `powerplay-cruiser`, `suzo-the-arcade-turbo`, `advanced-gravis-gamepad` | `quickshot` |
| Amiga `AnalogJoystick` | `konix-speedking-analog` | `konix-speedking-analog` |
| Amiga `Cd32Pad` | `commodore-cd32`, `competition-pro-cd32` | `commodore-cd32` |
| Amiga `None` | — | — |
| Atari 2600 `Joystick` | `atari-cx40` | `atari-cx40` |
| Autres ordinateurs Atari `Joystick` | `quickshot`, `quickshot-deluxe`, `quickshot-ii-turbo`, `competition-pro-5000`, `zipstik-super-pro`, `konix-speedking-left-hand`, `konix-speedking-right-hand`, `suncom-tac-2`, `powerplay-cruiser`, `suzo-the-arcade-turbo`, `advanced-gravis-gamepad`, `atari-cx40` | `quickshot` |
| Atari 5200 `AnalogJoystick` | `atari-5200-controller` | `atari-5200-controller` |
| Atari `Paddle` | `atari-paddle` | `atari-paddle` |
| Atari 2600 `DrivingController` | `atari-2600-driving-controller` | `atari-2600-driving-controller` |
| Atari 2600 `BoosterGrip` | `atari-booster-grip` | `atari-booster-grip` |
| Atari 2600 `GenesisController` | — | — |
| Atari 2600 `Joy2BPlus` | `atari-joy2b-plus` | `atari-joy2b-plus` |
| Atari 7800 `ProLineController` | `atari-7800-control-pad-europe`, `atari-7800-pro-line-cx24` | `atari-7800-control-pad-europe` |
| Atari 7800 `LightGun` | `atari-xg-1-light-gun` | `atari-xg-1-light-gun` |
| Atari Lynx `EnhancedController` | `atari-lynx`, `atari-lynx-ii` | `atari-lynx` |
| Atari Jaguar et Jaguar CD `EnhancedController` | `atari-jaguar-controller`, `atari-jaguar-pro-controller` | `atari-jaguar-controller` |
| Atari `None` | — | — |

`mega-drive-3` reste enregistré dans le catalogue général pour un futur module Mega Drive, mais aucune DLL de console Mega Drive actuellement disponible ne peut encore le déclarer comme visuel de port.

#### Profils dont l’image existe déjà dans l’application

| VisualId | Modèle matériel | Image |
| --- | --- | --- |
| `quickshot` | QuickShot | `quickshot.png` |
| `quickshot-deluxe` | QuickShot Deluxe | `quickshot-deluxe.png` |
| `quickshot-ii-turbo` | QuickShot II Turbo | `quickshot-ii-turbo.png` |
| `competition-pro-5000` | Competition Pro 5000 | `competition-pro-5000.png` |
| `zipstik-super-pro` | Zipstik Super Pro | `zipstik-super-pro.png` |
| `konix-speedking-left-hand` | Konix Speedking, modèle pour gaucher | `konix-speedking-left-hand.png` |
| `konix-speedking-right-hand` | Konix Speedking, modèle pour droitier | `konix-speedking-right-hand.png` |
| `konix-speedking-analog` | Konix Speedking analogique | `konix-speedking-analog.png` |
| `suncom-tac-2` | Suncom TAC-2 | `suncom-tac-2.png` |
| `powerplay-cruiser` | Powerplay Cruiser | `powerplay-cruiser.png` |
| `suzo-the-arcade-turbo` | Suzo The Arcade Turbo | `suzo-the-arcade-turbo.png` |

| `commodore-cd32` | Manette Commodore CD32 | `commodore-cd32.png` |
| `competition-pro-cd32` | Competition Pro CD32 | `competition-pro-cd32.png` |
| `atari-cx40` | Atari CX40 | `atari-cx40.png` |
| `atari-5200-controller` | Contrôleur Atari 5200 | `atari-5200-controller.png` |
| `atari-7800-pro-line-cx24` | Atari 7800 Pro-Line CX24 | `atari-7800-pro-line-cx24.png` |
| `atari-7800-control-pad-europe` | Atari 7800 Control Pad européen | `atari-7800-control-pad-europe.png` |
| `atari-jaguar-controller` | Manette Atari Jaguar | `atari-jaguar-controller.png` |
| `atari-jaguar-pro-controller` | Manette Atari Jaguar Pro | `atari-jaguar-pro-controller.png` |


Le fichier `advanced-gravis-gamepad.png` présent dans le dossier ne reproduit pas le modèle matériel exact : sa commande directionnelle n’a pas la croix violette de l’Advanced Gravis GamePad. Aucun profil ni aucune zone ne lui sont associés. Le VisualId peut rester déclaré par les DLL pour une utilisation future, mais l’application l’exclut du sélecteur tant qu’une image conforme et ses zones n’ont pas été validées.

#### Correspondance des rôles visuels avec les commandes des DLL

Les noms de la colonne **Rôle visuel** sont les valeurs typées communes utilisées par les profils d’image. La colonne **Identifiant de commande DLL** reprend exclusivement un identifiant présent dans les `InputBindingDefinition` de la ligne concernée. Une ligne absente signifie que la zone correspondante du profil reste inactive pour ce choix.

| Module, machines et choix | Rôle visuel | Identifiant de commande DLL |
| --- | --- | --- |
| Amiga, tous les modèles, `Joystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Amiga, tous les modèles, `Joystick` | `PrimaryAction`, `SecondaryAction`, `Turbo` | `B`, `A`, `L2` |
| Amiga, tous les modèles, `AnalogJoystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Amiga, tous les modèles, `AnalogJoystick` | `PrimaryAction`, `SecondaryAction`, `Turbo` | `B`, `A`, `L2` |
| Amiga CD32, `Cd32Pad` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Amiga CD32, `Cd32Pad` | `PrimaryAction`, `SecondaryAction`, `TertiaryAction`, `QuaternaryAction` | `B`, `A`, `Y`, `X` |
| Amiga CD32, `Cd32Pad` | `LeftShoulder`, `RightShoulder`, `Start`, `Turbo` | `L`, `R`, `Start`, `L2` |
| Atari ST/STF/STFM/Mega ST/STE/Mega STE/TT/Falcon, `Joystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari ST/STF/STFM/Mega ST/STE/Mega STE/TT/Falcon, `Joystick` | `PrimaryAction`, `Turbo` | `Fire1`, `Turbo` |
| Atari 400/800/800XL/130XE/XEGS/XL-XE/2600, `Joystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight`, `PrimaryAction` | `Up`, `Down`, `Left`, `Right`, `Fire1` |
| Atari 5200, `AnalogJoystick` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari 5200, `AnalogJoystick` | `PrimaryAction`, `SecondaryAction`, `Start`, `Pause`, `Reset` | `Fire1`, `Fire2`, `Start`, `Pause`, `Reset` |
| Atari 5200, `AnalogJoystick` | `Key0` à `Key9`, `KeyStar`, `KeyHash` | `Key0` à `Key9`, `Star`, `Hash` |
| Atari 400/800/800XL/130XE/XEGS/XL-XE/2600, `Paddle` | `PrimaryAction` | `Fire1` |
| Atari 2600, `DrivingController` | `PrimaryAction` | `Fire1` |
| Atari 2600, `BoosterGrip` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari 2600, `BoosterGrip` | `PrimaryAction`, `SecondaryAction`, `Turbo` | `Fire1`, `Fire2`, `Turbo` |
| Atari 2600, `GenesisController` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight`, `PrimaryAction` | `Up`, `Down`, `Left`, `Right`, `Fire1` |
| Atari 2600, `Joy2BPlus` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari 2600, `Joy2BPlus` | `PrimaryAction`, `SecondaryAction` | `Fire1`, `Fire2` |
| Atari 7800, `ProLineController` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari 7800, `ProLineController` | `PrimaryAction`, `SecondaryAction` | `Fire1`, `Fire2` |
| Atari 7800, `LightGun` | `PrimaryAction` | `Fire1` |
| Atari Lynx, `EnhancedController` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari Lynx, `EnhancedController` | `PrimaryAction`, `SecondaryAction`, `Option1`, `Option2`, `Pause` | `Fire1`, `Fire2`, `Option1`, `Option2`, `Pause` |
| Atari Jaguar/Jaguar CD, `EnhancedController` | `DirectionUp`, `DirectionDown`, `DirectionLeft`, `DirectionRight` | `Up`, `Down`, `Left`, `Right` |
| Atari Jaguar/Jaguar CD, `EnhancedController` | `PrimaryAction`, `SecondaryAction`, `TertiaryAction`, `Option`, `Pause` | `A`, `B`, `C`, `Option`, `Pause` |
| Atari Jaguar/Jaguar CD, `EnhancedController` | `Key0` à `Key9`, `KeyStar`, `KeyHash` | `Key0` à `Key9`, `Star`, `Hash` |
| Amiga ou Atari, `None` | — | — |


#### Zones du profil `quickshot`

Les coordonnées sont exprimées en pourcentage de l’image `quickshot.png`, mesurée sur 924 × 898 pixels. Les quatre zones directionnelles partagent l’emprise de la tête du joystick ; le rôle indique le secteur actif et permet au rendu commun de combiner les directions simultanées. La zone du bouton rouge est prioritaire au survol et au clic lorsqu’elle recouvre cette emprise.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 22,6 % | 0,0 % | 54,0 % | 52,6 % |
| `DirectionDown` | `JoystickDirection` | 22,6 % | 0,0 % | 54,0 % | 52,6 % |
| `DirectionLeft` | `JoystickDirection` | 22,6 % | 0,0 % | 54,0 % | 52,6 % |
| `DirectionRight` | `JoystickDirection` | 22,6 % | 0,0 % | 54,0 % | 52,6 % |
| `PrimaryAction` | `RoundedRectangle` | 43,8 % | 4,3 % | 13,4 % | 28,0 % |


#### Zones du profil `quickshot-deluxe`

Les coordonnées sont exprimées en pourcentage de l’image `quickshot-deluxe.png`, mesurée sur 820 × 832 pixels. Les quatre directions partagent l’emprise de la tête du joystick. Le bouton rouge central correspond à l’action principale ; les deux boutons bleus correspondent à l’action secondaire et au turbo.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 6,0 % | 0,0 % | 88,4 % | 49,2 % |
| `DirectionDown` | `JoystickDirection` | 6,0 % | 0,0 % | 88,4 % | 49,2 % |
| `DirectionLeft` | `JoystickDirection` | 6,0 % | 0,0 % | 88,4 % | 49,2 % |
| `DirectionRight` | `JoystickDirection` | 6,0 % | 0,0 % | 88,4 % | 49,2 % |
| `PrimaryAction` | `RoundedRectangle` | 37,4 % | 6,7 % | 24,4 % | 14,5 % |
| `SecondaryAction` | `RoundedRectangle` | 15,6 % | 7,5 % | 14,4 % | 11,7 % |
| `Turbo` | `RoundedRectangle` | 69,3 % | 7,6 % | 14,6 % | 11,5 % |


#### Zones du profil `quickshot-ii-turbo`

Les coordonnées sont exprimées en pourcentage de l’image `quickshot-ii-turbo.png`, mesurée sur 810 × 877 pixels. Le profil comporte la tête directionnelle centrale et son bouton rouge visible. Aucune zone `Turbo` séparée n’est ajoutée, car aucune commande de turbo distincte n’est visible sur cette image.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 33,3 % | 8,9 % | 33,4 % | 61,8 % |
| `DirectionDown` | `JoystickDirection` | 33,3 % | 8,9 % | 33,4 % | 61,8 % |
| `DirectionLeft` | `JoystickDirection` | 33,3 % | 8,9 % | 33,4 % | 61,8 % |
| `DirectionRight` | `JoystickDirection` | 33,3 % | 8,9 % | 33,4 % | 61,8 % |
| `PrimaryAction` | `RoundedRectangle` | 39,9 % | 15,6 % | 20,0 % | 41,4 % |


#### Zones du profil `competition-pro-5000`

Les coordonnées sont exprimées en pourcentage de l’image `competition-pro-5000.png`, mesurée sur 726 × 1045 pixels. La boule centrale porte les quatre directions. Le bouton rouge gauche correspond à l’action principale et le bouton rouge droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 22,5 % | 38,0 % | 54,7 % | 39,1 % |
| `DirectionDown` | `JoystickDirection` | 22,5 % | 38,0 % | 54,7 % | 39,1 % |
| `DirectionLeft` | `JoystickDirection` | 22,5 % | 38,0 % | 54,7 % | 39,1 % |
| `DirectionRight` | `JoystickDirection` | 22,5 % | 38,0 % | 54,7 % | 39,1 % |
| `PrimaryAction` | `Ellipse` | 5,8 % | 3,3 % | 30,9 % | 23,1 % |
| `SecondaryAction` | `Ellipse` | 63,9 % | 3,3 % | 31,0 % | 23,3 % |


#### Zones du profil `zipstik-super-pro`

Les coordonnées sont exprimées en pourcentage de l’image `zipstik-super-pro.png`, mesurée sur 700 × 947 pixels. La commande centrale porte les quatre directions. Le bouton jaune gauche correspond à l’action principale et le bouton jaune droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 22,1 % | 39,7 % | 56,1 % | 41,9 % |
| `DirectionDown` | `JoystickDirection` | 22,1 % | 39,7 % | 56,1 % | 41,9 % |
| `DirectionLeft` | `JoystickDirection` | 22,1 % | 39,7 % | 56,1 % | 41,9 % |
| `DirectionRight` | `JoystickDirection` | 22,1 % | 39,7 % | 56,1 % | 41,9 % |
| `PrimaryAction` | `RoundedRectangle` | 7,4 % | 5,1 % | 21,9 % | 16,8 % |
| `SecondaryAction` | `RoundedRectangle` | 71,0 % | 5,1 % | 21,7 % | 16,8 % |


#### Zones du profil `konix-speedking-left-hand`

Les coordonnées sont exprimées en pourcentage de l’image `konix-speedking-left-hand.png`, mesurée sur 554 × 1041 pixels. Seule la commande directionnelle visible du dessus possède des zones. Les gâchettes latérales non visibles ne reçoivent pas de fausse zone.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 30,7 % | 12,0 % | 41,0 % | 23,8 % |
| `DirectionDown` | `JoystickDirection` | 30,7 % | 12,0 % | 41,0 % | 23,8 % |
| `DirectionLeft` | `JoystickDirection` | 30,7 % | 12,0 % | 41,0 % | 23,8 % |
| `DirectionRight` | `JoystickDirection` | 30,7 % | 12,0 % | 41,0 % | 23,8 % |

#### Zones du profil `konix-speedking-right-hand`

Les coordonnées sont exprimées en pourcentage de l’image `konix-speedking-right-hand.png`, mesurée sur 584 × 1041 pixels. Seule la commande directionnelle visible du dessus possède des zones. Les gâchettes latérales non visibles ne reçoivent pas de fausse zone.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 27,1 % | 12,0 % | 44,0 % | 23,8 % |
| `DirectionDown` | `JoystickDirection` | 27,1 % | 12,0 % | 44,0 % | 23,8 % |
| `DirectionLeft` | `JoystickDirection` | 27,1 % | 12,0 % | 44,0 % | 23,8 % |
| `DirectionRight` | `JoystickDirection` | 27,1 % | 12,0 % | 44,0 % | 23,8 % |


#### Zones du profil `konix-speedking-analog`

Les coordonnées sont exprimées en pourcentage de l’image `konix-speedking-analog.png`, mesurée sur 1290 × 1219 pixels. La boule centrale porte les directions analogiques. Les boutons `A` et `B` correspondent aux actions principale et secondaire. Le réglage `ADJ CENTRE` et l’interrupteur `CENTRE RETURN` n’ont pas de zone, car ils ne correspondent à aucune `InputBindingDefinition` du choix.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 25,0 % | 17,1 % | 50,0 % | 53,4 % |
| `DirectionDown` | `JoystickDirection` | 25,0 % | 17,1 % | 50,0 % | 53,4 % |
| `DirectionLeft` | `JoystickDirection` | 25,0 % | 17,1 % | 50,0 % | 53,4 % |
| `DirectionRight` | `JoystickDirection` | 25,0 % | 17,1 % | 50,0 % | 53,4 % |
| `PrimaryAction` | `Ellipse` | 12,9 % | 74,0 % | 14,3 % | 15,0 % |
| `SecondaryAction` | `Ellipse` | 73,4 % | 74,0 % | 14,0 % | 15,0 % |


#### Zones du profil `suncom-tac-2`

Les coordonnées sont exprimées en pourcentage de l’image `suncom-tac-2.png`, mesurée sur 1290 × 1219 pixels. La boule centrale porte les quatre directions. Le bouton rouge gauche correspond à l’action principale et le bouton rouge droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 37,2 % | 32,2 % | 25,0 % | 26,0 % |
| `DirectionDown` | `JoystickDirection` | 37,2 % | 32,2 % | 25,0 % | 26,0 % |
| `DirectionLeft` | `JoystickDirection` | 37,2 % | 32,2 % | 25,0 % | 26,0 % |
| `DirectionRight` | `JoystickDirection` | 37,2 % | 32,2 % | 25,0 % | 26,0 % |
| `PrimaryAction` | `Ellipse` | 13,6 % | 67,4 % | 16,4 % | 18,3 % |
| `SecondaryAction` | `Ellipse` | 69,4 % | 67,4 % | 16,8 % | 18,3 % |


#### Zones du profil `powerplay-cruiser`

Les coordonnées sont exprimées en pourcentage de l’image `powerplay-cruiser.png`, mesurée sur 1199 × 1312 pixels. La commande centrale porte les quatre directions. Le bouton jaune gauche correspond à l’action principale et le bouton jaune droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 29,8 % | 10,6 % | 41,4 % | 39,3 % |
| `DirectionDown` | `JoystickDirection` | 29,8 % | 10,6 % | 41,4 % | 39,3 % |
| `DirectionLeft` | `JoystickDirection` | 29,8 % | 10,6 % | 41,4 % | 39,3 % |
| `DirectionRight` | `JoystickDirection` | 29,8 % | 10,6 % | 41,4 % | 39,3 % |
| `PrimaryAction` | `Ellipse` | 14,8 % | 64,9 % | 19,2 % | 17,8 % |
| `SecondaryAction` | `Ellipse` | 67,8 % | 65,0 % | 18,9 % | 17,8 % |


#### Zones du profil `suzo-the-arcade-turbo`

Les coordonnées sont exprimées en pourcentage de l’image `suzo-the-arcade-turbo.png`, mesurée sur 1254 × 1254 pixels. La commande noire centrale porte les quatre directions, son bouton rouge correspond à l’action principale et la commande rouge séparée en bas correspond au turbo.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 34,4 % | 20,4 % | 30,8 % | 32,5 % |
| `DirectionDown` | `JoystickDirection` | 34,4 % | 20,4 % | 30,8 % | 32,5 % |
| `DirectionLeft` | `JoystickDirection` | 34,4 % | 20,4 % | 30,8 % | 32,5 % |
| `DirectionRight` | `JoystickDirection` | 34,4 % | 20,4 % | 30,8 % | 32,5 % |
| `PrimaryAction` | `Ellipse` | 43,1 % | 30,0 % | 13,2 % | 13,4 % |
| `Turbo` | `RoundedRectangle` | 39,9 % | 81,2 % | 20,0 % | 9,3 % |


#### Zones du profil `commodore-cd32`

Les coordonnées sont exprimées en pourcentage de l’image `commodore-cd32.png`, mesurée sur 1534 × 603 pixels. Le disque gauche porte les quatre directions. Les actions principale à quaternaire suivent les boutons rouge, bleu, vert et jaune. Les commandes supérieures correspondent au rembobinage et à l’avance rapide ; le bouton noir central correspond à lecture-pause. Aucune zone `Turbo` distincte n’est ajoutée.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 5,9 % | 24,0 % | 13,7 % | 34,0 % |
| `DirectionDown` | `DirectionalPad` | 5,9 % | 24,0 % | 13,7 % | 34,0 % |
| `DirectionLeft` | `DirectionalPad` | 5,9 % | 24,0 % | 13,7 % | 34,0 % |
| `DirectionRight` | `DirectionalPad` | 5,9 % | 24,0 % | 13,7 % | 34,0 % |
| `PrimaryAction` | `Ellipse` | 80,2 % | 47,8 % | 6,6 % | 16,6 % |
| `SecondaryAction` | `Ellipse` | 88,8 % | 44,6 % | 6,7 % | 16,7 % |
| `TertiaryAction` | `Ellipse` | 78,7 % | 26,5 % | 6,5 % | 16,4 % |
| `QuaternaryAction` | `Ellipse` | 87,2 % | 23,4 % | 6,6 % | 16,4 % |
| `LeftShoulder` | `RoundedRectangle` | 10,9 % | 0,0 % | 13,0 % | 2,7 % |
| `RightShoulder` | `RoundedRectangle` | 76,1 % | 0,0 % | 11,0 % | 2,7 % |
| `Start` | `RoundedRectangle` | 59,0 % | 67,5 % | 9,8 % | 7,1 % |


#### Zones du profil `competition-pro-cd32`

Les coordonnées sont exprimées en pourcentage de l’image `competition-pro-cd32.png`, mesurée sur 1568 × 807 pixels. Le disque gauche porte les directions. Les quatre boutons gris portant les symboles rouge, bleu, vert et jaune correspondent aux actions principale à quaternaire. Les palettes supérieures correspondent aux épaules gauche et droite. Les deux boutons argentés de lecture-pause portent tous deux le rôle `Start`. Le sélecteur supérieur situé sous `OFF / TURBO / AUTO` porte le rôle `Turbo`. Les autres curseurs de réglage n’ont pas de zone.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 7,3 % | 25,8 % | 25,1 % | 52,4 % |
| `DirectionDown` | `DirectionalPad` | 7,3 % | 25,8 % | 25,1 % | 52,4 % |
| `DirectionLeft` | `DirectionalPad` | 7,3 % | 25,8 % | 25,1 % | 52,4 % |
| `DirectionRight` | `DirectionalPad` | 7,3 % | 25,8 % | 25,1 % | 52,4 % |
| `PrimaryAction` | `Ellipse` | 74,2 % | 63,4 % | 7,7 % | 15,0 % |
| `SecondaryAction` | `Ellipse` | 84,5 % | 53,7 % | 7,7 % | 14,7 % |
| `TertiaryAction` | `Ellipse` | 69,5 % | 43,0 % | 7,4 % | 15,2 % |
| `QuaternaryAction` | `Ellipse` | 80,0 % | 33,6 % | 7,7 % | 14,5 % |
| `LeftShoulder` | `RoundedRectangle` | 6,5 % | 4,5 % | 19,0 % | 22,5 % |
| `RightShoulder` | `RoundedRectangle` | 74,5 % | 4,5 % | 19,0 % | 22,5 % |
| `Start` | `RoundedRectangle` | 39,2 % | 63,1 % | 6,1 % | 8,1 % |
| `Start` | `RoundedRectangle` | 48,5 % | 63,1 % | 6,1 % | 8,1 % |
| `Turbo` | `RoundedRectangle` | 55,2 % | 23,0 % | 6,6 % | 5,3 % |


#### Zones du profil `atari-cx40`

Les coordonnées sont exprimées en pourcentage de l’image `atari-cx40.png`, mesurée sur 1254 × 1254 pixels. La commande centrale porte les quatre directions et l’unique bouton rouge correspond à l’action principale.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 26,6 % | 28,1 % | 46,3 % | 44,6 % |
| `DirectionDown` | `JoystickDirection` | 26,6 % | 28,1 % | 46,3 % | 44,6 % |
| `DirectionLeft` | `JoystickDirection` | 26,6 % | 28,1 % | 46,3 % | 44,6 % |
| `DirectionRight` | `JoystickDirection` | 26,6 % | 28,1 % | 46,3 % | 44,6 % |
| `PrimaryAction` | `Ellipse` | 15,8 % | 14,4 % | 14,5 % | 14,4 % |


#### Zones du profil `atari-5200-controller`

Les coordonnées sont exprimées en pourcentage de l’image `atari-5200-controller.png`, mesurée sur 858 × 1832 pixels. Le joystick central porte les directions analogiques. Les boutons latéraux supérieurs gauche et droit portent tous deux l’action principale ; les boutons latéraux inférieurs portent tous deux l’action secondaire. Les trois boutons système et les douze touches du clavier reprennent exactement les rôles déclarés pour le 5200.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 23,9 % | 17,1 % | 51,9 % | 24,0 % |
| `DirectionDown` | `JoystickDirection` | 23,9 % | 17,1 % | 51,9 % | 24,0 % |
| `DirectionLeft` | `JoystickDirection` | 23,9 % | 17,1 % | 51,9 % | 24,0 % |
| `DirectionRight` | `JoystickDirection` | 23,9 % | 17,1 % | 51,9 % | 24,0 % |
| `PrimaryAction` | `RoundedRectangle` | 13,4 % | 10,6 % | 3,6 % | 7,9 % |
| `PrimaryAction` | `RoundedRectangle` | 82,3 % | 10,6 % | 3,3 % | 7,9 % |
| `SecondaryAction` | `RoundedRectangle` | 13,4 % | 19,2 % | 3,6 % | 7,5 % |
| `SecondaryAction` | `RoundedRectangle` | 82,3 % | 19,2 % | 3,3 % | 7,5 % |
| `Start` | `RoundedRectangle` | 25,1 % | 8,4 % | 13,5 % | 4,6 % |
| `Pause` | `RoundedRectangle` | 43,5 % | 8,4 % | 13,6 % | 4,6 % |
| `Reset` | `RoundedRectangle` | 61,5 % | 8,4 % | 13,6 % | 4,6 % |
| `Key1` | `RoundedRectangle` | 27,9 % | 57,3 % | 13,0 % | 5,4 % |
| `Key2` | `RoundedRectangle` | 43,0 % | 57,3 % | 13,2 % | 5,4 % |
| `Key3` | `RoundedRectangle` | 60,6 % | 57,3 % | 13,2 % | 5,4 % |
| `Key4` | `RoundedRectangle` | 27,9 % | 63,3 % | 13,0 % | 5,4 % |
| `Key5` | `RoundedRectangle` | 43,0 % | 63,3 % | 13,2 % | 5,4 % |
| `Key6` | `RoundedRectangle` | 60,6 % | 63,3 % | 13,2 % | 5,4 % |
| `Key7` | `RoundedRectangle` | 27,9 % | 70,8 % | 13,0 % | 5,4 % |
| `Key8` | `RoundedRectangle` | 43,0 % | 70,8 % | 13,2 % | 5,4 % |
| `Key9` | `RoundedRectangle` | 60,6 % | 70,8 % | 13,2 % | 5,4 % |
| `KeyStar` | `RoundedRectangle` | 27,9 % | 78,3 % | 13,0 % | 5,4 % |
| `Key0` | `RoundedRectangle` | 43,0 % | 78,3 % | 13,2 % | 5,4 % |
| `KeyHash` | `RoundedRectangle` | 60,6 % | 78,3 % | 13,2 % | 5,4 % |


#### Zones du profil `atari-7800-pro-line-cx24`

Les coordonnées sont exprimées en pourcentage de l’image `atari-7800-pro-line-cx24.png`, mesurée sur 1023 × 1537 pixels. La commande centrale porte les quatre directions. Le bouton latéral rouge gauche correspond à l’action principale et le bouton latéral rouge droit à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `JoystickDirection` | 36,8 % | 27,4 % | 25,7 % | 18,4 % |
| `DirectionDown` | `JoystickDirection` | 36,8 % | 27,4 % | 25,7 % | 18,4 % |
| `DirectionLeft` | `JoystickDirection` | 36,8 % | 27,4 % | 25,7 % | 18,4 % |
| `DirectionRight` | `JoystickDirection` | 36,8 % | 27,4 % | 25,7 % | 18,4 % |
| `PrimaryAction` | `RoundedRectangle` | 26,2 % | 11,9 % | 6,8 % | 16,9 % |
| `SecondaryAction` | `RoundedRectangle` | 66,2 % | 11,7 % | 6,5 % | 17,0 % |


#### Zones du profil `atari-7800-control-pad-europe`

Les coordonnées sont exprimées en pourcentage de l’image `atari-7800-control-pad-europe.png`, mesurée sur 1518 × 1036 pixels. La croix gauche porte les quatre directions. Le bouton rouge `1` correspond à l’action principale et le bouton rouge `2` à l’action secondaire.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 14,8 % | 17,0 % | 19,4 % | 29,9 % |
| `DirectionDown` | `DirectionalPad` | 14,8 % | 17,0 % | 19,4 % | 29,9 % |
| `DirectionLeft` | `DirectionalPad` | 14,8 % | 17,0 % | 19,4 % | 29,9 % |
| `DirectionRight` | `DirectionalPad` | 14,8 % | 17,0 % | 19,4 % | 29,9 % |
| `PrimaryAction` | `Ellipse` | 46,4 % | 51,4 % | 10,3 % | 16,0 % |
| `SecondaryAction` | `Ellipse` | 64,8 % | 51,4 % | 10,3 % | 16,0 % |


#### Zones du profil `atari-jaguar-controller`

Les coordonnées sont exprimées en pourcentage de l’image `atari-jaguar-controller.png`, mesurée sur 1402 × 1122 pixels. La croix gauche porte les directions. Les boutons `A`, `B`, `C`, `Pause`, `Option` et les douze touches du clavier correspondent directement aux rôles déclarés pour la Jaguar.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 17,5 % | 16,9 % | 19,3 % | 23,4 % |
| `DirectionDown` | `DirectionalPad` | 17,5 % | 16,9 % | 19,3 % | 23,4 % |
| `DirectionLeft` | `DirectionalPad` | 17,5 % | 16,9 % | 19,3 % | 23,4 % |
| `DirectionRight` | `DirectionalPad` | 17,5 % | 16,9 % | 19,3 % | 23,4 % |
| `PrimaryAction` | `RoundedRectangle` | 71,8 % | 14,6 % | 10,7 % | 10,9 % |
| `SecondaryAction` | `RoundedRectangle` | 65,6 % | 23,5 % | 10,5 % | 10,6 % |
| `TertiaryAction` | `RoundedRectangle` | 59,2 % | 32,7 % | 11,1 % | 10,4 % |
| `Pause` | `RoundedRectangle` | 42,4 % | 32,6 % | 5,1 % | 7,4 % |
| `Option` | `RoundedRectangle` | 49,0 % | 32,6 % | 4,8 % | 7,4 % |
| `Key1` | `RoundedRectangle` | 35,3 % | 55,1 % | 7,8 % | 4,0 % |
| `Key2` | `RoundedRectangle` | 46,2 % | 55,1 % | 7,6 % | 4,0 % |
| `Key3` | `RoundedRectangle` | 56,8 % | 55,1 % | 7,7 % | 4,0 % |
| `Key4` | `RoundedRectangle` | 35,3 % | 63,6 % | 7,8 % | 4,0 % |
| `Key5` | `RoundedRectangle` | 46,2 % | 63,6 % | 7,6 % | 4,0 % |
| `Key6` | `RoundedRectangle` | 56,8 % | 63,6 % | 7,7 % | 4,0 % |
| `Key7` | `RoundedRectangle` | 35,3 % | 72,3 % | 7,8 % | 4,0 % |
| `Key8` | `RoundedRectangle` | 46,2 % | 72,3 % | 7,6 % | 4,0 % |
| `Key9` | `RoundedRectangle` | 56,8 % | 72,3 % | 7,7 % | 4,0 % |
| `KeyStar` | `RoundedRectangle` | 35,3 % | 80,7 % | 7,8 % | 4,1 % |
| `Key0` | `RoundedRectangle` | 46,2 % | 80,7 % | 7,6 % | 4,1 % |
| `KeyHash` | `RoundedRectangle` | 56,8 % | 80,7 % | 7,7 % | 4,1 % |


#### Zones du profil `atari-jaguar-pro-controller`

Les coordonnées sont exprimées en pourcentage de l’image `atari-jaguar-pro-controller.png`, mesurée sur 1337 × 1176 pixels. Seules les commandes produites par la DLL Jaguar actuelle possèdent une zone : directions, `A`, `B`, `C`, `Pause`, `Option` et clavier. Les commandes `X`, `Y`, `Z`, `L` et `R` visibles sur ce modèle Pro restent sans zone.

| Rôle visuel | Forme | X | Y | Largeur | Hauteur |
| --- | --- | ---: | ---: | ---: | ---: |
| `DirectionUp` | `DirectionalPad` | 22,0 % | 25,4 % | 14,8 % | 17,3 % |
| `DirectionDown` | `DirectionalPad` | 22,0 % | 25,4 % | 14,8 % | 17,3 % |
| `DirectionLeft` | `DirectionalPad` | 22,0 % | 25,4 % | 14,8 % | 17,3 % |
| `DirectionRight` | `DirectionalPad` | 22,0 % | 25,4 % | 14,8 % | 17,3 % |
| `PrimaryAction` | `Ellipse` | 73,2 % | 26,0 % | 6,7 % | 7,7 % |
| `SecondaryAction` | `Ellipse` | 66,7 % | 31,7 % | 6,4 % | 7,1 % |
| `TertiaryAction` | `Ellipse` | 61,3 % | 38,2 % | 6,2 % | 7,1 % |
| `Pause` | `RoundedRectangle` | 42,7 % | 36,2 % | 4,3 % | 4,9 % |
| `Option` | `RoundedRectangle` | 48,2 % | 36,2 % | 4,5 % | 4,9 % |
| `Key1` | `RoundedRectangle` | 36,6 % | 53,7 % | 5,8 % | 2,5 % |
| `Key2` | `RoundedRectangle` | 46,5 % | 53,7 % | 5,6 % | 2,5 % |
| `Key3` | `RoundedRectangle` | 56,1 % | 53,7 % | 5,8 % | 2,5 % |
| `Key4` | `RoundedRectangle` | 36,6 % | 60,5 % | 5,8 % | 2,5 % |
| `Key5` | `RoundedRectangle` | 46,5 % | 60,5 % | 5,6 % | 2,5 % |
| `Key6` | `RoundedRectangle` | 56,1 % | 60,5 % | 5,8 % | 2,5 % |
| `Key7` | `RoundedRectangle` | 36,6 % | 67,4 % | 5,8 % | 2,6 % |
| `Key8` | `RoundedRectangle` | 46,5 % | 67,4 % | 5,6 % | 2,6 % |
| `Key9` | `RoundedRectangle` | 56,1 % | 67,4 % | 5,8 % | 2,6 % |
| `KeyStar` | `RoundedRectangle` | 36,6 % | 74,4 % | 5,8 % | 2,5 % |
| `Key0` | `RoundedRectangle` | 46,5 % | 74,4 % | 5,6 % | 2,5 % |
| `KeyHash` | `RoundedRectangle` | 56,1 % | 74,4 % | 5,8 % | 2,5 % |

Chaque représentation doit être :

- réaliste ;
- vue du dessus dans son sens normal d’utilisation ;
- correctement réalisée, et non remplacée par un dessin générique de mauvaise qualité ;
- fournie avec un fond transparent ;
- accompagnée de zones de surimpression correctement placées sur ses directions, boutons et autres commandes ;
- accompagnée, au passage de la souris sur une zone cliquable, d’un petit halo ou d’un changement de couleur du halo permettant de voir immédiatement quelle commande peut être assignée.

### Réutilisation du système existant

Le système de représentation déjà utilisé dans l’onglet général **Manettes** doit être repris et adapté. Il ne faut pas en créer une copie indépendante pour les périphériques émulés.

Chaque image possède sa propre définition des positions, dimensions et formes de ses zones, puisque les commandes ne se trouvent pas au même endroit d’un périphérique à l’autre. Ces coordonnées propres à l’image sont exprimées en pourcentage par rapport à celle-ci afin de rester correctement alignées lorsque l’image est redimensionnée dans son bloc.

Pour un port donné, seules les zones correspondant aux `InputBindingDefinition` produites par la DLL pour le type de périphérique émulé sont actives, survolables et cliquables. Les commandes supplémentaires éventuellement visibles sur l’image ne créent aucune commande que l’émulateur ne gère pas.

Un profil d’image ne contient pas directement les identifiants de commandes propres à Amiga, Atari ou à un autre module. Il décrit ses zones avec des rôles visuels neutres et typés. Chaque DLL associe elle-même ces rôles aux identifiants exacts de ses `InputBindingDefinition` pour chaque `EmulationControllerChoice`. L’application active une zone uniquement lorsque cette association existe et que l’identifiant associé fait partie des définitions du choix courant. Ainsi, un même profil QuickShot reste unique tout en utilisant `B` sur Amiga et `Fire1` sur Atari pour son bouton principal, sans chaîne de commande propre à un module dans le catalogue de l’application.

Les différences portent principalement sur les images utilisées, les commandes représentées et la taille disponible.

### Refonte de la surimpression commune

Le rendu général des surimpressions existantes doit être revu, car son apparence actuelle n’est pas satisfaisante, particulièrement pour les commandes analogiques.

Cette amélioration concerne le système commun afin que le nouvel affichage des périphériques émulés et l’affichage existant des manettes bénéficient du même rendu corrigé.

Le style général des halos et des zones blanches doit être revu conformément aux comportements décrits ci-dessous. Le fait qu’un halo puisse recouvrir une partie de l’image n’est pas considéré comme un problème à corriger, et aucun autre style de couleur, de bordure ou d’agrandissement n’est décidé dans ce document.

Pour un stick analogique de manette, la surimpression doit être ronde comme le stick représenté. Ce rond se déplace depuis la position centrale dans la même direction que le stick physique, avec un déplacement correspondant à son inclinaison. Il ne faut pas afficher de trait terminé par un point.

Les joysticks à manche et les gâchettes analogiques utilisent le même principe : un halo ancré au centre de la commande et dont la longueur augmente progressivement selon la valeur analogique reçue.

- pour un joystick à manche, le halo s’étire depuis le centre dans la direction du manche ;
- pour une gâchette analogique, le halo s’étire depuis le centre vers le bas selon la pression exercée.

La forme précise de ce halo commun reste à tester et à valider lors de sa réalisation.

### Seuil des commandes analogiques

Un seuil doit être appliqué avant de modifier la surimpression d’un stick, d’un joystick ou d’une gâchette analogique. Tant que la valeur reste sous ce seuil, le visuel conserve sa position neutre afin de ne pas afficher les petits mouvements parasites du périphérique.

Le pourcentage définitif n’est pas encore choisi. Il devra être testé avec plusieurs périphériques.

Lorsqu’une machine ou un port possède déjà un réglage de zone morte analogique, il faudra étudier la réutilisation de cette valeur pour que le visuel corresponde au comportement réel de l’entrée. Le visualiseur général, qui ne dépend pas de la configuration d’une machine, aura besoin d’une valeur par défaut commune.

Les autres rendus précis seront conçus et validés lorsque cette amélioration sera réalisée ; ils ne doivent pas être inventés dans le présent document.
### Visualisation des appuis

Lorsqu’une entrée physique déjà associée est utilisée, la commande correspondante doit être mise en évidence sur la représentation du périphérique émulé.

Tous les appuis simultanés doivent être représentés en même temps, quel que soit leur nombre et quelle que soit leur origine.

Pour une commande analogique, la surimpression suit les comportements définis dans la section **Refonte de la surimpression commune** : halo rond mobile pour un stick de manette et halo progressif ancré au centre pour un joystick à manche ou une gâchette.

Une modification d’association ne change pas de manière permanente la représentation. La mise en évidence sert à montrer les entrées reçues en direct.

### Modification depuis la représentation

Un clic sur une commande de la représentation doit :

1. sélectionner la ligne correspondante dans le tableau ;
2. activer immédiatement la capture d’une nouvelle association.

Il ne faut ni double-clic ni bouton supplémentaire sur la représentation. Les boutons **Assigner** existants dans le tableau sont toutefois conservés et déclenchent la même capture.

### Sources des associations

La capture ne doit exiger aucune sélection préalable du périphérique physique.

Une association peut provenir de n’importe quel périphérique d’entrée pris en charge par GW GUI, notamment :

- une manette ou un joystick physique ;
- le clavier ;
- la souris ;
- un trackball ;
- les autres périphériques d’entrée qui seront pris en charge.

Le champ permettant de choisir globalement une manette physique, comme **Périphérique de la manette 1**, ainsi que ses équivalents, doit être retiré de cet écran.

Cela ne concerne pas le choix du type de périphérique émulé, qui reste nécessaire.

### Nom affiché pour un périphérique déconnecté

Aucun changement n’est demandé concernant l’identifiant technique visible lorsqu’une manette est déconnectée. Après reconnexion et retour dans l’onglet, le nom de la manette est déjà correctement affiché.

## Points généraux restant à décider ou à étudier

- la présence et l’apparence éventuelle d’une icône accompagnant la couleur des machines déjà configurées ;
- la présentation graphique définitive du tableau des configurations et de ses icônes ;
- la liste complète des filtres vidéo, leurs groupes, leurs réglages et leur méthode technique de réalisation ;
- les aspects encore ouverts de l’idée future des habillages d’écran ;
- le pourcentage du seuil utilisé avant tout changement visuel analogique ;
- les périphériques supplémentaires à représenter après les modèles basiques.

## Ordre général de réalisation

Cet ordre de groupes est validé. Les checklists détaillées de chaque point devront respecter cet ordre global, même lorsque deux éléments appartiennent à une même section fonctionnelle du présent document.

1. Enregistrement automatique fiable de toutes les configurations.
2. Nouveau tableau des configurations et suppression correcte.
3. Retour automatique du focus à l’émulation.
4. Destination des ROM.
5. Aides contextuelles.
6. Réutilisation et amélioration du visualiseur de manettes.
7. Recherche et architecture des filtres vidéo.
8. Habillages d’écran, beaucoup plus tard.

## Règles de rédaction et de suivi des tâches

- Les groupes et les tâches sont toujours écrits et réalisés exactement dans leur ordre réel d’exécution.
- Une sous-tâche d’action est cochée uniquement après l’écriture, la création, la modification, la copie ou le déplacement demandé et sa vérification.
- Une tâche finalisée est cochée lorsque toutes ses sous-tâches sont cochées. Le même principe s’applique ensuite en remontant jusqu’au groupe général.
- Une lecture, une recherche ou une réflexion n’est jamais une tâche isolée : elle fait partie d’une action qui produit ou modifie dans la même sous-tâche un fichier identifié.
- Lorsqu’un fichier doit être créé, sa création précède toujours l’ajout de son contenu.
- Toute modification indique le fichier concerné avant de décrire les changements à y effectuer.
- Un déplacement de code commence par le déplacement ou la copie du code existant en conservant exactement son fonctionnement. La suppression de l’ancien emplacement intervient seulement après vérification du déplacement. Toute modification fonctionnelle éventuelle constitue une tâche ultérieure séparée.
- Aucun comportement n’est modifié, corrigé ou remplacé par préférence personnelle. Une correction non prévue n’est effectuée que si une erreur réelle est constatée.
- Ne jamais inventer et ne jamais extrapoler un comportement, une donnée, une dépendance, une solution ou une tâche.
- Ne jamais sauter une étape : chaque tâche et chaque sous-tâche est réalisée dans l’ordre écrit, uniquement lorsque toutes les étapes précédentes nécessaires sont réellement terminées, vérifiées et cochées.
- Ne passer à la tâche suivante qu’après avoir coché la tâche précédente réellement terminée. Si une tâche nécessaire a été oubliée pendant l’exécution, l’inscrire d’abord au bon endroit puis la réaliser avant de reprendre la suite.
- Lorsqu’une action potentiellement nécessaire n’est pas inscrite, lire d’abord les fichiers et le fonctionnement directement concernés afin de déterminer si elle est réellement indispensable et entièrement justifiée. Si elle l’est, ajouter la tâche correspondante au bon endroit dans l’ordre d’exécution, puis seulement effectuer cette action.
- Si cette vérification ne permet pas de trancher sans inventer, extrapoler ou choisir un comportement non validé, arrêter le travail et demander une décision avant toute modification.
- Lorsque plusieurs informations ou décisions sont nécessaires pour poursuivre, identifier toutes les questions réellement bloquantes et les poser ensemble afin de pouvoir compléter les tâches puis les exécuter sans interruptions évitables.
- Ne jamais casser le code : préserver le fonctionnement existant qui n’est pas explicitement concerné, vérifier chaque modification et corriger uniquement les régressions qu’elle provoque avant de poursuivre.
- Lorsqu’un changement nécessaire touche un système existant, l’améliorer sans le remplacer ni retirer son fonctionnement. Écrire auparavant toutes les tâches nécessaires après avoir relu les fichiers et le fonctionnement concernés ; tout remplacement explicitement nécessaire doit être décrit et validé avant son exécution.
- Toujours respecter l’ensemble des règles de rédaction, d’ordre, d’exécution, de vérification et de suivi des tâches, sans exception implicite.
- Avant toute modification, lire les fichiers directement concernés et uniquement les contrats, appels, dépendances, présentateurs ou contrôleurs pertinents pour la tâche, dans l’étendue nécessaire pour comprendre le fonctionnement réel et l’architecture utilisée, sans relire inutilement des fichiers inchangés déjà compris.
- Lire les tests existants lorsqu’une tâche demande de créer, modifier ou exécuter des tests, notamment pour vérifier qu’un fichier ou un scénario équivalent n’existe pas déjà ; ne pas parcourir des tests sans rapport avec l’action à réaliser.
- Ne jamais écrire, modifier ou extrapoler du code sans savoir comment la partie concernée de l’application fonctionne réellement. Si le fonctionnement ou l’architecture ne peut pas être établi avec certitude depuis le projet, arrêter le travail et demander une décision.
- Toujours respecter l’architecture existante du projet : placer les énumérations dans des fichiers d’énumérations sous le dossier Enums approprié, les constantes dans des fichiers de constantes sous le dossier Constants approprié et les fonctions dans des fichiers de fonctions sous le dossier Functions approprié.
- Lorsqu’une énumération, une constante ou une fonction peut être commune, l’écrire une seule fois pour l’usage commun et la placer dans la couche et le dossier communs correspondant à sa portée réelle, sans duplication locale.
- Ne laisser aucun nombre, texte ou autre valeur brute inexpliquée dans le code : toute valeur utilisée par le fonctionnement doit être portée par une constante nommée dans le fichier de constantes approprié.
- Ne laisser aucun texte visible directement dans le code. Tout texte affiché doit utiliser une ressource de localisation placée dans le fichier approprié, même lorsque sa valeur est identique dans toutes les langues ou qu’aucune variation de traduction n’est attendue.
- Lorsqu’un texte visible est ajouté ou modifié, créer ou modifier sa ressource dans la base appropriée puis dans tous les fichiers de langues pris en charge avant d’utiliser cette ressource dans l’interface.
- Les tests intermédiaires doivent être ciblés et rapides. Un petit test créé uniquement pour vérifier ponctuellement une action peut être retiré après cette vérification lorsqu’aucune tâche ne demande de le conserver.
- La création d’un test durable, plus large ou regroupant plusieurs vérifications doit toujours être prévue par une tâche écrite avant la création ou la modification de son fichier.

## Checklist détaillée — Point 1 : écran d’émulation

Cette checklist détaille uniquement le retour du focus du point 1. Dans l’ordre global, ce travail correspond au groupe 3. Les filtres vidéo et les habillages sont détaillés séparément dans les checklists des points 7 et 8 afin de ne pas dupliquer leurs tâches ici. Chaque dernière case constitue une modification atomique qui doit laisser le projet compilable, être vérifiée, puis être cochée avant la suivante.

- [ ] Retour automatique du focus vers l’instance d’émulation ouverte
  - [x] Limiter la restitution du focus à l’instance affichée dans l’onglet actif
    - [x] Transporter la sélection réelle du TabControl jusqu’au contrôleur de machine
      - [x] Dans src/GWGUI.App/Contracts/Machine/MachineControllerOptions.cs, ajouter `Func<bool> IsActive` avant les paramètres facultatifs; dans `OpenMachineAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionMachineFunctions.cs, conserver la référence du `MachineController` créé et fournir une fonction `IsActive` qui compare cette référence à `_machines.SelectedContent`.
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, ajouter le champ `Func<bool> _isActive` et le paramètre de constructeur qui l’initialise; dans le constructeur de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, transmettre `options.IsActive` à ce nouveau paramètre.
    - [x] Centraliser la restitution vers la cible active et courante
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, ajouter RestoreFocus avec exactement deux chemins : retourner lorsque _powered est faux ou lorsque _isActive() est faux; sinon appeler RelativeMouseCapture.Focus(_inputView, _inputHandle). Ne faire aucun appel à Capture, ReleasePointer, SetInputView ou _view.Screen.Focus() dans cette méthode.
  - [x] Rendre le clic de la zone grise au contrôleur d’entrée
    - [x] Raccorder uniquement le fond extérieur à l’écran
      - [x] Dans src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, abonner `MouseLeftButtonDown` de `_view.DisplayHost` dans le constructeur, créer `DisplayHostMouseLeftButtonDown` pour appeler `RestoreFocus` uniquement lorsque `args.OriginalSource` est exactement `_view.DisplayHost`, puis désabonner ce gestionnaire dans `Dispose`.
  - [x] Restituer le focus après les commandes de la barre d’outils
    - [x] Faire transporter l’opération commune par la barre sans casser sa construction
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineCommandBar.cs, ajouter le champ `Action _restoreFocus` et le paramètre de constructeur qui l’initialise; dans le constructeur de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, créer `_input` avant `_commands` et fournir `_input.RestoreFocus` au nouveau paramètre.
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineCommandBar.cs, rendre `RunAsync` non statique et ajouter un bloc `finally` qui appelle `_restoreFocus()` après le `try/catch` existant, sans modifier `Command` ni les actions qui lui sont fournies.
    - [x] Retirer les restitutions particulières remplacées par le chemin commun
      - [x] Dans `TogglePowerAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `if (_session.IsPowered) _video.InputView.Focus()` et laisser inchangées les mises à jour de session, d’entrée, de commandes, de visibilité vidéo et de statut.
      - [x] Dans `ExecuteShortcutAsync` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, ajouter un bloc `finally` qui appelle `_input.RestoreFocus()` après le `try/catch`, sans modifier le `switch`, les actions appelées ni la gestion actuelle des erreurs.
  - [x] Restituer le focus après les commandes des lecteurs
    - [x] Faire transporter l’opération commune jusqu’aux boutons de média sans dupliquer les erreurs
      - [x] Dans src/GWGUI.App/Views/Controls/Emulation/Machine/MachineView.cs, ajouter `Action restoreFocus` à `SetDevices`, `DeviceItem` et `RunAsync`, transmettre ce paramètre à chaque appel intermédiaire et l’appeler dans un `finally` de `RunAsync`; dans `RebuildMediaDevices` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, fournir `_input.RestoreFocus` au nouvel argument de `_view.SetDevices`. Ne modifier ni `InsertMediaAsync`, ni `EjectMediaAsync`, ni le `catch` qui appelle `showError`.
  - [x] Conserver la séquence du plein écran avec la même opération de focus
    - [x] Utiliser la restitution commune après le déplacement de Screen
      - [x] Dans `CompleteHostTransition` de src/GWGUI.App/Controllers/Emulation/Machine/MachineInputController.cs, remplacer uniquement `RelativeMouseCapture.Focus(_inputView, _inputHandle)` par `RestoreFocus`, sans déplacer la lecture de `_restorePointerAfterHostTransition`, la remise à zéro de `_hostTransition` ni la restauration conditionnelle de `_pointerCapture`.
      - [x] Dans `EnterFullscreen` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `_video.InputView.Focus()` et conserver dans le même ordre `BeginHostTransition`, le déplacement de `Screen`, l’affichage et l’activation de la fenêtre, puis `CompleteHostTransition`.
      - [x] Dans `ExitFullscreen` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, supprimer uniquement `_video.InputView.Focus()` et conserver dans le même ordre `BeginHostTransition`, le replacement de `Screen`, la fermeture de la fenêtre, puis `CompleteHostTransition`.
      - [x] Dans `FullscreenContentRendered` de src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs, remplacer uniquement `_video.InputView.Focus()` par `_input.RestoreFocus()` après `_video.FitScreen()`.
  - [ ] Verrouiller chaque comportement par des tests ciblés et rapides
    - [x] Préparer le fichier de tests unique du point 1
      - [x] Créer le fichier vide tests/GWGUI.Tests/MachineFocusTests.cs sans ajouter son contenu dans la même action.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter uniquement les doubles minimaux de `IEmulatedMachine` et `IEmulationInput`, les créations de `MachineView` et les déclencheurs d’événements nécessaires aux scénarios suivants; vérifier que le projet de tests compile avant de cocher cette case.
    - [x] Vérifier la cible commune
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test d’une instance active et allumée qui appelle `RestoreFocus` puis vérifie le focus de la surface WPF courante et l’absence de capture; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui remplace la surface par `SetInputView`, appelle `RestoreFocus` puis vérifie que la nouvelle surface, et non l’ancienne, reçoit le focus; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test d’une instance éteinte qui appelle RestoreFocus puis vérifie que le focus existant ne change pas; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test de deux contrôleurs dont les fonctions `IsActive` renvoient des valeurs opposées, puis vérifier que seul le contrôleur actif déplace le focus; exécuter uniquement ce test avant de cocher la case.
    - [x] Vérifier les deux zones de clic
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui déclenche `MouseLeftButtonDown` avec `DisplayHost` comme source d’origine puis vérifie le retour du focus sans capture; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test qui déclenche le clic existant sur la surface avec la capture autorisée puis vérifie que le comportement de capture reste actif; exécuter uniquement ce test avant de cocher la case.
    - [x] Vérifier les commandes communes
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test de `MachineCommandBar` qui exécute une commande réussie puis une commande en erreur, et vérifie dans les deux cas un appel de restitution ainsi que la transmission de la seule erreur à `showError`; exécuter uniquement ce test avant de cocher la case.
      - [x] Dans tests/GWGUI.Tests/MachineFocusTests.cs, ajouter le test des boutons de média de `MachineView` qui exécute une action terminée sans modification, représentant le retour Annuler, puis une action en erreur, et vérifie dans les deux cas un appel de restitution ainsi que la transmission de la seule erreur à `showError`; exécuter uniquement ce test avant de cocher la case.
    - [ ] Terminer la validation du point
      - [x] Exécuter tous les tests de tests/GWGUI.Tests/MachineFocusTests.cs et corriger uniquement les régressions produites par les fichiers du point 1 avant de cocher cette case.
      - [ ] Exécuter toute la suite tests/GWGUI.Tests/GWGUI.Tests.csproj et corriger uniquement les régressions produites par les fichiers du point 1 avant de cocher cette case.
## Checklist détaillée — Point 2 : identification et enregistrement de la machine modifiée

Cette checklist couvre uniquement les modifications faites par l’utilisateur. Une machine qui possède un fichier est enregistrée directement dans ce fichier. Une machine qui n’en possède pas conserve un brouillon distinct en mémoire pendant toute l’exécution de l’application, jusqu’à l’utilisation du bouton Créer. Les champs de saisie sont pris en compte à la perte du focus ; les sélecteurs, options, cases et actions spécialisées le sont immédiatement.

- [x] Conserver les brouillons des machines non créées pendant l’exécution de l’application
  - [x] Créer le stockage applicatif avant toute utilisation
    - [x] Créer le fichier vide src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs.
    - [x] Modifier src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs pour conserver en mémoire au plus un IEmulationConfiguration par identifiant de module et identifiant de machine.
    - [x] Modifier src/GWGUI.App/Services/Emulation/EmulationConfigurationDraftStore.cs pour permettre la lecture, le remplacement et le retrait du brouillon d’une machine sans écrire de fichier.

- [x] Traiter chaque modification faite par l’utilisateur
  - [x] Créer le traitement commun avant de raccorder les contrôles
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter un traitement commun qui capture dans _configuration les champs génériques, les entrées et le stockage actuellement affichés.
    - [x] Modifier ce traitement dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour remplacer le brouillon de la machine dans EmulationConfigurationDraftStore lorsqu’aucune configuration enregistrée correspondante n’existe.
    - [x] Modifier ce traitement dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler IEmulationModule.SaveConfigurationAsync et signaler ConfigurationSaved lorsque la configuration de la machine existe déjà.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour utiliser le sémaphore existant lors des écritures déclenchées par l’utilisateur et écrire la dernière modification reçue après une écriture déjà en cours.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour exécuter ce traitement par ExecuteAsync afin d’afficher une erreur lorsqu’une écriture échoue.
  - [x] Raccorder les sélecteurs, options et cases
    - [x] Modifier CreateSelection dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun immédiatement après un changement effectué par l’utilisateur.
    - [x] Modifier CreateToggle dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun immédiatement après un changement effectué par l’utilisateur.
    - [x] Modifier CreateSelection et CreateToggle dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour différer le raccordement de leurs gestionnaires utilisateur et y terminer CaptureEditorValues ainsi que la reconstruction demandée par RefreshSettingsOnChange avant l’appel au traitement commun.
    - [x] Modifier ApplySettingsRules dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour raccorder ensuite les gestionnaires utilisateur différés, terminer les règles existantes avant leur appel au traitement commun et ne pas traiter séparément les changements de contrôle produits par ces règles.
  - [x] Raccorder les champs de saisie
    - [x] Modifier CreateField dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus des éditeurs Text, Number et Percentage.
    - [x] Modifier CreatePath dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus du chemin saisi et immédiatement après la sélection réussie d’un fichier.
    - [x] Modifier CreateDirectoryPath dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun à la perte du focus du chemin saisi et immédiatement après la sélection réussie d’un dossier.
  - [x] Raccorder les actions spécialisées
    - [x] Ajouter ConfigurationChanged dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs et le déclencher immédiatement après qu’un utilisateur a appliqué un firmware compatible avec Utiliser.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun lorsque EmulationFirmwareManagementController signale ConfigurationChanged.
    - [x] Ajouter SettingsChanged dans src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs et le déclencher immédiatement après qu’un utilisateur a ajouté, supprimé ou configuré un lecteur ou son média.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appeler le traitement commun lorsque EmulationStorageSettingsController signale SettingsChanged.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour remplacer la sauvegarde particulière des entrées par le traitement commun lorsque EmulationInputSettingsController signale SettingsChanged.

- [x] Charger, créer et supprimer les configurations sans perdre les brouillons
  - [x] Charger la machine demandée depuis la bonne source
    - [x] Modifier ReloadAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour afficher la configuration enregistrée de cette machine lorsqu’elle existe, sinon son brouillon applicatif lorsqu’il existe, sinon les valeurs de base créées par le module.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter ReloadWhenOpenedAsync qui exécute ReloadAsync par ExecuteAsync et conserve la présentation d’erreur existante.
    - [x] Modifier ModuleTabSelectionChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour appeler ReloadWhenOpenedAsync chaque fois que l’utilisateur rouvre un onglet Amiga ou Atari dont la section existe déjà, en conservant le chargement initial par Loaded lors de sa première ouverture.
    - [x] Modifier MachineChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour afficher la configuration enregistrée, le brouillon applicatif ou les valeurs de base de la machine choisie sans réutiliser les valeurs d’une autre machine.
  - [x] Disposer de Common.Create avant de modifier le bouton existant
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/00-Base/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ar-SA/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/cs-CZ/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/da-DK/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/de-DE/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/el-GR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/en-US/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/es-ES/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/fi-FI/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/fr-FR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/he-IL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/hu-HU/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/id-ID/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/it-IT/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ja-JP/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ko-KR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/nb-NO/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/nl-NL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pl-PL/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pt-BR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/pt-PT/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ro-RO/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/ru-RU/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/sv-SE/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/th-TH/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/tr-TR/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/uk-UA/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/vi-VN/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/zh-Hans/Actions.resx.
    - [x] Conserver Common.Create dans src/GWGUI.App/Resources/zh-Hant/Actions.resx.
  - [x] Utiliser le bouton existant uniquement lorsque la configuration n’existe pas
    - [x] Modifier BuildGeneralHeader dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour conserver le bouton local existant, lui affecter Common.Create et le masquer lorsque les configurations chargées contiennent la machine affichée.
    - [x] Modifier SaveAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour retirer le brouillon correspondant uniquement après la réussite de SaveConfigurationAsync, signaler ConfigurationSaved et reconstruire l’éditeur afin de masquer Créer.
  - [x] Revenir à une machine non créée après sa suppression
    - [x] Modifier DeleteSelectedConfigurationAsync dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour demander à la section déjà créée du même module de recharger ses configurations après la suppression réussie.

- [x] Distinguer visuellement les machines possédant une configuration
  - [x] Porter l’existence de la configuration dans chaque choix
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineChoice.cs pour ajouter l’état enregistré de Definition sans modifier DisplayName ni ToString.
    - [x] Modifier la création des choix dans le constructeur, ReloadAsync et RefreshLocalizedContent de src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour établir cet état uniquement depuis les configurations chargées du module.
  - [x] Construire la présentation commune du sélecteur
    - [x] Créer le fichier vide src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs.
    - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs pour définir les couleurs du fond gris clair et du texte vert forêt.
    - [x] Créer le fichier vide src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour créer le DataTemplate qui affiche normalement une machine non créée et applique le fond gris clair, le texte vert forêt et la graisse forte à une machine configurée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour affecter ce DataTemplate à _machines dans la liste ouverte et dans la sélection fermée.

- [x] Afficher la marque et la machine modifiées dans le titre de Paramètres
  - [x] Ajouter le format localisé du titre avant son utilisation
    - [x] Modifier src/GWGUI.App/Resources/00-Base/Options.resx pour créer Options.EmulationMachineTitle avec les paramètres du titre Paramètres, de la marque et de la machine.
    - [x] Modifier src/GWGUI.App/Resources/ar-SA/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/cs-CZ/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/da-DK/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/de-DE/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/el-GR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/en-US/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/es-ES/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/fi-FI/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/fr-FR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/he-IL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/hu-HU/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/id-ID/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/it-IT/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ja-JP/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ko-KR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/nb-NO/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/nl-NL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pl-PL/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pt-BR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/pt-PT/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ro-RO/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/ru-RU/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/sv-SE/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/th-TH/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/tr-TR/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/uk-UA/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/vi-VN/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hans/Options.resx pour ajouter Options.EmulationMachineTitle.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hant/Options.resx pour ajouter Options.EmulationMachineTitle.
  - [x] Faire remonter la machine affichée jusqu’à Paramètres
    - [x] Créer le fichier vide src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs.
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs pour porter le nom localisé du module et le DisplayName de la machine affichée.
    - [x] Ajouter EditingContextChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs et le déclencher après ReloadAsync, MachineChanged et RefreshLocalizedContent.
    - [x] Ajouter EditingContextChanged dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour transmettre le contexte de la section Amiga ou Atari active et l’absence de contexte dans Général, Raccourcis et Configuration.
  - [x] Modifier uniquement le titre de la fenêtre
    - [x] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml et src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour raccorder SelectionChanged de Navigation, écouter EditingContextChanged, afficher Options.Title seul hors de l’éditeur d’une machine et afficher Options.EmulationMachineTitle dans tous les sous-onglets de cette machine.
    - [x] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml.cs pour recalculer le titre après un changement de langue sans modifier le texte des onglets.

- [x] Corriger la présentation des machines possédant une configuration
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire, avant toute correction, la fermeture de l’instance ouverte, la correction des couleurs sur toute la ligne et toute la sélection fermée, la compilation, la vérification visuelle puis la fermeture.
  - [x] Fermer l’instance de GW GUI actuellement ouverte avant de modifier les fichiers utilisés par l’application.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire le déplacement préalable de la palette Compatible vers les constantes visuelles communes et sa compilation avant sa réutilisation par le sélecteur de machines.
  - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/ControlVisualConstants.cs et src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour déplacer les trois couleurs existantes de l’état Compatible vers les constantes visuelles communes, remplacer immédiatement leurs anciennes valeurs locales et conserver exactement le rendu du badge.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier uniquement le déplacement de la palette Compatible.
  - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/EmulationMachineChoiceVisualConstants.cs pour remplacer le gris par le fond vert clair, le texte vert et la bordure verte de la palette Compatible commune.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour retirer le fond limité au texte, conserver le texte en gras pour une machine configurée et créer les styles qui appliquent le fond, le texte et la bordure à toute la ligne déroulée ainsi qu’à toute la sélection fermée.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour appliquer au ComboBox des machines les deux styles créés, sans modifier sa sélection ni son fonctionnement.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette correction visuelle.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md après la première vérification visuelle pour inscrire la correction de la liaison de l’état configuré au contexte de données réel de chaque ComboBoxItem avant de relancer l’application.
  - [x] Modifier CreateItemContainerStyle dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMachineChoiceLayout.cs pour lier directement HasSavedConfiguration depuis EmulationMachineChoice au lieu de rechercher Content.HasSavedConfiguration sur cet objet.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette correction de liaison.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier que chaque machine configurée colore toute sa ligne et toute la sélection fermée en vert clair, sans rectangle gris limité au texte, tandis qu’une machine non configurée conserve la présentation normale.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.

## Checklist détaillée — Point 3 : tableau des configurations

Dans l’ordre général, ce point constitue le groupe 2. Il utilise l’état fiable des configurations établi au point 2 et doit être terminé avant le retour automatique du focus du point 1. La présentation textuelle actuelle est remplacée par un tableau sans sélection de ligne. Seuls le filtre de marque, le crayon, le double-clic et la poubelle produisent une action.

- [x] Préparer les fonctions de présentation communes
  - [x] Créer le fichier commun avant d’y déplacer les fonctions
    - [x] Créer src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs vide.
  - [x] Déplacer entièrement chaque fonction avant de passer à la suivante
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer DisplayValue dans le fichier commun avec exactement les mêmes choix, valeurs de repli et règles de localisation, remplacer ses appels puis supprimer immédiatement sa définition privée d’origine.
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Machine/EmulationMachineEditingContext.cs pour remplacer le record qui hérite de EventArgs par une classe sealed conservant le même constructeur et les mêmes propriétés ModuleDisplayName et MachineDisplayName.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de DisplayValue.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer DefaultNumericValue dans le fichier commun avec exactement les mêmes sources numériques et la même valeur de repli, remplacer son appel puis supprimer immédiatement sa définition privée d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de DefaultNumericValue.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsValuePresentationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer FormatMemorySize dans le fichier commun avec exactement les mêmes seuils, formats et unités, remplacer son appel puis supprimer immédiatement sa définition privée d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de FormatMemorySize.

- [x] Préparer les constantes et le style déjà utilisés
  - [x] Déplacer le glyphe du clavier vers sa portée commune
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationInputSettingsConstants.cs et src/GWGUI.App/Constants/Emulation/EmulationMachineTabConstants.cs pour déplacer la valeur U+E765 dans KeyboardIcon, remplacer immédiatement la valeur littérale de l’onglet Clavier puis ne conserver qu’une seule définition du glyphe.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement du glyphe Clavier.
  - [x] Ajouter le glyphe de modification manquant
    - [x] Modifier src/GWGUI.App/Constants/Controls/Visual/ControlVisualConstants.cs pour ajouter EditGlyph avec la valeur U+E70F déjà utilisée par l’action de modification, sans modifier DeleteGlyph.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier EditGlyph.
  - [x] Déplacer le style d’en-tête vers les ressources globales
    - [x] Modifier src/GWGUI.App/App.xaml et src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml pour déplacer TableHeaderText dans Application.Resources avec FontWeight à SemiBold, VerticalAlignment à Center et Margin à 14,0, laisser InputBindingEditor utiliser la ressource déplacée puis supprimer immédiatement sa déclaration locale.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement complet de TableHeaderText.

- [x] Ajouter les textes visibles du nouveau tableau
  - [x] Créer les ressources de base
    - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour créer Emulation.Configuration.Brand, Emulation.Configuration.Machine, Emulation.Configuration.TotalRam, Emulation.Configuration.Readers, Emulation.Configuration.Peripherals, Emulation.Configuration.Actions et Emulation.Configuration.DeleteConfirm ; DeleteConfirm reçoit uniquement la marque et la machine.
  - [x] Ajouter les sept ressources dans toutes les langues prises en charge
    - [x] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx.
    - [x] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier toutes les ressources ajoutées.

- [x] Créer les données structurées des lignes
  - [x] Créer le contrat avant son contenu
    - [x] Créer src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs vide.
  - [x] Définir uniquement les données nécessaires aux colonnes et aux actions
    - [x] Modifier src/GWGUI.App/Contracts/Emulation/Configurations/EmulationConfigurationTableRow.cs pour porter IEmulationModule, IEmulationConfiguration, le nom localisé de la machine, le CPU, la RAM totale, la liste des glyphes de lecteurs et la liste des glyphes de périphériques.
  - [x] Créer le présentateur avant son contenu
    - [x] Créer src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs vide.
  - [x] Produire Machine, CPU et RAM totale depuis les données structurées
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour créer une ligne depuis IEmulationModule et IEmulationConfiguration, retrouver la machine dans IEmulationModule.Machines et localiser sa DisplayResourceKey sans analyser EmulationConfigurationSummary ni le DisplayName textuel existant.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationHardwareSettingsConstants.cs et src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour déplacer la clé existante Emulation.Cpu.Model dans CpuModelResourceKey, remplacer immédiatement son utilisation actuelle puis ne conserver qu’une seule définition de cette valeur.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le déplacement de CpuModelResourceKey.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour obtenir le CPU depuis le champ visible Emulation.Cpu.Model de l’onglet CPU renvoyé par IEmulationModule.Describe et le formater avec EmulationSettingsValuePresentationFunctions.DisplayValue.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationMemorySettingsConstants.cs pour ajouter ValueUnitSeparator avec un espace unique destiné à séparer la valeur de RAM de son unité dans le tableau.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier ValueUnitSeparator.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour additionner les valeurs numériques des champs visibles de l’onglet RAM avec EmulationSettingsValuePresentationFunctions.DefaultNumericValue, formater le total avec FormatMemorySize et laisser CPU ou RAM vide uniquement en l’absence réelle de donnée correspondante.
  - [x] Produire exactement une icône par lecteur configuré
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour appeler IEmulationStorageSettingsManager.DescribeStorageSettings lorsque le module fournit ce service, parcourir ConfiguredSlots, retrouver chaque périphérique par EmulationMediaSlot dans AvailableDevices et produire un glyphe par occurrence avec FloppyGlyph, HardDiskGlyph, CompactDiscGlyph, CassetteGlyph ou CartridgeGlyph selon EmulationMediaType.
  - [x] Produire exactement une icône par périphérique configuré
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour appeler IEmulationInputSettingsManager.DescribeInputSettings lorsque le module fournit ce service, ajouter KeyboardIcon lorsque Keyboard existe et MouseIcon lorsque Mouse existe.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationInputSettingsConstants.cs pour ajouter NoneControllerResourceKey avec Emulation.Controller.None, KeyboardControllerId avec Keyboard et MouseControllerId avec Mouse afin d’identifier les choix de port sans valeur brute.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier les identifiants de contrôleur.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour résoudre chaque SelectedControllerId dans ControllerChoices, ignorer le choix dont la ressource est Emulation.Controller.None et ajouter pour chaque autre port le glyphe clavier, souris ou manette correspondant au choix.
  - [x] Classer et limiter les lignes
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Configurations/EmulationConfigurationTablePresenter.cs pour classer les lignes par nom de machine avec StringComparer.CurrentCulture et ne produire aucun identifiant technique affichable, ROM, moteur vidéo, format vidéo, état audio ou action de lancement.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le contrat et le présentateur.

- [x] Créer le tableau sans mécanisme de sélection
  - [x] Créer le contrôle avant son contenu
    - [x] Créer src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs vide.
  - [x] Construire les six colonnes et les lignes
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour recevoir des EmulationConfigurationTableRow, utiliser un ItemsControl dans un ScrollViewer et ne créer ni SelectedItem, ni SelectedIndex, ni état visuel de sélection.
    - [x] Créer src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs vide.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour définir dans l’ordre les six clés d’en-tête, TableHeaderTextStyleResource, CellMargin, HeaderSeparatorThickness et RowSeparatorThickness avec les valeurs déjà utilisées par les tableaux existants.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour créer dans l’ordre Machine, CPU, RAM totale, Lecteurs, Périphériques et Actions en utilisant les ressources Emulation.Configuration correspondantes, Emulation.Tab.Cpu, TableHeaderText, CardBrush et BorderBrush.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour afficher Machine, CPU et RAM totale, puis chaque glyphe de lecteur et de périphérique séparément sans nombre, texte permanent, infobulle de port ni information supplémentaire.
  - [x] Ajouter uniquement les trois interactions autorisées
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ajouter EditRequested et DeleteRequested en transmettant directement la EmulationConfigurationTableRow concernée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour créer le bouton crayon avec EditGlyph et le bouton poubelle avec DeleteGlyph dans la colonne Actions.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour envoyer le bouton crayon et le double-clic de ligne vers le même chemin interne qui déclenche EditRequested.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour traiter l’action du bouton poubelle avant DeleteRequested afin qu’un double-clic sur ce bouton ne remonte jamais vers EditRequested.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ne déclencher aucune action lors d’un simple clic ailleurs dans une ligne.
  - [x] Actualiser les textes du contrôle
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour ajouter RefreshLocalizedContent et y reconstruire les six en-têtes avec les ressources de la langue active.
    - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour définir CellMargin avec les quatre côtés attendus par Thickness.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le contrôle avant son raccordement.

- [x] Préparer l’ouverture complète d’une configuration
  - [x] Déplacer entièrement la création différée d’une section de marque
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour déplacer le bloc de création de EmulationModuleSettingsSection depuis ModuleTabSelectionChanged vers GetOrCreateModuleSection, conserver les abonnements ConfigurationSaved et EditingContextChanged, l’ajout dans _moduleSections et l’affectation à TabItem.Content, remplacer immédiatement l’ancien bloc par l’appel à la méthode puis supprimer le bloc d’origine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier que l’ouverture manuelle des onglets de marque utilise GetOrCreateModuleSection et conserve ReloadWhenOpenedAsync.
  - [x] Ajouter l’ouverture explicite de la configuration choisie
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter EditConfigurationAsync recevant IEmulationConfiguration, recharger _saved, retenir exactement la configuration transmise, sélectionner sa machine, fixer _selectedTab à EmulationMachineTab.General, reconstruire tous les sous-onglets et actualiser l’état de l’émulateur installé sans lancer la machine.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier l’entrée explicite de l’éditeur.
  - [x] Ajouter la remise à zéro après une suppression
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter ReloadAfterConfigurationDeletedAsync recevant l’identifiant et la machine supprimés, retirer le brouillon de cette machine uniquement si l’identifiant supprimé est actuellement chargé, puis réutiliser ReloadAsync afin de recharger _saved, reconstruire les valeurs de base de cette machine et faire réapparaître Créer.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour typer configurationId en Guid comme IEmulationConfiguration.Id.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier la remise à zéro ciblée.

- [x] Préparer le nouveau contenu avant de remplacer l’ancienne liste
  - [x] Ajouter les champs du filtre et du tableau
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour ajouter le TextBlock du libellé Marque, le ComboBox de marque, la collection de EmulationModuleListItem des marques configurées, la liste complète de EmulationConfigurationTableRow et EmulationConfigurationTable sans supprimer encore _configurations, _configurationList ni _removeConfiguration.
  - [x] Alimenter le filtre et le tableau pendant le chargement existant
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour construire les nouvelles lignes avec EmulationConfigurationTablePresenter après chaque chargement tout en continuant provisoirement d’alimenter _configurations.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour reconstruire la liste des marques avec uniquement les modules possédant au moins une configuration, conserver la marque choisie si elle existe encore et laisser le ComboBox sans sélection si elle a disparu ou si aucune configuration n’existe.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour afficher uniquement les lignes de la marque choisie et laisser le tableau vide sans marque choisie.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder SelectionChanged du ComboBox à ce filtrage sans créer de sélection dans le tableau.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier les données du nouveau contenu.
  - [x] Raccorder l’ouverture de ligne
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ajouter EditConfigurationAsync recevant EmulationConfigurationTableRow, obtenir la section de row.Module par GetOrCreateModuleSection, appeler son EditConfigurationAsync avec row.Configuration puis sélectionner le TabItem correspondant une fois la configuration chargée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder EditRequested du tableau à cette méthode unique afin que le crayon et le double-clic ne puissent pas diverger.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le raccordement de l’ouverture.
  - [x] Raccorder la suppression de ligne
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ajouter DeleteConfigurationAsync recevant EmulationConfigurationTableRow et ouvrir une MessageBox Oui/Non utilisant Common.Delete comme titre et Emulation.Configuration.DeleteConfirm comme message avec uniquement la marque localisée et la machine localisée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour ne rien supprimer ni recharger lorsque la réponse n’est pas Oui, appeler row.Module.DeleteConfigurationAsync avec row.Configuration.Id uniquement après Oui et présenter toute erreur avec ControlErrorPresenter sans retirer la ligne lorsque la suppression échoue.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour recharger après une suppression réussie, conserver la marque tant qu’elle possède une ligne, laisser le ComboBox et le tableau sans sélection après sa dernière ligne sans choisir une autre marque, puis appeler ReloadAfterConfigurationDeletedAsync sur la section correspondante lorsqu’elle existe déjà.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour raccorder DeleteRequested à DeleteConfigurationAsync sans utiliser SelectedItem.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs pour importer System.Windows utilisé par la MessageBox de confirmation.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le raccordement de la suppression.
  - [x] Raccorder l’actualisation localisée
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour importer EmulationConfigurationTablePresenter utilisé par RefreshLocalizedContent.
    - [x] Modifier RefreshLocalizedContent dans src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour actualiser le libellé Marque, appeler EmulationConfigurationTable.RefreshLocalizedContent et reconstruire les marques et les lignes localisées en conservant la marque choisie si elle existe encore sans en sélectionner une autre.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier l’actualisation localisée du nouveau contenu.

- [x] Remplacer définitivement l’ancienne liste par le nouveau contenu
  - [x] Effectuer le remplacement complet dans une seule tâche
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionLayoutFunctions.cs, src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSectionConfigurationFunctions.cs et src/GWGUI.App/Views/Controls/Emulation/Options/OptionsEmulationSection.cs pour ajouter le libellé Marque, son ComboBox et EmulationConfigurationTable dans BuildConfigurationsTab, puis supprimer immédiatement l’ancienne ListBox, le bouton Supprimer global, RemoveConfiguration, DeleteSelectedConfigurationAsync, l’alimentation de _configurations, les champs _configurations, _configurationList et _removeConfiguration, leur gestionnaire SelectionChanged, l’ancienne actualisation de Common.Delete et les directives using devenues inutiles.
    - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier le remplacement complet et la suppression de tout le fonctionnement fondé sur une ligne sélectionnée.

- [x] Espacer uniformément les icônes des lecteurs et des périphériques
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire, avant toute correction, l’ajout d’un espacement commun de 8 pixels, son application aux deux colonnes, la compilation, la vérification visuelle puis la fermeture.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationConfigurationTableConstants.cs pour ajouter un espacement horizontal de 8 pixels entre deux icônes d’une même cellule.
  - [x] Modifier GlyphCell dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationConfigurationTable.cs pour appliquer cet espacement entre les icônes sans marge extérieure supplémentaire, afin que Lecteurs et Périphériques utilisent exactement la même règle.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cet espacement.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier dans le tableau Configuration que toutes les icônes multiples de Lecteurs et de Périphériques possèdent le même espacement, sans modifier le nombre ni l’ordre des icônes.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.

## Checklist détaillée — Point 4 : destination des ROM détectées

Cette checklist réalise la demande fonctionnelle décrite dans la section 5. Elle conserve la liste et le bouton Utiliser existants. La destination affichée provient du même identifiant de champ que celui consommé par Utiliser ; l’application ne maintient aucune seconde correspondance.

- [x] Inscrire les deux décisions d’affichage encore manquantes avant de modifier le code
  - [x] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 5, pour remplacer le nombre maximal de caractères restant à fixer par la valeur validée et préciser si l’ellipse est comprise dans cette limite.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 5, pour inscrire la position validée de Destination par rapport au nom de la ROM et à Compatibilité.

- [x] Faire porter la destination par le résultat commun du scan
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationFirmwareCandidate.cs pour ajouter l’identifiant optionnel du champ de destination à la ROM détectée.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour renseigner cet identifiant depuis le type déjà obtenu par AmigaFirmwareCatalog, avec KickstartPath, ExtendedRomPath ou RomKeyPath, et le laisser vide lorsqu’aucun de ces champs ne correspond.
  - [x] Modifier UseFirmware dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour utiliser l’identifiant porté par EmulationFirmwareCandidate afin de choisir le champ à remplacer et supprimer la seconde inspection actuellement réalisée uniquement pour retrouver cette destination.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour renseigner SystemFirmware lorsque la ROM détectée possède une destination pour la machine affichée et laisser l’identifiant vide sinon.
  - [x] Modifier UseFirmware dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour vérifier et consommer ce même identifiant avant d’appliquer la sélection Atari existante, sans ajouter une autre table de routage.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par l’ajout de la destination au contrat commun.

- [x] Transmettre le module nécessaire à la résolution du libellé
  - [x] Modifier le constructeur de src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour recevoir le IEmulationModule déjà détenu par EmulationModuleSettingsSection.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour placer le raccordement de EmulationModuleSettingsSection avant l’utilisation du nouveau constructeur et réunir dans une seule tâche la résolution et la transmission du libellé après l’extension de FirmwareRow.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour transmettre _module à EmulationFirmwareManagementController sans changer les raccordements de ConfigurationChanged.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour renommer ce groupe selon les actions qu’il contient maintenant, avant de le cocher.

- [x] Ajouter la cellule informative à la ligne existante
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour ajouter la limite de caractères validée et uniquement les dimensions nécessaires à la colonne validée.
  - [x] Modifier FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour recevoir le libellé de destination, le limiter avec une ellipse selon la décision inscrite et l’afficher comme texte simple à la position validée, dans la présentation de la compatibilité existante.
  - [x] Modifier RefreshAsync dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour rechercher DestinationFieldId dans les champs retournés par IEmulationModule.Describe pour la machine et la configuration affichées, localiser directement LabelResourceKey, transmettre ce texte à FirmwareRow et transmettre un texte vide si l’identifiant est absent ou introuvable, sans modifier le nom, la version, la compatibilité, le chemin ou l’ordre des ROM.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la nouvelle cellule.

- [x] Corriger les écarts constatés dans l’affichage réel avant de reprendre la vérification
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire, avant toute correction, la correction du libellé Kickstart, de la présentation et de la largeur de Destination, la compilation puis la reprise de la vérification dans une nouvelle exécution.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx et chaque src/GWGUI.App/Resources/<langue>/Emulation.resx pour ajouter une clé de ressource Kickstart dont la valeur visible reste Kickstart dans toutes les langues prises en charge.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Constants/AmigaSettingsDescriptionFunctionsConstants.cs et src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour remplacer le texte brut Kickstart utilisé comme LabelResourceKey par la nouvelle clé de ressource, afin que le champ existant et Destination affichent tous deux Kickstart sans crochets.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour remplacer la largeur de Destination copiée depuis Compatibilité par uniquement l’identifiant de groupe nécessaire à une largeur partagée calculée depuis le contenu.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire la copie préalable de la construction du badge Compatibilité dans une fonction commune et sa compilation avant le remplacement de l’ancien bloc.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour copier la construction actuelle du Border de Compatibilité dans une fonction FirmwareBadge recevant le texte et les couleurs, sans retirer ni remplacer le bloc existant.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier uniquement l’ajout de FirmwareBadge avant son utilisation.
  - [x] Modifier FirmwareSettingsPage et FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour partager automatiquement la largeur de Destination entre les lignes, rendre au nom de ROM l’espace restant, remplacer le bloc Compatibilité par FirmwareBadge puis afficher Destination avec la même fonction et les couleurs de compatibilité de la ligne.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ces corrections d’affichage.

- [x] Corriger la largeur des deux badges après le constat visuel
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire la fermeture de l’instance affichée, la largeur identique de Compatibilité et Destination, leur alignement à droite avec un petit espacement, l’espace restant réservé au nom, la compilation et la nouvelle vérification visuelle.
  - [x] Fermer l’instance de GW GUI utilisée pour constater cette disposition.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour remplacer les largeurs distinctes de Compatibilité et Destination par un seul groupe de largeur partagée entre les deux badges et ajouter uniquement l’espacement validé entre eux.
  - [x] Modifier FirmwareRow et FirmwareBadge dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour laisser la colonne du nom prendre tout l’espace restant, placer à droite deux colonnes automatiques dans le même groupe de largeur, étirer et centrer chaque badge dans sa colonne et conserver le petit espacement entre eux.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette disposition.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et ouvrir Options > Émulation > Amiga > ROM.
  - [x] Capturer uniquement la fenêtre Options et vérifier que le nom de ROM utilise l’espace restant tandis que Compatibilité et Destination ont exactement la même largeur, restent à droite et sont séparées par un petit espace.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification visuelle.
- [x] Restaurer le libellé Atari et supprimer le redimensionnement de la fenêtre Options
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire la fermeture de l’instance affichée, la restauration immédiate du libellé Atari, la désactivation du redimensionnement, la compilation et la vérification visuelle.
  - [x] Fermer l’instance de GW GUI affichée pendant ce constat.
  - [x] Restaurer dans src/GWGUI.App/Resources/fr-FR/Emulation.resx la valeur exacte ROM système pour Emulation.Firmware.Rom.System.
  - [x] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml pour remplacer ResizeMode=CanResizeWithGrip par ResizeMode=NoResize sans modifier Width, Height, MinWidth ni MinHeight.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ces deux corrections.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build, ouvrir Options > Émulation > Atari > ROM et vérifier que le titre et le champ affichent de nouveau ROM système et que la fenêtre ne possède plus de poignée ni de commande de redimensionnement.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
- [x] Restaurer l’identification TOS dans le nom des ROM Atari reconnues
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire le préfixe TOS devant la version d’une ROM TOS reconnue, la conservation du nom complet pour une ROM non reconnue, la compilation et la vérification visuelle dans l’application.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour afficher TOS suivi de la version lorsqu’une ROM TOS est reconnue et conserver Path.GetFileName(scanned.Path) lorsqu’elle n’est pas reconnue.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette modification.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build, ouvrir Options > Émulation > Atari > ROM et vérifier que les quatre ROM reconnues affichent TOS devant leur version au lieu du seul numéro.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
- [x] Vérifier le fonctionnement demandé avant de terminer le point
  - [x] Modifier docs/tasks/interface/emulation-improvements.md avant la nouvelle exécution pour séparer chaque cas vérifié, inscrire les fichiers et le libellé temporaires nécessaires aux données absentes, puis inscrire leur suppression ou restauration et la compilation finale.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour remplacer les fausses données de vérification prévues par les vraies données retrouvées : C:/Users/overt/Downloads/Recalbox_10.0.8_BIOS_Pack/rom.key et les quatre ROM TOS déjà présentes dans %APPDATA%/GW GUI/Emulation/Machines/Atari/Firmware/ST.
  - [x] Copier temporairement la vraie clé C:/Users/overt/Downloads/Recalbox_10.0.8_BIOS_Pack/rom.key vers %APPDATA%/GW GUI/Emulation/Machines/Amiga/Firmware/rom.key, uniquement si la cible n’existe pas, afin que le scan Amiga puisse la détecter sans modifier le fichier source.
  - [x] Modifier temporairement uniquement la valeur de Emulation.Firmware.Rom.System dans src/GWGUI.App/Resources/fr-FR/Emulation.resx de ROM système vers ROM système particulièrement longue pour vérifier la limite de 20 caractères et l’ellipse, sans modifier sa clé.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore afin d’intégrer uniquement le libellé temporaire nécessaire à cette vérification.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et ouvrir Options > Émulation.
  - [x] Vérifier dans Amiga > ROM qu’une Kickstart affiche Kickstart après Compatibilité, sans crochets, et que Utiliser renseigne le champ Kickstart sur une machine sans configuration enregistrée.
  - [x] Vérifier dans une machine Amiga CDTV ou CD32 qu’une ROM étendue réelle affiche ROM étendue après Compatibilité et correspond au champ ROM étendue.
  - [x] Vérifier dans une machine Amiga sans configuration enregistrée que rom.key affiche Clé ROM après Compatibilité et que Utiliser renseigne le champ Clé ROM.
  - [x] Vérifier dans une machine Atari ST sans configuration enregistrée qu’une des quatre vraies ROM TOS compatibles affiche le libellé système tronqué à 20 caractères avec une ellipse après Compatibilité et que Utiliser renseigne le champ système.
  - [x] Vérifier dans la même machine Atari ST que toute vraie ROM TOS incompatible laisse Destination vide et Utiliser désactivé lorsqu’elle est sélectionnée ; si les quatre ROM possèdent une destination pour le modèle affiché, constater explicitement que ce cas ne peut pas être vérifié avec les données réelles au lieu de fabriquer une ROM.
  - [x] Dans la même exécution, vérifier que les badges Destination utilisent la même présentation et les mêmes couleurs que Compatibilité, que le nom et la version des ROM disposent de l’espace restant, et que la sélection et le bouton Utiliser conservent leur comportement.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
  - [x] Supprimer uniquement la copie temporaire %APPDATA%/GW GUI/Emulation/Machines/Amiga/Firmware/rom.key créée pour cette vérification, sans modifier la vraie clé source ni les ROM Atari existantes.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier l’état final après suppression de tous les artefacts temporaires.

## Checklist détaillée — Point 5 : aides contextuelles sur les champs

Cette checklist réalise la demande fonctionnelle décrite dans la section 4. Les aides concernent uniquement les champs explicitement validés dans les éditeurs Amiga et Atari. ExplanationResourceKey devient la clé de l’aide courte ; une seconde clé distincte transporte l’aide concise au clic.

- [x] Fixer le périmètre et le contenu avant de créer l’interface
  - [x] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 4, pour ajouter un tableau des champs visibles provenant de src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs, src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs et des champs fixes construits par l’application, en excluant les boutons et titres.
  - [x] Modifier le tableau de la section 4 dans docs/tasks/interface/emulation-improvements.md pour marquer uniquement les champs dont le libellé ne suffit pas, après validation de leur présence ou de leur absence d’aide ; ne pas prévoir d’aide pour le sélecteur de périphérique physique dont la suppression est demandée au point 6.
  - [x] Modifier le tableau de la section 4 dans docs/tasks/interface/emulation-improvements.md pour inscrire, pour chaque champ retenu, la clé d’aide courte, son texte d’une ligne, la clé d’aide concise et son texte expliquant uniquement le rôle, les choix et leurs différences utiles.
  - [x] Modifier la section 4 dans docs/tasks/interface/emulation-improvements.md pour inscrire la présentation validée du post-it, notamment ses dimensions maximales, son placement et ses couleurs, afin qu’aucune valeur visuelle ne soit choisie pendant l’implémentation.

- [x] Étendre les contrats communs avant de modifier les mises en page
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationSettingsField.cs pour conserver ExplanationResourceKey comme clé optionnelle de l’aide courte et ajouter DetailedExplanationResourceKey comme clé optionnelle de l’aide concise.
  - [x] Modifier src/GWGUI.App/Contracts/Emulation/Settings/EmulationSettingsControlField.cs pour transporter le libellé, le contrôle, l’aide courte localisée et l’aide concise localisée, tout en autorisant l’absence des deux aides.
  - [x] Modifier src/GWGUI.App/Contracts/Views/Emulation/Settings/EmulationCpuSettingsContent.cs pour transporter des EmulationSettingsControlField pour les champs CPU actuellement séparés, sans intégrer le résumé du processeur à un champ d’aide.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par l’extension de ces contrats.

- [x] Créer le libellé réutilisable avant de remplacer les libellés actuels
  - [x] Créer le fichier vide src/GWGUI.App/Constants/Emulation/EmulationSettingsFieldHelpConstants.cs.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationSettingsFieldHelpConstants.cs pour définir uniquement les dimensions, espacements et couleurs validés du post-it.
  - [x] Créer le fichier vide src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour reproduire le TextBlock actuel lorsque les deux aides sont absentes et ne créer aucune icône dans ce cas.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour afficher immédiatement après le libellé une icône permanente utilisant ControlVisualConstants.InformationGlyph lorsque les deux aides sont présentes, avec uniquement sa taille visible comme zone cliquable.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour affecter l’aide courte à une infobulle sans retour à la ligne ni défilement, visible seulement pendant le survol.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour ouvrir au clic un Popup de type post-it contenant le libellé et l’aide concise, selon les valeurs validées, et activer le défilement uniquement lorsque le contenu dépasse ses dimensions maximales.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationSettingsFieldLabel.cs pour fermer ce Popup sur toute touche ou sur le clic suivant, sans le fermer pendant le clic d’ouverture, puis détacher tous ses gestionnaires lors de la fermeture et de Unloaded.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ce contrôle.

- [x] Faire passer les champs décrits par les modules par un seul chemin
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour ajouter CreateControlField, qui crée le contrôle existant, localise LabelResourceKey et les deux clés d’aide lorsqu’elles existent, puis retourne EmulationSettingsControlField.
  - [x] Modifier AddBlocks dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour utiliser CreateControlField sans modifier l’ordre, les colonnes, la visibilité ou les contrôles des blocs.
  - [x] Modifier BuildCpuSettingsTab et BuildMemorySettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleHardwareSettingsSection.cs pour utiliser CreateControlField sans modifier les choix, les règles, les résumés ni le calcul de RAM totale.
  - [x] Modifier BuildInputSettingsTab dans src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleInputSettingsSection.cs pour utiliser CreateControlField sans modifier les associations ni leur enregistrement.

- [x] Remplacer les libellés des mises en page par le contrôle commun
  - [x] Modifier CompactForm dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationSettingsLayout.cs pour recevoir des EmulationSettingsControlField et construire leurs libellés avec EmulationSettingsFieldLabel, puis conserver une surcharge sans aide pour les appelants hors des éditeurs de machine.
  - [x] Modifier SettingsFields et SettingsFieldGrid dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationHardwareSettingsLayout.cs pour recevoir des EmulationSettingsControlField, utiliser EmulationSettingsFieldLabel et lier sa visibilité à celle du contrôle correspondant.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationCpuSettingsLayout.cs pour consommer les EmulationSettingsControlField de EmulationCpuSettingsContent sans modifier les cartes Processeur, Compatibilité et Accélération.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationMemorySettingsLayout.cs pour transmettre les EmulationSettingsControlField sans perdre les aides ni modifier les cadres de mémoire.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationInputSettingsLayout.cs pour transmettre les EmulationSettingsControlField de la souris et des options analogiques sans modifier les tableaux d’associations.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md après validation du tableau pour ajouter à cet emplacement une sous-tâche distincte, nommant son fichier, pour chaque champ fixe approuvé qui ne passe pas encore par EmulationSettingsControlField ; n’effectuer aucune modification de ce champ avant l’ajout de sa sous-tâche.
    - Résultat du tableau validé : aucun champ fixe construit par l’application n’est approuvé pour recevoir une aide ; aucune sous-tâche de modification d’un champ fixe n’est donc ajoutée.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par le remplacement des libellés.

- [x] Ajouter les paires de textes validées dans toutes les ressources avant de les utiliser
  - [x] Examiner tous les catalogues de src/GWGUI.App/Resources/00-Base et retirer de chaque catalogue localisé les clés dont la valeur est un nom propre, un modèle ou un identifiant technique invariant, notamment les modèles de machines, sans supprimer ces clés de 00-Base.
    - Résultat : 201 clés invariantes réparties dans 8 catalogues restent uniquement dans 00-Base ; 5829 entrées localisées ont été retirées des 29 langues.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour ajouter exactement les clés et textes validés dans le tableau de la section 4.
      - [x] Modifier src/GWGUI.App/Resources/ar-SA/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/cs-CZ/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/da-DK/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/de-DE/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/el-GR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/en-US/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/es-ES/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/fi-FI/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/fr-FR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/he-IL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/hu-HU/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/id-ID/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/it-IT/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/ja-JP/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/ko-KR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/nb-NO/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/nl-NL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/pl-PL/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/pt-BR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/pt-PT/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/ro-RO/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/ru-RU/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/sv-SE/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/th-TH/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/tr-TR/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/uk-UA/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/vi-VN/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/zh-Hans/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
      - [x] Modifier src/GWGUI.App/Resources/zh-Hant/Emulation.resx pour ajouter exactement les mêmes clés avec les deux textes traduits.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour affecter les deux clés uniquement aux champs Amiga approuvés dans le tableau.
  - [x] Modifier src/GWGUI.Emulation.Atari/Functions/AtariSettingsDescriptionFunctions.cs pour affecter les deux clés uniquement aux champs Atari approuvés dans le tableau, sans réutiliser les explications de compatibilité propres à Atari.
  - [x] Réaliser dans l’ordre chaque sous-tâche de champ fixe ajoutée à docs/tasks/interface/emulation-improvements.md afin de transporter exactement les deux clés approuvées, sans étendre l’aide à un autre élément.

- [x] Vérifier les ressources et le comportement avant de terminer le point
  - [x] Exécuter un contrôle de parité des clés d’aide entre src/GWGUI.App/Resources/00-Base/Emulation.resx et les 29 fichiers de langue, puis corriger uniquement les clés absentes ou supplémentaires créées par ce point.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les ressources et les clés d’aide.
  - [x] Corriger le post-it observé pendant la vérification : empêcher le contenu de reprendre la police d’icônes afin que le texte reste lisible, l’ouvrir sous le libellé sans masquer le champ de saisie ou le sélecteur, et ne le replier au-dessus que si l’espace inférieur est insuffisant.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore après cette correction et corriger uniquement les erreurs qu’elle introduit.
  - [x] Fermer l’instance de GW GUI ayant révélé les défauts du post-it avant de modifier de nouveau son implémentation.
  - [x] Remplacer le Popup séparé par un post-it intégré à la fenêtre Options, visuellement jaune et sans bande noire, placé sous le champ associé sans le masquer, maintenu entièrement dans les limites de la fenêtre et replié au-dessus uniquement si nécessaire.
  - [x] Rendre l’aide courte visible immédiatement au survol et rendre chaque clic sur l’icône fiable pour ouvrir ou fermer le post-it.
  - [x] Relire le comportement réel de chaque champ approuvé et réécrire entièrement ses aides courte et détaillée dans 00-Base avec un texte exact, neutre et réutilisable, sans nom de machine ou d’émulateur.
  - [x] Reporter exactement les aides courte et détaillée réécrites dans le tableau de la section 4 de docs/tasks/interface/emulation-improvements.md.
  - [x] Répercuter les textes d’aide corrigés dans les 29 langues avec le traducteur IA du dépôt, sans traduire les noms propres, modèles et identifiants techniques conservés uniquement dans 00-Base.
  - [x] Contrôler les 106 aides dans les 29 langues, retirer tout caractère de remplacement ou reste de protection technique, puis corriger manuellement les formulations françaises inexactes avant la compilation.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore après ces corrections et corriger uniquement les erreurs qu’elles introduisent.
  - [x] Fermer l’instance de GW GUI utilisée pour constater que le post-it intégré recouvre encore le champ de la ligne suivante.
  - [x] Fermer l’instance de GW GUI utilisée pour constater l’extrapolation qui déplace la mise en page.
  - [x] Retirer l’insertion de ligne, puis afficher dans le dialogue Options un post-it jaune flottant de taille fixe 380 × 240, sous l’icône et sous le bord du champ associé, entièrement contraint à Options, sans pousser ni masquer ce champ, avec une barre de défilement uniquement lorsque le texte dépasse.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore après ce retour au comportement demandé et corriger uniquement les erreurs qu’il introduit.
  - [x] Lire les erreurs produites par l’instance actuelle de GW GUI et identifier précisément leur cause avant toute nouvelle modification.
  - [x] Fermer l’instance de GW GUI après la lecture de ses erreurs.
  - [x] Corriger uniquement les erreurs relevées, réduire la taille fixe du post-it et lui donner un effet de papier autocollant sans bande noire ni sortie hors du dialogue Options.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore après ces corrections et corriger uniquement les erreurs qu’elles introduisent.
  - [x] Fermer l’instance de développement de GW GUI avant de produire le paquet Debug demandé.
  - [x] Exécuter scripts/build.ps1 -Configuration Debug et produire build/Debug/GW GUI/gwgui.exe pour le test manuel.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier chaque champ approuvé dans les onglets Amiga et Atari : icône toujours visible, aide courte d’une ligne au survol et post-it au clic.
  - [x] Dans la même exécution, vérifier qu’une touche et le clic suivant ferment le post-it, que le défilement n’apparaît qu’en cas de dépassement et qu’aucune icône n’est présente sur un bouton ou un titre.
  - [x] Dans la même exécution, vérifier au minimum le français, l’anglais et une langue de droite à gauche, puis vérifier que le changement de langue actualise le libellé, l’infobulle et le post-it.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.

## Checklist détaillée — Point 6 : associations et visualisation des manettes et joysticks

Cette checklist adapte le ControllerVisualizer déjà utilisé dans l’onglet général Manettes. Elle ne crée aucun second visualiseur. Les identifiants des périphériques émulés et de leurs commandes restent ceux fournis par AmigaInputSettingsFunctions et AtariInputSettingsFunctions.

- [x] Inscrire les décisions et l’inventaire nécessaires avant de créer des images ou des zones
  - [x] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 6, pour ajouter un tableau de toutes les valeurs EmulationControllerChoice réellement produites par src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs, avec les machines concernées et leurs InputBindingDefinition.
  - [x] Modifier le tableau de la section 6 dans docs/tasks/interface/emulation-improvements.md après validation pour identifier les périphériques basiques à réaliser maintenant et laisser les autres comme ajouts ultérieurs, sans inventer de périphérique absent des deux listes.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire, pour chaque périphérique réellement produit par une DLL, les VisualId compatibles, le VisualId par défaut et, lorsqu’il existe déjà, le nom exact de l’image présente dans src/GWGUI.App/Assets/Controllers avec son modèle matériel.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour préciser que les profils portent des rôles visuels neutres et typés, puis que chaque DLL associe ces rôles uniquement aux identifiants de commandes de ses propres InputBindingDefinition.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément, pour chaque EmulationControllerChoice réellement produit, la correspondance entre ses rôles visuels et les identifiants exacts de commandes produits par sa DLL.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `quickshot` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `quickshot-deluxe` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `quickshot-ii-turbo` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `competition-pro-5000` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `zipstik-super-pro` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones des profils `konix-speedking-left-hand` et `konix-speedking-right-hand` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `konix-speedking-analog` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `suncom-tac-2` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `powerplay-cruiser` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `suzo-the-arcade-turbo` en pourcentage et avec leur rôle visuel typé.
  - [x] Inspecter src/GWGUI.App/Assets/Controllers/advanced-gravis-gamepad.png, inscrire dans la section 6 son exclusion des profils disponibles tant que le modèle exact n’est pas remplacé et validé, et ne créer aucune zone pour l’image non conforme.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `commodore-cd32` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `competition-pro-cd32` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `atari-cx40` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `atari-5200-controller` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `atari-7800-pro-line-cx24` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `atari-7800-control-pad-europe` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `atari-jaguar-controller` en pourcentage et avec leur rôle visuel typé.
  - [x] Modifier la section 6 dans docs/tasks/interface/emulation-improvements.md pour inscrire séparément les zones du profil `atari-jaguar-pro-controller` en pourcentage et avec leur rôle visuel typé.

- [x] Séparer l’état visuel des données GameInput sans changer le visualiseur général
  - [x] Créer le fichier vide src/GWGUI.App/Enums/Input/ControllerVisualControl.cs.
  - [x] Modifier src/GWGUI.App/Enums/Input/ControllerVisualControl.cs pour déclarer uniquement les contrôles généraux effectivement consommés par le ControllerVisualizer existant, sans nom écrit en chaîne brute.
  - [x] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerVisualState.cs.
  - [x] Modifier src/GWGUI.App/Contracts/Input/ControllerVisualState.cs pour transporter par des propriétés typées les valeurs numériques et les états actifs du visualiseur général, ainsi que les valeurs des commandes émulées indexées uniquement par les identifiants fournis par les profils et les InputBindingDefinition, sans contrôle WPF ni nom d’axe écrit en chaîne brute.
  - [x] Modifier src/GWGUI.App/Contracts/Input/ControllerVisualState.cs pour distinguer les états standard des états résolus par libellé, mémoriser la présence des états Gamepad, volant, vol et arcade, et transporter la première direction de commutateur afin de préserver les priorités et replis actuels.
  - [x] Compléter src/GWGUI.App/Enums/Input/ControllerVisualControl.cs et src/GWGUI.App/Contracts/Input/ControllerVisualState.cs avec les commandes C/Z et un ensemble de directions du premier commutateur afin de conserver les diagonales sans dépendre des enums GameInput.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualInput.cs pour convertir GameInputLiveState vers ControllerVisualState, préserver exactement les priorités et replis actuels entre états standard et contrôles bruts, puis lire uniquement les propriétés typées de cet état commun.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour conserver sans changement les propriétés Model et State de l’onglet général, convertir State par ControllerVisualInput et permettre à l’éditeur d’émulation de fournir directement un ControllerVisualState sans remplacer le chemin existant.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la séparation de l’état visuel.
- [x] Décrire les images et zones en pourcentage dans le visualiseur existant
  - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs.
  - [x] Modifier src/GWGUI.Emulation/Enums/EmulationControllerVisualControl.cs pour déclarer uniquement les rôles visuels neutres utilisés par les profils validés, sans identifiant de module ni texte affiché.
  - [x] Créer le fichier vide src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs.
  - [x] Modifier src/GWGUI.Emulation/Constants/EmulationControllerVisualIds.cs pour centraliser les VisualId neutres des modèles matériels, y compris ceux préparés pour de futurs modules, sans nom de module ni texte affiché.
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationControllerChoice.cs pour transporter la liste des VisualId compatibles déclarée par la DLL, son VisualId par défaut et la correspondance typée entre rôles visuels et identifiants de ses InputBindingDefinition, sans dépendre de WPF ni de l’existence d’une image dans l’application.
  - [x] Créer le fichier vide src/GWGUI.Emulation/Constants/EmulationControllerCommandIds.cs.
  - [x] Modifier src/GWGUI.Emulation/Constants/EmulationControllerCommandIds.cs pour centraliser uniquement les identifiants de commandes communs réellement utilisés par les profils, sans texte visible ni identifiant de module.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Constants/AmigaInputSettingsFunctionsConstants.cs, src/GWGUI.Emulation.Atari/Constants/AtariInputSettingsFunctionsConstants.cs et src/GWGUI.Emulation.Atari/Constants/AtariControllerConstants.cs pour réutiliser les constantes communes correspondant exactement à leurs valeurs actuelles, sans modifier les InputBindingDefinition produites.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour déclarer les VisualId compatibles et la correspondance entre rôles visuels et commandes de chaque EmulationControllerChoice réellement produit, utiliser QuickShot par défaut pour leurs types Joystick et ne pas déclarer un visuel propre à une console absente.
  - [x] Créer le fichier vide src/GWGUI.App/Enums/Input/ControllerVisualZoneShape.cs.
  - [x] Modifier src/GWGUI.App/Enums/Input/ControllerVisualZoneShape.cs pour déclarer uniquement les formes effectivement validées dans le tableau de la section 6.
  - [x] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerVisualZone.cs.
  - [x] Modifier src/GWGUI.App/Contracts/Input/ControllerVisualZone.cs pour porter un EmulationControllerVisualControl neutre, la forme et les coordonnées en pourcentage propres à l’image, sans identifiant de commande propre à un module.
  - [x] Créer le fichier vide src/GWGUI.App/Contracts/Input/ControllerArtworkProfile.cs.
  - [x] Modifier src/GWGUI.App/Contracts/Input/ControllerArtworkProfile.cs pour porter l’image et la liste de ControllerVisualZone sans dupliquer le rendu.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs pour conserver le catalogue des ControllerVisualModel actuels, exposer les profils réellement disponibles par VisualId et retourner uniquement l’intersection entre ce catalogue et les VisualId compatibles déclarés par la DLL.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualizer.cs pour afficher un ControllerArtworkProfile avec le même calcul de redimensionnement que les images existantes et exposer le survol et le clic de ses zones.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour dessiner les halos des profils avec les fonctions communes déjà utilisées par les modèles généraux et aligner les zones depuis leurs pourcentages.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les profils et zones.

- [x] Enregistrer le choix du visuel de chaque port sans modifier le périphérique émulé
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationControllerPort.cs pour transporter un VisualId facultatif après les données existantes, sans modifier leur ordre ni leur valeur.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Contracts/AmigaControllerBinding.cs et src/GWGUI.Emulation.Atari/Contracts/AtariControllerBinding.cs pour enregistrer un VisualId facultatif à la fin de chaque contrat afin que les anciennes configurations restent lisibles.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour transporter VisualId entre la configuration du module et EmulationControllerPort sans modifier le type, DeviceId, les associations ni DeadZonePercent.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour créer le sélecteur de visuel du port et conserver séparément le type émulé, le VisualId sélectionné et les associations.
  - [x] Modifier src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs pour transporter le sélecteur de visuel avec les contrôles du port.
  - [x] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour remplir le sélecteur avec l’intersection des VisualId compatibles déclarés par la DLL et des profils présents dans ControllerArtworkCatalog, restaurer le VisualId enregistré ou le défaut déclaré par la DLL, conserver le choix dans l’état d’édition courant et le transmettre à Apply sans modifier les associations.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx pour ajouter les noms invariants des modèles matériels disponibles, sans les ajouter aux fichiers de langues.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx et tous les fichiers src/GWGUI.App/Resources/*/Emulation.resx pris en charge pour ajouter uniquement le libellé traduisible du sélecteur de visuel.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par le transport et l’enregistrement du VisualId.

### Backlog non bloquant — visuels matériels supplémentaires

Les tâches d’images ci-dessous sont conservées pour la reprise ultérieure de la bibliothèque de périphériques. Elles ne font pas partie de l’ordre d’exécution actuel du point 6, qui utilise uniquement les images déjà présentes et validées.

- [ ] Ajouter une image réaliste validée pour chaque périphérique supplémentaire
  - [x] Ajouter dans cette checklist, avant toute création, une sous-tâche Créer distincte donnant le chemin exact de chaque image validée dans le tableau de la section 6.
  - [ ] Réaliser ensuite chaque sous-tâche ajoutée dans l’ordre pour créer uniquement l’image correspondante à partir d’une vraie photographie du modèle exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent ; conserver exactement la forme, les proportions, les couleurs, les boutons, la marque, le logo et les inscriptions visibles du modèle, sans rendre le câble nécessaire, puis vérifier sa correspondance avec le périphérique avant de cocher sa création.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/competition-pro-5000.png à partir d’une vraie photographie du Competition Pro 5000 noir et rouge, sans l’ajouter au catalogue avant sa validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/zipstik-super-pro.png à partir de plusieurs vraies photographies du Zipstik Super Pro, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans l’ajouter au catalogue avant sa validation.
  - [x] Enregistrer la premiere previsualisation validee dans src/GWGUI.App/Assets/Controllers/konix-speedking-right-hand.png.
  - [x] Enregistrer la deuxieme previsualisation validee dans src/GWGUI.App/Assets/Controllers/konix-speedking-left-hand.png.
  - [x] Creer une previsualisation de src/GWGUI.App/Assets/Controllers/quickshot.png a partir de plusieurs vraies photographies du QuickShot exact, vue du dessus dans son sens normal utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Creer une previsualisation de src/GWGUI.App/Assets/Controllers/quickshot-deluxe.png a partir de plusieurs vraies photographies du QuickShot Deluxe exact, vue du dessus dans son sens normal utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Creer une previsualisation de src/GWGUI.App/Assets/Controllers/quickshot-ii-turbo.png a partir de plusieurs vraies photographies du QuickShot II Turbo exact, vue du dessus dans son sens normal utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/suncom-tac-2.png à partir de plusieurs vraies photographies du Suncom TAC-2 exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/powerplay-cruiser.png à partir de plusieurs vraies photographies du Powerplay Cruiser exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/suzo-the-arcade-turbo.png à partir de plusieurs vraies photographies du Suzo The Arcade Turbo exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/konix-speedking-analog.png à partir de plusieurs vraies photographies du Konix Speedking analogique exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/commodore-cd32.png à partir de plusieurs vraies photographies de la manette Commodore CD32 originale exacte, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/competition-pro-cd32.png à partir de plusieurs vraies photographies de la manette Competition Pro CD32 exacte, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/advanced-gravis-gamepad.png à partir de plusieurs vraies photographies de l’Advanced Gravis GamePad exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/commodore-vc-1312-paddles.png à partir de plusieurs vraies photographies des paddles Commodore VC-1312 exacts, vue du dessus dans leur sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/magnavox-odyssey-controller.png à partir de plusieurs vraies photographies du contrôleur Magnavox Odyssey original exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/magnavox-odyssey2-videopac-joystick.png à partir de plusieurs vraies photographies du joystick Magnavox Odyssey² / Philips Videopac original exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/philips-cdi-gamepad.png à partir de plusieurs vraies photographies de la manette Philips CD-i 22ER9021 originale exacte, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/philips-cdi-paddle-controller.png à partir de plusieurs vraies photographies du Philips CD-i Paddle Controller original exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/philips-cdi-roller-controller.png à partir de plusieurs vraies photographies du Philips CD-i Roller Controller 22ER9012 original exact, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/philips-cdi-mouse.png à partir de plusieurs vraies photographies de la souris Philips CD-i 22ER9011 originale exacte, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer une prévisualisation de src/GWGUI.App/Assets/Controllers/philips-cdi-remote-control.png à partir de plusieurs vraies photographies de la télécommande Philips CD-i Commander IR 22ER9055 originale exacte fournie avec la majorité des lecteurs grand public, vue du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-mouse-22er9010.png à partir de plusieurs vraies photographies de la souris Philips CD-i 22ER9010 réservée au CDI 180, modèle original exact vu du dessus dans son sens normal d’utilisation, câble non visible et fond transparent, sans ajout au catalogue avant validation.
  - [x] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-trackerball.png à partir de plusieurs vraies photographies du Philips CD-i Trackerball 22ER9013 anthracite original exact, avec ses trois commandes, vue du dessus dans son sens normal d’utilisation, câble non visible et fond transparent, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-touchpad-22er9017.png à partir de plusieurs vraies photographies du Philips CD-i Touchpad 22ER9017 original exact, vu du dessus dans son sens normal d’utilisation, câble non visible et fond transparent, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-wired-controller-22er9019.png à partir de plusieurs vraies photographies du Philips CD-i Wired Controller 22ER9019 original exact, vu du dessus dans son sens normal d’utilisation, câble non visible et fond transparent, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-graphic-control-22er9030.png à partir de plusieurs vraies photographies du Philips CD-i Graphic Control 22ER9030 réservé au CDI 180, modèle original exact vu du dessus dans son sens normal d’utilisation, câble non visible et fond transparent, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-ir-remote-22er9050.png à partir de plusieurs vraies photographies de la télécommande Philips CD-i IR 22ER9050 réservée au CDI 180, modèle original exact vu du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-thumbstick-ir-22er9051.png à partir de plusieurs vraies photographies du Philips CD-i Thumbstick IR 22ER9051 original exact, vu du dessus dans son sens normal d’utilisation et avec fond transparent, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-ir-receiver-22er9054.png à partir de plusieurs vraies photographies du récepteur Philips CD-i IR Set 22ER9054 original exact, vu du dessus dans son sens normal d’utilisation et avec fond transparent, sans dupliquer sa Commander 22ER9055 déjà représentée et sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-peacekeeper-22er9020.png à partir de plusieurs vraies photographies du Philips CD-i Peacekeeper 22ER9020 original exact, vu du côté utilisé pour viser avec toutes ses commandes visibles et avec fond transparent, câble non visible, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-keyboard-22er9040.png à partir de plusieurs vraies photographies du clavier Philips CD-i 22ER9040 original exact, vu du dessus et avec fond transparent, câble non visible, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-keyboard-22er9041.png à partir de plusieurs vraies photographies du clavier Philips CD-i 22ER9041 original exact, vu du dessus et avec fond transparent, câble non visible, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-keycontrol-22er9042.png à partir de plusieurs vraies photographies du Philips CD-i KeyControl 22ER9042 original exact, vu du dessus et avec fond transparent, câble non visible, sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-joystick-prototype-22er9014.png à partir de plusieurs vraies photographies du prototype Philips CD-i Joystick 22ER9014 exact, vu du dessus dans son sens normal d’utilisation et avec fond transparent, câble non visible, identifié comme prototype et sans ajout au catalogue avant validation.
  - [ ] Créer src/GWGUI.App/Assets/Controllers/philips-cdi-turbotrack-remote-prototype-22er9016.png à partir de plusieurs vraies photographies du prototype Philips CD-i TurboTrack Remote 22ER9016 exact, vu du dessus dans son sens normal d’utilisation et avec fond transparent, identifié comme prototype et sans ajout au catalogue avant validation.
  - [x] [6] Créer src/GWGUI.App/Assets/Controllers/fairchild-channel-f.png à partir de plusieurs vraies photographies du Fairchild Channel F Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [7] Créer src/GWGUI.App/Assets/Controllers/atari-cx40.png à partir de plusieurs vraies photographies du Atari CX40 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [8] Créer src/GWGUI.App/Assets/Controllers/atari-5200-controller.png à partir de plusieurs vraies photographies du Atari 5200 Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [9] Créer src/GWGUI.App/Assets/Controllers/atari-5200-trak-ball.png à partir de plusieurs vraies photographies du Atari 5200 Trak-Ball original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [10] Créer src/GWGUI.App/Assets/Controllers/atari-7800-pro-line-cx24.png à partir de plusieurs vraies photographies du Atari 7800 Pro-Line CX24 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [x] [11] Créer src/GWGUI.App/Assets/Controllers/atari-7800-control-pad-europe.png à partir de plusieurs vraies photographies du Atari 7800 European Control Pad original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [12] Créer src/GWGUI.App/Assets/Controllers/atari-jaguar-controller.png à partir de plusieurs vraies photographies du Atari Jaguar Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [13] Créer src/GWGUI.App/Assets/Controllers/atari-jaguar-pro-controller.png à partir de plusieurs vraies photographies du Atari Jaguar Pro Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [14] Créer src/GWGUI.App/Assets/Controllers/bally-astrocade-controller.png à partir de plusieurs vraies photographies du Bally Professional Arcade / Astrocade Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [15] Créer src/GWGUI.App/Assets/Controllers/mattel-intellivision-controller.png à partir de plusieurs vraies photographies du Mattel Intellivision Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [x] [16] Créer src/GWGUI.App/Assets/Controllers/mattel-hyperscan-controller.png à partir de plusieurs vraies photographies du Mattel HyperScan Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [17] Créer src/GWGUI.App/Assets/Controllers/colecovision-controller.png à partir de plusieurs vraies photographies du ColecoVision Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [18] Créer src/GWGUI.App/Assets/Controllers/gce-vectrex-controller.png à partir de plusieurs vraies photographies du GCE Vectrex Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [19] Créer src/GWGUI.App/Assets/Controllers/interton-vc-4000-controller.png à partir de plusieurs vraies photographies du Interton VC 4000 Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
- [x] [20] Créer src/GWGUI.App/Assets/Controllers/emerson-arcadia-2001-controller.png à partir de plusieurs vraies photographies du Emerson Arcadia 2001 Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [x] [21] Créer src/GWGUI.App/Assets/Controllers/view-master-interactive-vision-controller.png à partir de plusieurs vraies photographies du View-Master Interactive Vision Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [x] [22] Créer src/GWGUI.App/Assets/Controllers/vtech-socrates-controller.png à partir de plusieurs vraies photographies du VTech Socrates Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [x] [23] Créer src/GWGUI.App/Assets/Controllers/famicom-controller-i.png à partir de plusieurs vraies photographies du Famicom Controller I original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [x] [24] Créer src/GWGUI.App/Assets/Controllers/nes-famicom-dogbone.png à partir de plusieurs vraies photographies du NES/Famicom Dogbone original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [25] Créer src/GWGUI.App/Assets/Controllers/nes-advantage.png à partir de plusieurs vraies photographies du NES Advantage original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [26] Créer src/GWGUI.App/Assets/Controllers/nes-max.png à partir de plusieurs vraies photographies du NES Max original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [27] Créer src/GWGUI.App/Assets/Controllers/virtual-boy-controller.png à partir de plusieurs vraies photographies du Virtual Boy Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [28] Créer src/GWGUI.App/Assets/Controllers/gamecube-controller.png à partir de plusieurs vraies photographies du GameCube Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [29] Créer src/GWGUI.App/Assets/Controllers/gamecube-wavebird.png à partir de plusieurs vraies photographies du GameCube WaveBird original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [30] Créer src/GWGUI.App/Assets/Controllers/wii-remote-nunchuk.png à partir de plusieurs vraies photographies du Wii Remote avec Nunchuk original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [31] Créer src/GWGUI.App/Assets/Controllers/wii-classic-controller.png à partir de plusieurs vraies photographies du Wii Classic Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [32] Créer src/GWGUI.App/Assets/Controllers/wii-classic-controller-pro.png à partir de plusieurs vraies photographies du Wii Classic Controller Pro original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [33] Créer src/GWGUI.App/Assets/Controllers/wii-u-gamepad.png à partir de plusieurs vraies photographies du Wii U GamePad original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [34] Créer src/GWGUI.App/Assets/Controllers/wii-u-pro-controller.png à partir de plusieurs vraies photographies du Wii U Pro Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [35] Créer src/GWGUI.App/Assets/Controllers/nintendo-switch-joy-con.png à partir de plusieurs vraies photographies du Nintendo Switch Joy-Con original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [36] Créer src/GWGUI.App/Assets/Controllers/nintendo-switch-pro-controller.png à partir de plusieurs vraies photographies du Nintendo Switch Pro Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [37] Créer src/GWGUI.App/Assets/Controllers/nintendo-switch-2-joy-con-2.png à partir de plusieurs vraies photographies du Nintendo Switch 2 Joy-Con 2 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [38] Créer src/GWGUI.App/Assets/Controllers/nintendo-switch-2-pro-controller.png à partir de plusieurs vraies photographies du Nintendo Switch 2 Pro Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [39] Créer src/GWGUI.App/Assets/Controllers/sega-sg-1000-sj-200-joystick.png à partir de plusieurs vraies photographies du Sega SG-1000 SJ-200 Joystick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [40] Créer src/GWGUI.App/Assets/Controllers/sega-sg-1000-ii-sj-150-joypad.png à partir de plusieurs vraies photographies du Sega SG-1000 II SJ-150 Joypad original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [41] Créer src/GWGUI.App/Assets/Controllers/sega-saturn-3d-control-pad.png à partir de plusieurs vraies photographies du Sega Saturn 3D Control Pad original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [42] Créer src/GWGUI.App/Assets/Controllers/playstation-3-sixaxis.png à partir de plusieurs vraies photographies du PlayStation 3 Sixaxis original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [43] Créer src/GWGUI.App/Assets/Controllers/playstation-3-dualshock-3.png à partir de plusieurs vraies photographies du PlayStation 3 DualShock 3 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [44] Créer src/GWGUI.App/Assets/Controllers/playstation-move.png à partir de plusieurs vraies photographies du PlayStation Move original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [45] Créer src/GWGUI.App/Assets/Controllers/playstation-move-navigation-controller.png à partir de plusieurs vraies photographies du PlayStation Move Navigation Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [46] Créer src/GWGUI.App/Assets/Controllers/xbox-duke.png à partir de plusieurs vraies photographies du Xbox original Duke original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [47] Créer src/GWGUI.App/Assets/Controllers/xbox-controller-s.png à partir de plusieurs vraies photographies du Xbox original Controller S original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [48] Créer src/GWGUI.App/Assets/Controllers/xbox-360-controller.png à partir de plusieurs vraies photographies du Xbox 360 Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [49] Créer src/GWGUI.App/Assets/Controllers/xbox-elite-wireless-controller-series-2.png à partir de plusieurs vraies photographies du Xbox Elite Wireless Controller Series 2 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [50] Créer src/GWGUI.App/Assets/Controllers/xbox-adaptive-controller.png à partir de plusieurs vraies photographies du Xbox Adaptive Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [51] Créer src/GWGUI.App/Assets/Controllers/pc-engine-controller.png à partir de plusieurs vraies photographies du PC Engine Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [52] Créer src/GWGUI.App/Assets/Controllers/pc-engine-turbo-stick.png à partir de plusieurs vraies photographies du PC Engine Turbo Stick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [53] Créer src/GWGUI.App/Assets/Controllers/pc-engine-supergrafx-controller.png à partir de plusieurs vraies photographies du PC Engine SuperGrafx Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [54] Créer src/GWGUI.App/Assets/Controllers/nec-pc-fx-controller.png à partir de plusieurs vraies photographies du NEC PC-FX Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [55] Créer src/GWGUI.App/Assets/Controllers/neo-geo-aes-controller.png à partir de plusieurs vraies photographies du Neo Geo AES Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [56] Créer src/GWGUI.App/Assets/Controllers/neo-geo-controller-pro.png à partir de plusieurs vraies photographies du Neo Geo Controller Pro original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [57] Créer src/GWGUI.App/Assets/Controllers/panasonic-3do-controller.png à partir de plusieurs vraies photographies du Panasonic 3DO Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [58] Créer src/GWGUI.App/Assets/Controllers/goldstar-3do-controller.png à partir de plusieurs vraies photographies du GoldStar 3DO Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [59] Créer src/GWGUI.App/Assets/Controllers/amstrad-gx4000-cpc-plus-controller.png à partir de plusieurs vraies photographies du Amstrad GX4000 / CPC Plus Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [60] Créer src/GWGUI.App/Assets/Controllers/commodore-1311-joystick.png à partir de plusieurs vraies photographies du Commodore 1311 Joystick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [61] Créer src/GWGUI.App/Assets/Controllers/sinclair-sjs-1.png à partir de plusieurs vraies photographies du Sinclair SJS-1 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [62] Créer src/GWGUI.App/Assets/Controllers/fm-towns-marty-controller.png à partir de plusieurs vraies photographies du FM Towns Marty Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [63] Créer src/GWGUI.App/Assets/Controllers/pippin-applejack-white.png à partir de plusieurs vraies photographies du Pippin AppleJack Controller blanc original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [64] Créer src/GWGUI.App/Assets/Controllers/pippin-applejack-black.png à partir de plusieurs vraies photographies du Pippin AppleJack Controller noir original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [65] Créer src/GWGUI.App/Assets/Controllers/nuon-samsung-n2000-controller.png à partir de plusieurs vraies photographies du Nuon Samsung N2000 Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [66] Créer src/GWGUI.App/Assets/Controllers/nuon-logitech-controller.png à partir de plusieurs vraies photographies du Nuon Logitech Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [67] Créer src/GWGUI.App/Assets/Controllers/ouya-controller.png à partir de plusieurs vraies photographies du Ouya Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [68] Créer src/GWGUI.App/Assets/Controllers/steam-controller.png à partir de plusieurs vraies photographies du Steam Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [69] Créer src/GWGUI.App/Assets/Controllers/google-stadia-controller.png à partir de plusieurs vraies photographies du Google Stadia Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [70] Créer src/GWGUI.App/Assets/Controllers/amazon-luna-controller.png à partir de plusieurs vraies photographies du Amazon Luna Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [71] Créer src/GWGUI.App/Assets/Controllers/microsoft-sidewinder-game-pad.png à partir de plusieurs vraies photographies du Microsoft SideWinder Game Pad original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [72] Créer src/GWGUI.App/Assets/Controllers/microsoft-sidewinder-3d-pro.png à partir de plusieurs vraies photographies du Microsoft SideWinder 3D Pro original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [73] Créer src/GWGUI.App/Assets/Controllers/microsoft-sidewinder-precision-2.png à partir de plusieurs vraies photographies du Microsoft SideWinder Precision 2 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [74] Créer src/GWGUI.App/Assets/Controllers/ch-products-mach-ii.png à partir de plusieurs vraies photographies du CH Products Mach II original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [75] Créer src/GWGUI.App/Assets/Controllers/sega-control-stick.png à partir de plusieurs vraies photographies du Sega Control Stick 3020 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [76] Créer src/GWGUI.App/Assets/Controllers/sega-light-phaser.png à partir de plusieurs vraies photographies du Sega Light Phaser original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [77] Créer src/GWGUI.App/Assets/Controllers/sega-sports-pad.png à partir de plusieurs vraies photographies du Sega Sports Pad original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [78] Créer src/GWGUI.App/Assets/Controllers/sega-paddle-control.png à partir de plusieurs vraies photographies du Sega Paddle Control original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [79] Créer src/GWGUI.App/Assets/Controllers/sega-handle-controller.png à partir de plusieurs vraies photographies du Sega Handle Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [80] Créer src/GWGUI.App/Assets/Controllers/sega-arcade-power-stick.png à partir de plusieurs vraies photographies du Sega Arcade Power Stick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [81] Créer src/GWGUI.App/Assets/Controllers/sega-arcade-power-stick-6b.png à partir de plusieurs vraies photographies du Sega Arcade Power Stick 6B original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [82] Créer src/GWGUI.App/Assets/Controllers/sega-xe-1-ap.png à partir de plusieurs vraies photographies du Sega/Dempa XE-1 AP original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [83] Créer src/GWGUI.App/Assets/Controllers/sega-menacer.png à partir de plusieurs vraies photographies du Sega Menacer original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [84] Créer src/GWGUI.App/Assets/Controllers/sega-mega-mouse.png à partir de plusieurs vraies photographies du Sega Mega Mouse original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [85] Créer src/GWGUI.App/Assets/Controllers/sega-activator.png à partir de plusieurs vraies photographies du Sega Activator original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [86] Créer src/GWGUI.App/Assets/Controllers/sega-saturn-virtua-gun.png à partir de plusieurs vraies photographies du Sega Saturn Virtua Gun original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [87] Créer src/GWGUI.App/Assets/Controllers/sega-saturn-shuttle-mouse.png à partir de plusieurs vraies photographies du Sega Saturn Shuttle Mouse original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [88] Créer src/GWGUI.App/Assets/Controllers/sega-saturn-mission-stick.png à partir de plusieurs vraies photographies du Sega Saturn Mission Stick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [89] Créer src/GWGUI.App/Assets/Controllers/sega-saturn-arcade-racer.png à partir de plusieurs vraies photographies du Sega Saturn Arcade Racer original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [90] Créer src/GWGUI.App/Assets/Controllers/sega-saturn-twin-stick.png à partir de plusieurs vraies photographies du Sega Saturn Twin Stick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [91] Créer src/GWGUI.App/Assets/Controllers/sega-saturn-virtua-stick.png à partir de plusieurs vraies photographies du Sega Saturn Virtua Stick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [92] Créer src/GWGUI.App/Assets/Controllers/sega-dreamcast-mouse.png à partir de plusieurs vraies photographies du Sega Dreamcast Mouse original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [93] Créer src/GWGUI.App/Assets/Controllers/sega-dreamcast-keyboard.png à partir de plusieurs vraies photographies du Sega Dreamcast Keyboard original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [94] Créer src/GWGUI.App/Assets/Controllers/sega-dreamcast-light-gun.png à partir de plusieurs vraies photographies du Sega Dreamcast Light Gun original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [95] Créer src/GWGUI.App/Assets/Controllers/sega-dreamcast-fishing-controller.png à partir de plusieurs vraies photographies du Sega Dreamcast Fishing Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [96] Créer src/GWGUI.App/Assets/Controllers/sega-dreamcast-arcade-stick.png à partir de plusieurs vraies photographies du Sega Dreamcast Arcade Stick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [97] Créer src/GWGUI.App/Assets/Controllers/sega-dreamcast-twin-stick.png à partir de plusieurs vraies photographies du Sega Dreamcast Twin Stick original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [98] Créer src/GWGUI.App/Assets/Controllers/sega-dreamcast-maracas.png à partir de plusieurs vraies photographies du Sega Dreamcast Samba de Amigo Maracas original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [99] Créer src/GWGUI.App/Assets/Controllers/atari-cx30-paddles.png à partir de plusieurs vraies photographies du Atari CX30 Paddles original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [100] Créer src/GWGUI.App/Assets/Controllers/atari-cx20-driving-controller.png à partir de plusieurs vraies photographies du Atari CX20 Driving Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [101] Créer src/GWGUI.App/Assets/Controllers/atari-cx50-keyboard-controllers.png à partir de plusieurs vraies photographies du Atari CX50 Keyboard Controllers original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [102] Créer src/GWGUI.App/Assets/Controllers/atari-cx21-video-touch-pad.png à partir de plusieurs vraies photographies du Atari CX21 Video Touch Pad original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [103] Créer src/GWGUI.App/Assets/Controllers/atari-cx80-trak-ball.png à partir de plusieurs vraies photographies du Atari CX80 Trak-Ball original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [104] Créer src/GWGUI.App/Assets/Controllers/atari-xg-1-light-gun.png à partir de plusieurs vraies photographies du Atari XG-1 Light Gun original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [105] Créer src/GWGUI.App/Assets/Controllers/coleco-super-action-controller.png à partir de plusieurs vraies photographies du Coleco Super Action Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [106] Créer src/GWGUI.App/Assets/Controllers/coleco-roller-controller.png à partir de plusieurs vraies photographies du Coleco Roller Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [107] Créer src/GWGUI.App/Assets/Controllers/coleco-driving-module.png à partir de plusieurs vraies photographies du Coleco Driving Module original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [108] Créer src/GWGUI.App/Assets/Controllers/nintendo-zapper.png à partir de plusieurs vraies photographies du Nintendo Zapper original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [109] Créer src/GWGUI.App/Assets/Controllers/nintendo-power-glove.png à partir de plusieurs vraies photographies du Nintendo Power Glove original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [110] Créer src/GWGUI.App/Assets/Controllers/super-nintendo-super-scope.png à partir de plusieurs vraies photographies du Super Nintendo Super Scope original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [111] Créer src/GWGUI.App/Assets/Controllers/super-nintendo-mouse.png à partir de plusieurs vraies photographies du Super Nintendo Mouse original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [112] Créer src/GWGUI.App/Assets/Controllers/gamecube-dk-bongos.png à partir de plusieurs vraies photographies du Nintendo GameCube DK Bongos original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [113] Créer src/GWGUI.App/Assets/Controllers/gamecube-ascii-keyboard-controller.png à partir de plusieurs vraies photographies du Nintendo GameCube ASCII Keyboard Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [114] Créer src/GWGUI.App/Assets/Controllers/wii-balance-board.png à partir de plusieurs vraies photographies du Nintendo Wii Balance Board original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [115] Créer src/GWGUI.App/Assets/Controllers/playstation-mouse.png à partir de plusieurs vraies photographies du Sony PlayStation Mouse original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [116] Créer src/GWGUI.App/Assets/Controllers/playstation-analog-joystick.png à partir de plusieurs vraies photographies du Sony PlayStation Analog Joystick SCPH-1110 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [117] Créer src/GWGUI.App/Assets/Controllers/playstation-negcon.png à partir de plusieurs vraies photographies du Sony PlayStation neGcon original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [118] Créer src/GWGUI.App/Assets/Controllers/playstation-jogcon.png à partir de plusieurs vraies photographies du Sony PlayStation Jogcon original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [119] Créer src/GWGUI.App/Assets/Controllers/playstation-guncon.png à partir de plusieurs vraies photographies du Namco GunCon / G-Con 45 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [120] Créer src/GWGUI.App/Assets/Controllers/playstation-guncon-2.png à partir de plusieurs vraies photographies du Namco GunCon 2 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [121] Créer src/GWGUI.App/Assets/Controllers/playstation-guncon-3.png à partir de plusieurs vraies photographies du Namco GunCon 3 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [122] Créer src/GWGUI.App/Assets/Controllers/pc-engine-mouse.png à partir de plusieurs vraies photographies du NEC PC Engine Mouse original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [123] Créer src/GWGUI.App/Assets/Controllers/pc-engine-avenue-pad-3.png à partir de plusieurs vraies photographies du NEC PC Engine Avenue Pad 3 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [124] Créer src/GWGUI.App/Assets/Controllers/pc-engine-avenue-pad-6.png à partir de plusieurs vraies photographies du NEC PC Engine Avenue Pad 6 original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [125] Créer src/GWGUI.App/Assets/Controllers/xbox-steel-battalion-controller.png à partir de plusieurs vraies photographies du Xbox Steel Battalion Controller original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
+  - [ ] [126] Créer src/GWGUI.App/Assets/Controllers/xbox-360-speed-wheel.png à partir de plusieurs vraies photographies du Xbox 360 Wireless Speed Wheel original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [127] Créer src/GWGUI.App/Assets/Controllers/microsoft-sidewinder-freestyle-pro.png à partir de plusieurs vraies photographies du Microsoft SideWinder Freestyle Pro original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [128] Créer src/GWGUI.App/Assets/Controllers/microsoft-sidewinder-force-feedback-pro.png à partir de plusieurs vraies photographies du Microsoft SideWinder Force Feedback Pro original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [129] Créer src/GWGUI.App/Assets/Controllers/microsoft-sidewinder-strategic-commander.png à partir de plusieurs vraies photographies du Microsoft SideWinder Strategic Commander original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] [130] Créer src/GWGUI.App/Assets/Controllers/gce-vectrex-light-pen.png à partir de plusieurs vraies photographies du GCE Vectrex Light Pen original exact, dans sa vue normale d’utilisation permettant de voir toutes ses commandes, câble non visible lorsqu’il n’est pas nécessaire et fond transparent, sans ajout au catalogue avant validation.
  - [ ] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerArtworkCatalog.cs après la création de chaque image pour ajouter uniquement son profil validé et ses zones, puis vérifier l’alignement de chaque zone à plusieurs tailles.
  - [ ] Vérifier que src/GWGUI.App/GWGUI.App.csproj continue d’embarquer toutes les images ajoutées par son motif Assets\Controllers\*.png sans ajouter une seconde règle de ressources.

- [x] Retirer le choix global du périphérique physique sans perdre les configurations existantes
  - [x] Modifier src/GWGUI.Emulation/Functions/EmulationInputMappingFunctions.cs pour exposer la valeur d’une source de manette identifiée et faire conserver à IsControllerSourcePressed ses résultats actuels en utilisant cette valeur.
  - [x] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSnapshotFunctions.cs pour résoudre, pour chaque association, l’identifiant de périphérique inclus dans sa source et conserver DeviceId enregistré comme repli pour les anciennes associations.
  - [x] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSnapshotFunctions.cs pour accepter les sources clavier et souris déjà représentées dans EmulationInputSnapshot, comme le chemin Amiga, sans modifier les commandes cibles.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSnapshotFunctions.cs uniquement pour faire passer ses sources de manette par la valeur commune ajoutée, en conservant la résolution par association, les sources clavier et souris et le repli DeviceId existants.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Functions/AmigaInputSettingsFunctions.cs pour autoriser la souris parmi les sources capturables des ports Amiga, sans modifier les types de périphériques émulés.
  - [x] Modifier src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs pour autoriser la souris parmi les sources capturables des ports Atari, sans modifier les types de périphériques émulés.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour supprimer le ComboBox Device et son choix automatique après capture, tout en conservant la valeur PhysicalDeviceId déjà enregistrée comme donnée de compatibilité non modifiable.
  - [x] Modifier src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs pour retirer le contrôle Device et conserver uniquement les éléments encore affichés.
  - [x] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour ne plus remplir ni enregistrer un sélecteur physique, préserver PhysicalDeviceId d’une configuration existante et laisser chaque nouvelle association conserver sa propre source.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationControllerSettingsSection.cs pour supprimer la détection et la sélection globales devenues inutilisées.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour retirer le champ Périphérique du port et conserver le choix du type de périphérique émulé.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par ce retrait et l’élargissement des sources.

- [x] Placer le visualiseur à droite du tableau du port actif
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationControllerSettingsConstants.cs pour ajouter uniquement la largeur nécessaire à l’icône de la colonne État, sans créer de dimensions propres à une copie du visualiseur.
  - [x] Modifier src/GWGUI.App/Contracts/Emulation/Controllers/EmulationControllerPortSettings.cs pour transporter le ControllerVisualizer du port avec son type et son InputBindingEditor.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour créer un seul ControllerVisualizer par port et lui affecter le profil correspondant au type émulé sélectionné.
  - [x] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour conserver ModuleId et MachineId de la configuration courante et les transmettre à chaque EmulationControllerPortEditor sans les déduire d’un libellé affiché.
  - [x] Modifier UpdateControllerBindings dans src/GWGUI.App/Controllers/Emulation/Input/EmulationInputSettingsController.cs pour changer ensemble les lignes et le profil lorsqu’un type de périphérique émulé est choisi.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationControllerSettingsLayout.cs pour réutiliser le ControllerVisualizer commun à droite du tableau du même port, le conserver hors du défilement vertical et le contraindre à l’espace restant afin qu’il ne dépasse pas, sans réduire le tableau ni créer un second bloc visuel.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml pour réduire la colonne État à son icône, retirer uniquement StateText de la ligne et conserver les boutons Assigner et Supprimer.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette disposition.

- [x] Relier les associations et la représentation sans créer un second chemin de capture
  - [x] Créer le fichier vide src/GWGUI.App/Controllers/Emulation/Input/EmulationBindingVisualizationController.cs.
  - [x] Modifier src/GWGUI.App/Controllers/Emulation/Input/EmulationBindingVisualizationController.cs pour lire les associations courantes de InputBindingEditor, les états clavier, souris et GameInput disponibles et produire un ControllerVisualState contenant tous les appuis simultanés.
  - [x] Reporter l’application d’un seuil ou de DeadZonePercent dans EmulationBindingVisualizationController tant que le choix entre réglage général, émulateur ou machine n’est pas validé ; transmettre entre-temps les valeurs analogiques brutes sans inventer de règle.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditor.xaml.cs pour exposer une opération commune qui sélectionne une ligne par son identifiant et démarre sa capture.
  - [x] Modifier AssignClicked dans src/GWGUI.App/Views/Controls/Emulation/Input/InputBindingEditorCaptureFunctions.cs pour appeler cette opération commune sans changer les sources ni le délai de capture.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour raccorder le clic d’une zone du ControllerVisualizer à la même opération commune et ne créer ni double-clic ni bouton supplémentaire.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Input/EmulationControllerPortEditor.cs pour démarrer et arrêter EmulationBindingVisualizationController avec le chargement et le déchargement du port, sans laisser de temporisateur ou de gestionnaire attaché.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la visualisation en direct et le clic des zones.

- [ ] Corriger les régressions constatées pendant la validation du point 6
  - [ ] Sérialiser les sauvegardes automatiques Amiga et Atari afin d’empêcher toute collision sur leur fichier temporaire lors d’un changement de type, notamment vers Aucune.
  - [ ] Supprimer toute association physique générique fournie par les DLL et ne pas recycler les associations de l’ancien type lors d’un changement de type.
  - [ ] Limiter l’Atari 2600 au visuel CX40 et utiliser le Control Pad européen comme visuel par défaut de l’Atari 7800.
  - [ ] Conserver le tableau et le visualiseur à dimensions fixes, faire défiler uniquement les lignes du tableau et garder le visualiseur visible à droite.
  - [ ] Rendre les listes de types et de visuels défilantes, puis élargir la colonne État sans couper son icône.
  - [ ] Agrandir l’image dans son espace fixe sans déformer son rapport d’aspect.
  - [ ] Refaire les surimpressions communes sans point blanc ni barre de manche et compléter les zones de boutons des deux Konix Speedking.
  - [ ] Construire avec scripts/build.ps1 -Configuration Debug puis relancer le binaire Debug pour vérifier les huit défauts signalés.

- [ ] Refaire la surimpression analogique dans le système commun
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour remplacer le trait terminé par un point des sticks par un rond partant du centre et se déplaçant selon la direction et l’inclinaison.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour faire partir le halo des joysticks à manche du centre et l’allonger selon leur direction et leur valeur.
  - [x] Modifier src/GWGUI.App/Views/Controls/Options/ControllerVisualization/ControllerVisualizer.Artwork.cs pour faire partir le halo des gâchettes du centre et l’allonger vers le bas selon leur pression.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par les trois rendus analogiques.
  - [ ] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier ces trois rendus avec plusieurs périphériques physiques ; ne cocher cette tâche que lorsque la forme est validée.
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md, dans la section 6, pour inscrire la forme précise validée pendant cette vérification.
  - [ ] Fermer l’instance de GW GUI utilisée pour cette vérification.

- [ ] Vérifier tout le point dans l’application
  - [ ] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et vérifier successivement chaque périphérique basique validé dans chaque port Amiga et Atari où il est proposé.
  - [ ] Dans la même exécution, vérifier que le changement d’onglet de port affiche un seul tableau avec son seul visuel, que le visuel reste fixe pendant le défilement et que le tableau ne rétrécit pas lorsque la fenêtre se resserre.
  - [ ] Dans la même exécution, vérifier simultanément des associations provenant de plusieurs manettes, du clavier, de la souris et d’un périphérique déconnecté, sans sélection préalable d’un périphérique physique.
  - [ ] Dans la même exécution, vérifier qu’un clic sur chaque zone sélectionne la bonne ligne et démarre la même capture que Assigner, puis vérifier que la modification d’association ne laisse aucun halo permanent.
  - [ ] Dans la même exécution, vérifier une configuration ancienne contenant PhysicalDeviceId afin de confirmer que son repli continue à fonctionner alors que le champ n’est plus affiché.
  - [ ] Dans la même exécution, revenir à l’onglet général Manettes et vérifier que le visualiseur existant utilise toujours ses modèles et bénéficie du nouveau rendu analogique commun.
  - [ ] Fermer l’instance de GW GUI utilisée pour cette vérification.

## Checklist détaillée — Point 7 : recherche et architecture des filtres vidéo

Ce point produit la recherche et les décisions d’architecture demandées. Il ne crée aucun filtre, shader, réglage de configuration ou contrôle d’interface avant validation du catalogue et de l’architecture.

### Décisions validées

- Les cinq réglages généraux restent toujours visibles : luminosité, contraste, gamma, saturation et netteté.
- Le gamma GW GUI utilise la plage -10 à +10, la valeur 0 est neutre et `exposant = 2^(-valeur / 10)` ; une valeur positive éclaircit les tons moyens et une valeur négative les assombrit.
- Snapshot contient la sortie finale des traitements GW GUI, avant tout futur bezel, cadre ou habillage externe ; la capture correspond ainsi à l’image visible dans la zone d’émulation sur les quatre renderers.
- L’échantillonnage utilise un sélecteur unique et une seule méthode active.
- La technologie d’affichage utilise un sélecteur unique : **Normal**, **CRT**, **Écran à pixels fixes**, **Plasma** ou **Écran vectoriel**. Le panneau de paramètres change avec ce choix.
- CRT utilise un sous-choix couleur ou monochrome vert, ambre, blanc ou gris, avec palettes prédéfinies et future teinte personnalisée. Scanlines et trame/moiré volontaire appartiennent uniquement au panneau CRT.
- Écran à pixels fixes utilise un sous-choix LCD, LCD rétroéclairé par LED ou OLED. Les réglages communs restent partagés ; un réglage propre n’apparaît que s’il produit une différence visible réelle.
- La rémanence et le temps de réponse des pixels appartiennent au panneau Écran à pixels fixes. Le désentrelacement reste un traitement indépendant ou une option interne du moteur.
- Plasma et Écran vectoriel seront réalisés après le premier socle, mais font partie de la cible complète.
- Les scalers, la restauration, les traitements temporels, VFD, matrices LED ou de points, segments, papier électronique, projection, simulations de signal et effets stylistiques restent dans le catalogue pour les étapes ultérieures.
- Toutes les technologies validées seront réalisées pas à pas ; aucune ne doit être retirée de la checklist parce qu’elle est planifiée plus tard.
- Les douze présélections initiales sont Normal, CRT Arcade couleur, CRT Téléviseur couleur, CRT Monochrome vert, ambre et blanc, LCD couleur, LCD monochrome, LCD rétroéclairé LED, OLED, Plasma et Écran vectoriel ; leurs identifiants et valeurs exactes sont définis dans docs/reference/emulation-video-filters.md.
- Les intensités vidéo utilisent `0..100` et les durées temporelles utilisent `0..1000 ms` ; une rémanence sans suffixe `ms` est une intensité.

### Journal des décisions autonomes

Ce journal conserve les questions qui auraient pu bloquer l’avancement. Les numéros de ligne sont
ceux relevés au moment de la décision ; les titres de section restent les repères durables si une
modification ultérieure décale ces lignes.

- **2026-09-01 — Gamma**
  - Question : quelle plage, quelle valeur neutre et quelle conversion utiliser pour le gamma ?
  - Décision : utiliser la plage `-10..+10`, avec `0` neutre, et convertir la valeur affichée par
    `exposant = 2^(-valeur / 10)` ; une valeur positive éclaircit les tons moyens.
  - Motif : cette échelle symétrique est cohérente avec les autres réglages généraux et conserve une
    conversion exponentielle explicite, stable et réversible autour de la valeur neutre.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Gamma retenu`, lignes
    304 à 313 ; `docs/tasks/interface/emulation-improvements.md`, section `Décisions validées`, ligne
    1851. Ces valeurs restent révisables avant la création du contrat correspondant.

- **2026-09-01 — Contenu de Snapshot**
  - Question : Snapshot doit-il enregistrer l’image brute émise par le moteur ou l’image après les
    traitements vidéo de GW GUI ?
  - Décision : enregistrer la sortie finale des traitements GW GUI, avant tout bezel, cadre ou autre
    habillage externe futur ; la capture correspond à l’image visible dans la zone d’émulation.
  - Motif : un utilisateur attend d’une capture qu’elle reproduise le rendu qu’il a configuré. La
    frontière avant habillage conserve une image propre à l’émulation et évite de figer une future
    présentation de fenêtre dans le fichier capturé.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Snapshot`, lignes 168 à
    179 ; `docs/architecture/emulation.md`, section `Snapshot`, lignes 545 à 552 ;
    `docs/tasks/interface/emulation-improvements.md`, section `Décisions validées`, ligne 1852.
    Les surfaces actuelles concernées sont `WpfVideoSurface`, `OpenGlVideoSurface` et
    `VeldridVideoSurface` ; leur code n’est pas modifié par cette tâche documentaire.

- **2026-09-01 — Présélections vidéo initiales**
  - Question : quels noms, identifiants persistants, contenus et valeurs exactes fournir pour les
    premières présélections ?
  - Décision : fournir douze présélections couvrant Normal, deux CRT couleur, trois CRT
    monochromes, quatre écrans à pixels fixes, Plasma et Écran vectoriel. Les réglages généraux
    utilisent `-10..+10`, les intensités `0..100` et les durées des millisecondes ; le tableau de
    référence fixe chaque valeur et chaque identifiant persistant.
  - Motif : cette sélection couvre chaque technologie et chaque sous-choix déjà retenus sans créer
    de panneau supplémentaire. Les unités normalisées gardent les presets indépendants du shader ou
    du backend qui réalisera l’effet.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Présélections`, lignes 299
    à 330 ; `docs/tasks/interface/emulation-improvements.md`, section `Décisions validées`, ligne
    1861. Aucune constante ni ressource traduite n’est créée pendant cette tâche documentaire.

- **2026-09-01 — Bornes des valeurs temporelles**
  - Question : quelle borne commune utiliser pour les durées exprimées en millisecondes, et comment
    les distinguer des intensités de rémanence ?
  - Décision : borner les durées à `0..1000 ms` et conserver les intensités, y compris la rémanence
    sans suffixe `ms`, dans `0..100`.
  - Motif : une seconde couvre les temps de réponse et historiques visés tout en empêchant une
    configuration déraisonnable ; des unités distinctes évitent qu’un preset mélange durée et force.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Présélections`, lignes 303
    à 306 ; `docs/tasks/interface/emulation-improvements.md`, section `Décisions validées`, ligne
    1862 ; le code correspondant sera centralisé dans `EmulationVideoProcessingLimits.cs`.

- **2026-09-01 — Application immuable de la configuration vidéo commune**
  - Question : comment le panneau commun peut-il remplacer `VideoProcessing` dans une configuration
    immuable sans connaître `AmigaMachineConfiguration` ou `AtariMachineConfiguration` ?
  - Décision : ajouter `ApplyVideoProcessing` au contrat `IEmulationModule`, puis laisser chaque
    module spécialisé produire sa propre nouvelle configuration typée.
  - Motif : App reste indépendante des familles et ne sérialise pas une configuration en JSON pour
    la modifier ; chaque bibliothèque conserve la responsabilité de son type concret.
  - Modifications : `src/GWGUI.Emulation/Interfaces/IEmulationModule.cs`,
    `src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs`,
    `src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs`, puis raccordement dans
    `src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs`.

- **2026-09-01 — Catalogue neutre utilisé par Argos**
  - Question : comment exécuter la commande Argos imposée alors que le script cherche
    `Resources/Emulation.resx`, fichier absent du dépôt ?
  - Décision : corriger le script pour cibler `Resources/00-Base/<catalogue>`, qui est le catalogue
    neutre déclaré par `GWGUI.App.csproj`, sans changer sa syntaxe de commande.
  - Motif : la commande documentée reste identique et chaque clé continue d’être ajoutée par Argos
    dans le catalogue neutre, en-US et toutes les cultures prises en charge.
  - Modifications : `scripts/translate-resx-argos.py`, fonction `main`, avant tout ajout de clé vidéo.

- **2026-09-01 — Ordre de propagation de la configuration vidéo**
  - Question : comment `MachineVideoPresenter` peut-il réappliquer la configuration courante lors
    d’un changement ou d’un repli de surface alors que `IEmulationVideoSurface` ne sait pas encore
    la recevoir ?
  - Décision : ajouter avant la propagation deux tâches atomiques donnant aux surfaces un contrat
    de configuration et un stockage normalisé sans effet visuel, puis exécuter la propagation dans
    l’ordre surface, presenter, controller et instance ciblée.
  - Motif : chaque dépendance précède ainsi son utilisation ; la future chaîne de traitement
    consommera ce contrat déjà présent sans modifier le rendu pendant cette étape.
  - Modifications : checklist `Appliquer les changements à la seule instance correspondante`, puis
    `IEmulationVideoSurface` et les trois surfaces de rendu.

- **2026-09-01 — Configuration vidéo initiale du controller**
  - Question : d’où `MachineController` reçoit-il la configuration vidéo initiale alors que
    `MachineControllerOptions.Machine` est une instance `IEmulatedMachine` sans paramètres persistés ?
  - Décision : ajouter la configuration commune à `MachineControllerOptions` et la fournir depuis
    `EmulationMachineRuntime.Configuration` au même endroit que `VideoRenderer`.
  - Motif : le controller reçoit ainsi un instantané cohérent de la configuration persistée sans
    ajouter de réglage d’interface au contrat de la machine émulée.
  - Modifications : `MachineControllerOptions.cs` et `EmulationSectionMachineFunctions.cs` avant
    la tâche `MachineController.cs`.

- **2026-09-01 — Confirmation des technologies d’écran incompatibles**
  - Question : comment tester la confirmation exigée alors que le panneau remplace actuellement la
    technologie active sans consulter le catalogue d’incompatibilités ?
  - Décision : demander confirmation uniquement lors du passage direct d’une technologie simulée
    non normale à une autre technologie simulée incompatible ; Normal reste une activation ou une
    désactivation sans avertissement.
  - Motif : le sélecteur garantit déjà l’exclusivité, mais un remplacement CRT vers LCD, Plasma ou
    Vector peut surprendre ; une fonction de confirmation injectable rend ce choix testable.
  - Modifications : `EmulationVideoProcessingSettingsSection.cs` avant ses tests comportementaux.

- **2026-09-01 — Couture de test du ciblage d’une instance**
  - Question : comment vérifier le ciblage d’une seule instance sans construire une fenêtre complète
    et une machine émulée réelle autour du gestionnaire privé `ConfigurationSaved` ?
  - Décision : extraire une fonction interne générique qui recherche exactement la clé
    `(ModuleId, ConfigurationId)` et applique une action au seul élément trouvé ; le gestionnaire
    continuera de contrôler que le contenu trouvé est un `MachineController`.
  - Motif : le test couvre la règle de sélection indépendamment du démarrage d’un moteur, tandis
    que le chemin de production conserve son dictionnaire et son type concret.
  - Modifications : nouveau fichier de fonctions sous
    `Functions/Views/Emulation/Machine`, puis utilisation dans
    `EmulationSectionConfigurationFunctions.cs`.

- **2026-09-01 — Couture de test brouillon ou autosauvegarde**
  - Question : comment tester la décision de persistance vidéo sans charger les styles globaux de
    toute la fenêtre d’options dans le runner WPF ?
  - Décision : extraire la seule décision « stocker le brouillon si la machine n’a aucune
    configuration, sinon sauvegarder » dans une fonction interne appelée par
    `EmulationModuleSettingsSection`.
  - Motif : le test vérifie la vraie règle de production sans dépendre des ressources visuelles du
    sélecteur de machine ; le verrou et la notification restent dans le contrôle.
  - Modifications : nouveau fichier sous `Functions/Views/Emulation/Settings`, puis raccordement
    dans `EmulationModuleSettingsSection.cs`.

- **2026-09-01 — Préservation vidéo dans les reconstructions Atari**
  - Question : les reconstructions explicites de `AtariMachineConfiguration` conservent-elles le
    nouveau dernier membre `VideoProcessing` après une modification sans rapport avec la vidéo ?
  - Décision : transmettre explicitement la configuration vidéo dans les chemins firmware, média,
    entrée, stockage et configuration courante utilisée par les états sauvegardés, puis ajouter un
    test de non-régression.
  - Motif : le paramètre facultatif masque l’oubli à la compilation et réinitialise silencieusement
    les filtres ; les opérations non vidéo doivent conserver la valeur enregistrée.
  - Modifications : fichiers Atari concernés et `EmulationVideoConfigurationTests.cs` avant la
    création de la chaîne de traitement.

- **2026-09-01 — Exécuteur neutre avant la fabrique de chaînes**
  - Question : quel exécuteur la fabrique peut-elle sélectionner alors qu’aucune implémentation de
    `IEmulationVideoProcessingPipeline` ne précède sa tâche ?
  - Décision : créer un exécuteur pass-through commun portant le renderer demandé et retournant la
    trame inchangée après validation des tailles ; les groupes CPU, OpenGL et Veldrid le remplaceront
    ensuite progressivement.
  - Motif : la fabrique a une dépendance concrète, la chaîne vide reste vérifiable et aucun effet
    Normal n’est implémenté prématurément.
  - Modifications : nouveau fichier sous `Rendering/Emulation/Processing` avant la fabrique.

- **2026-09-01 — Chemin commun entre traitement et Snapshot**
  - Question : comment démontrer que les trois familles de surface capturent bien la sortie traitée
    alors qu’elles dupliquent encore l’appel de chaîne puis la conversion BGRA ?
  - Décision : extraire une fonction commune retournant la `VideoFrame` traitée et ses pixels
    BGRA, puis l’utiliser dans WPF, OpenGL et Veldrid avant affichage et mise à jour de Snapshot.
  - Motif : le test peut couvrir le point commun aux quatre renderers, et aucune surface ne peut
    capturer par mégarde la trame brute après l’introduction d’un effet.
  - Modifications : nouveau fichier sous `Functions/Rendering/Emulation`, raccordement des trois
    surfaces, puis test Snapshot.

- **2026-09-01 — Injection déterministe du pipeline WPF pour Snapshot**
  - Question : comment prouver que `WpfVideoSurface.Snapshot` reçoit une trame modifiée par la
    chaîne plutôt que la source brute, sans implémenter prématurément un effet Normal ?
  - Décision : permettre au constructeur interne WPF de recevoir facultativement un pipeline de
    test, tout en conservant la création WPF neutre par défaut utilisée par la fabrique.
  - Motif : un double déterministe peut remplacer la trame, puis le test lit les pixels réels de
    Snapshot ; OpenGL et Veldrid partagent déjà la même fonction en amont.
  - Modifications : `WpfVideoSurface.cs` avant le test Snapshot.

- **2026-09-01 — Conversions CPU des cinq réglages généraux**
  - Question : quelles conversions exactes appliquer à luminosité, contraste, saturation et netteté,
    dont seule la plage était validée ?
  - Décision : traiter en lumière linéaire, dans l’ordre luminosité, contraste, gamma, saturation et
    netteté ; utiliser respectivement un décalage `valeur / 20`, un contraste
    `2^(valeur / 5)` autour de 0,18, le gamma validé, une saturation `1 + valeur / 10` autour de
    la luminance Rec. 709, puis un masque flou ou un lissage 3×3 de force absolue `valeur / 10`.
  - Motif : les conversions sont symétriques autour de 0, bornées, testables et n’ajoutent aucune
    dépendance à un shader tiers.
  - Modifications : `docs/reference/emulation-video-filters.md` avant l’exécution CPU.

- **2026-09-01 — Validation visuelle CRT sans médias propriétaires**
  - Question : comment vérifier les présélections CRT sur Amiga et Atari lorsque le workspace ne
    contient ni ROM, ni disque, ni configuration de machine exécutable ?
  - Décision : utiliser deux mires déterministes distinctes, portant les rapports logiques Amiga et
    Atari, puis présenter les cinq présélections CRT dans les surfaces WPF, OpenGL, Direct3D 11 et
    Vulkan. Vérifier automatiquement que chaque rendu est non noir et distinct, produire une
    planche par renderer et inspecter visuellement les quatre planches. Cette validation ne prétend
    pas qu’une ROM Amiga ou Atari absente a été lancée.
  - Motif : les filtres de GW GUI reçoivent une `VideoFrame` après le moteur et ne dépendent pas du
    contenu propriétaire qui l’a produite. Les mires couvrent les couleurs, gradients, diagonales et
    damiers nécessaires pour voir palettes, masque, scanlines, courbure, halo et vignettage.
  - Modifications : `tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs` et planches
    générées dans `build/validation/crt-validation` ; ordre des colonnes : Arcade couleur, Téléviseur
    couleur, vert, ambre, blanc ; lignes : Amiga puis Atari.

- **2026-09-02 — Paramètres exacts du modèle Plasma**
  - Question : quelles conversions appliquer aux cellules, à la diffusion, au tramage temporel et à
    la rémanence avant d’écrire leur référence CPU ?
  - Décision : utiliser quatre intensités `0..100` neutres à zéro ; cellules RGB avec atténuation
    maximale de `35 %` et interstice maximal de `20 %`, tramage Bayer 4×4 séquencé d’amplitude
    maximale `4 %`, diffusion 3×3 maximale `50 %`, puis rémanence maximale sur une unique image
    précédente pondérée par l’intensité.
  - Motif : ces formules originales sont bornées, déterministes et portables dans les shaders, sans
    dépendance de licence tierce ni historique non borné.
  - Modifications : `docs/reference/emulation-video-filters.md`, section `Plasma` ; la définition
    existante `EmulationPlasmaVideoConfiguration` conserve ses quatre champs validés.

- **2026-09-02 — Approximation raster de l’écran vectoriel**
  - Question : comment définir lignes, halo et persistance sans primitives vectorielles fournies par
    les moteurs actuels ?
  - Décision : utiliser le gradient Sobel de luminance linéaire, un seuil `0..1` avec transition
    `0,10`, un renforcement de ligne `0..100`, un halo 3×3 maximal `50 %` et une persistance sur une
    seule image précédente pondérée par `0..100`.
  - Motif : cette approximation conserve explicitement son origine raster, reste déterministe et
    portable, et ne revendique pas la précision d’un moteur transmettant des primitives.
  - Modifications : `docs/reference/emulation-video-filters.md`, section
    `Écran vectoriel — approximation raster`.

- **2026-09-02 — Variante xBR intégrable**
  - Question : quelle variante, quelle source et quelle licence retenir pour le premier scaler pixel
    art sans casser les quatre renderers ni les échelles non entières ?
  - Décision : adapter le niveau 1 mono-passe de `xbr-lv3.glsl` de Hyllian, avec ses constantes
    publiées (`Y=48`, transition de coin `1,10..1,90`) et une sélection déterministe du coin au poids
    maximal ; le choix `xBR` rejoint le sélecteur d’échantillonnage existant.
  - Motif : le fichier de référence porte explicitement la licence MIT, la variante reste symétrique,
    bornée et portable en CPU, GLSL 1.20 et shader Veldrid, sans runtime ni texture Libretro.
  - Modifications : `docs/reference/emulation-video-filters.md`, section
    `xBR — niveau 1 mono-passe`, et `THIRD-PARTY-NOTICES.md`.

- **2026-09-02 — xBRZ sans contamination GPL**
  - Question : comment proposer xBRZ alors que le fichier Libretro de référence attribue cette
    partie à un code GPL-3.0 dont l’exception de liaison vise MAME seulement ?
  - Décision : ne copier aucun code xBRZ et développer une formule originale 3×3, documentée sous
    `xBRZ — réimplémentation compatible avec la licence MIT de GW GUI`, exécutée par la référence CPU
    commune dans les quatre surfaces.
  - Motif : la séparation protège la licence MIT du projet, donne un résultat strictement identique
    sur tous les backends et évite d’alourdir encore les shaders monolithiques.
  - Modifications : `docs/reference/emulation-video-filters.md`, modèle commun, référence CPU,
    surfaces et tests dédiés.

- **2026-09-02 — Source HQx permissive**
  - Question : quelle implémentation HQx peut être adaptée sans LUT externe ni licence incompatible ?
  - Décision : adapter le noyau HQ2x mono-passe de mGBA (`hq2x.fs`), sous MIT, dans la référence CPU
    commune et conserver ses seuils, motifs et poids.
  - Motif : cette source est explicite, autonome et compatible avec la licence MIT de GW GUI ; le
    repli commun garantit l’identité des quatre surfaces.
  - Modifications : référence, notice tierce, modèle, pipeline, surfaces et tests HQx.

- **2026-09-02 — Adaptation portable de ScaleFX**
  - Question : faut-il intégrer les cinq passes GPU de ScaleFX séparément dans chacun des quatre
    renderers, ou conserver une référence unique malgré son coût CPU ?
  - Décision : adapter la chaîne MIT officielle de Sp00kyFox en une reconstruction CPU 3× à palette
    préservée, puis présenter son résultat par une copie neutre dans les quatre surfaces.
  - Motif : la source et ses cinq passes sont explicitement MIT, mais les pipelines WPF, OpenGL et
    Veldrid n’exposent pas le même mécanisme de textures intermédiaires ; la référence commune évite
    des résultats différents et reste cohérente avec les replis xBRZ et HQx déjà validés.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:416` (source, licence et
    comportement), `THIRD-PARTY-NOTICES.md:25` (notice MIT),
    `EmulationVideoSampling.cs:12` et `EmulationResourceKeys.cs:55` (modèle et constante), les 30
    fichiers `Resources/*/Emulation.resx` aux lignes finales `682`, `819` ou `1284` via Argos,
    `CpuScaleFxVideoScaler.cs:1` et `CpuEmulationVideoProcessingPipeline.cs:332` (traitement),
    `OpenGlVideoSurface.cs:122` et `VeldridVideoSurface.cs:89` (repli commun), puis
    `EmulationVideoProcessingPipelineTests.cs:196`, `:260`, `:957` et `:1059` (couverture).

- **2026-09-02 — ScaleNx sans reprise du shader GPL**
  - Question : peut-on intégrer le shader Scale3x officiel alors que son en-tête impose la GNU GPL ?
  - Décision : ne copier aucun code du shader et réimplémenter indépendamment les règles publiques
    de Scale2x/Scale3x dans la référence CPU commune, sous la licence MIT du projet.
  - Motif : les règles de voisinage sont simples et vérifiables, tandis qu’une copie du fichier GPL
    serait incompatible avec la politique de licence déjà validée ; le repli commun maintient la
    même image sur les quatre surfaces.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:438`,
    `EmulationVideoSampling.cs:13`, `EmulationResourceKeys.cs:56`, les 30 ressources
    `Emulation.resx` via Argos, `CpuScaleNxVideoScaler.cs:1`,
    `CpuEmulationVideoProcessingPipeline.cs:339`, `OpenGlVideoSurface.cs:122`,
    `VeldridVideoSurface.cs:89`, puis `EmulationVideoProcessingPipelineTests.cs:197`, `:265`,
    `:963` et `:1095`.

- **2026-09-02 — SABR sans reprise du shader GPL**
  - Question : la variante SABR v3.0 peut-elle être intégrée directement au projet MIT ?
  - Décision : ne copier aucun élément de la source GPL-2.0-or-later et développer un scaler
    original à interpolation diagonale, documenté sous la licence MIT de GW GUI.
  - Motif : l’en-tête de la source est sans ambiguïté ; un noyau CPU commun à voisinage 3×3 permet
    néanmoins de fournir l’effet anti-crénelé attendu et une sortie identique sur les quatre
    renderers sans contamination de licence.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:456`,
    `EmulationVideoSampling.cs:14`, `EmulationResourceKeys.cs:57`, les 30 ressources
    `Emulation.resx` via Argos, `CpuSabrVideoScaler.cs:1`,
    `CpuEmulationVideoProcessingPipeline.cs:346`, `OpenGlVideoSurface.cs:122`,
    `VeldridVideoSurface.cs:89`, puis `EmulationVideoProcessingPipelineTests.cs:198`, `:270`,
    `:969` et `:1134`.

- **2026-09-02 — Dé-dithering indépendant avant scaler**
  - Question : un dé-dithering GW GUI duplique-t-il une option Amiga/Atari, et quelle première
    variante possède une licence compatible et un comportement borné ?
  - Décision : aucun doublon n’existe dans les deux modules ; adapter le détecteur de damiers MIT de
    Hyllian dans une passe CPU commune, avec une intensité `0..100` et valeur neutre `0`.
  - Motif : le motif en damier est vérifiable sans heuristique temporelle, la source est explicitement
    MIT et l’exécution avant scaler empêche que le redimensionnement masque le motif original.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:476`,
    `THIRD-PARTY-NOTICES.md:47`, `EmulationImageRestorationConfiguration.cs:1`,
    `EmulationVideoProcessingConfiguration.cs:11`,
    `EmulationVideoProcessingConfigurationFunctions.cs:17`,
    `EmulationVideoProcessingCatalog.cs:13`, `EmulationResourceKeys.cs:40`, les 30 ressources
    `Emulation.resx` via Argos, `EmulationVideoProcessingSettingsSection.cs:113`,
    `EmulationImageRestorationFunctions.cs:1`, `CpuEmulationVideoProcessingPipeline.cs:47`,
    `OpenGlVideoSurface.cs:23`, `VeldridVideoSurface.cs:44`, puis les tests de configuration,
    panneau et pipeline aux lignes `33`, `43`, `327` et `1210` de leurs fichiers respectifs.

- **2026-09-02 — Débruitage bilatéral original avant scaler**
  - Question : un débruitage GW GUI duplique-t-il une option Amiga/Atari, et la passe bilatérale
    Libretro peut-elle être intégrée directement au projet MIT ?
  - Décision : aucun doublon n’existe dans les deux modules ; ne reprendre aucun code du shader
    bilatéral GPL-2.0-or-later et développer une passe bilatérale 3×3 originale, avec intensité
    `0..100`, valeur neutre `0`, après le dé-dithering et avant le scaler.
  - Motif : la pondération spatiale et colorimétrique réduit les petites variations d’un aplat sans
    mélanger les deux côtés d’un contour marqué ; elle reste distincte de la reconnaissance des
    damiers et donne un résultat déterministe commun aux quatre renderers.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:497`,
    `EmulationImageRestorationConfiguration.cs:7`,
    `EmulationVideoProcessingConfigurationFunctions.cs:27`,
    `EmulationVideoProcessingCatalog.cs:14`, `EmulationResourceKeys.cs:83`, les 30 ressources
    `Emulation.resx` via Argos (`00-Base/Emulation.resx:824`, correction française validée à
    `fr-FR/Emulation.resx:687`), `EmulationVideoProcessingSettingsSection.cs:120`,
    `EmulationImageRestorationFunctions.cs:53`, `CpuEmulationVideoProcessingPipeline.cs:49`,
    `OpenGlVideoSurface.cs:126`, `VeldridVideoSurface.cs:93`, puis les tests de configuration,
    panneau et pipeline aux lignes `33`, `44`, `374` et `1292` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : tests ciblés `3/3`, équivalence renderer `4/4`, suite
    vidéo/configuration/interface/localisation `105/105`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Réduction des bandes originale sans grain**
  - Question : la réduction des bandes du catalogue duplique-t-elle une option Amiga/Atari, et la
    source Libretro peut-elle être reprise dans le projet MIT ?
  - Décision : aucun doublon n’existe dans les modules ; ne reprendre aucun code de la source
    GPL/LGPL issue de mpv et créer une reconstruction déterministe des faibles marches, avec
    intensité `0..100`, valeur neutre `0`, après débruitage et avant scaler.
  - Motif : la sélection d’une direction de gradient cohérente lisse les transitions quantifiées,
    tout en rejetant les pics locaux comme bruit et les grands écarts comme contours ; aucun grain
    pseudo-aléatoire n’est ajouté.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:520`,
    `EmulationImageRestorationConfiguration.cs:8`,
    `EmulationVideoProcessingConfigurationFunctions.cs:28`,
    `EmulationVideoProcessingCatalog.cs:15`, `EmulationResourceKeys.cs:84`, les 30 ressources
    `Emulation.resx` via Argos (`00-Base/Emulation.resx:825`, `fr-FR/Emulation.resx:688`),
    `EmulationVideoProcessingSettingsSection.cs:123`,
    `EmulationImageRestorationFunctions.cs:98`, `CpuEmulationVideoProcessingPipeline.cs:51`,
    `OpenGlVideoSurface.cs:127`, `VeldridVideoSurface.cs:94`, puis les tests de configuration,
    panneau et pipeline aux lignes `33`, `45`, `421` et `1371` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : tests ciblés `3/3`, équivalence renderer `4/4`, suite
    vidéo/configuration/interface/localisation `110/110`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Netteté avancée distincte sous le nom Récupération de détails**
  - Question : comment proposer une netteté avancée sans dupliquer le réglage général déjà présent ?
  - Décision : conserver `Netteté` comme ajustement global `-10..+10` après mise à l’échelle, et
    ajouter `Récupération de détails` comme restauration positive `0..100`, neutre à `0`, à la
    résolution source avant scaler ; aucun moteur Amiga/Atari n’expose cette fonction.
  - Motif : la passe renforce uniquement le résidu de micro-détail, réduit progressivement sa force
    près des contours marqués et borne la sortie autour des extrema locaux ; elle ne sert ni à
    flouter, ni à régler la netteté finale du modèle d’affichage.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:541`,
    `EmulationImageRestorationConfiguration.cs:9`,
    `EmulationVideoProcessingConfigurationFunctions.cs:29`,
    `EmulationVideoProcessingCatalog.cs:16`, `EmulationResourceKeys.cs:85`, les 30 ressources
    `Emulation.resx` via Argos (`00-Base/Emulation.resx:826`, correction française validée à
    `fr-FR/Emulation.resx:689`), `EmulationVideoProcessingSettingsSection.cs:126`,
    `EmulationImageRestorationFunctions.cs:142`, `CpuEmulationVideoProcessingPipeline.cs:53`,
    `OpenGlVideoSurface.cs:128`, `VeldridVideoSurface.cs:95`, puis les tests de configuration,
    panneau et pipeline aux lignes `34`, `46`, `468` et `1451` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : tests ciblés `3/3`, équivalence renderer `4/4`, suite
    vidéo/configuration/interface/localisation `115/115`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Désentrelacement spatial avec champ explicite**
  - Question : comment désentrelacer sans métadonnée d’entrelacement ou de dominance de champ, et
    sans dupliquer une option moteur ?
  - Décision : aucun doublon n’existe dans les modules Amiga/Atari ; proposer un select
    `Désactivé`, `Bob — lignes paires`, `Bob — lignes impaires` et `Fusion verticale`, sans
    détection automatique ni historique implicite.
  - Motif : le choix explicite évite de prétendre connaître le champ dominant ; Bob reconstruit les
    lignes manquantes depuis le champ conservé, tandis que Fusion réduit le peigne par pondération
    verticale au prix d’un adoucissement documenté. Le mode désactivé reste strictement neutre.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:562`,
    `EmulationDeinterlacingMode.cs:3`, `EmulationImageRestorationConfiguration.cs:11`,
    `EmulationVideoProcessingConfigurationFunctions.cs:30`,
    `EmulationVideoProcessingCatalog.cs:17`, `EmulationResourceKeys.cs:86`, les cinq clés dans les
    30 ressources `Emulation.resx` via Argos (`00-Base/Emulation.resx:827`, corrections françaises
    validées à `fr-FR/Emulation.resx:690`), `EmulationVideoProcessingSettingsSection.cs:129`,
    `EmulationImageRestorationFunctions.cs:11`, `CpuEmulationVideoProcessingPipeline.cs:47`,
    `OpenGlVideoSurface.cs:129`, `VeldridVideoSurface.cs:96`, puis les tests de configuration,
    panneau et pipeline aux lignes `34`, `47`, `515` et `1531` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : tests ciblés `3/3`, équivalence renderer `4/4`, suite
    vidéo/configuration/interface/localisation `120/120`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Audit des incompatibilités des filtres indépendants**
  - Question : quelles combinaisons exigent une nouvelle confirmation Oui/Non ?
  - Décision : aucune incompatibilité interne n’existe parmi les filtres implémentés ; ne créer
    aucune boîte inutile. Les scalers sont exclusifs par leur select unique et les cinq restaurations
    sont composables dans un ordre fixe. La confirmation existante reste réservée au remplacement
    d’une technologie d’affichage active.
  - Motif : un test exécute simultanément toutes les restaurations avec chacun des dix scalers sans
    effacer de valeur ; le test d’interface vérifie désormais qu’un refus de remplacement conserve
    l’intégralité de la configuration, y compris scaler et restaurations.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:614`,
    `EmulationVideoProcessingPipelineTests.cs:1563` et
    `EmulationVideoSettingsSectionTests.cs:224`.
  - Vérifications terminées avant cochage : tests ciblés `2/2`, suite
    vidéo/configuration/interface/localisation `121/121`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Validation visuelle consolidée des filtres avancés**
  - Question : comment laisser une preuve visuelle comparable de chaque scaler et restauration sur
    les quatre renderers malgré leurs tailles natives différentes hors écran ?
  - Décision : produire une image déterministe contenant damier, bandes, bruit, micro-détail,
    entrelacement et diagonales ; générer douze cellules (témoin, six scalers, cinq restaurations),
    vérifier que chaque cellule active diffère du témoin, puis écrire une planche PNG par renderer.
  - Motif : les tests dédiés assurent déjà l’équivalence fonctionnelle 4/4 ; les planches natives
    complètent cette preuve sans comparer abusivement WPF 598×48 aux hôtes GPU 1510×88. OpenGL,
    Direct3D 11 et Vulkan produisent exactement le même PNG et le même SHA-256.
  - Modifications réalisées : `EmulationVideoProcessingPipelineTests.cs:635`, image source à
    `:1784`, écriture PNG généralisée à `:1837` et racine du dépôt à `:1875`. Artefacts :
    `build/validation/advanced-filter-validation/advanced-wpf.png`, `advanced-opengl.png`,
    `advanced-direct3d11.png` et `advanced-vulkan.png`.
  - Vérifications terminées avant cochage : planche ciblée `1/1`, suite finale
    vidéo/configuration/interface/localisation `122/122`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — VFD comme technologie d’affichage spécialisée**
  - Question : quels paramètres VFD peuvent être simulés honnêtement depuis une frame raster sans
    machine actuelle fournissant les segments physiques ?
  - Décision : ajouter VFD comme technologie exclusive avec phosphore bleu, vert, ambre ou rouge,
    intensité `70`, halo `25` et persistance `20` par défaut, les trois intensités restant réglables
    sur `0..100`.
  - Motif : les documents constructeur Noritake confirment l’émission par phosphore, les couleurs et
    la luminance ; la passe est explicitement une approximation raster monochrome avec halo 3×3 et
    historique borné, sans code ni actif tiers.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md:602`,
    `EmulationVideoDisplayTechnology.cs:10`, `EmulationVfdColor.cs:3`,
    `EmulationVfdVideoConfiguration.cs:5`, `EmulationVideoProcessingConfiguration.cs:16`,
    `EmulationVideoProcessingConfigurationFunctions.cs:84`,
    `EmulationVideoProcessingCatalog.cs:57`, `EmulationResourceKeys.cs:49`, les neuf clés dans les
    30 ressources `Emulation.resx` via Argos (`00-Base/Emulation.resx:832`, corrections françaises
    à `fr-FR/Emulation.resx:695`), `EmulationVideoProcessingSettingsSection.cs:302`,
    `CpuVfdVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:216`,
    `OpenGlVideoSurface.cs:130`, `VeldridVideoSurface.cs:97`, puis les tests aux lignes `195`,
    `1669`, `39` et `35` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage VFD et matrice quatre renderers `4/4`,
    configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement ni
    erreur et `git diff --check` sans erreur.

- **2026-09-02 — Matrice LED comme technologie d’affichage spécialisée**
  - Question : comment distinguer une matrice LED du LCD rétroéclairé par LED déjà présent sans
    prétendre disposer de la géométrie physique de la machine émulée ?
  - Décision : ajouter une technologie exclusive `Matrice LED`, avec select RGB, rouge, vert, ambre,
    bleu ou blanc, puis taille des cellules `35`, espacement `30`, diffusion `20` et luminosité `75`
    par défaut ; tous les réglages numériques sont bornés à `0..100`.
  - Motif : les guides matériels Adafruit confirment les panneaux à pas distincts et l’effet d’une
    plaque diffusante. La passe est donc explicitement une approximation raster par cellules de
    `2..8` pixels, masque circulaire, espace sombre et diffusion, sans code ni actif tiers.
  - Compatibilité : technologie exclusive des autres affichages, mais compatible avec chaque scaler,
    restauration et réglage général. WPF utilise le pipeline CPU commun ; OpenGL, Direct3D 11 et
    Vulkan utilisent son repli CPU déterministe.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:11`, `EmulationLedMatrixColor.cs:3`,
    `EmulationLedMatrixVideoConfiguration.cs:5`, `EmulationVideoProcessingConfiguration.cs:17`,
    `EmulationVideoProcessingConfigurationFunctions.cs:18`,
    `EmulationVideoProcessingCatalog.cs:61`, `EmulationResourceKeys.cs:50`, les douze clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises à
    `fr-FR/Emulation.resx:704`), `EmulationVideoProcessingSettingsSection.cs:319`,
    `CpuLedMatrixVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:318`,
    `OpenGlVideoSurface.cs:130`, `VeldridVideoSurface.cs:97`, puis les tests aux lignes `41`, `207`,
    `1714` et `37` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    rendu LED et matrice quatre renderers `7/7`, classes complètes configuration/localisation/interface
    `18/18`, compilation GWGUI.App sans avertissement ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Matrice de points distincte de la matrice LED et des segments**
  - Question : quels paramètres permettent une simulation utile depuis une frame raster sans
    confondre matrice de points LCD, matrice LED et futur affichage à segments ?
  - Décision : ajouter une technologie exclusive `Matrice de points` avec palettes LCD vert, LCD
    gris, ambre ou bleu, forme ronde ou carrée, taille `55`, contraste `70` et réponse `120 ms` par
    défaut. Taille et contraste utilisent `0..100`, la réponse `0..1000 ms`.
  - Motif : les références Crystalfontz et Newhaven confirment la matrice LCD et une taille de point
    matérielle. La passe originale moyenne la luminance par cellule, masque chaque point et interpole
    fond/encre ; un historique séparé applique la réponse exponentielle, sans actif ni code tiers.
  - Compatibilité : technologie exclusive des autres affichages, compatible avec tous les scalers,
    restaurations et réglages généraux ; repli CPU commun déterministe pour WPF, OpenGL, Direct3D 11
    et Vulkan.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:12`, `EmulationDotMatrixPalette.cs:3`,
    `EmulationDotMatrixShape.cs:3`, `EmulationDotMatrixVideoConfiguration.cs:5`,
    `EmulationVideoProcessingConfiguration.cs:18`,
    `EmulationVideoProcessingConfigurationFunctions.cs:19`,
    `EmulationVideoProcessingCatalog.cs:66`, `EmulationResourceKeys.cs`, les douze clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises à
    `fr-FR/Emulation.resx:716`), `EmulationVideoProcessingSettingsSection.cs:339`,
    `CpuDotMatrixVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:255`,
    `OpenGlVideoSurface.cs:132`, `VeldridVideoSurface.cs:99`, puis les tests aux lignes `43`, `221`,
    `1768` et `39` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    rendu spatial/temporel et matrice quatre renderers `7/7`, classes complètes
    configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement ni erreur
    et `git diff --check` sans erreur.

- **2026-09-02 — Affichages à 7, 14 et 16 segments**
  - Question : comment rendre un affichage à segments depuis une frame raster sans prétendre décoder
    les chiffres ou caractères propres à une machine ?
  - Décision : ajouter une technologie exclusive avec dispositions 7, 14 ou 16 segments, couleurs
    rouge, verte, ambre, bleue ou blanche, épaisseur `55`, contraste `80`, halo `20` et réponse
    `30 ms` par défaut ; les trois intensités utilisent `0..100` et la réponse `0..1000 ms`.
  - Motif : les fiches Broadcom confirment les géométries sept et quatorze segments, l’émission LED
    uniforme et l’intérêt d’une surface contrastée. La passe reste une approximation raster
    géométrique, complétée par seize segments, sans décodage sémantique ni actif ou code tiers.
  - Compatibilité : technologie exclusive des autres affichages, compatible avec tous les scalers,
    restaurations et réglages généraux ; historique de réponse séparé et repli CPU commun
    déterministe pour WPF, OpenGL, Direct3D 11 et Vulkan.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:13`, `EmulationSegmentDisplayLayout.cs:3`,
    `EmulationSegmentDisplayColor.cs:3`, `EmulationSegmentDisplayVideoConfiguration.cs:5`,
    `EmulationVideoProcessingConfiguration.cs:19`,
    `EmulationVideoProcessingConfigurationFunctions.cs:20`,
    `EmulationVideoProcessingCatalog.cs:71`, `EmulationResourceKeys.cs`, les quinze clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises à
    `fr-FR/Emulation.resx:728`), `EmulationVideoProcessingSettingsSection.cs:363`,
    `CpuSegmentDisplayVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:298`,
    `OpenGlVideoSurface.cs:133`, `VeldridVideoSurface.cs:100`, puis les tests aux lignes `45`, `235`,
    `1839` et `41` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    géométrie/couleurs/halo/réponse et matrice quatre renderers `7/7`, classes complètes
    configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement ni erreur
    et `git diff --check` sans erreur.

- **2026-09-02 — Papier électronique à palette et historique dédiés**
  - Question : quels paramètres rendent le papier électronique utile sans le réduire à un simple
    filtre niveaux de gris ni dupliquer la réponse LCD ?
  - Décision : ajouter une technologie exclusive avec modes monochrome, seize niveaux de gris ou
    4096 couleurs, contraste `70`, tramage `35`, rafraîchissement `500 ms` et image fantôme `20` par
    défaut ; les intensités utilisent `0..100` et le rafraîchissement `0..1000 ms`.
  - Motif : E Ink documente seize gris, 4096 couleurs, le tramage, les vitesses de rafraîchissement et
    le ghosting. La passe originale quantifie avec une matrice Bayer et utilise un historique propre,
    sans code ni actif tiers.
  - Compatibilité : technologie exclusive des autres affichages, compatible avec tous les scalers,
    restaurations et réglages généraux ; repli CPU commun déterministe pour WPF, OpenGL,
    Direct3D 11 et Vulkan.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:14`, `EmulationEPaperColorMode.cs:3`,
    `EmulationEPaperVideoConfiguration.cs:5`, `EmulationVideoProcessingConfiguration.cs:20`,
    `EmulationVideoProcessingConfigurationFunctions.cs:22`,
    `EmulationVideoProcessingCatalog.cs:77`, `EmulationResourceKeys.cs`, les neuf clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises à
    `fr-FR/Emulation.resx:743`), `EmulationVideoProcessingSettingsSection.cs:392`,
    `CpuEPaperVideoProcessingPasses.cs:6`, `CpuEmulationVideoProcessingPipeline.cs:341`,
    `OpenGlVideoSurface.cs:134`, `VeldridVideoSurface.cs:101`, puis les tests aux lignes `48`, `249`,
    `1911` et `43` de leurs fichiers respectifs.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    modes/contraste/tramage/rafraîchissement/ghosting et matrice quatre renderers `7/7`, classes
    complètes configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement
    ni erreur et `git diff --check` sans erreur.

- **2026-09-02 — Projection à traitement optique raster**
  - Question : la projection apporte-t-elle un rendu distinct des technologies d’écran déjà
    présentes, et quels réglages peuvent rester compréhensibles et bornés ?
  - Décision : ajouter une technologie exclusive avec flou optique `20`, diffusion lumineuse `15`,
    texture de toile `10` et convergence RGB `5` par défaut, chaque intensité étant réglable sur
    `0..100`. La convergence translate le rouge et le bleu autour du vert jusqu’à trois pixels.
  - Motif : la documentation Epson distingue l’alignement des panneaux rouge et bleu sur le vert et
    l’influence de la surface de projection. Une passe originale reproduit ces propriétés sans code,
    shader ni actif tiers ; elle reste sous licence MIT.
  - Compatibilité : technologie exclusive des autres affichages, compatible avec tous les scalers,
    restaurations et réglages généraux ; traitement spatial sans historique, avec repli CPU commun
    déterministe pour WPF, OpenGL, Direct3D 11 et Vulkan.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationVideoDisplayTechnology.cs:15`, `EmulationProjectionVideoConfiguration.cs:3`,
    `EmulationVideoProcessingConfiguration.cs:21`,
    `EmulationVideoProcessingConfigurationFunctions.cs:23`,
    `EmulationVideoProcessingCatalog.cs:82`, `EmulationResourceKeys.cs:54`, les cinq clés dans les
    30 ressources `Emulation.resx` via Argos (corrections françaises dans `fr-FR/Emulation.resx`),
    `EmulationVideoProcessingSettingsSection.cs:414`,
    `CpuProjectionVideoProcessingPasses.cs:5`, `CpuEmulationVideoProcessingPipeline.cs:475`,
    `OpenGlVideoSurface.cs:135`, `VeldridVideoSurface.cs:102`, puis les tests de configuration,
    interface et rendu, dont `EmulationVideoProcessingPipelineTests.cs:1973`.
  - Vérifications terminées avant cochage : ciblage normalisation/persistance/interface/localisation,
    flou/diffusion/texture/convergence et matrice quatre renderers `7/7`, classes complètes
    configuration/localisation/interface `18/18`, compilation GWGUI.App sans avertissement ni erreur
    et `git diff --check` sans erreur.

- **2026-09-02 — Rémanence générale indépendante des technologies d’écran**
  - Question : comment proposer une traînée lumineuse générale sans confondre son intensité avec le
    temps de réponse ou la persistance interne des LCD/OLED et autres affichages spécialisés ?
  - Décision : ajouter un contrat temporel indépendant et un panneau permanent. `Rémanence générale`
    utilise une intensité `0..100`, neutre et initialisée à `0`, puis conserve par maximum une part de
    l’unique frame précédente ; aucune durée en millisecondes n’est introduite.
  - Motif : la spécification slang valide le besoin architectural d’un historique de frames, sans que
    GW GUI ne reprenne de shader ni de code tiers. L’algorithme original, sous MIT, possède son propre
    historique et intervient après les réponses propres aux technologies d’écran.
  - Compatibilité : compatible avec toutes les technologies, scalers, restaurations et réglages
    généraux ; une valeur nulle, une première frame, un changement de taille, une séquence non
    croissante ou la destruction du pipeline réinitialisent l’historique. Les quatre renderers
    utilisent le pipeline CPU commun lorsque l’effet est actif.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationTemporalVideoConfiguration.cs:3`, `EmulationVideoProcessingConfiguration.cs`,
    `EmulationVideoProcessingConfigurationFunctions.cs:41`,
    `EmulationVideoProcessingCatalog.cs:18`, `EmulationResourceKeys.cs:94`, les deux clés dans les
    30 ressources `Emulation.resx` via Argos (correction française à `fr-FR/Emulation.resx:758`),
    `EmulationVideoProcessingSettingsSection.cs:138`,
    `CpuEmulationVideoProcessingPipeline.cs:389`, `OpenGlVideoSurface.cs:130`,
    `VeldridVideoSurface.cs:97`, puis les tests aux lignes `35`, `60`, `275` et `1977` de leurs
    fichiers respectifs.
  - Vérifications terminées avant cochage : normalisation/persistances Amiga et Atari/interface/effet
    et remises à zéro ciblés `5/5`, matrice quatre renderers `1/1`, classes complètes
    configuration/interface `16/16` et localisation `2/2`, compilation GWGUI.App sans avertissement
    ni erreur ; contrôle final de format réalisé séparément juste avant cochage.

- **2026-09-02 — Flou de mouvement limité à la frame précédente**
  - Question : comment différencier le flou de mouvement de la rémanence générale déjà cumulative ?
  - Décision : ajouter une intensité indépendante `0..100`, neutre et initialisée à `0`, qui mélange
    la frame courante avec la précédente puis mémorise la frame courante non mélangée. L’effet reste
    donc limité à une seule frame.
  - Motif : l’historique de frames prévu par la spécification slang convient à l’architecture, mais
    l’algorithme est original, sous MIT, sans code, shader, coefficient ni actif tiers.
  - Compatibilité : compatible avec toutes les technologies et tous les traitements indépendants ;
    historique séparé réinitialisé à la première frame, à zéro, au redimensionnement, en cas de
    séquence non croissante ou à la destruction. Il précède la rémanence générale dans la chaîne.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationTemporalVideoConfiguration.cs:5`,
    `EmulationVideoProcessingConfigurationFunctions.cs:44`,
    `EmulationVideoProcessingCatalog.cs:19`, `EmulationResourceKeys.cs:95`, la clé ajoutée aux
    30 ressources `Emulation.resx` via Argos, `EmulationVideoProcessingSettingsSection.cs:144`,
    `CpuEmulationVideoProcessingPipeline.cs`, `OpenGlVideoSurface.cs:131`,
    `VeldridVideoSurface.cs:98`, puis les tests aux lignes `35`, `61`, `279` et `2023` de leurs
    fichiers respectifs.
  - Vérifications terminées avant cochage : normalisation/persistances/interface/localisation/effet
    ciblés `7/7`, matrice quatre renderers `1/1`, classes complètes configuration/localisation/interface
    `18/18` ; compilation et contrôle de format finaux réalisés juste avant cochage.

- **2026-09-02 — Scintillement modulé distinct des images noires**
  - Question : comment rendre le scintillement visible sans anticiper ni dupliquer l’insertion
    d’images noires prévue par une autre tâche ?
  - Décision : ajouter une intensité indépendante `0..100`, neutre et initialisée à `0`. Les frames
    impaires sont atténuées jusqu’à `50 %`, les frames paires restent intactes ; aucune frame n’est
    supprimée, même à l’intensité maximale.
  - Motif : la modulation par `VideoFrame.Sequence` est déterministe, ne requiert aucun historique et
    constitue une formule originale sous MIT, sans code, shader, coefficient ni actif tiers.
  - Compatibilité : compatible avec toutes les technologies, scalers, restaurations et effets
    temporels ; elle intervient avant le flou de mouvement et la rémanence générale. Les quatre
    renderers utilisent le pipeline CPU commun lorsque l’effet est actif.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationTemporalVideoConfiguration.cs`,
    `EmulationVideoProcessingConfigurationFunctions.cs`,
    `EmulationVideoProcessingCatalog.cs`, `EmulationResourceKeys.cs`, la clé dans les 30 ressources
    `Emulation.resx` via Argos (correction française en `Scintillement`),
    `EmulationVideoProcessingSettingsSection.cs`, `CpuEmulationVideoProcessingPipeline.cs`,
    `OpenGlVideoSurface.cs`, `VeldridVideoSurface.cs`, puis les tests de configuration, interface,
    alternance bornée et matrice quatre renderers.
  - Vérifications terminées avant cochage : ciblage configuration/persistances/interface/localisation
    et rendu `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et contrôle de format finaux réalisés
    juste avant cochage.

- **2026-09-02 — Entrelacement simulé séparé du désentrelacement**
  - Question : comment ajouter un effet entrelacé sans le confondre avec la correction de sources
    entrelacées déjà placée dans la restauration ?
  - Décision : ajouter une intensité indépendante `0..100`, neutre et initialisée à `0`, qui atténue
    alternativement les lignes paires et impaires selon `VideoFrame.Sequence`, avec un plancher de
    `25 %` à l’intensité maximale.
  - Motif : l’effet agit après le rendu pour créer volontairement des champs alternés, tandis que le
    désentrelacement agit avant redimensionnement sur une source existante. La formule est originale,
    déterministe, sans historique ni actif tiers, sous MIT.
  - Compatibilité : compatible avec toutes les technologies, scalers, restaurations et autres effets
    temporels ; le repli CPU commun garantit le même résultat sur les quatre renderers.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`, le contrat temporel, sa
    normalisation et son catalogue, la constante de localisation, la clé générée par Argos dans les
    30 ressources `Emulation.resx`, le panneau temporel, le pipeline CPU, les replis OpenGL/Veldrid,
    puis les tests de bornage, persistance, interface, alternance des champs et quatre renderers.
  - Vérifications terminées avant cochage : ciblage configuration/persistances/interface/localisation
    et rendu `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et contrôle de format finaux réalisés
    juste avant cochage.

- **2026-09-02 — Insertion d’images noires appliquée en dernier**
  - Question : comment garantir que cette option reste différente du scintillement et qu’un autre
    traitement temporel ne rééclaire pas la frame noire ?
  - Décision : ajouter un interrupteur indépendant, désactivé par défaut, qui conserve les séquences
    paires et met entièrement à noir les séquences impaires. La passe est la dernière de la chaîne.
  - Motif : l’alternance déterministe par `VideoFrame.Sequence` ne demande aucun historique et la
    position finale préserve la sémantique BFI. La formule originale reste sous MIT sans actif tiers.
  - Compatibilité : compatible avec toutes les technologies, scalers, restaurations et effets
    temporels ; distincte du scintillement qui conserve au moins la moitié de la lumière. Les quatre
    renderers utilisent le pipeline CPU commun lorsque l’option est active.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`, le contrat temporel et son
    catalogue, la constante de localisation, la clé générée par Argos dans les 30 ressources
    `Emulation.resx` avec correction française, le panneau temporel, le pipeline CPU, les replis
    OpenGL/Veldrid, puis les tests de persistance, interface, ordre final et quatre renderers.
  - Vérifications terminées avant cochage : ciblage persistances/interface/localisation/rendu `6/6`,
    matrice quatre renderers `1/1`, classes complètes configuration/localisation/interface `18/18` ;
    compilation et contrôle de format finaux réalisés juste avant cochage.

- **2026-09-02 — Simulation composite GW GUI sans duplication moteur**
  - Question : l’effet composite peut-il être proposé sans recréer les normes, timings ou artefacts
    déjà produits par PUAE, Hatari ou Atari800 ?
  - Décision : créer un panneau permanent `Simulations de signal` et une intensité composite
    `0..100`, neutre à `0`, qui agit uniquement sur le `VideoFrame` déjà produit. Les options moteur
    PAL/NTSC, fréquence, région et artifacting ne sont ni lues, ni écrites, ni remplacées.
  - Motif : le catalogue common-shaders confirme la famille visuelle, tandis que l’audit des moteurs
    réserve la génération du signal à l’émulateur. La passe originale limite la chrominance, adoucit
    la luminance horizontalement et ajoute une faible alternance de phase ; elle reste sous MIT sans
    reprise de code ou d’actif tiers.
  - Compatibilité : compatible avec toutes les technologies et traitements ; placée après restauration
    et avant scaler. WPF, OpenGL, Direct3D 11 et Vulkan partagent le repli CPU déterministe lorsqu’elle
    est active.
  - Modifications réalisées : `docs/reference/emulation-video-filters.md`,
    `EmulationSignalSimulationConfiguration.cs`, le contrat vidéo principal, sa normalisation et le
    catalogue, les constantes de localisation, deux clés générées via Argos dans les 30 ressources,
    le panneau permanent, `CpuCompositeVideoProcessingPasses.cs`, le pipeline CPU, les replis
    OpenGL/Veldrid, puis les tests de bornage, persistance, interface, phases composite et renderers.
  - Vérifications terminées avant cochage : ciblage configuration/persistances/interface/localisation
    et rendu `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et contrôle de format finaux réalisés
    juste avant cochage.

- **2026-09-02 — Simulation S-Video GW GUI à luminance séparée**
  - Question : comment rendre S-Video distinct de composite sans recréer une option de connectique du
    moteur ?
  - Décision : ajouter une intensité post-traitement `0..100`, neutre à `0`, qui préserve la luminance
    linéaire et ne lisse que la chrominance horizontale à hauteur maximale de `12 %`, sans phase ni
    dot crawl.
  - Motif : cette séparation reproduit la différence visuelle utile avec composite tout en agissant
    uniquement sur le `VideoFrame`. La passe originale reste sous MIT sans reprise tierce.
  - Compatibilité : compatible avec toutes les technologies et traitements ; les simulations
    explicitement demandées peuvent se composer, mais aucune norme, région, fréquence ou option
    d’artifacting du moteur n’est lue ou modifiée. Repli CPU commun sur les quatre renderers.
  - Modifications réalisées : référence, contrat et normalisation des simulations, catalogue et
    localisation, clé Argos dans 30 ressources, panneau, `CpuSVideoVideoProcessingPasses.cs`, pipeline,
    replis GPU, tests de persistance/interface, indépendance de séquence et luminance linéaire.
  - Vérifications terminées avant cochage : après correction du test pour mesurer la lumière linéaire,
    ciblage S-Video vert, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et format vérifiés juste avant cochage.

- **2026-09-02 — Simulation RF GW GUI déterministe**
  - Question : comment évoquer une transmission RF sans toucher au tuner, à la fréquence ou à la
    région du moteur ?
  - Décision : ajouter une intensité post-traitement `0..100`, neutre à `0`, qui adoucit
    horizontalement l’image puis ajoute un bruit commun borné à `±8 %`, déterminé par position et
    numéro de frame.
  - Motif : cette dégradation reste visuelle et reproductible, sans état global, tout en étant
    distincte de composite et S-Video. La passe originale reste sous MIT sans reprise tierce.
  - Compatibilité : compatible avec toutes les technologies et traitements ; aucune option moteur
    n’est lue ou modifiée. Repli CPU commun sur WPF, OpenGL, Direct3D 11 et Vulkan.
  - Modifications réalisées : référence, contrat/normalisation/catalogue/localisation RF, clé Argos
    dans 30 ressources, panneau, `CpuRfVideoProcessingPasses.cs`, pipeline, replis GPU et tests de
    persistance/interface, déterminisme, changement de séquence et renderers.
  - Vérifications terminées avant cochage : ciblage RF `7/7`, matrice quatre renderers `1/1`, classes
    complètes configuration/localisation/interface `18/18` ; compilation et format vérifiés juste
    avant cochage.

- **2026-09-02 — Simulation PAL GW GUI sans changement de norme**
  - Question : comment évoquer PAL sans modifier le standard ou les timings déjà gérés par le moteur ?
  - Décision : intensité post-traitement `0..100`, neutre à `0`, mélange chromatique vertical borné
    et faible phase alternée `±2,5 %` selon la parité des lignes, indépendante de la séquence.
  - Motif : l’effet reste uniquement visuel ; la norme, la région et la fréquence demeurent des
    options moteur. Passe originale MIT sans reprise tierce.
  - Compatibilité : toutes technologies et traitements, repli CPU commun sur les quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation PAL, clé Argos dans
    30 ressources, panneau, `CpuPalVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : ciblage PAL `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et format vérifiés avant cochage.

- **2026-09-02 — Simulation NTSC GW GUI sans changement de norme**
  - Question : comment évoquer NTSC sans modifier standard, fréquence, région ou timing du moteur ?
  - Décision : intensité post-traitement `0..100`, neutre à `0`, avec léger mélange de luminance,
    retard chromatique horizontal et phase de teinte à trois états reproductible par séquence.
  - Motif : l’effet reste visuel et explicitement nommé ; passe originale MIT sans reprise tierce.
  - Compatibilité : toutes technologies et traitements, repli CPU commun sur les quatre renderers,
    aucune option moteur lue ou modifiée.
  - Modifications : référence, contrat/normalisation/catalogue/localisation NTSC, clé Argos dans
    30 ressources avec correction française, panneau, `CpuNtscVideoProcessingPasses.cs`, pipeline,
    replis GPU et tests de déterminisme, phase et renderers.
  - Vérifications : ciblage NTSC `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et format vérifiés avant cochage.

- **2026-09-02 — Grain stylistique distinct du bruit RF**
  - Question : comment ajouter du grain sans réutiliser ni confondre le bruit de transmission RF ?
  - Décision : nouveau contrat et panneau `Effets stylistiques`, intensité `0..100` neutre à `0`,
    bruit monochrome déterministe borné à `±7 %` et appliqué à la résolution de sortie.
  - Motif : RF reste une dégradation du signal avant scaler ; le grain est un effet final fin. Passe
    originale MIT sans code, texture ou actif tiers.
  - Compatibilité : toutes technologies et traitements, repli CPU commun sur les quatre renderers.
  - Modifications : référence, contrat stylistique, configuration/normalisation/catalogue,
    localisation et deux clés Argos dans 30 ressources avec correction française, panneau,
    `CpuGrainVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : ciblage grain `7/7`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/interface `18/18` ; compilation et format vérifiés avant cochage.

- **2026-09-02 — VHS déterministe à la résolution de sortie**
  - Question : quels défauts VHS restent distincts du grain et des simulations de signal ?
  - Décision : intensité `0..100` neutre à `0`, décalage par ligne jusqu’à trois pixels, bavure rouge
    et bleue jusqu’à `45 %`, et atténuation déterministe d’une ligne sur dix-sept.
  - Motif : effet stylistique final original MIT, sans code ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation VHS, clé Argos dans
    30 ressources, panneau, `CpuVhsVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : test VHS corrigé pour comparer les lignes en excluant alpha et tenir compte du
    transfert sRGB, matrice quatre renderers `1/1`, classes complètes `18/18` ; build et format avant coche.

- **2026-09-02 — Aberration chromatique spatiale et déterministe**
  - Question : quelle séparation RGB reste lisible sans devenir une seconde simulation de signal ?
  - Décision : intensité `0..100` neutre à `0`, rouge et bleu décalés en sens opposés jusqu’à
    trois pixels avec interpolation, vert conservé et bords pincés.
  - Motif : effet stylistique final original MIT, sans code, shader, texture ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; après VHS, avant grain et effets
    temporels ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation, clé Argos dans
    30 ressources, panneau, `CpuChromaticAberrationVideoProcessingPasses.cs`, pipeline, replis GPU
    et tests.
  - Vérifications : test dédié `1/1`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/UI `18/18` ; build et format avant coche.

- **2026-09-02 — Bloom borné des hautes lumières**
  - Question : comment proposer un bloom général sans dupliquer les halos propres aux technologies ?
  - Décision : intensité `0..100` neutre à `0`, seuil linéaire à `60 %`, diffusion dans un
    rayon de deux pixels et réaddition bornée à `35 %`.
  - Motif : effet stylistique final original MIT, sans code, shader, texture ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; les halos CRT, vectoriel et VFD restent
    des paramètres distincts ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation Bloom, clé Argos dans
    30 ressources, panneau, `CpuBloomVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : test dédié `1/1`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/UI `18/18` ; build et format avant coche.

- **2026-09-02 — Sépia progressif en lumière linéaire**
  - Question : quelle transformation sépia reste progressive et préserve un état réellement neutre ?
  - Décision : intensité `0..100` neutre à `0`, luminance Rec. 709 puis cible chaude
    `1,07 / 0,93 / 0,74`, mélangée à la couleur source et bornée.
  - Motif : effet stylistique final original MIT, sans code, shader, texture ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; les niveaux de gris appliqués ensuite
    retirent explicitement la teinte ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation Sépia, clé Argos dans
    30 ressources, panneau, `CpuSepiaVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : test dédié `1/1`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/UI `18/18` ; build et format avant coche.

- **2026-09-02 — Niveaux de gris progressifs par luminance**
  - Question : comment distinguer l’effet global des palettes et modes monochromes des écrans ?
  - Décision : intensité `0..100` neutre à `0`, luminance linéaire Rec. 709 mélangée aux trois
    composantes ; à `100`, RGB sont égaux sans modifier la technologie choisie.
  - Motif : effet stylistique final original MIT, sans code, shader, texture ni actif tiers.
  - Compatibilité : toutes technologies et traitements ; après sépia, dont la teinte est retirée
    à intensité maximale ; repli CPU commun aux quatre renderers.
  - Modifications : référence, contrat/normalisation/catalogue/localisation Grayscale, clé Argos
    dans 30 ressources avec correction française « Niveaux de gris », panneau,
    `CpuGrayscaleVideoProcessingPasses.cs`, pipeline, replis GPU et tests.
  - Vérifications : test dédié `1/1`, matrice quatre renderers `1/1`, classes complètes
    configuration/localisation/UI `18/18` ; build et format avant coche.

- **2026-09-02 — Validation réelle finale du point 7**
  - Question : comment couvrir rapidement les transitions d’affichage sans confondre les options
    natives des moteurs avec les traitements communs de GW GUI ?
  - Décision : utiliser une seule instance Debug, conserver les réglages dans leurs configurations
    Amiga et Atari respectives, puis compléter l’observation réelle Direct3D 11 par la matrice
    automatisée des quatre renderers et du repli WPF.
  - Modifications vérifiées : configurations Amiga `be1c4348b0a84927b53a29e940251e6f`
    et Atari `6c532ba3b52f451680029fa836a020fc`; aucune modification de code supplémentaire.
  - Vérifications : même PID `12004` pour Amiga et Atari ; technologies CRT et Plasma persistées
    séparément ; Amiga 1200 ouvert en Direct3D 11 ; redimensionnement et plein écran fonctionnels ;
    matrices WPF/OpenGL/Direct3D 11/Vulkan et repli WPF couvertes par les tests ciblés ; fermeture
    propre ; journal d’erreurs inchangé à `13 227` octets ; suite finale `145/145` réussie.

- [x] Créer le document de recherche avant d’y inscrire des résultats
  - [x] Créer le fichier vide docs/reference/emulation-video-filters.md.
  - [x] Modifier docs/reference/emulation-video-filters.md pour décrire le périmètre, distinguer les filtres réalisés par GW GUI des options de signal fournies par les émulateurs et reprendre les questions encore ouvertes de la section Filtres vidéo.

- [x] Établir le catalogue depuis les sources de référence
  - [x] Modifier docs/reference/emulation-video-filters.md à partir de la documentation officielle Libretro et des catalogues officiels slang-shaders et common-shaders pour recenser les familles de filtres, notamment CRT, scanlines, LCD et moiré horizontal ou vertical, avec un lien vers chaque source consultée.
  - [x] Modifier docs/reference/emulation-video-filters.md pour inscrire, pour chaque filtre ou famille, son effet, ses réglages utiles, ses dépendances éventuelles, ses combinaisons connues et son statut de licence ; ne recopier aucun code de shader dont la licence n’autorise pas clairement l’usage retenu.
  - [x] Modifier docs/reference/emulation-video-filters.md après examen des moteurs Amiga et Atari utilisés par le projet pour séparer leurs options RGB, composite, S-Video, RF, PAL, NTSC ou équivalentes des effets propres à GW GUI.
  - [x] Modifier docs/reference/emulation-video-filters.md pour inclure les filtres utiles aux futures machines sans limiter le catalogue aux capacités Amiga et Atari actuelles.

- [x] Comparer le catalogue aux quatre surfaces de rendu actuelles
  - [x] Modifier docs/reference/emulation-video-filters.md après examen de src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs, src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs et src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour décrire où les pixels sont disponibles et où un traitement commun peut être appliqué.
  - [x] Modifier docs/reference/emulation-video-filters.md pour comparer WPF, OpenGL, Direct3D 11 et Vulkan pour chaque famille de filtre, indiquer ce qui peut partager une définition et ce qui exige une implémentation de backend, sans choisir silencieusement de supprimer un renderer.
  - [x] Modifier docs/reference/emulation-video-filters.md pour décrire l’effet du traitement envisagé sur Snapshot, le rapport d’aspect, le redimensionnement, le repli actuel vers WPF et l’application immédiate à une instance ouverte.

- [x] Valider les groupes, compatibilités et réglages avant l’architecture définitive
  - [x] Modifier docs/reference/emulation-video-filters.md pour proposer, à partir du catalogue établi, les groupes logiques, les combinaisons compatibles et les incompatibilités nécessitant la confirmation décrite dans la demande.
  - [x] Modifier docs/reference/emulation-video-filters.md pour proposer les présélections et les réglages propres à chaque fonctionnalité, sans dupliquer luminosité, contraste, gamma, saturation et netteté.
  - [x] Modifier docs/reference/emulation-video-filters.md après validation pour remplacer les propositions par les technologies, sous-choix, compatibilités et familles ultérieures retenus.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour inscrire toutes les décisions fonctionnelles déjà validées sans marquer comme validés le gamma, Snapshot ou les valeurs exactes des présélections.
  - [x] Déterminer la plage, la valeur neutre et la conversion du gamma, puis modifier docs/reference/emulation-video-filters.md et la section Filtres vidéo du présent document avec la décision exacte.
  - [x] Déterminer si Snapshot contient l’image traitée ou l’image brute, puis inscrire cette décision dans docs/reference/emulation-video-filters.md et docs/architecture/emulation.md.
  - [x] Déterminer le nom, le contenu et les valeurs exactes des présélections avant de créer leurs constantes ou leurs ressources.

- [x] Inscrire l’architecture validée sans commencer son implémentation
  - [x] Modifier docs/architecture/emulation.md pour décrire la séparation validée entre configuration commune, catalogue de filtres, chaîne de traitement et implémentations propres aux backends.
  - [x] Modifier docs/architecture/emulation.md pour décrire l’enregistrement par configuration de machine, l’application immédiate à la seule instance correspondante et l’utilisation au prochain démarrage lorsqu’aucune instance n’est ouverte.
  - [x] Modifier docs/architecture/emulation.md pour décrire l’emplacement unique des contrôles dans l’onglet Vidéo, la séparation visuelle avec les options internes de l’émulateur et le maintien permanent des cinq réglages généraux.
  - [x] Modifier docs/tasks/interface/emulation-improvements.md pour ajouter la checklist d’implémentation progressive donnant les fichiers et actions retenus ; ne créer ni contrat, ni shader, ni contrôle pendant cette seule tâche documentaire.

### Checklist d’implémentation progressive — socle validé, à exécuter tâche par tâche

Chaque dernière case ci-dessous est une modification atomique. Elle doit laisser le projet compilable, être vérifiée, puis être cochée avant la suivante. Les shaders Libretro restent uniquement des références tant que la licence du fichier exact et de toutes ses dépendances n’est pas inscrite dans docs/reference/emulation-video-filters.md.

- [x] Auditer la checklist avant de commencer le code
  - [x] Vérifier les dossiers de responsabilités, les projets, les surfaces, les configurations, les tests et le script de traduction réellement présents dans le dépôt.
  - [x] Ajouter les tâches atomiques oubliées pour les types communs, les fonctions, les présélections, Snapshot, Argos et leurs tests sans déplacer de responsabilité vers un mauvais projet.
  - [x] Relire la checklist corrigée, vérifier que chaque dépendance précède son utilisation et qu’aucun groupe parent n’est coché avant ses enfants.

- [x] Créer le modèle commun de configuration avant toute interface ou tout shader
  - [x] Créer les enums communs un par un
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationVideoDisplayTechnology.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationVideoDisplayTechnology.cs pour déclarer uniquement Normal, Crt, FixedPixel, Plasma et Vector, sans texte visible ni valeur propre à un moteur.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationVideoSampling.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationVideoSampling.cs pour déclarer uniquement Nearest, Bilinear, SharpBilinear et Bicubic, sans ajouter les scalers avancés dans ce sélecteur.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationCrtColorMode.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationCrtColorMode.cs pour déclarer Color, Green, Amber, White, Gray et Custom sans texte visible.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationFixedPixelTechnology.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationFixedPixelTechnology.cs pour déclarer Lcd, LedBacklitLcd et Oled sans dupliquer les réglages communs.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationCrtMask.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationCrtMask.cs pour déclarer None, ApertureGrille, ShadowMask et SlotMask sans texte visible.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationPatternOrientation.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationPatternOrientation.cs pour déclarer Horizontal et Vertical et le réutiliser pour scanlines et trame volontaire.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationSubpixelLayout.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationSubpixelLayout.cs pour déclarer Monochrome, Rgb et Bgr sans dépendance graphique.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Enums/EmulationVideoPreset.cs.
    - [x] Modifier src/GWGUI.Emulation/Enums/EmulationVideoPreset.cs avec exactement Normal, CrtArcadeColor, CrtTelevisionColor, CrtGreen, CrtAmber, CrtWhite, LcdColor, LcdMonochrome, LedBacklitLcd, Oled, Plasma et Vector.
  - [x] Créer les constantes communes avant les contrats qui les consomment
    - [x] Créer le fichier vide src/GWGUI.Emulation/Constants/EmulationVideoProcessingLimits.cs.
    - [x] Modifier src/GWGUI.Emulation/Constants/EmulationVideoProcessingLimits.cs avec `-10..+10` pour les cinq réglages généraux, `0..100` pour les intensités et des bornes temporelles explicites en millisecondes, sans valeur de preset.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Constants/EmulationVideoProcessingDefaults.cs.
    - [x] Modifier src/GWGUI.Emulation/Constants/EmulationVideoProcessingDefaults.cs avec les seules valeurs neutres communes, sans enum, fonction, dictionnaire ni texte visible.
  - [x] Créer les contrats sérialisables un par un
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationImageAdjustments.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationImageAdjustments.cs pour transporter les cinq valeurs générales avec leurs valeurs neutres validées, sans conversion graphique.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationCrtVideoConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationCrtVideoConfiguration.cs pour transporter couleur/palette, faisceau, masque, géométrie, scanlines et trame volontaire, sans shader ni ressource WPF.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationFixedPixelVideoConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationFixedPixelVideoConfiguration.cs pour transporter technologie, grille, sous-pixels et réponse temporelle, avec valeurs propres facultatives.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationPlasmaVideoConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationPlasmaVideoConfiguration.cs pour transporter uniquement les paramètres Plasma validés à cette étape.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationVectorVideoConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationVectorVideoConfiguration.cs pour transporter uniquement les paramètres de ligne, halo et persistance validés à cette étape.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Contracts/EmulationVideoProcessingConfiguration.cs.
    - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationVideoProcessingConfiguration.cs pour agréger technologie, échantillonnage, réglages généraux et configurations propres, avec Normal et valeurs neutres par défaut.
  - [x] Créer les fonctions communes séparément des contrats et du catalogue
    - [x] Créer le fichier vide src/GWGUI.Emulation/Functions/EmulationImageAdjustmentFunctions.cs.
    - [x] Modifier src/GWGUI.Emulation/Functions/EmulationImageAdjustmentFunctions.cs pour borner les cinq réglages et convertir uniquement le gamma par `2^(-valeur / 10)`, sans traiter de pixels.
    - [x] Créer le fichier vide src/GWGUI.Emulation/Functions/EmulationVideoProcessingConfigurationFunctions.cs.
    - [x] Modifier src/GWGUI.Emulation/Functions/EmulationVideoProcessingConfigurationFunctions.cs pour normaliser et valider une configuration sérialisée, créer des sous-configurations neutres manquantes et ne dépendre d’aucun renderer.
  - [x] Exposer et enregistrer le contrat commun sans casser les anciennes configurations
    - [x] Modifier src/GWGUI.Emulation/Interfaces/IEmulationConfiguration.cs pour exposer EmulationVideoProcessingConfiguration en plus de VideoRenderer.
    - [x] Modifier src/GWGUI.Emulation.Amiga/Contracts/AmigaMachineConfiguration.cs pour porter le nouveau contrat facultatif et produire les valeurs neutres lorsqu’il est absent d’un ancien JSON.
    - [x] Modifier src/GWGUI.Emulation.Atari/Contracts/AtariMachineConfiguration.cs avec exactement la même règle de compatibilité ascendante.
    - [x] Modifier src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs et src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour préserver la configuration vidéo commune dans ApplySettings sans transformer les options natives des moteurs.
    - [x] Modifier src/GWGUI.Emulation.Atari/Contracts/AtariConfigurationDocument.cs pour transporter facultativement EmulationVideoProcessingConfiguration en dernier, afin que le JSON du schéma courant dépourvu de ce membre reste lisible.
    - [x] Modifier src/GWGUI.Emulation.Amiga/Services/AmigaConfigurationStore.cs, src/GWGUI.Emulation.Atari/Services/AtariConfigurationStore.cs et src/GWGUI.Emulation.Atari/Functions/AtariConfigurationStoreFunctions.cs uniquement si leur sérialisation explicite exige le nouveau membre ; ne pas ajouter de migration lorsque la désérialisation facultative suffit.
    - [x] Créer le fichier vide tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs.
    - [x] Modifier tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs pour vérifier valeurs neutres, sérialisation aller-retour Amiga/Atari et lecture d’anciens documents sans propriété vidéo commune.
    - [x] Exécuter uniquement tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs, puis compiler src/GWGUI.Emulation/GWGUI.Emulation.csproj, src/GWGUI.Emulation.Amiga/GWGUI.Emulation.Amiga.csproj et src/GWGUI.Emulation.Atari/GWGUI.Emulation.Atari.csproj avec --no-restore.

- [x] Construire l’interface commune et son enregistrement immédiat avant les effets
  - [x] Créer le catalogue et le panneau sans texte brut
    - [x] Créer le fichier vide src/GWGUI.Emulation/Dictionaries/EmulationVideoProcessingCatalog.cs.
    - [x] Modifier src/GWGUI.Emulation/Dictionaries/EmulationVideoProcessingCatalog.cs pour décrire technologies, sous-choix, paramètres, valeurs neutres, dépendances et compatibilités avec uniquement des identifiants et clés de ressources, puis associer les douze EmulationVideoPreset à des configurations complètes conformes au tableau validé.
    - [x] Créer le fichier vide src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsSection.cs.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsSection.cs pour créer les deux sélecteurs, le panneau conditionnel et les cinq réglages permanents, sans logique Amiga ou Atari.
    - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationVideoSettingsLayout.cs pour séparer visuellement les options natives du moteur et les traitements GW GUI, sans ajouter ces contrôles ailleurs.
    - [x] Modifier src/GWGUI.Emulation/Interfaces/IEmulationModule.cs pour appliquer une EmulationVideoProcessingConfiguration commune à une configuration immuable sans exposer son type spécialisé à App.
    - [x] Modifier src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs et src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour implémenter ApplyVideoProcessing en conservant toutes les autres valeurs de la configuration typée.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour utiliser le panneau commun dans l’onglet Vidéo de chaque module et transmettre ses changements au chemin d’enregistrement automatique existant.
  - [x] Ajouter et vérifier tous les textes avant la validation visuelle
    - [x] Modifier src/GWGUI.App/Constants/Localization/EmulationResourceKeys.cs pour ajouter une constante par libellé de technologie, paramètre, incompatibilité, limitation et présélection, sans texte traduit.
    - [x] Modifier scripts/translate-resx-argos.py pour écrire le catalogue neutre dans src/GWGUI.App/Resources/00-Base tout en conservant la commande documentée et les catalogues de culture existants.
    - [x] Ajouter dans src/GWGUI.App/Resources/00-Base/Emulation.resx toutes les clés neutres des technologies, paramètres, incompatibilités, présélections validées et limitations de backend.
    - [x] Pour chaque nouvelle clé, exécuter `python scripts/translate-resx-argos.py Emulation.resx <clé> "<texte anglais>"` afin d’ajouter la base, en-US et toutes les traductions ; ne pas remplir manuellement les catalogues à la place d’Argos.
    - [x] Relire les sorties Argos dans tous les catalogues src/GWGUI.App/Resources/*/Emulation.resx, corriger seulement les traductions manifestement erronées et vérifier qu’aucun texte brut n’est ajouté au code.
    - [x] Créer le fichier vide tests/GWGUI.Tests/EmulationVideoLocalizationTests.cs.
    - [x] Modifier tests/GWGUI.Tests/EmulationVideoLocalizationTests.cs pour vérifier la présence de chaque clé dans toutes les langues et le remplacement immédiat des textes du panneau lors d’un changement de langue.
  - [x] Appliquer les changements à la seule instance correspondante
    - [x] Modifier src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs pour recevoir une EmulationVideoProcessingConfiguration normalisée, sans encore modifier les pixels.
    - [x] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs et src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour conserver la configuration reçue sans changer le rendu actuel.
    - [x] Modifier src/GWGUI.App/Presenters/Emulation/Machine/MachineVideoPresenter.cs pour conserver la configuration courante et la réappliquer lors d’un changement ou d’un repli de surface.
    - [x] Modifier src/GWGUI.App/Contracts/Machine/MachineControllerOptions.cs et src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionMachineFunctions.cs pour transmettre la configuration vidéo initiale enregistrée avec VideoRenderer lors de la création du controller.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/MachineController.cs pour transmettre la nouvelle configuration à MachineVideoPresenter sans recréer IEmulatedMachine.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Machine/EmulationSectionConfigurationFunctions.cs pour transmettre la configuration vidéo commune avec VideoRenderer à l’unique MachineController ciblé par ModuleId et ConfigurationId.
    - [x] Créer le fichier vide tests/GWGUI.Tests/EmulationVideoSettingsSectionTests.cs.
    - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationVideoProcessingSettingsSection.cs pour confirmer un remplacement direct entre deux technologies simulées incompatibles, sans avertissement pour activer ou désactiver Normal, avec une fonction de confirmation injectable pour les tests.
    - [x] Créer puis modifier src/GWGUI.App/Functions/Views/Emulation/Machine/EmulationOpenMachineConfigurationFunctions.cs et raccorder EmulationSectionConfigurationFunctions.cs afin d’exposer une sélection générique testable par ModuleId et ConfigurationId sans construire de machine réelle.
    - [x] Créer puis modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationConfigurationPersistenceFunctions.cs et raccorder EmulationModuleSettingsSection.cs afin de rendre testable la décision brouillon ou autosauvegarde sans charger la fenêtre d’options.
    - [x] Modifier tests/GWGUI.Tests/EmulationVideoSettingsSectionTests.cs pour vérifier affichage conditionnel, cinq réglages permanents, brouillons, enregistrement automatique, confirmation des incompatibilités et ciblage d’une seule instance.
    - [x] Exécuter uniquement tests/GWGUI.Tests/EmulationVideoSettingsSectionTests.cs, puis compiler src/GWGUI.App/GWGUI.App.csproj avec --no-restore.

- [ ] Reprendre l'ergonomie des réglages vidéo après validation visuelle
  - [x] Supprimer la confirmation lors d'un changement de technologie d'affichage et vérifier que la nouvelle valeur s'applique immédiatement.
  - [x] Réunir Affichage et le bloc Rendu, Échantillonnage et Technologie d'affichage dans un même onglet, côte à côte et de manière équilibrée.
  - [x] Remplacer le placement adaptatif irrégulier par une grille stable de deux colonnes pour tous les blocs vidéo.
  - [x] Limiter globalement tous les sélecteurs de l'application à cinq éléments visibles, puis afficher un défilement vertical.
  - [x] Organiser Image, Restauration, Mouvement, Signal et Effets dans des onglets localisés, nommer l'onglet technologique d'après la technologie choisie et conserver l'onglet actif lors d'une reconstruction.
  - [x] Remplacer le libellé utilisateur « Plus proche voisin » par « Normal » dans toutes les langues avec Argos.
  - [x] Encadrer séparément les paramètres d'affichage de l'émulateur et le traitement vidéo GW GUI dans l'onglet Affichage.
  - [x] Regrouper les réglages permanents dans les seuls onglets Image et Effets, avec des cadres internes pour restauration, mouvement, signal et style.
  - [x] Appliquer un changement de traitement vidéo sans recalcul synchrone immédiat sur le thread de l'interface.
  - [x] Supprimer les allocations par pixel des échantillonneurs pixel-art et vérifier leur temps d'exécution sur une sortie de grande taille.
  - [x] Encoder et écrire les captures PNG hors du thread de l'interface sans perdre la frame capturée.
  - [x] Traiter les replis CPU et la génération de Snapshot hors du thread de l'interface, en abandonnant les frames intermédiaires lorsqu'une plus récente est disponible.
  - [ ] Refaire l’architecture et les rendus des filtres vidéo
    - [x] Renommer les classes et fichiers Cpu… par filtre fonctionnel en Filter…, notamment FilterBloom, FilterXbr, FilterXbrz, FilterHqx, FilterScaleFx, FilterScaleNx et FilterSabr.
    - [x] Extraire Normal, Bilinéaire, Bilinéaire net, Bicubique, xBR, xBRZ, HQx, ScaleFX, ScaleNx et SABR dans un fichier propre à chaque filtre, avec ses variantes CPU, OpenGL et Vulkan/Direct3D lorsque nécessaires.
    - [x] Réduire OpenGlVideoProcessingProgram et VeldridVideoProcessingShaders à la composition des modules et au seul répartiteur sélectionné par EmulationVideoSampling.
    - [ ] Corriger Bilinéaire net, Bicubique, xBR, xBRZ, HQx, ScaleFX, ScaleNx et SABR afin que chaque rendu GPU soit visuellement conforme et significativement distinct sur une image pixel-art réelle.
    - [x] Ajouter HQ2x, HQ3x, HQ4x, 2xSaI, Super 2xSaI, Super Eagle, EPX / Scale2x, JINC2 et Lanczos dans le même sélecteur, rangés dans un ordre logique.
    - [x] Traduire chaque nouveau libellé dans toutes les langues prises en charge.
    - [ ] Remplacer le test de simples hash par des mesures visuelles ciblées des contours, diagonales, aplats et niveaux de flou, puis valider OpenGL, Direct3D11 et Vulkan.
    - [ ] Exécuter les tests vidéo et de localisation, puis scripts/build.ps1 -Configuration Debug avant de cocher ce groupe.
    - [x] Présenter Luminosité, Contraste, Gamma, Saturation et Netteté sous forme de cinq curseurs verticaux compacts dans l’onglet Image.
    - [x] Isoler tout le bloc d’interface dans EmulationImageParametersSettingsBlock.cs, sans conserver sa construction dans la section vidéo monolithique.
    - [x] Séparer chaque paramètre dans son fichier Video…ParameterFunctions.cs, avec sa fonction logicielle et sa fonction shader partagée par OpenGL, Direct3D11 et Vulkan.
    - [x] Renommer les anciens effets Cpu… en Filter… et réserver Software… au pipeline et au worker de repli, afin que les noms décrivent la responsabilité plutôt que le processeur utilisé.
    - [x] Isoler l’interface de restauration dans EmulationImageRestorationSettingsBlock.cs et la placer à côté des paramètres d’image dans un cadre de même niveau.
    - [x] Présenter Débruitage, Réduction des bandes et Détails fins sous forme de trois curseurs verticaux compacts, puis renommer le libellé utilisateur « Récupération de détails » en « Détails fins » dans toutes les langues.
    - [x] Corriger le libellé utilisateur « Dédithering » en « Détramage » et remplacer son intensité continue par exactement quatre niveaux localisés : Aucun, Léger, Moyen et Fort, mappés sur 0, 33, 67 et 100, tous contenus dans la largeur disponible et accompagnés de quatre graduations visibles.
    - [x] Réduire le désentrelacement à un sélecteur compact de 220 pixels au lieu de lui attribuer toute la largeur du cadre.
    - [x] Renforcer les traitements GPU de restauration : reconnaissance de damier pour le détramage, débruitage bilatéral 3 × 3, réduction directionnelle des bandes et récupération bornée des détails fins, sans limitation artificielle à 25 ou 30 %.
    - [x] Ajouter les tests de disposition, des quatre niveaux de détramage, de conservation des changements combinés, de localisation et de compilation/rendu des shaders sur WPF, OpenGL, Direct3D11 et Vulkan.
    - [x] Isoler le bloc des effets temporels dans EmulationTemporalEffectsSettingsBlock.cs et chaque traitement dans FilterGeneralPersistence.cs, FilterMotionBlur.cs, FilterFlicker.cs, FilterInterlacing.cs et FilterBlackFrameInsertion.cs.
    - [x] Remplacer l’intensité d’entrelacement par une activation binaire et conserver un réglage séparé de visibilité des trames.
    - [x] Appliquer l’entrelacement temporel sur les lignes de deux frames source consécutives avant agrandissement, à 50 champs/s en PAL et 60 champs/s en NTSC lorsque le moteur produit ces cadences.
    - [x] Cadencer le scintillement et l’insertion d’images noires sur la parité des frames source et les appliquer directement dans le pipeline de chaque renderer.
    - [x] Vérifier la compilation des shaders, les rendus GPU, la localisation et les contrôles temporels par des tests Debug ciblés.
    - [x] Remplacer les cinq intensités cumulables de signal par une liaison exclusive (`RGB/Péritel`, composante, S-Video, composite ou RF), une norme exclusive (`Automatique`, PAL, NTSC ou SECAM) et une intensité commune par famille.
    - [x] Supprimer le bruit artificiel des normes PAL et NTSC, réserver le bruit animé à RF et réunir chaque fonction CPU et shader dans son fichier `SignalConnection…` ou `SignalStandard…` propre.
    - [ ] Vérifier qu'aucune barre de défilement n'est visible à la taille normale, qu'elle reste disponible si la fenêtre est réduite, contrôler le build Debug et terminer le groupe seulement après validation.

- [ ] Découpler entièrement la présentation vidéo des DLL d'émulation avant d'ajouter de nouveaux émulateurs
  - Constat : `VideoRenderer` et `VideoProcessing` sont actuellement transportés dans `IEmulationConfiguration`, puis recopiés et sauvegardés par Amiga et Atari. Les cœurs ne consomment pas ces réglages pour produire leurs frames ; ils servent uniquement ensuite à GW GUI pour choisir la surface et traiter l'image. Cette dépendance oblige donc inutilement chaque DLL d'émulation à connaître et préserver des données appartenant à l'interface hôte.
  - Cible : une DLL d'émulation ne doit exposer que la vidéo brute nécessaire à l'émulation (`VideoFrame`, dimensions, format de pixels, ratio, horodatage et informations natives réellement produites par le cœur). Le renderer, l'échantillonnage, la technologie d'affichage simulée, les corrections d'image et les effets doivent appartenir à GW GUI ou à une bibliothèque de présentation indépendante qui n'est référencée par aucune DLL d'émulation.
  - [ ] Inventorier chaque champ vidéo et classer explicitement les réglages en deux groupes : réglages qui modifient réellement la machine ou le signal brut émulé, à conserver dans le module concerné, et réglages de renderer/post-traitement, à déplacer dans la couche hôte.
  - [ ] Créer un contrat hôte unique, par exemple `EmulationVideoPresentationProfile`, regroupant au minimum le renderer et `EmulationVideoProcessingConfiguration`, dans `GWGUI.App` ou dans une bibliothèque de présentation dédiée située au-dessus des DLL d'émulation.
  - [ ] Créer un stockage hôte générique des profils vidéo, indexé par `(ModuleId, ConfigurationId)`, afin de conserver des réglages différents pour chaque instance sans ajouter de propriété dans les configurations Amiga, Atari ou celles des futurs émulateurs.
  - [ ] Définir le cycle de vie du profil hôte : création avec valeurs par défaut, chargement avant construction de la surface, application immédiate à `IEmulationVideoSurface`, copie lors de la duplication d'une configuration et suppression lors de la suppression définitive de celle-ci.
  - [ ] Modifier les presenters et contrôleurs de GW GUI pour lire et enregistrer directement ce profil, puis appeler la surface vidéo, sans passer par `IEmulationModule.ApplyVideoProcessing` ni reconstruire `IEmulationConfiguration`.
  - [ ] Retirer `VideoProcessing` de `IEmulationConfiguration` et `ApplyVideoProcessing` de `IEmulationModule`, puis supprimer leurs implémentations Amiga/Atari et tous les passages artificiels de cette valeur dans les fonctions d'entrée, de stockage, de firmware, de média et dans les services de machine.
  - [ ] Sortir également `VideoRenderer` des configurations propres aux émulateurs s'il reste confirmé qu'il ne modifie jamais le cœur ; le conserver dans le même profil hôte pour que le choix WPF/OpenGL/Direct3D11/Vulkan ne nécessite aucune modification des modules.
  - [ ] Préparer une migration ascendante des configurations existantes : détecter les anciens champs `videoRenderer` et `videoProcessing` dans les JSON Amiga et Atari, les importer une seule fois dans le stockage hôte, préserver les valeurs non neutres, puis tolérer les anciens champs lors des lectures ultérieures.
  - [ ] Prévoir une stratégie de reprise en cas de migration interrompue : écriture atomique du profil hôte avant suppression ou ignorance des anciennes données, migration idempotente et journalisation exploitable sans bloquer le démarrage.
  - [ ] Remplacer l'asymétrie actuelle des copies de configurations par des mises à jour propres à chaque domaine : Amiga peut garder ses `with`; Atari doit utiliser `with`, des propriétés `init` ou une fonction de copie centralisée pour ses seules données d'émulation, sans constructeur positionnel fragile à mettre à jour pour chaque nouveau champ.
  - [ ] Déplacer hors des modules les choix et libellés de renderer actuellement produits par `AmigaSettingsDescriptionFunctions` et `AtariSettingsDescriptionFunctions`, ainsi que leur présence dans les résumés de configuration ; l'interface hôte doit fournir ce bloc de manière identique à tous les émulateurs.
  - [ ] Retirer `VideoRenderer` des empreintes de compatibilité des sauvegardes d'état, notamment `AtariStateConfigurationFingerprint`, car un changement de backend de présentation ne modifie pas l'état de la machine émulée ; ajouter un test garantissant qu'un état reste chargeable après changement de renderer ou de filtre.
  - [ ] Ajouter un faux module d'émulation minimal ne référençant aucun type de présentation et vérifier qu'il bénéficie automatiquement du choix du renderer et de tous les traitements vidéo fournis par GW GUI.
  - [ ] Tester l'isolation par instance, la persistance après redémarrage, les changements d'entrée/stockage/firmware/média, la duplication, la suppression, la migration Amiga/Atari et l'absence de dépendance des projets `GWGUI.Emulation*` vers les types de présentation.
  - [ ] Mettre à jour `docs/architecture/emulation.md` avec la direction des dépendances et la règle obligatoire pour les futurs modules : produire une frame brute, ne jamais stocker ni appliquer les effets de présentation de GW GUI.
  - [ ] Exécuter les tests de migration, de configuration et de rendu, puis `scripts/build.ps1 -Configuration Debug` avant de retirer l'ancien chemin et de cocher ce groupe.
- [x] Préserver la configuration vidéo dans toutes les reconstructions Atari
  - [x] Modifier src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs, src/GWGUI.Emulation.Atari/Functions/AtariInputSettingsFunctions.cs, src/GWGUI.Emulation.Atari/Functions/AtariStorageSettingsFunctions.cs et src/GWGUI.Emulation.Atari/Services/AtariMachine.cs pour conserver VideoProcessing dans chaque reconstruction non vidéo.
  - [x] Modifier tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs pour vérifier qu’une reconstruction Atari non vidéo conserve une configuration vidéo non neutre.
  - [x] Exécuter uniquement tests/GWGUI.Tests/EmulationVideoConfigurationTests.cs, puis compiler src/GWGUI.Emulation.Atari/GWGUI.Emulation.Atari.csproj avec --no-restore.

- [x] Créer la chaîne de traitement sans effet avant d’implémenter Normal
  - [x] Créer le fichier vide src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoProcessingPipeline.cs.
  - [x] Modifier src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoProcessingPipeline.cs pour recevoir configuration, frame, tailles source/sortie et produire la sortie du backend sans dépendre d’une famille de machine.
  - [x] Créer le fichier vide src/GWGUI.App/Factories/Rendering/Emulation/EmulationVideoProcessingPipelineFactory.cs.
  - [x] Créer puis modifier src/GWGUI.App/Rendering/Emulation/Processing/PassthroughEmulationVideoProcessingPipeline.cs pour implémenter le contrat avec le renderer demandé, valider les tailles positives et retourner exactement la frame reçue.
  - [x] Modifier src/GWGUI.App/Factories/Rendering/Emulation/EmulationVideoProcessingPipelineFactory.cs pour choisir uniquement l’exécuteur correspondant au renderer déjà sélectionné.
  - [x] Modifier src/GWGUI.App/Interfaces/Rendering/Emulation/IEmulationVideoSurface.cs pour préciser que Snapshot expose la sortie traitée avant tout habillage externe, conformément à la décision validée, et raccorder le contrat de configuration déjà présent à la chaîne.
  - [x] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs, src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs et src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs pour raccorder une chaîne vide qui reproduit exactement le rendu actuel.
  - [x] Créer le fichier vide tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs.
  - [x] Modifier tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs pour vérifier qu’une chaîne Normal neutre préserve pixels, rapport d’aspect, redimensionnement et repli WPF.
  - [x] Créer puis modifier src/GWGUI.App/Functions/Rendering/Emulation/EmulationVideoSurfaceFrameFunctions.cs et raccorder les trois surfaces afin que l’affichage et Snapshot consomment ensemble la frame traitée et sa conversion BGRA commune.
  - [x] Modifier src/GWGUI.App/Rendering/Emulation/Surfaces/WpfVideoSurface.cs pour accepter facultativement un IEmulationVideoProcessingPipeline interne dans les tests tout en conservant la fabrique WPF neutre par défaut.
  - [x] Ajouter dans tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs un test vérifiant que Snapshot reçoit la sortie après traitements GW GUI et avant tout habillage, sur le chemin commun utilisé par WPF, OpenGL et Veldrid.
  - [x] Exécuter uniquement tests/GWGUI.Tests/EmulationVideoProcessingPipelineTests.cs, puis compiler src/GWGUI.App/GWGUI.App.csproj avec --no-restore.

- [x] Implémenter le premier socle Normal sur les quatre renderers
  - [x] Modifier docs/reference/emulation-video-filters.md pour fixer l’ordre et les conversions CPU exactes de luminosité, contraste, gamma, saturation et netteté, avec 0 strictement neutre.
  - [x] Créer l’exécution CPU de référence pour luminosité, contraste, gamma validé, saturation et netteté, avec conversions sRGB/linéaire testées.
  - [x] Ajouter le sélecteur nearest, bilinéaire, sharp-bilinear et bicubique dans l’exécution CPU/WPF avec comportement stable aux échelles non entières.
  - [x] Remplacer dans src/GWGUI.App/Rendering/Emulation/Surfaces/VeldridVideoSurface.cs le shader de copie fixe par la première chaîne portable commune Direct3D 11/Vulkan et ses buffers de paramètres.
  - [x] Remplacer dans src/GWGUI.App/Rendering/Emulation/Surfaces/OpenGlVideoSurface.cs `glDrawPixels` par texture, quad et programme GLSL capables d’exécuter les mêmes réglages et méthodes d’échantillonnage.
  - [x] Ajouter aux tests de pipeline des images déterministes couvrant chaque valeur neutre, les bornes validées et l’équivalence tolérée entre CPU, Direct3D 11, Vulkan et OpenGL.
  - [x] Exécuter les tests ciblés, compiler GWGUI.App sans restauration, puis vérifier Normal dans l’application avec les quatre renderers avant de cocher le groupe.

- [x] Implémenter CRT après validation de ses paramètres exacts et des licences
  - [x] Implémenter couleur et monochrome vert, ambre, blanc, gris et personnalisé dans la définition commune et l’exécution CPU de référence.
  - [x] Implémenter faisceau, masque, halo, courbure et vignettage dans des passes composables sans reprendre de code dont la licence n’est pas validée.
  - [x] Implémenter les scanlines horizontales et verticales uniquement dans le panneau CRT avec intensité, épaisseur, phase et compensation.
  - [x] Implémenter la trame volontaire horizontale et verticale uniquement dans CRT, séparément de la réduction du moiré accidentel.
  - [x] Porter les mêmes passes vers Veldrid et OpenGL, avec repli CPU/WPF et construction atomique en cas d’erreur.
  - [x] Ajouter des tests déterministes pour chaque sous-choix CRT, compatibilité, valeur neutre, redimensionnement et changement en direct.
  - [x] Vérifier visuellement chaque présélection CRT validée sur Amiga et Atari avec WPF, OpenGL, Direct3D 11 et Vulkan avant de cocher le groupe.

- [x] Implémenter les écrans à pixels fixes après CRT
  - [x] Implémenter le panneau partagé et le sous-choix LCD, LCD/LED ou OLED sans panneaux principaux dupliqués.
  - [x] Implémenter grille, sous-pixels, ordre des couleurs, netteté et paramètres communs dans l’exécution CPU de référence.
  - [x] Ajouter uniquement les paramètres conditionnels dont une différence LCD/LED/OLED a été documentée et validée.
  - [x] Implémenter rémanence et temps de réponse avec historique borné, sans les présenter comme désentrelacement.
  - [x] Porter les passes vers Veldrid et OpenGL, ajouter les tests déterministes et vérifier les quatre renderers avant de cocher le groupe.
  - [x] Reprendre le modèle après la validation visuelle signalant des contrôles sans effet.
    - [x] Extraire l’interface dans EmulationFixedPixelSettingsBlock.cs et regrouper type d’écran, structure des pixels, lumière/contraste et réponse temporelle dans quatre cartes qui restent dans la largeur disponible.
    - [x] Remplacer la saisie ARGB du monochrome par la palette commune vert, gris, ambre, bleu et blanc, en réutilisant les traductions existantes.
    - [x] Séparer grille, sous-pixels, lumière commune, LCD, LCD rétroéclairé LED, OLED, temps de réponse et persistance dans des fichiers Filter… propres à leur responsabilité.
    - [x] Calculer grille et sous-pixels dans les coordonnées de la frame fournie par l’émulateur, indépendamment de la définition physique de l’écran.
    - [x] Donner à LCD, LCD/LED et OLED des courbes différentes de rétroéclairage, plancher noir, halo local et contraste, sans afficher de rétroéclairage pour OLED.
    - [x] Raccorder temps de réponse et persistance au chemin Vulkan/Direct3D 11, où leurs paramètres étaient transmis mais non consommés.
    - [x] Regrouper les changements rapides des contrôles vidéo avant autosauvegarde et sérialiser la lecture/remplacement de machine.json entre instances.
    - [x] Ajouter les tests de distinction des technologies, palette monochrome, disposition conditionnelle, compilation SPIR-V, OpenGL, Direct3D 11 et concurrence de sauvegarde.

- [x] Implémenter Plasma après les écrans à pixels fixes
  - [x] Faire valider les paramètres exacts de cellules, diffusion, tramage temporel et rémanence, puis les inscrire dans docs/reference/emulation-video-filters.md.
  - [x] Compléter EmulationPlasmaVideoConfiguration, le catalogue, les ressources et le panneau conditionnel sans modifier les autres technologies.
  - [x] Implémenter la référence CPU, puis les passes Veldrid et OpenGL avec les mêmes valeurs.
  - [x] Ajouter les tests déterministes et vérifier Plasma avec les quatre renderers avant de cocher le groupe.
  - [x] Reprendre le modèle après la validation visuelle des quatre contrôles Plasma.
    - [x] Isoler le bloc d’interface dans EmulationPlasmaSettingsBlock.cs, supprimer l’agrégateur
      FilterPlasma.cs et placer chaque traitement dans son propre fichier : structure des cellules,
      profondeur des noirs, intensité des phosphores, réponse gamma, tramage temporel, diffusion
      lumineuse, persistance et limiteur automatique de luminosité fondé sur la luminance moyenne
      de l’image complète.
    - [x] Présenter les paramètres dans quatre cartes fonctionnelles sans cadre Plasma redondant :
      dalle et cellules, phosphores, gestion de la lumière et réponse temporelle.
    - [x] Supprimer les bandes RGB non résolubles en intégrant le motif selon le rapport entre la
      définition de la vidéo émulée et la taille de sortie.
    - [x] Remplacer le bruit du tramage temporel par une quantification Bayer animée et bornée.
    - [x] Rendre la diffusion lumineuse visible et dépendante des hautes lumières, à deux rayons
      exprimés dans les coordonnées de la vidéo émulée.
    - [x] Raccorder diffusion et persistance au shader Veldrid utilisé par Direct3D 11 et Vulkan,
      puis rendre la persistance décroissante afin que la valeur maximale ne fige pas l’image.

- [x] Implémenter l’écran vectoriel après Plasma
  - [x] Faire valider l’approximation raster, les paramètres de lignes, halo et persistance, puis les inscrire dans docs/reference/emulation-video-filters.md.
  - [x] Compléter EmulationVectorVideoConfiguration, le catalogue, les ressources et le panneau conditionnel sans prétendre recevoir des primitives vectorielles du moteur.
  - [x] Implémenter la détection/renforcement de lignes et la persistance dans la référence CPU, puis dans Veldrid et OpenGL.
  - [x] Ajouter les tests déterministes et vérifier l’écran vectoriel avec les quatre renderers avant de cocher le groupe.
  - [x] Séparer le traitement en FilterVectorLineDetection.cs,
    FilterVectorLineIntensity.cs, FilterVectorHalo.cs et FilterVectorPersistence.cs, puis présenter
    les réglages dans deux cartes sans cadre vectoriel redondant : détection et tracé, lueur et
    rémanence.
  - [x] Compléter le modèle vectoriel avec la largeur/focalisation du faisceau, le
    mode de phosphore (couleurs de la source, vert, ambre, blanc ou gris) et le rayon du halo. Ces
    paramètres doivent rester indépendants de la résolution physique de l’écran et s’appliquer à
    l’approximation raster issue de la vidéo émulée.

- [x] Ajouter les filtres indépendants avancés un groupe à la fois
  - [x] Faire valider xBR, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider xBRZ, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider HQx, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider ScaleFX, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider ScaleNx, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider SABR, inscrire sa source et sa licence dans docs/reference/emulation-video-filters.md, puis l’implémenter et le vérifier seul dans les quatre renderers.
  - [x] Faire valider le dé-dithering, vérifier qu’il ne duplique aucun traitement moteur, puis l’implémenter et le tester seul.
  - [x] Faire valider le débruitage, vérifier qu’il ne duplique aucun traitement moteur, puis l’implémenter et le tester seul.
  - [x] Faire valider la réduction des bandes, vérifier qu’elle ne duplique aucun traitement moteur, puis l’implémenter et la tester seule.
  - [x] Faire valider la netteté avancée sans dupliquer le réglage général, puis l’implémenter et la tester seule.
  - [x] Faire valider le désentrelacement GW GUI, vérifier qu’il ne duplique aucun traitement moteur, puis l’implémenter et le tester seul.
  - [x] Faire confirmer toute incompatibilité entre filtres indépendants avant d’ajouter sa boîte Oui/Non et vérifier que Non ne modifie aucune valeur.
  - [x] Ajouter les tests et validations visuelles propres à chaque filtre avant de cocher sa tâche individuelle.

- [x] Traiter le catalogue ultérieur sans regrouper plusieurs effets dans une seule modification
  - [x] Refaire VFD avec seuil d’émission, verre fumé, structure graphique/matrice, taille et
    espacement des cellules, halo avec rayon et persistance exprimée en millisecondes ; séparer
    chaque fonction dans son propre fichier, organiser l’interface en trois cartes et raccorder les
    paramètres aux quatre renderers dans les coordonnées de la vidéo émulée.
  - [x] Refaire la matrice LED dans les coordonnées de la vidéo émulée : cellules régulières rondes
    ou carrées, couleur réellement appliquée, luminosité pouvant atteindre l’extinction, espacement,
    halo (intensité et rayon) et profondeur des noirs indépendants ; séparer chaque fonction dans son
    propre fichier, organiser l’interface en deux cartes et compiler ce code uniquement lorsque la
    technologie Matrice LED est sélectionnée.
  - [x] Préparer puis implémenter la matrice de points lorsqu’une machine prise en charge le nécessite.
  - [x] Préparer puis implémenter les affichages à segments lorsqu’une machine prise en charge le nécessite.
  - [x] Préparer puis implémenter le papier électronique après validation de son utilité et de ses paramètres.
  - [x] Préparer puis implémenter la projection après validation de son utilité et de ses paramètres.
  - [x] Préparer puis implémenter la rémanence générale sans la confondre avec la réponse des écrans à pixels fixes.
  - [x] Préparer puis implémenter le flou de mouvement.
  - [x] Préparer puis implémenter le scintillement.
  - [x] Préparer puis implémenter l’entrelacement.
  - [x] Préparer puis implémenter l’insertion d’images noires.
  - [x] Préparer puis implémenter la simulation composite comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter la simulation S-Video comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter la simulation RF comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter la simulation PAL comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter la simulation NTSC comme effet explicitement nommé, uniquement si elle ne duplique pas une option du moteur ciblé.
  - [x] Préparer puis implémenter le grain.
  - [x] Préparer puis implémenter l’effet VHS.
  - [x] Préparer puis implémenter l’aberration chromatique.
  - [x] Préparer puis implémenter le bloom.
  - [x] Préparer puis implémenter le sépia.
  - [x] Préparer puis implémenter les niveaux de gris.
  - [x] Pour chaque tâche, inscrire d’abord sources, licence, paramètres, compatibilités et tests dans docs/reference/emulation-video-filters.md, puis vérifier les quatre renderers avant de la cocher.

- [x] Terminer la validation globale du point 7 après toutes les étapes retenues
  - [x] Exécuter tous les tests vidéo ciblés et corriger uniquement les régressions du point 7.
  - [x] Exécuter toute la suite tests/GWGUI.Tests/GWGUI.Tests.csproj et corriger uniquement les régressions du point 7.
  - [x] Exécuter scripts/build.ps1 -Configuration Debug et vérifier qu’aucun avertissement nouveau n’est introduit.
  - [x] Dans une seule exécution réelle, vérifier Amiga et Atari, l’enregistrement par machine, l’application à la seule instance ouverte, le prochain démarrage sans instance, les quatre renderers, le redimensionnement, le plein écran et le repli WPF.
  - [x] Dans la même exécution, vérifier que les options natives des moteurs restent séparées, que les cinq réglages généraux restent visibles et que les panneaux conditionnels n’apparaissent que pour la technologie choisie.
  - [x] Fermer l’instance utilisée pour la validation et vérifier les journaux d’erreurs avant de cocher le groupe.

## Checklist détaillée — Point 8 : habillages d’écran en plein écran

La section Idée future : habillages d’écran indique explicitement que cette fonction ne doit pas être réalisée maintenant. Aucune tâche de code, d’image, de configuration, de test ou de traduction n’est donc autorisée dans l’état actuel du document.

- [ ] Autoriser explicitement le démarrage de cette idée future avant toute autre action
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md, dans la section Idée future : habillages d’écran, uniquement après une décision explicite de réalisation, pour inscrire que le point 8 peut commencer et conserver la date de cette décision.

- [ ] Compléter les décisions encore ouvertes avant d’écrire une checklist d’implémentation
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md après l’autorisation pour inscrire les décisions validées concernant le mode fenêtré, les variantes initiales, les images à produire ou rechercher, leur redistribution, le recadrage autorisé et le comportement lorsqu’un habillage manque.
  - [ ] Modifier docs/tasks/interface/emulation-improvements.md après ces décisions pour remplacer le présent bloc par une checklist d’implémentation fondée sur les fichiers alors réellement présents, sans anticiper maintenant une architecture, des actifs ou des comportements non validés.
