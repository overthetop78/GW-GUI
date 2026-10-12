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
  - [ ] Corriger les dépendances natives encore constatées.
    - [ ] Modifier `src/GWGUI.Emulation.Apple/Common/Machines/Common/Contracts/ConfigurationContracts.cs`, `Functions/ConfigurationFunctions.cs`, `src/GWGUI.Emulation.Apple/Common/Services/Engine.cs`, `Machine.Commands.cs` et `EmulationGlobalUsings.cs` : déplacer initialisation/validation et règles propres aux cœurs vers les adaptateurs, enregistrer les capacités neutres utiles avant réalisation ; détailler les fichiers d’adaptateurs concernés après lecture, sans refaire les inventaires déjà acquis.

Contrôles acquis sur les DLL livrées : 10 descriptions de machines, défauts et légendes des choix Apple IIe, 1402 chaînes dans les cultures, fréquences/résolutions sans ressources numériques ; audit Argos 28 cultures et 4 catalogues, build complet 23 modules réussi. Contrôle temporaire supprimé. Les autres dépendances natives constatées restent dans le groupe suivant.
