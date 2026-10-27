# Configuration des lecteurs Acorn — corrections demandées

- [x] Cadrer les corrections.
  - [x] Créer `docs/tasks/acorn-storage-settings.md` : consigner chaque défaut signalé, les fichiers concernés et les vérifications prévues.
- [x] Corriger les choix et les données du module.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Functions/StorageFunctions.cs`, `StorageFunctions.HardDisks.cs`, les constantes associées et les services `EmulatorCore.*Sound.cs` : supprimer les sélecteurs de sons, appliquer les profils définis par le matériel et les replis existants, ne conserver que l'activation et le volume ; présenter la vitesse en boutons radio.
  - [x] Modifier les définitions Winchester dans MediaEngine et leur relais dans `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Dictionaries/HardDiskFormats.cs` : déclarer les géométries et capacités réellement prises en charge par le profil et ADFS, sélectionner un défaut valide et respecter les limites à la création et au montage ; documenter les limites matérielles distinctes des unités logiques émulées.
  - [x] Modifier les fonctions de création Winchester dans MediaEngine et leur relais SDK : permettre réellement la préallocation du DAT sans perdre la publication atomique DAT/DSC.
- [x] Corriger les fenêtres App en réutilisant les contrats existants.
  - [x] Modifier `src/GWGUI.App/Controllers/Emulation/Storage/EmulationStorageSettingsController.cs` et les appels de dialogue concernés : transmettre le nom affiché de la machine au lieu de son identifiant technique.
  - [x] Modifier `src/GWGUI.Emulation/Contracts/FloppyDriveDialogOptions.cs` et créer `Constants/EmulationDriveSpeedChoices.cs` : transmettre les vitesses déclarées par le module au dialogue partagé, avec les choix génériques existants centralisés dans le SDK.
  - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/FloppyDriveConfigurationDialog.cs` : corriger le chemin initial vide de la création, conserver le choix utilisateur de destination et les actions créer/créer et monter, regrouper les réglages, employer des boutons radio pour la vitesse et un volume vertical compact.
  - [x] Modifier `src/GWGUI.App/Views/Dialogs/Emulation/Storage/HardDiskDriveConfigurationDialog.cs`, `Functions/Views/Emulation/Storage/StorageConfigurationFieldFunctions.cs` et les constantes dédiées : intégrer les informations lecteur/interface dans l'en-tête, compacter les groupes, employer des curseurs verticaux bornés et des choix de vitesse adaptés, masquer les tailles personnalisées interdites et corriger le texte de préparation indépendant d'Atari.
- [x] Conserver les capacités compatibles au-delà des modèles commerciaux.
  - [x] Modifier les catalogues HDD MediaEngine, les contrats de profils et le relais Acorn : conserver un profil Acorn SCSI personnalisable, limité par l'adressage utilisé et la préparation ADFS, en plus des présélections Winchester 110/130 ; ne pas confondre matériel officiel et capacités compatibles de l'émulation.
