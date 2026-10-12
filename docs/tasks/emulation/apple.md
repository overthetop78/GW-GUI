# Corriger les paramètres et ressources Apple

- [x] Raccorder les descriptions aux adaptateurs sans dépendance native dans Common.
  - [x] Déplacer la propriété des paramètres exposés.
    - [x] Créer `src/GWGUI.Emulation.Apple/Common/Interfaces/IEmulatorSettings.cs` : capacité neutre DescribeSettings retournant les blocs SDK pour une configuration.
    - [x] Déplacer `src/GWGUI.Emulation.Apple/Common/Machines/Common/Functions/SettingsFunctions.AppleII.cs` vers `Emulators/AppleWin/Functions/SettingsFunctions.cs` et `SettingsFunctions.Macintosh.cs` vers `Emulators/MiniVmac/Functions/SettingsFunctions.cs` : namespaces nommés, constructeurs de blocs propres aux adaptateurs, appels aux helpers Common conservés et suffixes de redémarrage retirés.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/AppleWin/Factories/AppleWinMachineFactory.cs`, `Emulators/MiniVmac/Factories/MiniVmacMachineFactory.cs` et `Common/Machines/Common/Functions/SettingsFunctions.cs` : les fabriques implémentent IEmulatorSettings ; Common délègue selon l’adaptateur sélectionné et garde le matériel et les helpers neutres.
  - [x] Présenter les valeurs invariantes sans catalogue numérique.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Contracts/CoreContracts.cs`, `Common/Machines/Common/Functions/SettingsFunctions.cs`, `Emulators/AppleWin/Dictionaries/OptionCatalog.cs` et `Common/Machines/Common/Constants/SettingsChoiceConstants.cs` : valeur d’affichage invariante explicite, formatage SDK des fréquences, dimensions numériques lisibles ; conserver les identifiants natifs, retirer les constantes de ressources par résolution/fréquence.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Resources/*/Emulation.resx` : supprimer les quatre ressources numériques remplacées par leurs valeurs et conserver toutes les traductions utiles ; invariants restants seulement dans 00-Base, une entrée complète par ligne.
  - [x] Vérifier et nettoyer le raccordement.
    - [x] Créer `artifacts/temp/apple-settings-validation/Validation.csproj` et `Program.cs` : vérifier les descriptions et défauts pour chaque machine/adaptateur, le chargement des textes dans toutes les cultures et les invariants numériques, sans fenêtre ou fichier externe ; supprimer ce contrôle temporaire après utilisation.
    - [x] Produire `build/Debug/GW GUI/gwgui.exe` et les modules avec `scripts/local-building.cmd --building=debug --modules=A` : contrôler le résultat terminal et les DLL livrées, après préparation complète des changements.
    - [x] Supprimer `artifacts/temp/apple-settings-validation` après sortie du contrôle et libération des ressources détenues.
    - [x] Commiter les corrections vérifiées, sans push.
- [ ] Achever la neutralité des autres commandes Apple.
  - [x] Confier l’initialisation et la validation native aux adaptateurs.
    - [x] Créer `src/GWGUI.Emulation.Apple/Common/Interfaces/IEmulatorConfiguration.cs` et `Emulators/AppleWin/Functions/ConfigurationFunctions.cs`, `Emulators/MiniVmac/Functions/ConfigurationFunctions.cs` : capacité neutre de préparation des options et validation ; conserver le modèle AppleWin et les limites de supports Mini vMac dans leurs adaptateurs.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/AppleWin/Factories/AppleWinMachineFactory.cs` et `Emulators/MiniVmac/Factories/MiniVmacMachineFactory.cs` : implémenter cette capacité avec leurs fonctions nommées.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Contracts/ConfigurationContracts.cs` et `Functions/ConfigurationFunctions.cs` : choisir le défaut matériel existant, déléguer préparation et validation natives ; conserver les contrôles de schéma, matériel, ports et associations sans imports d’émulateur concret.
  - [x] Découvrir les adaptateurs indépendamment de leur backend.
    - [x] Créer `src/GWGUI.Emulation.Apple/Common/Dictionaries/EmulatorCatalog.cs` et modifier `Common/Services/Engine.cs` : découverte par IEmulatorAdapter, sélection par identité neutre, sans imposer MachineFactory ou le catalogue natif.
  - [x] Vérifier cette séparation avant les changements suivants.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/MiniVmac/Functions/SettingsFunctions.cs` : qualifier l’appel au contrôle matériel commun SupportsRuntime pour éviter le masquage par les nouvelles fonctions de configuration natives.
    - [x] Modifier `docs/tasks/emulation/methode-modules.md` : préciser les propriétaires de préparation et validation, ainsi que la vérification des appels masqués lors d’un déplacement de fonctions homonymes.
    - [x] Produire `build/Debug/GW GUI/gwgui.exe` et les DLL des modules avec `scripts/local-building.cmd --building=debug --modules=A` ; vérifier la réussite terminale et les fichiers livrés.
    - [x] Créer `artifacts/temp/apple-configuration-validation/Validation.csproj` et `Program.cs` : comparer défauts et rejets de configurations des deux adaptateurs à leur comportement précédent, sans cœur externe.
    - [x] Supprimer `artifacts/temp/apple-configuration-validation` après la sortie effective du contrôle.
    - [x] Commiter ces corrections vérifiées, sans push.
  - [x] Confier les règles de session aux adaptateurs.
    - [x] Créer `src/GWGUI.Emulation.Apple/Common/Interfaces/IEmulatorRuntimePolicy.cs`, `Emulators/AppleWin/Functions/RuntimeFunctions.cs` et `Emulators/MiniVmac/Functions/RuntimeFunctions.cs` : décisions neutres pour le changement des supports et les options ; conserver exactement les règles actuelles de chaque cœur.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Emulators/AppleWin/Factories/AppleWinMachineFactory.cs` et `Emulators/MiniVmac/Factories/MiniVmacMachineFactory.cs` : exposer les décisions de leurs fonctions natives par cette capacité.
    - [x] Modifier `src/GWGUI.Emulation.Apple/Common/Services/Machine.cs`, `Machine.Commands.cs` et `Common/Machines/Common/Functions/RuntimeFunctions.cs` : sélectionner une fois la capacité de session, déléguer les décisions et supprimer la liste native d’options du Common.
    - [x] Modifier `src/GWGUI.Emulation.Apple/GWGUI.Emulation.Apple.csproj` : réunir les attributs XML de chaque balise sur une seule ligne sans modifier les éléments ni leurs valeurs.
    - [x] Modifier `docs/tasks/emulation/methode-modules.md` : préciser que la machine commune consomme les capacités de session sélectionnées et ne compare pas les noms de cœurs ou leurs options natives.
    - [x] Produire `build/Debug/GW GUI/gwgui.exe` et les modules par le build Debug complet ; vérifier le résultat terminal et les DLL déclarées.
    - [x] Créer `artifacts/temp/apple-runtime-policy-validation/Validation.csproj` et `Program.cs` : contrôler les règles de supports et d’options des deux adaptateurs sur la DLL livrée, sans DLL native externe.
    - [x] Supprimer `artifacts/temp/apple-runtime-policy-validation` après la sortie effective du contrôle et commiter cette correction, sans push.
  - [ ] Achever les autres dépendances natives constatées.
    - [ ] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Functions/FirmwareFunctions.cs`, `FirmwareFunctions.Scan.cs`, `StateFunctions.Saved.cs`, `RuntimeFunctions.cs`, `EmulationGlobalUsings.cs` et les adaptateurs correspondants : déléguer firmware et empreintes de supports, isoler la conversion de région native, rapatrier les contrats de transport neutres puis retirer les imports globaux de backend ; préciser les déplacements et appels avant réalisation.

Contrôles acquis sur les DLL livrées : 10 descriptions de machines, défauts et légendes des choix Apple IIe, 1402 chaînes dans les cultures, fréquences/résolutions sans ressources numériques ; audit Argos 28 cultures et 4 catalogues, build complet 23 modules réussi. Contrôle temporaire supprimé. Les autres dépendances natives constatées restent dans le groupe suivant.

Validation de cette correction : 68 contrôles sur les 10 modèles, exécutés contre la DLL livrée ; build complet réussi et 23 manifests/DLL contrôlés. Aucun cœur natif exécuté pour ce contrôle ; fichiers temporaires supprimés.

Règles de session : 80 contrôles des adaptateurs et de leurs consommateurs Common, sur la DLL livrée ; aucune DLL native lancée, cœur factice libéré une seule fois, aucun résidu. Build complet réussi, 23 manifests/DLL contrôlés et contrôle temporaire supprimé.
