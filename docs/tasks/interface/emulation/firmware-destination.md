# Destination des ROM

[Sommaire](../emulation-improvements.md) · [Règles communes](rules.md)

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

## Checklist détaillée — Point 4 : destination des ROM détectées

Cette checklist réalise la demande fonctionnelle décrite dans la section 5. Elle conserve la liste et le bouton Utiliser existants. La destination affichée provient du même identifiant de champ que celui consommé par Utiliser ; l’application ne maintient aucune seconde correspondance.

- [x] Inscrire les deux décisions d’affichage encore manquantes avant de modifier le code
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md, dans la section 5, pour remplacer le nombre maximal de caractères restant à fixer par la valeur validée et préciser si l’ellipse est comprise dans cette limite.
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md, dans la section 5, pour inscrire la position validée de Destination par rapport au nom de la ROM et à Compatibilité.

- [x] Faire porter la destination par le résultat commun du scan
  - [x] Modifier src/GWGUI.Emulation/Contracts/EmulationFirmwareCandidate.cs pour ajouter l’identifiant optionnel du champ de destination à la ROM détectée.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour renseigner cet identifiant depuis le type déjà obtenu par AmigaFirmwareCatalog, avec KickstartPath, ExtendedRomPath ou RomKeyPath, et le laisser vide lorsqu’aucun de ces champs ne correspond.
  - [x] Modifier UseFirmware dans src/GWGUI.Emulation.Amiga/Modules/AmigaEmulationModule.cs pour utiliser l’identifiant porté par EmulationFirmwareCandidate afin de choisir le champ à remplacer et supprimer la seconde inspection actuellement réalisée uniquement pour retrouver cette destination.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour renseigner SystemFirmware lorsque la ROM détectée possède une destination pour la machine affichée et laisser l’identifiant vide sinon.
  - [x] Modifier UseFirmware dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour vérifier et consommer ce même identifiant avant d’appliquer la sélection Atari existante, sans ajouter une autre table de routage.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs provoquées par l’ajout de la destination au contrat commun.

- [x] Transmettre le module nécessaire à la résolution du libellé
  - [x] Modifier le constructeur de src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour recevoir le IEmulationModule déjà détenu par EmulationModuleSettingsSection.
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md pour placer le raccordement de EmulationModuleSettingsSection avant l’utilisation du nouveau constructeur et réunir dans une seule tâche la résolution et la transmission du libellé après l’extension de FirmwareRow.
  - [x] Modifier src/GWGUI.App/Views/Controls/Emulation/Options/EmulationModuleSettingsSection.cs pour transmettre _module à EmulationFirmwareManagementController sans changer les raccordements de ConfigurationChanged.
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md pour renommer ce groupe selon les actions qu’il contient maintenant, avant de le cocher.

- [x] Ajouter la cellule informative à la ligne existante
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour ajouter la limite de caractères validée et uniquement les dimensions nécessaires à la colonne validée.
  - [x] Modifier FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour recevoir le libellé de destination, le limiter avec une ellipse selon la décision inscrite et l’afficher comme texte simple à la position validée, dans la présentation de la compatibilité existante.
  - [x] Modifier RefreshAsync dans src/GWGUI.App/Controllers/Emulation/Firmware/EmulationFirmwareManagementController.cs pour rechercher DestinationFieldId dans les champs retournés par IEmulationModule.Describe pour la machine et la configuration affichées, localiser directement LabelResourceKey, transmettre ce texte à FirmwareRow et transmettre un texte vide si l’identifiant est absent ou introuvable, sans modifier le nom, la version, la compatibilité, le chemin ou l’ordre des ROM.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par la nouvelle cellule.

- [x] Corriger les écarts constatés dans l’affichage réel avant de reprendre la vérification
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md pour inscrire, avant toute correction, la correction du libellé Kickstart, de la présentation et de la largeur de Destination, la compilation puis la reprise de la vérification dans une nouvelle exécution.
  - [x] Modifier src/GWGUI.App/Resources/00-Base/Emulation.resx et chaque src/GWGUI.App/Resources/<langue>/Emulation.resx pour ajouter une clé de ressource Kickstart dont la valeur visible reste Kickstart dans toutes les langues prises en charge.
  - [x] Modifier src/GWGUI.Emulation.Amiga/Constants/AmigaSettingsDescriptionFunctionsConstants.cs et src/GWGUI.Emulation.Amiga/Functions/AmigaSettingsDescriptionFunctions.cs pour remplacer le texte brut Kickstart utilisé comme LabelResourceKey par la nouvelle clé de ressource, afin que le champ existant et Destination affichent tous deux Kickstart sans crochets.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour remplacer la largeur de Destination copiée depuis Compatibilité par uniquement l’identifiant de groupe nécessaire à une largeur partagée calculée depuis le contenu.
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md pour inscrire la copie préalable de la construction du badge Compatibilité dans une fonction commune et sa compilation avant le remplacement de l’ancien bloc.
  - [x] Modifier src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour copier la construction actuelle du Border de Compatibilité dans une fonction FirmwareBadge recevant le texte et les couleurs, sans retirer ni remplacer le bloc existant.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore pour vérifier uniquement l’ajout de FirmwareBadge avant son utilisation.
  - [x] Modifier FirmwareSettingsPage et FirmwareRow dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour partager automatiquement la largeur de Destination entre les lignes, rendre au nom de ROM l’espace restant, remplacer le bloc Compatibilité par FirmwareBadge puis afficher Destination avec la même fonction et les couleurs de compatibilité de la ligne.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ces corrections d’affichage.

