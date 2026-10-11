# Travail restant : thomson

- [ ] Réaliser, vérifier et commiter le module Thomson avec les frontières et la carte de fichiers communes.
  - [ ] Qualifier les machines et les adaptateurs avant fabrication.
    - [ ] Créer `docs/tasks/emulation/thomson-native-contract.json` : relever les métadonnées officielles Théodore, sa recette de dépôt/révision, les modèles, BIOS, supports, options et entrées ; distinguer les informations du cœur distribué de celles du dépôt courant.
    - [ ] Créer `docs/tasks/emulation/thomson-file-map.json` : définir les fichiers Common, les familles matérielles, les fichiers de chaque adaptateur nommé et les raccordements SDK/App/build/registry à partir de la carte commune ; ne recopier aucune donnée Sinclair.
  - [ ] Créer le projet et les modèles selon les contrats qualifiés.
    - [ ] Créer `src/GWGUI.Emulation.Thomson/GWGUI.Emulation.Thomson.csproj` et `module.json` : nom d’assembly, manifeste, compatibilité et ressources conformes aux autres projets sans référence App.
    - [ ] Créer `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Enums/MachineModel.cs` : identifiants stables des modèles matériels qualifiés, sans nom de cœur dans la machine.
    - [ ] Créer `src/GWGUI.Emulation.Thomson/Common/Machines/Common/Enums/Emulator.cs` : None et les seuls moteurs confirmés, avec identifiants persistés stables.
  - [ ] Détailler puis réaliser les fichiers de la carte dans l’ordre des dépendances.
    - [ ] Créer `docs/tasks/emulation/thomson-contrats.md` : contrats internes précis et correspondance des commandes communes avec les adaptateurs ; enregistrer les limites restant à qualifier avant fabrication.
  - [ ] Livrer et vérifier le module complet.
    - [ ] Créer `docs/tasks/emulation/thomson-verification.json` : preuves de ressources, intégration, supports, entrées modifiables et sessions livrées réellement contrôlées ; conserver les limites et ne déclarer aucun essai avant réalisation.
    - [ ] Supprimer les contrôles temporaires et leurs fichiers détenus énumérés dans les actions avant utilisation, après arrêt et déchargement effectifs.
    - [ ] Modifier `.git/index` et la référence de branche par le commit du module Thomson entièrement validé, sans push.
