# Extensions mémoire du BBC Micro Model A

- [x] Définir les cartes et leurs contraintes matérielles.
  - [x] Créer `docs/project/emulations/Acorn/BBC Micro/Model A/memory-extensions.md` : consigner les notices constructeur, capacités, registres, ROM de gestion, prérequis et incompatibilités des cartes effectivement retenues ; expliquer les banques ROM de 16 Kio et la réutilisation future du Model A par le B.
- [x] Ajouter les réglages et la mémoire émulée.
  - [x] Créer les constantes et contrats de cartes dans `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Constants/MemoryExtensionConstants.cs` et `Contracts/MemoryExtensionSettings.cs` : identifiants persistants, capacités, registres et contraintes documentés.
  - [x] Modifier `Common/Machines/Common/Enums/FirmwareEnums.cs`, `Dictionaries/FirmwareCatalog.cs`, `Common/Machines/BbcMicro/Dictionaries/FirmwareCompatibilityCatalog.cs` et `Functions/FirmwareFunctions.cs` dans Acorn : identifier les ROM de gestion Aries/Watford par le catalogue firmware existant et accepter leurs banques de 8/16 Kio sans les confondre avec une ROM disque.
  - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Functions/MemoryExtensionFunctions.cs` et modifier la fabrique AcornEmu : bloc RAM avec Aucune, choix des cartes/capacités, persistance et validation des incompatibilités ; réutiliser les champs et sélecteurs ROM existants.
  - [x] Créer `src/GWGUI.Emulation.Acorn/Emulators/AcornEmu/Machines/BbcMicro/ModelA/Services/MemoryExtension.cs` et modifier `ModelAMachine.cs`, les banques ROM et la vidéo concernés : implémenter les registres, le routage CPU/vidéo et les réinitialisations des cartes retenues sans remplacer la base Model A.
- [x] Localiser et vérifier.
  - [x] Modifier les ressources `src/GWGUI.Emulation.Acorn/Resources/*/Emulation.resx` : traduire les nouveaux libellés dans toutes les langues avec Argos et placer les valeurs invariantes dans la base commune.
  - [x] Créer `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuMemoryExtensionTests.cs` : vérifier l'isolation de la mémoire principale/écran/banques, les registres, les conflits et le fonctionnement sans extension.
  - [x] Modifier `Emulators/AcornEmu/Functions/FirmwareFunctions.cs`, les constantes et `Resources/*/Emulation.resx` : indiquer l'onglet RAM, et non ROM, lorsque la ROM de la carte mémoire est absente ; conserver la RAM principale avant le bloc d'extension dans la présentation.
  - [x] Créer `artifacts/acorn-memory-extensions-check/check.csproj` et ses contrôles temporaires : compiler les tests Acorn et vérifier l'intégration des cartes, les destinations ROM, les messages et la présentation RAM dans un processus isolé ; fermer et libérer les objets dans finally, puis supprimer le vérificateur et ses artefacts.
  - [x] Modifier `docs/tasks/acorn-memory-extensions.md` : consigner les vérifications exécutées, les cartes réalisées et les limites restantes ; ne pas fermer l'application utilisateur ni remplacer son build pendant son utilisation.

Les travaux de configuration stockage déjà autorisés restent suivis dans `docs/tasks/acorn-storage-settings.md` ; ils ne sont pas abandonnés. Aucun renommage de Model A en base A/B n'est autorisé. Le Model B réutilisera le code Model A et restera une machine distincte. Une capacité de ROM totale de 256 Kio n'autorise pas à charger arbitrairement un fichier de 256 Kio dans une banque de 16 Kio.

## Vérifications du 27 octobre 2026

Les 218 tests autonomes Acorn passent, dont dix cas supplémentaires sur les cartes d'extension RAM : séparation des mémoires, zone basse partagée, adressage complet Watford, sélection sans mémoire répondante, fonctionnement sans carte, BREAK et mise sous tension, commutation par un véritable programme 6502, prérequis 32 Kio et conservation des chemins ROM.

Deux contrôles temporaires supplémentaires passent : sélection/chargement réel d'un fichier synthétique 8 Kio par le gestionnaire existant, banque 12 sans remplacement de BASIC, ROM non modifiable, refus d'un fichier de 32 Kio pour cette destination, diagnostic RAM et résolution des noms invariants dans les cultures contrôlées. Total 220 réussites. Les catalogues de toutes les langues contiennent les huit nouveaux libellés ; noms Aries/Watford et capacités des choix sont dans 00-Base uniquement. Les fichiers utilisateur sont inchangés.

La vidéo continue de lire la RAM BBC, conformément aux cartes retenues. Aucune ROM propriétaire n'est intégrée. Les commandes RAMON/SHADOW et buffers dépendent de la ROM de gestion choisie : les contrôles de bus et de chargement n'équivalent pas à un essai de toutes les versions de ROM originales. Les cartes Aries B32, Solidisk, coprocesseurs Tube et nouvelles cartes ROM ne sont pas implémentés par ces deux profils et ne sont pas proposés comme tels.

Le vérificateur temporaire et ses fichiers ont été supprimés. Le build Debug autorisé s'est terminé avec succès ; exécutable et modules Acorn/Example présents sous `build/Debug/GW GUI`. Aucun commit ni push n'a été effectué.

## Retour utilisateur : total RAM et sélection par la ROM

- [ ] Corriger le total et la sélection sans demander deux choix indépendants.
  - [x] Modifier `Common/Interfaces/IEmulatorSettingsApplication.cs` et `Common/Services/Engine.cs` : transmettre aussi la configuration précédente au traitement natif, avec repli vers le contrat existant pour les autres adaptateurs, afin de distinguer un fichier ROM changé d’une capture inchangée du formulaire.
  - [x] Modifier `Emulators/AcornEmu/Functions/MemoryExtensionFunctions.cs` et `Factories/AcornEmuMachineFactory.cs` : renseigner les capacités numériques des choix pour le total App existant, annoncer le total physique au résumé, reconnaître la carte depuis une ROM connue lors du parcours/Utiliser et autoriser ce parcours sans carte préalablement sélectionnée ; conserver Aucune pour retirer la carte et la sélection manuelle pour les ROM inconnues.
  - [ ] Modifier les constantes et `Resources/*/Emulation.resx` : expliquer les capacités, le rôle de la ROM et la détection automatique ; corriger les traductions erronées du titre Shadow RAM avec Argos et le français.
  - [ ] Modifier `tests/GWGUI.LocalEmulationTests/Emulation/AcornEmuMemoryExtensionTests.cs`, créer puis supprimer `artifacts/acorn-memory-selection-check/` : contrôler 32/52/64 Kio, retrait de carte, préservation des chemins, changement par ROM connue et refus des faux formats, sans modifier les fichiers utilisateur.
  - [ ] Modifier `docs/project/emulations/Acorn/BBC Micro/Model A/memory-extensions.md` et ce suivi : documenter les résultats, les liens de ROM vérifiés, les deux cartes implémentées et les capacités plus grandes non encore réalisées ; ne pas relancer un build de livraison pendant l'essai utilisateur sans son accord.

La réflexion demandée ensuite conserve le choix du matériel et de sa capacité : la ROM ne doit pas changer silencieusement la carte. Le traitement automatique essayé est retiré, ainsi que son extension du contrat Common. Le total numérique est conservé ; la préparation des ROM intégrées doit suivre le matériel réellement implémenté.