- [x] Corriger la largeur des deux badges après le constat visuel
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md pour inscrire la fermeture de l’instance affichée, la largeur identique de Compatibilité et Destination, leur alignement à droite avec un petit espacement, l’espace restant réservé au nom, la compilation et la nouvelle vérification visuelle.
  - [x] Fermer l’instance de GW GUI utilisée pour constater cette disposition.
  - [x] Modifier src/GWGUI.App/Constants/Emulation/EmulationFirmwareSettingsConstants.cs pour remplacer les largeurs distinctes de Compatibilité et Destination par un seul groupe de largeur partagée entre les deux badges et ajouter uniquement l’espacement validé entre eux.
  - [x] Modifier FirmwareRow et FirmwareBadge dans src/GWGUI.App/Functions/Views/Emulation/Settings/EmulationFirmwareSettingsLayout.cs pour laisser la colonne du nom prendre tout l’espace restant, placer à droite deux colonnes automatiques dans le même groupe de largeur, étirer et centrer chaque badge dans sa colonne et conserver le petit espacement entre eux.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette disposition.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build et ouvrir Options > Émulation > Amiga > ROM.
  - [x] Capturer uniquement la fenêtre Options et vérifier que le nom de ROM utilise l’espace restant tandis que Compatibilité et Destination ont exactement la même largeur, restent à droite et sont séparées par un petit espace.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification visuelle.
- [x] Restaurer le libellé Atari et supprimer le redimensionnement de la fenêtre Options
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md pour inscrire la fermeture de l’instance affichée, la restauration immédiate du libellé Atari, la désactivation du redimensionnement, la compilation et la vérification visuelle.
  - [x] Fermer l’instance de GW GUI affichée pendant ce constat.
  - [x] Restaurer dans src/GWGUI.App/Resources/fr-FR/Emulation.resx la valeur exacte ROM système pour Emulation.Firmware.Rom.System.
  - [x] Modifier src/GWGUI.App/Views/Windows/Options/OptionsWindow.xaml pour remplacer ResizeMode=CanResizeWithGrip par ResizeMode=NoResize sans modifier Width, Height, MinWidth ni MinHeight.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par ces deux corrections.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build, ouvrir Options > Émulation > Atari > ROM et vérifier que le titre et le champ affichent de nouveau ROM système et que la fenêtre ne possède plus de poignée ni de commande de redimensionnement.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
- [x] Restaurer l’identification TOS dans le nom des ROM Atari reconnues
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md pour inscrire le préfixe TOS devant la version d’une ROM TOS reconnue, la conservation du nom complet pour une ROM non reconnue, la compilation et la vérification visuelle dans l’application.
  - [x] Modifier ScanFirmwareAsync dans src/GWGUI.Emulation.Atari/Modules/AtariEmulationModule.cs pour afficher TOS suivi de la version lorsqu’une ROM TOS est reconnue et conserver Path.GetFileName(scanned.Path) lorsqu’elle n’est pas reconnue.
  - [x] Compiler src/GWGUI.App/GWGUI.App.csproj avec dotnet build --no-restore et corriger uniquement les erreurs introduites par cette modification.
  - [x] Exécuter GW GUI avec dotnet run --project src/GWGUI.App/GWGUI.App.csproj --no-build, ouvrir Options > Émulation > Atari > ROM et vérifier que les quatre ROM reconnues affichent TOS devant leur version au lieu du seul numéro.
  - [x] Fermer l’instance de GW GUI utilisée pour cette vérification.
- [x] Vérifier le fonctionnement demandé avant de terminer le point
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md avant la nouvelle exécution pour séparer chaque cas vérifié, inscrire les fichiers et le libellé temporaires nécessaires aux données absentes, puis inscrire leur suppression ou restauration et la compilation finale.
  - [x] Modifier docs/tasks/interface/emulation/firmware-destination.md pour remplacer les fausses données de vérification prévues par les vraies données retrouvées : C:/Users/overt/Downloads/Recalbox_10.0.8_BIOS_Pack/rom.key et les quatre ROM TOS déjà présentes dans %APPDATA%/GW GUI/Emulation/Machines/Atari/Firmware/ST.
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