- [x] Traduire et documenter.
  - [x] Modifier les ressources App et Acorn dans toutes les langues avec Argos : libellés courts et précis pour les bruits du lecteur, le volume et la préparation d'image ; noms et formats invariants dans la base commune.
  - [x] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/model-a-reference.md` et `docs/project/mechanical-audio-sources.md` : décrire les contraintes vérifiées, la sélection automatique des sons et les enregistrements réels disponibles ou manquants, sans attribuer un son à une référence non attestée.
- [x] Finaliser les présélections et leur description.
  - [x] Modifier `src/GWGUI.App/Constants/Emulation/EmulationControlDefaults.cs` et `Views/Dialogs/Emulation/Storage/HardDiskDriveConfigurationDialog.cs` : centraliser les présélections génériques et inclure la capacité par défaut exacte du profil personnalisé.
  - [x] Modifier `src/GWGUI.Emulation/Constants/EmulationMechanicalSoundConstants.cs` : corriger la description du profil BBC sans lui attribuer les enregistrements Panasonic.
- [x] Vérifier la réalisation.
  - [x] Modifier les tests autonomes existants concernés dans `tests/GWGUI.LocalEmulationTests/Emulation/` : vérifier les champs supprimés, les profils appliqués et les tailles autorisées ; ajouter uniquement les contrôles nécessaires.
  - [x] Modifier `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Functions/StorageFunctions.cs` et `StorageFunctions.HardDisks.cs` : ajouter l'import explicite des constantes de présentation signalé par la compilation isolée.
  - [x] Modifier `src/GWGUI.App/Functions/Views/Emulation/Storage/StorageConfigurationFieldFunctions.cs`, `Views/Dialogs/Emulation/Storage/HardDiskDriveConfigurationDialog.cs`, les constantes de dimensions et `Resources/*/Emulation/EmulationHdd.resx` : corriger après inspection visuelle le texte de capacité encore figé à 512 octets, les groupes sonores trop hauts et l'accès au volume HDD ; ne pas créer un dossier vide lors du parcours d'une image existante.
  - [x] Créer un vérificateur temporaire sous `artifacts/acorn-storage-settings-check/` : vérifier la création effective disquette et HDD, la préallocation, les restrictions et le rendu des fenêtres ; fermer les fenêtres et libérer tous les objets possédés dans finally, puis supprimer le vérificateur et ses artefacts.
  - [x] Créer le build Debug Acorn avec `scripts/local-building.cmd --building=debug --modules=acorn` une fois les corrections vérifiées et l'application fermée ; ne pas fermer un essai utilisateur en cours.
  - [x] Modifier ce document : consigner les résultats effectifs et toute limite restant à traiter.

## Points signalés

Le choix de profil sonore est inutile pour les périphériques du Model A : les sons doivent découler du matériel, avec les replis convenus lorsqu'un enregistrement manque. Le volume doit être vertical et compact. Les libellés contenant « mécanique » sont à remplacer par des intitulés destinés à l'utilisateur.

Les modèles de disquette doivent rester issus de MediaEngine et des capacités physiques autorisées. L'erreur « path vide » survient avant l'ouverture du dialogue de création lorsque le dossier de disquettes n'est pas configuré : aucun nouveau dossier arbitraire ne doit être imposé.

L'identifiant `BbcMicroModelA` n'est pas un nom à afficher. Les informations HDD/interface ne justifient pas deux cartes. Les réglages de comportement, de volume, de vitesse et de latence doivent être compacts et regroupés.

La préallocation Winchester est actuellement désactivée par le profil. Le texte mentionnant Atari est une ressource commune utilisée à tort. Les capacités génériques de 20/40/80 Mio ne sont pas une description du Winchester Acorn : les limites du format, de la préparation et du matériel doivent déterminer les choix.

Le manuel Acorn ADFS décrit Winchester 0, réserve 1–3 aux extensions futures et affecte 4–5 aux disquettes. Quatre unités logiques fonctionnelles dans AcornEmu ne constituent pas la preuve de quatre disques standard sur un Model A.

Sources : [manuel Acorn ADFS](https://8bs.com/othrdnld/manuals/hnooijen/Acorn/Advanced-Disc-Filing-System-User-Guide.pdf), [manuel de maintenance Winchester](https://www.theoddys.com/acorn/acorn_system_filing_systems/winchester_disks/Acorn%20Winchester%20Disc%20Service%20Manual.pdf).

## Validation isolée du 27 octobre 2026

Les 208 tests Acorn passent. Deux vérifications temporaires supplémentaires ont créé/réouvert les formats vierges annoncés par les modèles floppy, refusé les géométries incompatibles, créé les profils Winchester 110/130 avec et sans préallocation, créé un profil personnalisé, relu les DAT/DSC et contrôlé le refus d'écrasement. Total : 210 réussites, zéro échec. Après le dernier ajustement des curseurs, les deux vérifications ciblées passent à nouveau.

Les fenêtres françaises ont été rendues et inspectées : disquette 760 de large, HDD 900 × 720, profils sonores supprimés, volume vertical 72, latences à crans, vitesses radio, nom BBC Micro Model A. Le texte des limites donne les capacités du profil et les secteurs de 256 octets, sans texte Atari. Le profil personnalisé sélectionne bien sa capacité par défaut exacte. Le chemin vide n'est plus transmis à Directory.CreateDirectory avant le choix de fichier floppy, ni lors du parcours HDD existant. Aucune application utilisateur n'a été fermée.

Les 30 catalogues App contiennent les nouveaux libellés avec leurs paramètres conservés. Les fichiers de contrôle, les images de test, leurs captures et la sortie compilée isolée ont été supprimés après vérification. Après autorisation de l’utilisateur, le script Debug Acorn s’est terminé avec le code 0. L’exécutable build/Debug/GW GUI/gwgui.exe et les DLL des modules acorn/example sont présents. Les sons exacts du Winchester réel restent à fournir ; le repli documenté ne devient pas un enregistrement Rodime par changement d'étiquette.
