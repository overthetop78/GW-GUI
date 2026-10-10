# Manettes sans affectation automatique — tous les modules

- [x] Supprimer les associations imposées sans effacer les choix enregistrés.
  - [x] Corriger les définitions et leur utilisation à l’exécution.
    - [x] Modifier `src/GWGUI.Emulation.Fairchild/Common/Machines/Common/Dictionaries/ControllerProtocolDictionary.cs` : retirer les sources physiques par défaut et conserver les identifiants du protocole natif.
    - [x] Modifier `src/GWGUI.Emulation.Epoch/Common/Machines/Common/Dictionaries/ControllerProtocolDictionary.cs` : laisser toutes les associations vides, y compris pavé, axes et souris.
    - [x] Modifier `src/GWGUI.Emulation.Coleco/Common/Machines/Common/Functions/InputFunctions.Settings.cs` : supprimer les associations de manette par défaut, conserver celles du clavier ADAM.
    - [x] Modifier `src/GWGUI.Emulation.{Acorn,Apple,Atari,Bandai,Amstrad,Commodore,Microsoft,Nec,Nintendo,Sega,Sony}/Common/Machines/Common/Functions/InputFunctions.Snapshot.cs` : supprimer la transmission automatique des boutons lorsque les associations sont absentes ou vides ; conserver le traitement des associations explicites.
  - [x] Prévenir les régressions et vérifier tous les modules.
    - [x] Modifier `docs/tasks/emulation/methode-modules.md` : imposer des associations de manette initialement vides, sans confusion entre bouton natif et source physique.
    - [x] Modifier `tests/GWGUI.Tests/Emulation/Fairchild/FairchildModuleTests.cs` : vérifier que des entrées physiques actives ne produisent aucune commande sans association explicite.
    - [x] Créer `docs/tasks/emulation/manettes-sans-affectation-verification.md` : consigner l’audit des définitions et du runtime de chaque module et les résultats des contrôles.
    - [x] Produire `build/Debug/GW GUI/gwgui.exe` et les modules par `scripts/local-building.cmd --building=debug --modules=A`, puis vérifier les sorties.
